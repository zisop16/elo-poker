using Godot;
using Poker;
using System;

public partial class PhysicalHand : Control {
    [Export]
    bool IsLocalHand;
    PhysicalCard Card1;
    PhysicalCard Card2;
    public override void _Ready() {
        Card1 = GetNode<PhysicalCard>("Card1");
        Card2 = GetNode<PhysicalCard>("Card2");
        CallDeferred(MethodName.AfterReady);
    }
    public void AfterReady() {
        Global.Lobby.JoinedLobby += OnJoinedLobby;
    }
    void OnJoinedLobby() {
        if (IsLocalHand) {
            Hand hand = Global.LocalHand;
            Card1.Card = hand.Card1;
            Card2.Card = hand.Card2;
        } else {

        }
    }
}
