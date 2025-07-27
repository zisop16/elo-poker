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
        TextSubmitted += HandleTextSubmit;
        TextChanged += HandleTextChange;
    }
    public void HandleTextSubmit(string newText) {
        int? size = DetermineChipsValue(newText);
        if (size == null) return;
        ChipsValue = size.Value;
    }
    public void HandleTextChange(string newText) {
        int? size = DetermineChipsValue(newText);
        if (size == null) return;
        _chipsValue = size.Value;
    }

    public int? DetermineChipsValue(string text) {
        int chips = 0;
        Player localPlayer = Global.LocalGame.Players[Global.LocalSeat];
        int stack = localPlayer.Stack;
        int potSize = Global.LocalGame.TotalPot;
        int currentInvested = localPlayer.InvestedThisStreet;
        int currentBetSize = Global.LocalGame.CurrentBetSize;
        int chipsToCall = currentBetSize - currentInvested;
        int potAfterCall = potSize + chipsToCall;
        int minRaise = currentBetSize + Global.LocalGame.CurrentRaiseSize;
        bool foundSize = false;
        if (int.TryParse(text, out int value)) {
            chips = value;
            foundSize = true;
        } else {
            text = text.ToLower();
            text = text.RemoveWhitespace();
            switch (text) {
                case "pot":
                case "pawt":
                    chips = potAfterCall * 2 - potSize;
                    foundSize = true;
                    break;
                case "allin":
                    chips = stack;
                    foundSize = true;
                    break;
            }
            int length = text.Length;
            if (!foundSize) {
                if (text[length - 1] == '%') {
                    bool validPercent = double.TryParse(text.AsSpan(0, length - 1), out double percent);
                    if (validPercent) {
                        int totalAmount = (int)Math.Round(percent / 100 * potAfterCall);
                        chips = totalAmount;
                        foundSize = true;
                    }
                }
            }
            if (!foundSize) {
                if (text.EndsWith("bb")) {
                    bool validBBAmount = double.TryParse(text.AsSpan(0, length - 2), out double bbAmount);
                    if (validBBAmount) {
                        int totalAmount = (int)Math.Round(Global.LocalGame.Settings.BigBlind * bbAmount);
                        chips = totalAmount;
                        foundSize = true;
                    }
                }
            }
        }
        if (foundSize) {
            if (chips > stack) chips = stack;
            if (chips < minRaise) chips = minRaise;
            return chips;
        } else {
            return null;
        }
    }
}