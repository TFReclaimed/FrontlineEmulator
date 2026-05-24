namespace Frontline.Battle;

public class BattleAiWorker : BackgroundService
{
    private readonly IBattleService _battleService;

    private readonly TimeSpan _tickRate = TimeSpan.FromSeconds(2);

    public BattleAiWorker(IBattleService battleService)
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
                _battleService.ProcessAiTurns();
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}