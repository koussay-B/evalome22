namespace api.data.Entities
{
    public class Company : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? RequesterName { get; set; }
        public string? RequesterEmail { get; set; }
        public CompanyStatus Status { get; set; } = CompanyStatus.Pending;
        public string? RejectionReason { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }

        public bool IsActive { get; set; } = false;
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}
