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
        
        var player = game.GetPlayer(userId);
        if (player is null)
        {
            await SendNotFoundAsync();
            return;
        }

        req.Param.CcgEventsLog = [];

        if (req.Param.GameEvent == GameEvent.DoInitialSwap)
        {
            // TODO: don't trust the client. check if we can do this
            game.ClearCcgEventLog();
            
            var mulliganEvent = (GameEventMulliganParams) req.Param;
            foreach (var i in mulliganEvent.HandCardIdsToReplace)
            {
                Logger.LogInformation(i.ToString());
            }

            var initialSwapEventResult = new InitialSwapEventResult
            {
                CardIdsRemovedFromHand = mulliganEvent.HandCardIdsToReplace,
                DeckReplacementIndices = new int[mulliganEvent.HandCardIdsToReplace.Length]
            };
            
            game.DoInitialSwap(player, initialSwapEventResult.CardIdsRemovedFromHand, initialSwapEventResult.DeckReplacementIndices);

            req.Param.EventResult = initialSwapEventResult;
            req.Param.CcgEventsLog = game.GetCcgEventLog();
        }
        else if (req.Param.GameEvent == GameEvent.EndTurn)
        {
            req.Param.EventResult = new DiscardEventResult
            {
                CardIdsRemovedFromHand = (req.Param as GameEventEndTurnParams)!.HandCardIdsToDiscard
            };
        }
        else if (req.Param.GameEvent == GameEvent.Surrender)
        {
            game.Surrender(req.Param.PlayerIndex);
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