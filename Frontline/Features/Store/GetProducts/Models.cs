using System.Text.Json.Serialization;

namespace Frontline.Features.Store.GetProducts;

public class Product
{
    [JsonPropertyName("ProductID")]
    public string ProductID { get; set; }
    [JsonPropertyName("Title")]
    public string Title { get; set; }
    [JsonPropertyName("RealPrice")]
    public float RealPrice { get; set; }
    [JsonPropertyName("RealCurrencyCode")]
    public string RealCurrencyCode { get; set; }
    [JsonPropertyName("SoftVirtualPrice")]
    public int SoftVirtualPrice { get; set; }
    [JsonPropertyName("HardVirtualPrice")]
    public int HardVirtualPrice { get; set; }
    [JsonPropertyName("IsAvailableReal")]
    public bool IsAvailableReal { get; set; }
    [JsonPropertyName("IsConsumable")]
    public bool IsConsumable { get; set; }
    [JsonPropertyName("SKU_Apple")]
    public string SkuApple { get; set; }
    [JsonPropertyName("SKU_Google")]
    public string SkuGoogle { get; set; }
}