using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Frontline.Endpoints.Guilds.KickLeaveGuild;

public class KickLeaveGuildEndpoint : Endpoint<KickLeaveGuildRequest, Ok>
{
    private readonly IGuildRepository _guildRepository;

    private readonly IGuildMemberRepository _guildMemberRepository;

    public KickLeaveGuildEndpoint(IGuildRepository guildRepository, IGuildMemberRepository guildMemberRepository)
    {
        _guildRepository = guildRepository;
        _guildMemberRepository = guildMemberRepository;
    }

    public override void Configure()
    {
        Delete("/guildapi/guilds/{GuildId}/members/{UserId}");
    }

    public override async Task HandleAsync(KickLeaveGuildRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var member = await _guildMemberRepository.GetByIdAsync(userId);
        if (member is null || member.GuildId != req.GuildId)
        {
            Logger.LogWarning("Player {UserId} tried to leave guild {GuildId} but is not a member",
                userId, req.GuildId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        if (userId == req.UserId)
        {
            var guild = await _guildRepository.GetWithMembersAsync(req.GuildId);
            if (guild is null)
            {
                Logger.LogWarning("Player {UserId} tried to leave guild {GuildId} but the guild does not exist",
                    userId, req.GuildId);
                await Send.ResultAsync(TypedResults.BadRequest());
                return;
            }

            if (member.Rank == MemberRank.LEADER &&
                !guild.Members.Any(m => m.Rank == MemberRank.LEADER && m.UserId != userId))
            {
                Logger.LogInformation("Player {UserId} left their guild '{GuildName}' ({GuildId}), causing it to be deleted",
                    userId, guild.Name, req.GuildId);
                await _guildRepository.DeleteAsync(guild);
            }
            else
            {
                Logger.LogInformation("Player {UserId} left guild '{GuildName}' ({GuildId})",
                    userId, guild.Name, req.GuildId);
                await _guildMemberRepository.DeleteAsync(member);
            }
        }
        else
        {
            var target = await _guildMemberRepository.GetByIdAsync(req.UserId);
            if (target is null || target.GuildId != req.GuildId)
            {
                Logger.LogWarning("Player {UserId} tried to kick player {TargetId} from guild {GuildId} but the target is not a member",
                    userId, req.UserId, req.GuildId);
                await Send.ResultAsync(TypedResults.BadRequest());
                return;
            }

            if (member.Rank < target.Rank)
            {
                Logger.LogWarning("Player {UserId} tried to kick player {TargetId} from guild {GuildId} but does not have permission",
                    userId, req.UserId, req.GuildId);
                await Send.ResultAsync(TypedResults.BadRequest());
                return;
            }

            Logger.LogInformation("Player {UserId} kicked player {TargetId} from guild {GuildId}",
                userId, req.UserId, req.GuildId);
            await _guildMemberRepository.DeleteAsync(target);
        }

        await Send.OkAsync();
    }
}