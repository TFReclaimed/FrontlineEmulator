using System.Text.Json.Serialization;

namespace Frontline.Features.Store.GetProducts;

public class Product
{
    [JsonPropertyName("ProductID")]
    public string ProductID { get; set; } = string.Empty;
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("RealPrice")]
    public float RealPrice { get; set; }
    [JsonPropertyName("RealCurrencyCode")]
    public string RealCurrencyCode { get; set; } = string.Empty;
    [JsonPropertyName("SoftVirtualPrice")]
    public int SoftVirtualPrice { get; set; }
    [JsonPropertyName("HardVirtualPrice")]
    public int HardVirtualPrice { get; set; }
    [JsonPropertyName("IsAvailableReal")]
    public bool IsAvailableReal { get; set; }
    [JsonPropertyName("IsConsumable")]
    public bool IsConsumable { get; set; }
    [JsonPropertyName("SKU_Apple")]
    public string SkuApple { get; set; } = string.Empty;
    [JsonPropertyName("SKU_Google")]
    public string SkuGoogle { get; set; } = string.Empty;
}