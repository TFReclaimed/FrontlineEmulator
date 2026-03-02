namespace Frontline.Battle.Matchmaking;

public class MatchmakingWorker : BackgroundService
{
    private readonly IMatchmakingService _matchmakingService;

    private readonly TimeSpan _tickRate = TimeSpan.FromSeconds(2);

    public MatchmakingWorker(IMatchmakingService matchmakingService)
    {
        _matchmakingService = matchmakingService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_tickRate, stoppingToken);

                _matchmakingService.ProcessQueue();
                _matchmakingService.CleanupStaleTickets();
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}