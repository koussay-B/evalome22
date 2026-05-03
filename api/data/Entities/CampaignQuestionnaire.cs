using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("campaign_questionnaires")]
    public class CampaignQuestionnaire : BaseEntity
    {
        public int CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        [JsonIgnore]
        public Campaign? Campaign { get; set; }

        public int QuestionnaireId { get; set; }
        [ForeignKey(nameof(QuestionnaireId))]
        public Questionnaire? Questionnaire { get; set; }

        public int Order { get; set; } = 1;
        public string? Label { get; set; }
        public bool IsRequired { get; set; } = true;
    }
}
