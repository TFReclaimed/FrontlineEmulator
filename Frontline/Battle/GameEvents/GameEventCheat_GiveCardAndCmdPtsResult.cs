namespace Frontline.Battle.GameEvents;

internal class GameEventCheat_GiveCardAndCmdPtsResult : GameEventResult
{
    public byte playerIndex;

    public int cardTemplateId;

    public int cardRank = 1;

    public int commandPoints;
}