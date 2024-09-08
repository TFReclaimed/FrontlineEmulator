using FastEndpoints;
using Frontline.Extensions;
using Frontline.Services;

namespace Frontline.Features.Session.Info;

public class Endpoint : EndpointWithoutRequest<SessionInfoResponse>
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
        Get("/session/info");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = this.GetUserId();

        const string notInGame = "0";
        const string gameRequestPending = "-1";

        var gameId = notInGame;
        if (_gameService.IsPlayerInGame(userId, out var game))
        {
            gameId = game.IsFull ? game.Id.ToString() : gameRequestPending;
        }
        
        var response = new SessionInfoResponse
        {
            CurrentGameInstance = gameId,
            UserChangeCounter = _userService.GetChangeCounter(userId)
        };
        
        await SendAsync(response);
    }
}