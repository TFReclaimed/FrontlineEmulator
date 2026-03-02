using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonDerivedType(typeof(Card), "Card")]
public class Item
{
    public int InstanceId { get; set; }

    public int TemplateId { get; set; }

    protected Item()
    {
    }
}