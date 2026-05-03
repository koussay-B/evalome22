using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("questionnaires")]
    public class Questionnaire : BaseEntity
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? Instructions { get; set; }
        public string? CoverImageUrl { get; set; }
        public QuestionnaireStatus Status { get; set; } = QuestionnaireStatus.Draft;

        public int CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))]
        [JsonIgnore]
        public Company? Company { get; set; }

        public int CreatedById { get; set; }
        [ForeignKey(nameof(CreatedById))]
        [JsonIgnore]
        public AppUser? CreatedBy { get; set; }

        public ICollection<QuestionnaireQuestion> QuestionnaireQuestions { get; set; } = new List<QuestionnaireQuestion>();

        [JsonIgnore]
        public ICollection<CampaignQuestionnaire> CampaignQuestionnaires { get; set; } = new List<CampaignQuestionnaire>();
    }
}
