using System.ComponentModel.DataAnnotations;

namespace api.DTOs.request
{
    public class AdminUpdateUserDto
    {
        [Required]
        [MinLength(3)]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Role { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
