using System.Text.Json.Serialization;
using Frontline.Game;

namespace Frontline.Battle;

public class Rewards
{
    public bool IsWinner { get; set; }

    [JsonIgnore]
    public int PlayerXp { get; private set; }

    [JsonIgnore]
    public int Trophies { get; private set; }

    [JsonIgnore]
    public int Credits { get; private set; }

    [JsonIgnore]
    public int Supply { get; private set; }

    [JsonIgnore]
    public int Boosters { get; private set; }

    [JsonIgnore]
    public int Tokens { get; private set; }

    public void Generate(bool winner, List<RewardsTemplate> rewards)
    {
        IsWinner = winner;
        ClearTotals();
        for (int i = 0; i < rewards.Count; i++)
        {
            PlayerXp += rewards[i].PlayerXp;
            Trophies += rewards[i].Trophies;
            Credits += rewards[i].Credits;
            Supply += rewards[i].Supply;
            Boosters += rewards[i].Boosters;
            Tokens += rewards[i].Tokens;
        }
    }

    private void ClearTotals()
    {
        PlayerXp = 0;
        Trophies = 0;
        Credits = 0;
        Supply = 0;
        Boosters = 0;
        Tokens = 0;
    }
}