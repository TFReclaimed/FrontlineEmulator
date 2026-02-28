using System.Text.Json.Serialization;
using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class CcgGameState
{
    public const sbyte GameOverIndicator = -1;

    public const sbyte GameStartIndicator = -2;

    public readonly GameLogger Logger;

    [JsonInclude]
    public readonly Guid GameInstanceId;

    [JsonInclude]
    public readonly int GameTemplateId;

    [JsonInclude]
    public readonly VersusType GameType;

    [JsonInclude]
    public readonly Player[] Players = new Player[2];

    [JsonInclude]
    public readonly GameBoard Board;

    [JsonInclude]
    public readonly Rewards[] Rewards = new Rewards[2];

    public sbyte CurrentRound { get; set; }

    public sbyte PlayerTurn { get; set; }

    public long PlayerTurnStart { get; set; }

    public long PlayerDiscardStart { get; set; }

    public sbyte WinningPlayer { get; set; } = -1;

    public bool SurrenderGameOver { get; set; }

    public int NextSummonInstanceId { get; set; } = -1;

    private readonly GameTemplate _gameRules;

    private readonly CcgGame _game;

    private readonly List<RewardsTemplate> _winGameRewards = [];

    private readonly List<RewardsTemplate> _loseGameRewards = [];

    private readonly List<ActiveTrait> _battleEffects = [];

    private readonly List<ActiveTrait> _temporaryEffects = [];

    private readonly BaseTrait _pilotEmbarkTrait;

    private readonly BaseTrait _titanPilotEmbarkTrait;

    private readonly List<CcgEventData> _ccgEventsLog = [];

    public CcgGameState(CcgGame game, GameLogger logger, Guid gameInstance, int gameId, VersusType gameType)
    {
        _game = game;
        Logger = logger;
        GameInstanceId = gameInstance;
        GameTemplateId = gameId;
        GameType = gameType;

        var gameRules = RulesetParser.GetGameTemplate(GameTemplateId);
        if (gameRules == null)
        {
            throw new Exception($"Game rules not found for GameTemplateId: {GameTemplateId}");
        }

        _gameRules = gameRules;

        Board = new GameBoard(this);

        var winRewards = RulesetParser.GetRewardsTemplate(_gameRules.WinRewardId);
        if (winRewards == null)
        {
            throw new Exception($"Win rewards not found for WinRewardId: {_gameRules.WinRewardId}");
        }

        _winGameRewards.Add(winRewards);

        var lossRewards = RulesetParser.GetRewardsTemplate(_gameRules.LossRewardId);
        if (lossRewards == null)
        {
            throw new Exception($"Loss rewards not found for LossRewardId: {_gameRules.LossRewardId}");
        }

        _loseGameRewards.Add(lossRewards);

        var pilotEmbarkTrait = RulesetParser.GetTraitTemplate(_gameRules.EmbarkedPilotTrait);
        if (pilotEmbarkTrait == null)
        {
            throw new Exception($"Pilot embark trait not found for EmbarkedPilotTrait: {_gameRules.EmbarkedPilotTrait}");
        }

        _pilotEmbarkTrait = pilotEmbarkTrait;
        
        var titanPilotEmbarkTrait = RulesetParser.GetTraitTemplate(_gameRules.PilotTitanEmbarkedTrait);
        if (titanPilotEmbarkTrait == null)
        {
            throw new Exception($"Titan pilot embark trait not found for PilotTitanEmbarkedTrait: {_gameRules.PilotTitanEmbarkedTrait}");
        }

        _titanPilotEmbarkTrait = titanPilotEmbarkTrait;

        _pilotEmbarkTrait.Init(this);
        _titanPilotEmbarkTrait.Init(this);
    }

    public void CreatePlayers(int[] playerIds, string[] playerNames, List<List<Card>> deckCards,
        List<List<Card>> supportCards, List<CommanderCard> commanders)
    {
        for (var i = 0; i < 2; i++)
        {
            Players[i] = new Player(this, playerIds[i], playerNames[i], deckCards[i], supportCards[i],
                commanders[i], (sbyte) i, false);
            Rewards[i] = new Rewards();
        }

        foreach (var player in Players)
        {
            player.ActivateCommander();
        }

        PlayerTurn = GameStartIndicator;
    }

    public CcgGame GetGame()
    {
        return _game;
    }

    public GameTemplate GetGameTemplate()
    {
        return _gameRules;
    }

    public int GetNextSummonInstanceId()
    {
        var num = NextSummonInstanceId--;
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
        var b = (sbyte) (playerIndex + 1);
        if (b >= Players.Length)
        {
            b = 0;
        }

        return b;
    }

    public BaseTrait GetPilotEmbarkTrait()
    {
        return _pilotEmbarkTrait;
    }

    public BaseTrait GetTitanPilotEmbarkTrait()
    {
        return _titanPilotEmbarkTrait;
    }

    public bool IsGameOver()
    {
        return SurrenderGameOver || PlayerTurn == GameOverIndicator;
    }

    public Card FindTraitActor(sbyte playerIndex, int cardId)
    {
        if (playerIndex >= 0 && playerIndex < Players.Length)
        {
            var player = Players[playerIndex];
            var card = player.FindTraitActor(cardId);
            if (card != null)
            {
                return card;
            }
        }

        return Board.FindTraitActor(cardId, playerIndex);
    }

    public Region GetTraitActorRegion(sbyte playerIndex, int cardId)
    {
        var result = Region.NumRegions;
        if (playerIndex >= 0 && playerIndex < Players.Length)
        {
            var player = Players[playerIndex];
            var card = player.FindTraitActor(cardId);
            if (card != null)
            {
                return result;
            }
        }

        return Board.GetTraitActorRegion(cardId, playerIndex);
    }

    public List<ActiveTrait> GetBattleEffects()
    {
        return _battleEffects;
    }

    public bool HasInterceptBattleEffect(int owner)
    {
        ActiveTrait activeTrait = null;
        for (var i = 0; i < _battleEffects.Count; i++)
        {
            activeTrait = _battleEffects[i];
            if (activeTrait.GetTraitInfo().IsIntercept(activeTrait) && activeTrait.Target.Owner != owner)
            {
                return true;
            }
        }

        return false;
    }

    public void CaptureTemporaryEffect(ActiveTrait active)
    {
        _temporaryEffects.Add(active);
    }

    public void PurgeTemporaryEffects()
    {
        for (var i = 0; i < _temporaryEffects.Count; i++)
        {
            _temporaryEffects[i].Deactivate(false);
        }

        _temporaryEffects.Clear();
    }

    public List<ActiveTrait> GetTemporaryEffects()
    {
        return _temporaryEffects;
    }

    public List<CardStack> FindCards(TraitTargeting info, Region region, Card source)
    {
        var list = new List<CardStack>();
        if (info.Area == TargetableArea.AnyAreas || info.Area == TargetableArea.BattleField ||
            info.Area == TargetableArea.AnyCommander)
        {
            var owner = source.ActiveData.Owner;
            for (var i = 0; i < Players.Length; i++)
            {
                if ((info.CheckFriendly() && i == owner) || (info.CheckEnemy() && i != owner))
                {
                    var primaryCard = Players[i].Commander.PrimaryCard;
                    if (info.DoesMatchType(primaryCard))
                    {
                        list.Add(Players[i].Commander);
                    }
                }
            }
        }
        else if (info.Area == TargetableArea.FriendlyCommander)
        {
            var owner2 = source.ActiveData.Owner;
            var primaryCard2 = Players[owner2].Commander.PrimaryCard;
            if (info.DoesMatchType(primaryCard2))
            {
                list.Add(Players[owner2].Commander);
            }
        }
        else if (info.Area == TargetableArea.EnemyCommander)
        {
            var opponentPlayerIndex = GetOpponentPlayerIndex(source.ActiveData.Owner);
            var primaryCard3 = Players[opponentPlayerIndex].Commander.PrimaryCard;
            if (info.DoesMatchType(primaryCard3))
            {
                list.Add(Players[opponentPlayerIndex].Commander);
            }
        }

        if (info.Area == TargetableArea.AnyAreas || info.Area == TargetableArea.FriendlyDiscard ||
            info.Area == TargetableArea.EnemyDiscard)
        {
            var owner3 = source.ActiveData.Owner;
            for (var j = 0; j < Players.Length; j++)
            {
                if ((!info.CheckFriendly() || j != owner3) && (!info.CheckEnemy() || j == owner3))
                {
                    continue;
                }

                for (var k = 0; k < Players[j].Discard.Cards.Count; k++)
                {
                    var card = Players[j].Discard.Cards[k];
                    if (info.CardTargetMatch(this, card, source))
                    {
                        var cardStack = new CardStack(this)
                        {
                            PrimaryCard = Players[j].Discard.Cards[k]
                        };
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
        var list = new List<CardStack>();
        for (var i = 0; i < Players.Length; i++)
        {
            var primaryCard = Players[i].Commander.PrimaryCard;
            if (primaryCard.EqualsTo(card))
            {
                list.Add(Players[i].Commander);
            }
        }

        Board.FindCardStack(card, list);
        return list;
    }

    public bool CanDeploy(sbyte playerIndex, int cardId, TargetableArea area, Region target, sbyte slotIndex,
        sbyte pushDir)
    {
        if (PlayerTurn == playerIndex)
        {
            var player = Players[playerIndex];
            if (player.CanSubmitActions())
            {
                var card = player.FindCard(cardId);
                if (card != null)
                {
                    var commandUnits = player.Resources.CommandUnits;
                    if (card.GetCurrentCost() <= commandUnits)
                    {
                        var opponentPlayerIndex = GetOpponentPlayerIndex(playerIndex);
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
        var player = Players[playerIndex];
        Card card = null;
        card = traitCause != null
            ? player.RemoveCardForTrait(cardId, playerIndex, traitCause)
            : player.DeployCard(cardId);
        if (card != null)
        {
            var cardTransitionCCGEvent = new CardTransitionCcgEvent(CcgEventType.DeployUnit, cardId,
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
                cardTransitionCCGEvent.Transition = card.GetTemplate().Type == CardType.BurnCard
                    ? CcgEventType.DeployBurn
                    : CcgEventType.DeploySecret;
                if (CheckSpecialCardDeployment(card, targetIndex, targetId, area, target, slotIndex))
                {
                    for (var i = 0; i < Board.Regions.Length; i++)
                    {
                        for (var j = 0; j < Board.Regions[i].Slots.Length; j++)
                        {
                            Board.Regions[i].Slots[j].CardDeployed(card);
                        }
                    }

                    for (var k = 0; k < Players.Length; k++)
                    {
                        Players[k].Commander.CardDeployed(card);
                    }

                    Board.CheckDiscards(Players);
                    _gameRules.CheckEndGame(this);
                    return true;
                }
            }

            var cardStack = Board.Deploy(card, target, slotIndex, pushDir, cardTransitionCCGEvent);
            if (cardStack == null)
            {
                return false;
            }

            for (var l = 0; l < Players.Length; l++)
            {
                Players[l].Commander.CardDeployed(card);
            }

            Board.CheckDiscards(Players);
            _gameRules.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public bool CheckSpecialCardDeployment(Card deployed, sbyte targetIndex, int targetId, TargetableArea area,
        Region region, sbyte slotIndex)
    {
        var player = Players[deployed.ActiveData.Owner];
        var player2 = Players[targetIndex];
        Card card = null;
        CardStack cardStack = null;
        switch (area)
        {
            case TargetableArea.FriendlyDiscard:
                card = player.Discard.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack(this)
                    {
                        PrimaryCard = card
                    };
                }

                break;
            case TargetableArea.EnemyDiscard:
                card = player2.Discard.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack(this)
                    {
                        PrimaryCard = card
                    };
                }

                break;
            case TargetableArea.FriendlyHand:
                card = player.Hand.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack(this)
                    {
                        PrimaryCard = card
                    };
                }

                break;
            case TargetableArea.EnemyHand:
                card = player2.Hand.FindCard(targetId);
                if (card != null)
                {
                    cardStack = new CardStack(this)
                    {
                        PrimaryCard = card
                    };
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

    public bool CanMove(sbyte playerIndex, int cardId, Region target, sbyte slotIndex, sbyte pushDir)
    {
        if (PlayerTurn == playerIndex && pushDir >= -1 && pushDir <= 1)
        {
            var player = Players[playerIndex];
            if (player.CanSubmitActions())
            {
                return Board.CanMove(cardId, playerIndex, target, slotIndex, pushDir, _gameRules);
            }
        }

        Logger.Debug("CCG.CanMove false - player cannot move now");
        return false;
    }

    public bool Move(sbyte playerIndex, int cardId, Region target, sbyte slotIndex, sbyte pushDir,
        BaseTraitEffect traitCause)
    {
        var flag = false;
        if (pushDir == 0 && Board.Regions[(uint) target].Slots[slotIndex].PrimaryCard != null)
        {
            flag = true;
        }

        var cardTransitionCCGEvent = new CardTransitionCcgEvent(CcgEventType.Move, cardId,
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
                var card = Board.FindTraitActor(cardId, playerIndex);
                var list = FindCardStack(card);
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

    public bool CanDisembark(sbyte playerIndex, int cardId)
    {
        if (PlayerTurn == playerIndex)
        {
            var player = Players[playerIndex];
            if (player.CanSubmitActions())
            {
                return Board.CanDisembark(cardId, playerIndex);
            }
        }

        return false;
    }

    public bool Disembark(sbyte playerIndex, int cardId, bool eject, BaseTraitEffect traitCause)
    {
        var card = Board.FindTraitActor(cardId, playerIndex);
        var list = FindCardStack(card);
        if (list.Count <= 0 || list[0].PrimaryCard == null || !list[0].PrimaryCard.HasPilot())
        {
            return false;
        }

        var cardStack = list[0];
        var unitCard = (UnitCard) cardStack.PrimaryCard;
        var embarkedPilot = unitCard.EmbarkedPilot;
        if (embarkedPilot.GetTemplate().Type != 0)
        {
            return false;
        }

        _pilotEmbarkTrait.Deactivate(unitCard, embarkedPilot);
        Board.Disembark(cardId, playerIndex, eject, traitCause);
        return true;
    }

    private CardStack FindCardStackForSummon(sbyte playerIndex, bool isTitan, bool reverseSearch,
        Region currentRegion, TargetableArea targetableArea)
    {
        var num = -1;
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
        var cardTemplate = RulesetParser.GetCardTemplate(cardTemplateId, 0);
        if (cardTemplate != null)
        {
            var isTitan = cardTemplate.Type == CardType.Titan;
            var reverseSearch = cardTemplate.IsSupportUnit();
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
        var cardTemplate = RulesetParser.GetCardTemplate(cardTemplateId, 0);
        if (cardTemplate == null)
        {
            return false;
        }

        var isTitan = cardTemplate.Type == CardType.Titan;
        var reverseSearch = cardTemplate.IsSupportUnit();
        var cardStack =
            FindCardStackForSummon(playerIndex, isTitan, reverseSearch, currentRegion, targetableArea);
        if (cardStack != null)
        {
            var card = cardTemplate.GenerateCard(this);
            card.InstanceId = GetNextSummonInstanceId();
            card.ActiveData.Owner = playerIndex;
            card.Setup();
            Logger.Debug("**** CCG.Summon - Spanwed New Card * " + card.InstanceId);
            card.Deploy(cardStack, false, currentRegion, null);
            currentRegion = GetTraitActorRegion(playerIndex, card.InstanceId);
            var slots = Board.Regions[(uint) currentRegion].Slots;
            sbyte indexSlot = 0;
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].PrimaryCard != null && slots[i].PrimaryCard.EqualsTo(card))
                {
                    indexSlot = (sbyte) i;
                }
            }

            var cardTransitionCCGEvent = new CardTransitionCcgEvent(CcgEventType.CardSummon,
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
            var player = Players[playerIndex];
            if (player.Surrender || player.InitialCardsSwapped)
            {
                return false;
            }

            var hand = player.Hand;
            var num = cardIdsToSwap.Length;
            if (num <= hand.Cards.Count && num <= _gameRules.MulliganDiscard)
            {
                for (var i = 0; i < cardIdsToSwap.Length; i++)
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

    public bool DoInitialSwap(sbyte playerIndex, int[] cardIdsToSwap, int[] newDeckIndices)
    {
        var player = Players[playerIndex];
        var hand = player.Hand;
        var deck = player.Deck;
        var array = new Card[cardIdsToSwap.Length];
        for (var i = 0; i < cardIdsToSwap.Length; i++)
        {
            array[i] = hand.RemoveCard(cardIdsToSwap[i]);
        }

        if (cardIdsToSwap.Length > 0)
        {
            var mulliganDrawCCGEvent = new MulliganDrawCcgEvent(playerIndex);
            for (var j = 0; j < cardIdsToSwap.Length; j++)
            {
                var card = hand.DrawFromDeck(deck, this, playerIndex);
                if (card != null)
                {
                    mulliganDrawCCGEvent.AddDrawnCard(card);
                }
            }

            AddCCGEventLog(mulliganDrawCCGEvent);
        }

        var count = deck.Cards.Count;
        for (var k = 0; k < array.Length; k++)
        {
            deck.InsertCardAtIndex(index: newDeckIndices[k] = Random.Shared.Next(0, count + k), card: array[k]);
        }

        player.InitialCardsSwapped = true;
        var flag = true;
        for (var l = 0; l < Players.Length; l++)
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
            var player = Players[playerIndex];
            if (player.Surrender)
            {
                return false;
            }

            var hand = player.Hand;
            for (var i = 0; i < cardIdsToDiscard.Length; i++)
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
        var hand = Players[playerIndex].Hand;
        for (var i = 0; i < cardIdsToDiscard.Length; i++)
        {
            var card = hand.RemoveCard(cardIdsToDiscard[i]);
            card.Discard(Players);
        }

        return true;
    }

    public bool GiveCardAndCmdPts(sbyte playerIndex, int cardTemplateId, int rank, int commandPoints)
    {
        var player = GetPlayer(playerIndex);
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
            var cardTemplate = RulesetParser.GetCardTemplate(cardTemplateId, (sbyte) rank);
            if (cardTemplate != null)
            {
                var card = cardTemplate.GenerateCard(this);
                if (card != null)
                {
                    card.InstanceId = GetNextSummonInstanceId();
                    card.Setup();
                    card.ActiveData.Owner = playerIndex;
                    player.Hand.Cards.Add(card);
                    Logger.Debug("**** CCG.GiveCardAndCmdPts - Spanwed New Card * " + card.InstanceId);
                }
            }
        }

        return true;
    }

    public bool CanTriggerEndTurnTraits(sbyte playerIndex)
    {
        if (PlayerTurn == playerIndex)
        {
            var player = Players[playerIndex];
            if (!player.CanTriggerEndTurnTraits(_gameRules))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public bool TriggerEndTurnTraits(sbyte playerIndex)
    {
        var logData = new TurnChangeCcgEvent(CcgEventType.EndTurn, playerIndex);
        AddCCGEventLog(logData);
        var player = Players[playerIndex];
        if (player.TriggerEndTurnTraits(_gameRules, playerIndex))
        {
            Board.EndTurn(playerIndex);
            for (var i = 0; i < Players.Length; i++)
            {
                Players[i].Commander.EndTurn(playerIndex);
            }

            Board.CheckDiscards(Players);
            return true;
        }

        return false;
    }

    public bool CanEndTurn(sbyte playerIndex, int[] cardsToDiscard)
    {
        if (PlayerTurn == playerIndex)
        {
            var player = Players[playerIndex];
            if (!player.CanEndTurn(_gameRules, cardsToDiscard))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public bool EndTurn(sbyte playerIndex, int[] cardIdsToDiscard)
    {
        var player = Players[playerIndex];
        if (player.EndTurn(_gameRules, playerIndex))
        {
            var hand = Players[playerIndex].Hand;
            for (var i = 0; i < cardIdsToDiscard.Length; i++)
            {
                var card = hand.RemoveCard(cardIdsToDiscard[i]);
                card.Discard(Players);
            }

            var nextPlayerIndex = GetNextPlayerIndex(playerIndex);
            StartNewTurn(nextPlayerIndex);
            return true;
        }

        return false;
    }

    public bool CanAttack(sbyte playerIndex, int cardId, sbyte targetOwner, int targetId)
    {
        if (PlayerTurn == playerIndex)
        {
            var player = Players[playerIndex];
            if (player.CanSubmitActions())
            {
                return Board.CanAttack(playerIndex, cardId, targetOwner, targetId, Players);
            }
        }

        Logger.Debug("CCG.CanAttack false - player cannot attack now");
        return false;
    }

    public bool Attack(sbyte playerIndex, int cardId, sbyte ownerId, int targetId)
    {
        if (Board.Attack(playerIndex, cardId, ownerId, targetId, Players))
        {
            Board.CheckDiscards(Players);
            _gameRules.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public bool CanActivate(sbyte playerIndex, int cardId, sbyte ownerId, int targetId, TargetableArea area,
        Region region)
    {
        var player = Players[playerIndex];
        if (!player.CanSubmitActions())
        {
            return false;
        }

        var card = FindTraitActor(playerIndex, cardId);
        if (card == null)
        {
            return false;
        }

        var target = FindTraitActor(ownerId, targetId);
        return card.CanActivate(target, region);
    }

    public bool ActivateTrait(sbyte playerIndex, int cardId, sbyte ownerId, int targetId, TargetableArea area,
        Region region)
    {
        if (Board.ActivateTrait(playerIndex, cardId, ownerId, targetId, area, region, Players))
        {
            Board.CheckDiscards(Players);
            _gameRules.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        Board.CardMoved(card, target, region, origin);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardMoved(card, target, region, origin);
        }
    }

    public void CardAttacked(Card attacker, Card target)
    {
        Board.CardAttacked(attacker, target);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardAttacked(attacker, target);
        }
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        Board.CardCounterAttacked(attacker, target);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardCounterAttacked(attacker, target);
        }
    }

    public void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType)
    {
        Board.CardGainedStatus(theCard, source, statusType);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardGainedStatus(theCard, source, statusType);
        }
    }

    public void CardDamaged(Card damangedCard, Card source)
    {
        Board.CardDamaged(damangedCard, source);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardDamaged(damangedCard, source);
        }
    }

    public void CardDied(Card deadCard, Card source)
    {
        Board.CardDied(deadCard, source);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardDied(deadCard, source);
        }
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        Board.CardDrawn(drawnCard, regularDraw, isNewTurn);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        Board.CardDiscardEffect(playerIndex, numberOfCards);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public void SecretTriggered(Card secret, Card source)
    {
        Board.SecretTriggered(secret, source);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.SecretTriggered(secret, source);
        }
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        Board.SecretDestroyed(secret, source);
        for (var i = 0; i < Players.Length; i++)
        {
            Players[i].Commander.SecretDestroyed(secret, source);
        }
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, Region region)
    {
        Board.TraitEffectActivating(effect, source, target, region);
        for (var i = 0; i < Players.Length; i++)
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
        var player = Players[playerIndex];
        player.Surrender = true;
        var num = Players.Length;
        var num2 = 0;
        for (var i = 0; i < num; i++)
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
        if (SurrenderGameOver || Players == null || _gameRules == null)
        {
            return GameOverIndicator;
        }

        var num = Players.Length;
        var b = playerIndex;
        do
        {
            b++;
            if (b == num)
            {
                b = 0;
            }
        } while (b != playerIndex && !_gameRules.IsActive(b, this));

        if (b != playerIndex)
        {
            return b;
        }

        return GameOverIndicator;
    }

    public void GenerateRewards()
    {
        if (GameType != VersusType.PvpRanked)
        {
            return;
        }

        for (var i = 0; i < Rewards.Length; i++)
        {
            if (i == WinningPlayer || (SurrenderGameOver && !Players[i].Surrender))
            {
                Rewards[i].Generate(true, _winGameRewards);
            }
            else
            {
                Rewards[i].Generate(false, _loseGameRewards);
            }
        }
    }

    public void AddCCGEventLog(CcgEventData logData)
    {
        _ccgEventsLog.Add(logData);
    }

    public List<CcgEventData> GetCCGEventLog()
    {
        return _ccgEventsLog;
    }

    private void StartNewTurn(sbyte playerIndex)
    {
        PlayerTurn = playerIndex;
        SetCurrentRound();
        PlayerTurnStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (PlayerTurn >= 0)
        {
            var logData = new TurnChangeCcgEvent(CcgEventType.NewTurn, playerIndex);
            AddCCGEventLog(logData);
            Players[PlayerTurn].NewTurn(PlayerTurn, GetDrawCount());
            Board.NewTurn(PlayerTurn);
            for (var i = 0; i < Players.Length; i++)
            {
                Players[i].Commander.NewTurn(playerIndex);
            }
        }

        Board.CheckDiscards(Players);
        _gameRules.CheckEndGame(this);
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
        var flag = CurrentRound == 0;
        var flag2 = PlayerTurn == 0;
        if (flag && flag2)
        {
            return _gameRules.FirstTurnDrawFirstPlayer;
        }

        if (flag && !flag2)
        {
            return _gameRules.FirstTurnDrawOtherPlayer;
        }

        return _gameRules.NewTurnDraw;
    }
}