namespace sms.api.Interfaces.Shared;

public interface IBackgroundTaskQueue
{
    void Enqueue(Func<CancellationToken, Task> workItem);
    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken ct);
}

