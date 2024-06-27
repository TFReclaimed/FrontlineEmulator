using System.Text.Json.Serialization;

namespace Frontline.Game;

[JsonDerivedType(typeof(Card))]
[JsonDerivedType(typeof(CommanderCard))]
[JsonDerivedType(typeof(ResourceCard))]
public class Item
{
    public int InstanceId { get; set; }
    public int TemplateId { get; set; }
    [JsonPropertyName("bundle")]
    public string AssetBundle { get; set; }
}