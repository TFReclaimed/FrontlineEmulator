namespace Frontline.Battle.GameEvents;

internal class GameEventCheat_GiveCardAndCmdPts : GameEventParams
{
    public int CardTemplateId { get; }

    public int CardRank { get; }

    public int CommandPoints { get; }

    public GameEventCheat_GiveCardAndCmdPts()
    {
    }

    public GameEventCheat_GiveCardAndCmdPts(int _cardTemplateId, int _rank, int _commandPoints, sbyte player)
        : base(GameEvent.Cheat_GiveCardAndCmdPts, player)
    {
        CardTemplateId = _cardTemplateId;
        CommandPoints = _commandPoints;
        CardRank = _rank;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        GameEventCheat_GiveCardAndCmdPtsResult gameEventCheat_GiveCardAndCmdPtsResult =
            (GameEventCheat_GiveCardAndCmdPtsResult) EventResult;
        gameEventCheat_GiveCardAndCmdPtsResult = new GameEventCheat_GiveCardAndCmdPtsResult();
        gameEventCheat_GiveCardAndCmdPtsResult.CardTemplateId = CardTemplateId;
        gameEventCheat_GiveCardAndCmdPtsResult.CommandPoints = CommandPoints;
        gameEventCheat_GiveCardAndCmdPtsResult.CardRank = CardRank;

        if (game.Cheat_GiveCardAndCommandPoints(PlayerIndex, gameEventCheat_GiveCardAndCmdPtsResult.CardTemplateId,
                gameEventCheat_GiveCardAndCmdPtsResult.CardRank, gameEventCheat_GiveCardAndCmdPtsResult.CommandPoints,
                true) == 1)
        {
            CcgEventsLog = game.GameState.GetCCGEventLog();
            return gameEventCheat_GiveCardAndCmdPtsResult;
        }

        return null;
    }
}