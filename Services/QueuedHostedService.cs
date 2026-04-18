using sms.api.Interfaces.Shared;

namespace sms.api.Services;

public class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(IBackgroundTaskQueue queue, ILogger<QueuedHostedService> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Func<CancellationToken, Task> workItem;
            try
            {
                workItem = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while dequeueing background task");
                continue;
            }

            try
            {
                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing background task");
            }
        }
    }
}

