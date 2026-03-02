using Frontline.Battle.CcgEvents;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class GameRegion
{
    public CardStack[] Slots { get; set; }

    public Region RegionLocation { get; set; }

    private readonly CcgGameState _gameState;

    private readonly short[] _titanSlots;

    private readonly bool _slotIndependent = true;

    public GameRegion(CcgGameState gameState, Region region)
    {
        _gameState = gameState;
        _titanSlots = [];

        var size = 0;
        RegionLocation = region;
        switch (region)
        {
            case Region.Player0:
                size = _gameState.GetGameTemplate().FirstPlayerRegionSize;
                break;
            case Region.Player1:
                size = _gameState.GetGameTemplate().OtherPlayerRegionSize;
                break;
            case Region.Control:
                size = _gameState.GetGameTemplate().ControlRegionSize;
                _slotIndependent = _gameState.GetGameTemplate().ControlRegionSlotIndependent;
                _titanSlots = _gameState.GetGameTemplate().ControlRegionTitanSlots;
                break;
        }

        Slots = new CardStack[size];
        for (var i = 0; i < size; i++)
        {
            Slots[i] = new CardStack(_gameState);
        }
    }

    public void NewTurn(sbyte playerTurn)
    {
        foreach (var slot in Slots)
        {
            slot.NewTurn(playerTurn);
        }
    }

    public bool CanDeploy(Card card, TargetableArea area, sbyte slotIndex, sbyte pushDir)
    {
        var template = card.GetTemplate();
        var slotCount = Slots.Length;
        var titanSlotCount = _titanSlots.Length;
        var emptyAvailable = _slotIndependent && HasEmpty();
        var flag = _slotIndependent && pushDir != 0;
        var flag2 = false;
        var isTitan = template.Type == CardType.Titan;
        var isPilot = template.Type == CardType.Pilot;

        if (area == TargetableArea.FriendlyPerimeter || area == TargetableArea.FriendlyRegions ||
            area == TargetableArea.FriendlyDiscard || area == TargetableArea.FriendlyHand ||
            area == TargetableArea.EnemyPerimeter || area == TargetableArea.EnemyRegions ||
            area == TargetableArea.EnemyDiscard || area == TargetableArea.EnemyHand ||
            area == TargetableArea.Frontline || area == TargetableArea.AnyRegion ||
            area == TargetableArea.BattleField || area == TargetableArea.BattleFieldNc ||
            area == TargetableArea.AnyAreas)
        {
            if (card.CanDeploy(RegionLocation, area))
            {
                return true;
            }

            if (area != TargetableArea.AnyAreas)
            {
                _gameState.Logger.Debug("GameRegion.CanDeploy false - invalid region {0} {1}", area, RegionLocation);
                return false;
            }
        }

        if (area == TargetableArea.UnitStack && pushDir == 0)
        {
            flag2 = true;
        }

        if (slotIndex == -1 || flag)
        {
            if (isTitan && titanSlotCount > 0)
            {
                for (var i = 0; i < titanSlotCount; i++)
                {
                    var target = Slots[_titanSlots[i]];
                    if (card.CanDeploy(target, RegionLocation, emptyAvailable, flag2))
                    {
                        return true;
                    }

                    if (!flag2 && slotIndex == -1 && area == TargetableArea.AnyAreas &&
                        card.CanDeploy(target, RegionLocation, emptyAvailable, true))
                    {
                        return true;
                    }
                }

                _gameState.Logger.Debug("GameRegion.CanDeploy false - No valid Titan Slots found");
            }
            else
            {
                for (var j = 0; j < slotCount; j++)
                {
                    var target = Slots[j];
                    if (card.CanDeploy(target, RegionLocation, emptyAvailable, flag2))
                    {
                        return true;
                    }

                    if (!flag2 && slotIndex == -1 && area == TargetableArea.AnyAreas && (isTitan || isPilot) &&
                        card.CanDeploy(target, RegionLocation, emptyAvailable, true))
                    {
                        return true;
                    }
                }

                _gameState.Logger.Debug("GameRegion.CanDeploy false - No valid deploy Slots found");
            }
        }

        if (slotIndex >= 0 && slotIndex < slotCount)
        {
            if (isTitan && !IsTitanSlotIndex(slotIndex))
            {
                _gameState.Logger.Debug("GameRegion.CanDeploy false - Not a Titan Slot");
                return false;
            }

            var target = Slots[slotIndex];
            return card.CanDeploy(target, RegionLocation, emptyAvailable, flag2);
        }

        return false;
    }

    public CardStack? Deploy(Card card, sbyte slotIndex, sbyte pushDir, Region target,
        CardTransitionCcgEvent deployEvent)
    {
        var didDeploy = card.Deploy(Slots[slotIndex], pushDir == 0, target, deployEvent);
        if (!didDeploy)
        {
            PushEmpty(slotIndex, pushDir);
            didDeploy = card.Deploy(Slots[slotIndex], pushDir == 0, target, deployEvent);
        }

        if (didDeploy)
        {
            return Slots[slotIndex];
        }

        return null;
    }

    public bool CanDisembark()
    {
        return HasEmpty();
    }

    public void Disembark(int titanCardId, sbyte indexOfPlayerOwner, bool isFrontline, bool doesEject,
        BaseTraitEffect? traitCause)
    {
        var stackIndex = GetCardStackIdx(titanCardId, indexOfPlayerOwner);
        var cardStack = Slots[stackIndex];
        var unitCard = (UnitCard) cardStack.PrimaryCard!;
        var embarkedPilot = unitCard.EmbarkedPilot;
        sbyte pushDir = 0;
        if (embarkedPilot == null)
        {
            return;
        }

        if (!doesEject)
        {
            if (!isFrontline)
            {
                pushDir = 1;
                PushEmpty(stackIndex, pushDir);
                cardStack = Slots[stackIndex];
                if (cardStack.PrimaryCard != null)
                {
                    cardStack = FindEmptyCardStack(true, false);
                }
            }
            else
            {
                stackIndex = (sbyte) GetEmptyCardStackIndex(true, false);
                cardStack = Slots[stackIndex];
            }

            if (cardStack == null)
            {
                return;
            }

            cardStack.PrimaryCard = embarkedPilot;
        }
        else
        {
            Slots[stackIndex].SetEjectedCard(embarkedPilot);
            cardStack = Slots[stackIndex];
        }

        var disembarkEvent = new CardTransitionCcgEvent(CcgEventType.Disembark,
            embarkedPilot.InstanceId, embarkedPilot.ActiveData.Owner, unitCard.InstanceId, unitCard.ActiveData.Owner,
            doesEject, RegionLocation, stackIndex, pushDir);
        _gameState.AddCcgEventLog(disembarkEvent);
        if (traitCause != null)
        {
            disembarkEvent.EffectId = traitCause.EffectTraitId;
            disembarkEvent.TraitId = traitCause.TraitParentId;
        }

        unitCard.DisembarkTraits();
        embarkedPilot.DisembarkTraits();
        unitCard.EmbarkedPilot = null;
        embarkedPilot.Disembark(cardStack, RegionLocation);
    }

    public bool CanMove(CardStack stack, sbyte slotIndex, sbyte pushDir)
    {
        var slotCount = Slots.Length;
        var titanSlotCount = _titanSlots.Length;
        var emptyAvailable = _slotIndependent && pushDir != 0 && HasEmpty();
        var primaryCard = stack.PrimaryCard!;
        var template = primaryCard.GetTemplate();
        var isTitan = template.Type == CardType.Titan;

        if (slotIndex >= 0)
        {
            if (slotIndex < slotCount)
            {
                if (isTitan && !IsTitanSlotIndex(slotIndex))
                {
                    return false;
                }

                var cardStack = Slots[slotIndex];
                return stack != cardStack && primaryCard.CanMove(stack, cardStack, emptyAvailable, pushDir == 0);
            }
        }
        else if (slotIndex == -1)
        {
            if (isTitan && titanSlotCount > 0)
            {
                for (var i = 0; i < titanSlotCount; i++)
                {
                    var cardStack = Slots[_titanSlots[i]];
                    if (stack != cardStack && primaryCard.CanMove(stack, cardStack, emptyAvailable, pushDir == 0))
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (var j = 0; j < slotCount; j++)
                {
                    var cardStack = Slots[j];
                    if (stack != cardStack && primaryCard.CanMove(stack, cardStack, emptyAvailable, pushDir == 0))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void Move(Card current, sbyte slotIndex, sbyte pushDir, Region origin)
    {
        if (!current.Move(Slots[slotIndex], RegionLocation, origin, pushDir == 0))
        {
            PushEmpty(slotIndex, pushDir);
            current.Move(Slots[slotIndex], RegionLocation, origin, pushDir == 0);
        }
    }

    public void EndTurn(sbyte playerIndex)
    {
        foreach (var slot in Slots)
        {
            slot.EndTurn(playerIndex);
        }
    }

    public Card? FindTraitActor(int cardId, sbyte ownerId)
    {
        foreach (var slot in Slots)
        {
            var card = slot.FindTraitActor(cardId, ownerId);
            if (card != null)
            {
                return card;
            }
        }

        return null;
    }

    public void FindCards(TraitTargeting info, Card source, List<CardStack> found)
    {
        foreach (var slot in Slots)
        {
            slot.FindCards(info, source, found);
        }
    }

    public bool FindCardStack(Card card, List<CardStack> found)
    {
        foreach (var slot in Slots)
        {
            if (slot.FindCardStack(card, found))
            {
                return true;
            }
        }

        return false;
    }

    public CardStack? FindCard(int cardId, sbyte ownerId)
    {
        foreach (var slot in Slots)
        {
            var cardStack = slot.FindCard(cardId, ownerId);
            if (cardStack != null)
            {
                return cardStack;
            }
        }

        return null;
    }

    public sbyte GetCardStackIdx(int cardId, sbyte ownerId)
    {
        for (sbyte b = 0; b < Slots.Length; b++)
        {
            var cardStack = Slots[b].FindCard(cardId, ownerId);
            if (cardStack != null)
            {
                return b;
            }
        }

        return -1;
    }

    public Card? RemoveCard(int cardId, sbyte ownerId)
    {
        foreach (var slot in Slots)
        {
            var card = slot.RemoveCard(cardId, ownerId);
            if (card != null)
            {
                return card;
            }
        }

        return null;
    }

    public bool CheckDiscards(Player[] players)
    {
        var result = false;
        foreach (var slot in Slots)
        {
            if (slot.CheckDiscard(players))
            {
                result = true;
            }
        }

        return result;
    }

    private void PushEmpty(sbyte desiredSlotIndex, sbyte pushDir)
    {
        if (pushDir == 0)
        {
            return;
        }

        var slotCount = Slots.Length;
        int slotIndex = desiredSlotIndex;
        int i;
        for (i = desiredSlotIndex + pushDir; pushDir <= 0 ? i >= 0 : i < slotCount; i += pushDir)
        {
            if (Slots[i].PrimaryCard == null)
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex != desiredSlotIndex)
        {
            var cardStack = Slots[slotIndex];
            i = slotIndex;
            while (pushDir <= 0 ? i < desiredSlotIndex : i > desiredSlotIndex)
            {
                Slots[i] = Slots[i - pushDir];
                i -= pushDir;
            }

            Slots[desiredSlotIndex] = cardStack;
            return;
        }

        i = desiredSlotIndex - pushDir;
        while (pushDir <= 0 ? i < slotCount : i >= 0)
        {
            if (Slots[i].PrimaryCard == null)
            {
                slotIndex = i;
                break;
            }

            i -= pushDir;
        }

        if (slotIndex != desiredSlotIndex)
        {
            var cardStack = Slots[slotIndex];
            for (i = slotIndex; pushDir <= 0 ? i > desiredSlotIndex : i < desiredSlotIndex; i += pushDir)
            {
                Slots[i] = Slots[i + pushDir];
            }

            Slots[desiredSlotIndex] = cardStack;
        }
    }

    public bool HasEmpty()
    {
        var foundEmpty = false;
        foreach (var slot in Slots)
        {
            if (slot.PrimaryCard == null)
            {
                foundEmpty = true;
            }
        }

        return foundEmpty;
    }

    private bool IsSlotEmpty(int index, bool titanOnly)
    {
        if (Slots[index].PrimaryCard == null)
        {
            if (!titanOnly)
            {
                return true;
            }

            if (IsTitanSlotIndex(index))
            {
                return true;
            }
        }

        return false;
    }

    public CardStack? FindEmptyCardStack(bool titanOnly, bool reverseSearch)
    {
        var emptyCardStackIndex = GetEmptyCardStackIndex(titanOnly, reverseSearch);
        if (emptyCardStackIndex == -1)
        {
            return null;
        }

        return Slots[emptyCardStackIndex];
    }

    public int GetEmptyCardStackIndex(bool titanOnly, bool reverseSearch)
    {
        if (!reverseSearch)
        {
            for (var i = 0; i < Slots.Length; i++)
            {
                if (IsSlotEmpty(i, titanOnly))
                {
                    return i;
                }
            }
        }
        else
        {
            for (var i = Slots.Length - 1; i >= 0; i--)
            {
                if (IsSlotEmpty(i, titanOnly))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private bool IsTitanSlotIndex(int slotIndex)
    {
        var titanSlotCount = _titanSlots.Length;
        if (titanSlotCount == 0)
        {
            return true;
        }

        for (var i = 0; i < titanSlotCount; i++)
        {
            if (_titanSlots[i] == slotIndex)
            {
                return true;
            }
        }

        return false;
    }
}