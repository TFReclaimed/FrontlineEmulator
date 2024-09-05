using System.Text.Json;

namespace Frontline.Game;

public static class RulesetParser
{
    public static Ruleset? Ruleset { get; private set; }
    
    public static string? RulesetJson { get; private set; }
    
    private static readonly HashSet<int> CommandDeckCardIds = [];

    public static void Initialize()
    {
        var rulesetPath = Path.Combine(AppContext.BaseDirectory, "ruleset.json");
        RulesetJson = File.ReadAllText(rulesetPath);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        Ruleset = JsonSerializer.Deserialize<Ruleset>(RulesetJson, options);
        
        foreach (var cardTemplate in Ruleset!.CardsRuleset.Cards.Values)
        {
            if (cardTemplate.Type != CardType.Commander)
            {
                continue;
            }
            
            var commanderCard = (CommanderCardTemplate) cardTemplate;
            foreach (var supportId in commanderCard.SupportIds)
            {
                CommandDeckCardIds.Add(supportId);
            }
        }
    }
    
    public static CardTemplate? GetCardTemplate(int templateId)
    {
        return Ruleset?.CardsRuleset.Cards.GetValueOrDefault(templateId.ToString());
    }
    
    public static CardXpEntry? GetXpEntry(CardType type, int rank)
    {
        var xpRanks = type switch
        {
            CardType.Pilot => Ruleset?.PilotXpRanksRuleset.XpRanks,
            CardType.Titan => Ruleset?.TitanXpRanksRuleset.XpRanks,
            _ => null
        };

        return xpRanks?.FirstOrDefault(entry => entry.Rank == rank);
    }
    
    public static bool IsCommandDeckCard(int templateId)
    {
        return CommandDeckCardIds.Contains(templateId);
    }
}