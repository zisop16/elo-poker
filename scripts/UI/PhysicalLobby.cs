using Godot;
using Poker;
using System;
using System.Collections.Generic;

public partial class PhysicalLobby : Control {
    [Signal]
    public delegate void JoinedLobbyEventHandler();
    [Signal]
    public delegate void PokerActionEventHandler();
    [Signal]
    public delegate void BoardChangeEventHandler();
    public PokerGame Game;
    HBoxContainer BoardContainer;
    int LastDrawn;
    public int ActionIndex = 0;
    public int LocalSeat { get; private set; }
    public Button CallButton { get; private set; }
    public Button BetButton { get; private set; }
    public Button FoldButton { get; private set; }
    public Button CheckButton { get; private set; }
    public ChipsInput ChipsInput { get; private set; }

    public override void _Ready() {
        CallButton = GetNode<Button>("%CallButton");
        CallButton.ButtonDown += Call;
        BetButton = GetNode<Button>("%BetButton");
        BetButton.ButtonDown += Bet;
        FoldButton = GetNode<Button>("%FoldButton");
        FoldButton.ButtonDown += Fold;
        CheckButton = GetNode<Button>("%CheckButton");
        CheckButton.ButtonDown += Check;
        ChipsInput = GetNode<ChipsInput>("%ChipsInput");
    }

    public void ReceiveAction(Poker.Action action, int amount, int actionIndex, PokerCard[] newlyDealtCards) {
        bool newCards = newlyDealtCards != null;
        if (newCards) {
            Game.ForceBoardCards(newlyDealtCards);
        }

        if (actionIndex > ActionIndex) {
            bool success = Game.Act(action, amount);
            if (!success) {
                Assert.That(false, "Client attempted to perform an illegal action");
                return;
            }
            ActionIndex++;
            if (newCards) {
                EmitSignal(SignalName.BoardChange);
            }
            EmitSignal(SignalName.PokerAction);
        } else {
            // Server has sent back the client's action along with drawn board cards
            EmitSignal(SignalName.BoardChange);
        }
    }

    void Act(Poker.Action action, int amount = 0) {
        bool success = Game.Act(action, amount);
        if (!success) {
            Assert.That(false, "Client attempted to perform an illegal action");
            return;
        }
        ActionIndex++;
        Global.Client.HandleAction(action, amount);
        EmitSignal(SignalName.PokerAction);
    }

    void Call() {
        Act(Poker.Action.CALL);
    }
    void Bet() {
        int amount = ChipsInput.ChipsValue;
        Act(Poker.Action.BET, amount);
    }
    void Check() {
        Act(Poker.Action.CHECK);
    }
    void Fold() {
        Act(Poker.Action.FOLD);
    }

    public void HandleLobbyStart(LobbyStartPacket pack) {
        Hand hand = pack.Hand;
        int[] ids = pack.PlayerIDs;
        TableSettings settings = pack.Settings;
        LocalSeat = pack.SeatNumber;
        Game = new(settings, ids);
        Tuple<int, Hand> forceHand = new(LocalSeat, hand);
        Game.Deal(forceHand, pack.BigBlindPosition);
        GD.Print("Joined Lobby");
        EmitSignal(SignalName.JoinedLobby);
    }
}
