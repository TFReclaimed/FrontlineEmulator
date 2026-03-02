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

    public Player? GetPlayer(sbyte playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < Players.Length)
        {
            return Players[playerIndex];
        }

        return null;
    }

    public sbyte GetOpponentPlayerIndex(sbyte playerIndex)
    {
        return (sbyte) (1 - playerIndex);
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

    public Card? FindTraitActor(sbyte playerIndex, int cardId)
    {
        if (playerIndex < 0 || playerIndex >= Players.Length)
        {
            return Board.FindTraitActor(cardId, playerIndex);
        }

        var player = Players[playerIndex];
        var card = player.FindTraitActor(cardId);
        if (card != null)
        {
            return card;
        }

        return Board.FindTraitActor(cardId, playerIndex);
    }

    public Region GetTraitActorRegion(sbyte playerIndex, int cardId)
    {
        if (playerIndex < 0 || playerIndex >= Players.Length)
        {
            return Board.GetTraitActorRegion(cardId, playerIndex);
        }

        var player = Players[playerIndex];
        var card = player.FindTraitActor(cardId);
        if (card != null)
        {
            return Region.NumRegions;
        }

        return Board.GetTraitActorRegion(cardId, playerIndex);
    }

    public List<ActiveTrait> GetBattleEffects()
    {
        return _battleEffects;
    }

    public bool HasInterceptBattleEffect(int owner)
    {
        foreach (var activeTrait in _battleEffects)
        {
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
        foreach (var effect in _temporaryEffects)
        {
            effect.Deactivate(false);
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

                foreach (var card in Players[j].Discard.Cards)
                {
                    if (info.CardTargetMatch(this, card, source))
                    {
                        var cardStack = new CardStack(this)
                        {
                            PrimaryCard = card
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
        foreach (var player in Players)
        {
            var primaryCard = player.Commander.PrimaryCard!;
            if (primaryCard.EqualsTo(card))
            {
                list.Add(player.Commander);
            }
        }

        Board.FindCardStack(card, list);
        return list;
    }

    public bool CanDeploy(sbyte playerIndex, int cardId, TargetableArea area, Region target, sbyte slotIndex,
        sbyte pushDir)
    {
        if (PlayerTurn != playerIndex)
        {
            return false;
        }

        var player = Players[playerIndex];
        if (!player.CanSubmitActions())
        {
            return false;
        }

        var card = player.FindCard(cardId);
        if (card == null)
        {
            return false;
        }

        var commandUnits = player.Resources.CommandUnits;
        if (card.GetCurrentCost() > commandUnits)
        {
            return false;
        }

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

    public bool Deploy(sbyte playerIndex, int cardId, sbyte targetIndex, int targetId, TargetableArea area,
        Region target, sbyte slotIndex, sbyte pushDir, BaseTraitEffect? traitCause)
    {
        var player = Players[playerIndex];
        var card = traitCause != null
            ? player.RemoveCardForTrait(cardId, playerIndex, traitCause)
            : player.DeployCard(cardId);
        if (card == null)
        {
            return false;
        }

        var deployUnitEvent = new CardTransitionCcgEvent(CcgEventType.DeployUnit, cardId,
            playerIndex, targetId, targetIndex, false, target, slotIndex, pushDir);
        deployUnitEvent.TemplateId = card.TemplateId;
        deployUnitEvent.Rank = card.Rank;
        if (traitCause != null)
        {
            deployUnitEvent.EffectId = traitCause.EffectTraitId;
            deployUnitEvent.TraitId = traitCause.TraitParentId;
        }

        AddCcgEventLog(deployUnitEvent);
        if (card.GetTemplate().Type == CardType.BurnCard || card.GetTemplate().Type == CardType.Secret)
        {
            deployUnitEvent.Transition = card.GetTemplate().Type == CardType.BurnCard
                ? CcgEventType.DeployBurn
                : CcgEventType.DeploySecret;
            if (CheckSpecialCardDeployment(card, targetIndex, targetId, area, target, slotIndex))
            {
                foreach (var region in Board.Regions)
                {
                    foreach (var slot in region.Slots)
                    {
                        slot.CardDeployed(card);
                    }
                }

                foreach (var loopPlayer in Players)
                {
                    loopPlayer.Commander.CardDeployed(card);
                }

                Board.CheckDiscards(Players);
                GameTemplate.CheckEndGame(this);
                return true;
            }
        }

        var cardStack = Board.Deploy(card, target, slotIndex, pushDir, deployUnitEvent);
        if (cardStack == null)
        {
            return false;
        }

        foreach (var loopPlayer in Players)
        {
            loopPlayer.Commander.CardDeployed(card);
        }

        Board.CheckDiscards(Players);
        GameTemplate.CheckEndGame(this);
        return true;
    }

    public bool CheckSpecialCardDeployment(Card deployed, sbyte targetIndex, int targetId, TargetableArea area,
        Region region, sbyte slotIndex)
    {
        var player = Players[deployed.ActiveData.Owner];
        var player2 = Players[targetIndex];
        Card? card = null;
        CardStack? cardStack = null;
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
                return Board.CanMove(cardId, playerIndex, target, slotIndex, pushDir);
            }
        }

        Logger.Debug("CCG.CanMove false - player cannot move now");
        return false;
    }

    public bool Move(sbyte playerIndex, int cardId, Region target, sbyte slotIndex, sbyte pushDir,
        BaseTraitEffect? traitCause)
    {
        var flag = false;
        if (pushDir == 0 && Board.Regions[(uint) target].Slots[slotIndex].PrimaryCard != null)
        {
            flag = true;
        }

        var moveEvent = new CardTransitionCcgEvent(CcgEventType.Move, cardId,
            playerIndex, 0, 0, false, target, slotIndex, pushDir);
        AddCcgEventLog(moveEvent);
        if (traitCause != null)
        {
            moveEvent.EffectId = traitCause.EffectTraitId;
            moveEvent.TraitId = traitCause.TraitParentId;
        }

        if (!Board.Move(cardId, playerIndex, target, slotIndex, pushDir))
        {
            return false;
        }

        if (!flag)
        {
            return true;
        }

        var card = Board.FindTraitActor(cardId, playerIndex)!;
        var list = FindCardStack(card);
        if (list.Count <= 0)
        {
            return true;
        }

        var cardStack = list[0];
        if (!cardStack.PrimaryCard!.HasPilot())
        {
            return true;
        }

        var unitCard2 = (UnitCard) cardStack.PrimaryCard;
        var unitCard = unitCard2.EmbarkedPilot!;
        if (unitCard2.GetTemplate().Type != CardType.Titan ||
            unitCard.GetTemplate().Type != CardType.Pilot)
        {
            return true;
        }

        moveEvent.Embark = true;
        if (card.EqualsTo(unitCard))
        {
            moveEvent.TargetId = unitCard2.InstanceId;
            moveEvent.TargetOwner = unitCard2.ActiveData.Owner;
        }
        else
        {
            moveEvent.TargetId = unitCard.InstanceId;
            moveEvent.TargetOwner = unitCard.ActiveData.Owner;
        }

        return true;
    }

    public bool CanDisembark(sbyte playerIndex, int cardId)
    {
        if (PlayerTurn != playerIndex)
        {
            return false;
        }

        var player = Players[playerIndex];
        if (player.CanSubmitActions())
        {
            return Board.CanDisembark(cardId, playerIndex);
        }

        return false;
    }

    public bool Disembark(sbyte playerIndex, int cardId, bool eject, BaseTraitEffect? traitCause)
    {
        var card = Board.FindTraitActor(cardId, playerIndex)!;
        var list = FindCardStack(card);
        if (list.Count <= 0 || list[0].PrimaryCard == null || !list[0].PrimaryCard!.HasPilot())
        {
            return false;
        }

        var cardStack = list[0];
        var unitCard = (UnitCard) cardStack.PrimaryCard!;
        var embarkedPilot = unitCard.EmbarkedPilot!;
        if (embarkedPilot.GetTemplate().Type != 0)
        {
            return false;
        }

        _pilotEmbarkTrait.Deactivate(unitCard, embarkedPilot);
        Board.Disembark(cardId, playerIndex, eject, traitCause);
        return true;
    }

    private CardStack? FindCardStackForSummon(sbyte playerIndex, bool isTitan, bool reverseSearch,
        Region currentRegion, TargetableArea targetableArea)
    {
        int region;
        switch (targetableArea)
        {
            case TargetableArea.FriendlyPerimeter:
                region = 0 + playerIndex;
                break;
            case TargetableArea.EnemyPerimeter:
                region = 0 + GetOpponentPlayerIndex(playerIndex);
                break;
            case TargetableArea.Frontline:
                region = 2;
                break;
            case TargetableArea.CurrentRegion:
                region = (int) currentRegion;
                break;
            default:
                return null;
        }

        if (region == -1)
        {
            return null;
        }

        return Board.Regions[region].FindEmptyCardStack(isTitan, reverseSearch);
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
        BaseTraitEffect? traitCause)
    {
        var cardTemplate = RulesetParser.GetCardTemplate(cardTemplateId, 0);
        if (cardTemplate == null)
        {
            return false;
        }

        var isTitan = cardTemplate.Type == CardType.Titan;
        var reverseSearch = cardTemplate.IsSupportUnit();
        var cardStack = FindCardStackForSummon(playerIndex, isTitan, reverseSearch, currentRegion, targetableArea);
        if (cardStack == null)
        {
            return false;
        }

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
            if (slots[i].PrimaryCard != null && slots[i].PrimaryCard!.EqualsTo(card))
            {
                indexSlot = (sbyte) i;
            }
        }

        var cardSummonEvent = new CardTransitionCcgEvent(CcgEventType.CardSummon,
            card.InstanceId, playerIndex, 0, 0, false, currentRegion, indexSlot, 1);
        AddCcgEventLog(cardSummonEvent);
        if (traitCause != null)
        {
            cardSummonEvent.EffectId = traitCause.EffectTraitId;
            cardSummonEvent.TraitId = traitCause.TraitParentId;
        }

        return true;
    }

    public bool CanDoInitialSwap(sbyte playerIndex, int[] cardIdsToSwap)
    {
        if (playerIndex < 0 || playerIndex >= Players.Length)
        {
            return false;
        }

        var player = Players[playerIndex];
        if (player.Surrender || player.InitialCardsSwapped)
        {
            return false;
        }

        var hand = player.Hand;
        var discardCount = cardIdsToSwap.Length;
        if (discardCount > hand.Cards.Count || discardCount > _gameRules.MulliganDiscard)
        {
            return false;
        }

        foreach (var cardId in cardIdsToSwap)
        {
            if (hand.FindCard(cardId) == null)
            {
                return false;
            }
        }

        return true;
    }

    public bool DoInitialSwap(sbyte playerIndex, int[] cardIdsToSwap, int[] newDeckIndices)
    {
        var player = Players[playerIndex];
        var hand = player.Hand;
        var deck = player.Deck;
        var discardedCards = new Card[cardIdsToSwap.Length];
        for (var i = 0; i < cardIdsToSwap.Length; i++)
        {
            discardedCards[i] = hand.RemoveCard(cardIdsToSwap[i])!;
        }

        if (cardIdsToSwap.Length > 0)
        {
            var mulliganDrawEvent = new MulliganDrawCcgEvent(playerIndex);
            for (var j = 0; j < cardIdsToSwap.Length; j++)
            {
                var card = hand.DrawFromDeck(deck, this, playerIndex);
                if (card != null)
                {
                    mulliganDrawEvent.AddDrawnCard(card);
                }
            }

            AddCcgEventLog(mulliganDrawEvent);
        }

        var count = deck.Cards.Count;
        for (var k = 0; k < discardedCards.Length; k++)
        {
            deck.InsertCardAtIndex(index: newDeckIndices[k] = Random.Shared.Next(0, count + k), card: discardedCards[k]);
        }

        player.InitialCardsSwapped = true;
        var canStartGame = true;
        foreach (var loopPlayer in Players)
        {
            if (!loopPlayer.InitialCardsSwapped)
            {
                canStartGame = false;
            }
        }

        if (canStartGame)
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
        if (playerIndex < 0 || playerIndex >= Players.Length)
        {
            return false;
        }

        var player = Players[playerIndex];
        if (player.Surrender)
        {
            return false;
        }

        var hand = player.Hand;
        foreach (var cardId in cardIdsToDiscard)
        {
            if (hand.FindCard(cardId) == null)
            {
                return false;
            }
        }

        return true;
    }

    public bool DoCardDiscard(sbyte playerIndex, int[] cardIdsToDiscard)
    {
        var hand = Players[playerIndex].Hand;
        foreach (var cardId in cardIdsToDiscard)
        {
            var card = hand.RemoveCard(cardId);
            card?.Discard(Players);
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

        if (cardTemplateId == 0)
        {
            return true;
        }

        var cardTemplate = RulesetParser.GetCardTemplate(cardTemplateId, (sbyte) rank);
        if (cardTemplate == null)
        {
            return true;
        }

        var card = cardTemplate.GenerateCard(this);
        card.InstanceId = GetNextSummonInstanceId();
        card.Setup();
        card.ActiveData.Owner = playerIndex;
        player.Hand.Cards.Add(card);
        Logger.Debug("**** CCG.GiveCardAndCmdPts - Spanwed New Card * " + card.InstanceId);

        return true;
    }

    public bool CanTriggerEndTurnTraits(sbyte playerIndex)
    {
        if (PlayerTurn != playerIndex)
        {
            return false;
        }

        var player = Players[playerIndex];
        if (!player.CanTriggerEndTurnTraits())
        {
            return false;
        }

        return true;
    }

    public bool TriggerEndTurnTraits(sbyte playerIndex)
    {
        var turnChangeEvent = new TurnChangeCcgEvent(CcgEventType.EndTurn, playerIndex);
        AddCcgEventLog(turnChangeEvent);
        var player = Players[playerIndex];
        if (!player.TriggerEndTurnTraits())
        {
            return false;
        }

        Board.EndTurn(playerIndex);
        foreach (var loopPlayer in Players)
        {
            loopPlayer.Commander.EndTurn(playerIndex);
        }

        Board.CheckDiscards(Players);
        return true;
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
        if (!player.EndTurn())
        {
            return false;
        }

        var hand = Players[playerIndex].Hand;
        foreach (var cardId in cardIdsToDiscard)
        {
            var card = hand.RemoveCard(cardId);
            card?.Discard(Players);
        }

        var nextPlayerIndex = GetNextPlayerIndex(playerIndex);
        StartNewTurn(nextPlayerIndex);
        return true;
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
            GameTemplate.CheckEndGame(this);
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
            GameTemplate.CheckEndGame(this);
            return true;
        }

        return false;
    }

    public void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        Board.CardMoved(card, target, region, origin);
        foreach (var player in Players)
        {
            player.Commander.CardMoved(card, target, region, origin);
        }
    }

    public void CardAttacked(Card attacker, Card target)
    {
        Board.CardAttacked(attacker, target);
        foreach (var player in Players)
        {
            player.Commander.CardAttacked(attacker, target);
        }
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        Board.CardCounterAttacked(attacker, target);
        foreach (var player in Players)
        {
            player.Commander.CardCounterAttacked(attacker, target);
        }
    }

    public void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType)
    {
        Board.CardGainedStatus(theCard, source, statusType);
        foreach (var player in Players)
        {
            player.Commander.CardGainedStatus(theCard, source, statusType);
        }
    }

    public void CardDamaged(Card damangedCard, Card source)
    {
        Board.CardDamaged(damangedCard, source);
        foreach (var player in Players)
        {
            player.Commander.CardDamaged(damangedCard, source);
        }
    }

    public void CardDied(Card deadCard, Card source)
    {
        Board.CardDied(deadCard, source);
        foreach (var player in Players)
        {
            player.Commander.CardDied(deadCard, source);
        }
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        Board.CardDrawn(drawnCard, regularDraw, isNewTurn);
        foreach (var player in Players)
        {
            player.Commander.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        Board.CardDiscardEffect(playerIndex, numberOfCards);
        foreach (var player in Players)
        {
            player.Commander.CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public void SecretTriggered(Card secret, Card? source)
    {
        Board.SecretTriggered(secret, source);
        foreach (var player in Players)
        {
            player.Commander.SecretTriggered(secret, source);
        }
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        Board.SecretDestroyed(secret, source);
        foreach (var player in Players)
        {
            player.Commander.SecretDestroyed(secret, source);
        }
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack? target, Region region)
    {
        Board.TraitEffectActivating(effect, source, target, region);
        foreach (var player in Players)
        {
            player.Commander.TraitEffectActivating(effect, source, target, region);
        }
    }

    public bool CanSurrender(sbyte playerIndex)
    {
        return PlayerTurn >= 0 && !Players[playerIndex].Surrender;
    }

    public bool Surrender(sbyte playerIndex)
    {
        var player = Players[playerIndex];
        player.Surrender = true;
        var players = Players.Length;
        var surrenderCount = 0;
        for (var i = 0; i < players; i++)
        {
            if (Players[i].Surrender)
            {
                surrenderCount++;
            }
        }

        if (surrenderCount == players - 1)
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
        return PlayerTurn >= 0 && !Players[playerIndex].Surrender;
    }

    public sbyte GetNextPlayerIndex(sbyte playerIndex)
    {
        if (SurrenderGameOver)
        {
            return GameOverIndicator;
        }

        var playerCount = Players.Length;
        var nextIndex = playerIndex;
        do
        {
            nextIndex++;
            if (nextIndex == playerCount)
            {
                nextIndex = 0;
            }
        } while (nextIndex != playerIndex && !GameTemplate.IsActive(nextIndex, this));

        if (nextIndex != playerIndex)
        {
            return nextIndex;
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

    public void AddCcgEventLog(CcgEventData logData)
    {
        _ccgEventsLog.Add(logData);
    }

    public List<CcgEventData> GetCcgEventLog()
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
            AddCcgEventLog(logData);
            Players[PlayerTurn].NewTurn(PlayerTurn, GetDrawCount());
            Board.NewTurn(PlayerTurn);
            foreach (var player in Players)
            {
                player.Commander.NewTurn(playerIndex);
            }
        }

        Board.CheckDiscards(Players);
        GameTemplate.CheckEndGame(this);
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
        var firstRound = CurrentRound == 0;
        var firstPlayer = PlayerTurn == 0;
        if (firstRound && firstPlayer)
        {
            return _gameRules.FirstTurnDrawFirstPlayer;
        }

        if (firstRound && !firstPlayer)
        {
            return _gameRules.FirstTurnDrawOtherPlayer;
        }

        return _gameRules.NewTurnDraw;
    }
}