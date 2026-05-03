using api.data;
using api.data.Entities;
using api.data.Repositories;
using api.DTOs.request;
using api.DTOs.response;
using api.Extensions;
using api.Helper;
using api.services.EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Authorize]
    public class UsersController(
        IRepositoryWrapper repo,
        IEmailService emailService,
        IConfiguration config,
        ApplicationContext db) : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager = repo.UserManager;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _config = config;
        private readonly ApplicationContext _db = db;

        // ── Roles permitted for each caller role ──────────────────────────────
        private static readonly string[] AdminAllowedRoles = ["Admin", "CompanyAdmin", "Formateur", "Condidat"];
        private static readonly string[] LimitedAllowedRoles = ["Formateur", "Condidat"];

        // GET /api/users
        [HttpGet]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers(
            [FromQuery] UserSearchParams p)
        {
            var query = _userManager.Users.AsQueryable();

            if (p.IncludeCompany)
                query = query.Include(u => u.Company);

            if (User.IsInRole("CompanyAdmin") || User.IsInRole("Formateur"))
            {
                var userId = User.GetUserId();
                var currentUser = await _userManager.FindByIdAsync(userId.ToString());
                if (currentUser != null && currentUser.CompanyId.HasValue)
                    query = query.Where(u => u.CompanyId == currentUser.CompanyId);
                else
                    return Ok(new List<UserDto>());
            }

            if (!string.IsNullOrWhiteSpace(p.Role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(p.Role);
                var ids = usersInRole.Select(u => u.Id).ToHashSet();
                query = query.Where(u => ids.Contains(u.Id));
            }

            if (!string.IsNullOrWhiteSpace(p.Search))
            {
                var term = $"%{p.Search.Trim().ToLower()}%";
                query = query.Where(u =>
                    (u.UserName != null && EF.Functions.ILike(u.UserName, term)) ||
                    (u.Email != null && EF.Functions.ILike(u.Email, term)));
            }

            query = p.OrderBy switch
            {
                "email" => query.OrderBy(u => u.Email),
                "email_desc" => query.OrderByDescending(u => u.Email),
                "username_desc" => query.OrderByDescending(u => u.UserName),
                _ => query.OrderBy(u => u.UserName),
            };

            var result = await PaginationHelper.CreateAsync(query, p.PageNumber, p.PageSize);

            var dtos = new List<UserDto>(result.Items.Count);
            foreach (var user in result.Items)
            {
                var roles = await _userManager.GetRolesAsync(user);
                dtos.Add(ToDto(user, roles.FirstOrDefault() ?? string.Empty));
            }

            AddPaginationHeaders(result.Metadata);
            return Ok(dtos);
        }

        // POST /api/users/invite
        [HttpPost("invite")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ── Resolve caller ────────────────────────────────────────────────
            var callerUserId = User.GetUserId();
            var callerUser = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == callerUserId);

            var callerRole = User.IsInRole("Admin") ? "Admin"
                           : User.IsInRole("CompanyAdmin") ? "CompanyAdmin"
                           : "Formateur";

            // ── Validate assigned role ────────────────────────────────────────
            var allowedRoles = callerRole == "Admin" ? AdminAllowedRoles : LimitedAllowedRoles;
            if (!allowedRoles.Contains(dto.Role))
                return BadRequest($"You are not allowed to assign the '{dto.Role}' role.");

            // ── Resolve company ───────────────────────────────────────────────
            int? companyId = callerRole == "Admin"
                ? dto.CompanyId
                : callerUser?.CompanyId;

            // ── Uniqueness checks ─────────────────────────────────────────────
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return BadRequest("An account with this email already exists.");
            if (await _userManager.FindByNameAsync(dto.UserName) != null)
                return BadRequest("An account with this username already exists.");

            // ── Create user ───────────────────────────────────────────────────
            var tempPassword = GenerateSecurePassword();
            var appUser = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                CompanyId = companyId,
            };

            var createResult = await _userManager.CreateAsync(appUser, tempPassword);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(appUser, dto.Role);

            // ── Send welcome email ────────────────────────────────────────────
            var frontendUrl = _config["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            var companyName = callerUser?.Company?.Name ?? "the platform";

            try
            {
                await _emailService.SendWithTemplate(new SendTemplatedEmailRequest
                {
                    ToEmail = dto.Email,
                    Subject = "Welcome to CandidatApp — Your account is ready",
                    TemplatePath = "templates/WelcomeEmail.html",
                    Variables = new Dictionary<string, string>
                    {
                        { "firstname",   dto.UserName },
                        { "email",       dto.Email },
                        { "username",    dto.UserName },
                        { "password",    tempPassword },
                        { "role",        dto.Role },
                        { "companyName", companyName },
                        { "setupUrl",    $"{frontendUrl}/login" },
                        { "supportUrl",  $"{frontendUrl}/support" },
                    }
                });
            }
            catch
            {
                // Email failure is non-fatal — user is created, log and continue
            }

            // ── Return ────────────────────────────────────────────────────────
            var createdUser = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == appUser.Id);

            var assignedRoles = await _userManager.GetRolesAsync(appUser);
            return Ok(ToDto(createdUser ?? appUser, assignedRoles.FirstOrDefault() ?? dto.Role));
        }

        // PATCH /api/users/me/profile
        [HttpPatch("me/profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            // Username uniqueness (exclude self)
            var existingName = await _userManager.FindByNameAsync(dto.UserName);
            if (existingName != null && existingName.Id != userId)
                return BadRequest("Username is already taken.");

            // Email uniqueness (exclude self)
            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail != null && existingEmail.Id != userId)
                return BadRequest("Email is already in use.");

            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.NormalizedUserName = _userManager.NormalizeName(dto.UserName);
            user.NormalizedEmail = _userManager.NormalizeEmail(dto.Email);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(ToDto(user, roles.FirstOrDefault() ?? string.Empty));
        }

        // PATCH /api/users/me/avatar
        [HttpPatch("me/avatar")]
        public async Task<IActionResult> UpdateMyAvatar([FromBody] UpdateAvatarDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            user.ImageUrl = dto.ImageUrl;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return Ok(new { imageUrl = user.ImageUrl });
        }

        // PATCH /api/users/me/password
        [HttpPatch("me/password")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description).FirstOrDefault()
                                  ?? "Password change failed.");

            return Ok(new { message = "Password updated successfully." });
        }

        // PATCH /api/users/{id}
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var caller = await _userManager.GetUserAsync(User);
            var target = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (caller == null || target == null) return NotFound();
            if (caller.Id == target.Id) return BadRequest("Use your account settings to update your own profile.");

            var callerRole = GetCallerRole();
            var targetRole = await GetPrimaryRoleAsync(target);

            if (!CanManageTarget(callerRole, caller.CompanyId, target.CompanyId, targetRole))
                return Forbid();

            if (!CanAssignRole(callerRole, dto.Role))
                return BadRequest($"You are not allowed to assign the '{dto.Role}' role.");

            var existingName = await _userManager.FindByNameAsync(dto.UserName);
            if (existingName != null && existingName.Id != id)
                return BadRequest("Username is already taken.");

            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail != null && existingEmail.Id != id)
                return BadRequest("Email is already in use.");

            target.UserName = dto.UserName.Trim();
            target.Email = dto.Email.Trim();
            target.NormalizedUserName = _userManager.NormalizeName(target.UserName);
            target.NormalizedEmail = _userManager.NormalizeEmail(target.Email);
            target.LockoutEnd = dto.IsActive ? null : DateTimeOffset.MaxValue;
            target.LockoutEnabled = !dto.IsActive;

            var updateResult = await _userManager.UpdateAsync(target);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors.Select(e => e.Description));

            if (!string.Equals(targetRole, dto.Role, StringComparison.Ordinal))
            {
                var currentRoles = await _userManager.GetRolesAsync(target);
                if (currentRoles.Count > 0)
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(target, currentRoles);
                    if (!removeResult.Succeeded)
                        return BadRequest(removeResult.Errors.Select(e => e.Description));
                }

                var addRoleResult = await _userManager.AddToRoleAsync(target, dto.Role);
                if (!addRoleResult.Succeeded)
                    return BadRequest(addRoleResult.Errors.Select(e => e.Description));
            }

            return Ok(ToDto(target, dto.Role));
        }

        // POST /api/users/{id}/reset-password
        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> ResetUserPassword(int id)
        {
            var caller = await _userManager.GetUserAsync(User);
            var target = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (caller == null || target == null) return NotFound();

            var callerRole = GetCallerRole();
            var targetRole = await GetPrimaryRoleAsync(target);

            if (!CanManageTarget(callerRole, caller.CompanyId, target.CompanyId, targetRole))
                return Forbid();

            var tempPassword = GenerateSecurePassword();
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(target);
            var resetResult = await _userManager.ResetPasswordAsync(target, resetToken, tempPassword);

            if (!resetResult.Succeeded)
                return BadRequest(resetResult.Errors.Select(e => e.Description));

            target.LockoutEnd = null;
            target.LockoutEnabled = false;
            await _userManager.UpdateAsync(target);

            await SendResetPasswordEmailAsync(target, tempPassword, targetRole);

            return Ok(new { message = "Temporary password generated and emailed successfully." });
        }

        // DELETE /api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var caller = await _userManager.GetUserAsync(User);
            var target = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (caller == null || target == null) return NotFound();
            if (caller.Id == target.Id) return BadRequest("You cannot delete your own account.");

            var callerRole = GetCallerRole();
            var targetRole = await GetPrimaryRoleAsync(target);

            if (!CanManageTarget(callerRole, caller.CompanyId, target.CompanyId, targetRole))
                return Forbid();

            var deleteResult = await _userManager.DeleteAsync(target);
            if (!deleteResult.Succeeded)
                return BadRequest(deleteResult.Errors.Select(e => e.Description));

            return NoContent();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static UserDto ToDto(AppUser u, string role) => new()
        {
            Id = u.Id,
            UserName = u.UserName ?? string.Empty,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            ImageUrl = u.ImageUrl,
            Role = role,
            IsActive = !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.UtcNow,
            CompanyId = u.CompanyId,
            CompanyName = u.Company?.Name,
            CompanyLogo = u.Company?.Logo,
        };

        private static string GenerateSecurePassword()
        {
            const string lower = "abcdefghijkmnpqrstuvwxyz";
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string digits = "23456789";
            const string special = "!@#$";
            const string all = lower + upper + digits + special;

            Span<byte> bytes = stackalloc byte[16];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);

            var chars = new char[12];
            chars[0] = lower[bytes[0] % lower.Length];
            chars[1] = upper[bytes[1] % upper.Length];
            chars[2] = digits[bytes[2] % digits.Length];
            chars[3] = special[bytes[3] % special.Length];
            for (int i = 4; i < 12; i++)
                chars[i] = all[bytes[i] % all.Length];

            // Fisher-Yates shuffle
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            for (int i = 11; i > 0; i--)
            {
                int j = bytes[i] % (i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }

        // ── GET /api/users/{id}/stats ────────────────────────────────────────
        [HttpGet("{id}/stats")]
        [Authorize(Roles = "Admin,CompanyAdmin,Formateur")]
        public async Task<IActionResult> GetUserStats(int id)
        {
            var caller = await _userManager.GetUserAsync(User);
            var target = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (target == null) return NotFound();

            var callerRole = GetCallerRole();
            var targetRole = await GetPrimaryRoleAsync(target);

            if (!CanManageTarget(callerRole, caller?.CompanyId, target.CompanyId, targetRole, allowAdminEverywhere: true))
                return Forbid();

            if (targetRole == "Condidat")
            {
                var attempts = await _db.CandidateAttempts
                    .Where(a => a.CandidateId == id && a.Enable)
                    .ToListAsync();
                var active = await _db.CampaignCandidates
                    .CountAsync(cc => cc.CandidateId == id && cc.Enable
                        && (cc.Status == CampaignCandidateStatus.Invited
                            || cc.Status == CampaignCandidateStatus.InProgress));
                var certs = await _db.Certificates
                    .CountAsync(c => c.CandidateId == id && c.Enable);

                return Ok(new
                {
                    totalAttempts      = attempts.Count,
                    passedAttempts     = attempts.Count(a => a.Passed),
                    activeCampaigns    = active,
                    certificatesEarned = certs,
                });
            }

            if (targetRole == "Formateur")
            {
                var campaigns = await _db.Campaigns
                    .CountAsync(c => c.CreatedById == id && c.Enable);
                var quests = await _db.Questionnaires
                    .CountAsync(q => q.CreatedById == id && q.Enable);
                var managed = await _db.CampaignCandidates
                    .CountAsync(cc => cc.Campaign!.CreatedById == id && cc.Enable);

                return Ok(new
                {
                    campaignsCreated        = campaigns,
                    questionnairesCreated   = quests,
                    totalCandidatesManaged  = managed,
                });
            }

            if (targetRole == "CompanyAdmin")
            {
                if (!target.CompanyId.HasValue)
                    return Ok(new { });

                var companyId = target.CompanyId.Value;
                var usersInCompany = await _userManager.Users
                    .Where(u => u.CompanyId == companyId)
                    .ToListAsync();
                var activeUsers = usersInCompany.Count(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.UtcNow);

                var formateurs = 0;
                foreach (var user in usersInCompany)
                {
                    var role = await GetPrimaryRoleAsync(user);
                    if (role == "Formateur") formateurs++;
                }

                var activeCampaigns = await _db.Campaigns.CountAsync(c =>
                    c.CompanyId == companyId &&
                    c.Enable &&
                    c.Status == CampaignStatus.Active);
                var questionnaires = await _db.Questionnaires.CountAsync(q =>
                    q.CompanyId == companyId && q.Enable);

                return Ok(new
                {
                    employeesCount = usersInCompany.Count,
                    activeUsers,
                    activeCampaigns,
                    questionnairesCount = questionnaires,
                    formateursCount = formateurs,
                });
            }

            if (targetRole == "Admin")
            {
                var users = await _userManager.Users.ToListAsync();
                var activeUsers = users.Count(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.UtcNow);
                var managementAccounts = 0;

                foreach (var user in users)
                {
                    var role = await GetPrimaryRoleAsync(user);
                    if (role == "Admin" || role == "CompanyAdmin")
                        managementAccounts++;
                }

                var companies = await _db.Companies.CountAsync(c => c.Enable);

                return Ok(new
                {
                    totalUsers = users.Count,
                    activeUsers,
                    companiesCount = companies,
                    managementAccounts,
                });
            }

            return Ok(new { });
        }

        private string GetCallerRole()
        {
            if (User.IsInRole("Admin")) return "Admin";
            if (User.IsInRole("CompanyAdmin")) return "CompanyAdmin";
            if (User.IsInRole("Formateur")) return "Formateur";
            return string.Empty;
        }

        private async Task<string> GetPrimaryRoleAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? string.Empty;
        }

        private static bool CanAssignRole(string callerRole, string requestedRole)
        {
            if (callerRole == "Admin")
                return AdminAllowedRoles.Contains(requestedRole);

            return LimitedAllowedRoles.Contains(requestedRole);
        }

        private static bool CanManageTarget(
            string callerRole,
            int? callerCompanyId,
            int? targetCompanyId,
            string targetRole,
            bool allowAdminEverywhere = false)
        {
            if (callerRole == "Admin")
                return allowAdminEverywhere || AdminAllowedRoles.Contains(targetRole);

            if (!callerCompanyId.HasValue || callerCompanyId != targetCompanyId)
                return false;

            if (callerRole == "CompanyAdmin")
                return targetRole is "CompanyAdmin" or "Formateur" or "Condidat";

            if (callerRole == "Formateur")
                return targetRole is "Formateur" or "Condidat";

            return false;
        }

        private async Task SendResetPasswordEmailAsync(AppUser user, string tempPassword, string role)
        {
            var frontendUrl = _config["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            await _emailService.SendWithTemplate(new SendTemplatedEmailRequest
            {
                ToEmail = user.Email ?? string.Empty,
                Subject = "CandidatApp — Your password has been reset",
                TemplatePath = "templates/ResetPasswordEmail.html",
                Variables = new Dictionary<string, string>
                {
                    { "firstname", user.UserName ?? user.Email ?? "there" },
                    { "email", user.Email ?? string.Empty },
                    { "username", user.UserName ?? string.Empty },
                    { "password", tempPassword },
                    { "role", role },
                    { "setupUrl", $"{frontendUrl}/login" },
                    { "supportUrl", $"{frontendUrl}/support" },
                }
            });
        }
    }
}
