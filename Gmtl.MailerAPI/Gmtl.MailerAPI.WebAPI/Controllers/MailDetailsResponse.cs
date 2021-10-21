using Gmtl.MailerAPI.WebAPI.Domain;
using System;

namespace Gmtl.MailerAPI.WebAPI.Controllers
{
    public class MailDetailsResponse : AbstractApiResponse
    {
        public MailMessage Mail { get; set; }

        public static MailDetailsResponse Create(MailMessage mail)
        {
            return new MailDetailsResponse
            {
                Mail = mail
            };
        }
    }
}