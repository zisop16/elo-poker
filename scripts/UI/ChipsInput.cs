using Godot;
using Poker;
using System;

public partial class ChipsInput : LineEdit {
    int _chipsValue = 0;
    public int ChipsValue {
        get => _chipsValue;
        set {
            _chipsValue = value;
            Text = "" + _chipsValue;
        }
    }
    PokerGame Game { get => Global.Client.Lobby.Game; }
    public override void _Ready() {
        TextSubmitted += HandleTextSizing;
    }
    public void HandleTextSizing(string newText) {
        Player localPlayer = Game.Players[Global.LocalSeat];
        int stack = localPlayer.Stack;
        int potSize = Game.TotalPot;
        int currentInvested = localPlayer.InvestedThisStreet;
        int currentBetSize = Game.CurrentBetSize;
        int chipsToCall = currentBetSize - currentInvested;
        int potAfterCall = potSize + chipsToCall;
        int minRaise = currentBetSize + Game.CurrentRaiseSize;
        bool foundSize = false;
        if (int.TryParse(newText, out int value)) {
            ChipsValue = value;
            foundSize = true;
        } else {
            newText = newText.ToLower();
            newText = newText.RemoveWhitespace();
            switch (newText) {
                case "pot":
                case "pawt":
                    ChipsValue = potAfterCall * 2 - potSize;
                    foundSize = true;
                    break;
                case "allin":
                    ChipsValue = stack;
                    foundSize = true;
                    break;
            }
            int length = newText.Length;
            if (!foundSize) {
                if (newText[length - 1] == '%') {
                    bool validPercent = double.TryParse(newText.AsSpan(0, length - 1), out double percent);
                    if (validPercent) {
                        int additionalChips = (int)Math.Round(percent / 100 * potAfterCall);
                        ChipsValue = potAfterCall + additionalChips;
                        foundSize = true;
                    }
                }
            }
            if (!foundSize) {
                if (newText.EndsWith("bb")) {
                    bool validBBAmount = double.TryParse(newText.AsSpan(0, length - 2), out double bbAmount);
                    if (validBBAmount) {
                        int totalAmount = (int)Math.Round(Global.LocalGame.Settings.BigBlind * bbAmount);
                        ChipsValue = totalAmount;
                        foundSize = true;
                    }
                }
            }
        }
        if (foundSize) {
            if (ChipsValue > stack) ChipsValue = stack;
            if (ChipsValue < minRaise) ChipsValue = minRaise;
        }
    }
}