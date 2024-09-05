using FastEndpoints;
using Frontline.Options;

namespace Frontline.Features.Store.GetProducts;

public class Mapper : ResponseMapper<List<ProductDto>, List<Product>>
{
    public override List<ProductDto> FromEntity(List<Product> e)
    {
        return e.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            Title = p.Title,
            RealPrice = p.RealPrice,
            RealCurrencyCode = p.RealCurrencyCode,
            SoftVirtualPrice = p.SoftVirtualPrice,
            HardVirtualPrice = p.HardVirtualPrice,
            IsAvailableReal = p.IsAvailableReal,
            IsConsumable = p.IsConsumable,
            SkuApple = p.SkuApple,
            SkuGoogle = p.SkuGoogle
        }).ToList();
    }
}