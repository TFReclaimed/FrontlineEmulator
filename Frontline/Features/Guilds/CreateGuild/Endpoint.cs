using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Frontline.Features.Guilds.CreateGuild;

public class Endpoint : Endpoint<CreateGuildRequest, Ok>
{
    private readonly IGuildRepository _guildRepository;

    public Endpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public override void Configure()
    {
        Post("/guildapi/guilds");
    }

    public override async Task HandleAsync(CreateGuildRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var guild = await _guildRepository.GetPlayerGuildAsync(userId);
        if (guild is not null)
        {
            Logger.LogWarning("User {UserId} tried to create a guild while already being in one", userId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var newGuild = new GuildEntity
        {
            Name = req.Name,
            Description = req.Description,
            AvatarId = req.AvatarId,
            Mode = req.Mode,
            Locale = req.Locale
        };
        
        var member = new GuildMemberEntity
        {
            UserId = userId,
            GuildId = newGuild.Id,
            Rank = MemberRank.LEADER
        };
        
        Logger.LogInformation("User {UserId} created guild '{GuildName}'", userId, newGuild.Name);
        
        await _guildRepository.CreateGuildAsync(newGuild, member);
        
        await Send.OkAsync();
    }
}