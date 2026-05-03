using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.data.Repositories.ThemeRepo
{
    public class ThemeRepository : BaseRepository<Theme>, IThemeRepository
    {
        private readonly ApplicationContext _context;

        public ThemeRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IList<Theme>> GetRootThemes(int companyId)
        {
            return await _context.Themes
                .Where(t => t.ParentId == null && t.CompanyId == companyId && t.Enable)
                .Include(t => t.Children.Where(c => c.Enable))
                .ToListAsync();
        }

        public async Task<IList<Theme>> GetAllRootThemes()
        {
            return await _context.Themes
                .Where(t => t.ParentId == null && t.Enable)
                .Include(t => t.Children.Where(c => c.Enable))
                .OrderBy(t => t.CompanyId)
                .ToListAsync();
        }

        public async Task<IList<Theme>> GetChildren(int parentId)
        {
            return await _context.Themes
                .Where(t => t.ParentId == parentId && t.Enable)
                .ToListAsync();
        }
    }
}
