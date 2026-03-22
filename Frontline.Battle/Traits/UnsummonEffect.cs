using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class UnsummonEffect : BaseTraitEffect
{
    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var owner = card.ActiveData.Owner;
        var player = GameState.Players[owner];
        if (Targets.Area is TargetableArea.EnemyDiscard or TargetableArea.FriendlyDiscard)
        {
            var discard = player.Discard;
            discard.RemoveCard(card.InstanceId);
        }
        else
        {
            var list = GameState.FindCardStack(card);
            if (list.Count == 0)
            {
                return;
            }

            var card2 = list[0].RemoveCard(card.InstanceId, owner)!;
            var secrets = card2.GetSecrets();
            for (var num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].Discard(GameState.Players);
                GameState.SecretDestroyed(secrets[num], source);
            }

            if (card2.HasPilot())
            {
                card2 = card2.GetEmbarkedPilot()!;
                secrets = card2.GetSecrets();
                for (var num2 = secrets.Count - 1; num2 >= 0; num2--)
                {
                    secrets[num2].Discard(GameState.Players);
                    GameState.SecretDestroyed(secrets[num2], source);
                }

                card2.ResetCard();
                player.Hand.Cards.Add(card2);
                var unsummonEvent = new CardTransitionCcgEvent(CcgEventType.CardUnsummon, card2.InstanceId,
                    card2.ActiveData.Owner, 0, 0, false, Region.NumRegions, 0, 0)
                {
                    TemplateId = card2.TemplateId,
                    Rank = card2.Rank,
                    EffectId = EffectTraitId,
                    TraitId = TraitParentId
                };
                GameState.AddCcgEventLog(unsummonEvent);
            }
        }

        card.ResetCard();
        player.Hand.Cards.Add(card);
        var unsummonEvent2 = new CardTransitionCcgEvent(CcgEventType.CardUnsummon, card.InstanceId,
            card.ActiveData.Owner, 0, 0, false, Region.NumRegions, 0, 0)
        {
            TemplateId = card.TemplateId,
            Rank = card.Rank,
            EffectId = EffectTraitId,
            TraitId = TraitParentId
        };
        GameState.AddCcgEventLog(unsummonEvent2);
    }
}