using System.Text.Json.Serialization;
using FastEndpoints;

namespace Frontline.Endpoints.Store.Purchase;

public class PurchaseRequest
{
    [BindFrom("Player")]
    public int PlayerId { get; set; }
    [BindFrom("Method")]
    public StoreCurrencyType PaymentMethod { get; set; }
    public string Product { get; set; } = string.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter<StoreCurrencyType>))]
public enum StoreCurrencyType
{
    SoftVirtual,
    HardVirtual,
    Real
}

public class PurchaseResponse
{
    public bool Fulfillment { get; set; }
}