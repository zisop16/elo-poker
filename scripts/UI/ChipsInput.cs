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
    public override void _Ready() {
        TextSubmitted += HandleTextSizing;
    }
    public void HandleTextSizing(string newText) {
        Player localPlayer = Global.LocalGame.Players[Global.LocalSeat];
        int stack = localPlayer.Stack;
        int potSize = Global.LocalGame.TotalPot;
        int currentInvested = localPlayer.InvestedThisStreet;
        int currentBetSize = Global.LocalGame.CurrentBetSize;
        int chipsToCall = currentBetSize - currentInvested;
        int potAfterCall = potSize + chipsToCall;
        int minRaise = currentBetSize + Global.LocalGame.CurrentRaiseSize;
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
                        int totalAmount = (int)Math.Round(percent / 100 * potAfterCall);
                        ChipsValue = totalAmount;
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