using FastEndpoints;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Features.Store.GetProducts;

public class Endpoint : EndpointWithoutRequest<List<ProductDto>, Mapper>
{
    private readonly IOptions<ProductOptions> _productOptions;

    public Endpoint(IOptions<ProductOptions> productOptions)
    {
        _productOptions = productOptions;
    }

    public override void Configure()
    {
        Get("/Store/v1/product");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var products = Map.FromEntity(_productOptions.Value.Products);
        await Send.OkAsync(products);
    }
}