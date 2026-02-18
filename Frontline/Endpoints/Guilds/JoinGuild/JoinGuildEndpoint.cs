using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Frontline.Endpoints.Guilds.JoinGuild;

public class JoinGuildEndpoint : Endpoint<JoinGuildRequest, Ok>
{
    private readonly IGuildRepository _guildRepository;

    private readonly IGuildMemberRepository _guildMemberRepository;

    public JoinGuildEndpoint(IGuildRepository guildRepository, IGuildMemberRepository guildMemberRepository)
    {
        _guildRepository = guildRepository;
        _guildMemberRepository = guildMemberRepository;
    }

    public override void Configure()
    {
        Post("/guildapi/guilds/{GuildId}/members");
    }

    public override async Task HandleAsync(JoinGuildRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var member = await _guildMemberRepository.GetByIdAsync(userId);
        if (member is not null)
        {
            Logger.LogWarning("User {UserId} is already a member of a guild", userId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var guild = await _guildRepository.GetWithMembersAsync(req.GuildId);
        if (guild is null)
        {
            Logger.LogWarning("Player {UserId} tried to join non-existing guild {GuildId}", userId, req.GuildId);
            await Send.NotFoundAsync();
            return;
        }

        if (guild.Members.Count >= guild.MaxNumberOfMembers)
        {
            Logger.LogWarning("Player {UserId} tried to join guild {GuildId} which is full", userId, req.GuildId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        Logger.LogInformation("Player {UserId} joined guild '{GuildName}' ({GuildId})", userId, guild.Name, req.GuildId);

        var newMember = new GuildMemberEntity
        {
            UserId = userId,
            GuildId = req.GuildId,
            Rank = MemberRank.Member
        };

        await _guildMemberRepository.AddAsync(newMember);

        await Send.OkAsync();
    }
}