using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
    }
}
