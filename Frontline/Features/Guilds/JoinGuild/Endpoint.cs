using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Frontline.Features.Guilds.JoinGuild;

public class Endpoint : Endpoint<JoinGuildRequest, Ok>
{
    private readonly IGuildRepository _guildRepository;

    public Endpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public override void Configure()
    {
        Post("/guildapi/guilds/{GuildId}/members");
    }

    public override async Task HandleAsync(JoinGuildRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var member = await _guildRepository.GetPlayerMembershipAsync(userId);
        if (member is not null)
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
        
        if (guild.Members.Count >= guild.MaxNumberOfMembers)
        {
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        await _guildRepository.JoinGuildAsync(userId, req.GuildId);
        
        await SendResultAsync(TypedResults.Ok());
    }
}