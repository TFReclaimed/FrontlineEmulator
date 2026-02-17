using System.Text.Json.Serialization;

namespace Frontline.Endpoints.Profiles.GetProfile;

public class GetProfileRequest
{
    public int UserId { get; set; }
    public ProfileType ProfileType { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProfileType
{
    Public,
    Private
}