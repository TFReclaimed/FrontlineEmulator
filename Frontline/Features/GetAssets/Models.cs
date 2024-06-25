using System.Xml.Serialization;

namespace Frontline.Features.GetAssets;

[XmlRoot("Root")]
public class AssetBundlesResponse : List<AssetBundle>
{
}

[XmlType("Contents")]
public class AssetBundle
{
    [XmlElement("Key")]
    public string Name { get; set; }
}