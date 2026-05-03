using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("questions")]
    public class Question : BaseEntity
    {
        public required string Title { get; set; }
        public string? Explanation { get; set; }
        public string? Hint { get; set; }
        public QuestionType Type { get; set; }
        public ComplexityLevel Complexity { get; set; }
        public int? TimeLimitSeconds { get; set; }
        public decimal Points { get; set; } = 1;

        public int CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))]
        [JsonIgnore]
        public Company? Company { get; set; }

        public int CreatedById { get; set; }
        [ForeignKey(nameof(CreatedById))]
        [JsonIgnore]
        public AppUser? CreatedBy { get; set; }

        public int ThemeId { get; set; }
        [ForeignKey(nameof(ThemeId))]
        [JsonIgnore]
        public Theme? Theme { get; set; }

        public ICollection<QuestionChoice> Choices { get; set; } = new List<QuestionChoice>();
        public ICollection<QuestionPrerequisite> Prerequisites { get; set; } = new List<QuestionPrerequisite>();

        [JsonIgnore]
        public ICollection<QuestionnaireQuestion> QuestionnaireQuestions { get; set; } = new List<QuestionnaireQuestion>();
    }
}
