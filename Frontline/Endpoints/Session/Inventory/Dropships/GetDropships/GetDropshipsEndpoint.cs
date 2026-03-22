using FastEndpoints;
using Frontline.Battle.Data;
using Frontline.Battle.Data.Card;
using Frontline.Data.Repositories;
using Frontline.Extensions;

namespace Frontline.Endpoints.Session.Inventory.Dropships.GetDropships;

public class GetDropshipsEndpoint : Endpoint<GetInventoryRequest, List<DropshipInfo>>
{
    private readonly IPlayerRepository _playerRepository;

    private readonly IDropshipRepository _dropshipRepository;

    public GetDropshipsEndpoint(IPlayerRepository playerRepository, IDropshipRepository dropshipRepository)
    {
        _playerRepository = playerRepository;
        _dropshipRepository = dropshipRepository;
    }

    public override void Configure()
    {
        Get("/session/dropships");
    }

    public override async Task HandleAsync(GetInventoryRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player not found: {UserId}", userId);
            await Send.NotFoundAsync();
            return;
        }

        var dropshipItems = await _dropshipRepository.GetDropshipItems(userId);

        var response = new List<DropshipInfo>();

        foreach (var dropship in dropshipItems.GroupBy(x => x.DropshipId).ToList())
        {
            var slottedCards = new CardDto?[41];

            var dropshipEntities = dropship.ToList();

            foreach (var dropshipEntity in dropshipEntities)
            {
                var item = dropshipEntity.Item!;

                var cardTemplate = RulesetParser.GetCardTemplate(item.TemplateId);
                var isCommander = cardTemplate!.Type == CardType.Commander;

                slottedCards[dropshipEntity.SlotIndex] = new CardDto
                {
                    Type = isCommander ? "CommanderCard" : "Card",
                    InstanceId = item.ItemId,
                    TemplateId = item.TemplateId,
                    Rank = item.Rank,
                    Xp = item.Xp
                };
            }

            for (var i = 0; i < 41; i++)
            {
                if (slottedCards[i] != null)
                {
                    continue;
                }

                slottedCards[i] = new CardDto
                {
                    Type = "Card",
                    InstanceId = 0,
                    TemplateId = 0
                };
            }

            response.Add(new DropshipInfo
            {
                Index = dropship.Key,
                SlottedCards = slottedCards!,
                InstanceId = dropship.Key
            });
        }

        await Send.OkAsync(response);
    }
}