using System.Threading.Tasks;
using api.data.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.data.Repositories.AttemptReportRepo
{
    public class AttemptReportRepository : BaseRepository<AttemptReport>, IAttemptReportRepository
    {
        private readonly ApplicationContext _context;

        public AttemptReportRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<AttemptReport?> GetByAttempt(int attemptId)
        {
            return await _context.AttemptReports
                .FirstOrDefaultAsync(r => r.AttemptId == attemptId && r.Enable);
        }
    }
}
