using FastEndpoints;

namespace Frontline.Features.Guilds.JoinGuild;

// Needs to be a plain text request, otherwise FastEndpoints will complain about no JSON body
public class JoinGuildRequest : IPlainTextRequest
{
    public Guid GuildId { get; set; }
    public string Content { get; set; } = string.Empty;
}