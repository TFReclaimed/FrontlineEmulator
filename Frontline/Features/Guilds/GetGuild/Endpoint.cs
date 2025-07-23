using FastEndpoints;
using Frontline.Data.Repositories;

namespace Frontline.Features.Guilds.GetGuild;

public class Endpoint : Endpoint<GetGuildRequest, GuildProfile, Mapper>
{
    private readonly IGuildRepository _guildRepository;

    public Endpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }
    
    public override void Configure()
    {
        Get("/guildapi/guilds/{GuildId}");
    }

    public override async Task HandleAsync(GetGuildRequest req, CancellationToken ct)
    {
        var guild = await _guildRepository.GetGuildAsync(req.GuildId, true);
        if (guild is null)
        {
            await Send.NotFoundAsync();
            return;
        }
        
        var response = Map.FromEntity(guild);
        
        await Send.OkAsync(response);
    }
}