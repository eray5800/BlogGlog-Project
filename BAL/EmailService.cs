using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

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

        public async Task SendEmail(string token, string email, string operation, string newEmail = null)
        {
            string msg = string.Empty;
            try
            {
                var message = new MailMessage();
                var smtp = new SmtpClient();

                // Load email settings from configuration
                var emailSettings = _configuration.GetSection("EmailSettings");

                string link;
                string subject = string.Empty;
                string body = string.Empty;

                if (operation == "emailConfirm")
                {
                    link = $"https://localhost:7181/api/Auth/verifyemail?token={Uri.EscapeDataString(token)}&email={email}";
                    subject = "Confirm your email";
                    body = $"Please confirm your account by clicking this link: <a href='{link}'>link</a>";
                }
                else if (operation == "emailChange")
                {
                    link = $"https://localhost:7181/api/Account/ChangeEmail?token={Uri.EscapeDataString(token)}&oldEmail={email}&newEmail={newEmail}";
                    subject = "Change your email";
                    body = $"Please click this link if you want to change your email. If you did not request this change, please update your password: <a href='{link}'>link</a>";
                }

                message.From = new MailAddress(emailSettings["From"], emailSettings["DisplayName"]);
                message.To.Add(email);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;

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
