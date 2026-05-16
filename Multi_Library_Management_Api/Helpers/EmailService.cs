using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;

namespace Multi_Library_Management_Api.Helpers
{
    public class EmailService : IEmailService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(AppDbContext context, ILogger<EmailService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body, int libraryId, byte[]? attachment = null, string? attachmentName = null)
        {
            try
            {
                var settings = await _context.GeneralSettings
                    .Where(gs => gs.LibraryId == libraryId)
                    .ToListAsync();

                var smtpHost = settings.FirstOrDefault(s => s.Key.Equals("host", StringComparison.OrdinalIgnoreCase))?.Value;
                var smtpPortStr = settings.FirstOrDefault(s => s.Key.Equals("port", StringComparison.OrdinalIgnoreCase))?.Value;
                var smtpUser = settings.FirstOrDefault(s => s.Key.Equals("email", StringComparison.OrdinalIgnoreCase))?.Value;
                var smtpPass = settings.FirstOrDefault(s => s.Key.Equals("password", StringComparison.OrdinalIgnoreCase))?.Value;
                var smtpFrom = settings.FirstOrDefault(s => s.Key.Equals("email", StringComparison.OrdinalIgnoreCase))?.Value;

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogWarning($"SMTP settings not configured for Library ID: {libraryId}. Cannot send email to {to}.");
                    return;
                }

                int smtpPort = int.TryParse(smtpPortStr, out var port) ? port : 587;

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpFrom ?? smtpUser),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(to);

                    if (attachment != null && !string.IsNullOrEmpty(attachmentName))
                    {
                        var stream = new MemoryStream(attachment);
                        mailMessage.Attachments.Add(new Attachment(stream, attachmentName));
                    }

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Email sent successfully to {to} for Library ID: {libraryId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to} for Library ID: {libraryId}. Error: {ex.Message}");
            }
        }
        public async Task SendSystemEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:Host"];
                var smtpPortStr = _configuration["EmailSettings:Port"];
                var smtpUser = _configuration["EmailSettings:Email"];
                var smtpPass = _configuration["EmailSettings:Password"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogWarning($"System SMTP settings not configured. Cannot send email to {to}.");
                    return;
                }

                int smtpPort = int.TryParse(smtpPortStr, out var port) ? port : 587;

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUser),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(to);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"System email sent successfully to {to}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send system email to {to}. Error: {ex.Message}");
            }
        }
    }
}
