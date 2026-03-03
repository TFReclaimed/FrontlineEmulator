using System.Text.Json.Serialization;

namespace Frontline.Endpoints.Session.Inventory;

public class GetInventoryRequest
{
    public required InventoryRequest Param { get; set; }
}

public class InventoryRequest
{
    public int MinItem { get; set; }
    public int MaxItem { get; set; } = -1;
    public InventoryType Type { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<InventoryType>))]
public enum InventoryType
{
    Card,
    Dropship,
    BoosterPack,
    [JsonStringEnumMemberName("CardXPBoost")]
    CardXpBoost,
    OperationalIntel,
    TechnicalIntel,
    PersonnelIntel,
    NumTypes
}