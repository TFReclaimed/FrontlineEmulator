namespace Frontline.Battle;

public class BattleCleanupWorker : BackgroundService
{
    private readonly IBattleService _battleService;

    private readonly TimeSpan _tickRate = TimeSpan.FromSeconds(10);

    public BattleCleanupWorker(IBattleService battleService)
    {
        _battleService = battleService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_tickRate, stoppingToken);
                _battleService.CleanupStaleBattles();
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}