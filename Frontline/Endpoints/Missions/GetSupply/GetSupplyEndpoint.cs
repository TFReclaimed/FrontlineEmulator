using FastEndpoints;
using Frontline.Extensions;
using Frontline.Services;

namespace Frontline.Endpoints.Missions.GetSupply;

public class GetSupplyEndpoint : EndpointWithoutRequest<GetSupplyResponse>
{
    private readonly ISupplyService _supplyService;

    public GetSupplyEndpoint(ISupplyService supplyService)
    {
        _supplyService = supplyService;
    }

    public override void Configure()
    {
        Post("/Missions/v1/supply");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = this.GetUserId();
        var lastSupplySync = await _supplyService.UpdateSupplyAsync(userId);

        var response = new GetSupplyResponse
        {
            LastSupplySync = lastSupplySync.ToString("yyyy-MM-dd HH:mm:ss.fff")
        };

        await Send.OkAsync(response);
    }
}