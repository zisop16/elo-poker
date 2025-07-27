using System.Collections.Generic;
using Godot;
using Poker;

public class ActionResult {
    public bool Success = false;
    public bool StreetChange { get => NewCards != null; }
    public PokerCard[] NewCards = null;
}

public class NetworkLobby {
    readonly TableSettings Settings;
    public readonly PokerGame Game;

    public int BigBlindPosition { get => Game.BigBlindPosition.Val; }
    public int ButtonPosition { get => Game.ButtonPosition.Val; }
    public int SmallBlindPosition { get => Game.SmallBlindPosition.Val; }
    public int ActingPosition { get => Game.ActingPosition.Val; }


    public readonly int[] ConnectedPlayers;
    Street Street;
    int NumCardsOnBoard = 0;
    int ActionIndex = 0;
    public int NextActionIndex { get => ActionIndex + 1; }

    public NetworkLobby(TableSettings settings, int[] playerIDs) {
        Settings = settings;
        Game = new PokerGame(Settings, playerIDs);
        ConnectedPlayers = playerIDs;
    }
    public bool PlayerIsActing(int PlayerID) {
        if (!Game.HandActive) return false;
        return Game.ActingPlayer == PlayerID;
    }
    public void Deal() {
        Game.Deal();
        Street = Street.PREFLOP;
        ActionResult result = new();
        HandleStreetChange(result);
        if (result.StreetChange) {
            NumCardsOnBoard = 0;
        }
    }

    public ActionResult Act(Action action, int amount) {
        ActionResult result = new();
        result.Success = Game.Act(action, amount);
        if (result.Success) {
            ActionIndex++;
            HandleStreetChange(result);
        }
        return result;
    }
    void HandleStreetChange(ActionResult result) {
        if (Game.Street == Street) return;
        int cardsDrawn = Game.NumCardsOnBoard - NumCardsOnBoard;
        Street = Game.Street;
        int ind = 0;
        result.NewCards = new PokerCard[cardsDrawn];
        for (int i = NumCardsOnBoard; i < Game.NumCardsOnBoard; i++) {
            PokerCard curr = Game.Board[i];
            result.NewCards[ind++] = curr;
        }
        if (Street == Street.SHOWDOWN) {
            HandleShowdown();
        } else if (Street == Street.ALL_FOLDED) {
            HandleAllFolded();
        }
        NumCardsOnBoard = Game.NumCardsOnBoard;
    }
    void HandleShowdown() {

    }
    void HandleAllFolded() {

    }
}