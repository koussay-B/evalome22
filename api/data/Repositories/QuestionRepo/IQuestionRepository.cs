using System.Collections.Generic;
using System.Threading.Tasks;
using api.data.Entities;

namespace api.data.Repositories.QuestionRepo
{
    public interface IQuestionRepository : IBaseRepository<Question>
    {
        Task<IList<Question>> GetByTheme(int themeId);
        Task<IList<Question>> GetByCompany(int companyId);
    }
}
