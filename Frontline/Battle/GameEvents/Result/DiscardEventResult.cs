namespace Frontline.Battle.GameEvents.Result;

public class DiscardEventResult : GameEventResult
{
    public int[] CardIdsRemovedFromHand { get; set; }
}