using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using api.data;
using api.data.Entities;
using api.data.Repositories;
using api.services.EmailService;
using Microsoft.EntityFrameworkCore;
using api.services.NotificationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttemptController(IRepositoryWrapper repo, ApplicationContext db, INotificationService notificationService, IHttpClientFactory httpClientFactory, IEmailService emailService) : ControllerBase
    {
        private readonly IRepositoryWrapper _repo = repo;
        private readonly ApplicationContext _db = db;
        private readonly INotificationService _notificationService = notificationService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IEmailService _emailService = emailService;

        public class StartAttemptRequest
        {
            public int  CampaignId      { get; set; }
            public int? QuestionnaireId { get; set; }
        }

        [HttpPost("start")]
        [Authorize(Roles = "Condidat")]
        public async Task<IActionResult> StartAttempt([FromBody] StartAttemptRequest request)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.GetWithCandidates(request.CampaignId);
            if (campaign == null) return NotFound("Campaign not found.");
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            var candidateLink = campaign.CampaignCandidates
                .FirstOrDefault(cc => cc.CandidateId == user.Id && cc.Enable);

            if (candidateLink == null) return Forbid("You are not invited to this campaign.");
            if (campaign.Status != CampaignStatus.Active) return BadRequest("Campaign is not active.");

            var now = DateTime.UtcNow;
            if (now < campaign.AvailableFrom || now > campaign.AvailableUntil)
            {
                return BadRequest("Campaign is not currently available within its timeframe.");
            }

            // Resolve which questionnaire to use: explicit request or first in campaign
            int questionnaireId = request.QuestionnaireId ?? 0;
            if (questionnaireId == 0)
            {
                var campaignWithQ = await _repo.Campaign.GetWithQuestionnaires(request.CampaignId);
                questionnaireId = campaignWithQ?.CampaignQuestionnaires
                    .Where(cq => cq.Enable)
                    .OrderBy(cq => cq.Order)
                    .FirstOrDefault()?.QuestionnaireId ?? 0;
            }
            var questionnaire = await _repo.Questionnaire.GetWithQuestions(questionnaireId);
            if (questionnaire == null) return BadRequest("Questionnaire missing.");

            var attemptsCount = await _repo.CandidateAttempt.CountAttemptsByQuestionnaire(candidateLink.Id, questionnaire.Id);
            // MaxAttempts is now a campaign-level setting
            if (campaign.MaxAttempts != -1 && attemptsCount >= campaign.MaxAttempts)
            {
                return BadRequest($"Max attempts reached ({campaign.MaxAttempts}). You cannot retry this test.");
            }

            var previousAttempts = await _repo.CandidateAttempt.GetByCampaignCandidateAndQuestionnaire(candidateLink.Id, questionnaire.Id);
            if (campaign.CooldownBetweenAttemptsMinutes.HasValue)
            {
                var lastSubmit = previousAttempts
                    .Where(a => a.Status == AttemptStatus.Submitted)
                    .OrderByDescending(a => a.SubmittedAt)
                    .FirstOrDefault();

                if (lastSubmit?.SubmittedAt != null)
                {
                    if (now < lastSubmit.SubmittedAt.Value.AddMinutes(campaign.CooldownBetweenAttemptsMinutes.Value))
                    {
                        return BadRequest("Cooldown period active.");
                    }
                }
            }

            // In a real app we deep clone questions here. Instead, just serialize to JSON
            var snapshotOpts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            // Apply randomization using campaign settings
            var questionList = questionnaire.QuestionnaireQuestions.ToList();
            if (campaign.RandomizeQuestions)
                questionList = questionList.OrderBy(_ => Guid.NewGuid()).ToList();

            var questionsSnapshot = JsonSerializer.Serialize(questionList.Select(q => new
            {
                q.QuestionId,
                Title             = q.Question?.Title ?? "Unknown",
                Explanation       = q.Question?.Explanation,
                Hint              = q.Question?.Hint,
                Type              = q.Question?.Type ?? QuestionType.QCU,
                Complexity        = q.Question?.Complexity ?? ComplexityLevel.Beginner,
                Points            = q.Question?.Points ?? 0,
                TimeLimitSeconds  = q.Question?.TimeLimitSeconds,
                q.Weight,
                q.Order,
                Choices = ((IEnumerable<QuestionChoice>?)q.Question?.Choices ?? new List<QuestionChoice>())
                    .OrderBy(c => campaign.RandomizeChoices ? Guid.NewGuid() : (object)c.Order)
                    .Select(c => new { c.Id, c.Text, c.Order })
            }), snapshotOpts);

            // Reset status if retrying after failure/completion
            if (candidateLink.Status != CampaignCandidateStatus.InProgress)
            {
                candidateLink.Status = CampaignCandidateStatus.InProgress;
                _db.CampaignCandidates.Update(candidateLink);
                await _db.SaveChangesAsync();
            }

            var newAttempt = new CandidateAttempt
            {
                CampaignId          = campaign.Id,
                QuestionnaireId     = questionnaire.Id,
                CampaignCandidateId = candidateLink.Id,
                CandidateId         = user.Id,
                AttemptNumber       = attemptsCount + 1,
                StartedAt           = now,
                ExpiresAt           = campaign.GlobalDurationMinutes.HasValue ? now.AddMinutes(campaign.GlobalDurationMinutes.Value) : (DateTime?)null,
                Status              = AttemptStatus.InProgress,
                QuestionsSnapshot   = questionsSnapshot
            };

            var created = await _repo.CandidateAttempt.Create(newAttempt);

            return Ok(new {
                attemptId           = created.Id,
                questions           = questionsSnapshot,
                questionnaireName   = questionnaire.Title,
                showTimer           = campaign.ShowTimer,
                durationMinutes     = campaign.GlobalDurationMinutes,
                allowNavigationBack = campaign.AllowNavigationBack,
                tabSwitchMaxCount   = campaign.TabSwitchMaxCount ?? 0,
                maxAttempts         = campaign.MaxAttempts,
                campaignCandidateId = candidateLink.Id,
            });
        }

        public class AnswerRequest
        {
            public int QuestionId { get; set; }
            public List<int> SelectedChoiceIds { get; set; } = new();
            public int TimeSpentSeconds { get; set; }
        }

        [HttpPost("{id}/answer")]
        [Authorize(Roles = "Condidat")]
        public async Task<IActionResult> Answer(int id, [FromBody] AnswerRequest request)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var attempt = await _repo.CandidateAttempt.GetWithAnswers(id);
            if (attempt == null) return NotFound();
            if (attempt.CandidateId != user.Id) return Forbid();
            if (attempt.Status != AttemptStatus.InProgress) return BadRequest("Attempt no longer in progress.");

            if (attempt.ExpiresAt.HasValue && DateTime.UtcNow > attempt.ExpiresAt.Value)
            {
                attempt.Status = AttemptStatus.TimedOut;
                await _repo.CandidateAttempt.Update(attempt);
                return BadRequest("Time expired.");
            }

            var existingAnswer = attempt.Answers.FirstOrDefault(a => a.QuestionId == request.QuestionId);
            if (existingAnswer == null)
            {
                attempt.Answers.Add(new AttemptAnswer
                {
                    AttemptId = id,
                    QuestionId = request.QuestionId,
                    SelectedChoiceIds = JsonSerializer.Serialize(request.SelectedChoiceIds),
                    TimeSpentSeconds = request.TimeSpentSeconds,
                    AnsweredAt = DateTime.UtcNow
                });
            }
            else
            {
                existingAnswer.SelectedChoiceIds = JsonSerializer.Serialize(request.SelectedChoiceIds);
                existingAnswer.TimeSpentSeconds += request.TimeSpentSeconds;
                existingAnswer.AnsweredAt = DateTime.UtcNow;
            }

            await _repo.CandidateAttempt.Update(attempt);
            return Ok();
        }

        [HttpPost("{id}/submit")]
        [Authorize(Roles = "Condidat")]
        public async Task<IActionResult> Submit(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var attempt = await _repo.CandidateAttempt.GetWithAnswers(id);
            if (attempt == null) return NotFound();
            if (attempt.CandidateId != user.Id) return Forbid();
            if (attempt.Status != AttemptStatus.InProgress) return BadRequest("Attempt no longer in progress.");

            // Scoring logic
            var campaign = await _repo.Campaign.GetWithCandidates(attempt.CampaignId);
            if (campaign == null) return NotFound("Campaign not found.");
            var questionnaire = await _repo.Questionnaire.GetWithQuestions(attempt.QuestionnaireId ?? 0);
            if (questionnaire == null) return NotFound("Questionnaire not found.");

            decimal rawScore = 0;
            decimal maxScore = 0;
            var allowPartial = campaign.AllowPartialScore;

            foreach (var qq in questionnaire.QuestionnaireQuestions)
            {
                var q = qq.Question;
                if (q == null) continue;

                var ans = attempt.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                var qMaxPoints = q.Points * qq.Weight;
                maxScore += qMaxPoints;

                if (ans == null) continue;

                var selectedIds = string.IsNullOrEmpty(ans.SelectedChoiceIds) ? new List<int>() : JsonSerializer.Deserialize<List<int>>(ans.SelectedChoiceIds) ?? new List<int>();
                var correctChoices = q.Choices.Where(c => c.IsCorrect).Select(c => c.Id).ToList();

                bool isCorrect = false;
                decimal partialScore = 0;
                decimal earnedPoints = 0;

                if (q.Type == QuestionType.QCU || q.Type == QuestionType.TrueFalse)
                {
                    if (selectedIds.Count == 1 && correctChoices.Contains(selectedIds.First()))
                    {
                        isCorrect = true;
                        partialScore = 1;
                        earnedPoints = qMaxPoints;
                    }
                }
                else if (q.Type == QuestionType.QCM)
                {
                    var correctSelected = selectedIds.Count(sid => correctChoices.Contains(sid));
                    var wrongSelected = selectedIds.Count(sid => !correctChoices.Contains(sid));

                    if (wrongSelected == 0 && correctSelected == correctChoices.Count && correctChoices.Count > 0)
                    {
                        isCorrect = true;
                        partialScore = 1;
                        earnedPoints = qMaxPoints;
                    }
                    else if (allowPartial && correctChoices.Count > 0 && wrongSelected == 0) // Basic partial credit logic (no penalty for wrong, just omit)
                    {
                        partialScore = (decimal)correctSelected / correctChoices.Count;
                        earnedPoints = qMaxPoints * partialScore;
                    }
                }

                ans.IsCorrect = isCorrect;
                ans.PartialScore = partialScore;
                ans.EarnedPoints = earnedPoints;
                ans.MaxPoints = qMaxPoints;

                rawScore += earnedPoints;
            }

            attempt.RawScore = rawScore;
            attempt.MaxPossibleScore = maxScore;
            attempt.Percentage = maxScore > 0 ? (rawScore / maxScore) * 100 : 0;
            attempt.Passed = attempt.Percentage >= campaign.PassingScorePercent;
            attempt.Status = AttemptStatus.Submitted;
            attempt.SubmittedAt = DateTime.UtcNow;

            // Save basic report placeholder (updated below if campaign completes)
            attempt.Report = new AttemptReport
            {
                AttemptId   = attempt.Id,
                GeneratedAt = DateTime.UtcNow,
                ThemeScores = "{}"
            };

            await _repo.CandidateAttempt.Update(attempt);

            // ── Check how many attempts remain for this questionnaire ─────────
            var candidateLink = campaign.CampaignCandidates.FirstOrDefault(c => c.Id == attempt.CampaignCandidateId);
            var attemptsUsed = await _repo.CandidateAttempt.CountAttemptsByQuestionnaire(attempt.CampaignCandidateId, questionnaire.Id);
            var hasMoreAttempts = campaign.MaxAttempts == -1 || attemptsUsed < campaign.MaxAttempts;

            // ── Check if ALL required questionnaires in the campaign are passed ─
            var campaignWithQ = await _repo.Campaign.GetWithQuestionnaires(attempt.CampaignId);
            var requiredQIds  = campaignWithQ?.CampaignQuestionnaires
                .Where(cq => cq.Enable && cq.IsRequired)
                .Select(cq => cq.QuestionnaireId)
                .ToHashSet() ?? new HashSet<int>();

            var passedQIdsList = await _db.CandidateAttempts
                .Where(a => a.CandidateId == user.Id
                         && a.CampaignId  == attempt.CampaignId
                         && a.Status      == AttemptStatus.Submitted
                         && a.Passed      == true
                         && a.Id          != attempt.Id
                         && a.QuestionnaireId != null)
                .Select(a => (int)a.QuestionnaireId!)
                .ToListAsync();
            var passedQIds = passedQIdsList.ToHashSet();

            if (attempt.Passed && attempt.QuestionnaireId.HasValue)
                passedQIds.Add(attempt.QuestionnaireId.Value);

            var campaignPassed = requiredQIds.Count > 0 && requiredQIds.All(qid => passedQIds.Contains(qid));

            if (campaignPassed)
            {
                // Mark candidate as Completed + update denormalized counters
                if (candidateLink != null)
                {
                    candidateLink.Status = CampaignCandidateStatus.Completed;
                    campaign.CompletedCount = campaign.CampaignCandidates
                        .Count(cc => cc.Enable && cc.Status == CampaignCandidateStatus.Completed);
                    campaign.PassedCount++;
                    await _repo.Campaign.Update(campaign);
                }

                // Issue campaign certificate (one per candidate per campaign)
                var alreadyHasCert = _db.Certificates.Any(c => c.CandidateId == user.Id && c.CampaignId == attempt.CampaignId);
                if (!alreadyHasCert)
                {
                    var campaignPct = requiredQIds.Count > 0
                        ? (decimal)passedQIds.Count(qid => requiredQIds.Contains(qid)) / requiredQIds.Count * 100
                        : attempt.Percentage;

                    _db.Certificates.Add(new Certificate
                    {
                        CandidateId     = user.Id,
                        CampaignId      = attempt.CampaignId,
                        AttemptId       = attempt.Id,
                        Percentage      = campaignPct,
                        CertificateCode = Guid.NewGuid().ToString("N"),
                        IssuedAt        = DateTime.UtcNow,
                        CreatedAt       = DateTime.UtcNow,
                        Enable          = true
                    });
                    await _db.SaveChangesAsync();
                }

                // Generate AI report for this attempt
                try
                {
                    var (strengths, recommendations) = await GenerateCampaignAiReportAsync(
                        user.Id, attempt.CampaignId, campaign.Name, attempt);
                    if (attempt.Report != null)
                    {
                        attempt.Report.AiStrengths       = strengths;
                        attempt.Report.AiRecommendations = recommendations;
                        await _repo.CandidateAttempt.Update(attempt);
                    }
                }
                catch { /* AI is best-effort */ }

                // Notify campaign passed
                await _notificationService.SendNotificationAsync(
                    user.Id,
                    $"Campaign Passed 🎉",
                    $"Congratulations! You passed the '{campaign.Name}' campaign.",
                    NotificationType.Success,
                    $"/candidat/campaign/{attempt.CampaignId}"
                );
            }
            else if (!attempt.Passed)
            {
                if (hasMoreAttempts)
                {
                    // Failed but can retry — notify with retry info
                    var remaining = campaign.MaxAttempts == -1 ? -1 : campaign.MaxAttempts - attemptsUsed;
                    var retryMsg = remaining == -1 ? "unlimited attempts" : $"{remaining} attempt(s) remaining";
                    await _notificationService.SendNotificationAsync(
                        user.Id,
                        "Questionnaire Failed - Retry Available",
                        $"You scored {Math.Round(attempt.Percentage, 1)}% on {questionnaire.Title}. You can retry ({retryMsg}).",
                        NotificationType.Warning,
                        $"/candidat/result/{attempt.Id}"
                    );
                }
                else
                {
                    // Failed questionnaire with no retries left — notify result
                    await _notificationService.SendNotificationAsync(
                        user.Id,
                        "Questionnaire Completed",
                        $"You scored {Math.Round(attempt.Percentage, 1)}% on {questionnaire.Title}. No more attempts remaining.",
                        NotificationType.Warning,
                        $"/candidat/result/{attempt.Id}"
                    );
                }
            }
            // else: passed questionnaire but campaign not yet complete → no notification yet

            return Ok(new
            {
                attempt.RawScore,
                attempt.MaxPossibleScore,
                attempt.Percentage,
                attempt.Passed,
                campaignPassed,
                campaignId = attempt.CampaignId,
            });
        }

        // ── POST /api/attempt/{id}/generate-report ────────────────────────────
        // On-demand AI report for candidate (generated once, cached after)
        [HttpPost("{id}/generate-report")]
        [Authorize(Roles = "Condidat")]
        public async Task<IActionResult> GenerateReport(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var attempt = await _repo.CandidateAttempt.GetWithAnswers(id);
            if (attempt == null) return NotFound();
            if (attempt.CandidateId != user.Id) return Forbid();
            if (attempt.Status != AttemptStatus.Submitted) return BadRequest("Attempt not yet submitted.");

            // Return cached report if it already has AI content
            if (attempt.Report?.AiStrengths != null || attempt.Report?.AiRecommendations != null)
                return Ok(new { aiStrengths = attempt.Report!.AiStrengths, aiRecommendations = attempt.Report.AiRecommendations });

            var campaign = await _repo.Campaign.Get(attempt.CampaignId);
            try
            {
                var (strengths, recommendations) = await GenerateCampaignAiReportAsync(
                    user.Id, attempt.CampaignId, campaign?.Name ?? "Test", attempt);

                if (attempt.Report == null)
                {
                    attempt.Report = new AttemptReport { AttemptId = attempt.Id, GeneratedAt = DateTime.UtcNow, ThemeScores = "{}" };
                }
                attempt.Report.AiStrengths       = strengths;
                attempt.Report.AiRecommendations = recommendations;
                attempt.Report.GeneratedAt       = DateTime.UtcNow;
                await _repo.CandidateAttempt.Update(attempt);

                return Ok(new { aiStrengths = strengths, aiRecommendations = recommendations });
            }
            catch (Exception ex)
            {
                return BadRequest($"AI report generation failed: {ex.Message}");
            }
        }

        [HttpPost("{id}/tab-switch")]
        [Authorize(Roles = "Condidat")]
        public async Task<IActionResult> TabSwitch(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var attempt = await _repo.CandidateAttempt.Get(id);
            if (attempt == null) return NotFound();
            if (attempt.CandidateId != user.Id) return Forbid();
            if (attempt.Status != AttemptStatus.InProgress) return BadRequest("Attempt no longer in progress.");

            attempt.TabSwitchCount++;

            // Focus lost event formatting naive
            var focusEvents = attempt.FocusLostEvents == null ? new List<object>() : JsonSerializer.Deserialize<List<object>>(attempt.FocusLostEvents) ?? new List<object>();
            focusEvents.Add(new { timestamp = DateTime.UtcNow, durationSeconds = 0 });
            attempt.FocusLostEvents = JsonSerializer.Serialize(focusEvents);

            // Check TabSwitchMaxCount from campaign settings
            var tabCampaign = await _repo.Campaign.Get(attempt.CampaignId);
            if (tabCampaign?.TabSwitchMaxCount != null && attempt.TabSwitchCount > tabCampaign.TabSwitchMaxCount.Value)
            {
                attempt.Status = AttemptStatus.Abandoned;
            }

            await _repo.CandidateAttempt.Update(attempt);
            return Ok();
        }

        [HttpGet("{id}/result")]
        [Authorize(Roles = "Condidat,Formateur,CompanyAdmin")]
        public async Task<IActionResult> GetResult(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var attempt = await _repo.CandidateAttempt.GetWithAnswers(id);
            if (attempt == null) return NotFound();

            var isCandidate = await _repo.UserManager.IsInRoleAsync(user, "Condidat");
            if (isCandidate && attempt.CandidateId != user.Id)
            {
                return Forbid(); // Candidates only see their own
            }

            if (!isCandidate)
            {
                var campaign = await _repo.Campaign.Get(attempt.CampaignId);
                if (campaign == null || campaign.CompanyId != user.CompanyId)
                {
                    return Forbid();
                }
            }

            // Compute remaining attempts from campaign settings
            int remainingAttempts = -1;
            int maxAttempts       = -1;
            string? questionnaireName = null;
            var resultCampaign = await _repo.Campaign.Get(attempt.CampaignId);
            if (attempt.QuestionnaireId.HasValue && attempt.CampaignCandidateId > 0)
            {
                var q = await _repo.Questionnaire.Get(attempt.QuestionnaireId.Value);
                if (q != null) questionnaireName = q.Title;
                if (resultCampaign != null)
                {
                    maxAttempts = resultCampaign.MaxAttempts;
                    var used    = attempt.QuestionnaireId.HasValue
                        ? await _repo.CandidateAttempt.CountAttemptsByQuestionnaire(attempt.CampaignCandidateId, attempt.QuestionnaireId.Value)
                        : 0;
                    remainingAttempts = maxAttempts == -1 ? -1 : Math.Max(0, maxAttempts - used);
                }
            }

            return Ok(new {
                attempt.Id,
                attempt.CampaignId,
                attempt.QuestionnaireId,
                questionnaireName,
                attempt.CampaignCandidateId,
                attempt.RawScore,
                attempt.MaxPossibleScore,
                attempt.Percentage,
                attempt.Passed,
                attempt.StartedAt,
                attempt.SubmittedAt,
                Status = attempt.Status.ToString(),
                attempt.TabSwitchCount,
                report = attempt.Report == null ? null : new {
                    themeScores       = attempt.Report.ThemeScores,
                    aiRecommendations = attempt.Report.AiRecommendations,
                    aiStrengths       = attempt.Report.AiStrengths,
                },
                remainingAttempts,
                maxAttempts,
            });
        }

        // ── AI Report generation ──────────────────────────────────────────────

        private async Task<(string? strengths, string? recommendations)> GenerateCampaignAiReportAsync(
            int candidateId, int campaignId, string campaignName, CandidateAttempt lastAttempt)
        {
            var aiModel = await _repo.AiModel.GetDefault();
            if (aiModel == null || !aiModel.IsEnabled) return (null, null);

            // Collect answers for context
            var correctCount = lastAttempt.Answers.Count(a => a.IsCorrect == true);
            var totalCount   = lastAttempt.Answers.Count;
            var percentage   = Math.Round(lastAttempt.Percentage, 1);

            // Build question summary (title + correct/incorrect) from snapshot
            var questionSummaries = new List<string>();
            try
            {
                var snapshotItems = JsonSerializer.Deserialize<List<JsonElement>>(lastAttempt.QuestionsSnapshot ?? "[]");
                foreach (var item in snapshotItems ?? [])
                {
                    var qIdEl  = item.TryGetProperty("questionId", out var el) ? el : item.GetProperty("QuestionId");
                    var qId    = qIdEl.GetInt32();
                    var title  = item.TryGetProperty("title", out var t) ? t.GetString() : item.GetProperty("Title").GetString();
                    var ans    = lastAttempt.Answers.FirstOrDefault(a => a.QuestionId == qId);
                    var result = ans?.IsCorrect == true ? "✓ Correct" : "✗ Incorrect";
                    questionSummaries.Add($"- {title}: {result}");
                }
            }
            catch { /* best-effort */ }

            var questionDetail = questionSummaries.Count > 0
                ? string.Join("\n", questionSummaries)
                : $"Score: {correctCount}/{totalCount}";

            var systemPrompt = """
                You are an expert educational assessor. Based on a candidate's test results, write a short
                personalized analysis in 2-3 sentences per section. Be encouraging but honest.
                Return ONLY a valid JSON object with two fields: "strengths" and "recommendations".
                No markdown, no extra text.
                """;

            var userMessage = $"""
                Campaign: {campaignName}
                Score: {percentage}% ({correctCount}/{totalCount} correct)

                Questions answered:
                {questionDetail}

                Write:
                1. "strengths": what the candidate did well and demonstrated mastery of
                2. "recommendations": specific areas to study and improve for future tests
                """;

            var responseText = aiModel.Provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase)
                ? await CallAnthropicAsync(aiModel, systemPrompt, userMessage)
                : await CallOpenAiAsync(aiModel, systemPrompt, userMessage);

            if (string.IsNullOrWhiteSpace(responseText)) return (null, null);

            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;
                var strengths       = root.TryGetProperty("strengths",       out var s) ? s.GetString() : null;
                var recommendations = root.TryGetProperty("recommendations", out var r) ? r.GetString() : null;
                return (strengths, recommendations);
            }
            catch { return (null, null); }
        }

        private async Task<string?> CallAnthropicAsync(api.data.Entities.AiModel model, string system, string user)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", model.ApiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            var baseUrl = string.IsNullOrWhiteSpace(model.ApiBaseUrl) ? "https://api.anthropic.com" : model.ApiBaseUrl.TrimEnd('/');
            var body = new { model = model.ModelIdentifier, max_tokens = 512, system, messages = new[] { new { role = "user", content = user } } };
            var response = await client.PostAsync($"{baseUrl}/v1/messages",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return null;
            using var doc = JsonDocument.Parse(text);
            return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
        }

        private async Task<string?> CallOpenAiAsync(api.data.Entities.AiModel model, string system, string user)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", model.ApiKey);
            var baseUrl = string.IsNullOrWhiteSpace(model.ApiBaseUrl) ? "https://api.openai.com" : model.ApiBaseUrl.TrimEnd('/');
            var body = new { model = model.ModelIdentifier, max_tokens = 512, temperature = 0.7,
                messages = new[] { new { role = "system", content = system }, new { role = "user", content = user } } };
            var response = await client.PostAsync($"{baseUrl}/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return null;
            using var doc = JsonDocument.Parse(text);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }
}
