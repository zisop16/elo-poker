using Godot;
using Poker;
using System;

[Tool]
[GlobalClass]
public partial class PhysicalHand : Control {
    bool _isLocalHand = false;
    [Export]
    bool IsLocalHand {
        get => _isLocalHand;
        set {
            _isLocalHand = value;
            // Card1 will be null when this node is first loaded
            if (Card1 == null) return;
            UpdateCards();
        }
    }
    PhysicalCard Card1;
    PhysicalCard Card2;
    public override void _Ready() {
        Card1 = GetNode<PhysicalCard>("PhysicalCard1");
        Card2 = GetNode<PhysicalCard>("PhysicalCard2");
        CallDeferred(MethodName.AfterReady);
    }
    public void AfterReady() {
        if (Engine.IsEditorHint()) return;
        Global.Lobby.JoinedLobby += UpdateCards;
        GD.Print("hi");
    }
    void UpdateCards() {
        GD.Print("hi2");
        if (IsLocalHand) {
            Card1.BackFacing = false;
            Card2.BackFacing = false;
            if (Engine.IsEditorHint()) return;
            Hand hand = Global.LocalHand;
            GD.Print(hand);
            Card1.Card = hand.Card1;
            Card2.Card = hand.Card2;
        } else {
            Card1.BackFacing = true;
            Card2.BackFacing = true;
        }
    }
}
