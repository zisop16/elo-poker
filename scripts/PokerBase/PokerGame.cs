using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using Godot;
using HoldemPoker.Cards;
using HoldemPoker.Evaluator;
using Poker;

namespace Poker {
    public enum Action : byte { CHECK, CALL, FOLD, BET };
    public enum GameType : byte { HOLDEM, OMAHA };
    public enum Street : byte { PREFLOP, FLOP, TURN, RIVER, ALL_FOLDED, SHOWDOWN };   
    public readonly struct Hand(PokerCard[] cards) {
        public readonly PokerCard[] Cards = cards;
        public override string ToString() {
            String cards = "";
            foreach (PokerCard curr in Cards) {
                cards += curr;
            }
            return cards;
        }
    }

}


struct Player {
    /// <summary>
    /// Total amount of money the player has invested in the hand
    /// </summary>
    public int Invested;
    /// <summary>
    /// Amount of (NON-ANTE) money the player has invested this street
    /// </summary>
    public int InvestedThisStreet;
    public int Stack;
    public bool Folded;
    public bool SittingIn;
    public Hand Hand;
    public string Name;
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
    int NumCardsOnBoard;
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
    ModInt ButtonPosition {
        get {
            if (NumDealtPlayers == 2) {
                return SmallBlindPosition;
            } else {
                return PrevPositionLastHand(SmallBlindPosition);
            }
        }
    }
    ModInt SmallBlindPosition { get => PrevPositionLastHand(BigBlindPosition); }
    ModInt BigBlindPosition;
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
            Players[i].Name = "Player " + i;
        }
        DealtInPlayers = [];
        RemainingPlayers = [];
        InitialHand = true;
        HandActive = false;
    }

    ModInt NextPosition(ModInt from) {
        do {
            from++;
        } while (!DealtInPlayers.Contains(from.Val));
        return from;
    }

    ModInt NextPositionLastHand(ModInt from) {
        do {
            from++;
        } while (!DealtInPlayersLastHand.Contains(from.Val));
        return from;
    }

    ModInt PrevPositionLastHand(ModInt from) {
        do {
            from--;
        } while (!DealtInPlayersLastHand.Contains(from.Val));
        return from;
    }

    void DrawBoardCard() {
        Board[NumCardsOnBoard] = Deck.Draw();
        NumCardsOnBoard += 1;
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
        NumCardsOnBoard = 0;
        for (int i = 0; i < Settings.NumPlayers; i++) {
            if ((!Players[i].SittingIn) || Players[i].Stack == 0) {
                continue;
            }
            PokerCard c1 = Deck.Draw();
            PokerCard c2 = Deck.Draw();
            if (c1.Type < c2.Type) {
                PokerCard temp = c1;
                c1 = c2;
                c2 = temp;
            }
            Players[i].Hand = new Hand([c1, c2]);
            Players[i].Invested = 0;
            Players[i].InvestedThisStreet = 0;
            Players[i].Folded = false;
            DealtInPlayers.Add(i);
            RemainingPlayers.Add(i);
        }
        foreach (int i in DealtInPlayers) {
            Invest(new ModInt(i, Settings.NumPlayers), Settings.Ante);
            Players[i].InvestedThisStreet = 0;
        }

        if (InitialHand) {
            BigBlindPosition = new ModInt(DealtInPlayers[System.Security.Cryptography.RandomNumberGenerator.GetInt32(NumDealtPlayers)], Settings.NumPlayers);
            DealtInPlayersLastHand = [.. DealtInPlayers];
            InitialHand = false;
        } else {
            BigBlindPosition = NextPosition(BigBlindPosition);
        }
        ActingPosition = NextPosition(BigBlindPosition);
        CurrentRaiseSize = Settings.BigBlind;
        CurrentBetSize = Settings.BigBlind;
        PlayersLeftToAct = NumDealtPlayers;
        Street = Street.PREFLOP;

        if (DealtInPlayers.Contains(SmallBlindPosition.Val)) {
            Invest(SmallBlindPosition, Settings.SmallBlind);
        }
        Invest(BigBlindPosition, Settings.BigBlind);
        if (PotIsAllIn()) {
            HandleAllinPot();
        }
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
        Players[p].Invested += investable;
        Players[p].Stack -= investable;
        Players[p].InvestedThisStreet += investable;
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
        int[] invests = [.. Players.Select(p => p.Invested)];
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
        return Players[ActingPosition.Val].InvestedThisStreet < CurrentBetSize;
    }

    bool LegalCheck() {
        return Players[ActingPosition.Val].InvestedThisStreet == CurrentBetSize;
    }

    void SetNextActor() {
        while (true) {
            ActingPosition = NextPosition(ActingPosition);
            if (Players[ActingPosition.Val].Folded) continue;
            if (IsAllIn(ActingPosition.Val)) continue;
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
        int[] invests = [.. Players.Select(p => p.Invested)];
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
    public bool Act(Poker.Action action, int amount = 0) {
        switch (action) {
            case Poker.Action.BET:
                if (!LegalBet(amount)) return false;
                InternalBet(amount);
                break;
            case Poker.Action.CALL:
                if (!LegalCall()) return false;
                InternalCall();
                break;
            case Poker.Action.CHECK:
                if (!LegalCheck()) return false;
                InternalCheck();
                break;
            case Poker.Action.FOLD:
                InternalFold();
                break;
        }
        if (PlayersLeftToAct > 0) { PlayersLeftToAct -= 1; }
        bool betsMatched = true;
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
            if (Players[pos].InvestedThisStreet != CurrentBetSize) {
                betsMatched = false;
                break;
            }
        }
        bool isAllin = PotIsAllIn();
        if (isAllin) {
            HandleAllinPot();
        } else if (!(betsMatched && PlayersLeftToAct == 0)) {
            SetNextActor();
        } else {
            switch (Street) {
                case Street.PREFLOP:
                    Street = Street.FLOP;
                    DrawBoardCard();
                    DrawBoardCard();
                    DrawBoardCard();
                    break;

                case Street.FLOP:
                    Street = Street.TURN;
                    DrawBoardCard();
                    break;

                case Street.TURN:
                    Street = Street.RIVER;
                    DrawBoardCard();
                    break;

                case Street.RIVER:
                    Street = Street.SHOWDOWN;
                    int[] winnings = DetermineShowdownWinnings();
                    AwardWinnings(winnings);
                    break;
            }
            if (Street != Street.SHOWDOWN) {
                CurrentBetSize = 0;
                CurrentRaiseSize = Settings.BigBlind;
                ActingPosition = ButtonPosition;
                SetNextActor();
                PlayersLeftToAct = RemainingPlayers.Count - allInPlayers;
                foreach (int p in DealtInPlayers) {
                    Players[p].InvestedThisStreet = 0;
                }
            }
        }
        return true;
    }
    public bool Bet(int amount) {
        return Act(Poker.Action.BET, amount);
    }
    public bool Call() {
        return Act(Poker.Action.CALL);
    }
    public bool Fold() {
        return Act(Poker.Action.FOLD);
    }
    public bool Check() {
        return Act(Poker.Action.CHECK);
    }

    void InternalBet(int amount) {
        int raiseSize = amount - CurrentBetSize;
        Invest(ActingPosition, amount - Players[ActingPosition.Val].InvestedThisStreet);
        CurrentRaiseSize = raiseSize;
        CurrentBetSize = amount;
    }
    void InternalCall() {
        int callAmount = CurrentBetSize - Players[ActingPosition.Val].InvestedThisStreet;
        Invest(ActingPosition, callAmount);
    }
    void InternalFold() {
        Players[ActingPosition.Val].Folded = true;
        RemainingPlayers.Remove(ActingPosition.Val);
    }
    void InternalCheck() {
    }

    void HandleAllinPot() {
        for (int i = NumCardsOnBoard; i < 5; i++) {
            DrawBoardCard();
        }
        int[] winnings = DetermineShowdownWinnings();
        AwardWinnings(winnings);
        Street = Street.SHOWDOWN;
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
        string state = "Table State:\n";
        for (int i = 0; i < Settings.NumPlayers; i++) {
            state += Players[i].Name + ": " + Players[i].Stack + " chips (" + Players[i].InvestedThisStreet + ")";
            if (HandActive && DealtInPlayers.Contains(i)) {
                state += "\nHand: " + Players[i].Hand;
                if (Players[i].Folded) { state += " (Folded)"; }
            }
            if ((i == Settings.NumPlayers - 1) && !HandActive) {
                break;
            } else {
                state += '\n';
            }
        }
        if (HandActive) {
            state += "Board: ";
            for (int i = 0; i < NumCardsOnBoard; i++) {
                state += Board[i];
                if (i != NumCardsOnBoard - 1) { state += " "; }
            }
            state += '\n';
            state += "Pot: " + TotalPot + '\n';
            state += "Current Active Bet: " + CurrentBetSize + '\n';
            state += "Action on: Seat " + ActingPosition.Val;
        }
        return state;
    }
}