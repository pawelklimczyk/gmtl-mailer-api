using Gmtl.MailerAPI.WebAPI.Domain;
using Gmtl.MailerAPI.WebAPI.Events;
using System;
using System.Collections.Generic;

namespace Gmtl.MailerAPI.WebAPI.Controllers
{
    public class SendMailRequest
    {
        public string From { get; set; }
        public string[] To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime? ScheduledDelivery { get; set; }
        public bool IsDraft { get; set; }

        public IEnumerable<IncomingMailEvent> ToEvents()
        {
            List<IncomingMailEvent> events = new List<IncomingMailEvent>();

            foreach (var recipient in To)
            {
                IncomingMailEvent e = new IncomingMailEvent
                {
                    Message = MailMessage.Create(From, recipient, Subject, Body, ScheduledDelivery.HasValue ? ScheduledDelivery.Value : DateTime.Now, (IsDraft ? MailStatus.Draft : MailStatus.Pending))
                };
                events.Add(e);
            }         

            return events;
        }
    }
}