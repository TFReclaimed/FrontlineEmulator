using Microsoft.Extensions.Logging;

namespace Frontline.Battle;

public sealed class GameLogger
{
    private readonly ILogger _logger;

    private readonly string _prefix;

    public GameLogger(ILogger logger, Guid gameId)
    {
        _logger = logger;
        _prefix = $"[Game {gameId}] ";
    }

    public void Debug(string message)
    {
        _logger.LogDebug("{Prefix}{Message}", _prefix, message);
    }

    public void Debug(string format, params object?[] args)
    {
        _logger.LogDebug(_prefix + format, args);
    }

    public void Warning(string message)
    {
        _logger.LogWarning("{Prefix}{Message}", _prefix, message);
    }

    public void Warning(string format, params object?[] args)
    {
        _logger.LogWarning(_prefix + format, args);
    }
}