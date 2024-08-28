﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.EmailContents
{
    public class EmailChangeContent : IEmailContent
    {
        public string Token { get; }
        public string OldEmail { get; }
        public string NewEmail { get; }

        public EmailChangeContent(string token, string oldEmail, string newEmail)
        {
            Token = token;
            OldEmail = oldEmail;
            NewEmail = newEmail;
        }

        public string Subject => "Change your email";

        public string Body => $"Please click this link if you want to change your email. If you did not request this change, please update your password: <a href='https://localhost:7181/api/Account/ChangeEmail?token={Uri.EscapeDataString(Token)}&oldEmail={OldEmail}&newEmail={NewEmail}'>link</a>";
    }
}
