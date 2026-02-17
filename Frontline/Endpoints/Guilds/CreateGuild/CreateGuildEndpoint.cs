using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Frontline.Endpoints.Guilds.CreateGuild;

public class CreateGuildEndpoint : Endpoint<CreateGuildRequest, Ok>
{
    private readonly IGuildRepository _guildRepository;

    private readonly IGuildMemberRepository _guildMemberRepository;

    public CreateGuildEndpoint(IGuildRepository guildRepository, IGuildMemberRepository guildMemberRepository)
    {
        _guildRepository = guildRepository;
        _guildMemberRepository = guildMemberRepository;
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

        Logger.LogInformation("User {UserId} created guild '{GuildName}'", userId, req.Name);

        var newGuild = new GuildEntity
        {
            Name = req.Name,
            Description = req.Description,
            AvatarId = req.AvatarId,
            Mode = req.Mode,
            Locale = req.Locale
        };

        await _guildRepository.AddAsync(newGuild);

        var newMember = new GuildMemberEntity
        {
            UserId = userId,
            GuildId = newGuild.Id,
            Rank = MemberRank.LEADER
        };

        await _guildMemberRepository.AddAsync(newMember);

        await Send.OkAsync();
    }
}