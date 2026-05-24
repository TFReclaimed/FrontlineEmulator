using System.Text.Json;
using FastEndpoints;
using Frontline.Battle;
using Frontline.Extensions;

namespace Frontline.Endpoints.Game.GetGame;

public class GetGameEndpoint : Endpoint<GetGameRequest, CcgGame>
{
    private readonly IBattleService _battleService;

    private static readonly JsonSerializerOptions JsonOptions;

    public GetGameEndpoint(IBattleService battleService)
    {
        _battleService = battleService;
    }

    static GetGameEndpoint()
    {
        JsonOptions = new JsonSerializerOptions().AddSerializerContextsFromFrontline();
    }

    public override void Configure()
    {
        Get("/gameserver/{GameId}");
    }

    public override async Task HandleAsync(GetGameRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();

        var game = _battleService.GetBattle(req.GameId);
        if (game is null || !game.IsPlayerInGame(userId))
        {
            await Send.NotFoundAsync();
            return;
        }

        var rootNode = JsonSerializer.SerializeToNode(game, JsonOptions);
        rootNode?["GameState"]?["LocalPlayer"] = game.Player1Id == userId ? 0 : 1;
        rootNode?["GameState"]?["GameType"] = "PVP_RANKED";
        var json = rootNode!.ToJsonString();

        await Send.StringAsync(json, 200, "application/json");
    }
}