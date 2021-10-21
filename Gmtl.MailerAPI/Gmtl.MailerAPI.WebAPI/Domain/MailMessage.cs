using System;

namespace Gmtl.MailerAPI.WebAPI.Domain
{
    public class MailMessage
    {
        public int Id { get; set; }

        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public MailStatus MailStatus { get; set; }
        public DateTime ScheduledDelivery { get; set; }

        public bool Processing { get; set; }

        private MailMessage() { }

        public static MailMessage Create(string from, string to, string subject, string body, DateTime scheduledDeliveryDate, MailStatus delivery)
        {
            return new MailMessage
            {
                From = from,
                To = to,
                Subject = subject,
                Body = body,
                ScheduledDelivery = scheduledDeliveryDate,
                MailStatus = delivery
            };
        }

        public static MailMessage Create(string from, string to, string subject, string body, MailStatus delivery)
        {
            return Create(from, to, subject, body, DateTime.Now, delivery);
        }

        public override string ToString()
        {
            return $"From:{From} To:{To} Subject:{Subject}";
        }
    }

    public enum MailStatus
    {
        Draft = 1,
        Pending = 2,
        Delivered = 3,
        Error = 10
    }

    public class MailMessageFilter
    {
        public static MailMessageFilter Empty => new MailMessageFilter();
    }
}
