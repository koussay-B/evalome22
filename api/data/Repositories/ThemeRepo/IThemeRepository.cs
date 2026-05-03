using System.Collections.Generic;
using System.Threading.Tasks;
using api.data.Entities;

namespace api.data.Repositories.ThemeRepo
{
    public interface IThemeRepository : IBaseRepository<Theme>
    {
        Task<IList<Theme>> GetRootThemes(int companyId);
        Task<IList<Theme>> GetAllRootThemes();
        Task<IList<Theme>> GetChildren(int parentId);
    }
}
