using System.Linq;
using System.Threading.Tasks;
using api.data.Entities;
using api.data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController(IRepositoryWrapper repo) : ControllerBase
    {
        private readonly IRepositoryWrapper _repo = repo;

        [HttpGet("attempt/{attemptId}")]
        [Authorize(Roles = "Condidat,Formateur,CompanyAdmin")]
        public async Task<IActionResult> GetByAttempt(int attemptId)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var report = await _repo.AttemptReport.GetByAttempt(attemptId);
            if (report == null) return NotFound();

            var attempt = await _repo.CandidateAttempt.Get(attemptId);
            if (attempt == null) return NotFound();

            var isCandidate = await _repo.UserManager.IsInRoleAsync(user, "Condidat");
            if (isCandidate && attempt.CandidateId != user.Id)
            {
                return Forbid();
            }

            if (!isCandidate)
            {
                var campaign = await _repo.Campaign.Get(attempt.CampaignId);
                if (campaign == null || campaign.CompanyId != user.CompanyId)
                {
                    return Forbid();
                }
            }

            return Ok(report);
        }

        [HttpGet("campaign/{campaignId}")]
        [Authorize(Roles = "Formateur,CompanyAdmin")]
        public async Task<IActionResult> GetByCampaign(int campaignId)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _repo.Campaign.GetWithCandidates(campaignId);
            if (campaign == null) return NotFound();
            if (campaign.CompanyId != user.CompanyId) return Forbid();
            if (await _repo.UserManager.IsInRoleAsync(user, "Formateur") && campaign.CreatedById != user.Id)
            {
                return Forbid();
            }

            // Simple aggregation logic for the endpoint 
            // Better to handle in repo
            var attempts = _repo.CandidateAttempt.GetAllAsQueryable()
                .Where(a => a.CampaignId == campaignId && a.Status == AttemptStatus.Submitted && a.Enable)
                .Include(a => a.Candidate)
                .Include(a => a.Questionnaire)
                .ToList();

            if (!attempts.Any())
            {
                return Ok(new { Attempts = attempts, GroupAverageScore = 0, PassRate = 0 });
            }

            var groupAverageScore = attempts.Average(a => a.Percentage);
            var passRate = (attempts.Count(a => a.Passed) / (decimal)attempts.Count) * 100;

            return Ok(new
            {
                Attempts = attempts.Select(a => new
                {
                    a.Id,
                    a.CandidateId,
                    candidateName = a.Candidate != null ? a.Candidate.UserName : "Unknown",
                    candidateEmail = a.Candidate != null ? a.Candidate.Email : null,
                    a.QuestionnaireId,
                    questionnaireTitle = a.Questionnaire != null ? a.Questionnaire.Title : null,
                    a.AttemptNumber,
                    a.Percentage,
                    a.Passed,
                    status = a.Status.ToString(),
                    a.StartedAt,
                    a.SubmittedAt,
                }).ToList(),
                GroupAverageScore = groupAverageScore,
                PassRate = passRate
            });
        }

        [HttpGet("candidate/{candidateId}")]
        [Authorize(Roles = "Formateur,CompanyAdmin")]
        public async Task<IActionResult> GetByCandidate(int candidateId)
        {
            var user = await _repo.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Ideally fetch user then attempts across all campaigns within COMPANY context
            var attempts = _repo.CandidateAttempt.GetAllAsQueryable()
                .Where(a => a.CandidateId == candidateId && a.Enable)
                .ToList();

            var companyCampaignIds = _repo.Campaign.GetAllAsQueryable()
                .Where(c => c.CompanyId == user.CompanyId && c.Enable)
                .Select(c => c.Id)
                .ToList();

            var filteredAttempts = attempts.Where(a => companyCampaignIds.Contains(a.CampaignId)).ToList();

            return Ok(filteredAttempts);
        }
    }
}
