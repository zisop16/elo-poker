using System.Collections.Generic;
using Godot;
using Poker;

public class NetworkLobby {
    readonly TableSettings Settings;
    public readonly PokerGame Game;

    public int BigBlindPosition { get => Game.BigBlindPosition.Val; }
    public int ButtonPosition { get => Game.ButtonPosition.Val; }
    public int SmallBlindPosition { get => Game.SmallBlindPosition.Val; }
    public int ActingPosition { get => Game.ActingPosition.Val; }
    

    int[] ConnectedPlayers;
    Street Street;
    int NumCardsOnBoard = 0;

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
        if (Game.Street == Street.SHOWDOWN) {
            // do something
        } else {
            Street = Street.PREFLOP;
            NumCardsOnBoard = 0;
        }
    }

    public bool Bet(int amount) {
        bool success = Game.Bet(amount);
        return success;
    }
    public bool Fold() {
        bool success = Game.Fold();
        return success;
    }
    public bool Call() {
        bool success = Game.Call();
        return success;
    }
    public bool Check() {
        bool success = Game.Check();
        return success;
    }
    void HandleStreetChange() {
        if (Game.Street == Street) return;
        int cardsDrawn = Game.NumCardsOnBoard - NumCardsOnBoard;
        Street = Game.Street;
        if (Street == Street.SHOWDOWN) {
            HandleShowdown();
        } else if (Street == Street.ALL_FOLDED) {
            HandleAllFolded();
        }
    }
    void HandleShowdown() {

    }
    void HandleAllFolded() {

    }
}