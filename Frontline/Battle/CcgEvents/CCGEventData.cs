using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

[JsonDerivedType(typeof(CardDrawCCGEvent), "CardDrawCCGEvent")]
[JsonDerivedType(typeof(CardInfoCCGEvent), "CardInfoCCGEvent")]
[JsonDerivedType(typeof(CardTransitionCCGEvent), "CardTransitionCCGEvent")]
[JsonDerivedType(typeof(CardTraumaCCGEvent), "CardTraumaCCGEvent")]
[JsonDerivedType(typeof(CombatBuffsCCGEvent), "CombatBuffsCCGEvent")]
[JsonDerivedType(typeof(CombatCCGEvent), "CombatCCGEvent")]
[JsonDerivedType(typeof(DiscardEffectCCGEvent), "DiscardEffectCCGEvent")]
[JsonDerivedType(typeof(MulliganDrawCCGEvent), "MulliganDrawCCGEvent")]
[JsonDerivedType(typeof(MulliganDrawCCGEventCardData), "MulliganDrawCCGEventCardData")]
[JsonDerivedType(typeof(ServerDataEvent), "ServerDataEvent")]
[JsonDerivedType(typeof(ServerErrorEvent), "ServerErrorEvent")]
[JsonDerivedType(typeof(TraitActivateCCGEvent), "TraitActivateCCGEvent")]
[JsonDerivedType(typeof(TraitInfoCCGEvent), "TraitInfoCCGEvent")]
[JsonDerivedType(typeof(TurnChangeCCGEvent), "TurnChangeCCGEvent")]
public class CCGEventData
{
    public virtual CcgEventType Type()
    {
        return CcgEventType.NumTypes;
    }

    public virtual CCGEventData Sanitize(sbyte playerIndex)
    {
        return this;
    }
}