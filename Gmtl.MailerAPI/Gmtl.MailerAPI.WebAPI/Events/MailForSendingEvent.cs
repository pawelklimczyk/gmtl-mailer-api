using Gmtl.MailerAPI.WebAPI.Domain;

namespace Gmtl.MailerAPI.WebAPI.Events
{
    public class MailForSendingEvent
    {
        public MailMessage Message { get; private set; }

        public static MailForSendingEvent Create(MailMessage message)
        {
            return new MailForSendingEvent
            {
                Message = message
            };
        }
    }
}
