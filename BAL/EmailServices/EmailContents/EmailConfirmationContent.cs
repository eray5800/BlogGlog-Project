using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.EmailServices.EmailContents
{
    public class EmailConfirmationContent : IEmailContent
    {
        public string Token { get; }
        public string Email { get; }

        public EmailConfirmationContent(string token, string email)
        {
            Token = token;
            Email = email;
        }

        public string Subject => "Confirm your email";

        public string Body => $"Please confirm your account by clicking this link: <a href='https://blogprojeapi20240904220317.azurewebsites.net/api/Auth/verifyemail?token={Uri.EscapeDataString(Token)}&email={Email}'>link</a>";
    }
}
