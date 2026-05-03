using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.services.EmailService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
        Task SendWithTemplate(SendTemplatedEmailRequest emailRequest);
    }
}