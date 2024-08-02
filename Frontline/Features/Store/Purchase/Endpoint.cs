using FastEndpoints;
using Frontline.Data.Repositories;

namespace Frontline.Features.Store.Purchase;

public class Endpoint : Endpoint<PurchaseRequest, PurchaseResponse>
{
    private readonly IPlayerRepository _playerRepository;

    public Endpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Post("/Store/v1/purchase");
        AllowFormData(urlEncoded: true);
        AllowAnonymous(); // Yes, the game really doesn't pass in any auth headers for this endpoint
    }

    public override async Task HandleAsync(PurchaseRequest req, CancellationToken ct)
    {
        // TODO: Figure out if we can make this endpoint secure despite the lack of auth headers
        var player = await _playerRepository.GetPlayerAsync(req.PlayerId);
        if (player == null)
        {
            await SendNotFoundAsync();
            return;
        }
        
        // TODO: Move all product stuff to a config file
        var priceMap = new Dictionary<string, (int price, int boosterCount)>
        {
            { "BOOSTER_1X", (25, 1) },
            { "BOOSTER_5X", (125, 5) },
            { "BOOSTER_10X", (250, 10) },
            { "BOOSTER_20X", (500, 20) },
            { "BOOSTER_60X", (1250, 60) }
        };
        
        if (!priceMap.TryGetValue(req.Product, out var value))
        {
            Logger.LogWarning("Player {PlayerId} attempted to purchase unknown product {Product}.",
                req.PlayerId, req.Product);
            
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        if (player.Tokens < value.price)
        {
            Logger.LogWarning("Player {PlayerId} attempted to purchase product {Product} but doesn't have enough tokens!",
                req.PlayerId, req.Product);
            
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        Logger.LogInformation("Player {PlayerId} purchased product {Product} with payment method {PaymentMethod}.",
            req.PlayerId, req.Product, req.PaymentMethod);
        
        player.Tokens -= value.price;
        player.BoosterPackCount += value.boosterCount;
        await _playerRepository.UpdatePlayerAsync(player);

        var response = new PurchaseResponse
        {
            Fulfillment = true
        };
        
        await SendAsync(response);
    }
}