namespace Frontline.Battle.CcgEvents;

public class TraitActivateCCGEvent : CCGEventData
{
    public int traitID;

    public int effectID;

    public int cardID;

    public sbyte owner;

    public bool deactivate;

    public RegionEnum region = RegionEnum.NumRegions;

    public ActiveTraitCardInfo[] targets;

    public TraitActivateCCGEvent()
    {
    }

    public TraitActivateCCGEvent(int baseTraitID, int traitEffectID, int sourceCardID, sbyte cardOwner,
        bool deactivateTrait)
    {
        traitID = baseTraitID;
        effectID = traitEffectID;
        cardID = sourceCardID;
        owner = cardOwner;
        deactivate = deactivateTrait;
    }

    public override CCGEventType Type()
    {
        return CCGEventType.TraitActivation;
    }
}