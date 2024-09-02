using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.EmailServices.EmailContents
{
    public interface IEmailContent
    {
        string Subject { get; }
        string Body { get; }
    }
}
