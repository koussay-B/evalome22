using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("candidate_attempts")]
    public class CandidateAttempt : BaseEntity
    {
        public int CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        [JsonIgnore]
        public Campaign? Campaign { get; set; }

        public int? QuestionnaireId { get; set; }
        [ForeignKey(nameof(QuestionnaireId))]
        [JsonIgnore]
        public Questionnaire? Questionnaire { get; set; }

        public int CampaignCandidateId { get; set; }
        [ForeignKey(nameof(CampaignCandidateId))]
        [JsonIgnore]
        public CampaignCandidate? CampaignCandidate { get; set; }

        public int CandidateId { get; set; }
        [ForeignKey(nameof(CandidateId))]
        public AppUser? Candidate { get; set; }

        public int AttemptNumber { get; set; } = 1;
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;
        public int TabSwitchCount { get; set; } = 0;
        public string? FocusLostEvents { get; set; }
        public decimal RawScore { get; set; }
        public decimal MaxPossibleScore { get; set; }
        public decimal Percentage { get; set; }
        public bool Passed { get; set; }
        public string? QuestionsSnapshot { get; set; }

        public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
        public AttemptReport? Report { get; set; }
    }
}
