using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonDerivedType(typeof(ActiveEntityCardData), "ActiveEntityCardData")]
[JsonDerivedType(typeof(ActiveUnitCardData), "ActiveUnitCardData")]
public class ActiveCardData
{
    public List<ActiveTrait> ActiveTraits { get; set; } = [];

    public bool[] TraitActivated { get; set; } = [];

    public sbyte Owner { get; set; }

    public void Init(CcgGameState game, Card ownerCard)
    {
        for (var num = ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveTraits[num];
            activeTrait.Init(ownerCard);
        }

        if (ownerCard.TemplateId != 0 && TraitActivated.Length != ownerCard.GetNumTraits())
        {
            game.Logger.Warning("Active Data Init for card " + ownerCard.InstanceId + " - traitActivated is invalid!");
            TraitActivated = new bool[ownerCard.GetNumTraits()];
            for (var i = 0; i < TraitActivated.Length; i++)
            {
                TraitActivated[i] = false;
            }
        }
    }

    public virtual void Setup(Card card)
    {
        if (TraitActivated.Length != card.GetNumTraits())
        {
            TraitActivated = new bool[card.GetNumTraits()];
        }

        for (var i = 0; i < TraitActivated.Length; i++)
        {
            TraitActivated[i] = false;
        }
    }

    public void DeactivateTraits()
    {
        for (var num = ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveTraits[num];
            activeTrait.Deactivate(true);
        }
    }

    public void MoveTraits(CardStack location, Region region, bool embark)
    {
        for (var num = ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveTraits[num];
            activeTrait.GetTraitInfo().Move(location, region, embark, activeTrait);
        }
    }

    public void AttackTraits(Card target)
    {
        for (var num = ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveTraits[num];
            activeTrait.GetTraitInfo().Attack(target, activeTrait);
        }
    }

    public void EmbarkTraits()
    {
        for (var num = ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveTraits[num];
            activeTrait.Embark();
        }
    }

    public void DisembarkTraits(bool hasDeter)
    {
        for (var num = ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveTraits[num];
            activeTrait.Disembark(hasDeter);
        }
    }
}