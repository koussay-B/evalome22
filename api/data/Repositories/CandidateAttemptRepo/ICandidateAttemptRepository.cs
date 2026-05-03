using System.Collections.Generic;
using System.Threading.Tasks;
using api.data.Entities;

namespace api.data.Repositories.CandidateAttemptRepo
{
    public interface ICandidateAttemptRepository : IBaseRepository<CandidateAttempt>
    {
        Task<CandidateAttempt?> GetWithAnswers(int id);
        Task<IList<CandidateAttempt>> GetByCampaignCandidate(int campaignCandidateId);
        Task<int> CountAttempts(int campaignCandidateId);
        Task<int> CountAttemptsByQuestionnaire(int campaignCandidateId, int questionnaireId);
        Task<IList<CandidateAttempt>> GetByCampaignCandidateAndQuestionnaire(int campaignCandidateId, int questionnaireId);
    }
}
