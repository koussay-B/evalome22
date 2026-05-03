using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("certificates")]
    public class Certificate : BaseEntity
    {
        public int CandidateId { get; set; }
        [ForeignKey(nameof(CandidateId))]
        [JsonIgnore]
        public AppUser? Candidate { get; set; }

        public int CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        [JsonIgnore]
        public Campaign? Campaign { get; set; }

        public int AttemptId { get; set; }
        [ForeignKey(nameof(AttemptId))]
        [JsonIgnore]
        public CandidateAttempt? Attempt { get; set; }

        public decimal Percentage { get; set; }
        public string CertificateCode { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}
