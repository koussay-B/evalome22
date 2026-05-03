using api.data.Entities;
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

namespace api.data.Repositories
{
    public class RepositoryWrapper(ApplicationContext context, UserManager<AppUser> userManager) : IRepositoryWrapper
    {
        private readonly ApplicationContext _context = context;
        private INotificationRepository? _notification;
        private ICompanyRepository? _company;

        private IThemeRepository? _theme;
        private IQuestionRepository? _question;
        private IQuestionnaireRepository? _questionnaire;
        private ICampaignRepository? _campaign;
        private ICandidateAttemptRepository? _candidateAttempt;
        private IAttemptReportRepository? _attemptReport;
        private IAiModelRepository? _aiModel;

        public UserManager<AppUser> UserManager => userManager;
        public INotificationRepository Notification => _notification ??= new NotificationRepository(_context);
        public ICompanyRepository Company => _company ??= new CompanyRepository(_context);

        public IThemeRepository Theme => _theme ??= new ThemeRepository(_context);
        public IQuestionRepository Question => _question ??= new QuestionRepository(_context);
        public IQuestionnaireRepository Questionnaire => _questionnaire ??= new QuestionnaireRepository(_context);
        public ICampaignRepository Campaign => _campaign ??= new CampaignRepository(_context);
        public ICandidateAttemptRepository CandidateAttempt => _candidateAttempt ??= new CandidateAttemptRepository(_context);
        public IAttemptReportRepository AttemptReport => _attemptReport ??= new AttemptReportRepository(_context);
        public IAiModelRepository AiModel => _aiModel ??= new AiModelRepository(_context);
    }
}