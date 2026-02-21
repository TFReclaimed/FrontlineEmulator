namespace Frontline.Battle.CcgEvents;

public class TraitInfoCCGEvent : CCGEventData
{
    public CCGEventType infoType;

    public int traitID;

    public int effectID;

    public int targetCardID;

    public sbyte targetOwner;

    public int sourceCardID;

    public sbyte sourceOwner;

    public sbyte data;

    public RegionEnum region = RegionEnum.NumRegions;

    public ActiveTraitCardInfo[] targets;

    public TraitInfoCCGEvent()
    {
    }

    public TraitInfoCCGEvent(CCGEventType type, int baseTraitID, int traitEffectID, int targetInstanceID,
        sbyte targetPlayerIdx, int sourceInstanceID, sbyte sourcePlayerIdx, sbyte info)
    {
        infoType = type;
        traitID = baseTraitID;
        effectID = traitEffectID;
        targetCardID = targetInstanceID;
        targetOwner = targetPlayerIdx;
        sourceCardID = sourceInstanceID;
        sourceOwner = sourcePlayerIdx;
        data = info;
    }

    public override CCGEventType Type()
    {
        return infoType;
    }
}