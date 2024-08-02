using FastEndpoints;

namespace Frontline.Features.Store.Purchase;

public class PurchaseRequest
{
    [BindFrom("Player")]
    public int PlayerId { get; set; }
    [BindFrom("Method")]
    public string PaymentMethod { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
}

public class PurchaseResponse
{
    public bool Fulfillment { get; set; }
}