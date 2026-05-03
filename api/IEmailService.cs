namespace api.services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    Task SendResultEmailAsync(string toEmail, string candidateName, double score, bool passed);
}