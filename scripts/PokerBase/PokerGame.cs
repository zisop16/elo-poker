using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using HoldemPoker.Cards;
using HoldemPoker.Evaluator;

public enum Action { CHECK, CALL, FOLD, BET };
public enum GameType {HOLDEM, OMAHA};
public enum Street {PREFLOP, FLOP, TURN, RIVER, ALL_FOLDED, SHOWDOWN};

struct Hand {
    public PokerCard[] Cards;
}

struct Player {
    public int Invested;
    public int Stack;
    public bool Folded;
    public bool SittingIn;
    public Hand Hand;
}

public class PokerGame {
    public Deck Deck;
    TableSettings Settings;
    PokerCard[] Board;
    Player[] Players;
    List<int> DealtInPlayers;
    List<int> DealtInPlayersLastHand;
    List<int> RemainingPlayers;
    int NumDealtPlayers { get => DealtInPlayers.Count; }
    int CurrentRaiseSize;
    int CurrentBetSize;
    int MaxInvested { get => Players.Max(p => p.Invested); }
    int TotalPot { get => Players.Sum(p => p.Invested); }
    int PlayersLeftToAct;
    public int ActivePlayers {
        get {
            int total = 0;
            foreach (Player curr in Players) {
                int stack = curr.Stack;
                if (stack == 0) continue;
                total += 1;
            }
            return total;
        }
    }
    bool HandActive;
    bool InitialHand;
    ModInt ButtonPosition;
    ModInt SmallBlindPosition {
        get {
            if (NumDealtPlayers == 2) {
                return ButtonPosition;
            } else {
                return ButtonPosition + 1;
            }
        }
    }
    ModInt BigBlindPosition { get => SmallBlindPosition + 1; }
    ModInt ActingPosition;
    Street Street;

    public PokerGame(TableSettings settings) {
        Settings = settings;
        Deck = new Deck();
        Board = new PokerCard[5];
        Players = new Player[Settings.NumPlayers];
        for (int i = 0; i < Settings.NumPlayers; i++) {
            Players[i].Stack = Settings.StartStack;
            Players[i].SittingIn = true;
        }
        DealtInPlayers = [];
        RemainingPlayers = [];
        InitialHand = true;
        HandActive = false;
    }

    ModInt NextPositionLastHand(ModInt from) {
        do {
            from++;
        } while (!DealtInPlayersLastHand.Contains(from.Val));
        return from;
    }
    /// <summary>
    /// Deals hands to players and posts blinds
    /// Assumes that there are at least 2 ActivePlayers
    /// </summary>
    public void Deal() {
        Deck.Shuffle();
        DealtInPlayers.Clear();
        RemainingPlayers.Clear();
        HandActive = true;
        for (int i = 0; i < Settings.NumPlayers; i++) {
            if ((!Players[i].SittingIn) || Players[i].Stack == 0) {
                continue;
            }
            PokerCard c1 = Deck.Draw();
            PokerCard c2 = Deck.Draw();
            Players[i].Hand.Cards = [c1, c2];
            Players[i].Invested = 0;
            Players[i].Folded = false;
            DealtInPlayers.Add(i);
            RemainingPlayers.Add(i);
        }
        foreach (int i in DealtInPlayers) {
            Invest(new ModInt(i, Settings.NumPlayers), Settings.Ante);
        }
        Invest(SmallBlindPosition, Settings.SmallBlind);
        Invest(BigBlindPosition, Settings.BigBlind);
        if (InitialHand) {
            ButtonPosition = new ModInt(DealtInPlayers[RandomNumberGenerator.GetInt32(NumDealtPlayers)], Settings.NumPlayers);
            DealtInPlayersLastHand = [.. DealtInPlayers];
        } else {
            ButtonPosition = NextPositionLastHand(ButtonPosition);
        }
        ActingPosition = SmallBlindPosition + 2;
        CurrentRaiseSize = Settings.BigBlind;
        CurrentBetSize = Settings.BigBlind;
        PlayersLeftToAct = NumDealtPlayers;
        Street = Street.PREFLOP;
    }
    /// <summary>
    /// Invest an amount of chips from a given position into the pot
    /// </summary>
    /// <param name="position"></param>
    /// <param name="amount"></param>
    /// <returns>Number of remaining chips that could not be invested, if shortstacked</returns>
    int Invest(ModInt position, int amount) {
        int p = position.Val;
        int investable = Math.Min(Players[p].Stack, amount);
        Players[position.Val].Invested = investable;
        Players[p].Stack -= investable;
        return amount - investable;
    }
    /// <summary>
    /// Calculate (pot sizes, investAmounts)
    /// </summary>
    /// <returns>Array of pot sizes. Index i is the number of chips in the i'th sidepot as (totalSize, amountPerPlayer). The "main" pot is sidepot 0</returns>
    (int, int)[] PotSizes() {
        HashSet<int> uniqueInvestAmounts = [];
        foreach (int i in DealtInPlayers) {
            if (Players[i].Folded) continue;
            if (Players[i].Invested == 0) continue;
            uniqueInvestAmounts.Add(Players[i].Invested);
        }
        int[] sortedUniqueInvests = uniqueInvestAmounts.ToArray<int>();
        Array.Sort(sortedUniqueInvests);
        for (int i = 0; i < sortedUniqueInvests.Count() - 1; i++) {
            for (int j = i + 1; j < sortedUniqueInvests.Count(); j++) {
                sortedUniqueInvests[j] -= sortedUniqueInvests[i];
            }
        }
        int[] invests = (int[])Players.Select(p => p.Invested);
        (int, int)[] sizes = new (int, int)[sortedUniqueInvests.Length];
        for (int i = 0; i < sortedUniqueInvests.Length; i++) {
            int currInvestAmount = sortedUniqueInvests[i];
            int currSize = 0;
            foreach (int p in DealtInPlayers) {
                int investable = Math.Min(invests[p], currInvestAmount);
                invests[p] -= investable;
                currSize += investable;
            }
            sizes[i] = (currSize, currInvestAmount);
        }
        return sizes;
    }


