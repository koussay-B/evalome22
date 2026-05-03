using api.data.Entities;
using api.services.TokenService;
using api.dtos.request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

public class AuthController(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IWebHostEnvironment env) : BaseApiController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Normalize the input for lookup
        string normalizedInput = dto.Username.ToUpper();

        var user = await userManager.Users
              .Include(u => u.Company)
              .FirstOrDefaultAsync(x =>
                  x.NormalizedUserName == normalizedInput ||
                  x.NormalizedEmail == normalizedInput);

        if (user == null)
        {
            return Unauthorized("Invalid username or email.");
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);

        // En mode dev : si le mot de passe échoue, essayer avec le mot de passe par défaut du seed
        if (!passwordValid && env.IsDevelopment())
        {
            passwordValid = await userManager.CheckPasswordAsync(user, "Password");
        }

        if (!passwordValid)
        {
            return Unauthorized("Invalid username/email or password.");
        }

        // Vérifier si l'utilisateur est désactivé
        if (user.LockoutEnabled && user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            return Unauthorized("Votre compte a été désactivé. Veuillez contacter l'administrateur.");
        }

        // Get user role (single)
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "";
        var redirectTo = GetRedirectUrlByRole(role);

        return Ok(new
        {
            Token = await tokenService.CreateToken(user),
            RedirectTo = redirectTo,
            User = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.ImageUrl,
                user.PhoneNumber,
                user.CompanyId,
                Role = role,
            },
            CurrentCompany = user.Company == null ? null : new
            {
                user.Company.Id,
                user.Company.Name,
                user.Company.Logo,
                user.Company.Email,
                user.Company.Phone,
                user.Company.Website,
                user.Company.Address,
                user.Company.IsActive,
            },
        });
    }

    [HttpGet("dev/users")]
    public async Task<IActionResult> GetDevUsers()
    {
        if (!env.IsDevelopment())
            return NotFound();

        var users = await userManager.Users.ToListAsync();
        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.ImageUrl,
                Roles = roles
            });
        }

        return Ok(result);
    }

    private static string GetRedirectUrlByRole(string role)
    {
        return role switch
        {
            "Admin"        => "/admin",
            "CompanyAdmin" => "/company",
            "Formateur"    => "/formateur",
            "Condidat"     => "/candidat",
            _              => "/candidat"
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
    }
}
