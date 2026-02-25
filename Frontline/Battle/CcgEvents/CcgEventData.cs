using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

[JsonDerivedType(typeof(CardDrawCcgEvent), "CardDrawCCGEvent")]
[JsonDerivedType(typeof(CardInfoCcgEvent), "CardInfoCCGEvent")]
[JsonDerivedType(typeof(CardTransitionCcgEvent), "CardTransitionCCGEvent")]
[JsonDerivedType(typeof(CardTraumaCcgEvent), "CardTraumaCCGEvent")]
[JsonDerivedType(typeof(CombatBuffsCcgEvent), "CombatBuffsCCGEvent")]
[JsonDerivedType(typeof(CombatCcgEvent), "CombatCCGEvent")]
[JsonDerivedType(typeof(DiscardEffectCcgEvent), "DiscardEffectCCGEvent")]
[JsonDerivedType(typeof(MulliganDrawCcgEvent), "MulliganDrawCCGEvent")]
[JsonDerivedType(typeof(MulliganDrawCcgEventCardData), "MulliganDrawCCGEventCardData")]
[JsonDerivedType(typeof(ServerDataEvent), "ServerDataEvent")]
[JsonDerivedType(typeof(ServerErrorEvent), "ServerErrorEvent")]
[JsonDerivedType(typeof(TraitActivateCcgEvent), "TraitActivateCCGEvent")]
[JsonDerivedType(typeof(TraitInfoCcgEvent), "TraitInfoCCGEvent")]
[JsonDerivedType(typeof(TurnChangeCcgEvent), "TurnChangeCCGEvent")]
public class CcgEventData
{
}