using FastEndpoints;
using Frontline.Extensions;
using Frontline.Game;
using Frontline.Services;

namespace Frontline.Features.Game.LogGameEvent;

public class Endpoint : Endpoint<GameEventRequest, GameEventResponse>
{
    private readonly IGameService _gameService;

    public Endpoint(IGameService gameService)
    {
        _gameService = gameService;
    }

    public override void Configure()
    {
        Post("/gameserver/event/{GameId}");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(GameEventRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        
        var game = _gameService.GetGame(req.GameId);
        if (game is null || !game.IsPlayerInGame(userId))
        {
            await SendNotFoundAsync();
            return;
        }

        req.Param.CcgEventsLog = [];

        if (req.Param.GameEvent == GameEvent.DoInitialSwap)
        {
            req.Param.EventResult = new InitialSwapEventResult
            {
                CardIdsRemovedFromHand = [],
                DeckReplacementIndices = []
            };
        }
        else if (req.Param.GameEvent == GameEvent.EndTurn)
        {
            req.Param.EventResult = new DiscardEventResult
            {
                CardIdsRemovedFromHand = (req.Param as GameEventEndTurnParams)!.HandCardIdsToDiscard
            };
        }
        
        Logger.LogInformation("New game event: {GameEvent}", req.Param.GameEvent); // TODO: remove once ready
        
        game.IncreaseChangeCounter(req.Param);
        
        var response = new GameEventResponse
        {
            SequenceNum = game.CurrentEventCount
        };
        
        await SendAsync(response);
    }
}