using api.data.Entities;
using api.services;
using Microsoft.EntityFrameworkCore;
using api.data;
public class CampaignReminderWorker(IServiceProvider services, ILogger<CampaignReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now = DateTime.UtcNow;
            // Logic to find campaigns starting soon that haven't sent reminders yet
            var upcoming = await db.Campaigns
                .Where(c => c.Status == CampaignStatus.Active && c.ReminderHoursBefore > 0)
                .ToListAsync();

            foreach(var campaign in upcoming) {
                // Send emails to campaign.CampaignCandidates...
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
