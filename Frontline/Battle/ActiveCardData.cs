using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonDerivedType(typeof(ActiveEntityCardData), "ActiveEntityCardData")]
[JsonDerivedType(typeof(ActiveUnitCardData), "ActiveUnitCardData")]
public class ActiveCardData
{
    public List<ActiveTrait> ActiveTraits { get; set; }

    public bool[] TraitActivated { get; set; }

    public sbyte Owner { get; set; }

    public ActiveCardData()
    {
        ActiveTraits = new List<ActiveTrait>();
    }

    public void Init(CCG game, Card ownerCard)
    {
        if (ActiveTraits == null)
        {
            ActiveTraits = new List<ActiveTrait>();
        }
        else
        {
            for (var num = ActiveTraits.Count - 1; num >= 0; num--)
            {
                var activeTrait = ActiveTraits[num];
                activeTrait.Init(game, ownerCard);
            }
        }

        if (ownerCard.TemplateId != 0 && (TraitActivated == null || TraitActivated.Length != ownerCard.GetNumTraits()))
        {
            Console.WriteLine("Active Data Init for card " + ownerCard.InstanceId + " - traitActivated is invalid!");
            TraitActivated = new bool[ownerCard.GetNumTraits()];
            for (var i = 0; i < TraitActivated.Length; i++)
            {
                TraitActivated[i] = false;
            }
        }
    }

    public virtual void Setup(Card card)
    {
        if (TraitActivated == null || TraitActivated.Length != card.GetNumTraits())
        {
            TraitActivated = new bool[card.GetNumTraits()];
        }

        for (var i = 0; i < TraitActivated.Length; i++)
        {
            TraitActivated[i] = false;
        }
    }

    public void DeactivateTrait(int traitId, Card card, Card source)
    {
        for (var num = ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveTraits[num];
            if (activeTrait.TraitEffectId == traitId)
            {
                var source2 = activeTrait.Source;
                if (source2.InstanceId == source.InstanceId && source2.Owner == source.ActiveData.Owner)
                {
                    activeTrait.Deactivate(true);
                }
            }
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