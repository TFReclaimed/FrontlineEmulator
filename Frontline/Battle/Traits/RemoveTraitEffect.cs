using System.Text.Json.Serialization;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class RemoveTraitEffect : BaseTraitEffect
{
    public const sbyte BurnCard = 1;

    public const sbyte Secret = 2;

    public const sbyte Activated = 3;

    public const sbyte Stealth = 4;

    public const sbyte TraitId = 5;

    [JsonPropertyName("traitType")]
    public sbyte RemoveTraitType { get; set; }

    [JsonPropertyName("templateID")]
    public int TemplateId { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        for (var num = card.ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = card.ActiveData.ActiveTraits[num];
            if (DoesTraitMatch(activeTrait))
            {
                activeTrait.Deactivate(true);
            }
        }
    }

    private bool DoesTraitMatch(ActiveTrait active)
    {
        switch (RemoveTraitType)
        {
            case BurnCard:
                if (active.GetTraitSource() != null)
                {
                    var traitSource2 = active.GetTraitSource();
                    if (traitSource2.GetTemplate().Type == CardType.BurnCard)
                    {
                        return true;
                    }
                }
                else
                {
                    var traitTemplate2 = RulesetParser.GetTraitTemplate(active.TraitSourceId);
                    if (traitTemplate2.TraitType == TraitType.BurnCard)
                    {
                        return true;
                    }
                }

                break;
            case Secret:
                if (active.GetTraitSource() != null)
                {
                    var traitSource = active.GetTraitSource();
                    if (traitSource.GetTemplate().Type == CardType.Secret)
                    {
                        return true;
                    }
                }
                else
                {
                    var traitTemplate = RulesetParser.GetTraitTemplate(active.TraitSourceId);
                    if (traitTemplate.TraitType == TraitType.Secret)
                    {
                        return true;
                    }
                }

                break;
            case Stealth:
                if (active.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.Stealth, active))
                {
                    return true;
                }

                break;
            case TraitId:
                if (active.TraitSourceId == TemplateId)
                {
                    return true;
                }

                break;
        }

        return false;
    }
}