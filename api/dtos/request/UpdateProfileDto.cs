using System.ComponentModel.DataAnnotations;

namespace api.DTOs.request
{
    public class UpdateProfileDto
    {
        [Required]
        [MinLength(3)]
        public required string UserName { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class UpdateAvatarDto
    {
        [Required]
        public required string ImageUrl { get; set; }
    }
}
