using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Missions;

namespace Frontline.Endpoints.Missions.CancelMission;

public class CancelMissionEndpoint : Endpoint<CancelMissionRequest, List<MissionStageStatus>>
{
    private readonly IActiveMissionRepository _activeMissionRepository;

    public CancelMissionEndpoint(IActiveMissionRepository activeMissionRepository)
    {
        _activeMissionRepository = activeMissionRepository;
    }

    public override void Configure()
    {
        Post("/Missions/v1/cancelmission");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(CancelMissionRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();

        var key = MissionsParser.GetMissionKey(req.Key.Region, req.Key.Faction, req.Key.MissionId);

        var missionData = MissionsParser.GetMission(key);
        if (missionData is null)
        {
            Logger.LogWarning("Player {UserId} attempted to cancel mission {Key} but mission doesn't exist.",
                userId, key);
            await Send.NotFoundAsync();
            return;
        }

        var mission = await _activeMissionRepository.GetActiveMissionAsync(userId, key);
        if (mission is null)
        {
            Logger.LogWarning("Player {UserId} attempted to cancel mission {Key} but mission is not started.",
                userId, key);
            await Send.NotFoundAsync();
            return;
        }

        Logger.LogInformation("Player {UserId} cancelled mission {Key}.", userId, key);

        await _activeMissionRepository.DeleteAsync(mission);

        var response = new List<MissionStageStatus>
        {
            new()
            {
                Region = req.Key.Region,
                Faction = req.Key.Faction,
                MissionId = req.Key.MissionId,
                CurrentState = MissionStageState.Available
            }
        };

        await Send.OkAsync(response);
    }
}