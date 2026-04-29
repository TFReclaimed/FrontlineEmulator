namespace Frontline.Battle;

public class LogsCleanupWorker : BackgroundService
{
    private readonly ILogger<LogsCleanupWorker> _logger;
    
    private readonly TimeSpan _tickRate =  TimeSpan.FromMinutes(30);
    
    private readonly TimeSpan _daysToKeepSpan = TimeSpan.FromDays(7);

    public LogsCleanupWorker(ILogger<LogsCleanupWorker> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_tickRate, stoppingToken);
                CleanupOldLogs();
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
    
    private void CleanupOldLogs()
    {
        try
        {
            var staleLogsDateTime = DateTime.Now.Subtract(_daysToKeepSpan);

            var staleLogs = Directory
                .GetFiles(GameLogger.LogFolder, "*.log")
                .ToList()
                .FindAll(file => File.GetCreationTime(file) < staleLogsDateTime);

            if (staleLogs.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Cleaning {Counter} stale logs.", staleLogs.Count);
            
            staleLogs.ForEach(File.Delete);
        }
        catch (Exception ex)
        {
            _logger.LogError("Following error occured while cleaning stale logs: {Message}", ex.Message);
        }
    }
}