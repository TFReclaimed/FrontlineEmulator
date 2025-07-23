using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
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
        var userId = this.GetUserId();
        var member = await _guildRepository.GetPlayerMembershipAsync(userId);
        if (member is null || member.Rank != MemberRank.LEADER || member.GuildId != req.GuildId)
        {
            Logger.LogWarning("User {UserId} is not the leader of guild {GuildId}", userId, req.GuildId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var guild = await _guildRepository.GetGuildAsync(req.GuildId);
        if (guild is null)
        {
            Logger.LogWarning("Player {UserId} tried to update non-existing guild {GuildId}", userId, req.GuildId);
            await Send.NotFoundAsync();
            return;
        }
        
        Logger.LogInformation("Player {UserId} updated guild '{GuildName}' ({GuildId})",
            userId, guild.Name, req.GuildId);
        
        guild.Description = req.Description;
        guild.Mode = req.Mode;
        guild.AvatarId = req.AvatarId;
        guild.Locale = req.Locale;
        
        await _guildRepository.UpdateGuildAsync(guild);
        
        await Send.OkAsync();
    }
}