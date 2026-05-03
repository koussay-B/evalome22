using System.ComponentModel.DataAnnotations;

namespace api.DTOs.request
{
    public class RejectCompanyRequestDto
    {
        [Required(ErrorMessage = "Rejection reason is required")]
        [MinLength(5, ErrorMessage = "Rejection reason must be at least 5 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}
