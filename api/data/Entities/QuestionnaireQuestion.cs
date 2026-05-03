using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("questionnaire_questions")]
    public class QuestionnaireQuestion : BaseEntity
    {
        public int QuestionnaireId { get; set; }
        [ForeignKey(nameof(QuestionnaireId))]
        [JsonIgnore]
        public Questionnaire? Questionnaire { get; set; }

        public int QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public Question? Question { get; set; }

        public int Order { get; set; }
        public decimal Weight { get; set; } = 1.0m;
        public bool IsMandatory { get; set; } = true;
        public int? PoolGroup { get; set; }
    }
}
