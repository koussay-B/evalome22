using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using api.data.Entities;
using api.data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
    public class AiController(IRepositoryWrapper repo, IHttpClientFactory httpClientFactory) : ControllerBase
    {
        private readonly IRepositoryWrapper _repo = repo;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        // ── Request/Response DTOs ─────────────────────────────────────────────

        public record GenerateQuestionsRequest(
            int? ModelId,
            int Count,
            GenerationContext Context
        );

        public record GenerationContext(
            string QuestionnaireTitle,
            string? QuestionnaireDescription,
            int? ThemeId,
            string? ThemeName,
            string? Language,
            List<string>? ExistingQuestions,
            string? Difficulty,
            List<string>? Topics,
            string? CustomContext
        );

        public record GeneratedChoiceDto(
            string Text,
            bool IsCorrect,
            int Order
        );

        public record GeneratedQuestionDto(
            string Title,
            string Type,
            string Complexity,
            double Points,
            int? TimeLimitSeconds,
            string? Explanation,
            List<GeneratedChoiceDto> Choices
        );

        // ── Endpoint ──────────────────────────────────────────────────────────

        [HttpPost("generate-questions")]
        public async Task<IActionResult> GenerateQuestions([FromBody] GenerateQuestionsRequest request)
        {
            // Load AI model
            AiModel? aiModel = request.ModelId.HasValue
                ? await _repo.AiModel.Get(request.ModelId.Value)
                : await _repo.AiModel.GetDefault();

            if (aiModel == null)
                return BadRequest("No AI model configured. Please add an AI model in admin settings.");

            if (!aiModel.IsEnabled)
                return BadRequest("The selected AI model is disabled.");

            var count = Math.Clamp(request.Count, 1, 20);
            var lang = string.IsNullOrWhiteSpace(request.Context.Language) ? "English" : request.Context.Language;
            var existing = request.Context.ExistingQuestions?.Count > 0
                ? $"\nAlready existing questions (DO NOT duplicate):\n{string.Join("\n", request.Context.ExistingQuestions.Select(q => $"- {q}"))}"
                : string.Empty;
            var difficultyLine = !string.IsNullOrWhiteSpace(request.Context.Difficulty)
                ? $"\nDifficulty level: {request.Context.Difficulty}"
                : string.Empty;
            var topicsLine = request.Context.Topics?.Count > 0
                ? $"\nTopics to focus on: {string.Join(", ", request.Context.Topics)}"
                : string.Empty;
            var customLine = !string.IsNullOrWhiteSpace(request.Context.CustomContext)
                ? $"\nAdditional context: {request.Context.CustomContext}"
                : string.Empty;

            var systemPrompt = $"""
                You are an expert assessment question generator. Generate exactly {count} quiz questions in {lang}.
                Return ONLY a valid JSON array — no markdown, no explanation, no extra text.

                Each question object must have these fields:
                - title (string): the question text
                - type (string): exactly one of "QCU" (single correct), "QCM" (multiple correct), or "TrueFalse"
                - complexity (string): exactly one of "Beginner", "Intermediate", "Advanced", "Expert"
                - points (number): 1 for Beginner/TrueFalse, 2 for Intermediate, 3 for Advanced or Expert
                - timeLimitSeconds (number): seconds to answer — 30 for Beginner, 45 for Intermediate, 60 for Advanced, 90 for Expert
                - explanation (string|null): optional explanation shown after the answer
                - choices (array): for QCU/QCM — 4 objects with text (string), isCorrect (bool), order (0-indexed int); for TrueFalse — 2 objects ["True","False"] with isCorrect set appropriately

                Rules:
                - QCU must have exactly 1 correct choice
                - QCM must have 2-3 correct choices
                - TrueFalse choices must be exactly ["True","False"]
                - All choices must be plausible, not obviously wrong
                - Questions must be relevant to the topic
                """;

            var userMessage = $"""
                Topic: {request.Context.QuestionnaireTitle}
                {(string.IsNullOrWhiteSpace(request.Context.QuestionnaireDescription) ? string.Empty : $"Description: {request.Context.QuestionnaireDescription}")}
                {(string.IsNullOrWhiteSpace(request.Context.ThemeName) ? string.Empty : $"Theme/Category: {request.Context.ThemeName}")}
                {difficultyLine}
                {topicsLine}
                {customLine}
                {existing}

                Generate {count} high-quality assessment questions on this topic.
                """;

            try
            {
                var questions = aiModel.Provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase)
                    ? await CallAnthropicAsync(aiModel, systemPrompt, userMessage)
                    : await CallOpenAiCompatibleAsync(aiModel, systemPrompt, userMessage);

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return BadRequest($"AI generation failed: {ex.Message}");
            }
        }

        // ── Anthropic API ─────────────────────────────────────────────────────

        private async Task<List<GeneratedQuestionDto>> CallAnthropicAsync(AiModel model, string system, string user)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", model.ApiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var baseUrl = string.IsNullOrWhiteSpace(model.ApiBaseUrl)
                ? "https://api.anthropic.com"
                : model.ApiBaseUrl.TrimEnd('/');

            var body = new
            {
                model = model.ModelIdentifier,
                max_tokens = 4096,
                system,
                messages = new[] { new { role = "user", content = user } }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{baseUrl}/v1/messages", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Anthropic API error {response.StatusCode}: {responseText}");

            using var doc = JsonDocument.Parse(responseText);
            var textContent = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "[]";

            return ParseGeneratedQuestions(textContent);
        }

        // ── OpenAI-Compatible API ─────────────────────────────────────────────

        private async Task<List<GeneratedQuestionDto>> CallOpenAiCompatibleAsync(AiModel model, string system, string user)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", model.ApiKey);

            var baseUrl = string.IsNullOrWhiteSpace(model.ApiBaseUrl)
                ? "https://api.openai.com"
                : model.ApiBaseUrl.TrimEnd('/');

            var body = new
            {
                model = model.ModelIdentifier,
                messages = new[]
                {
                    new { role = "system", content = system },
                    new { role = "user",   content = user }
                },
                temperature = 0.7,
                max_tokens = 4096
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{baseUrl}/v1/chat/completions", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI API error {response.StatusCode}: {responseText}");

            using var doc = JsonDocument.Parse(responseText);
            var textContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "[]";

            return ParseGeneratedQuestions(textContent);
        }

        // ── JSON Parser ───────────────────────────────────────────────────────

        private static List<GeneratedQuestionDto> ParseGeneratedQuestions(string rawText)
        {
            // Strip markdown code fences if present
            var text = rawText.Trim();
            if (text.StartsWith("```"))
            {
                var start = text.IndexOf('[');
                var end = text.LastIndexOf(']');
                if (start >= 0 && end > start)
                    text = text[start..(end + 1)];
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var questions = JsonSerializer.Deserialize<List<GeneratedQuestionDto>>(text, options);
            return questions ?? [];
        }
    }
}
