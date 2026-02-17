using System.Text.Json.Serialization;

namespace Frontline.Endpoints.Session.Inventory.Consume;

public class ConsumeRequest
{
    public int ItemId { get; set; }
    public RetireFor? RetireFor { get; set; }
    public int? TargetId { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RetireFor
{
    CREDITS,
    XP
}

// Yes, they really used strings instead of integers...
public class RetireForCreditsResponse
{
    public string Credits { get; set; } = "0";
}

public class RetireForXpResponse
{
    public string Xp { get; set; } = "0";
}