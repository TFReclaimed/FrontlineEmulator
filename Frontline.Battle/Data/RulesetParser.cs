using System.Text.Json;
using Frontline.Battle.Data.Card;

namespace Frontline.Battle.Data;

public static class RulesetParser
{
    public static Ruleset? Ruleset { get; private set; }
    
    public static string? RulesetJson { get; private set; }

    private static readonly JsonSerializerOptions CloneOptions = new();

    private static readonly HashSet<int> CommandDeckCardIds = [];

    public static void Initialize()
    {
        var rulesetPath = Path.Combine(AppContext.BaseDirectory, "Ruleset.json");
        RulesetJson = File.ReadAllText(rulesetPath);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        Ruleset = JsonSerializer.Deserialize<Ruleset>(RulesetJson, options);

        MarkCommandDeckCards();
        AssignFusionUpgradeSequences();
    }

    private static void MarkCommandDeckCards()
    {
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

    private static void AssignFusionUpgradeSequences()
    {
        foreach (var (key, upgradeSequence) in Ruleset!.FusionUpgrades.Sequence)
        {
            var templateId = int.Parse(key);
            var cardTemplate = GetCardTemplate(templateId);
            if (cardTemplate is UnitCardTemplate unitTemplate && unitTemplate.IsCombatUnit())
            {
                unitTemplate.SetUpgradeSequence(upgradeSequence);
            }
        }
    }

    public static CardTemplate? GetCardTemplate(int templateId)
    {
        return Ruleset?.CardsRuleset.Cards.GetValueOrDefault(templateId.ToString());
    }

    public static CardTemplate? GetCardTemplate(int templateId, sbyte rank)
    {
        var cardTemplate = GetCardTemplate(templateId);
        return cardTemplate?.GetRankedTemplate(rank);
    }

    public static BaseTrait? GetTraitTemplate(int traitId)
    {
        var template = Ruleset?.CardsRuleset.Traits.GetValueOrDefault(traitId.ToString());
        if (template == null)
        {
            return null;
        }

        return new BaseTrait
        {
            TraitId = template.TraitId,
            TraitType = template.TraitType,
            Effects = GetTraitEffectsList(traitId)
        };
    }

    public static List<BaseTraitEffect> GetTraitEffectsList(int traitId)
    {
        var templateEffects = Ruleset?.CardsRuleset.Effects.GetValueOrDefault(traitId.ToString());
        if (templateEffects == null || templateEffects.Count == 0)
        {
            return [];
        }

        var clonedEffects = new List<BaseTraitEffect>(templateEffects.Count);
        foreach (var templateEffect in templateEffects)
        {
            clonedEffects.Add(CloneTraitEffect(templateEffect));
        }

        return clonedEffects;
    }

    private static BaseTraitEffect CloneTraitEffect(BaseTraitEffect templateEffect)
    {
        var clone = JsonSerializer.Deserialize(
            JsonSerializer.Serialize(templateEffect, templateEffect.GetType(), CloneOptions),
            templateEffect.GetType(),
            CloneOptions
        );

        if (clone is not BaseTraitEffect traitEffect)
        {
            throw new InvalidOperationException($"Failed to clone trait effect template {templateEffect.GetType().Name}");
        }

        return traitEffect;
    }

    public static GameTemplate? GetGameTemplate(int id)
    {
        return Ruleset?.GamesRuleset.Games.GetValueOrDefault(id.ToString());
    }

    public static RewardsTemplate? GetRewardsTemplate(int id)
    {
        return Ruleset?.GamesRuleset.Rewards.GetValueOrDefault(id.ToString());
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

    public static int GetXpTrigger(string triggerName)
    {
        var trigger = Ruleset?.XpTriggers.Triggers.FirstOrDefault(t => t.Trigger == triggerName);
        return trigger?.Xp ?? 0;
    }

    public static CardSetEntry? GetCardSetEntry(int setId)
    {
        return Ruleset?.CardSet.Sets.GetValueOrDefault(setId.ToString());
    }

    public static bool IsCommandDeckCard(int templateId)
    {
        return CommandDeckCardIds.Contains(templateId);
    }
}