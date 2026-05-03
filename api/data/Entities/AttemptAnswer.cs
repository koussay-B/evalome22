using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("attempt_answers")]
    public class AttemptAnswer : BaseEntity
    {
        public int AttemptId { get; set; }
        [ForeignKey(nameof(AttemptId))]
        [JsonIgnore]
        public CandidateAttempt? Attempt { get; set; }

        public int QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public Question? Question { get; set; }

        public string? SelectedChoiceIds { get; set; }
        public int TimeSpentSeconds { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public bool IsCorrect { get; set; }
        public decimal? PartialScore { get; set; }
        public decimal EarnedPoints { get; set; }
        public decimal MaxPoints { get; set; }
    }
}
