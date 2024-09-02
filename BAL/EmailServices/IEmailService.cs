using BAL.EmailServices.EmailContents;

namespace BAL.EmailServices
{
    public interface IEmailService
    {
        Task SendEmail<TEmailContent>(TEmailContent emailContent, string email) where TEmailContent : IEmailContent;
    }
}
