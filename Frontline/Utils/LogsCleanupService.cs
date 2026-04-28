using Frontline.Battle;

namespace Frontline.Utils;

public interface ILogsCleanupService
{
    void CleanupOldLogs();
}

public class LogsCleanupService: ILogsCleanupService
{
    private readonly ILogger<LogsCleanupService> _logger;
    
    private readonly TimeSpan _daysToKeepSpan = TimeSpan.FromDays(7);
    
    public LogsCleanupService(ILogger<LogsCleanupService> logger)
    {
        _logger = logger;
    }
    
    public void CleanupOldLogs()
    {
        try
        {
            DateTime staleLogsDateTime = DateTime.Now.Subtract(_daysToKeepSpan);

            var staleLogs = Directory
                .GetFiles(GameLogger.LogFolder, "*.log")
                .ToList()
                .FindAll(file => File.GetCreationTime(file) < staleLogsDateTime);

            _logger.LogInformation("Cleaning {Counter} stale logs.", staleLogs.Count);

            staleLogs.ForEach(File.Delete);
        }
        catch (Exception ex)
        {
            _logger.LogError("Following error occured while cleaning stale logs: {Message}", ex.Message);
        }
        
    }
}