    bool LegalBet(int amount) {
        if (amount <= 0) {
            return false;
        }
        if (amount > Players[ActingPosition.Val].Stack) {
            return false;
        }
        int raiseSize = amount - CurrentBetSize;
        if (raiseSize == Players[ActingPosition.Val].Stack) {
            return true;
        }
        if (raiseSize < CurrentRaiseSize) {
            return false;
        }
        return true;
    }

    /// <param name="pos"></param>
    /// <returns>Whether the player is allin</returns>
    bool IsAllIn(int pos) {
        if (!DealtInPlayers.Contains(pos)) {
            return false;
        }
        return Players[pos].Stack == 0;
    }

    bool LegalCall() {
        return Players[ActingPosition.Val].Invested < MaxInvested;
    }

    bool LegalCheck() {
        return Players[ActingPosition.Val].Invested == MaxInvested;
    }
    void SetNextActor() {
        while (true) {
            ActingPosition++;
            if (IsAllIn(ActingPosition.Val)) continue;
            if (Players[ActingPosition.Val].Folded) continue;
            if (RemainingPlayers.Contains(ActingPosition.Val)) continue;
            break;
        }
    }
    int DetermineHandRanking(int playerInd) {
        Player p = Players[playerInd];
        Card[] cards = PokerCard.ToCards([.. p.Hand.Cards, .. Board]);
        int rank = HoldemHandEvaluator.GetHandRanking(cards);
        return rank;
    }

