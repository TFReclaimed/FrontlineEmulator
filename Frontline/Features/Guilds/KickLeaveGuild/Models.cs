using FastEndpoints;

namespace Frontline.Features.Guilds.KickLeaveGuild;

// Needs to be a plain text request, otherwise FastEndpoints will complain about no JSON body
public class KickLeaveGuildRequest : IPlainTextRequest
{
    public Guid GuildId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
}