using FastEndpoints;
using Frontline.Data.Repositories;

namespace Frontline.Endpoints.Guilds.GetGuild;

public class GetGuildEndpoint : Endpoint<GetGuildRequest, GuildProfileDto>
{
    private readonly IGuildRepository _guildRepository;

    public GetGuildEndpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }
    
    public override void Configure()
    {
        Get("/guildapi/guilds/{GuildId}");
    }

    public override async Task HandleAsync(GetGuildRequest req, CancellationToken ct)
    {
        var guild = await _guildRepository.GetWithMembersAsync(req.GuildId);
        if (guild is null)
        {
            await Send.NotFoundAsync();
            return;
        }

        var response = GuildProfileDto.FromEntity(guild);
        await Send.OkAsync(response);
    }
}