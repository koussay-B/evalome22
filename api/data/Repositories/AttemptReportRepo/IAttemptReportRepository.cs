using System.Threading.Tasks;
using api.data.Entities;

namespace api.data.Repositories.AttemptReportRepo
{
    public interface IAttemptReportRepository : IBaseRepository<AttemptReport>
    {
        Task<AttemptReport?> GetByAttempt(int attemptId);
    }
}
