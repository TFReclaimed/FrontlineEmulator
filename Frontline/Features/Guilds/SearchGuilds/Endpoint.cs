using FastEndpoints;
using Frontline.Data.Repositories;

namespace Frontline.Features.Guilds.SearchGuilds;

public class Endpoint : Endpoint<SearchGuildRequest, GuildListResponse, Mapper>
{
    private readonly IGuildRepository _guildRepository;

    public Endpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public override void Configure()
    {
        Get("/guildapi/guilds");
    }

    public override async Task HandleAsync(SearchGuildRequest req, CancellationToken ct)
    {
        var guilds = _guildRepository.GetGuilds(req.Page, req.MaxCount, req.Search);

        var response = new GuildListResponse
        {
            Guilds = Map.FromEntity(guilds),
            FirstPage = req.Page == 0,
            LastPage = guilds.Count < req.MaxCount
        };

        await SendAsync(response, cancellation: ct);
    }
}