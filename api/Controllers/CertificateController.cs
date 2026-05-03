using api.data;
using api.data.Entities;
using api.data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificateController(ApplicationContext db, IRepositoryWrapper repo) : ControllerBase
    {
        private readonly ApplicationContext _db = db;
        private readonly IRepositoryWrapper _repo = repo;

        public class CertificateDto
        {
            public int      Id                { get; set; }
            public int      CampaignId        { get; set; }
            public string   CampaignName      { get; set; } = string.Empty;
            public string   CompanyName       { get; set; } = string.Empty;
            public decimal  Percentage        { get; set; }
            public DateTime IssuedAt          { get; set; }
            public string   CertificateCode   { get; set; } = string.Empty;
            public string   CandidateName     { get; set; } = string.Empty;
        }

        // GET /api/certificate/my
        [HttpGet("my")]
        [Authorize(Roles = "Condidat")]
        public async Task<IActionResult> GetMyCertificates()
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var certs = await _db.Certificates
                .Where(c => c.CandidateId == user.Id && c.Enable)
                .Include(c => c.Campaign)
                .OrderByDescending(c => c.IssuedAt)
                .Select(c => new CertificateDto
                {
                    Id              = c.Id,
                    CampaignId      = c.CampaignId,
                    CampaignName    = c.Campaign!.Name,
                    CompanyName     = string.Empty,
                    Percentage      = c.Percentage,
                    IssuedAt        = c.IssuedAt,
                    CertificateCode = c.CertificateCode,
                    CandidateName   = user.UserName ?? string.Empty,
                })
                .ToListAsync();

            return Ok(certs);
        }

        // GET /api/certificate/verify/{code}
        [HttpGet("verify/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify(string code)
        {
            var cert = await _db.Certificates
                .Where(c => c.CertificateCode == code && c.Enable)
                .Include(c => c.Campaign)
                    .ThenInclude(c => c!.Company)
                .Include(c => c.Candidate)
                .FirstOrDefaultAsync();

            if (cert == null) return NotFound("Certificate not found.");

            return Ok(new CertificateDto
            {
                Id              = cert.Id,
                CampaignId      = cert.CampaignId,
                CampaignName    = cert.Campaign?.Name ?? string.Empty,
                CompanyName     = cert.Campaign?.Company?.Name ?? string.Empty,
                Percentage      = cert.Percentage,
                IssuedAt        = cert.IssuedAt,
                CertificateCode = cert.CertificateCode,
                CandidateName   = cert.Candidate?.UserName ?? string.Empty,
            });
        }
    }
}
