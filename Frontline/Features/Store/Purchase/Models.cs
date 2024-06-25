using Microsoft.AspNetCore.Mvc;

namespace Frontline.Features.Store.Purchase;

public class PurchaseRequest
{
    [FromQuery(Name = "Player")]
    public int PlayerId { get; set; }
    [FromQuery(Name = "Method")]
    public string PaymentMethod { get; set; }
    [FromQuery(Name = "Product")]
    public string Product { get; set; }
}

public class PurchaseResponse
{
    public bool Fulfillment { get; set; }
}