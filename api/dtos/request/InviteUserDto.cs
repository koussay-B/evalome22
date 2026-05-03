using System.ComponentModel.DataAnnotations;

namespace api.DTOs.request
{
    public class InviteUserDto
    {
        [Required]
        [MinLength(3)]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Role { get; set; }

        /// <summary>
        /// Required for Admin. For CompanyAdmin/Formateur it is auto-resolved from the caller's company.
        /// </summary>
        public int? CompanyId { get; set; }
    }
}
