using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.data.Repositories.CampaignRepo
{
    public class CampaignRepository : BaseRepository<Campaign>, ICampaignRepository
    {
        private readonly ApplicationContext _context;

        public CampaignRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Campaign?> GetWithCandidates(int id)
        {
            return await _context.Campaigns
                .Include(c => c.CampaignCandidates)
                    .ThenInclude(cc => cc.Candidate)
                .FirstOrDefaultAsync(c => c.Id == id && c.Enable);
        }

        public async Task<Campaign?> GetWithQuestionnaires(int id)
        {
            return await _context.Campaigns
                .Include(c => c.CampaignQuestionnaires.Where(cq => cq.Enable).OrderBy(cq => cq.Order))
                    .ThenInclude(cq => cq.Questionnaire)
                .FirstOrDefaultAsync(c => c.Id == id && c.Enable);
        }

        public async Task<IList<Campaign>> GetByCompany(int companyId)
        {
            return await _context.Campaigns
                .Include(c => c.CampaignQuestionnaires.Where(cq => cq.Enable))
                .Where(c => c.CompanyId == companyId && c.Enable)
                .ToListAsync();
        }

        public async Task<IList<Campaign>> GetActiveCampaigns(int companyId)
        {
            var now = DateTime.UtcNow;
            return await _context.Campaigns
                .Where(c => c.CompanyId == companyId
                            && c.Enable
                            && c.Status == CampaignStatus.Active
                            && c.AvailableFrom <= now
                            && c.AvailableUntil >= now)
                .ToListAsync();
        }

        public async Task<bool> RemoveCandidate(int campaignId, int candidateId)
        {
            var cc = await _context.CampaignCandidates
                .FirstOrDefaultAsync(x => x.CampaignId == campaignId && x.CandidateId == candidateId && x.Enable);

            if (cc == null) return false;

            cc.Enable    = false;
            cc.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CampaignQuestionnaire?> AddQuestionnaire(int campaignId, int questionnaireId, string? label, bool isRequired)
        {
            var existing = await _context.CampaignQuestionnaires
                .FirstOrDefaultAsync(cq => cq.CampaignId == campaignId && cq.QuestionnaireId == questionnaireId && cq.Enable);
            if (existing != null) return existing;

            var maxOrder = await _context.CampaignQuestionnaires
                .Where(cq => cq.CampaignId == campaignId && cq.Enable)
                .MaxAsync(cq => (int?)cq.Order) ?? 0;

            var entry = new CampaignQuestionnaire
            {
                CampaignId      = campaignId,
                QuestionnaireId = questionnaireId,
                Order           = maxOrder + 1,
                Label           = label,
                IsRequired      = isRequired,
                CreatedAt       = DateTime.UtcNow,
                Enable          = true,
            };

            _context.CampaignQuestionnaires.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task<bool> RemoveQuestionnaire(int campaignId, int questionnaireId)
        {
            var cq = await _context.CampaignQuestionnaires
                .FirstOrDefaultAsync(x => x.CampaignId == campaignId && x.QuestionnaireId == questionnaireId && x.Enable);

            if (cq == null) return false;

            cq.Enable    = false;
            cq.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ReorderQuestionnaires(int campaignId, List<(int QuestionnaireId, int Order)> order)
        {
            var cqs = await _context.CampaignQuestionnaires
                .Where(cq => cq.CampaignId == campaignId && cq.Enable)
                .ToListAsync();

            foreach (var (qId, newOrder) in order)
            {
                var cq = cqs.FirstOrDefault(c => c.QuestionnaireId == qId);
                if (cq != null)
                {
                    cq.Order     = newOrder;
                    cq.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
