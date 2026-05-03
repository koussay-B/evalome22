using System.Collections.Generic;
using System.Threading.Tasks;
using api.data.Entities;

namespace api.data.Repositories.CampaignRepo
{
    public interface ICampaignRepository : IBaseRepository<Campaign>
    {
        Task<Campaign?> GetWithCandidates(int id);
        Task<Campaign?> GetWithQuestionnaires(int id);
        Task<IList<Campaign>> GetByCompany(int companyId);
        Task<IList<Campaign>> GetActiveCampaigns(int companyId);
        Task<bool> RemoveCandidate(int campaignId, int candidateId);
        Task<CampaignQuestionnaire?> AddQuestionnaire(int campaignId, int questionnaireId, string? label, bool isRequired);
        Task<bool> RemoveQuestionnaire(int campaignId, int questionnaireId);
        Task ReorderQuestionnaires(int campaignId, List<(int QuestionnaireId, int Order)> order);
    }
}
