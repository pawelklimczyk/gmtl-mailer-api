using Gmtl.MailerAPI.WebAPI.Events;
using Gmtl.MailerAPI.WebAPI.Persistance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gmtl.MailerAPI.WebAPI.Services
{
    public class IncomingEmailHandlerService : AbstractHostedService
    {
        private readonly ILogger<IncomingEmailHandlerService> _logger;
        private readonly IncomingMailQueue _queue;

        public IncomingEmailHandlerService(IncomingMailQueue queue, IServiceScopeFactory serviceScopeFactory,
             ILogger<IncomingEmailHandlerService> logger) : base(serviceScopeFactory)
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
                        await Process(mail, _stoppingCts.Token);
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

        protected async Task Process(IncomingMailEvent item, CancellationToken stoppingCtsToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetService<MailerDbContext>();

                    dbContext.Add(item.Message);
                    await dbContext.SaveChangesAsync();

                    if (item.Message.MailStatus == Domain.MailStatus.Draft)
                    {
                        _logger.LogInformation($"Mail {item.Message} saved as draft");
                        return;
                    }

                    if (item.Message.ScheduledDelivery < DateTime.Now)
                    {
                        var sendingQueue = scope.ServiceProvider.GetService<MailForSendingQueue>();
                        sendingQueue.Enqueue(MailForSendingEvent.Create(item.Message));
                    }

                    _logger.LogInformation($"Mail {item.Message} scheduled for sending at {item.Message.ScheduledDelivery}");
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
