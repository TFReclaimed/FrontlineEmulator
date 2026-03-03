using Frontline.Data.Repositories;

namespace Frontline.Xmpp;

public class ChatHistoryTrimWorker : BackgroundService
{
    private readonly ILogger<ChatHistoryTrimWorker> _logger;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public ChatHistoryTrimWorker(ILogger<ChatHistoryTrimWorker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);

                using var scope = _serviceScopeFactory.CreateScope();
                var chatMessageRepository = scope.ServiceProvider.GetRequiredService<IChatMessageRepository>();

                var deleted = await chatMessageRepository.TrimHistoryAsync(Globals.MaxMessages);
                if (deleted > 0)
                {
                    _logger.LogInformation("Deleted {Count} old chat messages.", deleted);
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trimming chat history.");
            }
        }
    }
}