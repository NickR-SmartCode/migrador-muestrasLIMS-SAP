using INTERFACES.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace SERVICES
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, Task>> _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>();

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            _queue.Writer.TryWrite(workItem);
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken ct)
        {
            return await _queue.Reader.ReadAsync(ct);
        }
    }
}
