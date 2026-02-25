using System.Text.Json.Serialization;

namespace Frontline.Battle.GameEvents.Result;

[JsonDerivedType(typeof(DiscardEventResult), "DiscardEventResult")]
[JsonDerivedType(typeof(GameEventCheat_GiveCardAndCmdPtsResult), "GameEventCheat_GiveCardAndCmdPtsResult")]
[JsonDerivedType(typeof(InitialSwapEventResult), "InitialSwapEventResult")]
public class GameEventResult
{
    public static readonly GameEventResult OkResult = new();
}