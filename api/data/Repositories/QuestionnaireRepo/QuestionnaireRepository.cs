using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.data.Repositories.QuestionnaireRepo
{
    public class QuestionnaireRepository : BaseRepository<Questionnaire>, IQuestionnaireRepository
    {
        private readonly ApplicationContext _context;

        public QuestionnaireRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Questionnaire?> GetWithQuestions(int id)
        {
            return await _context.Questionnaires
                .Include(q => q.QuestionnaireQuestions)
                    .ThenInclude(qq => qq.Question)
                        .ThenInclude(q => q.Choices)
                .Include(q => q.CampaignQuestionnaires.Where(cq => cq.Enable))
                .FirstOrDefaultAsync(q => q.Id == id && q.Enable);
        }

        public async Task<IList<Questionnaire>> GetByCompany(int companyId)
        {
            return await _context.Questionnaires
                .Include(q => q.QuestionnaireQuestions.Where(qq => qq.Enable))
                .Where(q => q.CompanyId == companyId && q.Enable)
                .ToListAsync();
        }
    }
}
