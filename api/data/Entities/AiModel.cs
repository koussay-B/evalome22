using System.ComponentModel.DataAnnotations;

namespace api.data.Entities
{
    public class AiModel : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>OpenAI | Anthropic | Other</summary>
        [Required]
        public string Provider { get; set; } = string.Empty;

        /// <summary>e.g. gpt-4o, claude-opus-4-5</summary>
        [Required]
        public string ModelIdentifier { get; set; } = string.Empty;

        [Required]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Optional override — used for Azure, local LLMs, or OpenAI-compatible providers</summary>
        public string? ApiBaseUrl { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsEnabled { get; set; } = true;
    }
}
