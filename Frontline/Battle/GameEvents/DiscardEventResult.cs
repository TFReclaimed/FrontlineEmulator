namespace Frontline.Battle.GameEvents;

public class DiscardEventResult : GameEventResult
{
    public int[] CardIdsRemovedFromHand { get; set; }
}