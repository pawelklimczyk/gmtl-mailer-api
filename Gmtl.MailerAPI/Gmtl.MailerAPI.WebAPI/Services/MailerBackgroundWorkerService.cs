using Gmtl.MailerAPI.WebAPI.Events;
using Gmtl.MailerAPI.WebAPI.Persistance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gmtl.MailerAPI.WebAPI.Services
{
    /// <summary>
    /// Service responsible for scheduling mails for sending
    /// </summary>
    public class MailerBackgroundWorkerService : AbstractHostedService
    {
        private readonly MailForSendingQueue _queue;
        private readonly ILogger<MailerBackgroundWorkerService> _logger;

        public MailerBackgroundWorkerService(MailForSendingQueue queue, IServiceScopeFactory serviceScopeFactory,
             ILogger<MailerBackgroundWorkerService> logger) : base(serviceScopeFactory)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Process(_stoppingCts.Token);
                    await Task.Delay(10000);
                }
            }
            catch (TaskCanceledException)
            {
                //leave that exception. It happends when service is shutdown
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }
        }

        protected async Task Process(CancellationToken stoppingCtsToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetService<MailerDbContext>();
                    var now = DateTime.Now;
                    var mailsToSend = dbContext.Mails.Where(m => m.MailStatus == Domain.MailStatus.Pending && m.ScheduledDelivery <= now && m.Processing == false);

                    foreach (var mail in mailsToSend)
                    {
                        _queue.Enqueue(MailForSendingEvent.Create(mail));
                        mail.Processing = true;
                    }

                    await dbContext.SaveChangesAsync();
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception exc)
                {
                    _logger.LogError(exc, exc.Message);
                }
            }
        }
    }
}
