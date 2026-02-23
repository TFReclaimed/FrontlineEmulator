using System.Text.Json.Serialization;
using Frontline.Battle;
using Frontline.Game.Card;

namespace Frontline.Game;

public class Ruleset
{
    public required CardsRuleset CardsRuleset { get; set; }
    public required GamesRuleset GamesRuleset { get; set; }
    public required CardXpRanks PilotXpRanksRuleset { get; set; }
    public required CardXpRanks TitanXpRanksRuleset { get; set; }
    public required FusionUpgrades FusionUpgrades { get; set; }
    public required XpTriggers XpTriggers { get; set; }
}

public class CardsRuleset
{
    public required Dictionary<string, CardTemplate> Cards { get; set; }
    public required Dictionary<string, BaseTrait> Traits { get; set; }
    public required Dictionary<string, List<BaseTraitEffect>> Effects { get; set; }
}

public class GamesRuleset
{
    public required Dictionary<string, GameTemplate> Games { get; set; }
    public required Dictionary<string, RewardsTemplate> Rewards { get; set; }
}

public class CardXpRanks
{
    [JsonPropertyName("XPRanks")]
    public required List<CardXpEntry> XpRanks { get; set; }
}

public class CardXpEntry
{
    [JsonPropertyName("Rank")]
    public int Rank { get; set; }
    [JsonPropertyName("XPRequired")]
    public int XpRequired { get; set; }
}

public class FusionUpgrades
{
    public required Dictionary<string, FusionUpgradeSequence> Sequence { get; set; }
}

public class FusionUpgradeSequence
{
    public required Dictionary<string, FusionUpgradeEntry> Upgrades { get; set; }
}

public class XpTriggers
{
    public required List<XpTrigger> Triggers { get; set; }
}

public class XpTrigger
{
    [JsonPropertyName("Trigger")]
    public string Trigger { get; set; } = string.Empty;
    public int Xp { get; set; }
}