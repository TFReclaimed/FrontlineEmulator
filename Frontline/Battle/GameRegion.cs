using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class GameRegion
{
    public CardStack[] slots;

    public RegionEnum regionLocation = RegionEnum.NumRegions;

    private readonly CCG _gameState;

    private short[] titanSlots;

    private bool slotIndependent = true;

    public GameRegion(CCG gameState)
    {
        _gameState = gameState;
    }

    public void Create(GameTemplate rules, RegionEnum region)
    {
        int num = 0;
        regionLocation = region;
        switch (region)
        {
            case RegionEnum.Player0:
                num = rules.FirstPlayerRegionSize;
                break;
            case RegionEnum.Player1:
                num = rules.OtherPlayerRegionSize;
                break;
            case RegionEnum.Control:
                num = rules.ControlRegionSize;
                slotIndependent = rules.ControlRegionSlotIndependent;
                titanSlots = rules.ControlRegionTitanSlots;
                break;
        }

        slots = new CardStack[num];
        for (int i = 0; i < num; i++)
        {
            slots[i] = new CardStack();
            slots[i].Create();
        }
    }

    public void Init(CCG game, bool independentSlots, short[] slotsForTitans)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Init(game);
        }

        slotIndependent = independentSlots;
        titanSlots = slotsForTitans;
    }

    public void InitActiveData()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].InitActiveData();
        }
    }

    public void NewTurn(sbyte playerTurn)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].NewTurn(playerTurn);
        }
    }

    public bool CanDeploy(Card card, TargetableArea area, sbyte slotIndex, sbyte pushDir)
    {
        CardTemplate template = card.GetTemplate();
        int num = slots.Length;
        int num2 = ((titanSlots != null) ? titanSlots.Length : 0);
        bool emptyAvailable = slotIndependent && HasEmpty();
        bool flag = slotIndependent && pushDir != 0;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        if (template.Type == CardType.Titan)
        {
            flag3 = true;
        }

        if (template.Type == CardType.Pilot)
        {
            flag4 = true;
        }

        if (area == TargetableArea.FriendlyPerimeter || area == TargetableArea.FriendlyRegions ||
            area == TargetableArea.FriendlyDiscard || area == TargetableArea.FriendlyHand ||
            area == TargetableArea.EnemyPerimeter || area == TargetableArea.EnemyRegions ||
            area == TargetableArea.EnemyDiscard || area == TargetableArea.EnemyHand ||
            area == TargetableArea.Frontline || area == TargetableArea.AnyRegion ||
            area == TargetableArea.BattleField || area == TargetableArea.BattleFieldNC ||
            area == TargetableArea.AnyAreas)
        {
            if (card.CanDeploy(regionLocation, area))
            {
                return true;
            }

            if (area != TargetableArea.AnyAreas)
            {
                Console.WriteLine("GameRegion.CanDeploy false - invalid region {0} {1}", area, regionLocation);
                return false;
            }
        }

        if (area == TargetableArea.UnitStack && pushDir == 0)
        {
            flag2 = true;
        }

        if (slotIndex == -1 || flag)
        {
            if (flag3 && num2 > 0)
            {
                for (int i = 0; i < num2; i++)
                {
                    CardStack target = slots[titanSlots[i]];
                    if (card.CanDeploy(target, regionLocation, emptyAvailable, flag2))
                    {
                        return true;
                    }

                    if (!flag2 && slotIndex == -1 && area == TargetableArea.AnyAreas &&
                        card.CanDeploy(target, regionLocation, emptyAvailable, true))
                    {
                        return true;
                    }
                }

                Console.WriteLine("GameRegion.CanDeploy false - No valid Titan Slots found");
            }
            else
            {
                for (int j = 0; j < num; j++)
                {
                    CardStack target = slots[j];
                    if (card.CanDeploy(target, regionLocation, emptyAvailable, flag2))
                    {
                        return true;
                    }

                    if (!flag2 && slotIndex == -1 && area == TargetableArea.AnyAreas && (flag3 || flag4) &&
                        card.CanDeploy(target, regionLocation, emptyAvailable, true))
                    {
                        return true;
                    }
                }

                Console.WriteLine("GameRegion.CanDeploy false - No valid deploy Slots found");
            }
        }

        if (slotIndex >= 0 && slotIndex < num)
        {
            if (flag3 && !IsTitanSlotIndex(slotIndex))
            {
                Console.WriteLine("GameRegion.CanDeploy false - Not a Titan Slot");
                return false;
            }

            CardStack target = slots[slotIndex];
            return card.CanDeploy(target, regionLocation, emptyAvailable, flag2);
        }

        return false;
    }

    public CardStack Deploy(Card card, sbyte slotIndex, sbyte pushDir, RegionEnum target, CardTransitionCCGEvent deployEvent)
    {
        bool flag = card.Deploy(slots[slotIndex], pushDir == 0, target, deployEvent);
        if (!flag)
        {
            PushEmpty(slotIndex, pushDir);
            flag = card.Deploy(slots[slotIndex], pushDir == 0, target, deployEvent);
        }

        if (flag)
        {
            return slots[slotIndex];
        }

        return null;
    }

    public bool CanDisembark()
    {
        return HasEmpty();
    }

    public void Disembark(int titanCardId, sbyte indexOfPlayerOwner, bool isFrontline, bool doesEject,
        BaseTraitEffect traitCause)
    {
        sbyte b = GetCardStackIdx(titanCardId, indexOfPlayerOwner);
        CardStack cardStack = slots[b];
        UnitCard unitCard = (UnitCard) cardStack.primaryCard;
        UnitCard embarkedPilot = unitCard.embarkedPilot;
        sbyte b2 = 0;
        if (embarkedPilot == null)
        {
            return;
        }

        if (!doesEject)
        {
            if (!isFrontline)
            {
                b2 = 1;
                PushEmpty(b, b2);
                cardStack = slots[b];
                if (cardStack.primaryCard != null)
                {
                    cardStack = FindEmptyCardStack(true, false);
                }
            }
            else
            {
                b = (sbyte) GetEmptyCardStackIndex(true, false);
                cardStack = slots[b];
            }

            if (cardStack == null)
            {
                return;
            }

            cardStack.primaryCard = embarkedPilot;
        }
        else
        {
            slots[b].SetEjectedCard(embarkedPilot);
            cardStack = slots[b];
        }

        CardTransitionCCGEvent cardTransitionCCGEvent = new CardTransitionCCGEvent(CCGEventType.Disembark,
            embarkedPilot.instanceId, embarkedPilot.activeData.owner, unitCard.instanceId, unitCard.activeData.owner,
            doesEject, regionLocation, b, b2);
        _gameState.AddCCGEventLog(cardTransitionCCGEvent);
        if (traitCause != null)
        {
            cardTransitionCCGEvent.effectID = traitCause.effectTraitID;
            cardTransitionCCGEvent.traitID = traitCause.traitParentID;
        }

        unitCard.DisembarkTraits();
        embarkedPilot.DisembarkTraits();
        unitCard.embarkedPilot = null;
        embarkedPilot.Disembark(cardStack, regionLocation);
    }

    public bool CanMove(CardStack stack, sbyte slotIndex, sbyte pushDir)
    {
        int num = slots.Length;
        int num2 = ((titanSlots != null) ? titanSlots.Length : 0);
        bool emptyAvailable = slotIndependent && pushDir != 0 && HasEmpty();
        Card primaryCard = stack.primaryCard;
        CardTemplate template = primaryCard.GetTemplate();
        bool flag = false;
        if (template.Type == CardType.Titan)
        {
            flag = true;
        }

        if (slotIndex >= 0)
        {
            if (slotIndex < num)
            {
                if (flag && !IsTitanSlotIndex(slotIndex))
                {
                    return false;
                }

                CardStack cardStack = slots[slotIndex];
                return stack != cardStack && primaryCard.CanMove(stack, cardStack, emptyAvailable, pushDir == 0);
            }
        }
        else if (slotIndex == -1)
        {
            if (flag && num2 > 0)
            {
                for (int i = 0; i < num2; i++)
                {
                    CardStack cardStack = slots[titanSlots[i]];
                    if (stack != cardStack && primaryCard.CanMove(stack, cardStack, emptyAvailable, pushDir == 0))
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int j = 0; j < num; j++)
                {
                    CardStack cardStack = slots[j];
                    if (stack != cardStack && primaryCard.CanMove(stack, cardStack, emptyAvailable, pushDir == 0))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void Move(Card current, sbyte slotIndex, sbyte pushDir, RegionEnum origin)
    {
        if (!current.Move(slots[slotIndex], regionLocation, origin, pushDir == 0))
        {
            PushEmpty(slotIndex, pushDir);
            current.Move(slots[slotIndex], regionLocation, origin, pushDir == 0);
        }
    }

    public void EndTurn(sbyte playerIndex)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].EndTurn(playerIndex);
        }
    }

    public bool CanAttack(CardStack stack, sbyte slotIndex)
    {
        Card primaryCard = stack.primaryCard;
        int num = slots.Length;
        if (slotIndex >= 0)
        {
            if (slotIndex < num)
            {
                CardStack target = slots[slotIndex];
                return primaryCard.CanAttack(stack, target);
            }
        }
        else if (slotIndex == -1)
        {
            for (int i = 0; i < num; i++)
            {
                CardStack target = slots[i];
                if (primaryCard.CanAttack(stack, target))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void Attack(CardStack stack, sbyte slotIndex)
    {
        Card primaryCard = stack.primaryCard;
        int num = slots.Length;
        if (slotIndex >= 0)
        {
            if (slotIndex < num)
            {
                CardStack cardStack = slots[slotIndex];
                primaryCard.Attack(stack, cardStack.primaryCard);
            }
        }
        else if (slotIndex == -1)
        {
            for (int i = 0; i < num; i++)
            {
                CardStack cardStack = slots[i];
                primaryCard.Attack(stack, cardStack.primaryCard);
            }
        }
    }

    public Card FindTraitActor(int cardId, sbyte ownerId)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Card card = slots[i].FindTraitActor(cardId, ownerId);
            if (card != null)
            {
                return card;
            }
        }

        return null;
    }

    public void FindCards(TraitTargeting info, Card source, List<CardStack> found)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].FindCards(info, source, found);
        }
    }

    public bool FindCardStack(Card card, List<CardStack> found)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].FindCardStack(card, found))
            {
                return true;
            }
        }

        return false;
    }

    public CardStack FindCard(int cardId, sbyte ownerId)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            CardStack cardStack = slots[i].FindCard(cardId, ownerId);
            if (cardStack != null)
            {
                return cardStack;
            }
        }

        return null;
    }

    public sbyte GetCardStackIdx(int cardId, sbyte ownerId)
    {
        for (sbyte b = 0; b < slots.Length; b++)
        {
            CardStack cardStack = slots[b].FindCard(cardId, ownerId);
            if (cardStack != null)
            {
                return b;
            }
        }

        return -1;
    }

    public Card RemoveCard(int cardId, sbyte ownerId)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Card card = slots[i].RemoveCard(cardId, ownerId);
            if (card != null)
            {
                return card;
            }
        }

        return null;
    }

    public bool CheckDiscards(Player[] players)
    {
        bool result = false;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].CheckDiscard(players))
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

        int num = slots.Length;
        int num2 = desiredSlotIndex;
        int i;
        for (i = desiredSlotIndex + pushDir; (pushDir <= 0) ? (i >= 0) : (i < num); i += pushDir)
        {
            if (slots[i].primaryCard == null)
            {
                num2 = i;
                break;
            }
        }

        if (num2 != desiredSlotIndex)
        {
            CardStack cardStack = slots[num2];
            i = num2;
            while ((pushDir <= 0) ? (i < desiredSlotIndex) : (i > desiredSlotIndex))
            {
                slots[i] = slots[i - pushDir];
                i -= pushDir;
            }

            slots[desiredSlotIndex] = cardStack;
            return;
        }

        i = desiredSlotIndex - pushDir;
        while ((pushDir <= 0) ? (i < num) : (i >= 0))
        {
            if (slots[i].primaryCard == null)
            {
                num2 = i;
                break;
            }

            i -= pushDir;
        }

        if (num2 != desiredSlotIndex)
        {
            CardStack cardStack = slots[num2];
            for (i = num2; (pushDir <= 0) ? (i > desiredSlotIndex) : (i < desiredSlotIndex); i += pushDir)
            {
                slots[i] = slots[i + pushDir];
            }

            slots[desiredSlotIndex] = cardStack;
        }
    }

    public bool HasEmpty()
    {
        bool result = false;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].primaryCard == null)
            {
                result = true;
            }
        }

        return result;
    }

    private bool IsSlotEmpty(int index, bool titanOnly)
    {
        if (slots[index].primaryCard == null)
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

    public CardStack FindEmptyCardStack(bool titanOnly, bool reverseSearch)
    {
        int emptyCardStackIndex = GetEmptyCardStackIndex(titanOnly, reverseSearch);
        if (emptyCardStackIndex == -1)
        {
            return null;
        }

        return slots[emptyCardStackIndex];
    }

    public int GetEmptyCardStackIndex(bool titanOnly, bool reverseSearch)
    {
        if (!reverseSearch)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (IsSlotEmpty(i, titanOnly))
                {
                    return i;
                }
            }
        }
        else
        {
            for (int num = slots.Length - 1; num >= 0; num--)
            {
                if (IsSlotEmpty(num, titanOnly))
                {
                    return num;
                }
            }
        }

        return -1;
    }

    private bool IsTitanSlotIndex(int slotIndex)
    {
        int num = ((titanSlots != null) ? titanSlots.Length : 0);
        if (num == 0)
        {
            return true;
        }

        for (int i = 0; i < num; i++)
        {
            if (titanSlots[i] == slotIndex)
            {
                return true;
            }
        }

        return false;
    }
}