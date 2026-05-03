using api.data;
using api.data.Repositories;
using api.services.CloudinaryService;
using api.services.EmailService;
using api.services.NotificationService;
using api.services.TokenService;
using api.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            services.AddScoped<ITokenService, TokenService>();

            // Email — SMTP with template support
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();

            // Cloudinary — image / file cloud storage
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ICloudService, CloudinaryService>();

            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

            services.AddSingleton<PresenceTracker>();

            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
