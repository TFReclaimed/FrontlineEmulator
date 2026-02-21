using System.Text.Json.Serialization;
using Frontline.Battle.GameEvents;

namespace Frontline.Battle;

[JsonDerivedType(typeof(DiscardEventResult), "DiscardEventResult")]
[JsonDerivedType(typeof(GameEventCheat_GiveCardAndCmdPtsResult), "GameEventCheat_GiveCardAndCmdPtsResult")]
[JsonDerivedType(typeof(InitialSwapEventResult), "InitialSwapEventResult")]
public class GameEventResult
{
    public static readonly GameEventResult OK_RESULT = new GameEventResult();

    public GameEventResult GetResultData()
    {
        if (GetType() == typeof(GameEventResult))
        {
            return null;
        }

        return this;
    }
}