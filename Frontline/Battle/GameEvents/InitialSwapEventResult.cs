namespace Frontline.Battle.GameEvents;

public class InitialSwapEventResult : GameEventResult
{
    public int[] CardIdsRemovedFromHand;

    public int[] DeckReplacementIndices;
}