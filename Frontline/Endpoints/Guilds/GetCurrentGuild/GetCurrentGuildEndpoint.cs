using FastEndpoints;
using Frontline.Data.Repositories;

namespace Frontline.Endpoints.Guilds.GetCurrentGuild;

public class GetCurrentGuildEndpoint : Endpoint<GetCurrentGuildRequest, GuildProfileDto>
{
    private readonly IGuildRepository _guildRepository;

    public GetCurrentGuildEndpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public override void Configure()
    {
        Get("/guildapi/guilds/current/{PlayerId}");
    }

    public override async Task HandleAsync(GetCurrentGuildRequest req, CancellationToken ct)
    {
        var guild = await _guildRepository.GetPlayerGuildAsync(req.PlayerId);
        if (guild == null)
        {
            await Send.NotFoundAsync();
            return;
        }

        var response = GuildProfileDto.FromEntity(guild);
        await Send.OkAsync(response);
    }
}