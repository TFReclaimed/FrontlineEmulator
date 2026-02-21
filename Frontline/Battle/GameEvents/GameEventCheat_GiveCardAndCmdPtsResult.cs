namespace Frontline.Battle.GameEvents;

internal class GameEventCheat_GiveCardAndCmdPtsResult : GameEventResult
{
    public byte PlayerIndex { get; }

    public int CardTemplateId { get; set; }

    public int CardRank { get; set; } = 1;

    public int CommandPoints { get; set; }
}