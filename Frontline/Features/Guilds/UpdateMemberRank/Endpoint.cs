using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Frontline.Features.Guilds.UpdateMemberRank;

public class Endpoint : Endpoint<UpdateMemberRankRequest, Ok>
{
    private readonly IGuildRepository _guildRepository;

    public Endpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public override void Configure()
    {
        Put("/guildapi/guilds/{GuildId}/members/{UserId}");
    }

    public override async Task HandleAsync(UpdateMemberRankRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var member = await _guildRepository.GetPlayerMembershipAsync(userId);
        if (member is null || member.GuildId != req.GuildId)
        {
            Logger.LogWarning("Player {UserId} attempted to update a member rank in guild {GuildId} but is not a member of that guild",
                userId, req.GuildId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var target = await _guildRepository.GetPlayerMembershipAsync(req.UserId);
        if (target is null || target.GuildId != req.GuildId)
        {
            Logger.LogWarning("Player {UserId} attempted to update a member rank in guild {GuildId} but the target member does not exist",
                userId, req.GuildId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }
        
        if (member.Rank < req.Member.Rank)
        {
            Logger.LogWarning("Player {UserId} attempted to update a member rank in guild {GuildId} but does not have permission",
                userId, req.GuildId);
            await Send.ForbiddenAsync();
            return;
        }
        
        Logger.LogInformation("Player {UserId} updated the rank of member {TargetUserId} in guild {GuildId} from {OldRank} to {NewRank}",
            userId, req.UserId, req.GuildId, target.Rank, req.Member.Rank);
        
        target.Rank = req.Member.Rank;
        await _guildRepository.UpdatePlayerMembershipAsync(target);
        
        await Send.OkAsync();
    }
}