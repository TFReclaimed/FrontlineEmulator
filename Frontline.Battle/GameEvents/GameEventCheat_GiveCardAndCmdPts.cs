using Frontline.Battle.GameEvents.Result;

namespace Frontline.Battle.GameEvents;

public class GameEventCheat_GiveCardAndCmdPts : GameEventParams
{
    public int CardTemplateId { get; set; }

    public int CardRank { get; set; }

    public int CommandPoints { get; set; }

    public override GameEventResult? ReplayEvent(CcgGame game)
    {
        var result = new GameEventCheat_GiveCardAndCmdPtsResult
        {
            CardTemplateId = CardTemplateId,
            CommandPoints = CommandPoints,
            CardRank = CardRank
        };

        if (game.Cheat_GiveCardAndCommandPoints(PlayerIndex, result.CardTemplateId, result.CardRank,
                result.CommandPoints))
        {
            CcgEventsLog = game.GameState.GetCcgEventLog();
            return result;
        }

        return null;
    }
}