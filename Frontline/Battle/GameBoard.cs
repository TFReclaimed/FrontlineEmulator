using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class GameBoard
{
    public GameRegion[] Regions { get; set; }

    private Region _sourceRegion = Region.NumRegions;

    private readonly CcgGameState _gameState;

    private const int NumRegions = (int) Region.NumRegions;

    public GameBoard(CcgGameState gameState)
    {
        _gameState = gameState;

        Regions = new GameRegion[NumRegions];
        for (var i = 0; i < NumRegions; i++)
        {
            Regions[i] = new GameRegion(_gameState, (Region) i);
        }
    }

    public void NewTurn(sbyte playerTurn)
    {
        foreach (var region in Regions)
        {
            region.NewTurn(playerTurn);
        }
    }

    public bool CanDeploy(Card card, TargetableArea area, Region target, sbyte slotIndex, sbyte pushDir)
    {
        var region = (Region) card.ActiveData.Owner;
        switch (area)
        {
            case TargetableArea.FriendlyPerimeter:
                if (target != region)
                {
                    _gameState.Logger.Debug("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                    return false;
                }

                return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
            case TargetableArea.EnemyPerimeter:
                if (target == region || target == Region.Control)
                {
                    _gameState.Logger.Debug("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                    return false;
                }

                return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
            case TargetableArea.FriendlyRegions:
                if (target == region || target == Region.Control)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                _gameState.Logger.Debug("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                return false;
            case TargetableArea.EnemyRegions:
                if (target != region)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                _gameState.Logger.Debug("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                return false;
            case TargetableArea.Frontline:
                if (target == Region.Control)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                _gameState.Logger.Debug("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                return false;
            case TargetableArea.CurrentRegion:
                if (target != Region.NumRegions)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                break;
            case TargetableArea.UnitStack:
                if (target != Region.NumRegions)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                break;
        }

        foreach (var loopRegion in Regions)
        {
            if (loopRegion.CanDeploy(card, area, slotIndex, pushDir))
            {
                return true;
            }
        }

        return false;
    }

    public CardStack? Deploy(Card card, Region target, sbyte slotIndex, sbyte pushDir,
        CardTransitionCcgEvent deployEvent)
    {
        var cardStack = Regions[(uint) target].Deploy(card, slotIndex, pushDir, target, deployEvent);
        if (cardStack == null)
        {
            return cardStack;
        }

        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.CardDeployed(card);
            }
        }

        return cardStack;
    }

    public bool CanMove(int cardId, sbyte ownerId, Region target, sbyte slotIndex, sbyte pushDir)
    {
        var cardStack = FindCard(cardId, ownerId);
        if (cardStack == null)
        {
            _gameState.Logger.Debug("GameBoard.CanMove false - cannot find card " + cardId);
            return false;
        }

        var primaryCard = cardStack.PrimaryCard!;
        if (primaryCard.HasActed(EntityActionType.Move))
        {
            _gameState.Logger.Debug("GameBoard.CanMove false - card Move flag is set");
            return false;
        }

        if (target == Region.NumRegions)
        {
            for (var i = 0; i < NumRegions; i++)
            {
                if (((int) _sourceRegion != i || _sourceRegion == Region.Control || pushDir == 0) &&
                    primaryCard.CanMove((Region) i) && Regions[i].CanMove(cardStack, slotIndex, pushDir))
                {
                    return true;
                }
            }

            return false;
        }

        if (_sourceRegion == target && pushDir != 0 && _sourceRegion != Region.Control)
        {
            _gameState.Logger.Debug("GameBoard.CanMove false - trying to move in the same region");
            return false;
        }

        return primaryCard.CanMove(target) && Regions[(uint) target].CanMove(cardStack, slotIndex, pushDir);
    }

    public bool Move(int cardId, sbyte ownerId, Region target, sbyte slotIndex, sbyte pushDir)
    {
        var traitActorRegion = _gameState.GetTraitActorRegion(ownerId, cardId);
        var card = RemoveCard(cardId, ownerId);
        if (card != null)
        {
            Regions[(uint) target].Move(card, slotIndex, pushDir, traitActorRegion);
            return true;
        }

        _gameState.Logger.Warning("MOVE FAILED - GameBoard.Move could not find Card ID-" + cardId);
        return false;
    }

    public bool CanDisembark(int cardId, sbyte ownerId)
    {
        var cardStack = FindCard(cardId, ownerId);
        if (cardStack?.PrimaryCard == null || !cardStack.PrimaryCard.HasPilot())
        {
            return false;
        }

        var unitCard = (UnitCard) cardStack.PrimaryCard;
        if (unitCard.HasActed(EntityActionType.AnyActionMask) || unitCard.GetTemplate().Type != CardType.Titan)
        {
            return false;
        }

        var embarkedPilot = unitCard.EmbarkedPilot!;
        if (!embarkedPilot.CanDisembark(cardStack) || embarkedPilot.HasActed(EntityActionType.Move))
        {
            return false;
        }

        return Regions[(uint) _sourceRegion].CanDisembark();
    }

    public bool Disembark(int cardId, sbyte ownerId, bool eject, BaseTraitEffect? traitCause)
    {
        if (!Regions[(uint) _sourceRegion].HasEmpty() && !eject)
        {
            return false;
        }

        Regions[(uint) _sourceRegion]
            .Disembark(cardId, ownerId, _sourceRegion == Region.Control, eject, traitCause);
        return true;
    }

    public void EndTurn(sbyte playerIndex)
    {
        foreach (var region in Regions)
        {
            region.EndTurn(playerIndex);
        }
    }

    public bool CanAttack(sbyte playerIndex, int cardId, sbyte targetOwner, int targetId, Player[] players)
    {
        var cardStack = FindCard(cardId, playerIndex);
        if (cardStack == null)
        {
            _gameState.Logger.Debug("GameBoard.CanAttack false - cannot find card - " + cardId);
            return false;
        }

        var primaryCard = cardStack.PrimaryCard!;
        if (primaryCard.HasActed(EntityActionType.Attack))
        {
            _gameState.Logger.Debug("GameBoard.CanAttack false - card attack flad set");
            return false;
        }

        var player = players[targetOwner];
        if (player.Resources.Health > 0 && targetId == player.Commander.PrimaryCard!.InstanceId)
        {
            if (_sourceRegion == Region.Control || primaryCard.HasStatusEffect(ApplyStatusTraitStatusType.Operative))
            {
                return primaryCard.CanAttack(cardStack, player.Commander);
            }

            return false;
        }

        var cardStack2 = FindCard(targetId, targetOwner);
        if (cardStack2 != null)
        {
            return primaryCard.CanAttack(cardStack, cardStack2);
        }

        _gameState.Logger.Debug("GameBoard.CanAttack false - cannot find target card - " + targetId);
        return false;
    }

    public bool Attack(sbyte playerIndex, int cardId, sbyte targetOwner, int targetId, Player[] players)
    {
        var cardStack = FindCard(cardId, playerIndex);
        if (cardStack == null)
        {
            _gameState.Logger.Warning("ATTACK FAILED - GameBoard.Attack count not find CardStack for ID" + cardId);
            return false;
        }

        var primaryCard = cardStack.PrimaryCard!;
        var player = players[targetOwner];
        var cardStack2 = targetId != player.Commander.PrimaryCard!.InstanceId
            ? FindCard(targetId, targetOwner)
            : player.Commander;
        if (cardStack2 == null)
        {
            _gameState.Logger.Warning("ATTACK FAILED - GameBoard.Attack count not find Target CardStack for ID" + targetId);
            return false;
        }

        primaryCard.Attack(cardStack, cardStack2.PrimaryCard);
        return true;
    }

    public bool ActivateTrait(sbyte playerIndex, int cardId, sbyte targetOwner, int targetId, TargetableArea area,
        Region region, Player[] players)
    {
        var cardStack = FindCard(cardId, playerIndex);
        if (cardStack == null)
        {
            return false;
        }

        var entityCard = (EntityCard) cardStack.PrimaryCard!;
        if (area == TargetableArea.UnitStack)
        {
            var player = players[targetOwner];
            if (targetId == player.Commander.PrimaryCard!.InstanceId)
            {
                entityCard.ActivateTrait(player.Commander, region, _gameState);
                return true;
            }

            var cardStack2 = FindCard(targetId, targetOwner);
            if (cardStack2 == null)
            {
                return false;
            }

            entityCard.ActivateTrait(cardStack2, region, _gameState);
        }
        else
        {
            entityCard.ActivateTrait(cardStack, region, _gameState);
        }

        return true;
    }

    public Card? FindTraitActor(int cardId, sbyte ownerId)
    {
        _sourceRegion = Region.NumRegions;
        for (var i = 0; i < NumRegions; i++)
        {
            var card = Regions[i].FindTraitActor(cardId, ownerId);
            if (card != null)
            {
                _sourceRegion = (Region) i;
                return card;
            }
        }

        return null;
    }

    public Region GetTraitActorRegion(int cardId, sbyte ownerId)
    {
        _sourceRegion = Region.NumRegions;
        for (var i = 0; i < NumRegions; i++)
        {
            var card = Regions[i].FindTraitActor(cardId, ownerId);
            if (card != null)
            {
                _sourceRegion = (Region) i;
                return _sourceRegion;
            }
        }

        return _sourceRegion;
    }

    public void FindCards(TraitTargeting info, Region region, Card source, List<CardStack> found)
    {
        if (info.Area == TargetableArea.CurrentRegion && region != Region.NumRegions)
        {
            Regions[(uint) region].FindCards(info, source, found);
            return;
        }

        for (var i = 0; i < NumRegions; i++)
        {
            if (info.CheckRegion((Region) i, source.ActiveData.Owner))
            {
                Regions[i].FindCards(info, source, found);
            }
        }
    }

    public void FindCardStack(Card card, List<CardStack> found)
    {
        for (var i = 0; i < NumRegions && !Regions[i].FindCardStack(card, found); i++)
        {
        }
    }

    public void CheckDiscards(Player[] players)
    {
        bool flag;
        do
        {
            flag = false;
            foreach (var region in Regions)
            {
                if (region.CheckDiscards(players))
                {
                    flag = true;
                }
            }
        } while (flag);
    }

    public void CardMoved(Card card, CardStack target, Region destination, Region origin)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.CardMoved(card, target, destination, origin);
            }
        }
    }

    public void CardAttacked(Card attacker, Card target)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.CardAttacked(attacker, target);
            }
        }
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.CardCounterAttacked(attacker, target);
            }
        }
    }

    public void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.CardGainedStatus(theCard, source, statusType);
            }
        }
    }

    public void CardDamaged(Card damagedCard, Card source)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.CardDamaged(damagedCard, source);
            }
        }
    }

    public void CardDied(Card deadCard, Card source)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.CardDied(deadCard, source);
            }
        }
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.CardDrawn(drawnCard, regularDraw, isNewTurn);
            }
        }
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.CardDiscardEffect(playerIndex, numberOfCards);
            }
        }
    }

    public void SecretTriggered(Card secret, Card? source)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.SecretTriggered(secret, source);
            }
        }
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        foreach (var region in Regions)
        {
            foreach (var slot in region.Slots)
            {
                slot.SecretDestroyed(secret, source);
            }
        }
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack? target, Region region)
    {
        foreach (var loopRegion in Regions)
        {
            foreach (var slot in loopRegion.Slots)
            {
                slot.TraitEffectActivating(effect, source, target, region);
            }
        }
    }

    private CardStack? FindCard(int cardId, sbyte ownerId)
    {
        _sourceRegion = Region.NumRegions;
        for (var i = 0; i < NumRegions; i++)
        {
            var cardStack = Regions[i].FindCard(cardId, ownerId);
            if (cardStack != null)
            {
                _sourceRegion = (Region) i;
                return cardStack;
            }
        }

        return null;
    }

    private Card? RemoveCard(int cardId, sbyte ownerId)
    {
        foreach (var region in Regions)
        {
            var card = region.RemoveCard(cardId, ownerId);
            if (card != null)
            {
                return card;
            }
        }

        return null;
    }
}