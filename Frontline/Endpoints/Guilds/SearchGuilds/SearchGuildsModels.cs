using System.Text.Json.Serialization;
using FastEndpoints;

namespace Frontline.Endpoints.Guilds.SearchGuilds;

public class SearchGuildRequest
{
    public int Page { get; set; }
    [BindFrom("size")]
    public int MaxCount { get; set; }
    public string Search { get; set; } = string.Empty;
}

public class GuildListResponse
{
    [JsonPropertyName("content")]
    public required List<GuildProfileDto> Guilds { get; set; }
    [JsonPropertyName("first")]
    public bool FirstPage { get; set; }
    [JsonPropertyName("last")]
    public bool LastPage { get; set; }
}