using BAL.EmailServices.EmailContents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace BAL.EmailServices
{
    public class EmailService : IEmailService
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
                // Load Mailjet API settings from configuration
                var apiKey = _configuration["Mailjet:ApiKey"];
                var apiSecret = _configuration["Mailjet:ApiSecret"];
                var fromEmail = _configuration["Mailjet:FromEmail"];
                var displayName = _configuration["Mailjet:DisplayName"];

                var client = new MailjetClient(apiKey, apiSecret);

                var request = new MailjetRequest
                {
                    Resource = Send.Resource
                }
                .Property(Send.FromEmail, fromEmail)
                .Property(Send.FromName, displayName)
                .Property(Send.Subject, emailContent.Subject)
                .Property(Send.HtmlPart, emailContent.Body)
                .Property(Send.Recipients, new JArray {
                    new JObject {
                        {"Email", email}
                    }
                });

                var response = await client.PostAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    msg = $"Successfully sent email to {email}.";
                    _logger.LogWarning(msg);
                }
                else
                {
                    msg = $"Failed to send email to {email}. Status Code: {response.StatusCode}, Error: {response.GetErrorMessage()}";
                    _logger.LogError(msg);
                }
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
                _logger.LogError(msg);
            }
        }
    }
}
