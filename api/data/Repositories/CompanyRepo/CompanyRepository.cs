using api.data.Entities;
using api.Helper;
using Microsoft.EntityFrameworkCore;

namespace api.data.Repositories.CompanyRepo
{
    public class CompanyRepository(ApplicationContext context)
        : BaseRepository<Company>(context), ICompanyRepository
    {
        public async Task<PaginatedResult<Company>> GetCompaniesAsync(CompanySearchParams p)
        {
            var query = _dbContext.Companies
                .Where(x => x.Enable)
                // .Include(c => c.Users)  ← uncomment to load related users
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(p.Search))
            {
                var term = $"%{p.Search.Trim().ToLower()}%";
                query = query.Where(c =>
                    EF.Functions.ILike(c.Name, term) ||
                    (c.Address != null && EF.Functions.ILike(c.Address, term)) ||
                    (c.Email != null && EF.Functions.ILike(c.Email, term))
                );
            }

            // Filter
            if (p.IsActive.HasValue)
                query = query.Where(c => c.IsActive == p.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(p.Status) &&
                Enum.TryParse<CompanyStatus>(p.Status, true, out var status))
            {
                query = query.Where(c => c.Status == status);
            }

            // Sort — default: most recently touched (updated or created) first
            query = p.OrderBy switch
            {
                "name"      => query.OrderBy(c => c.Name),
                "name_desc" => query.OrderByDescending(c => c.Name),
                _           => query.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            };

            return await PaginationHelper.CreateAsync(query, p.PageNumber, p.PageSize);
        }

        public async Task<IReadOnlyList<Company>> GetRequestsAsync(CompanyStatus? status = null)
        {
            var query = _dbContext.Companies
                .Where(c => c.Enable)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
