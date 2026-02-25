using System.Text.Json.Serialization;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class RemoveTraitEffect : BaseTraitEffect
{
    public const sbyte burnCard = 1;

    public const sbyte secret = 2;

    public const sbyte activated = 3;

    public const sbyte stealth = 4;

    public const sbyte traitID = 5;

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
            case 1:
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
            case 2:
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
            case 4:
                if (active.GetTraitInfo().IsCombatManipulationPassive(1, active))
                {
                    return true;
                }

                break;
            case 5:
                if (active.TraitSourceId == TemplateId)
                {
                    return true;
                }

                break;
        }

        return false;
    }
}