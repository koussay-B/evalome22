using System.Security.Cryptography;
using api.data.Entities;
using api.data.Repositories;
using api.DTOs.request;
using api.DTOs.response;
using api.Extensions;
using api.Helper;
using api.services.CloudinaryService;
using api.services.EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public class CompanyController(
        IRepositoryWrapper repositoryWrapper,
        UserManager<AppUser> userManager,
        ICloudService cloudService,
        IEmailService emailService,
        IConfiguration config) : BaseApiController
    {
        private readonly IRepositoryWrapper _repositoryWrapper = repositoryWrapper;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ICloudService _cloudService = cloudService;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _config = config;

        private static CompanyDto ToDto(Company c) => new()
        {
            Id              = c.Id,
            Name            = c.Name,
            Logo            = c.Logo,
            Description     = c.Description,
            Address         = c.Address,
            Email           = c.Email,
            Phone           = c.Phone,
            Website         = c.Website,
            RequesterName   = c.RequesterName,
            RequesterEmail  = c.RequesterEmail,
            Status          = c.Status.ToString(),
            RejectionReason = c.RejectionReason,
            ReviewedAt      = c.ReviewedAt,
            ReviewedByUserId = c.ReviewedByUserId,
            IsActive        = c.IsActive,
            CreatedAt       = c.CreatedAt,
            UpdatedAt       = c.UpdatedAt,
        };

        // GET /api/company
        [HttpGet]
        [Authorize(Roles = "Admin,CompanyAdmin")]
        public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetCompanies(
            [FromQuery] CompanySearchParams searchParams)
        {
            var result = await _repositoryWrapper.Company.GetCompaniesAsync(searchParams);
            AddPaginationHeaders(result.Metadata);
            return Ok(result.Items.Select(ToDto).ToList());
        }

        // GET /api/company/requests
        [HttpGet("requests")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetRequests([FromQuery] string? status = null)
        {
            CompanyStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<CompanyStatus>(status, true, out var requestedStatus))
            {
                parsedStatus = requestedStatus;
            }

            var requests = await _repositoryWrapper.Company.GetRequestsAsync(parsedStatus);
            return Ok(requests.Select(ToDto).ToList());
        }

        // GET /api/company/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,CompanyAdmin")]
        public async Task<IActionResult> GetCompany(int id)
        {
            var company = await _repositoryWrapper.Company.Get(id);
            if (company == null) return NotFound("Company not found");
            return Ok(ToDto(company));
        }

        // POST /api/company
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var company = new Company
            {
                Name           = dto.Name,
                Description    = dto.Description,
                Email          = dto.Email,
                Phone          = dto.Phone,
                Website        = dto.Website,
                Address        = dto.Address,
                Logo           = dto.Logo,
                RequesterName  = dto.RequesterName,
                RequesterEmail = dto.RequesterEmail,
                Status         = CompanyStatus.Approved,
                IsActive       = true,
                CreatedAt      = DateTime.UtcNow,
            };

            await _repositoryWrapper.Company.Create(company);
            return Ok(ToDto(company));
        }

        // POST /api/company/request
        [HttpPost("request")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateCompanyRequest([FromBody] CompanyRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var company = new Company
            {
                Name           = dto.Name.Trim(),
                Description    = dto.Description?.Trim(),
                Phone          = dto.Phone?.Trim(),
                Website        = dto.Website?.Trim(),
                Address        = dto.Address?.Trim(),
                Email          = dto.RequesterEmail.Trim(),
                RequesterName  = dto.RequesterName.Trim(),
                RequesterEmail = dto.RequesterEmail.Trim(),
                Status         = CompanyStatus.Pending,
                IsActive       = false,
            };

            await _repositoryWrapper.Company.Create(company);
            return Ok(ToDto(company));
        }

        // PUT /api/company/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdmin,Admin")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCompany = await _repositoryWrapper.Company.Get(id);
            if (existingCompany == null)
                return NotFound("Company not found");

            if (User.IsInRole("CompanyAdmin"))
            {
                var userId = User.GetUserId();
                var appUser = await _userManager.FindByIdAsync(userId.ToString());
                if (appUser == null || appUser.CompanyId != id)
                    return Forbid("You can only edit your own company.");
            }

            existingCompany.Name           = dto.Name;
            existingCompany.Description    = dto.Description;
            existingCompany.Email          = dto.Email;
            existingCompany.Phone          = dto.Phone;
            existingCompany.Website        = dto.Website;
            existingCompany.Address        = dto.Address;
            existingCompany.Logo           = dto.Logo;
            existingCompany.RequesterName  = dto.RequesterName ?? existingCompany.RequesterName;
            existingCompany.RequesterEmail = dto.RequesterEmail ?? existingCompany.RequesterEmail;
            existingCompany.UpdatedAt      = DateTime.UtcNow;

            await _repositoryWrapper.Company.Update(existingCompany);
            return Ok(ToDto(existingCompany));
        }

        // PATCH /api/company/{id}/approve
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveCompanyRequest(int id)
        {
            var company = await _repositoryWrapper.Company.Get(id);
            if (company == null)
                return NotFound("Company request not found");

            if (company.Status == CompanyStatus.Approved)
                return BadRequest("Company request has already been approved.");

            if (string.IsNullOrWhiteSpace(company.RequesterEmail))
                return BadRequest("Requester email is missing.");

            var existingUser = await _userManager.FindByEmailAsync(company.RequesterEmail);
            if (existingUser != null)
                return BadRequest("An account with this email already exists.");

            var tempPassword = GenerateSecurePassword();
            var appUser = new AppUser
            {
                UserName  = company.RequesterEmail,
                Email     = company.RequesterEmail,
                CompanyId = company.Id,
            };

            var createResult = await _userManager.CreateAsync(appUser, tempPassword);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(appUser, "CompanyAdmin");

            company.Status          = CompanyStatus.Approved;
            company.IsActive        = true;
            company.Email           = company.RequesterEmail;
            company.RejectionReason = null;
            company.ReviewedAt      = DateTime.UtcNow;
            company.ReviewedByUserId = User.GetUserId();
            await _repositoryWrapper.Company.Update(company);

            try
            {
                await SendApprovalEmailAsync(company, tempPassword);
            }
            catch
            {
                // Email delivery is best effort and should not undo approval.
            }

            return Ok(ToDto(company));
        }

        // PATCH /api/company/{id}/reject
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectCompanyRequest(int id, [FromBody] RejectCompanyRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var company = await _repositoryWrapper.Company.Get(id);
            if (company == null)
                return NotFound("Company request not found");

            if (company.Status == CompanyStatus.Approved)
                return BadRequest("Approved companies cannot be rejected.");

            company.Status           = CompanyStatus.Rejected;
            company.IsActive         = false;
            company.RejectionReason  = dto.Reason.Trim();
            company.ReviewedAt       = DateTime.UtcNow;
            company.ReviewedByUserId = User.GetUserId();
            await _repositoryWrapper.Company.Update(company);

            try
            {
                await SendRejectionEmailAsync(company);
            }
            catch
            {
                // Email delivery is best effort and should not undo rejection.
            }

            return Ok(ToDto(company));
        }

        // PATCH /api/company/{id}/logo  — upload a new logo file for this company
        [HttpPatch("{id}/logo")]
        [Authorize(Roles = "Admin,CompanyAdmin")]
        public async Task<IActionResult> UpdateLogo(int id, IFormFile logo)
        {
            if (logo == null || logo.Length == 0)
                return BadRequest("No logo file provided.");

            if (!logo.ContentType.StartsWith("image/"))
                return BadRequest("Only image files are accepted.");

            if (logo.Length > 5 * 1024 * 1024)
                return BadRequest("File size must be under 5 MB.");

            var company = await _repositoryWrapper.Company.Get(id);
            if (company == null)
                return NotFound("Company not found");

            if (User.IsInRole("CompanyAdmin"))
            {
                var userId = User.GetUserId();
                var appUser = await _userManager.FindByIdAsync(userId.ToString());
                if (appUser == null || appUser.CompanyId != id)
                    return Forbid("You can only edit your own company.");
            }

            await using var stream = logo.OpenReadStream();
            var logoUrl = await _cloudService.UploadFileAsync(stream, logo.FileName, "candidatapp/logos");

            company.Logo      = logoUrl;
            company.UpdatedAt = DateTime.UtcNow;
            await _repositoryWrapper.Company.Update(company);

            return Ok(new { logoUrl });
        }

        // GET /api/company/my  — scoped to the logged-in CompanyAdmin's company
        [HttpGet("my")]
        [Authorize(Roles = "CompanyAdmin")]
        public async Task<IActionResult> GetMyCompany()
        {
            var userId = User.GetUserId();
            var appUser = await _userManager.FindByIdAsync(userId.ToString());
            if (appUser == null || !appUser.CompanyId.HasValue)
                return NotFound("No company linked to your account.");

            var company = await _repositoryWrapper.Company.Get(appUser.CompanyId.Value);
            if (company == null) return NotFound("Company not found.");

            return Ok(ToDto(company));
        }

        // DELETE /api/company/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _repositoryWrapper.Company.Get(id);
            if (company == null)
                return NotFound("Company not found");

            await _repositoryWrapper.Company.Delete(id);
            return NoContent();
        }

        private async Task SendApprovalEmailAsync(Company company, string tempPassword)
        {
            if (string.IsNullOrWhiteSpace(company.RequesterEmail))
                return;

            var frontendUrl = _config["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            await _emailService.SendWithTemplate(new SendTemplatedEmailRequest
            {
                ToEmail = company.RequesterEmail,
                Subject = "Your company request has been approved",
                TemplatePath = "templates/CompanyRequestApprovedEmail.html",
                Variables = new Dictionary<string, string>
                {
                    { "requesterName", company.RequesterName ?? company.Name },
                    { "companyName", company.Name },
                    { "email", company.RequesterEmail },
                    { "password", tempPassword },
                    { "loginUrl", $"{frontendUrl}/login" },
                    { "supportUrl", $"{frontendUrl}/support" },
                }
            });
        }

        private async Task SendRejectionEmailAsync(Company company)
        {
            if (string.IsNullOrWhiteSpace(company.RequesterEmail))
                return;

            var frontendUrl = _config["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            await _emailService.SendWithTemplate(new SendTemplatedEmailRequest
            {
                ToEmail = company.RequesterEmail,
                Subject = "Your company request was not approved",
                TemplatePath = "templates/CompanyRequestRejectedEmail.html",
                Variables = new Dictionary<string, string>
                {
                    { "requesterName", company.RequesterName ?? company.Name },
                    { "companyName", company.Name },
                    { "reason", company.RejectionReason ?? string.Empty },
                    { "supportUrl", $"{frontendUrl}/support" },
                }
            });
        }

        private static string GenerateSecurePassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@$?_-";
            var all = upper + lower + digits + special;

            var chars = new[]
            {
                upper[RandomNumberGenerator.GetInt32(upper.Length)],
                lower[RandomNumberGenerator.GetInt32(lower.Length)],
                digits[RandomNumberGenerator.GetInt32(digits.Length)],
                special[RandomNumberGenerator.GetInt32(special.Length)],
                all[RandomNumberGenerator.GetInt32(all.Length)],
                all[RandomNumberGenerator.GetInt32(all.Length)],
                all[RandomNumberGenerator.GetInt32(all.Length)],
                all[RandomNumberGenerator.GetInt32(all.Length)],
                all[RandomNumberGenerator.GetInt32(all.Length)],
                all[RandomNumberGenerator.GetInt32(all.Length)],
            };

            return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }
    }
}
