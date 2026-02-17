using Frontline.Data.Entities;

namespace Frontline.Features.Session.Inventory.Dropships.GetDropships;

public class DropshipInfo
{
    public int Index { get; set; }
    public required CardDto[] SlottedCards { get; set; }
    public int InstanceId { get; set; }
}