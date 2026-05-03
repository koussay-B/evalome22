using System.Collections.Generic;
using System.Threading.Tasks;
using api.data.Entities;

namespace api.data.Repositories.QuestionnaireRepo
{
    public interface IQuestionnaireRepository : IBaseRepository<Questionnaire>
    {
        Task<Questionnaire?> GetWithQuestions(int id);
        Task<IList<Questionnaire>> GetByCompany(int companyId);
    }
}
