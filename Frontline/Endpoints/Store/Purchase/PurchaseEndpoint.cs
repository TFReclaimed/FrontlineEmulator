using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Endpoints.Store.Purchase;

public class PurchaseEndpoint : Endpoint<PurchaseRequest, PurchaseResponse>
{
    private readonly IPlayerRepository _playerRepository;
    
    private readonly IOptions<ProductOptions> _productOptions;

    public PurchaseEndpoint(IPlayerRepository playerRepository, IOptions<ProductOptions> productOptions)
    {
        _playerRepository = playerRepository;
        _productOptions = productOptions;
    }

    public override void Configure()
    {
        Post("/Store/v1/purchase");
        AllowFormData(true);
        AllowAnonymous(); // Yes, the game really doesn't pass in any auth headers for this endpoint
    }

    public override async Task HandleAsync(PurchaseRequest req, CancellationToken ct)
    {
        // TODO: Figure out if we can make this endpoint secure despite the lack of auth headers
        var player = await _playerRepository.GetByIdAsync(req.PlayerId);
        if (player == null)
        {
            Logger.LogWarning("Player {PlayerId} does not exist.", req.PlayerId);
            
            await Send.NotFoundAsync();
            return;
        }
        
        var product = _productOptions.Value.Products.FirstOrDefault(p => p.ProductId == req.Product);
        if (product is null)
        {
            Logger.LogWarning("Player {PlayerId} attempted to purchase unknown product {Product}.",
                req.PlayerId, req.Product);
            
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }
        
        if (player.Tokens < product.HardVirtualPrice)
        {
            Logger.LogWarning("Player {PlayerId} attempted to purchase product {Product} but doesn't have enough tokens!",
                req.PlayerId, req.Product);
            
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }
        
        Logger.LogInformation("Player {PlayerId} purchased product {Product} with payment method {PaymentMethod}.",
            req.PlayerId, req.Product, req.PaymentMethod);
        
        player.Tokens -= product.HardVirtualPrice;
        player.BoosterPackCount += product.BoosterCount;
        await _playerRepository.UpdateAsync(player);

        var response = new PurchaseResponse
        {
            Fulfillment = true
        };
        
        await Send.OkAsync(response);
    }
}