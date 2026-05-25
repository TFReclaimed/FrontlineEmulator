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
        var page = req.Page;
        var maxCount = req.MaxCount;
        if (page < 0)
        {
            page = 0;
        }

        if (maxCount <= 0)
        {
            maxCount = 50;
        }

        var guilds = await _guildRepository.SearchGuildsAsync(page, maxCount, req.Search);

        var response = new GuildListResponse
        {
            Guilds = guilds.Select(GuildProfileDto.FromEntity).ToList(),
            FirstPage = page == 0,
            LastPage = guilds.Count < maxCount
        };

        await Send.OkAsync(response);
    }
}