    int[] DetermineShowdownWinnings() {
        (int, int)[] potSizes = PotSizes();
        int[] invests = (int[])Players.Select(p => p.Invested);
        List<int> removed = [];
        int[] awardedAmounts = new int[Settings.NumPlayers];
        foreach ((int currPotSize, int currInvestAmount) in potSizes) {
            bool first = true;
            int minRank = 0;
            List<int> winningPlayers = [];
            for (int i = RemainingPlayers.Count - 1; i >= 0; i--) {
                int p = RemainingPlayers[i];
                if (first) {
                    first = false;
                    minRank = DetermineHandRanking(p);
                    winningPlayers.Add(p);
                } else {
                    int rank = DetermineHandRanking(p);
                    if (rank == minRank) {
                        winningPlayers.Add(p);
                    } else if (rank < minRank) {
                        winningPlayers = [p];
                    }
                }
                invests[p] -= currInvestAmount;
                if (invests[p] == 0) {
                    removed.Add(i);
                }
            }
            int numWinners = winningPlayers.Count;
            int potShare = currPotSize / numWinners;
            int remainder = currPotSize % numWinners;
            foreach (int p in winningPlayers) { awardedAmounts[p] += potShare; }
            ModInt remainderTaker = SmallBlindPosition;
            while (remainder > 0) {
                while (!winningPlayers.Contains(remainderTaker.Val)) { remainderTaker++; }
                awardedAmounts[remainderTaker.Val] += 1;
                remainderTaker++;
                remainder--;
            }

            foreach (int r in removed) {
                RemainingPlayers.RemoveAt(r);
            }
        }
        return awardedAmounts;
    }
    /// <summary>
    /// Advances the game by an action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="amount"></param>
    /// <returns>Whether the action was legal</returns>
    public bool Act(Action action, int amount = 0) {
        switch (action) {
            case Action.BET:
                if (!LegalBet(amount)) return false;
                Bet(amount);
                break;
            case Action.CALL:
                if (!LegalCall()) return false;
                Call();
                break;
            case Action.CHECK:
                if (!LegalCheck()) return false;
                Check();
                break;
            case Action.FOLD:
                Fold();
                break;
        }
        if (PlayersLeftToAct > 0) { PlayersLeftToAct -= 1; }
        bool betsMatched = false;
        if (RemainingPlayers.Count == 1) {
            int remainingPlayer = RemainingPlayers[0];
            int[] winnings = new int[Settings.NumPlayers];
            winnings[remainingPlayer] = TotalPot;
            AwardWinnings(winnings);
            Street = Street.ALL_FOLDED;
            return true;
        }
        int allInPlayers = 0;
        foreach (int pos in RemainingPlayers) {
            if (IsAllIn(pos)) {
                allInPlayers += 1;
                continue;
            }
            if (Players[pos].Invested == MaxInvested) continue;
            betsMatched = true;
            break;
        }
        if (!(betsMatched && PlayersLeftToAct == 0)) {
            SetNextActor();
        } else {
            switch (Street) {
                case Street.PREFLOP:
                    Street = Street.FLOP;
                    Board[0] = Deck.Draw();
                    Board[1] = Deck.Draw();
                    Board[2] = Deck.Draw();
                    break;

                case Street.FLOP:
                    Street = Street.TURN;
                    Board[3] = Deck.Draw();
                    break;

                case Street.TURN:
                    Street = Street.RIVER;
                    Board[4] = Deck.Draw();
                    break;

                case Street.RIVER:
                    Street = Street.SHOWDOWN;
                    int[] winnings = DetermineShowdownWinnings();
                    AwardWinnings(winnings);
                    break;
            }
            if (Street != Street.SHOWDOWN) {
                CurrentBetSize = 0;
                CurrentRaiseSize = 2;
                ActingPosition = BigBlindPosition;
                SetNextActor();
                PlayersLeftToAct = RemainingPlayers.Count - allInPlayers;
            }
        }
        HandleAllinPot();
        return true;
    }
    void Bet(int amount) {
        int raiseSize = amount - CurrentBetSize;
        Invest(ActingPosition, raiseSize);
        CurrentRaiseSize = raiseSize;
        CurrentBetSize = amount;
    }
    void Call() {
        int callAmount = CurrentBetSize - Players[ActingPosition.Val].Invested;
        Invest(ActingPosition, callAmount);
    }
    void Fold() {
        Players[ActingPosition.Val].Folded = true;
        RemainingPlayers.Remove(ActingPosition.Val);
    }
    void Check() {
    }

    void HandleAllinPot() {
        if (!PotIsAllIn()) { return; }
        int remainingCards = 0;
        switch (Street) {
            case Street.PREFLOP:
                remainingCards = 5;
                break;
            case Street.FLOP:
                remainingCards = 2;
                break;
            case Street.TURN:
                remainingCards = 1;
                break;
            case Street.RIVER:
                remainingCards = 0;
                break;
        }
        Street = Street.SHOWDOWN;
        for (int i = 5 - remainingCards; i < remainingCards; i++) {
            Board[i] = Deck.Draw();
        }
        int[] winnings = DetermineShowdownWinnings();
        AwardWinnings(winnings);
    }


    bool PotIsAllIn() {
        int nonAllInPlayer = 0;
        int nonAllInCount = 0;
        foreach (int p in RemainingPlayers) {
            if (!IsAllIn(p)) {
                nonAllInCount += 1;
                if (nonAllInCount == 2) { return false; }
                nonAllInPlayer = p;
            }
        }
        if (nonAllInCount == 0) { return true; }
        return Players[nonAllInPlayer].Invested == MaxInvested;
    }

    void AwardWinnings(int[] winnings) {
        for (int i = 0; i < Settings.NumPlayers; i++) {
            Players[i].Stack += winnings[i];
        }
        HandActive = false;
    }

    public override string ToString() {
        string Stacks = "";
        for (int i = 0; i < Settings.NumPlayers; i++) {

        }
    }
}