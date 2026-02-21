namespace Frontline.Battle.GameEvents;

internal class GameEventCheat_GiveCardAndCmdPts : GameEventParams
{
    public int cardTemplateId;

    public int cardRank;

    public int commandPoints;

    public GameEventCheat_GiveCardAndCmdPts()
    {
    }

    public GameEventCheat_GiveCardAndCmdPts(int _cardTemplateId, int _rank, int _commandPoints, sbyte player)
        : base(GameEvent.Cheat_GiveCardAndCmdPts, player)
    {
        cardTemplateId = _cardTemplateId;
        commandPoints = _commandPoints;
        cardRank = _rank;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        GameEventCheat_GiveCardAndCmdPtsResult gameEventCheat_GiveCardAndCmdPtsResult =
            (GameEventCheat_GiveCardAndCmdPtsResult) eventResult;
        gameEventCheat_GiveCardAndCmdPtsResult = new GameEventCheat_GiveCardAndCmdPtsResult();
        gameEventCheat_GiveCardAndCmdPtsResult.cardTemplateId = cardTemplateId;
        gameEventCheat_GiveCardAndCmdPtsResult.commandPoints = commandPoints;
        gameEventCheat_GiveCardAndCmdPtsResult.cardRank = cardRank;

        if (game.Cheat_GiveCardAndCommandPoints(playerIndex, gameEventCheat_GiveCardAndCmdPtsResult.cardTemplateId,
                gameEventCheat_GiveCardAndCmdPtsResult.cardRank, gameEventCheat_GiveCardAndCmdPtsResult.commandPoints,
                true) == 1)
        {
            ccgEventsLog = game.GameState.GetCCGEventLog();
            return gameEventCheat_GiveCardAndCmdPtsResult;
        }

        return null;
    }
}