using Godot;
using System;

public partial class VActionContainer : VBoxContainer {
    public override void _Ready() {
        CallDeferred(MethodName.AfterReady);
    }
    void AfterReady() {
        Global.Lobby.JoinedLobby += OnJoinedLobby;
        Global.Lobby.PokerAction += OnPokerAction;
    }
    void OnJoinedLobby() {
        UpdateActionsVisibility();
    }
    void OnPokerAction() {
        UpdateActionsVisibility();
    }
    void UpdateActionsVisibility() {
        bool shouldShow = Global.ActionOnSelf;
        Visible = shouldShow;
    }
}
