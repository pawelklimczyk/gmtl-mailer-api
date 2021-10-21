using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Gmtl.MailerAPI.WebAPI.Services
{
    public abstract class AbstractHostedService : IHostedService
    {
        protected Task _executingTask;
        protected readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        protected IServiceScopeFactory _serviceScopeFactory;

        public AbstractHostedService(IServiceScopeFactory serviceScopeFactor)
        {
            _serviceScopeFactory = serviceScopeFactor;

        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite,
                    cancellationToken));
            }
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
