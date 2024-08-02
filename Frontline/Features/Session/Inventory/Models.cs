using System.Text.Json.Serialization;

namespace Frontline.Features.Session.Inventory;

public class GetInventoryRequest
{
    public required InventoryRequest Param { get; set; }
}

public class InventoryRequest
{
    public int MinItem { get; set; }
    public int MaxItem { get; set; } = -1;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InventoryType Type { get; set; }
}

public enum InventoryType
{
    Card = 0,
    Dropship = 1,
    BoosterPack = 2,
    CardXPBoost = 3,
    OperationalIntel = 4,
    TechnicalIntel = 5,
    PersonnelIntel = 6,
    NumTypes = 7
}