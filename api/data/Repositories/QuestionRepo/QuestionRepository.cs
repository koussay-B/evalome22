using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.data.Repositories.QuestionRepo
{
    public class QuestionRepository : BaseRepository<Question>, IQuestionRepository
    {
        private readonly ApplicationContext _context;

        public QuestionRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IList<Question>> GetByTheme(int themeId)
        {
            return await _context.Questions
                .Where(q => q.ThemeId == themeId && q.Enable)
                .Include(q => q.Choices)
                .Include(q => q.Prerequisites)
                .ToListAsync();
        }

        public async Task<IList<Question>> GetByCompany(int companyId)
        {
            return await _context.Questions
                .Where(q => q.CompanyId == companyId && q.Enable)
                .Include(q => q.Theme)
                .ToListAsync();
        }
    }
}
