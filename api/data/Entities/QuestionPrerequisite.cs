using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("question_prerequisites")]
    public class QuestionPrerequisite : BaseEntity
    {
        public int QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        [JsonIgnore]
        public Question? Question { get; set; }

        public int RequiredThemeId { get; set; }
        [ForeignKey(nameof(RequiredThemeId))]
        public Theme? RequiredTheme { get; set; }

        public string? Note { get; set; }
    }
}
