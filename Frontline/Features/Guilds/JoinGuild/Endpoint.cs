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
            Logger.LogWarning("User {UserId} is already a member of a guild", userId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var guild = await _guildRepository.GetGuildAsync(req.GuildId);
        if (guild is null)
        {
            Logger.LogWarning("Player {UserId} tried to join non-existing guild {GuildId}", userId, req.GuildId);
            await SendNotFoundAsync();
            return;
        }
        
        if (guild.Members.Count >= guild.MaxNumberOfMembers)
        {
            Logger.LogWarning("Player {UserId} tried to join guild {GuildId} which is full", userId, req.GuildId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        Logger.LogInformation("Player {UserId} joined guild '{GuildName}' ({GuildId})", userId, guild.Name, req.GuildId);
        
        await _guildRepository.JoinGuildAsync(userId, req.GuildId);
        
        await SendOkAsync();
    }
}