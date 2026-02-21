using System.Text.Json.Serialization;
using Frontline.Battle;

namespace Frontline.Game;

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

    public void CheckEndGame(CCG gameState)
    {
        sbyte b = -127;
        int num = -1;
        Player[] players = gameState.players;
        for (int i = 0; i < players.Length; i++)
        {
            GameResources resources = players[i].resources;
            sbyte health = resources.health;
            if (health <= 0)
            {
                gameState.playerTurn = -1;
            }
            else if (health > b)
            {
                b = health;
                num = i;
            }
            else if (health == b)
            {
                num = i;
            }
        }
        if (gameState.playerTurn == -1)
        {
            gameState.winningPlayer = (sbyte)num;
            gameState.GenerateRewards();
        }
    }

    public bool IsActive(sbyte playerIndex, CCG gameState)
    {
        return gameState.players[playerIndex].resources.health > 0;
    }
}