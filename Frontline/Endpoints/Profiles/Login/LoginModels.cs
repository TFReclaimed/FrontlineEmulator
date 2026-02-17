using System.Text.Json.Serialization;

namespace Frontline.Endpoints.Profiles.Login;

public class LoginRequest
{
    public required LoginParams Param { get; set; }
}

public class LoginParams
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LoginType LoginType { get; set; }
    [JsonPropertyName("authID")]
    public string AuthId { get; set; } = string.Empty;
    [JsonPropertyName("pass")]
    public string Password { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Platform DeviceType { get; set; }
}

public enum LoginType
{
    Facebook = 0,
    GameCenter = 1,
    GooglePlay = 2,
    EmailLogin = 3,
    Guest = 4,
    TOY = 5,
    NumTypes = 6
}

public enum Platform
{
    iOS = 0,
    Android = 1,
    Computer = 2,
    NumPlatforms = 3
}