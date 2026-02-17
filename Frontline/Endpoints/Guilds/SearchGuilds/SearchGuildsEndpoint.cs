using FastEndpoints;
using Frontline.Data.Repositories;

namespace Frontline.Endpoints.Guilds.SearchGuilds;

public class SearchGuildsEndpoint : Endpoint<SearchGuildRequest, GuildListResponse>
{
    private readonly IGuildRepository _guildRepository;

    public SearchGuildsEndpoint(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public override void Configure()
    {
        Get("/guildapi/guilds");
    }

    public override async Task HandleAsync(SearchGuildRequest req, CancellationToken ct)
    {
        var guilds = await _guildRepository.SearchGuildsAsync(req.Page, req.MaxCount, req.Search);

        var response = new GuildListResponse
        {
            Guilds = guilds.Select(GuildProfileDto.FromEntity).ToList(),
            FirstPage = req.Page == 0,
            LastPage = guilds.Count < req.MaxCount
        };

        await Send.OkAsync(response);
    }
}