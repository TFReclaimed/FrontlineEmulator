using FastEndpoints;
using Frontline.Extensions;
using Frontline.Services;

namespace Frontline.Features.Session.Game.RequestGame;

public class Endpoint : Endpoint<RequestGameRequest, RequestGameResponse>
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
        Post("/session/game");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(RequestGameRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        
        if (_gameService.IsPlayerInGame(userId, out _))
        {
            Logger.LogInformation("User {UserId} is already in a game.", userId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        Logger.LogInformation("User {UserId} requested a game of {GameType} against {OpponentId}.",
            userId, req.Param.GameType, req.Param.OpponentId);

        var game = _gameService.GetEmptyGame(req.Param.GameType);
        if (game is null)
        {
            game = _gameService.CreateGame(userId, req.Param.GameType);
        }
        else
        {
            game.Player2Id = userId;
            _userService.IncrementChangeCounter(game.Player1Id);
            _userService.IncrementChangeCounter(game.Player2Id);
        }

        var response = new RequestGameResponse
        {
            MatchmakingCookie = game.Id.ToString()
        };
        
        await SendAsync(response);
    }
}