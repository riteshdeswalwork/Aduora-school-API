using System.Threading.Channels;
using sms.api.Interfaces.Shared;

namespace sms.api.Services;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundTaskQueue()
    {
        _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void Enqueue(Func<CancellationToken, Task> workItem)
    {
        if (workItem == null) throw new ArgumentNullException(nameof(workItem));
        if (!_queue.Writer.TryWrite(workItem))
        {
            throw new InvalidOperationException("Unable to queue background work item.");
        }
    }

    public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken ct)
    {
        var workItem = await _queue.Reader.ReadAsync(ct);
        return workItem;
    }
}

