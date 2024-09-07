using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Game;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Features.Profiles.Login;

public class Endpoint : Endpoint<LoginRequest, PlayerProfile, Mapper>
{
    private readonly IPlayerRepository _playerRepository;
    
    private readonly IInventoryRepository _inventoryRepository;
    
    private readonly IOptions<StarterItemOptions> _starterItemOptions;
    
    private readonly IOptions<JwtOptions> _jwtOptions;

    public Endpoint(IPlayerRepository playerRepository, IInventoryRepository inventoryRepository,
        IOptions<StarterItemOptions> starterItemOptions, IOptions<JwtOptions> jwtOptions)
    {
        _playerRepository = playerRepository;
        _inventoryRepository = inventoryRepository;
        _starterItemOptions = starterItemOptions;
        _jwtOptions = jwtOptions;
    }

    public override void Configure()
    {
        Post("/virtu/accts");
        AllowFormData(urlEncoded: true);
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        Logger.LogInformation("Login request received. Type: {LoginType}, AuthId: {AuthId}, Password {Password}, DeviceId: {DeviceId}, DeviceType: {DeviceType}",
            req.Param.LoginType, req.Param.AuthId, req.Param.Password, req.Param.DeviceId, req.Param.DeviceType);

        // Works fine until I reverse engineer Nexon's API
        const long startingId = 12530000000025341;

        var authId = long.Parse(req.Param.AuthId.Trim('"'));
        var userId = (int) (authId - startingId);
        
        var (player, created) = await _playerRepository.GetOrCreatePlayerAsync(userId);

        if (created)
        {
            await CreateStarterItems(userId);
        }

        var jwtToken = JwtBearer.CreateToken(o =>
        {
            o.SigningKey = _jwtOptions.Value.Key;
            o.ExpireAt = DateTime.UtcNow.AddDays(7);
            o.User["UserId"] = player.Id.ToString();
        });

        var profile = Map.FromEntity(player);
        profile.SessionId = jwtToken;
        
        await SendAsync(profile);
    }

    private async Task CreateStarterItems(int userId)
    {
        var items = new List<ItemEntity>();
        var dropshipItems = new List<DropshipEntity>();

        for (var i = 0; i < _starterItemOptions.Value.Items.Count; i++)
        {
            var starterItem = _starterItemOptions.Value.Items[i];
            var cardTemplate = RulesetParser.GetCardTemplate(starterItem.TemplateId);
            if (cardTemplate is null)
            {
                Logger.LogWarning("Card template not found: {TemplateId}", starterItem.TemplateId);
                continue;
            }

            var item = new ItemEntity
            {
                TemplateId = starterItem.TemplateId,
                Rank = (sbyte) cardTemplate.MinimumRank
            };

            items.Add(item);

            if (starterItem.Dropships is null || starterItem.Dropships.Count == 0)
            {
                continue;
            }
            
            foreach (var dropship in starterItem.Dropships)
            {
                var dropshipItem = new DropshipEntity
                {
                    UserId = userId,
                    DropshipId = dropship.DropshipId,
                    SlotIndex = dropship.SlotIndex,
                    ItemId = i + 1
                };

                dropshipItems.Add(dropshipItem);
            }
        }

        if (items.Count == 0)
        {
            return;
        }
        
        await _inventoryRepository.AddItemsAsync(userId, items);
        await _inventoryRepository.AddDropshipItemsAsync(userId, dropshipItems);
        
        Logger.LogInformation("Gave starter items to user: {UserId}", userId);
    }
}