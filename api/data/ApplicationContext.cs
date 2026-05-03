using api.data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.data
{
    public class ApplicationContext(DbContextOptions<ApplicationContext> options)
        : IdentityDbContext<AppUser, AppRole, int>(options)
    {


        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Company> Companies { get; set; } = null!;

        public DbSet<Theme> Themes { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<QuestionChoice> QuestionChoices { get; set; } = null!;
        public DbSet<QuestionPrerequisite> QuestionPrerequisites { get; set; } = null!;
        public DbSet<Questionnaire> Questionnaires { get; set; } = null!;
        public DbSet<QuestionnaireQuestion> QuestionnaireQuestions { get; set; } = null!;
        public DbSet<Campaign> Campaigns { get; set; } = null!;
        public DbSet<CampaignQuestionnaire> CampaignQuestionnaires { get; set; } = null!;
        public DbSet<CampaignCandidate> CampaignCandidates { get; set; } = null!;
        public DbSet<CandidateAttempt> CandidateAttempts { get; set; } = null!;
        public DbSet<AttemptAnswer> AttemptAnswers { get; set; } = null!;
        public DbSet<AttemptReport> AttemptReports { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;
        public DbSet<AiModel> AiModels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Enforce "one role per user" at the database level
            builder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasIndex(ur => ur.UserId).IsUnique();
            });

            builder.Entity<Notification>()
               .HasOne(n => n.User)
               .WithMany(e => e.Notifications)
               .HasForeignKey(n => n.UserId);

            builder.Entity<AppUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Theme self-referencing
            builder.Entity<Theme>()
              .HasOne(t => t.Parent)
              .WithMany(t => t.Children)
              .HasForeignKey(t => t.ParentId)
              .OnDelete(DeleteBehavior.Restrict);

            // AttemptReport 1-to-1
            builder.Entity<AttemptReport>()
              .HasOne(r => r.Attempt)
              .WithOne(a => a.Report)
              .HasForeignKey<AttemptReport>(r => r.AttemptId);

            // QuestionnaireQuestion composite key
            builder.Entity<QuestionnaireQuestion>()
              .HasIndex(q => new { q.QuestionnaireId, q.QuestionId })
              .IsUnique();

            // CampaignQuestionnaire join table
            builder.Entity<CampaignQuestionnaire>()
              .HasIndex(cq => new { cq.CampaignId, cq.QuestionnaireId })
              .IsUnique();

            builder.Entity<CampaignQuestionnaire>()
              .HasOne(cq => cq.Campaign)
              .WithMany(c => c.CampaignQuestionnaires)
              .HasForeignKey(cq => cq.CampaignId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignQuestionnaire>()
              .HasOne(cq => cq.Questionnaire)
              .WithMany(q => q.CampaignQuestionnaires)
              .HasForeignKey(cq => cq.QuestionnaireId)
              .OnDelete(DeleteBehavior.Restrict);

            // Certificate: unique per attempt
            builder.Entity<Certificate>()
              .HasIndex(c => c.AttemptId)
              .IsUnique();

            builder.Entity<Certificate>()
              .HasIndex(c => c.CertificateCode)
              .IsUnique();

            builder.Entity<Certificate>()
              .HasOne(c => c.Candidate)
              .WithMany()
              .HasForeignKey(c => c.CandidateId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Certificate>()
              .HasOne(c => c.Campaign)
              .WithMany()
              .HasForeignKey(c => c.CampaignId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Certificate>()
              .HasOne(c => c.Attempt)
              .WithMany()
              .HasForeignKey(c => c.AttemptId)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}