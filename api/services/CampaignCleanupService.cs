using api.data.Entities;
using api.data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace api.services
{
    public class CampaignCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<CampaignCleanupService> logger,
        IConfiguration configuration) : BackgroundService
    {
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(15);
        private readonly int _draftRetentionDays = Math.Max(
            1,
            configuration.GetValue<int?>("CampaignCleanup:DraftRetentionDays") ?? 1);

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
                    logger.LogError(ex, "Error occurred while running campaign cleanup.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            logger.LogInformation("Campaign Cleanup Service is stopping.");
        }

        private async Task DoCleanup()
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();

            var now = DateTime.UtcNow;

            var activatedCampaignsCount = await repo.Campaign.ActivateScheduledCampaigns(now);

            if (activatedCampaignsCount > 0)
            {
                logger.LogInformation("Auto-activated {Count} scheduled campaigns.", activatedCampaignsCount);
            }

            var closedCampaignsCount = await repo.Campaign.CloseExpiredActiveCampaigns(now);

            if (closedCampaignsCount > 0)
            {
                logger.LogInformation("Auto-closed {Count} expired active campaigns.", closedCampaignsCount);
            }

            var closedScheduledCampaignsCount = await repo.Campaign.CloseExpiredScheduledCampaigns(now);

            if (closedScheduledCampaignsCount > 0)
            {
                logger.LogInformation("Auto-closed {Count} expired scheduled campaigns.", closedScheduledCampaignsCount);
            }

            var cutoffDate = now.AddDays(-_draftRetentionDays);

            var oldDraftCampaigns = await repo.Campaign.GetAllAsQueryable()
                .Where(c => c.Status == CampaignStatus.Draft && c.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldDraftCampaigns.Any())
            {
                logger.LogInformation(
                    "Found {Count} draft campaigns older than {Days} day(s) to delete.",
                    oldDraftCampaigns.Count,
                    _draftRetentionDays);

                foreach (var draft in oldDraftCampaigns)
                {
                    await repo.Campaign.Delete(draft.Id);
                    logger.LogInformation("Auto-deleted draft campaign: {CampaignId} ({CampaignName})", draft.Id, draft.Name);
                }
            }
            else
            {
                logger.LogInformation("No old draft campaigns found for cleanup.");
            }

            var oldDraftQuestionnaires = await repo.Questionnaire.GetAllAsQueryable()
                .Where(q => q.Status == QuestionnaireStatus.Draft
                            && q.CreatedAt < cutoffDate
                            && !q.CampaignQuestionnaires.Any(cq => cq.Enable && cq.Campaign != null && cq.Campaign.Enable))
                .ToListAsync();

            if (oldDraftQuestionnaires.Any())
            {
                logger.LogInformation(
                    "Found {Count} draft questionnaires older than {Days} day(s) to delete.",
                    oldDraftQuestionnaires.Count,
                    _draftRetentionDays);

                foreach (var draft in oldDraftQuestionnaires)
                {
                    await repo.Questionnaire.Delete(draft.Id);
                    logger.LogInformation("Auto-deleted draft questionnaire: {QuestionnaireId} ({QuestionnaireTitle})", draft.Id, draft.Title);
                }
            }
            else
            {
                logger.LogInformation("No old draft questionnaires found for cleanup.");
            }
        }
    }
}
