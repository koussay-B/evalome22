using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("attempt_reports")]
    public class AttemptReport : BaseEntity
    {
        public int AttemptId { get; set; }
        [ForeignKey(nameof(AttemptId))]
        [JsonIgnore]
        public CandidateAttempt? Attempt { get; set; }

        public DateTime GeneratedAt { get; set; }
        public string? ThemeScores { get; set; }
        public string? AiRecommendations { get; set; }
        public string? AiPrerequisiteGaps { get; set; }
        public string? AiStrengths { get; set; }
        public decimal? CampaignPercentile { get; set; }
        public decimal? GroupAverageScore { get; set; }
        public string? PdfExportUrl { get; set; }
    }
}
