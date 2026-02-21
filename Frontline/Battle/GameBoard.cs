using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class GameBoard
{
    public GameRegion[] Regions { get; set; }

    private RegionEnum sourceRegion = RegionEnum.NumRegions;

    private readonly CCG _gameState;

    public GameBoard(CCG gameState)
    {
        _gameState = gameState;
    }

    public void Create(GameTemplate rules)
    {
        int num = 3;
        Regions = new GameRegion[num];
        for (int i = 0; i < num; i++)
        {
            Regions[i] = new GameRegion(_gameState);
            Regions[i].Create(rules, (RegionEnum) i);
        }
    }

    public void Init(GameTemplate rules, CCG game)
    {
        for (int i = 0; i < Regions.Length; i++)
        {
            bool independentSlots = true;
            short[] slotsForTitans = null;
            if (i == 2)
            {
                independentSlots = rules.ControlRegionSlotIndependent;
                slotsForTitans = rules.ControlRegionTitanSlots;
            }

            Regions[i].Init(game, independentSlots, slotsForTitans);
        }
    }

    public void InitActiveData()
    {
        for (int i = 0; i < Regions.Length; i++)
        {
            Regions[i].InitActiveData();
        }
    }

    public void NewTurn(sbyte playerTurn)
    {
        for (int i = 0; i < Regions.Length; i++)
        {
            Regions[i].NewTurn(playerTurn);
        }
    }

    public bool CanDeploy(Card card, TargetableArea area, RegionEnum target, sbyte slotIndex, sbyte pushDir)
    {
        RegionEnum regionEnum = (RegionEnum) card.ActiveData.Owner;
        switch (area)
        {
            case TargetableArea.FriendlyPerimeter:
                if (target != regionEnum)
                {
                    Console.WriteLine("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                    return false;
                }

                return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
            case TargetableArea.EnemyPerimeter:
                if (target == regionEnum || target == RegionEnum.Control)
                {
                    Console.WriteLine("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                    return false;
                }

                return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
            case TargetableArea.FriendlyRegions:
                if (target == regionEnum || target == RegionEnum.Control)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                Console.WriteLine("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                return false;
            case TargetableArea.EnemyRegions:
                if (target != regionEnum)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                Console.WriteLine("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                return false;
            case TargetableArea.Frontline:
                if (target == RegionEnum.Control)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                Console.WriteLine("GameBoard.CanDeploy false - invalid region {0} {1}", area, target);
                return false;
            case TargetableArea.CurrentRegion:
                if (target != RegionEnum.NumRegions)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                break;
            case TargetableArea.UnitStack:
                if (target != RegionEnum.NumRegions)
                {
                    return Regions[(uint) target].CanDeploy(card, area, slotIndex, pushDir);
                }

                break;
        }

        for (int i = 0; i < 3; i++)
        {
            if (Regions[i].CanDeploy(card, area, slotIndex, pushDir))
            {
                return true;
            }
        }

        return false;
    }

    public CardStack Deploy(Card card, RegionEnum target, sbyte slotIndex, sbyte pushDir,
        CardTransitionCCGEvent deployEvent)
    {
        CardStack cardStack = Regions[(uint) target].Deploy(card, slotIndex, pushDir, target, deployEvent);
        if (cardStack != null)
        {
            for (int i = 0; i < Regions.Length; i++)
            {
                for (int j = 0; j < Regions[i].Slots.Length; j++)
                {
                    Regions[i].Slots[j].CardDeployed(card);
                }
            }
        }

        return cardStack;
    }

    public bool CanMove(int cardId, sbyte ownerId, RegionEnum target, sbyte slotIndex, sbyte pushDir,
        GameTemplate gameRules)
    {
        CardStack cardStack = FindCard(cardId, ownerId);
        if (cardStack == null)
        {
            Console.WriteLine("GameBoard.CanMove false - cannot find card " + cardId);
            return false;
        }

        Card primaryCard = cardStack.PrimaryCard;
        if (primaryCard.HasActed(4))
        {
            Console.WriteLine("GameBoard.CanMove false - card Move flag is set");
            return false;
        }

        if (target == RegionEnum.NumRegions)
        {
            for (int i = 0; i < 3; i++)
            {
                if (((int) sourceRegion != i || sourceRegion == RegionEnum.Control || pushDir == 0) &&
                    primaryCard.CanMove((RegionEnum) i) && Regions[i].CanMove(cardStack, slotIndex, pushDir))
                {
                    return true;
                }
            }

            return false;
        }

        if (sourceRegion == target && pushDir != 0 && sourceRegion != RegionEnum.Control && pushDir != 0)
        {
            Console.WriteLine("GameBoard.CanMove false - trying to move in the same region");
            return false;
        }

        return primaryCard.CanMove(target) && Regions[(uint) target].CanMove(cardStack, slotIndex, pushDir);
    }

    public bool Move(int cardId, sbyte ownerId, RegionEnum target, sbyte slotIndex, sbyte pushDir)
    {
        RegionEnum traitActorRegion = _gameState.GetTraitActorRegion(ownerId, cardId);
        Card card = RemoveCard(cardId, ownerId);
        if (card != null)
        {
            Regions[(uint) target].Move(card, slotIndex, pushDir, traitActorRegion);
            return true;
        }

        Console.WriteLine("MOVE FAILED - GameBoard.Move could not find Card ID-" + cardId);
        return false;
    }

    public bool CanDisembark(int cardId, sbyte ownerId)
    {
        CardStack cardStack = FindCard(cardId, ownerId);
        if (cardStack == null || cardStack.PrimaryCard == null || !cardStack.PrimaryCard.HasPilot())
        {
            return false;
        }

        UnitCard unitCard = (UnitCard) cardStack.PrimaryCard;
        if (unitCard.HasActed(15) || unitCard.GetTemplate().Type != CardType.Titan)
        {
            return false;
        }

        UnitCard embarkedPilot = unitCard.EmbarkedPilot;
        if (!embarkedPilot.CanDisembark(cardStack) || embarkedPilot.HasActed(4))
        {
            return false;
        }

        return Regions[(uint) sourceRegion].CanDisembark();
    }

    public bool Disembark(int cardId, sbyte ownerId, bool eject, BaseTraitEffect traitCause)
    {
        if (Regions[(uint) sourceRegion].HasEmpty() || eject)
        {
            Regions[(uint) sourceRegion]
                .Disembark(cardId, ownerId, sourceRegion == RegionEnum.Control, eject, traitCause);
            return true;
        }

        return false;
    }

    public void EndTurn(sbyte playerIndex)
    {
        for (int i = 0; i < 3; i++)
        {
            Regions[i].EndTurn(playerIndex);
        }
    }

    public bool CanAttack(sbyte playerIndex, int cardId, sbyte targetOwner, int targetId, Player[] players)
    {
        CardStack cardStack = FindCard(cardId, playerIndex);
        if (cardStack == null)
        {
            Console.WriteLine("GameBoard.CanAttack false - cannot find card - " + cardId);
            return false;
        }

        Card primaryCard = cardStack.PrimaryCard;
        if (primaryCard.HasActed(2))
        {
            Console.WriteLine("GameBoard.CanAttack false - card attack flad set");
            return false;
        }

        Player player = players[targetOwner];
        if (player.Resources.Health > 0 && targetId == player.Commander.PrimaryCard.InstanceId)
        {
            if (sourceRegion == RegionEnum.Control || primaryCard.HasStatusEffect(8))
            {
                return primaryCard.CanAttack(cardStack, player.Commander);
            }

            return false;
        }

        CardStack cardStack2 = FindCard(targetId, targetOwner);
        if (cardStack2 != null)
        {
            return primaryCard.CanAttack(cardStack, cardStack2);
        }

        Console.WriteLine("GameBoard.CanAttack false - cannot find target card - " + targetId);
        return false;
    }

    public bool CanAttack(sbyte playerIndex, int cardId)
    {
        CardStack cardStack = FindCard(cardId, playerIndex);
        if (cardStack == null)
        {
            return false;
        }

        Card primaryCard = cardStack.PrimaryCard;
        if (primaryCard.HasActed(2))
        {
            return false;
        }

        for (int i = 0; i < 3; i++)
        {
            if (primaryCard.CanAttack((RegionEnum) i) || Regions[i].CanAttack(cardStack, -1))
            {
                return true;
            }
        }

        return false;
    }

    public bool Attack(sbyte playerIndex, int cardId, sbyte targetOwner, int targetId, Player[] players)
    {
        CardStack cardStack = FindCard(cardId, playerIndex);
        if (cardStack == null)
        {
            Console.WriteLine("ATTACK FAILED - GameBoard.Attack count not find CardStack for ID" + cardId);
            return false;
        }

        Card primaryCard = cardStack.PrimaryCard;
        CardStack cardStack2 = null;
        Player player = players[targetOwner];
        cardStack2 = ((targetId != player.Commander.PrimaryCard.InstanceId)
            ? FindCard(targetId, targetOwner)
            : player.Commander);
        if (cardStack2 == null)
        {
            Console.WriteLine("ATTACK FAILED - GameBoard.Attack count not find Target CardStack for ID" + targetId);
            return false;
        }

        primaryCard.Attack(cardStack, cardStack2.PrimaryCard);
        return true;
    }

    public bool ActivateTrait(sbyte playerIndex, int cardId, sbyte targetOwner, int targetId, TargetableArea area,
        RegionEnum region, Player[] players)
    {
        CardStack cardStack = FindCard(cardId, playerIndex);
        if (cardStack == null)
        {
            return false;
        }

        EntityCard entityCard = (EntityCard) cardStack.PrimaryCard;
        if (area == TargetableArea.UnitStack)
        {
            Player player = players[targetOwner];
            if (targetId == player.Commander.PrimaryCard.InstanceId)
            {
                entityCard.ActivateTrait(player.Commander, region, _gameState);
                return true;
            }

            CardStack cardStack2 = FindCard(targetId, targetOwner);
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

    public Card FindTraitActor(int cardId, sbyte ownerId)
    {
        sourceRegion = RegionEnum.NumRegions;
        for (int i = 0; i < 3; i++)
        {
            Card card = Regions[i].FindTraitActor(cardId, ownerId);
            if (card != null)
            {
                sourceRegion = (RegionEnum) i;
                return card;
            }
        }

        return null;
    }

    public RegionEnum GetTraitActorRegion(int cardId, sbyte ownerId)
    {
        sourceRegion = RegionEnum.NumRegions;
        for (int i = 0; i < 3; i++)
        {
            Card card = Regions[i].FindTraitActor(cardId, ownerId);
            if (card != null)
            {
                sourceRegion = (RegionEnum) i;
                return sourceRegion;
            }
        }

        return sourceRegion;
    }

    public void FindCards(TraitTargeting info, RegionEnum region, Card source, List<CardStack> found)
    {
        if (info.Area == TargetableArea.CurrentRegion && region != RegionEnum.NumRegions)
        {
            Regions[(uint) region].FindCards(info, source, found);
            return;
        }

        for (int i = 0; i < Regions.Length; i++)
        {
            if (info.CheckRegion((RegionEnum) i, source.ActiveData.Owner))
            {
                Regions[i].FindCards(info, source, found);
            }
        }
    }

    public void FindCardStack(Card card, List<CardStack> found)
    {
        for (int i = 0; i < Regions.Length && !Regions[i].FindCardStack(card, found); i++)
        {
        }
    }

    public void CheckDiscards(Player[] players)
    {
        bool flag;
        do
        {
            flag = false;
            for (int i = 0; i < 3; i++)
            {
                if (Regions[i].CheckDiscards(players))
                {
                    flag = true;
                }
            }
        } while (flag);
    }

    public void CardMoved(Card card, CardStack target, RegionEnum destination, RegionEnum origin)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].CardMoved(card, target, destination, origin);
            }
        }
    }

    public void CardAttacked(Card attacker, Card target)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].CardAttacked(attacker, target);
            }
        }
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].CardCounterAttacked(attacker, target);
            }
        }
    }

    public void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].CardGainedStatus(theCard, source, statusType);
            }
        }
    }

    public void CardDamaged(Card damagedCard, Card source)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].CardDamaged(damagedCard, source);
            }
        }
    }

    public void CardDied(Card deadCard, Card source)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].CardDied(deadCard, source);
            }
        }
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].CardDrawn(drawnCard, regularDraw, isNewTurn);
            }
        }
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].CardDiscardEffect(playerIndex, numberOfCards);
            }
        }
    }

    public void SecretTriggered(Card secret, Card source)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].SecretTriggered(secret, source);
            }
        }
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].SecretDestroyed(secret, source);
            }
        }
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        GameRegion gameRegion = null;
        for (int i = 0; i < 3; i++)
        {
            gameRegion = Regions[i];
            for (int j = 0; j < gameRegion.Slots.Length; j++)
            {
                gameRegion.Slots[j].TraitEffectActivating(effect, source, target, region);
            }
        }
    }

    private CardStack FindCard(int cardId, sbyte ownerId)
    {
        sourceRegion = RegionEnum.NumRegions;
        for (int i = 0; i < 3; i++)
        {
            CardStack cardStack = Regions[i].FindCard(cardId, ownerId);
            if (cardStack != null)
            {
                sourceRegion = (RegionEnum) i;
                return cardStack;
            }
        }

        return null;
    }

    private Card RemoveCard(int cardId, sbyte ownerId)
    {
        for (int i = 0; i < 3; i++)
        {
            Card card = Regions[i].RemoveCard(cardId, ownerId);
            if (card != null)
            {
                return card;
            }
        }

        return null;
    }

    private sbyte RegionToPlayerIndex(RegionEnum region, Player[] players)
    {
        sbyte b = (sbyte) region;
        if (b >= 0 && b < players.Length)
        {
            return b;
        }

        return -1;
    }
}