namespace Frontline.Battle.GameEvents;

public class InitialSwapEventResult : GameEventResult
{
    public int[] CardIdsRemovedFromHand { get; set; }

    public int[] DeckReplacementIndices { get; set; }
}