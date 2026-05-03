using System.ComponentModel.DataAnnotations;

namespace api.dtos.request;

public class LoginDto
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
