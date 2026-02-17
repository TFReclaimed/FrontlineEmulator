using FastEndpoints;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Endpoints.Store.GetProducts;

public class GetProductsEndpoint : EndpointWithoutRequest<List<ProductDto>>
{
    private readonly IOptions<ProductOptions> _productOptions;

    public GetProductsEndpoint(IOptions<ProductOptions> productOptions)
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
        var products = _productOptions.Value.Products
            .Select(ProductDto.FromProduct)
            .ToList();

        await Send.OkAsync(products);
    }
}