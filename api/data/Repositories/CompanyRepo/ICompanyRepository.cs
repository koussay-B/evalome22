using api.data.Entities;
using api.Helper;

namespace api.data.Repositories.CompanyRepo
{
    public interface ICompanyRepository : IBaseRepository<Company>
    {
        Task<PaginatedResult<Company>> GetCompaniesAsync(CompanySearchParams searchParams);
        Task<IReadOnlyList<Company>> GetRequestsAsync(CompanyStatus? status = null);
    }
}
