using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using api.data.Entities;
using api.data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/ai-model")]
    [Authorize(Roles = "Admin")]
    public class AiModelController(IRepositoryWrapper repo, IHttpClientFactory httpClientFactory) : ControllerBase
    {
        private readonly IRepositoryWrapper _repo = repo;
        private readonly IHttpClientFactory _http = httpClientFactory;

        // ── DTOs ────────────────────────────────────────────────────────────

        public record AiModelDto(
            int Id,
            string Name,
            string Provider,
            string ModelIdentifier,
            string ApiKeyMasked,
            string? ApiBaseUrl,
            bool IsDefault,
            bool IsEnabled,
            DateTime CreatedAt,
            DateTime? UpdatedAt
        );

        public record AiModelPayload(
            string Name,
            string Provider,
            string ModelIdentifier,
            string ApiKey,
            string? ApiBaseUrl,
            bool IsDefault,
            bool IsEnabled
        );

        private static AiModelDto ToDto(AiModel m) => new(
            m.Id,
            m.Name,
            m.Provider,
            m.ModelIdentifier,
            MaskKey(m.ApiKey),
            m.ApiBaseUrl,
            m.IsDefault,
            m.IsEnabled,
            m.CreatedAt,
            m.UpdatedAt
        );

        private static string MaskKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (key.Length <= 4) return new string('*', key.Length);
            return "****" + key[^4..];
        }

        // ── Endpoints ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var models = await _repo.AiModel.GetAll();
            return Ok(models.Select(ToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var model = await _repo.AiModel.Get(id);
            if (model == null) return NotFound();
            return Ok(ToDto(model));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AiModelPayload payload)
        {
            if (payload.IsDefault)
                await _repo.AiModel.UnsetAllDefaults();

            var model = new AiModel
            {
                Name            = payload.Name,
                Provider        = payload.Provider,
                ModelIdentifier = payload.ModelIdentifier,
                ApiKey          = payload.ApiKey,
                ApiBaseUrl      = payload.ApiBaseUrl,
                IsDefault       = payload.IsDefault,
                IsEnabled       = payload.IsEnabled,
            };

            var created = await _repo.AiModel.Create(model);
            return Ok(ToDto(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AiModelPayload payload)
        {
            var model = await _repo.AiModel.Get(id);
            if (model == null) return NotFound();

            if (payload.IsDefault)
                await _repo.AiModel.UnsetAllDefaults();

            model.Name            = payload.Name;
            model.Provider        = payload.Provider;
            model.ModelIdentifier = payload.ModelIdentifier;
            model.ApiBaseUrl      = payload.ApiBaseUrl;
            model.IsDefault       = payload.IsDefault;
            model.IsEnabled       = payload.IsEnabled;

            // Only update ApiKey if a new non-masked value is provided
            if (!string.IsNullOrWhiteSpace(payload.ApiKey) && !payload.ApiKey.StartsWith("****"))
                model.ApiKey = payload.ApiKey;

            var updated = await _repo.AiModel.Update(model);
            return Ok(ToDto(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _repo.AiModel.Get(id);
            if (model == null) return NotFound();
            await _repo.AiModel.Delete(id);
            return Ok();
        }

        [HttpPut("{id}/set-default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var model = await _repo.AiModel.Get(id);
            if (model == null) return NotFound();

            await _repo.AiModel.UnsetAllDefaults();
            model.IsDefault = true;
            var updated = await _repo.AiModel.Update(model);
            return Ok(ToDto(updated));
        }

        // ── Test Connection ──────────────────────────────────────────────────

        public record TestModelRequest(
            string Provider,
            string ModelIdentifier,
            string ApiKey,
            string? ApiBaseUrl,
            int? ModelId
        );

        public record TestModelResult(bool Success, string Message);

        [HttpPost("test-inline")]
        public async Task<IActionResult> TestInline([FromBody] TestModelRequest request)
        {
            var apiKey = request.ApiKey;

            // If the key is masked, load the real one from the saved model
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("****"))
            {
                if (!request.ModelId.HasValue)
                    return Ok(new TestModelResult(false, "Provide an API key or select a saved model."));

                var saved = await _repo.AiModel.Get(request.ModelId.Value);
                if (saved == null) return Ok(new TestModelResult(false, "Model not found."));
                apiKey = saved.ApiKey;
            }

            try
            {
                var isAnthropic = request.Provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase);
                if (isAnthropic)
                    await PingAnthropicAsync(request.ModelIdentifier, apiKey, request.ApiBaseUrl);
                else
                    await PingOpenAiCompatibleAsync(request.ModelIdentifier, apiKey, request.ApiBaseUrl);

                return Ok(new TestModelResult(true, "Connection successful"));
            }
            catch (Exception ex)
            {
                return Ok(new TestModelResult(false, ex.Message));
            }
        }

        private async Task PingAnthropicAsync(string modelId, string apiKey, string? baseUrl)
        {
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var url = (string.IsNullOrWhiteSpace(baseUrl) ? "https://api.anthropic.com" : baseUrl.TrimEnd('/'))
                      + "/v1/messages";

            var body = JsonSerializer.Serialize(new
            {
                model = modelId,
                max_tokens = 10,
                messages = new[] { new { role = "user", content = "Say ok" } }
            });

            var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"HTTP {(int)response.StatusCode}: {err[..Math.Min(200, err.Length)]}");
            }
        }

        private async Task PingOpenAiCompatibleAsync(string modelId, string apiKey, string? baseUrl)
        {
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var url = (string.IsNullOrWhiteSpace(baseUrl) ? "https://api.openai.com" : baseUrl.TrimEnd('/'))
                      + "/v1/chat/completions";

            var body = JsonSerializer.Serialize(new
            {
                model = modelId,
                max_tokens = 10,
                messages = new[] { new { role = "user", content = "Say ok" } }
            });

            var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"HTTP {(int)response.StatusCode}: {err[..Math.Min(200, err.Length)]}");
            }
        }
    }
}
