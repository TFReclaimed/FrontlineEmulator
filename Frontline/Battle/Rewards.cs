using Frontline.Game;

namespace Frontline.Battle;

public class Rewards
{
    public bool IsWinner { get; set; }

    private int playerXP;

    private int trophies;

    private int credits;

    private int supply;

    private int boosters;

    private int tokens;

    public int GetXPTotal()
    {
        return playerXP;
    }

    public int GetCreditsTotal()
    {
        return credits;
    }

    public int GetSupplyTotal()
    {
        return supply;
    }

    public int GetTrophiesTotal()
    {
        return trophies;
    }

    public int GetTokensTotal()
    {
        return tokens;
    }

    public int GetBoostersTotal()
    {
        return boosters;
    }

    public void Generate(bool winner, List<RewardsTemplate> rewards)
    {
        IsWinner = winner;
        ClearTotals();
        for (int i = 0; i < rewards.Count; i++)
        {
            playerXP += rewards[i].PlayerXp;
            trophies += rewards[i].Trophies;
            credits += rewards[i].Credits;
            supply += rewards[i].Supply;
            boosters += rewards[i].Boosters;
            tokens += rewards[i].Tokens;
        }
    }

    private void ClearTotals()
    {
        playerXP = 0;
        trophies = 0;
        credits = 0;
        supply = 0;
        boosters = 0;
        tokens = 0;
    }
}