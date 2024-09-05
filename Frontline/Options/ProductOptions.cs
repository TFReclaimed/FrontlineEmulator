using System.ComponentModel.DataAnnotations;

namespace Frontline.Options;

[OptionsSection("ProductSettings")]
public class ProductOptions
{
    [Required]
    public List<Product> Products { get; set; } = [];
}

public class Product
{
    public string ProductId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public float RealPrice { get; set; }
    public string RealCurrencyCode { get; set; } = string.Empty;
    public int SoftVirtualPrice { get; set; }
    public int HardVirtualPrice { get; set; }
    public bool IsAvailableReal { get; set; }
    public bool IsConsumable { get; set; }
    public string SkuApple { get; set; } = string.Empty;
    public string SkuGoogle { get; set; } = string.Empty;
    public int BoosterCount { get; set; }
}