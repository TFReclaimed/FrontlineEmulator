using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Missions;

namespace Frontline.Features.Missions.CancelMission;

public class Endpoint : Endpoint<CancelMissionRequest, List<MissionStageStatus>>
{
    private readonly IMissionRepository _missionRepository;

    public Endpoint(IMissionRepository missionRepository)
    {
        _missionRepository = missionRepository;
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
            await SendNotFoundAsync();
            return;
        }
        
        var mission = await _missionRepository.GetActiveMissionAsync(userId, key);
        if (mission is null)
        {
            Logger.LogWarning("Player {UserId} attempted to cancel mission {Key} but mission is not started.",
                userId, key);
            await SendNotFoundAsync();
            return;
        }
        
        Logger.LogInformation("Player {UserId} cancelled mission {Key}.", userId, key);
        
        await _missionRepository.DeleteActiveMissionAsync(mission);
        
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

        await SendAsync(response);
    }
}