using Godot;
using Poker;
using System;

public partial class ChipsInput : LineEdit {
    int ChipsValue = 0;
    PokerGame Game { get => Global.Client.Lobby.Game; }
    public override void _Ready() {
        TextSubmitted += OnTextSubmit;
    }
    public void OnTextSubmit(string newText) {
        Player localPlayer = Game.Players[Global.SeatNumber];
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
            if (!foundSize) {
                int length = newText.Length;
                if (newText[length - 2] == '%') {
                    bool validPercent = double.TryParse(newText.AsSpan(0, length - 1), out double percent);
                    if (validPercent) {
                        int additionalChips = (int)Math.Round(percent * potAfterCall);
                        ChipsValue = potAfterCall + additionalChips;
                        foundSize = true;
                    }
                }
            }
        }
        if (foundSize) {
            if (ChipsValue > stack) ChipsValue = stack;
            if (ChipsValue < minRaise) ChipsValue = minRaise;
        }
        Text = "" + ChipsValue;
    }
}