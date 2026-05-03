using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("campaign_candidates")]
    public class CampaignCandidate : BaseEntity
    {
        public int CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        [JsonIgnore]
        public Campaign? Campaign { get; set; }

        public int CandidateId { get; set; }
        [ForeignKey(nameof(CandidateId))]
        public AppUser? Candidate { get; set; }

        public DateTime InvitedAt { get; set; }
        public string? InviteToken { get; set; }
        public CampaignCandidateStatus Status { get; set; } = CampaignCandidateStatus.Invited;

        public ICollection<CandidateAttempt> Attempts { get; set; } = new List<CandidateAttempt>();
    }
}
