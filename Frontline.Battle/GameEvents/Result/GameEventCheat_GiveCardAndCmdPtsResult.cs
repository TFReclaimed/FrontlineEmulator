namespace Frontline.Battle.GameEvents.Result;

public class GameEventCheat_GiveCardAndCmdPtsResult : GameEventResult
{
    public byte PlayerIndex { get; set; }

    public int CardTemplateId { get; set; }

    public int CardRank { get; set; } = 1;

    public int CommandPoints { get; set; }
}