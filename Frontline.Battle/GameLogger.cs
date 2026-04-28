using Microsoft.Extensions.Logging;

namespace Frontline.Battle;

public sealed class GameLogger
{
    private readonly ILogger _logger;

    private readonly string _prefix;

    private readonly string _logFilePath;

    private const string LogFolder = "logs";

    public GameLogger(ILogger logger, Guid gameId)
    {
        _logger = logger;
        _prefix = $"[Game {gameId}] ";
        Directory.CreateDirectory(LogFolder);
        _logFilePath = $"{LogFolder}/game-{gameId}.log";
    }

    public void LogToFile(string level, string message)
    {
        if (!Directory.Exists(LogFolder))
        {
            Directory.CreateDirectory(LogFolder);
        }

        File.AppendAllText(_logFilePath, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}");
    }

    public void Debug(string message)
    {
        _logger.LogDebug("{Prefix}{Message}", _prefix, message);
        LogToFile("DEBUG", _prefix + message);
    }

    public void Debug(string format, params object?[] args)
    {
        _logger.LogDebug(_prefix + format, args);
        var argsString = args.Length == 0 ? string.Empty : $" | Args: {string.Join(", ", args)}";
        LogToFile("DEBUG", _prefix + format + argsString);
    }

    public void Warning(string message)
    {
        _logger.LogWarning("{Prefix}{Message}", _prefix, message);
        LogToFile("WARNING", _prefix + message);
    }

    public void Warning(string format, params object?[] args)
    {
        _logger.LogWarning(_prefix + format, args);
        var argsString = args.Length == 0 ? string.Empty : $" | Args: {string.Join(", ", args)}";
        LogToFile("WARNING", _prefix + format + argsString);
    }
}