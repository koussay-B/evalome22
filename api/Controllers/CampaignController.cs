using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data;
using api.data.Entities;
using api.data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignController(IRepositoryWrapper repo, ApplicationContext db) : ControllerBase
    {
        private readonly ApplicationContext _db = db;
        private readonly IRepositoryWrapper _repo = repo;

        // ── DTOs ────────────────────────────────────────────────────────────────

        public class InviteRequest
        {
            public List<int> UserIds { get; set; } = new();
        }

        public class CampaignCandidateDto
        {
            public int     CampaignCandidateId { get; set; }
            public int     CandidateId         { get; set; }
            public string  CandidateName       { get; set; } = string.Empty;
            public string  CandidateEmail      { get; set; } = string.Empty;
            public string? CandidateImageUrl   { get; set; }
            public string  Status              { get; set; } = string.Empty;
            public DateTime InvitedAt          { get; set; }
        }

        public class CampaignQuestionnaireDto
        {
            public int     Id                 { get; set; }
            public int     QuestionnaireId    { get; set; }
            public string  QuestionnaireTitle { get; set; } = string.Empty;
            public int     Order              { get; set; }
            public string? Label              { get; set; }
            public bool    IsRequired         { get; set; }
        }

        public class AddQuestionnaireRequest
        {
            public int     QuestionnaireId { get; set; }
            public string? Label           { get; set; }
            public bool    IsRequired      { get; set; } = true;
        }

        public class ReorderQuestionnairesRequest
        {
            public List<ReorderItem> Items { get; set; } = new();
            public class ReorderItem
            {
                public int QuestionnaireId { get; set; }
                public int Order           { get; set; }
            }
        }

        // ── GET /api/campaign ────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetCampaigns()
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null) return Unauthorized();

            var campaigns = await _repo.Campaign.GetByCompany((int)user.CompanyId);
            return Ok(campaigns);
        }

        // ── GET /api/campaign/{id} ───────────────────────────────────────────

        [HttpGet("{id}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetCampaign(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.GetWithCandidates(id);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            return Ok(campaign);
        }

        // ── POST /api/campaign ───────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> CreateCampaign([FromBody] Campaign campaignIn)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null) return Unauthorized();

            if (campaignIn.AvailableFrom >= campaignIn.AvailableUntil)
                return BadRequest("AvailableFrom must be before AvailableUntil.");

            campaignIn.CompanyId   = (int)user.CompanyId;
            campaignIn.CreatedById = user.Id;

            var created = await _repo.Campaign.Create(campaignIn);
            return Ok(created);
        }

        // ── GET /api/campaign/{id}/candidates ───────────────────────────────

        [HttpGet("{id}/candidates")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetCandidates(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.GetWithCandidates(id);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            var result = campaign.CampaignCandidates
                .Where(cc => cc.Enable)
                .Select(cc => new CampaignCandidateDto
                {
                    CampaignCandidateId = cc.Id,
                    CandidateId         = cc.CandidateId,
                    CandidateName       = cc.Candidate?.UserName ?? "Unknown",
                    CandidateEmail      = cc.Candidate?.Email    ?? "",
                    CandidateImageUrl   = cc.Candidate?.ImageUrl,
                    Status              = cc.Status.ToString(),
                    InvitedAt           = cc.InvitedAt,
                })
                .ToList();

            return Ok(result);
        }

        // ── DELETE /api/campaign/{id}/candidates/{candidateId} ──────────────

        [HttpDelete("{id}/candidates/{candidateId}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> RemoveCandidate(int id, int candidateId)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.Get(id);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            var removed = await _repo.Campaign.RemoveCandidate(id, candidateId);
            if (!removed) return NotFound("Candidate not found in this campaign.");

            // Recount after removal
            var freshCampaign = await _repo.Campaign.GetWithCandidates(id);
            if (freshCampaign != null)
            {
                freshCampaign.InvitedCount = freshCampaign.CampaignCandidates.Count(cc => cc.Enable);
                await _repo.Campaign.Update(freshCampaign);
            }

            return Ok();
        }

        // ── POST /api/campaign/{id}/invite ───────────────────────────────────

        [HttpPost("{id}/invite")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> Invite(int id, [FromBody] InviteRequest request)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.GetWithCandidates(id);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            var notificationsToCreate = new List<Notification>();

            foreach (var candidateId in request.UserIds)
            {
                var candidate = await _repo.UserManager.FindByIdAsync(candidateId.ToString());
                if (candidate == null || candidate.CompanyId != user.CompanyId)
                    continue;

                // Skip if already invited
                if (campaign.CampaignCandidates.Any(cc => cc.CandidateId == candidateId && cc.Enable))
                    continue;

                var cc = new CampaignCandidate
                {
                    CampaignId  = id,
                    CandidateId = candidateId,
                    InvitedAt   = DateTime.UtcNow,
                    InviteToken = Guid.NewGuid().ToString("N"),
                    Status      = CampaignCandidateStatus.Invited
                };

                campaign.CampaignCandidates.Add(cc);

                if (campaign.SendInviteEmail)
                {
                    notificationsToCreate.Add(new Notification
                    {
                        UserId    = candidateId,
                        Title     = "New Campaign Invitation",
                        Body      = $"You have been invited to campaign: {campaign.Name}",
                        Type      = NotificationType.Info,
                        ActionUrl = "/candidat/tests",
                        Received  = false,
                        Date      = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        Enable    = true
                    });
                }
            }

            // Recount invited absolutely to avoid drift
            campaign.InvitedCount = campaign.CampaignCandidates.Count(cc => cc.Enable);
            await _repo.Campaign.Update(campaign);

            foreach (var notif in notificationsToCreate)
                await _repo.Notification.Create(notif);

            return Ok();
        }

        // ── PUT /api/campaign/{id} ───────────────────────────────────────────

        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> UpdateCampaign(int id, [FromBody] Campaign campaignIn)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.Get(id);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            // Allow all edits regardless of status
            campaign.Name                = campaignIn.Name;
            campaign.Description         = campaignIn.Description;
            campaign.AvailableFrom       = campaignIn.AvailableFrom;
            campaign.AvailableUntil      = campaignIn.AvailableUntil;
            campaign.AllowedTimeSlots    = campaignIn.AllowedTimeSlots;
            campaign.SendInviteEmail     = campaignIn.SendInviteEmail;
            campaign.ReminderHoursBefore = campaignIn.ReminderHoursBefore;
            campaign.ThemeId             = campaignIn.ThemeId;
            campaign.Status              = campaignIn.Status;

            // Test execution settings
            campaign.PassingScorePercent            = campaignIn.PassingScorePercent;
            campaign.ScoringMode                    = campaignIn.ScoringMode;
            campaign.AllowPartialScore              = campaignIn.AllowPartialScore;
            campaign.GlobalDurationMinutes          = campaignIn.GlobalDurationMinutes;
            campaign.ShowTimer                      = campaignIn.ShowTimer;
            campaign.AllowNavigationBack            = campaignIn.AllowNavigationBack;
            campaign.RandomizeQuestions             = campaignIn.RandomizeQuestions;
            campaign.RandomizeChoices               = campaignIn.RandomizeChoices;
            campaign.ShowQuestionScore              = campaignIn.ShowQuestionScore;
            campaign.ShowCorrectAfterEach           = campaignIn.ShowCorrectAfterEach;
            campaign.ShowResultsImmediately         = campaignIn.ShowResultsImmediately;
            campaign.AllowTabSwitch                 = campaignIn.AllowTabSwitch;
            campaign.TabSwitchMaxCount              = campaignIn.TabSwitchMaxCount;
            campaign.MaxAttempts                    = campaignIn.MaxAttempts;
            campaign.CooldownBetweenAttemptsMinutes = campaignIn.CooldownBetweenAttemptsMinutes;

            var updated = await _repo.Campaign.Update(campaign);
            return Ok(updated);
        }

        // ── PUT /api/campaign/{id}/status ────────────────────────────────────

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] CampaignStatus status)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.Get(id);
            if (campaign == null) return NotFound();

            var roles = await _repo.UserManager.GetRolesAsync(user);
            if (!roles.Contains("Admin") && campaign.CompanyId != user.CompanyId)
                return Forbid();

            campaign.Status = status;
            var updated = await _repo.Campaign.Update(campaign);
            return Ok(updated);
        }

        // ── DELETE /api/campaign/{id} ────────────────────────────────────────

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.Get(id);
            if (campaign == null) return NotFound();

            var roles = await _repo.UserManager.GetRolesAsync(user);
            if (!roles.Contains("Admin") && campaign.CompanyId != user.CompanyId)
                return Forbid();

            // We allow deleting any campaign from the UI (Draft, Active, etc.)
            // because it is a soft-delete (Enable = false) that preserves data in the DB.
            await _repo.Campaign.Delete(id);
            return Ok();
        }

        // ── GET /api/campaign/{id}/questionnaires ────────────────────────────

        [HttpGet("{id}/questionnaires")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetQuestionnaires(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.GetWithQuestionnaires(id);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            var result = campaign.CampaignQuestionnaires
                .Where(cq => cq.Enable)
                .OrderBy(cq => cq.Order)
                .Select(cq => new CampaignQuestionnaireDto
                {
                    Id                 = cq.Id,
                    QuestionnaireId    = cq.QuestionnaireId,
                    QuestionnaireTitle = cq.Questionnaire?.Title ?? "Unknown",
                    Order              = cq.Order,
                    Label              = cq.Label,
                    IsRequired         = cq.IsRequired,
                })
                .ToList();

            return Ok(result);
        }

        // ── POST /api/campaign/{id}/questionnaires ───────────────────────────

        [HttpPost("{id}/questionnaires")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> AddQuestionnaire(int id, [FromBody] AddQuestionnaireRequest req)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.Get(id);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            var questionnaire = await _repo.Questionnaire.Get(req.QuestionnaireId);
            if (questionnaire == null || questionnaire.CompanyId != user.CompanyId)
                return BadRequest("Questionnaire not found or does not belong to your company.");

            var cq = await _repo.Campaign.AddQuestionnaire(id, req.QuestionnaireId, req.Label, req.IsRequired);

            return Ok(new CampaignQuestionnaireDto
            {
                Id                 = cq!.Id,
                QuestionnaireId    = cq.QuestionnaireId,
                QuestionnaireTitle = questionnaire.Title,
                Order              = cq.Order,
                Label              = cq.Label,
                IsRequired         = cq.IsRequired,
            });
        }

        // ── DELETE /api/campaign/{id}/questionnaires/{questionnaireId} ───────

        [HttpDelete("{id}/questionnaires/{questionnaireId}")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> RemoveQuestionnaire(int id, int questionnaireId)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.Get(id);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            var removed = await _repo.Campaign.RemoveQuestionnaire(id, questionnaireId);
            if (!removed) return NotFound("Questionnaire not found in this campaign.");

            return Ok();
        }

        // ── PUT /api/campaign/{id}/questionnaires/reorder ────────────────────

        [HttpPut("{id}/questionnaires/reorder")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> ReorderQuestionnaires(int id, [FromBody] ReorderQuestionnairesRequest req)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.Get(id);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();

            await _repo.Campaign.ReorderQuestionnaires(
                id,
                req.Items.Select(i => (i.QuestionnaireId, i.Order)).ToList()
            );

            return Ok();
        }

        // ── GET /api/campaign/my ─────────────────────────────────────────────

        public class MyCampaignDto
        {
            public int    CampaignId          { get; set; }
            public string Name                { get; set; } = string.Empty;
            public string StartDate           { get; set; } = string.Empty;
            public string EndDate             { get; set; } = string.Empty;
            public string Status              { get; set; } = string.Empty;
            public string CandidateStatus     { get; set; } = string.Empty;
            public int    QuestionnairesCount { get; set; }
        }

        [HttpGet("my")]
        [Authorize(Roles = "Condidat")]
        public async Task<IActionResult> GetMyCampaigns()
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var links = await _db.CampaignCandidates
                .Where(cc => cc.CandidateId == user.Id && cc.Enable)
                .Include(cc => cc.Campaign)
                    .ThenInclude(c => c!.CampaignQuestionnaires)
                .ToListAsync();

            var result = links.Select(cc => new MyCampaignDto
            {
                CampaignId          = cc.CampaignId,
                Name                = cc.Campaign?.Name ?? string.Empty,
                StartDate           = cc.Campaign?.AvailableFrom.ToString("yyyy-MM-dd") ?? string.Empty,
                EndDate             = cc.Campaign?.AvailableUntil.ToString("yyyy-MM-dd") ?? string.Empty,
                Status              = cc.Campaign?.Status.ToString() ?? string.Empty,
                CandidateStatus     = cc.Status.ToString(),
                QuestionnairesCount = cc.Campaign?.CampaignQuestionnaires.Count(cq => cq.Enable) ?? 0,
            }).ToList();

            return Ok(result);
        }

        // ── GET /api/campaign/{id}/my-detail ────────────────────────────────

        public class CampaignQuestionnaireDetailDto
        {
            public int     QuestionnaireId   { get; set; }
            public string  Title             { get; set; } = string.Empty;
            public string? Label             { get; set; }
            public bool    IsRequired        { get; set; }
            public int     Order             { get; set; }
            public int     QuestionCount     { get; set; }
            public int?    DurationMinutes   { get; set; }
            public decimal PassingScore      { get; set; }
            public string  AttemptStatus     { get; set; } = "NotStarted";
            public int?    AttemptId         { get; set; }
            public decimal? Score            { get; set; }
            public bool?   Passed            { get; set; }
            public int     MaxAttempts       { get; set; } = 1;
            public int     AttemptsUsed      { get; set; } = 0;
            public int     RemainingAttempts { get; set; } = 0;
        }

        public class MyCampaignDetailDto
        {
            public int    CampaignId      { get; set; }
            public string Name            { get; set; } = string.Empty;
            public string Description     { get; set; } = string.Empty;
            public string StartDate       { get; set; } = string.Empty;
            public string EndDate         { get; set; } = string.Empty;
            public string Status          { get; set; } = string.Empty;
            public string CandidateStatus { get; set; } = string.Empty;
            public List<CampaignQuestionnaireDetailDto> Questionnaires { get; set; } = new();
        }

        [HttpGet("{id}/my-detail")]
        [Authorize(Roles = "Condidat")]
        public async Task<IActionResult> GetMyCampaignDetail(int id)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var link = await _db.CampaignCandidates
                .Where(cc => cc.CandidateId == user.Id && cc.CampaignId == id && cc.Enable)
                .Include(cc => cc.Campaign)
                    .ThenInclude(c => c!.CampaignQuestionnaires)
                        .ThenInclude(cq => cq.Questionnaire)
                            .ThenInclude(q => q!.QuestionnaireQuestions)
                .FirstOrDefaultAsync();

            if (link == null) return NotFound("Campaign not found or you are not invited.");

            // Get all attempts for this candidate + campaign
            var attempts = await _db.CandidateAttempts
                .Where(a => a.CandidateId == user.Id && a.CampaignId == id)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();

            var questionnaires = link.Campaign!.CampaignQuestionnaires
                .Where(cq => cq.Enable)
                .OrderBy(cq => cq.Order)
                .Select(cq =>
                {
                    var q = cq.Questionnaire;
                    var qAttempts = attempts.Where(a => a.QuestionnaireId == cq.QuestionnaireId).ToList();
                    var lastAttempt = qAttempts.OrderByDescending(a => a.StartedAt).FirstOrDefault();
                    var attemptsUsed = qAttempts.Count;
                    var campaign     = link.Campaign!;
                    var maxAttempts  = campaign.MaxAttempts;
                    var remaining    = maxAttempts == -1 ? -1 : Math.Max(0, maxAttempts - attemptsUsed);

                    return new CampaignQuestionnaireDetailDto
                    {
                        QuestionnaireId   = cq.QuestionnaireId,
                        Title             = q?.Title ?? "Unknown",
                        Label             = cq.Label,
                        IsRequired        = cq.IsRequired,
                        Order             = cq.Order,
                        QuestionCount     = q?.QuestionnaireQuestions.Count(qq => qq.Enable) ?? 0,
                        DurationMinutes   = campaign.GlobalDurationMinutes,
                        PassingScore      = campaign.PassingScorePercent,
                        AttemptStatus     = lastAttempt == null ? "NotStarted" : lastAttempt.Status.ToString(),
                        AttemptId         = lastAttempt?.Id,
                        Score             = lastAttempt?.Percentage,
                        Passed            = lastAttempt?.Passed,
                        MaxAttempts       = maxAttempts,
                        AttemptsUsed      = attemptsUsed,
                        RemainingAttempts = remaining,
                    };
                }).ToList();

            return Ok(new MyCampaignDetailDto
            {
                CampaignId      = id,
                Name            = link.Campaign.Name,
                Description     = link.Campaign.Description ?? string.Empty,
                StartDate       = link.Campaign.AvailableFrom.ToString("yyyy-MM-dd"),
                EndDate         = link.Campaign.AvailableUntil.ToString("yyyy-MM-dd"),
                Status          = link.Campaign.Status.ToString(),
                CandidateStatus = link.Status.ToString(),
                Questionnaires  = questionnaires,
            });
        }

        // ── GET /api/campaign/candidates/available ───────────────────────────

        [HttpGet("candidates/available")]
        [Authorize(Roles = "CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetAvailableCandidates()
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null) return Unauthorized();

            var candidates = await _repo.UserManager.GetUsersInRoleAsync("Condidat");
            var result = candidates
                .Where(c => c.CompanyId == user.CompanyId)
                .OrderBy(c => c.UserName)
                .Select(c => new
                {
                    c.Id,
                    c.UserName,
                    c.Email,
                    c.ImageUrl,
                })
                .ToList();

            return Ok(result);
        }
    }
}
