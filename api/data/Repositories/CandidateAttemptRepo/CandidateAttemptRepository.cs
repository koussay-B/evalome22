using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.data.Repositories.CandidateAttemptRepo
{
    public class CandidateAttemptRepository : BaseRepository<CandidateAttempt>, ICandidateAttemptRepository
    {
        private readonly ApplicationContext _context;

        public CandidateAttemptRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CandidateAttempt?> GetWithAnswers(int id)
        {
            return await _context.CandidateAttempts
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.Question)
                .Include(a => a.Report)
                .FirstOrDefaultAsync(a => a.Id == id && a.Enable);
        }

        public async Task<IList<CandidateAttempt>> GetByCampaignCandidate(int campaignCandidateId)
        {
            return await _context.CandidateAttempts
                .Where(a => a.CampaignCandidateId == campaignCandidateId && a.Enable)
                .ToListAsync();
        }

        public async Task<int> CountAttempts(int campaignCandidateId)
        {
            return await _context.CandidateAttempts
                .CountAsync(a => a.CampaignCandidateId == campaignCandidateId && a.Enable);
        }

        public async Task<int> CountAttemptsByQuestionnaire(int campaignCandidateId, int questionnaireId)
        {
            return await _context.CandidateAttempts
                .CountAsync(a => a.CampaignCandidateId == campaignCandidateId
                               && a.QuestionnaireId == questionnaireId
                               && a.Enable);
        }

        public async Task<IList<CandidateAttempt>> GetByCampaignCandidateAndQuestionnaire(int campaignCandidateId, int questionnaireId)
        {
            return await _context.CandidateAttempts
                .Where(a => a.CampaignCandidateId == campaignCandidateId
                          && a.QuestionnaireId == questionnaireId
                          && a.Enable)
                .ToListAsync();
        }
    }
}
