using Godot;
using System;

public partial class PotAmount : Label {
    int Chips {
        set {
            Text = value + " chips";
        }
    }
    public override void _Ready() {
        var subscribeEvent = () => Global.Lobby.JoinedLobby += OnJoinedLobby;
        CallDeferred(MethodName.AfterReady);
    }
    void AfterReady() {
        Global.Lobby.JoinedLobby += OnJoinedLobby;
        Global.Lobby.PokerAction += OnAction;
    }
    void UpdateChips() {
        Chips = Global.LocalGame.TotalPot;
    }
    public void OnJoinedLobby() {
        UpdateChips();
    }
    public void OnAction() {
        UpdateChips();
    }
}
