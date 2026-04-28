using Frontline.Battle;

namespace Frontline.Utils;

public class LogsCleanupWorker : BackgroundService
{
    private readonly ILogsCleanupService _logsCleanupService;
    
    private readonly TimeSpan _tickRate =  TimeSpan.FromDays(1);

    public LogsCleanupWorker(ILogsCleanupService logsCleanupService)
    {
        _logsCleanupService = logsCleanupService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_tickRate, stoppingToken);
                _logsCleanupService.CleanupOldLogs();
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}