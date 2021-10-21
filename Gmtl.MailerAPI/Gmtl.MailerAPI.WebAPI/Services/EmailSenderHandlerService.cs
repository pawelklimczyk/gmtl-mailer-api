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
    public class EmailSenderHandlerService : AbstractHostedService
    {
        private readonly ILogger<EmailSenderHandlerService> _logger;
        private readonly MailForSendingQueue _queue;

        public EmailSenderHandlerService(MailForSendingQueue queue, IServiceScopeFactory serviceScopeFactory,
             ILogger<EmailSenderHandlerService> logger) : base(serviceScopeFactory)
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
                    while (_queue.Count > 0)
                    {
                        var mail = _queue.Dequeue();

                        try
                        {
                            await Process(mail, _stoppingCts.Token);
                        }
                        catch (Exception exc)
                        {
                            _logger.LogError(exc, exc.Message);
                        }
                    }

                    await Task.Delay(1000);
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

        protected async Task Process(MailForSendingEvent item, CancellationToken stoppingCtsToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    var mailDeliveryService = scope.ServiceProvider.GetService<MailDeliveryService>();
                    var dbContext = scope.ServiceProvider.GetService<MailerDbContext>();

                    bool result = await mailDeliveryService.SendMail(item.Message);

                    var mailFromDb = dbContext.Mails.FirstOrDefault(m => m.Id == item.Message.Id);

                    mailFromDb.MailStatus = result ? Domain.MailStatus.Delivered : Domain.MailStatus.Error;
                    mailFromDb.Processing = false;

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
