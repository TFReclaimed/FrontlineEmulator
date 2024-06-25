using FastEndpoints;

namespace Frontline.Features.Store.Purchase;

public class Endpoint : Endpoint<PurchaseRequest, PurchaseResponse>
{
    public override void Configure()
    {
        Post("/Store/v1/purchase");
        AllowFormData(urlEncoded: true);
        AllowAnonymous(); // Yes, the game really doesn't pass in any auth headers for this endpoint
    }

    public override async Task HandleAsync(PurchaseRequest req, CancellationToken ct)
    {
        Logger.LogInformation("Player {PlayerId} purchased product {Product} with payment method {PaymentMethod}",
            req.PlayerId, req.Product, req.PaymentMethod);

        var response = new PurchaseResponse
        {
            Fulfillment = true
        };
        
        await SendAsync(response);
    }
}