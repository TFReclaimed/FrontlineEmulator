using FastEndpoints;
using Frontline.Extensions;
using Frontline.Features.Session.Rulesets;
using Frontline.Game;
using Frontline.Options;
using Frontline.Services;
using Microsoft.Extensions.Options;

namespace Frontline.Features.Game.GetGame;

public class Endpoint : Endpoint<GetGameRequest, GetGameResponse>
{
    private readonly IGameService _gameService;
    
    private readonly IOptions<UrlOptions> _urlOptions;

    public Endpoint(IGameService gameService, IOptions<UrlOptions> urlOptions)
    {
        _gameService = gameService;
        _urlOptions = urlOptions;
    }

    public override void Configure()
    {
        Get("/gameserver/{GameId}");
    }

    public override async Task HandleAsync(GetGameRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        
        var game = _gameService.GetGame(req.GameId);
        if (game is null || !game.IsPlayerInGame(userId))
        {
            await SendNotFoundAsync();
            return;
        }

        var response = new GetGameResponse
        {
            GameState = new GameState
            {
                GameInstanceId = game.Id,
                Players =
                [
                    game.Player1!,
                    game.Player2!
                ],
                Board = new GameBoard
                {
                    Regions =
                    [
                        new GameRegion
                        {
                            Slots =
                            [
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                }
                            ],
                            RegionLocation = RegionEnum.Player0
                        },
                        new GameRegion
                        {
                            Slots =
                            [
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                }
                            ],
                            RegionLocation = RegionEnum.Player1
                        },
                        new GameRegion
                        {
                            Slots =
                            [
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                },
                                new CardStack
                                {
                                    PrimaryCard = null
                                }
                            ],
                            RegionLocation = RegionEnum.Control
                        }
                    ]
                },
                GameTemplateId = 1,
                PlayerTurn = -2,
                LocalPlayer = game.Player1Id == userId
                    ? (sbyte) 0
                    : (sbyte) 1,
                WinningPlayer = -1,
                SurrenderGameOver = game.SurrenderGameOver,
                NextSummonInstanceId = -1,
                GameType = game.VersusType,
                Rewards =
                [
                    new Rewards(),
                    new Rewards()
                ]
            },
            RulesetPath = new RulesetPathResponse
            {
                Uri = _urlOptions.Value.RulesetsUrl,
                Version = 0
            },
            GameChangeCounter = game.GameChangeCounter,
            CurrentEventCount = game.CurrentEventCount
        };
        
        await SendAsync(response);
    }
}