using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Repositories;
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
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var member = await _guildRepository.GetPlayerMembershipAsync(userId);
        if (member is null || member.GuildId != req.GuildId)
        {
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var target = await _guildRepository.GetPlayerMembershipAsync(req.UserId);
        if (target is null || target.GuildId != req.GuildId)
        {
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        if (member.Rank < req.Member.Rank)
        {
            await SendResultAsync(TypedResults.Forbid());
            return;
        }
        
        target.Rank = req.Member.Rank;
        await _guildRepository.UpdatePlayerMembershipAsync(target);
        
        await SendResultAsync(TypedResults.Ok());
    }
}