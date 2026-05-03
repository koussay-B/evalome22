using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api.data.Entities
{
    [Table("themes")]
    public class Theme : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int Order { get; set; } = 0;

        public int? ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        [JsonIgnore]
        public Theme? Parent { get; set; }

        public ICollection<Theme> Children { get; set; } = new List<Theme>();

        public int CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))]
        [JsonIgnore]
        public Company? Company { get; set; }

        public int CreatedById { get; set; }
        [ForeignKey(nameof(CreatedById))]
        [JsonIgnore]
        public AppUser? CreatedBy { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
