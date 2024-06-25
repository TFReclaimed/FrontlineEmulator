using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Frontline.Features.Guilds.UpdateGuild;

public class Endpoint : Endpoint<UpdateGuildRequest, Ok>
{
    private readonly IGuildRepository _guildRepository;

    public Endpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public override void Configure()
    {
        Put("/guildapi/guilds/{GuildId}");
    }

    public override async Task HandleAsync(UpdateGuildRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var member = await _guildRepository.GetPlayerMembershipAsync(userId);
        if (member is null || member.Rank != MemberRank.LEADER || member.GuildId != req.GuildId)
        {
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var guild = await _guildRepository.GetGuildAsync(req.GuildId);
        if (guild is null)
        {
            await SendResultAsync(TypedResults.NotFound());
            return;
        }
        
        guild.Description = req.Description;
        guild.Mode = req.Mode;
        guild.AvatarId = req.AvatarId;
        guild.Locale = req.Locale;
        
        await _guildRepository.UpdateGuildAsync(guild);
        
        await SendResultAsync(TypedResults.Ok());
    }
}