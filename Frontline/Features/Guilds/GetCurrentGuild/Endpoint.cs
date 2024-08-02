using FastEndpoints;
using Frontline.Data.Repositories;

namespace Frontline.Features.Guilds.GetCurrentGuild;

public class Endpoint : Endpoint<GetCurrentGuildRequest, GuildProfile, Mapper>
{
    private readonly IGuildRepository _guildRepository;

    public Endpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public override void Configure()
    {
        Get("/guildapi/guilds/current/{PlayerId}");
    }

    public override async Task HandleAsync(GetCurrentGuildRequest req, CancellationToken ct)
    {
        var guild = await _guildRepository.GetPlayerGuildAsync(req.PlayerId, true);
        if (guild == null)
        {
            await SendNotFoundAsync();
            return;
        }
        
        var response = Map.FromEntity(guild);
        
        await SendAsync(response);
    }
}