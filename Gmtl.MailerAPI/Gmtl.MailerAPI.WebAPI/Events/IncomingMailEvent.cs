using Gmtl.MailerAPI.WebAPI.Domain;

namespace Gmtl.MailerAPI.WebAPI.Events
{
    public class IncomingMailEvent
    {
        public MailMessage Message { get; set; }
    }
}
