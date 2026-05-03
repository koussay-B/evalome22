using api.data;
using api.data.Entities;
using api.data.Repositories;
using api.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController(ApplicationContext db, IRepositoryWrapper repo, PresenceTracker presenceTracker) : ControllerBase
    {
        private readonly ApplicationContext _db = db;
        private readonly IRepositoryWrapper _repo = repo;
        private readonly PresenceTracker _tracker = presenceTracker;

        // ── GET /api/stats/admin ─────────────────────────────────────────────
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminStats()
        {
            var totalUsers         = await _db.Users.CountAsync();
            var totalCompanies     = await _db.Companies.CountAsync(c => c.Enable);
            var totalCampaigns     = await _db.Campaigns.CountAsync(c => c.Enable);
            var totalQuestionnaires = await _db.Questionnaires.CountAsync(q => q.Enable);

            var roleDistribution = new Dictionary<string, int>();
            foreach (var r in new[] { "Admin", "CompanyAdmin", "Formateur", "Condidat" })
                roleDistribution[r] = (await _repo.UserManager.GetUsersInRoleAsync(r)).Count;

            var totalAttempts = await _db.CandidateAttempts.CountAsync();
            var statusCounts = await _db.CandidateAttempts
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var attemptStatusCounts = new
            {
                submitted  = statusCounts.FirstOrDefault(x => x.Status == AttemptStatus.Submitted)?.Count ?? 0,
                inProgress = statusCounts.FirstOrDefault(x => x.Status == AttemptStatus.InProgress)?.Count ?? 0,
                timedOut   = statusCounts.FirstOrDefault(x => x.Status == AttemptStatus.TimedOut)?.Count ?? 0,
                abandoned  = statusCounts.FirstOrDefault(x => x.Status == AttemptStatus.Abandoned)?.Count ?? 0,
            };

            var totalSubmitted = attemptStatusCounts.submitted;
            var avgPassRate = totalSubmitted > 0
                ? Math.Round((double)await _db.CandidateAttempts.CountAsync(a => a.Status == AttemptStatus.Submitted && a.Passed) / totalSubmitted * 100, 1)
                : 0.0;
            var avgScore = totalSubmitted > 0
                ? Math.Round(await _db.CandidateAttempts
                    .Where(a => a.Status == AttemptStatus.Submitted && a.MaxPossibleScore > 0)
                    .AverageAsync(a => (double)a.RawScore / (double)a.MaxPossibleScore * 100), 1)
                : 0.0;



            //  Fetch en mémoire (une seule requête SQL simple)
var rawCampaigns = await _db.Campaigns
    .Where(c => c.Enable && c.CompanyId != null)
    .Include(c => c.Company)
    .Select(c => new
    {
        companyName  = c.Company != null ? c.Company.Name : "Unknown",
        passedCount  = c.PassedCount,
        invitedCount = c.InvitedCount,
    })
    .ToListAsync();

//  passRates — groupé en mémoire
var passRates = rawCampaigns
    .Where(c => c.invitedCount > 0)
    .GroupBy(c => c.companyName)
    .Select(g => new
    {
        companyName = g.Key,
        passRate    = Math.Round(
            (double)g.Sum(c => c.passedCount) / g.Sum(c => c.invitedCount) * 100, 1
        )
    })
    .OrderByDescending(x => x.passRate)
    .Take(10)
    .ToList();





            // Members per company broken down by role (Condidat / Formateur / CompanyAdmin)
            var condidatIds      = (await _repo.UserManager.GetUsersInRoleAsync("Condidat")).Select(u => u.Id).ToHashSet();
            var formateurIds     = (await _repo.UserManager.GetUsersInRoleAsync("Formateur")).Select(u => u.Id).ToHashSet();
            var companyAdminIds  = (await _repo.UserManager.GetUsersInRoleAsync("CompanyAdmin")).Select(u => u.Id).ToHashSet();

            var allMemberIds = condidatIds.Concat(formateurIds).Concat(companyAdminIds).ToHashSet();
            var memberUsers = await _db.Users
                .Where(u => allMemberIds.Contains(u.Id) && u.CompanyId != null)
                .Select(u => new { u.Id, CompanyId = u.CompanyId!.Value })
                .ToListAsync();

            var membersByCompany = memberUsers
                .GroupBy(u => u.CompanyId)
                .ToDictionary(g => g.Key, g => new
                {
                    condidat     = g.Count(u => condidatIds.Contains(u.Id)),
                    formateur    = g.Count(u => formateurIds.Contains(u.Id)),
                    companyAdmin = g.Count(u => companyAdminIds.Contains(u.Id)),
                });

            var companies = await _db.Companies
                .Where(c => c.Enable)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            var companyMembersData = companies
                .Select(c =>
                {
                    var counts = membersByCompany.GetValueOrDefault(c.Id);
                    var condidat     = counts?.condidat     ?? 0;
                    var formateur    = counts?.formateur    ?? 0;
                    var companyAdmin = counts?.companyAdmin ?? 0;
                    return new
                    {
                        companyName = c.Name,
                        condidat,
                        formateur,
                        companyAdmin,
                        total = condidat + formateur + companyAdmin,
                    };
                })
                .OrderByDescending(x => x.total)
                .Take(10)
                .ToList();

            var recentActivity = await _db.CandidateAttempts
                .Where(a => a.Status == AttemptStatus.Submitted)
                .OrderByDescending(a => a.SubmittedAt)
                .Take(10)
                .Include(a => a.Candidate)
                .Include(a => a.Campaign)
                .Select(a => new
                {
                    type        = "Attempt",
                    description = (a.Candidate != null ? a.Candidate.UserName : "Unknown")
                                  + " submitted "
                                  + (a.Campaign != null ? a.Campaign.Name : "Unknown"),
                    at = a.SubmittedAt ?? a.StartedAt
                })
                .ToListAsync();

            // Attempt trend last 90 days (filled with zeros)
            var since = DateTime.UtcNow.AddDays(-89).Date; // Last 90 days including today
            var rawTrend = await _db.CandidateAttempts
                .Where(a => a.StartedAt >= since)
                .GroupBy(a => a.StartedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { date = g.Key, count = g.Count() })
                .ToListAsync();

            var attemptTrend = Enumerable.Range(0, 90)
                .Select(offset => since.AddDays(offset))
                .Select(d => new
                {
                    date  = d.ToString("yyyy-MM-dd"),
                    count = rawTrend.FirstOrDefault(r => r.date.Date == d.Date)?.count ?? 0
                })
                .ToList();
            //****************
            var campaignsPerCompany = rawCampaigns
    .GroupBy(c => c.companyName)
    .Select(g => new { companyName = g.Key, count = g.Count() })
    .OrderByDescending(x => x.count)
    .Take(10)
    .ToList();

            // Cumulative member trend (Condidat + Formateur + CompanyAdmin) for last 90 days
            var memberSince = DateTime.UtcNow.AddDays(-89).Date;
            var beforeMembersBaseline = await _db.Users
                .CountAsync(u => allMemberIds.Contains(u.Id) && u.CreatedAt < memberSince);

            var dailyMemberAdds = await _db.Users
                .Where(u => allMemberIds.Contains(u.Id) && u.CreatedAt >= memberSince)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { date = g.Key, count = g.Count() })
                .ToListAsync();

            var runningTotal = beforeMembersBaseline;
            var memberTrend = Enumerable.Range(0, 90)
                .Select(offset => memberSince.AddDays(offset))
                .Select(d =>
                {
                    runningTotal += dailyMemberAdds.FirstOrDefault(r => r.date.Date == d.Date)?.count ?? 0;
                    return new
                    {
                        date  = d.ToString("yyyy-MM-dd"),
                        count = runningTotal,
                    };
                })
                .ToList();

            return Ok(new
            {
                totalUsers,
                totalCompanies,
                totalCampaigns,
                totalQuestionnaires,
                totalAttempts,
                avgPassRate,
                avgScore,
                attemptStatusCounts,
                roleDistribution,
                passRates,
                companyMembers = companyMembersData,
                recentActivity,
                attemptTrend,
                memberTrend,
                campaignsPerCompany,
                onlineUsersCount = (await _tracker.GetOnlineUsers()).Length,
            });
        }

        // ── GET /api/stats/candidat ──────────────────────────────────────────
        [HttpGet("candidat")]
        [Authorize(Roles = "Condidat")]
        public async Task<IActionResult> GetCandidatStats()
        {
            var caller = await _repo.UserManager.GetUserAsync(User);
            if (caller == null) return Unauthorized();
            var uid = caller.Id;

            // All attempts this candidate has made
            var allAttempts = await _db.CandidateAttempts
                .Where(a => a.CandidateId == uid)
                .Include(a => a.Campaign)
                .ToListAsync();
            var submitted = allAttempts.Where(a => a.Status == AttemptStatus.Submitted).ToList();

            // Campaigns invited to
            var totalCampaigns = await _db.CampaignCandidates
                .CountAsync(cc => cc.CandidateId == uid);

            // Certificates earned
            var certificatesEarned = await _db.Certificates
                .CountAsync(c => c.CandidateId == uid);

            var passRate = submitted.Count > 0
                ? Math.Round((double)submitted.Count(a => a.Passed) / submitted.Count * 100, 1)
                : 0.0;
            var avgScore = submitted.Any(a => a.MaxPossibleScore > 0)
                ? Math.Round(submitted.Where(a => a.MaxPossibleScore > 0)
                    .Average(a => (double)a.RawScore / (double)a.MaxPossibleScore * 100), 1)
                : 0.0;

            var since = DateTime.UtcNow.AddDays(-89).Date;
            var rawTrend = allAttempts
                .Where(a => a.StartedAt >= since)
                .GroupBy(a => a.StartedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { date = g.Key, count = g.Count() })
                .ToList();

            var attemptTrend = Enumerable.Range(0, 90)
                .Select(offset => since.AddDays(offset))
                .Select(d => new
                {
                    date  = d.ToString("yyyy-MM-dd"),
                    count = rawTrend.FirstOrDefault(r => r.date.Date == d.Date)?.count ?? 0
                })
                .ToList();

            var scoresByCampaign = submitted
                .OrderByDescending(a => a.SubmittedAt ?? a.StartedAt)
                .Take(10)
                .Select(a => new
                {
                    campaignName = a.Campaign?.Name ?? "Unknown",
                    score        = a.MaxPossibleScore > 0
                                   ? Math.Round((double)a.RawScore / (double)a.MaxPossibleScore * 100, 1)
                                   : 0.0,
                    passed       = a.Passed,
                    submittedAt  = (a.SubmittedAt ?? a.StartedAt).ToString("yyyy-MM-dd"),
                })
                .ToList();

            return Ok(new
            {
                totalCampaigns,
                completedAttempts  = submitted.Count,
                certificatesEarned,
                passRate,
                avgScore,
                attemptTrend,
                scoresByCampaign,
                passFailRatio = new
                {
                    passed = submitted.Count(a => a.Passed),
                    failed = submitted.Count(a => !a.Passed),
                },
            });
        }

        // ── GET /api/stats/formateur ─────────────────────────────────────────
        [HttpGet("formateur")]
        [Authorize(Roles = "Formateur")]
        public async Task<IActionResult> GetFormateurStats()
        {
            var caller = await _repo.UserManager.GetUserAsync(User);
            if (caller == null) return Unauthorized();
            var uid = caller.Id;

            // Campaigns this formateur created
            var myCampaigns = await _db.Campaigns
                .Where(c => c.CreatedById == uid && c.Enable)
                .ToListAsync();
            var activeCamps     = myCampaigns.Count(c => c.Status == CampaignStatus.Active);
            var totalCandidates = myCampaigns.Sum(c => c.InvitedCount);

            // Questionnaires this formateur created
            var totalQuestionnaires = await _db.Questionnaires
                .CountAsync(q => q.CreatedById == uid && q.Enable);

            // Attempts on this company's campaigns (expanded scope for better UX)
            var cid = caller.CompanyId ?? 0;
            
            var statusCounts = await _db.CandidateAttempts
                .Where(a => a.Candidate!.CompanyId == cid)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var attemptStatusCounts = new
            {
                submitted  = statusCounts.FirstOrDefault(x => x.Status == AttemptStatus.Submitted)?.Count ?? 0,
                inProgress = statusCounts.FirstOrDefault(x => x.Status == AttemptStatus.InProgress)?.Count ?? 0,
                timedOut   = statusCounts.FirstOrDefault(x => x.Status == AttemptStatus.TimedOut)?.Count ?? 0,
                abandoned  = statusCounts.FirstOrDefault(x => x.Status == AttemptStatus.Abandoned)?.Count ?? 0,
            };

            var submittedCount = attemptStatusCounts.submitted;
            var passRate = submittedCount > 0
                ? Math.Round((double)await _db.CandidateAttempts.CountAsync(a => a.Candidate!.CompanyId == cid && a.Status == AttemptStatus.Submitted && a.Passed) / submittedCount * 100, 1)
                : 0.0;

            var avgScore = submittedCount > 0
                ? Math.Round(await _db.CandidateAttempts
                    .Where(a => a.Candidate!.CompanyId == cid && a.Status == AttemptStatus.Submitted && a.MaxPossibleScore > 0)
                    .AverageAsync(a => (double)a.RawScore / (double)a.MaxPossibleScore * 100), 1)
                : 0.0;

            var since = DateTime.UtcNow.AddDays(-89).Date;
            var rawTrend = await _db.CandidateAttempts
                .Where(a => a.Candidate!.CompanyId == cid && a.StartedAt >= since)
                .GroupBy(a => a.StartedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { date = g.Key, count = g.Count() })
                .ToListAsync();

            var attemptTrend = Enumerable.Range(0, 90)
                .Select(offset => since.AddDays(offset))
                .Select(d => new
                {
                    date  = d.ToString("yyyy-MM-dd"),
                    count = rawTrend.FirstOrDefault(r => r.date.Date == d.Date)?.count ?? 0
                })
                .ToList();

            var passRateByCampaign = myCampaigns
                .Where(c => c.InvitedCount > 0)
                .OrderByDescending(c => c.InvitedCount)
                .Take(10)
                .Select(c => new
                {
                    campaignName = c.Name,
                    passRate     = c.InvitedCount > 0 ? Math.Round((double)c.PassedCount / c.InvitedCount * 100, 1) : 0.0,
                    invitedCount = c.InvitedCount,
                    status       = c.Status.ToString(),
                })
                .ToList();
            
            // Get online users in this company
            var onlineIds = await _tracker.GetOnlineUsers();
            var companyOnlineCount = 0;
            if (cid > 0)
            {
                // We could optimize this, but for now we filter the online list against company users
                var companyUserIds = await _db.Users
                    .Where(u => u.CompanyId == cid)
                    .Select(u => u.Id)
                    .ToListAsync();
                companyOnlineCount = onlineIds.Count(id => companyUserIds.Contains(id));
            }

            return Ok(new
            {
                totalCampaigns      = myCampaigns.Count,
                activeCampaigns     = activeCamps,
                totalQuestionnaires,
                totalCandidates,
                passRate,
                avgScore,
                attemptTrend,
                attemptStatusCounts,
                passRateByCampaign,
                passFailRatio = new
                {
                    passed = await _db.CandidateAttempts.CountAsync(a => a.Candidate!.CompanyId == cid && a.Status == AttemptStatus.Submitted && a.Passed),
                    failed = await _db.CandidateAttempts.CountAsync(a => a.Candidate!.CompanyId == cid && a.Status == AttemptStatus.Submitted && !a.Passed),
                },
                onlineUsersCount = companyOnlineCount,
            });
        }

        // ── GET /api/stats/company ───────────────────────────────────────────
        [HttpGet("company")]
        [Authorize(Roles = "Admin,CompanyAdmin")]
        public async Task<IActionResult> GetCompanyStats()
        {
            var caller = await _repo.UserManager.GetUserAsync(User);
            if (caller?.CompanyId == null) return Unauthorized();
            var cid = caller.CompanyId.Value;

            var allUsers = await _repo.UserManager.Users
                .Where(u => u.CompanyId == cid)
                .ToListAsync();
            var formateurs = (await _repo.UserManager.GetUsersInRoleAsync("Formateur"))
                .Count(u => u.CompanyId == cid);
            var activeCamps = await _db.Campaigns
                .CountAsync(c => c.CompanyId == cid && c.Enable && c.Status == CampaignStatus.Active);

            var statusLine = await _db.CandidateAttempts
                .Where(a => a.Candidate!.CompanyId == cid)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var attemptStatusCounts = new
            {
                submitted  = statusLine.FirstOrDefault(x => x.Status == AttemptStatus.Submitted)?.Count ?? 0,
                inProgress = statusLine.FirstOrDefault(x => x.Status == AttemptStatus.InProgress)?.Count ?? 0,
                timedOut   = statusLine.FirstOrDefault(x => x.Status == AttemptStatus.TimedOut)?.Count ?? 0,
                abandoned  = statusLine.FirstOrDefault(x => x.Status == AttemptStatus.Abandoned)?.Count ?? 0,
            };

            var submittedCount = attemptStatusCounts.submitted;
            var passRate = submittedCount > 0
                ? Math.Round((double)await _db.CandidateAttempts.CountAsync(a => a.Candidate!.CompanyId == cid && a.Status == AttemptStatus.Submitted && a.Passed) / submittedCount * 100, 1)
                : 0.0;

            var avgScore = submittedCount > 0
                ? Math.Round(await _db.CandidateAttempts
                    .Where(a => a.Candidate!.CompanyId == cid && a.Status == AttemptStatus.Submitted && a.MaxPossibleScore > 0)
                    .AverageAsync(a => (double)a.RawScore / (double)a.MaxPossibleScore * 100), 1)
                : 0.0;

            var since = DateTime.UtcNow.AddDays(-89).Date;
            var rawTrend = await _db.CandidateAttempts
                .Where(a => a.Candidate!.CompanyId == cid && a.StartedAt >= since)
                .GroupBy(a => a.StartedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { date = g.Key, count = g.Count() })
                .ToListAsync();

            var attemptTrend = Enumerable.Range(0, 90)
                .Select(offset => since.AddDays(offset))
                .Select(d => new
                {
                    date  = d.ToString("yyyy-MM-dd"),
                    count = rawTrend.FirstOrDefault(r => r.date.Date == d.Date)?.count ?? 0
                })
                .ToList();

            var passRateByCampaign = await _db.Campaigns
                .Where(c => c.CompanyId == cid && c.Enable && c.InvitedCount > 0)
                .Select(c => new
                {
                    campaignName = c.Name,
                    passRate     = c.InvitedCount > 0 ? (double)c.PassedCount / c.InvitedCount * 100 : 0.0,
                    invitedCount = c.InvitedCount,
                    status       = c.Status.ToString(),
                })
                .Take(10)
                .ToListAsync();

            // Get online users in this company
            var onlineIds = await _tracker.GetOnlineUsers();
            var companyOnlineCount = 0;
            if (cid > 0)
            {
                var companyUserIds = await _db.Users
                    .Where(u => u.CompanyId == cid)
                    .Select(u => u.Id)
                    .ToListAsync();
                companyOnlineCount = onlineIds.Count(id => companyUserIds.Contains(id));
            }

            var roleDistribution = new Dictionary<string, int>();
            foreach (var r in new[] { "CompanyAdmin", "Formateur", "Condidat" })
            {
                var usersInRole = await _repo.UserManager.GetUsersInRoleAsync(r);
                roleDistribution[r] = usersInRole.Count(u => u.CompanyId == cid);
            }

            return Ok(new
            {
                totalMembers    = allUsers.Count,
                totalFormateurs = formateurs,
                activeCampaigns = activeCamps,
                passRate,
                avgScore,
                attemptTrend,
                attemptStatusCounts,
                passRateByCampaign,
                roleDistribution,
                passFailRatio = new
                {
                    passed = await _db.CandidateAttempts.CountAsync(a => a.Candidate!.CompanyId == cid && a.Status == AttemptStatus.Submitted && a.Passed),
                    failed = await _db.CandidateAttempts.CountAsync(a => a.Candidate!.CompanyId == cid && a.Status == AttemptStatus.Submitted && !a.Passed),
                },
                onlineUsersCount = companyOnlineCount,
            });
        }

        // GET /api/stats/company/creator-activity
        [HttpGet("company/creator-activity")]
        [Authorize(Roles = "Admin,CompanyAdmin")]
        public async Task<IActionResult> GetCompanyCreatorActivity([FromQuery] string period = "all")
        {
            var caller = await _repo.UserManager.GetUserAsync(User);
            if (caller?.CompanyId == null) return Unauthorized();
            var cid = caller.CompanyId.Value;

            var creators = new Dictionary<int, (AppUser User, string Role)>();

            var companyAdmins = await _repo.UserManager.GetUsersInRoleAsync("CompanyAdmin");
            foreach (var user in companyAdmins.Where(u => u.CompanyId == cid))
                creators[user.Id] = (user, "CompanyAdmin");

            var formateurs = await _repo.UserManager.GetUsersInRoleAsync("Formateur");
            foreach (var user in formateurs.Where(u => u.CompanyId == cid))
                creators[user.Id] = (user, "Formateur");

            var creatorIds = creators.Keys.ToList();
            var normalizedPeriod = NormalizeCreatorActivityPeriod(period);
            var startsAt = GetCreatorActivityStart(normalizedPeriod);

            var questionsQuery = _db.Questions
                .Where(q => q.CompanyId == cid && q.Enable && creatorIds.Contains(q.CreatedById));
            var campaignsQuery = _db.Campaigns
                .Where(c => c.CompanyId == cid && c.Enable && creatorIds.Contains(c.CreatedById));

            if (startsAt.HasValue)
            {
                questionsQuery = questionsQuery.Where(q => q.CreatedAt >= startsAt.Value);
                campaignsQuery = campaignsQuery.Where(c => c.CreatedAt >= startsAt.Value);
            }

            var questionCounts = await questionsQuery
                .GroupBy(q => q.CreatedById)
                .Select(g => new { userId = g.Key, count = g.Count() })
                .ToDictionaryAsync(x => x.userId, x => x.count);

            var campaignCounts = await campaignsQuery
                .GroupBy(c => c.CreatedById)
                .Select(g => new { userId = g.Key, count = g.Count() })
                .ToDictionaryAsync(x => x.userId, x => x.count);

            var items = creators.Values
                .Select(entry =>
                {
                    var questionsCount = questionCounts.GetValueOrDefault(entry.User.Id);
                    var campaignsCount = campaignCounts.GetValueOrDefault(entry.User.Id);

                    return new
                    {
                        userId = entry.User.Id,
                        userName = entry.User.UserName ?? entry.User.Email ?? "Unknown",
                        role = entry.Role,
                        questionsCount,
                        campaignsCount,
                        totalCount = questionsCount + campaignsCount,
                    };
                })
                .OrderByDescending(x => x.totalCount)
                .ThenBy(x => x.role)
                .ThenBy(x => x.userName)
                .ToList();

            return Ok(new
            {
                period = normalizedPeriod,
                items,
            });
        }

        private static string NormalizeCreatorActivityPeriod(string? period)
        {
            return (period ?? "all").Trim().ToLowerInvariant() switch
            {
                "30d" => "30d",
                "90d" => "90d",
                "12m" => "12m",
                _ => "all",
            };
        }

        private static DateTime? GetCreatorActivityStart(string normalizedPeriod)
        {
            var today = DateTime.UtcNow.Date;

            return normalizedPeriod switch
            {
                "30d" => today.AddDays(-29),
                "90d" => today.AddDays(-89),
                "12m" => today.AddMonths(-12),
                _ => null,
            };
        }
    }
}
