using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class CCG
{
    public const sbyte GAMEOVER_INDICATOR = -1;

    public const sbyte GAMESTART_INDICATOR = -2;

    public Guid gameInstanceId;

    public Player[] players;

    public GameBoard board;

    public int gameTemplateId;

    public sbyte currentRound;

    public sbyte playerTurn;

    public long playerTurnStart;

    public long playerDiscardStart;

    public sbyte localPlayer;

    public sbyte winningPlayer = -1;

    public bool surrenderGameOver;

    public Rewards[] rewards;

    public int nextSummonInstanceId = -1;

    public VersusType gameType;

    private GameTemplate gameRules;

    private readonly CcgGame _game;

    private List<RewardsTemplate> winGameRewards = new List<RewardsTemplate>();

    private List<RewardsTemplate> loseGameRewards = new List<RewardsTemplate>();

    private List<ActiveTrait> battleEffects = new List<ActiveTrait>();

    private List<ActiveTrait> temporaryEffects = new List<ActiveTrait>();

    private BaseTrait pilotEmbarkTrait;

    private BaseTrait titanPilotEmbarkTrait;

    private List<CCGEventData> ccgEventsLog = new List<CCGEventData>();

    public CCG(CcgGame game)
    {
        _game = game;
    }

    public CcgGame GetGame()
    {
        return _game;
    }

    public GameTemplate GetGameTemplate()
    {
        return gameRules;
    }

    public int GetNextSummonInstanceId()
    {
        int num = nextSummonInstanceId--;
        num = _game.GetServerIntValue(num, num);
        if (num < nextSummonInstanceId)
        {
            nextSummonInstanceId = num - 1;
        }

        return num;
    }

    public Player GetPlayer(sbyte playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < players.Length)
        {
            return players[playerIndex];
        }

        return null;
    }

    public sbyte GetOpponentPlayerIndex(sbyte playerIndex)
    {
        sbyte b = (sbyte) (playerIndex + 1);
        if (b >= players.Length)
        {
            b = 0;
        }

        return b;
    }

    public BaseTrait GetPilotEmbarkTrait()
    {
        return pilotEmbarkTrait;
    }

    public BaseTrait GetTitanPilotEmbarkTrait()
    {
        return titanPilotEmbarkTrait;
    }

    public void Create(Guid gameInstance, int gameId, int[] playerIds, string[] playerNames,
        List<List<Card>> deckCards, List<List<Card>> supportCards, List<Card> commanders, bool[] skipShuffle)
    {
        gameInstanceId = gameInstance;
        gameTemplateId = gameId;
        gameRules = RulesetParser.GetGameTemplate(gameTemplateId)!;
        board = new GameBoard(this);
        board.Create(gameRules);
        int num = playerIds.Length;
        players = new Player[num];
        for (int i = 0; i < num; i++)
        {
            players[i] = new Player(this);
            players[i].Create(playerIds[i], playerNames[i], deckCards[i], supportCards[i], commanders[i], gameRules,
                (sbyte) i, skipShuffle[i]);
        }

        rewards = new Rewards[num];
        for (int j = 0; j < num; j++)
        {
            rewards[j] = new Rewards();
            players[j].ActivateCommander();
        }

        playerTurn = -2;
    }

    public Card FindTraitActor(sbyte playerIndex, int cardId)
    {
        if (playerIndex >= 0 && playerIndex < players.Length)
        {
            Player player = players[playerIndex];
            Card card = player.FindTraitActor(cardId);
            if (card != null)
            {
                return card;
            }
        }

        return board.FindTraitActor(cardId, playerIndex);
    }

    public RegionEnum GetTraitActorRegion(sbyte playerIndex, int cardId)
    {
        RegionEnum result = RegionEnum.NumRegions;
        if (playerIndex >= 0 && playerIndex < players.Length)
        {
            Player player = players[playerIndex];
            Card card = player.FindTraitActor(cardId);
            if (card != null)
            {
                return result;
            }
        }

        return board.GetTraitActorRegion(cardId, playerIndex);
    }

    public List<ActiveTrait> GetBattleEffects()
    {
        return battleEffects;
    }

    public bool HasInterceptBattleEffect(int owner)
    {
        ActiveTrait activeTrait = null;
        for (int i = 0; i < battleEffects.Count; i++)
        {
            activeTrait = battleEffects[i];
            if (activeTrait.GetTraitInfo().IsIntercept(activeTrait) && activeTrait.target.owner != owner)
            {
                return true;
            }
        }

        return false;
    }

    public void CaptureTemporaryEffect(ActiveTrait active)
    {
        temporaryEffects.Add(active);
    }

    public void PurgeTemporaryEffects()
    {
        for (int i = 0; i < temporaryEffects.Count; i++)
        {
            temporaryEffects[i].Deactivate(false);
        }

        temporaryEffects.Clear();
    }

    public List<ActiveTrait> GetTemporaryEffects()
    {
        return temporaryEffects;
    }

    public List<CardStack> FindCards(TraitTargeting info, RegionEnum region, Card source)
    {
        List<CardStack> list = new List<CardStack>();
        if (info.area == TargetableArea.AnyAreas || info.area == TargetableArea.BattleField ||
            info.area == TargetableArea.AnyCommander)
        {
            sbyte owner = source.activeData.owner;
            for (int i = 0; i < players.Length; i++)
            {
                if ((info.CheckFriendly() && i == owner) || (info.CheckEnemy() && i != owner))
                {
                    Card primaryCard = players[i].commander.primaryCard;
                    if (info.DoesMatchType(primaryCard))
                    {
                        list.Add(players[i].commander);
                    }
                }
            }
        }
        else if (info.area == TargetableArea.FriendlyCommander)
        {
            sbyte owner2 = source.activeData.owner;
            Card primaryCard2 = players[owner2].commander.primaryCard;
            if (info.DoesMatchType(primaryCard2))
            {
                list.Add(players[owner2].commander);
            }
        }
        else if (info.area == TargetableArea.EnemyCommander)
        {
            sbyte opponentPlayerIndex = GetOpponentPlayerIndex(source.activeData.owner);
            Card primaryCard3 = players[opponentPlayerIndex].commander.primaryCard;
            if (info.DoesMatchType(primaryCard3))
            {
                list.Add(players[opponentPlayerIndex].commander);
            }
        }

        if (info.area == TargetableArea.AnyAreas || info.area == TargetableArea.FriendlyDiscard ||
            info.area == TargetableArea.EnemyDiscard)
        {
            sbyte owner3 = source.activeData.owner;
            for (int j = 0; j < players.Length; j++)
            {
                if ((!info.CheckFriendly() || j != owner3) && (!info.CheckEnemy() || j == owner3))
                {
                    continue;
                }

                for (int k = 0; k < players[j].discard.cards.Count; k++)
                {
                    Card card = players[j].discard.cards[k];
                    if (info.CardTargetMatch(this, card, source))
                    {
                        CardStack cardStack = new CardStack();
                        cardStack.Create();
                        cardStack.primaryCard = players[j].discard.cards[k];
                        list.Add(cardStack);
                    }
                }
            }
        }

        if (info.area != TargetableArea.AnyCommander && info.area != TargetableArea.FriendlyCommander &&
            info.area != TargetableArea.EnemyCommander && info.area != TargetableArea.FriendlyHand &&
            info.area != TargetableArea.EnemyHand && info.area != TargetableArea.FriendlyDiscard &&
            info.area != TargetableArea.EnemyDiscard)
        {
            board.FindCards(info, region, source, list);
        }

        return list;
    }

    public List<CardStack> FindCardStack(Card card)
    {
        List<CardStack> list = new List<CardStack>();
        for (int i = 0; i < players.Length; i++)
        {
            Card primaryCard = players[i].commander.primaryCard;
            if (primaryCard.EqualsTo(card))
            {
                list.Add(players[i].commander);
            }
        }

        board.FindCardStack(card, list);
        return list;
    }

    public bool CanDeploy(sbyte playerIndex, int cardId, TargetableArea area, RegionEnum target, sbyte slotIndex,
        sbyte pushDir, bool remote)
    {
        if (playerTurn == playerIndex && (remote || localPlayer == playerIndex))
        {
            Player player = players[playerIndex];
            if (player.CanSubmitActions())
            {
                Card card = player.FindCard(cardId);
                if (card != null)
                {
                    sbyte commandUnits = player.resources.commandUnits;
                    if (card.GetCurrentCost() <= commandUnits)
                    {
                        sbyte opponentPlayerIndex = GetOpponentPlayerIndex(playerIndex);
                        switch (area)
                        {
                            case TargetableArea.FriendlyCommander:
                                return card.CanDeploy(players[playerIndex].commander, RegionEnum.NumRegions, false,
                                    false);
                            case TargetableArea.EnemyCommander:
                                return card.CanDeploy(players[opponentPlayerIndex].commander, RegionEnum.NumRegions,
                                    false, false);
                            case TargetableArea.AnyCommander:
                                return card.CanDeploy(players[playerIndex].commander, RegionEnum.NumRegions, false,
                                    false) || card.CanDeploy(players[opponentPlayerIndex].commander,
                                    RegionEnum.NumRegions, false, false);
                            case TargetableArea.BattleField:
                                if (target == RegionEnum.NumRegions &&
                                    (card.CanDeploy(players[playerIndex].commander, RegionEnum.NumRegions, false,
                                        false) || card.CanDeploy(players[opponentPlayerIndex].commander,
                                        RegionEnum.NumRegions, false, false)))
                                {
                                    return true;
                                }

                                break;
                        }

                        if (area == TargetableArea.FriendlyDiscard || area == TargetableArea.EnemyDiscard)
                        {
                            return card.CanDeploy(RegionEnum.NumRegions, area);
                        }

                        return board.CanDeploy(card, area, target, slotIndex, pushDir);
                    }
                }
            }
        }

        return false;
    }

    public bool Deploy(sbyte playerIndex, int cardId, sbyte targetIndex, int targetId, TargetableArea area,
        RegionEnum target, sbyte slotIndex, sbyte pushDir, BaseTraitEffect traitCause)
    {
        Player player = players[playerIndex];
        Card card = null;
        card = ((traitCause != null)
            ? player.RemoveCardForTrait(cardId, playerIndex, traitCause)
            : player.DeployCard(cardId));
        if (card != null)
        {
            CardTransitionCCGEvent cardTransitionCCGEvent = new CardTransitionCCGEvent(CCGEventType.DeployUnit, cardId,
                playerIndex, targetId, targetIndex, false, target, slotIndex, pushDir);
            cardTransitionCCGEvent.templateId = card.templateId;
            cardTransitionCCGEvent.rank = card.rank;
            if (traitCause != null)
            {
                cardTransitionCCGEvent.effectID = traitCause.effectTraitID;
                cardTransitionCCGEvent.traitID = traitCause.traitParentID;
            }

            AddCCGEventLog(cardTransitionCCGEvent);
            if (card.GetTemplate().Type == CardType.BurnCard || card.GetTemplate().Type == CardType.Secret)
            {
                cardTransitionCCGEvent.transition = ((card.GetTemplate().Type == CardType.BurnCard)
                    ? CCGEventType.DeployBurn
                    : CCGEventType.DeploySecret);
                if (CheckSpecialCardDeployment(card, targetIndex, targetId, area, target, slotIndex))
                {
                    for (int i = 0; i < board.regions.Length; i++)
                    {
                        for (int j = 0; j < board.regions[i].slots.Length; j++)
                        {
                            board.regions[i].slots[j].CardDeployed(card);
                        }
                    }

                    for (int k = 0; k < players.Length; k++)
                    {
                        players[k].commander.CardDeployed(card);
                    }

                    board.CheckDiscards(players);
                    gameRules.CheckEndGame(this);
                    return true;
                }
            }

            CardStack cardStack = board.Deploy(card, target, slotIndex, pushDir, cardTransitionCCGEvent);
            if (cardStack == null)
            {
                return false;
            }

            for (int l = 0; l < players.Length; l++)
            {
                players[l].commander.CardDeployed(card);
            }

            board.CheckDiscards(players);
            gameRules.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public bool CheckSpecialCardDeployment(Card deployed, sbyte targetIndex, int targetId, TargetableArea area,
        RegionEnum region, sbyte slotIndex)
    {
        Player player = players[deployed.activeData.owner];
        Player player2 = players[targetIndex];
        Card card = null;
        CardStack cardStack = null;
        switch (area)
        {
            case TargetableArea.FriendlyDiscard:
                card = player.discard.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack();
                    cardStack.Create();
                    cardStack.primaryCard = card;
                }

                break;
            case TargetableArea.EnemyDiscard:
                card = player2.discard.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack();
                    cardStack.Create();
                    cardStack.primaryCard = card;
                }

                break;
            case TargetableArea.FriendlyHand:
                card = player.hand.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack();
                    cardStack.Create();
                    cardStack.primaryCard = card;
                }

                break;
            case TargetableArea.EnemyHand:
                card = player2.hand.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack();
                    cardStack.Create();
                    cardStack.primaryCard = card;
                }

                break;
            case TargetableArea.FriendlyCommander:
                cardStack = player.commander;
                card = cardStack.primaryCard;
                break;
            case TargetableArea.EnemyCommander:
                cardStack = player2.commander;
                card = cardStack.primaryCard;
                break;
        }

        if (card != null && cardStack != null)
        {
            deployed.Deploy(cardStack, false, region, null);
            return true;
        }

        return false;
    }

    public bool CanMove(sbyte playerIndex, int cardId, RegionEnum target, sbyte slotIndex, sbyte pushDir, bool remote)
    {
        if (playerTurn == playerIndex && (remote || localPlayer == playerIndex) && pushDir >= -1 && pushDir <= 1)
        {
            Player player = players[playerIndex];
            if (player.CanSubmitActions())
            {
                return board.CanMove(cardId, playerIndex, target, slotIndex, pushDir, gameRules);
            }
        }
        Console.WriteLine("CCG.CanMove false - player cannot move now");
        return false;
    }

    public bool Move(sbyte playerIndex, int cardId, RegionEnum target, sbyte slotIndex, sbyte pushDir,
        BaseTraitEffect traitCause)
    {
        bool flag = false;
        if (pushDir == 0 && board.regions[(uint) target].slots[slotIndex].primaryCard != null)
        {
            flag = true;
        }

        CardTransitionCCGEvent cardTransitionCCGEvent = new CardTransitionCCGEvent(CCGEventType.Move, cardId,
            playerIndex, 0, 0, false, target, slotIndex, pushDir);
        AddCCGEventLog(cardTransitionCCGEvent);
        if (traitCause != null)
        {
            cardTransitionCCGEvent.effectID = traitCause.effectTraitID;
            cardTransitionCCGEvent.traitID = traitCause.traitParentID;
        }

        if (board.Move(cardId, playerIndex, target, slotIndex, pushDir))
        {
            if (flag)
            {
                Card card = board.FindTraitActor(cardId, playerIndex);
                List<CardStack> list = FindCardStack(card);
                UnitCard unitCard = null;
                UnitCard unitCard2 = null;
                CardStack cardStack = null;
                if (list.Count > 0)
                {
                    cardStack = list[0];
                    if (cardStack.primaryCard.HasPilot())
                    {
                        unitCard2 = (UnitCard) cardStack.primaryCard;
                        unitCard = unitCard2.embarkedPilot;
                        if (unitCard2.GetTemplate().Type == CardType.Titan &&
                            unitCard.GetTemplate().Type == CardType.Pilot)
                        {
                            cardTransitionCCGEvent.embark = true;
                            if (card.EqualsTo(unitCard))
                            {
                                cardTransitionCCGEvent.targetId = unitCard2.instanceId;
                                cardTransitionCCGEvent.targetOwner = unitCard2.activeData.owner;
                            }
                            else
                            {
                                cardTransitionCCGEvent.targetId = unitCard.instanceId;
                                cardTransitionCCGEvent.targetOwner = unitCard.activeData.owner;
                            }
                        }
                    }
                }
            }

            return true;
        }

        return false;
    }

    public bool CanDisembark(sbyte playerIndex, int cardId, bool remote)
    {
        if (playerTurn == playerIndex && (remote || localPlayer == playerIndex))
        {
            Player player = players[playerIndex];
            if (player.CanSubmitActions())
            {
                return board.CanDisembark(cardId, playerIndex);
            }
        }

        return false;
    }

    public bool Disembark(sbyte playerIndex, int cardId, bool eject, BaseTraitEffect traitCause)
    {
        Card card = board.FindTraitActor(cardId, playerIndex);
        List<CardStack> list = FindCardStack(card);
        if (list.Count <= 0 || list[0].primaryCard == null || !list[0].primaryCard.HasPilot())
        {
            return false;
        }

        CardStack cardStack = list[0];
        UnitCard unitCard = (UnitCard) cardStack.primaryCard;
        UnitCard embarkedPilot = unitCard.embarkedPilot;
        if (embarkedPilot.GetTemplate().Type != 0)
        {
            return false;
        }

        pilotEmbarkTrait.Deactivate(unitCard, embarkedPilot);
        board.Disembark(cardId, playerIndex, eject, traitCause);
        return true;
    }

    private CardStack FindCardStackForSummon(sbyte playerIndex, bool isTitan, bool reverseSearch,
        RegionEnum currentRegion, TargetableArea targetableArea)
    {
        int num = -1;
        switch (targetableArea)
        {
            case TargetableArea.FriendlyPerimeter:
                num = 0 + playerIndex;
                break;
            case TargetableArea.EnemyPerimeter:
                num = 0 + GetOpponentPlayerIndex(playerIndex);
                break;
            case TargetableArea.Frontline:
                num = 2;
                break;
            case TargetableArea.CurrentRegion:
                num = (int) currentRegion;
                break;
            default:
                return null;
        }

        if (num == -1)
        {
            return null;
        }

        return board.regions[num].FindEmptyCardStack(isTitan, reverseSearch);
    }

    public bool CanSummon(sbyte playerIndex, int cardTemplateId, RegionEnum currentRegion,
        TargetableArea targetableArea)
    {
        CardTemplate cardTemplate = RulesetParser.GetCardTemplate(cardTemplateId, 0);
        if (cardTemplate != null)
        {
            bool isTitan = cardTemplate.Type == CardType.Titan;
            bool reverseSearch = cardTemplate.IsSupportUnit();
            if (FindCardStackForSummon(playerIndex, isTitan, reverseSearch, currentRegion, targetableArea) != null)
            {
                return true;
            }
        }

        return false;
    }

    public bool Summon(sbyte playerIndex, int cardTemplateId, RegionEnum currentRegion, TargetableArea targetableArea,
        BaseTraitEffect traitCause)
    {
        CardTemplate cardTemplate = RulesetParser.GetCardTemplate(cardTemplateId, 0);
        if (cardTemplate == null)
        {
            return false;
        }

        bool isTitan = cardTemplate.Type == CardType.Titan;
        bool reverseSearch = cardTemplate.IsSupportUnit();
        CardStack cardStack =
            FindCardStackForSummon(playerIndex, isTitan, reverseSearch, currentRegion, targetableArea);
        if (cardStack != null)
        {
            Card card = cardTemplate.GenerateCard(this);
            card.instanceId = GetNextSummonInstanceId();
            card.activeData.owner = playerIndex;
            card.Setup();
            Console.WriteLine("**** CCG.Summon - Spanwed New Card * " + card.instanceId);
            card.Deploy(cardStack, false, currentRegion, null);
            currentRegion = GetTraitActorRegion(playerIndex, card.instanceId);
            CardStack[] slots = board.regions[(uint) currentRegion].slots;
            sbyte indexSlot = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].primaryCard != null && slots[i].primaryCard.EqualsTo(card))
                {
                    indexSlot = (sbyte) i;
                }
            }

            CardTransitionCCGEvent cardTransitionCCGEvent = new CardTransitionCCGEvent(CCGEventType.CardSummon,
                card.instanceId, playerIndex, 0, 0, false, currentRegion, indexSlot, 1);
            AddCCGEventLog(cardTransitionCCGEvent);
            if (traitCause != null)
            {
                cardTransitionCCGEvent.effectID = traitCause.effectTraitID;
                cardTransitionCCGEvent.traitID = traitCause.traitParentID;
            }

            return true;
        }

        return false;
    }

    public bool CanDoInitialSwap(sbyte playerIndex, int[] cardIdsToSwap)
    {
        if (cardIdsToSwap != null && playerIndex >= 0 && playerIndex < players.Length)
        {
            Player player = players[playerIndex];
            if (player.surrender || player.initialCardsSwapped)
            {
                return false;
            }

            CardCollection hand = player.hand;
            int num = cardIdsToSwap.Length;
            if (num <= hand.cards.Count && num <= gameRules.MulliganDiscard)
            {
                for (int i = 0; i < cardIdsToSwap.Length; i++)
                {
                    if (hand.FindCard(cardIdsToSwap[i]) == null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        return false;
    }

    public bool DoInitialSwap(sbyte playerIndex, int[] cardIdsToSwap, int[] newDeckIndices, bool isServer)
    {
        Player player = players[playerIndex];
        CardCollection hand = player.hand;
        Deck deck = player.deck;
        Card[] array = new Card[cardIdsToSwap.Length];
        for (int i = 0; i < cardIdsToSwap.Length; i++)
        {
            array[i] = hand.RemoveCard(cardIdsToSwap[i]);
        }

        if (cardIdsToSwap.Length > 0)
        {
            MulliganDrawCCGEvent mulliganDrawCCGEvent = new MulliganDrawCCGEvent(playerIndex);
            for (int j = 0; j < cardIdsToSwap.Length; j++)
            {
                Card card = hand.DrawFromDeck(deck, this, playerIndex);
                if (card != null)
                {
                    mulliganDrawCCGEvent.AddDrawnCard(card);
                }
            }

            AddCCGEventLog(mulliganDrawCCGEvent);
        }

        int count = deck.cards.Count;
        for (int k = 0; k < array.Length; k++)
        {
            if (isServer)
            {
                deck.InsertCardAtIndex(index: newDeckIndices[k] = Random.Shared.Next(0, count + k), card: array[k]);
                continue;
            }

            int num = newDeckIndices[k];
            deck.count++;
        }

        player.initialCardsSwapped = true;
        bool flag = true;
        for (int l = 0; l < players.Length; l++)
        {
            if (!players[l].initialCardsSwapped)
            {
                flag = false;
            }
        }

        if (flag)
        {
            playerTurn = 0;
            players[playerTurn].NewTurn(playerTurn, GetDrawCount());
            board.NewTurn(playerTurn);
            playerTurnStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        return true;
    }

    public bool CanDoDiscard(sbyte playerIndex, int[] cardIdsToDiscard)
    {
        if (cardIdsToDiscard != null && playerIndex >= 0 && playerIndex < players.Length)
        {
            Player player = players[playerIndex];
            if (player.surrender)
            {
                return false;
            }

            CardCollection hand = player.hand;
            for (int i = 0; i < cardIdsToDiscard.Length; i++)
            {
                if (hand.FindCard(cardIdsToDiscard[i]) == null)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public bool DoCardDiscard(sbyte playerIndex, int[] cardIdsToDiscard)
    {
        CardCollection hand = players[playerIndex].hand;
        for (int i = 0; i < cardIdsToDiscard.Length; i++)
        {
            Card card = hand.RemoveCard(cardIdsToDiscard[i]);
            card.Discard(players);
        }

        return true;
    }

    public bool GiveCardAndCmdPts(sbyte playerIndex, int cardTemplateId, int rank, int commandPoints)
    {
        Player player = GetPlayer(playerIndex);
        if (player == null)
        {
            return false;
        }

        if (commandPoints != 0)
        {
            player.resources.AddCommandPoints((sbyte) commandPoints, GetGameTemplate());
        }

        if (cardTemplateId != 0)
        {
            CardTemplate cardTemplate = RulesetParser.GetCardTemplate(cardTemplateId, (sbyte) rank);
            if (cardTemplate != null)
            {
                Card card = cardTemplate.GenerateCard(this);
                if (card != null)
                {
                    card.instanceId = GetNextSummonInstanceId();
                    card.Setup();
                    card.activeData.owner = playerIndex;
                    player.hand.cards.Add(card);
                    Console.WriteLine("**** CCG.GiveCardAndCmdPts - Spanwed New Card * " + card.instanceId);
                }
            }
        }

        return true;
    }

    public bool CanTriggerEndTurnTraits(sbyte playerIndex, bool remote)
    {
        if (playerTurn == playerIndex && (remote || localPlayer == playerIndex))
        {
            Player player = players[playerIndex];
            if (!player.CanTriggerEndTurnTraits(gameRules))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public bool TriggerEndTurnTraits(sbyte playerIndex)
    {
        TurnChangeCCGEvent logData = new TurnChangeCCGEvent(CCGEventType.EndTurn, playerIndex);
        AddCCGEventLog(logData);
        Player player = players[playerIndex];
        if (player.TriggerEndTurnTraits(gameRules, playerIndex))
        {
            board.EndTurn(playerIndex);
            for (int i = 0; i < players.Length; i++)
            {
                players[i].commander.EndTurn(playerIndex);
            }

            board.CheckDiscards(players);
            return true;
        }

        return false;
    }

    public bool CanEndTurn(sbyte playerIndex, bool remote, int[] cardsToDiscard)
    {
        if (playerTurn == playerIndex && (remote || localPlayer == playerIndex))
        {
            Player player = players[playerIndex];
            if (!player.CanEndTurn(gameRules, cardsToDiscard))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public bool EndTurn(sbyte playerIndex, int[] cardIdsToDiscard)
    {
        Player player = players[playerIndex];
        if (player.EndTurn(gameRules, playerIndex))
        {
            CardCollection hand = players[playerIndex].hand;
            for (int i = 0; i < cardIdsToDiscard.Length; i++)
            {
                Card card = hand.RemoveCard(cardIdsToDiscard[i]);
                card.Discard(players);
            }

            sbyte nextPlayerIndex = GetNextPlayerIndex(playerIndex);
            StartNewTurn(nextPlayerIndex);
            return true;
        }

        return false;
    }

    public bool CanAttack(sbyte playerIndex, int cardId, sbyte targetOwner, int targetId, bool remote)
    {
        if (playerTurn == playerIndex && (remote || localPlayer == playerIndex))
        {
            Player player = players[playerIndex];
            if (player.CanSubmitActions())
            {
                return board.CanAttack(playerIndex, cardId, targetOwner, targetId, players);
            }
        }

        Console.WriteLine("CCG.CanAttack false - player cannot attack now");
        return false;
    }

    public bool CanAttack(sbyte playerIndex, int cardId)
    {
        if (playerTurn == playerIndex && localPlayer == playerIndex)
        {
            Player player = players[playerIndex];
            if (player.CanSubmitActions())
            {
                return board.CanAttack(playerIndex, cardId);
            }
        }

        return false;
    }

    public bool Attack(sbyte playerIndex, int cardId, sbyte ownerId, int targetId)
    {
        if (board.Attack(playerIndex, cardId, ownerId, targetId, players))
        {
            board.CheckDiscards(players);
            gameRules.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public bool CanActivate(sbyte playerIndex, int cardId, sbyte ownerId, int targetId, TargetableArea area,
        RegionEnum region, bool remote)
    {
        Player player = players[playerIndex];
        if (!player.CanSubmitActions())
        {
            return false;
        }

        Card card = FindTraitActor(playerIndex, cardId);
        if (card == null)
        {
            return false;
        }

        Card target = FindTraitActor(ownerId, targetId);
        return card.CanActivate(target, region);
    }

    public bool ActivateTrait(sbyte playerIndex, int cardId, sbyte ownerId, int targetId, TargetableArea area,
        RegionEnum region)
    {
        if (board.ActivateTrait(playerIndex, cardId, ownerId, targetId, area, region, players))
        {
            board.CheckDiscards(players);
            gameRules.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public void CardMoved(Card card, CardStack target, RegionEnum region, RegionEnum origin)
    {
        board.CardMoved(card, target, region, origin);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.CardMoved(card, target, region, origin);
        }
    }

    public void CardAttacked(Card attacker, Card target)
    {
        board.CardAttacked(attacker, target);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.CardAttacked(attacker, target);
        }
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        board.CardCounterAttacked(attacker, target);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.CardCounterAttacked(attacker, target);
        }
    }

    public void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        board.CardGainedStatus(theCard, source, statusType);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.CardGainedStatus(theCard, source, statusType);
        }
    }

    public void CardDamaged(Card damangedCard, Card source)
    {
        board.CardDamaged(damangedCard, source);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.CardDamaged(damangedCard, source);
        }
    }

    public void CardDied(Card deadCard, Card source)
    {
        board.CardDied(deadCard, source);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.CardDied(deadCard, source);
        }
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        board.CardDrawn(drawnCard, regularDraw, isNewTurn);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        board.CardDiscardEffect(playerIndex, numberOfCards);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public void SecretTriggered(Card secret, Card source)
    {
        board.SecretTriggered(secret, source);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.SecretTriggered(secret, source);
        }
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        board.SecretDestroyed(secret, source);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.SecretDestroyed(secret, source);
        }
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        board.TraitEffectActivating(effect, source, target, region);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].commander.TraitEffectActivating(effect, source, target, region);
        }
    }

    public bool CanSurrender(sbyte playerIndex)
    {
        if (playerTurn >= 0 && !players[playerIndex].surrender)
        {
            return true;
        }

        return false;
    }

    public bool Surrender(sbyte playerIndex)
    {
        Player player = players[playerIndex];
        player.surrender = true;
        int num = players.Length;
        int num2 = 0;
        for (int i = 0; i < num; i++)
        {
            if (players[i].surrender)
            {
                num2++;
            }
        }

        if (num2 == num - 1)
        {
            surrenderGameOver = true;
            if (currentRound > 3)
            {
                GenerateRewards();
            }
        }

        return true;
    }

    public bool CanMessage(sbyte playerIndex)
    {
        if (playerTurn >= 0 && !players[playerIndex].surrender)
        {
            return true;
        }

        return false;
    }

    public sbyte GetNextPlayerIndex(sbyte playerIndex)
    {
        if (surrenderGameOver || players == null || gameRules == null)
        {
            return -1;
        }

        int num = players.Length;
        sbyte b = playerIndex;
        do
        {
            b++;
            if (b == num)
            {
                b = 0;
            }
        } while (b != playerIndex && !gameRules.IsActive(b, this));

        if (b != playerIndex)
        {
            return b;
        }

        return -1;
    }

    public void GenerateRewards()
    {
        if (gameType != 0)
        {
            return;
        }

        for (int i = 0; i < rewards.Length; i++)
        {
            if (i == winningPlayer || (surrenderGameOver && !players[i].surrender))
            {
                rewards[i].Generate(true, winGameRewards);
            }
            else
            {
                rewards[i].Generate(false, loseGameRewards);
            }
        }
    }

    public void AddCCGEventLog(CCGEventData logData)
    {
        ccgEventsLog.Add(logData);
    }

    public List<CCGEventData> GetCCGEventLog()
    {
        return ccgEventsLog;
    }

    private void StartNewTurn(sbyte playerIndex)
    {
        playerTurn = playerIndex;
        SetCurrentRound();
        playerTurnStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (playerTurn >= 0)
        {
            TurnChangeCCGEvent logData = new TurnChangeCCGEvent(CCGEventType.NewTurn, playerIndex);
            AddCCGEventLog(logData);
            players[playerTurn].NewTurn(playerTurn, GetDrawCount());
            board.NewTurn(playerTurn);
            for (int i = 0; i < players.Length; i++)
            {
                players[i].commander.NewTurn(playerIndex);
            }
        }

        board.CheckDiscards(players);
        gameRules.CheckEndGame(this);
    }

    private void SetCurrentRound()
    {
        if (playerTurn == 0)
        {
            currentRound++;
        }
    }

    private sbyte GetDrawCount()
    {
        bool flag = currentRound == 0;
        bool flag2 = playerTurn == 0;
        if (flag && flag2)
        {
            return gameRules.FirstTurnDrawFirstPlayer;
        }

        if (flag && !flag2)
        {
            return gameRules.FirstTurnDrawOtherPlayer;
        }

        return gameRules.NewTurnDraw;
    }
}