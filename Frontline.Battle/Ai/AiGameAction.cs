namespace Frontline.Battle.Ai;

public class AiGameAction
{
    public GameEvent ActionType { get; set; }
    public float Weight { get; set; }
    public int SourceCardId { get; set; }
    public int TargetCardId { get; set; }
    public bool Hostile { get; set; }
    public TargetableArea Area { get; set; }
    public Region Region { get; set; }
    public sbyte SlotIndex { get; set; }
    public sbyte PushDir { get; set; }

    public static int SortByWeight(AiGameAction action1, AiGameAction action2)
    {
        if (action1.Weight > action2.Weight)
        {
            return -1;
        }

        if (action1.Weight < action2.Weight)
        {
            return 1;
        }

        return 0;
    }
}