using api.data.Entities;

namespace api.data.Repositories.AiModelRepo
{
    public interface IAiModelRepository : IBaseRepository<AiModel>
    {
        Task<AiModel?> GetDefault();
        Task UnsetAllDefaults();
    }
}
