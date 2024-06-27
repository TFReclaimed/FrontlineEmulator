using FastEndpoints;

namespace Frontline.Features.Store.Purchase;

public class PurchaseRequest
{
    [BindFrom("Player")]
    public int PlayerId { get; set; }
    [BindFrom("Method")]
    public string PaymentMethod { get; set; }
    public string Product { get; set; }
}

public class PurchaseResponse
{
    public bool Fulfillment { get; set; }
}