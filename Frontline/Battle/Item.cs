using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonDerivedType(typeof(Card), "Card")]
[JsonDerivedType(typeof(CommanderCard), "CommanderCard")]
[JsonDerivedType(typeof(EntityCard), "EntityCard")]
[JsonDerivedType(typeof(UnitCard), "UnitCard")]
public class Item
{
    public int InstanceId { get; set; }

    public int TemplateId { get; set; }

    protected Item()
    {
    }
}