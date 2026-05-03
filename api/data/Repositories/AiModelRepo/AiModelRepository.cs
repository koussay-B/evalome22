using api.data.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.data.Repositories.AiModelRepo
{
    public class AiModelRepository(ApplicationContext context) : BaseRepository<AiModel>(context), IAiModelRepository
    {
        public async Task<AiModel?> GetDefault()
        {
            return await _dbContext.AiModels
                .Where(m => m.Enable && m.IsDefault && m.IsEnabled)
                .FirstOrDefaultAsync();
        }

        public async Task UnsetAllDefaults()
        {
            var defaults = await _dbContext.AiModels
                .Where(m => m.Enable && m.IsDefault)
                .ToListAsync();

            foreach (var m in defaults)
                m.IsDefault = false;

            await _dbContext.SaveChangesAsync();
        }
    }
}
