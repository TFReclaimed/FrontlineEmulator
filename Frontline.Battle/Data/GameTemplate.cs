using System.Text.Json.Serialization;

namespace Frontline.Battle.Data;

public class GameTemplate
{
    public short[] ControlRegionTitanSlots { get; set; } = [];
    public int EmbarkedPilotTrait { get; set; }
    public int PilotTitanEmbarkedTrait { get; set; }
    public sbyte InitialDraw { get; set; }
    public sbyte MulliganDiscard { get; set; }
    public sbyte FirstTurnDrawFirstPlayer { get; set; }
    public sbyte FirstTurnDrawOtherPlayer { get; set; }
    public sbyte NewTurnDraw { get; set; }
    public sbyte NewTurnCommand { get; set; }
    public sbyte MaxCommandAccum { get; set; }
    public sbyte InitialPlayerHealth { get; set; }
    public sbyte FirstPlayerRegionSize { get; set; }
    public sbyte OtherPlayerRegionSize { get; set; }
    public sbyte ControlRegionSize { get; set; }
    public sbyte MaxCardsInHand { get; set; }
    public bool ControlRegionSlotIndependent { get; set; }
    [JsonPropertyName("winRewardID")]
    public int WinRewardId { get; set; }
    [JsonPropertyName("lossRewardID")]
    public int LossRewardId { get; set; }

    public static void CheckEndGame(CcgGameState gameState)
    {
        var mostHealth = sbyte.MinValue;
        var winningPlayer = -1;
        var players = gameState.Players;
        for (var i = 0; i < players.Length; i++)
        {
            var resources = players[i].Resources;
            var health = resources.Health;
            if (health <= 0)
            {
                gameState.PlayerTurn = CcgGameState.GameOverIndicator;
            }
            else if (health > mostHealth)
            {
                mostHealth = health;
                winningPlayer = i;
            }
            else if (health == mostHealth)
            {
                winningPlayer = i;
            }
        }

        if (gameState.PlayerTurn == CcgGameState.GameOverIndicator)
        {
            gameState.WinningPlayer = (sbyte) winningPlayer;
            gameState.GenerateRewards();
            gameState.GetGame().EndGame();
        }
    }

    public static bool IsActive(sbyte playerIndex, CcgGameState gameState)
    {
        return gameState.Players[playerIndex].Resources.Health > 0;
    }
}