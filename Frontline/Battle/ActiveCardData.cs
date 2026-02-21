namespace Frontline.Battle;

public class ActiveCardData
{
    public List<ActiveTrait> activeTraits;

    public bool[] traitActivated;

    public sbyte owner;

    public ActiveCardData()
    {
        activeTraits = new List<ActiveTrait>();
    }

    public void Init(CCG game, Card ownerCard)
    {
        if (activeTraits == null)
        {
            activeTraits = new List<ActiveTrait>();
        }
        else
        {
            for (int num = activeTraits.Count - 1; num >= 0; num--)
            {
                ActiveTrait activeTrait = activeTraits[num];
                activeTrait.Init(game, ownerCard);
            }
        }

        if (ownerCard.templateId != 0 && (traitActivated == null || traitActivated.Length != ownerCard.GetNumTraits()))
        {
            Console.WriteLine("Active Data Init for card " + ownerCard.instanceId + " - traitActivated is invalid!");
            traitActivated = new bool[ownerCard.GetNumTraits()];
            for (int i = 0; i < traitActivated.Length; i++)
            {
                traitActivated[i] = false;
            }
        }
    }

    public virtual void Setup(Card card)
    {
        if (traitActivated == null || traitActivated.Length != card.GetNumTraits())
        {
            traitActivated = new bool[card.GetNumTraits()];
        }

        for (int i = 0; i < traitActivated.Length; i++)
        {
            traitActivated[i] = false;
        }
    }

    public void DeactivateTrait(int traitId, Card card, Card source)
    {
        for (int num = activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeTraits[num];
            if (activeTrait.traitEffectId == traitId)
            {
                ActiveTraitCardInfo source2 = activeTrait.source;
                if (source2.instanceId == source.instanceId && source2.owner == source.activeData.owner)
                {
                    activeTrait.Deactivate(true);
                }
            }
        }
    }

    public void DeactivateTraits()
    {
        for (int num = activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeTraits[num];
            activeTrait.Deactivate(true);
        }
    }

    public void MoveTraits(CardStack location, RegionEnum region, bool embark)
    {
        for (int num = activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeTraits[num];
            activeTrait.GetTraitInfo().Move(location, region, embark, activeTrait);
        }
    }

    public void AttackTraits(Card target)
    {
        for (int num = activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeTraits[num];
            activeTrait.GetTraitInfo().Attack(target, activeTrait);
        }
    }

    public void EmbarkTraits()
    {
        for (int num = activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeTraits[num];
            activeTrait.Embark();
        }
    }

    public void DisembarkTraits(bool hasDeter)
    {
        for (int num = activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeTraits[num];
            activeTrait.Disembark(hasDeter);
        }
    }
}