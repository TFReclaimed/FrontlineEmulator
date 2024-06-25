using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Frontline.Features.Guilds.KickLeaveGuild;

public class Endpoint : Endpoint<KickLeaveGuildRequest, Ok>
{
    private readonly IGuildRepository _guildRepository;

    public Endpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public override void Configure()
    {
        Delete("/guildapi/guilds/{GuildId}/members/{UserId}");
    }

    public override async Task HandleAsync(KickLeaveGuildRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var member = await _guildRepository.GetPlayerMembershipAsync(userId);
        if (member is null || member.GuildId != req.GuildId)
        {
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        if (userId == req.UserId)
        {
            var guild = await _guildRepository.GetGuildAsync(req.GuildId, true);
            if (guild is null)
            {
                await SendResultAsync(TypedResults.BadRequest());
                return;
            }
            
            if (member.Rank == MemberRank.LEADER &&
                !guild.Members.Any(m => m.Rank == MemberRank.LEADER && m.UserId != userId))
            {
                await _guildRepository.DeleteGuildAsync(guild);
            }
            else
            {
                await _guildRepository.DeletePlayerMembershipAsync(member);
            }
        }
        else
        {
            var target = await _guildRepository.GetPlayerMembershipAsync(req.UserId);
            if (target is null || target.GuildId != req.GuildId)
            {
                await SendResultAsync(TypedResults.BadRequest());
                return;
            }

            if (member.Rank < target.Rank)
            {
                await SendResultAsync(TypedResults.BadRequest());
                return;
            }

            await _guildRepository.DeletePlayerMembershipAsync(target);
        }
        
        await SendResultAsync(TypedResults.Ok());
    }
}