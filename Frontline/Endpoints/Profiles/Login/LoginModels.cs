using System.Text.Json.Serialization;

namespace Frontline.Endpoints.Profiles.Login;

public class LoginRequest
{
    public required LoginParams Param { get; set; }
}

public class LoginParams
{
    [JsonPropertyName("type")]
    public LoginType LoginType { get; set; }
    [JsonPropertyName("authID")]
    public string AuthId { get; set; } = string.Empty;
    [JsonPropertyName("pass")]
    public string Password { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public Platform DeviceType { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<LoginType>))]
public enum LoginType
{
    Facebook,
    GameCenter,
    GooglePlay,
    EmailLogin,
    Guest,
    [JsonStringEnumMemberName("TOY")]
    Toy,
    NumTypes
}