using System.ComponentModel.DataAnnotations;

namespace Frontline.Options;

[OptionsSection("StarterItemSettings")]
public class StarterItemOptions
{
    [Required]
    public List<StarterItem> Items { get; set; } = [];
}

public class StarterItem
{
    public int TemplateId { get; set; }
    public List<Dropship>? Dropships { get; set; }
}

public class Dropship
{
    [RegularExpression("^(1|2|10|11|12|13|14|15|16|17)$")]
    public int DropshipId { get; set; }
    [Range(0, 40)]
    public int SlotIndex { get; set; }
}