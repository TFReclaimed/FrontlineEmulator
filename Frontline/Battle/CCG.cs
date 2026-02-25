using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class CCG
{
    public const sbyte GAMEOVER_INDICATOR = -1;

    public const sbyte GAMESTART_INDICATOR = -2;

    public Guid GameInstanceId { get; set; }

    public Player[] Players { get; set; }

    public GameBoard Board { get; set; }

    public int GameTemplateId { get; set; }

    public sbyte CurrentRound { get; set; }

    public sbyte PlayerTurn { get; set; }

    public long PlayerTurnStart { get; set; }

    public long PlayerDiscardStart { get; set; }

    public sbyte WinningPlayer { get; set; } = -1;

    public bool SurrenderGameOver { get; set; }

    public Rewards[] Rewards { get; set; }

    public int NextSummonInstanceId { get; set; } = -1;

    public VersusType GameType { get; set; }

    private GameTemplate gameRules;

    private readonly CcgGame _game;

    private List<RewardsTemplate> winGameRewards = new List<RewardsTemplate>();

    private List<RewardsTemplate> loseGameRewards = new List<RewardsTemplate>();

    private List<ActiveTrait> battleEffects = new List<ActiveTrait>();

    private List<ActiveTrait> temporaryEffects = new List<ActiveTrait>();

    private BaseTrait pilotEmbarkTrait;

    private BaseTrait titanPilotEmbarkTrait;

    private List<CcgEventData> ccgEventsLog = new List<CcgEventData>();

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
        int num = NextSummonInstanceId--;
        num = _game.GetServerIntValue(num, num);
        if (num < NextSummonInstanceId)
        {
            NextSummonInstanceId = num - 1;
        }

        return num;
    }

    public Player GetPlayer(sbyte playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < Players.Length)
        {
            return Players[playerIndex];
        }

        return null;
    }

    public sbyte GetOpponentPlayerIndex(sbyte playerIndex)
    {
        sbyte b = (sbyte) (playerIndex + 1);
        if (b >= Players.Length)
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
        GameInstanceId = gameInstance;
        GameTemplateId = gameId;
        gameRules = RulesetParser.GetGameTemplate(GameTemplateId)!;
        Board = new GameBoard(this);
        Board.Create(gameRules);
        int num = playerIds.Length;
        Players = new Player[num];
        for (int i = 0; i < num; i++)
        {
            Players[i] = new Player(this);
            Players[i].Create(playerIds[i], playerNames[i], deckCards[i], supportCards[i], commanders[i], gameRules,
                (sbyte) i, skipShuffle[i]);
        }

        Rewards = new Rewards[num];
        for (int j = 0; j < num; j++)
        {
            Rewards[j] = new Rewards();
            Players[j].ActivateCommander();
        }

        PlayerTurn = -2;

        winGameRewards.Add(RulesetParser.GetRewardsTemplate(gameRules.WinRewardId)!);
        loseGameRewards.Add(RulesetParser.GetRewardsTemplate(gameRules.LossRewardId)!);

        pilotEmbarkTrait = RulesetParser.GetTraitTemplate(gameRules.EmbarkedPilotTrait)!;
        titanPilotEmbarkTrait = RulesetParser.GetTraitTemplate(gameRules.PilotTitanEmbarkedTrait)!;

        pilotEmbarkTrait.Init(this);
        titanPilotEmbarkTrait.Init(this);
    }

    public bool IsGameOver()
    {
        return SurrenderGameOver || PlayerTurn == -1;
    }

    public Card FindTraitActor(sbyte playerIndex, int cardId)
    {
        if (playerIndex >= 0 && playerIndex < Players.Length)
        {
            Player player = Players[playerIndex];
            Card card = player.FindTraitActor(cardId);
            if (card != null)
            {
                return card;
            }
        }

        return Board.FindTraitActor(cardId, playerIndex);
    }

    public Region GetTraitActorRegion(sbyte playerIndex, int cardId)
    {
        Region result = Region.NumRegions;
        if (playerIndex >= 0 && playerIndex < Players.Length)
        {
            Player player = Players[playerIndex];
            Card card = player.FindTraitActor(cardId);
            if (card != null)
            {
                return result;
            }
        }

        return Board.GetTraitActorRegion(cardId, playerIndex);
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
            if (activeTrait.GetTraitInfo().IsIntercept(activeTrait) && activeTrait.Target.Owner != owner)
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

    public List<CardStack> FindCards(TraitTargeting info, Region region, Card source)
    {
        List<CardStack> list = new List<CardStack>();
        if (info.Area == TargetableArea.AnyAreas || info.Area == TargetableArea.BattleField ||
            info.Area == TargetableArea.AnyCommander)
        {
            sbyte owner = source.ActiveData.Owner;
            for (int i = 0; i < Players.Length; i++)
            {
                if ((info.CheckFriendly() && i == owner) || (info.CheckEnemy() && i != owner))
                {
                    Card primaryCard = Players[i].Commander.PrimaryCard;
                    if (info.DoesMatchType(primaryCard))
                    {
                        list.Add(Players[i].Commander);
                    }
                }
            }
        }
        else if (info.Area == TargetableArea.FriendlyCommander)
        {
            sbyte owner2 = source.ActiveData.Owner;
            Card primaryCard2 = Players[owner2].Commander.PrimaryCard;
            if (info.DoesMatchType(primaryCard2))
            {
                list.Add(Players[owner2].Commander);
            }
        }
        else if (info.Area == TargetableArea.EnemyCommander)
        {
            sbyte opponentPlayerIndex = GetOpponentPlayerIndex(source.ActiveData.Owner);
            Card primaryCard3 = Players[opponentPlayerIndex].Commander.PrimaryCard;
            if (info.DoesMatchType(primaryCard3))
            {
                list.Add(Players[opponentPlayerIndex].Commander);
            }
        }

        if (info.Area == TargetableArea.AnyAreas || info.Area == TargetableArea.FriendlyDiscard ||
            info.Area == TargetableArea.EnemyDiscard)
        {
            sbyte owner3 = source.ActiveData.Owner;
            for (int j = 0; j < Players.Length; j++)
            {
                if ((!info.CheckFriendly() || j != owner3) && (!info.CheckEnemy() || j == owner3))
                {
                    continue;
                }

                for (int k = 0; k < Players[j].Discard.Cards.Count; k++)
                {
                    Card card = Players[j].Discard.Cards[k];
                    if (info.CardTargetMatch(this, card, source))
                    {
                        CardStack cardStack = new CardStack();
                        cardStack.Create();
                        cardStack.PrimaryCard = Players[j].Discard.Cards[k];
                        list.Add(cardStack);
                    }
                }
            }
        }

        if (info.Area != TargetableArea.AnyCommander && info.Area != TargetableArea.FriendlyCommander &&
            info.Area != TargetableArea.EnemyCommander && info.Area != TargetableArea.FriendlyHand &&
            info.Area != TargetableArea.EnemyHand && info.Area != TargetableArea.FriendlyDiscard &&
            info.Area != TargetableArea.EnemyDiscard)
        {
            Board.FindCards(info, region, source, list);
        }

        return list;
    }

    public List<CardStack> FindCardStack(Card card)
    {
        List<CardStack> list = new List<CardStack>();
        for (int i = 0; i < Players.Length; i++)
        {
            Card primaryCard = Players[i].Commander.PrimaryCard;
            if (primaryCard.EqualsTo(card))
            {
                list.Add(Players[i].Commander);
            }
        }

        Board.FindCardStack(card, list);
        return list;
    }

    public bool CanDeploy(sbyte playerIndex, int cardId, TargetableArea area, Region target, sbyte slotIndex,
        sbyte pushDir, bool remote)
    {
        if (PlayerTurn == playerIndex && remote)
        {
            Player player = Players[playerIndex];
            if (player.CanSubmitActions())
            {
                Card card = player.FindCard(cardId);
                if (card != null)
                {
                    sbyte commandUnits = player.Resources.CommandUnits;
                    if (card.GetCurrentCost() <= commandUnits)
                    {
                        sbyte opponentPlayerIndex = GetOpponentPlayerIndex(playerIndex);
                        switch (area)
                        {
                            case TargetableArea.FriendlyCommander:
                                return card.CanDeploy(Players[playerIndex].Commander, Region.NumRegions, false,
                                    false);
                            case TargetableArea.EnemyCommander:
                                return card.CanDeploy(Players[opponentPlayerIndex].Commander, Region.NumRegions,
                                    false, false);
                            case TargetableArea.AnyCommander:
                                return card.CanDeploy(Players[playerIndex].Commander, Region.NumRegions, false,
                                    false) || card.CanDeploy(Players[opponentPlayerIndex].Commander,
                                    Region.NumRegions, false, false);
                            case TargetableArea.BattleField:
                                if (target == Region.NumRegions &&
                                    (card.CanDeploy(Players[playerIndex].Commander, Region.NumRegions, false,
                                        false) || card.CanDeploy(Players[opponentPlayerIndex].Commander,
                                        Region.NumRegions, false, false)))
                                {
                                    return true;
                                }

                                break;
                        }

                        if (area == TargetableArea.FriendlyDiscard || area == TargetableArea.EnemyDiscard)
                        {
                            return card.CanDeploy(Region.NumRegions, area);
                        }

                        return Board.CanDeploy(card, area, target, slotIndex, pushDir);
                    }
                }
            }
        }

        return false;
    }

    public bool Deploy(sbyte playerIndex, int cardId, sbyte targetIndex, int targetId, TargetableArea area,
        Region target, sbyte slotIndex, sbyte pushDir, BaseTraitEffect traitCause)
    {
        Player player = Players[playerIndex];
        Card card = null;
        card = ((traitCause != null)
            ? player.RemoveCardForTrait(cardId, playerIndex, traitCause)
            : player.DeployCard(cardId));
        if (card != null)
        {
            CardTransitionCcgEvent cardTransitionCCGEvent = new CardTransitionCcgEvent(CcgEventType.DeployUnit, cardId,
                playerIndex, targetId, targetIndex, false, target, slotIndex, pushDir);
            cardTransitionCCGEvent.TemplateId = card.TemplateId;
            cardTransitionCCGEvent.Rank = card.Rank;
            if (traitCause != null)
            {
                cardTransitionCCGEvent.EffectId = traitCause.EffectTraitId;
                cardTransitionCCGEvent.TraitId = traitCause.TraitParentId;
            }

            AddCCGEventLog(cardTransitionCCGEvent);
            if (card.GetTemplate().Type == CardType.BurnCard || card.GetTemplate().Type == CardType.Secret)
            {
                cardTransitionCCGEvent.Transition = ((card.GetTemplate().Type == CardType.BurnCard)
                    ? CcgEventType.DeployBurn
                    : CcgEventType.DeploySecret);
                if (CheckSpecialCardDeployment(card, targetIndex, targetId, area, target, slotIndex))
                {
                    for (int i = 0; i < Board.Regions.Length; i++)
                    {
                        for (int j = 0; j < Board.Regions[i].Slots.Length; j++)
                        {
                            Board.Regions[i].Slots[j].CardDeployed(card);
                        }
                    }

                    for (int k = 0; k < Players.Length; k++)
                    {
                        Players[k].Commander.CardDeployed(card);
                    }

                    Board.CheckDiscards(Players);
                    gameRules.CheckEndGame(this);
                    return true;
                }
            }

            CardStack cardStack = Board.Deploy(card, target, slotIndex, pushDir, cardTransitionCCGEvent);
            if (cardStack == null)
            {
                return false;
            }

            for (int l = 0; l < Players.Length; l++)
            {
                Players[l].Commander.CardDeployed(card);
            }

            Board.CheckDiscards(Players);
            gameRules.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public bool CheckSpecialCardDeployment(Card deployed, sbyte targetIndex, int targetId, TargetableArea area,
        Region region, sbyte slotIndex)
    {
        Player player = Players[deployed.ActiveData.Owner];
        Player player2 = Players[targetIndex];
        Card card = null;
        CardStack cardStack = null;
        switch (area)
        {
            case TargetableArea.FriendlyDiscard:
                card = player.Discard.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack();
                    cardStack.Create();
                    cardStack.PrimaryCard = card;
                }

                break;
            case TargetableArea.EnemyDiscard:
                card = player2.Discard.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack();
                    cardStack.Create();
                    cardStack.PrimaryCard = card;
                }

                break;
            case TargetableArea.FriendlyHand:
                card = player.Hand.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack();
                    cardStack.Create();
                    cardStack.PrimaryCard = card;
                }

                break;
            case TargetableArea.EnemyHand:
                card = player2.Hand.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack();
                    cardStack.Create();
                    cardStack.PrimaryCard = card;
                }

                break;
            case TargetableArea.FriendlyCommander:
                cardStack = player.Commander;
                card = cardStack.PrimaryCard;
                break;
            case TargetableArea.EnemyCommander:
                cardStack = player2.Commander;
                card = cardStack.PrimaryCard;
                break;
        }

        if (card != null && cardStack != null)
        {
            deployed.Deploy(cardStack, false, region, null);
            return true;
        }

        return false;
    }

    public bool CanMove(sbyte playerIndex, int cardId, Region target, sbyte slotIndex, sbyte pushDir, bool remote)
    {
        if (PlayerTurn == playerIndex && remote && pushDir >= -1 && pushDir <= 1)
        {
            Player player = Players[playerIndex];
            if (player.CanSubmitActions())
            {
                return Board.CanMove(cardId, playerIndex, target, slotIndex, pushDir, gameRules);
            }
        }
        Console.WriteLine("CCG.CanMove false - player cannot move now");
        return false;
    }

    public bool Move(sbyte playerIndex, int cardId, Region target, sbyte slotIndex, sbyte pushDir,
        BaseTraitEffect traitCause)
    {
        bool flag = false;
        if (pushDir == 0 && Board.Regions[(uint) target].Slots[slotIndex].PrimaryCard != null)
        {
            flag = true;
        }

        CardTransitionCcgEvent cardTransitionCCGEvent = new CardTransitionCcgEvent(CcgEventType.Move, cardId,
            playerIndex, 0, 0, false, target, slotIndex, pushDir);
        AddCCGEventLog(cardTransitionCCGEvent);
        if (traitCause != null)
        {
            cardTransitionCCGEvent.EffectId = traitCause.EffectTraitId;
            cardTransitionCCGEvent.TraitId = traitCause.TraitParentId;
        }

        if (Board.Move(cardId, playerIndex, target, slotIndex, pushDir))
        {
            if (flag)
            {
                Card card = Board.FindTraitActor(cardId, playerIndex);
                List<CardStack> list = FindCardStack(card);
                UnitCard unitCard = null;
                UnitCard unitCard2 = null;
                CardStack cardStack = null;
                if (list.Count > 0)
                {
                    cardStack = list[0];
                    if (cardStack.PrimaryCard.HasPilot())
                    {
                        unitCard2 = (UnitCard) cardStack.PrimaryCard;
                        unitCard = unitCard2.EmbarkedPilot;
                        if (unitCard2.GetTemplate().Type == CardType.Titan &&
                            unitCard.GetTemplate().Type == CardType.Pilot)
                        {
                            cardTransitionCCGEvent.Embark = true;
                            if (card.EqualsTo(unitCard))
                            {
                                cardTransitionCCGEvent.TargetId = unitCard2.InstanceId;
                                cardTransitionCCGEvent.TargetOwner = unitCard2.ActiveData.Owner;
                            }
                            else
                            {
                                cardTransitionCCGEvent.TargetId = unitCard.InstanceId;
                                cardTransitionCCGEvent.TargetOwner = unitCard.ActiveData.Owner;
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
        if (PlayerTurn == playerIndex && remote)
        {
            Player player = Players[playerIndex];
            if (player.CanSubmitActions())
            {
                return Board.CanDisembark(cardId, playerIndex);
            }
        }

        return false;
    }

    public bool Disembark(sbyte playerIndex, int cardId, bool eject, BaseTraitEffect traitCause)
    {
        Card card = Board.FindTraitActor(cardId, playerIndex);
        List<CardStack> list = FindCardStack(card);
        if (list.Count <= 0 || list[0].PrimaryCard == null || !list[0].PrimaryCard.HasPilot())
        {
            return false;
        }

        CardStack cardStack = list[0];
        UnitCard unitCard = (UnitCard) cardStack.PrimaryCard;
        UnitCard embarkedPilot = unitCard.EmbarkedPilot;
        if (embarkedPilot.GetTemplate().Type != 0)
        {
            return false;
        }

        pilotEmbarkTrait.Deactivate(unitCard, embarkedPilot);
        Board.Disembark(cardId, playerIndex, eject, traitCause);
        return true;
    }

    private CardStack FindCardStackForSummon(sbyte playerIndex, bool isTitan, bool reverseSearch,
        Region currentRegion, TargetableArea targetableArea)
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

        return Board.Regions[num].FindEmptyCardStack(isTitan, reverseSearch);
    }

    public bool CanSummon(sbyte playerIndex, int cardTemplateId, Region currentRegion,
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

    public bool Summon(sbyte playerIndex, int cardTemplateId, Region currentRegion, TargetableArea targetableArea,
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
            card.InstanceId = GetNextSummonInstanceId();
            card.ActiveData.Owner = playerIndex;
            card.Setup();
            Console.WriteLine("**** CCG.Summon - Spanwed New Card * " + card.InstanceId);
            card.Deploy(cardStack, false, currentRegion, null);
            currentRegion = GetTraitActorRegion(playerIndex, card.InstanceId);
            CardStack[] slots = Board.Regions[(uint) currentRegion].Slots;
            sbyte indexSlot = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].PrimaryCard != null && slots[i].PrimaryCard.EqualsTo(card))
                {
                    indexSlot = (sbyte) i;
                }
            }

            CardTransitionCcgEvent cardTransitionCCGEvent = new CardTransitionCcgEvent(CcgEventType.CardSummon,
                card.InstanceId, playerIndex, 0, 0, false, currentRegion, indexSlot, 1);
            AddCCGEventLog(cardTransitionCCGEvent);
            if (traitCause != null)
            {
                cardTransitionCCGEvent.EffectId = traitCause.EffectTraitId;
                cardTransitionCCGEvent.TraitId = traitCause.TraitParentId;
            }

            return true;
        }

        return false;
    }

    public bool CanDoInitialSwap(sbyte playerIndex, int[] cardIdsToSwap)
    {
        if (cardIdsToSwap != null && playerIndex >= 0 && playerIndex < Players.Length)
        {
            Player player = Players[playerIndex];
            if (player.Surrender || player.InitialCardsSwapped)
            {
                return false;
            }

            CardCollection hand = player.Hand;
            int num = cardIdsToSwap.Length;
            if (num <= hand.Cards.Count && num <= gameRules.MulliganDiscard)
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
        Player player = Players[playerIndex];
        CardCollection hand = player.Hand;
        Deck deck = player.Deck;
        Card[] array = new Card[cardIdsToSwap.Length];
        for (int i = 0; i < cardIdsToSwap.Length; i++)
        {
            array[i] = hand.RemoveCard(cardIdsToSwap[i]);
        }

        if (cardIdsToSwap.Length > 0)
        {
            MulliganDrawCcgEvent mulliganDrawCCGEvent = new MulliganDrawCcgEvent(playerIndex);
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

        int count = deck.Cards.Count;
        for (int k = 0; k < array.Length; k++)
        {
            if (isServer)
            {
                deck.InsertCardAtIndex(index: newDeckIndices[k] = Random.Shared.Next(0, count + k), card: array[k]);
                continue;
            }

            int num = newDeckIndices[k];
            deck.Count++;
        }

        player.InitialCardsSwapped = true;
        bool flag = true;
        for (int l = 0; l < Players.Length; l++)
        {
            if (!Players[l].InitialCardsSwapped)
            {
                flag = false;
            }
        }

        if (flag)
        {
            PlayerTurn = 0;
            Players[PlayerTurn].NewTurn(PlayerTurn, GetDrawCount());
            Board.NewTurn(PlayerTurn);
            PlayerTurnStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        return true;
    }

    public bool CanDoDiscard(sbyte playerIndex, int[] cardIdsToDiscard)
    {
        if (cardIdsToDiscard != null && playerIndex >= 0 && playerIndex < Players.Length)
        {
            Player player = Players[playerIndex];
            if (player.Surrender)
            {
                return false;
            }

            CardCollection hand = player.Hand;
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
        CardCollection hand = Players[playerIndex].Hand;
        for (int i = 0; i < cardIdsToDiscard.Length; i++)
        {
            Card card = hand.RemoveCard(cardIdsToDiscard[i]);
            card.Discard(Players);
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
            player.Resources.AddCommandPoints((sbyte) commandPoints, GetGameTemplate());
        }

        if (cardTemplateId != 0)
        {
            CardTemplate cardTemplate = RulesetParser.GetCardTemplate(cardTemplateId, (sbyte) rank);
            if (cardTemplate != null)
            {
                Card card = cardTemplate.GenerateCard(this);
                if (card != null)
                {
                    card.InstanceId = GetNextSummonInstanceId();
                    card.Setup();
                    card.ActiveData.Owner = playerIndex;
                    player.Hand.Cards.Add(card);
                    Console.WriteLine("**** CCG.GiveCardAndCmdPts - Spanwed New Card * " + card.InstanceId);
                }
            }
        }

        return true;
    }

    public bool CanTriggerEndTurnTraits(sbyte playerIndex, bool remote)
    {
        if (PlayerTurn == playerIndex && remote)
        {
            Player player = Players[playerIndex];
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
        TurnChangeCcgEvent logData = new TurnChangeCcgEvent(CcgEventType.EndTurn, playerIndex);
        AddCCGEventLog(logData);
        Player player = Players[playerIndex];
        if (player.TriggerEndTurnTraits(gameRules, playerIndex))
        {
            Board.EndTurn(playerIndex);
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i].Commander.EndTurn(playerIndex);
            }

            Board.CheckDiscards(Players);
            return true;
        }

        return false;
    }

    public bool CanEndTurn(sbyte playerIndex, bool remote, int[] cardsToDiscard)
    {
        if (PlayerTurn == playerIndex && remote)
        {
            Player player = Players[playerIndex];
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
        Player player = Players[playerIndex];
        if (player.EndTurn(gameRules, playerIndex))
        {
            CardCollection hand = Players[playerIndex].Hand;
            for (int i = 0; i < cardIdsToDiscard.Length; i++)
            {
                Card card = hand.RemoveCard(cardIdsToDiscard[i]);
                card.Discard(Players);
            }

            sbyte nextPlayerIndex = GetNextPlayerIndex(playerIndex);
            StartNewTurn(nextPlayerIndex);
            return true;
        }

        return false;
    }

    public bool CanAttack(sbyte playerIndex, int cardId, sbyte targetOwner, int targetId, bool remote)
    {
        if (PlayerTurn == playerIndex && remote)
        {
            Player player = Players[playerIndex];
            if (player.CanSubmitActions())
            {
                return Board.CanAttack(playerIndex, cardId, targetOwner, targetId, Players);
            }
        }

        Console.WriteLine("CCG.CanAttack false - player cannot attack now");
        return false;
    }

    public bool Attack(sbyte playerIndex, int cardId, sbyte ownerId, int targetId)
    {
        if (Board.Attack(playerIndex, cardId, ownerId, targetId, Players))
        {
            Board.CheckDiscards(Players);
            gameRules.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public bool CanActivate(sbyte playerIndex, int cardId, sbyte ownerId, int targetId, TargetableArea area,
        Region region, bool remote)
    {
        Player player = Players[playerIndex];
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
        Region region)
    {
        if (Board.ActivateTrait(playerIndex, cardId, ownerId, targetId, area, region, Players))
        {
            Board.CheckDiscards(Players);
            gameRules.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        Board.CardMoved(card, target, region, origin);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardMoved(card, target, region, origin);
        }
    }

    public void CardAttacked(Card attacker, Card target)
    {
        Board.CardAttacked(attacker, target);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardAttacked(attacker, target);
        }
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        Board.CardCounterAttacked(attacker, target);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardCounterAttacked(attacker, target);
        }
    }

    public void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        Board.CardGainedStatus(theCard, source, statusType);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardGainedStatus(theCard, source, statusType);
        }
    }

    public void CardDamaged(Card damangedCard, Card source)
    {
        Board.CardDamaged(damangedCard, source);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardDamaged(damangedCard, source);
        }
    }

    public void CardDied(Card deadCard, Card source)
    {
        Board.CardDied(deadCard, source);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardDied(deadCard, source);
        }
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        Board.CardDrawn(drawnCard, regularDraw, isNewTurn);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        Board.CardDiscardEffect(playerIndex, numberOfCards);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public void SecretTriggered(Card secret, Card source)
    {
        Board.SecretTriggered(secret, source);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.SecretTriggered(secret, source);
        }
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        Board.SecretDestroyed(secret, source);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.SecretDestroyed(secret, source);
        }
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, Region region)
    {
        Board.TraitEffectActivating(effect, source, target, region);
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.TraitEffectActivating(effect, source, target, region);
        }
    }

    public bool CanSurrender(sbyte playerIndex)
    {
        if (PlayerTurn >= 0 && !Players[playerIndex].Surrender)
        {
            return true;
        }

        return false;
    }

    public bool Surrender(sbyte playerIndex)
    {
        Player player = Players[playerIndex];
        player.Surrender = true;
        int num = Players.Length;
        int num2 = 0;
        for (int i = 0; i < num; i++)
        {
            if (Players[i].Surrender)
            {
                num2++;
            }
        }

        if (num2 == num - 1)
        {
            SurrenderGameOver = true;
            _game.EndGame();
            if (CurrentRound > 3)
            {
                GenerateRewards();
            }
        }

        return true;
    }

    public bool CanMessage(sbyte playerIndex)
    {
        if (PlayerTurn >= 0 && !Players[playerIndex].Surrender)
        {
            return true;
        }

        return false;
    }

    public sbyte GetNextPlayerIndex(sbyte playerIndex)
    {
        if (SurrenderGameOver || Players == null || gameRules == null)
        {
            return -1;
        }

        int num = Players.Length;
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
        if (GameType != 0)
        {
            return;
        }

        for (int i = 0; i < Rewards.Length; i++)
        {
            if (i == WinningPlayer || (SurrenderGameOver && !Players[i].Surrender))
            {
                Rewards[i].Generate(true, winGameRewards);
            }
            else
            {
                Rewards[i].Generate(false, loseGameRewards);
            }
        }
    }

    public void AddCCGEventLog(CcgEventData logData)
    {
        ccgEventsLog.Add(logData);
    }

    public List<CcgEventData> GetCCGEventLog()
    {
        return ccgEventsLog;
    }

    private void StartNewTurn(sbyte playerIndex)
    {
        PlayerTurn = playerIndex;
        SetCurrentRound();
        PlayerTurnStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (PlayerTurn >= 0)
        {
            TurnChangeCcgEvent logData = new TurnChangeCcgEvent(CcgEventType.NewTurn, playerIndex);
            AddCCGEventLog(logData);
            Players[PlayerTurn].NewTurn(PlayerTurn, GetDrawCount());
            Board.NewTurn(PlayerTurn);
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i].Commander.NewTurn(playerIndex);
            }
        }

        Board.CheckDiscards(Players);
        gameRules.CheckEndGame(this);
    }

    private void SetCurrentRound()
    {
        if (PlayerTurn == 0)
        {
            CurrentRound++;
        }
    }

    private sbyte GetDrawCount()
    {
        bool flag = CurrentRound == 0;
        bool flag2 = PlayerTurn == 0;
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