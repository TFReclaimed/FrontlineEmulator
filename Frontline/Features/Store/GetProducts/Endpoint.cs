using FastEndpoints;

namespace Frontline.Features.Store.GetProducts;

public class Endpoint : EndpointWithoutRequest<List<Product>>
{
    public override void Configure()
    {
        Get("/Store/v1/product");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var products = new List<Product>
        {
            new()
            {
                ProductID = "BOOSTER_1X",
                Title = "1 Booster Pack",
                RealPrice = 1.99f,
                RealCurrencyCode = "USD",
                SoftVirtualPrice = -1,
                HardVirtualPrice = 25,
                IsAvailableReal = false,
                IsConsumable = true,
                SkuApple = "REL_BOOSTER_1X_REAL",
                SkuGoogle = "rel_booster_1x_real"
            },
            new()
            {
                ProductID = "BOOSTER_5X",
                Title = "5 Booster Packs",
                RealPrice = 9.99f,
                RealCurrencyCode = "USD",
                SoftVirtualPrice = -1,
                HardVirtualPrice = 125,
                IsAvailableReal = false,
                IsConsumable = true,
                SkuApple = "REL_BOOSTER_5X_REAL",
                SkuGoogle = "rel_booster_5x_real"
            },
            new()
            {
                ProductID = "BOOSTER_10X",
                Title = "10 Booster Packs",
                RealPrice = 19.99f,
                RealCurrencyCode = "USD",
                SoftVirtualPrice = -1,
                HardVirtualPrice = 250,
                IsAvailableReal = false,
                IsConsumable = true,
                SkuApple = "REL_BOOSTER_10X_REAL",
                SkuGoogle = "rel_booster_10x_real"
            },
            new()
            {
                ProductID = "BOOSTER_20X",
                Title = "20 Booster Packs",
                RealPrice = 39.99f,
                RealCurrencyCode = "USD",
                SoftVirtualPrice = -1,
                HardVirtualPrice = 500,
                IsAvailableReal = false,
                IsConsumable = true,
                SkuApple = "REL_BOOSTER_20X_REAL",
                SkuGoogle = "rel_booster_20x_real"
            },
            new()
            {
                ProductID = "BOOSTER_60X",
                Title = "60 Booster Packs",
                RealPrice = 99.99f,
                RealCurrencyCode = "USD",
                SoftVirtualPrice = -1,
                HardVirtualPrice = 1250,
                IsAvailableReal = false,
                IsConsumable = true,
                SkuApple = "REL_BOOSTER_60X_REAL",
                SkuGoogle = "rel_booster_60x_real"
            }
        };

        await SendAsync(products);
    }
}