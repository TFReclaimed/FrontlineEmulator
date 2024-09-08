using FastEndpoints;
using Frontline.Extensions;
using Frontline.Services;

namespace Frontline.Features.Session.Game.CancelRequest;

public class Endpoint : Endpoint<CancelGameRequest>
{
    private readonly IGameService _gameService;
    
    private readonly IUserService _userService;

    public Endpoint(IGameService gameService, IUserService userService)
    {
        _gameService = gameService;
        _userService = userService;
    }

    public override void Configure()
    {
        Delete("/session/game");
    }

    public override async Task HandleAsync(CancelGameRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();

        if (!_gameService.IsPlayerInGame(userId, out var game))
        {
            Logger.LogWarning("User {UserId} tried to cancel a game but they're not in one.", userId);
            return;
        }

        if (game.IsFull)
        {
            Logger.LogWarning("User {UserId} tried to cancel a game that's already started.", userId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        Logger.LogInformation("User {UserId} canceled the game request.", userId);

        _gameService.DeleteGame(game.Id);
        _userService.IncrementChangeCounter(game.Player1Id);

        await SendOkAsync();
    }
}