using System.ComponentModel.DataAnnotations;

namespace api.DTOs.request
{
    public class CompanyRequestDto
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Requester name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Requester name must be between 2 and 100 characters")]
        public string RequesterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Requester email is required")]
        [EmailAddress(ErrorMessage = "Invalid requester email address")]
        public string RequesterEmail { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
    }
}
