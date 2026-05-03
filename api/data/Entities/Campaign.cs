using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("campaigns")]
    public class Campaign : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableUntil { get; set; }
        public string? AllowedTimeSlots { get; set; }
        public bool SendInviteEmail { get; set; } = true;
        public int? ReminderHoursBefore { get; set; }
        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

        // ── Test execution settings (override questionnaire defaults per campaign) ──
        public decimal PassingScorePercent { get; set; } = 60;
        public ScoringMode ScoringMode { get; set; } = ScoringMode.SumWeighted;
        public bool AllowPartialScore { get; set; } = true;
        public int? GlobalDurationMinutes { get; set; }
        public bool ShowTimer { get; set; } = true;
        public bool AllowNavigationBack { get; set; } = true;
        public bool RandomizeQuestions { get; set; } = false;
        public bool RandomizeChoices { get; set; } = false;
        public bool ShowQuestionScore { get; set; } = false;
        public bool ShowCorrectAfterEach { get; set; } = false;
        public bool ShowResultsImmediately { get; set; } = true;
        public bool AllowTabSwitch { get; set; } = false;
        public int? TabSwitchMaxCount { get; set; }
        public int MaxAttempts { get; set; } = 1;
        public int? CooldownBetweenAttemptsMinutes { get; set; }

        // ── Denormalized counters (updated on invite/submit/remove) ──────────
        public int InvitedCount   { get; set; } = 0;
        public int CompletedCount { get; set; } = 0;
        public int PassedCount    { get; set; } = 0;

        public int? ThemeId { get; set; }
        [ForeignKey(nameof(ThemeId))]
        [JsonIgnore]
        public Theme? Theme { get; set; }

        public int CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))]
        [JsonIgnore]
        public Company? Company { get; set; }

        public int CreatedById { get; set; }
        [ForeignKey(nameof(CreatedById))]
        [JsonIgnore]
        public AppUser? CreatedBy { get; set; }

        [JsonIgnore]
        public ICollection<CampaignCandidate> CampaignCandidates { get; set; } = new List<CampaignCandidate>();

        [JsonIgnore]
        public ICollection<CandidateAttempt> CandidateAttempts { get; set; } = new List<CandidateAttempt>();

        [JsonIgnore]
        public ICollection<CampaignQuestionnaire> CampaignQuestionnaires { get; set; } = new List<CampaignQuestionnaire>();

        [NotMapped]
        public int QuestionnairesCount => CampaignQuestionnaires.Count(cq => cq.Enable);
    }
}
