using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.services.EmailService
{
    /// <summary>
    /// Configuration settings for SMTP email sending.
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// The SMTP server hostname (e.g., smtp.gmail.com).
        /// </summary>
        public required string SmtpServer { get; set; }

        /// <summary>
        /// The SMTP server port (e.g., 587 for TLS).
        /// </summary>
        public required int SmtpPort { get; set; }

        /// <summary>
        /// The display name of the sender.
        /// </summary>
        public required string SenderName { get; set; }

        /// <summary>
        /// The sender's email address.
        /// </summary>
        public required string SenderEmail { get; set; }

        /// <summary>
        /// The SMTP username for authentication.
        /// </summary>
        public string Username { get; set; } =string.Empty;

        /// <summary>
        /// The SMTP password or app-specific password for authentication.
        /// </summary>
        public string Password { get; set; } =string.Empty;

        /// <summary>
        /// Indicates whether SSL/TLS should be enabled for the SMTP connection.
        /// </summary>
        public bool EnableSsl { get; set; }
    }
}