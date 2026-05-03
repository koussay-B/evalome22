using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace api.services.EmailService
{
    /// <summary>
    /// Service for sending emails using SMTP, supporting both plain text and templated HTML emails.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <param name="emailSettings">SMTP configuration settings from appsettings.json.</param>
        /// <param name="environment">Provides access to the hosting environment for file system operations.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="emailSettings"/> or <paramref name="environment"/> is null.</exception>
        public EmailService(IOptions<EmailSettings> emailSettings, IWebHostEnvironment environment)
        {
            _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Sends an email asynchronously using the configured SMTP settings.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="body">The body content of the email.</param>
        /// <param name="isHtml">Indicates whether the email body is in HTML format. Defaults to false (plain text).</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="toEmail"/> is null or empty.</exception>
        /// <exception cref="Exception">Thrown if the email fails to send, with the inner exception details.</exception>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            Console.WriteLine($"[EmailService] Tentative d'envoi d'email à : {toEmail}");
            if (string.IsNullOrEmpty(toEmail))
            {
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(toEmail));
            }

            try
            {
                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    client.EnableSsl = _emailSettings.EnableSsl;
                    client.Credentials = new System.Net.NetworkCredential(_emailSettings.Username, _emailSettings.Password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine("[EmailService] Email envoyé avec succès !");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] ERREUR SMTP : {ex.Message}");
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Sends an email using a template file, replacing placeholders with provided variables.
        /// </summary>
        /// <param name="emailRequest">The email request containing recipient, subject, template path, and variables.</param>
        /// <exception cref="ArgumentException">Thrown if <see cref="SendTemplatedEmailRequest.ToEmail"/>, 
        /// <see cref="SendTemplatedEmailRequest.TemplatePath"/>, or <see cref="SendTemplatedEmailRequest.Variables"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the template file specified in <see cref="SendTemplatedEmailRequest.TemplatePath"/> does not exist.</exception>
        /// <exception cref="Exception">Thrown if the email fails to send, with the inner exception details.</exception>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// The template file should contain placeholders in the format {variableName}, which will be replaced
        /// by values from the <see cref="SendTemplatedEmailRequest.Variables"/> dictionary.
        /// </remarks>
        public async Task SendWithTemplate(SendTemplatedEmailRequest emailRequest)
        {
            if (string.IsNullOrEmpty(emailRequest.ToEmail))
            {
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(emailRequest.ToEmail));
            }
            if (string.IsNullOrEmpty(emailRequest.TemplatePath))
            {
                throw new ArgumentException("Template path cannot be null or empty.", nameof(emailRequest.TemplatePath));
            }
            if (emailRequest.Variables == null)
            {
                throw new ArgumentException("Variables dictionary cannot be null.", nameof(emailRequest.Variables));
            }

            // Resolve the full path to the template file
            string fullPath = Path.Combine(_environment.ContentRootPath, emailRequest.TemplatePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Template file not found at {fullPath}");
            }

            // Read the template content
            string templateContent = await File.ReadAllTextAsync(fullPath);

            // Replace placeholders with variables
            string emailBody = ReplacePlaceholders(templateContent, emailRequest.Variables);

            // Send the email as HTML
            await SendEmailAsync(emailRequest.ToEmail, emailRequest.Subject, emailBody, isHtml: true);
        }

        /// <summary>
        /// Replaces placeholders in a template string with values from a dictionary.
        /// </summary>
        /// <param name="template">The template string containing placeholders in the format {variableName}.</param>
        /// <param name="variables">A dictionary of variable names and their corresponding values.</param>
        /// <returns>The template string with all placeholders replaced by their values.</returns>
        /// <remarks>
        /// If a placeholder has no matching key in the dictionary, it remains unchanged in the result.
        /// Null values in the dictionary are replaced with an empty string.
        /// </remarks>
        private string ReplacePlaceholders(string template, Dictionary<string, string> variables)
        {
            string result = template;
            foreach (var variable in variables)
            {
                string placeholder = $"{{{variable.Key}}}"; // e.g., "{Name}"
                result = result.Replace(placeholder, variable.Value ?? string.Empty);
            }
            return result;
        }
    }
}