using System.ComponentModel.DataAnnotations;

namespace api.DTOs.request
{
    public class UpdateCompanyDto
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Website { get; set; }

        public string? Address { get; set; }

        public string? Logo { get; set; }

        public string? RequesterName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid requester email address")]
        public string? RequesterEmail { get; set; }
    }
}
