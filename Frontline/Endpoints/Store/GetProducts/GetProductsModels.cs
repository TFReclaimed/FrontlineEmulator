using System.Text.Json.Serialization;
using Frontline.Options;

namespace Frontline.Endpoints.Store.GetProducts;

public class ProductDto
{
    [JsonPropertyName("ProductID")]
    public string ProductId { get; set; } = string.Empty;
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

    public static ProductDto FromProduct(Product product)
    {
        return new ProductDto
        {
            ProductId = product.ProductId,
            Title = product.Title,
            RealPrice = product.RealPrice,
            RealCurrencyCode = product.RealCurrencyCode,
            SoftVirtualPrice = product.SoftVirtualPrice,
            HardVirtualPrice = product.HardVirtualPrice,
            IsAvailableReal = product.IsAvailableReal,
            IsConsumable = product.IsConsumable,
            SkuApple = product.SkuApple,
            SkuGoogle = product.SkuGoogle
        };
    }
}