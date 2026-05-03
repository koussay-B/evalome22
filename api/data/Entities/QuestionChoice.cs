using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("question_choices")]
    public class QuestionChoice : BaseEntity
    {
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
        public int Order { get; set; } = 0;
        public string? Explanation { get; set; }

        public int QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        [JsonIgnore]
        public Question? Question { get; set; }
    }
}
