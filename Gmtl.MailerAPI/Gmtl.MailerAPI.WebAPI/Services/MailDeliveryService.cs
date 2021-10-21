using Gmtl.MailerAPI.WebAPI.Domain;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Gmtl.MailerAPI.WebAPI.Services
{
    /// <summary>
    /// Service responsible for sending email via SMTP server
    /// </summary>
    public class MailDeliveryService
    {
        private readonly ILogger<MailDeliveryService> _logger;

        public MailDeliveryService(ILogger<MailDeliveryService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendMail(MailMessage message)
        {
            //TODO connec to to SMTP and send mail;
            _logger.LogInformation($"Mail '{message}' was send [mocked service!]");

            return true;
        }
    }
}
