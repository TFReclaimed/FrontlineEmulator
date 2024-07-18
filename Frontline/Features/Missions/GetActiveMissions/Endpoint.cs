using FastEndpoints;
using Frontline.Missions;

namespace Frontline.Features.Missions.GetActiveMissions;

public class Endpoint : EndpointWithoutRequest<List<MissionStageStatus>>
{
    public override void Configure()
    {
        Get("/Missions/v1/activemissions");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new List<MissionStageStatus>
        {
            new()
            {
                Region = PveRegion.Demeter,
                Faction = Faction.Militia,
                MissionId = 1,
                CurrentState = MissionStageState.Available
            },
            new()
            {
                Region = PveRegion.DeepSpace,
                Faction = Faction.Militia,
                MissionId = 2,
                CurrentState = MissionStageState.Available
            }
        };

        await SendAsync(response, cancellation: ct);
    }
}