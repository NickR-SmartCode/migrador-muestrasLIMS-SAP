using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SERVICES
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BackgroundTaskQueue _taskQueue;

        public QueuedHostedService(
            BackgroundTaskQueue taskQueue,
            IServiceScopeFactory scopeFactory)
        {
            _taskQueue = taskQueue;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                await workItem(stoppingToken);
            }
        }
    }
}
