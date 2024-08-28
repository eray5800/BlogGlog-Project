using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using BAL.EmailContents;

namespace BAL
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmail<TEmailContent>(TEmailContent emailContent, string email) where TEmailContent : IEmailContent
        {
            string msg = string.Empty;
            try
            {
                var message = new MailMessage();
                var smtp = new SmtpClient();

                // Load email settings from configuration
                var emailSettings = _configuration.GetSection("EmailSettings");

                message.From = new MailAddress(emailSettings["From"], emailSettings["DisplayName"]);
                message.To.Add(email);
                message.Subject = emailContent.Subject;
                message.IsBodyHtml = true;
                message.Body = emailContent.Body;

                smtp.Port = int.Parse(emailSettings["SmtpPort"]);
                smtp.Host = emailSettings["SmtpHost"];
                smtp.EnableSsl = bool.Parse(emailSettings["EnableSsl"]);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                await smtp.SendMailAsync(message);

                msg = $"Successfully sent email to {email}.";
                _logger.LogWarning(msg);
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
                _logger.LogError(msg);
            }
        }
    }
}
