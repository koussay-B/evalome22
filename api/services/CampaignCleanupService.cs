using api.data.Entities;
using api.data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace api.services
{
    public class CampaignCleanupService(IServiceScopeFactory scopeFactory, ILogger<CampaignCleanupService> logger) : BackgroundService
    {
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Campaign Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoCleanup();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while cleaning up old drafts.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            logger.LogInformation("Campaign Cleanup Service is stopping.");
        }

        private async Task DoCleanup()
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();

            var cutoffDate = DateTime.UtcNow.AddDays(-15);

            // Access campaigns through the repository wrapper
            var oldDrafts = await repo.Campaign.GetAllAsQueryable()
                .Where(c => c.Status == CampaignStatus.Draft && c.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldDrafts.Any())
            {
                logger.LogInformation("Found {Count} old draft campaigns to delete.", oldDrafts.Count);
                foreach (var draft in oldDrafts)
                {
                    await repo.Campaign.Delete(draft.Id);
                    logger.LogInformation("Auto-deleted draft campaign: {CampaignId} ({CampaignName})", draft.Id, draft.Name);
                }
            }
            else
            {
                logger.LogInformation("No old draft campaigns found for cleanup.");
            }
        }
    }
}
