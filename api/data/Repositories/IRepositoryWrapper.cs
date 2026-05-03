using api.data.Repositories.CompanyRepo;
using api.data.Repositories.NotificationRepo;
using api.data.Repositories.ThemeRepo;
using api.data.Repositories.QuestionRepo;
using api.data.Repositories.QuestionnaireRepo;
using api.data.Repositories.CampaignRepo;
using api.data.Repositories.CandidateAttemptRepo;
using api.data.Repositories.AttemptReportRepo;
using api.data.Repositories.AiModelRepo;
using Microsoft.AspNetCore.Identity;
using api.data.Entities;

namespace api.data.Repositories
{
    public interface IRepositoryWrapper
    {
        UserManager<AppUser> UserManager { get; }
        INotificationRepository Notification { get; }
        ICompanyRepository Company { get; }

        IThemeRepository Theme { get; }
        IQuestionRepository Question { get; }
        IQuestionnaireRepository Questionnaire { get; }
        ICampaignRepository Campaign { get; }
        ICandidateAttemptRepository CandidateAttempt { get; }
        IAttemptReportRepository AttemptReport { get; }
        IAiModelRepository AiModel { get; }
    }
}