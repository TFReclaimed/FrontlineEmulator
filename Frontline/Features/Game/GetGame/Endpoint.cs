using FastEndpoints;
using Frontline.Extensions;
using Frontline.Features.Session.Inventory.GetInventory;
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
                    /*new Player
                    {
                        Deck = new Deck
                        {
                            Cards = []
                        },
                        SupportDeck = new SupportDeck
                        {
                            Cards =
                            [
                                new InventoryCard
                                {
                                    Type = "UnitCard",
                                    InstanceId = 4,
                                    TemplateId = 296
                                },
                                new InventoryCard
                                {
                                    Type = "UnitCard",
                                    InstanceId = 5,
                                    TemplateId = 296
                                }
                            ],
                            Count = 2,
                            CurrentSupport = 1
                        },
                        Hand = new CardCollection
                        {
                            Cards = []
                        },
                        Discard = new CardCollection
                        {
                            Cards = []
                        },
                        Resources = new GameResources
                        {
                            CommandAccum = 0,
                            CommandUnits = 0,
                            Health = 20,
                            MaxHealth = 20,
                            DrawDamage = 1
                        },
                        Commander = new CardStack
                        {
                            PrimaryCard = new InventoryCard
                            {
                                Type = "CommanderCard",
                                InstanceId = 1,
                                TemplateId = 282
                            }
                        },
                        Name = "Firs player",
                        UserId = game.Player1Id
                    },
                    new Player
                    {
                        Deck = new Deck
                        {
                            Cards = []
                        },
                        SupportDeck = new SupportDeck
                        {
                            Cards =
                            [
                                new InventoryCard
                                {
                                    Type = "UnitCard",
                                    InstanceId = 3,
                                    TemplateId = 296
                                },
                                new InventoryCard
                                {
                                    Type = "UnitCard",
                                    InstanceId = 6,
                                    TemplateId = 296
                                }
                            ],
                            Count = 2,
                            CurrentSupport = 1
                        },
                        Hand = new CardCollection
                        {
                            Cards = []
                        },
                        Discard = new CardCollection
                        {
                            Cards = []
                        },
                        Resources = new GameResources
                        {
                            CommandAccum = 0,
                            CommandUnits = 0,
                            Health = 20,
                            MaxHealth = 20,
                            DrawDamage = 1
                        },
                        Commander = new CardStack
                        {
                            PrimaryCard = new InventoryCard
                            {
                                Type = "CommanderCard",
                                InstanceId = 2,
                                TemplateId = 283
                            }
                        },
                        Name = "Second player",
                        UserId = game.Player2Id
                    }*/
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