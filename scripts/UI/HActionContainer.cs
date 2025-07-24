using Godot;
using Poker;
using System;
using static Global;

public partial class HActionContainer : HBoxContainer {
    bool CanCall, CanCheck, CanBet, CanFold, CanRaise;
    public override void _Ready() {
        CallDeferred(MethodName.AfterReady);
    }
    void AfterReady() {
        Lobby.JoinedLobby += OnJoinedLobby;
        Lobby.PokerAction += OnAction;
    }
    void OnJoinedLobby() {
        DeterminePossibleActions();
        UpdateButtons();
    }
    void OnAction() {
        DeterminePossibleActions();
        UpdateButtons();
    }
    void DeterminePossibleActions() {
        bool allin = LocallyAllin;
        if (allin) {
            CanCall = CanCheck = CanBet = CanFold = CanRaise = false;
        } else {
            CanCall = true;
            if (LocalPlayer.InvestedThisStreet == LocalGame.CurrentBetSize) {
                CanCall = false;
            }
            CanCheck = !CanCall;
            CanBet = CanCheck;
            CanFold = !CanCheck;
            CanRaise = !CanBet;
        }
        // Lobby.CallButton.
    }
    void UpdateButtons() {
        Lobby.CallButton.Disabled = !CanCall;
        if (CanBet) {
            Lobby.BetButton.Text = "Bet";
        } else if (CanRaise) {
            Lobby.BetButton.Text = "Raise";
        }
        Lobby.BetButton.Disabled = !CanBet && !CanRaise;
        Lobby.FoldButton.Disabled = !CanFold;
        Lobby.CheckButton.Disabled = !CanCheck;
    }
}
