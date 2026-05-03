using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace api.services.TokenService;

public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<string> CreateToken(AppUser user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            var tokenKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("TokenKey is missing in configuration.");
            //if (tokenKey.Length < 64)
            //{
            //    throw new InvalidOperationException("TokenKey must be at least 64 characters long for security.");
            //}

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            if (user.UserName == null)
            {
                throw new InvalidOperationException("User must have a valid username.");
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
