using System.Text.Json.Serialization;
using FastEndpoints;

namespace Frontline.Features.Guilds.SearchGuilds;

public class SearchGuildRequest
{
    public int Page { get; set; }
    [BindFrom("size")]
    public int MaxCount { get; set; }
    public string Search { get; set; }
}

public class GuildListResponse
{
    [JsonPropertyName("content")]
    public List<GuildProfile> Guilds { get; set; }
    [JsonPropertyName("first")]
    public bool FirstPage { get; set; }
    [JsonPropertyName("last")]
    public bool LastPage { get; set; }
}