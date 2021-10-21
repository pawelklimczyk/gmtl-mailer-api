using Gmtl.MailerAPI.WebAPI.Domain;
using System.Collections.Generic;

namespace Gmtl.MailerAPI.WebAPI.Controllers
{
    public class MailListResponse
    {
        public IEnumerable<MailMessage> Mails { get; private set; }

        public static MailListResponse Create(IEnumerable<MailMessage> mails)
        {
            return new MailListResponse { Mails = mails };
        }
    }
}