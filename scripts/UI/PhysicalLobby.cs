using Godot;
using Poker;
using System;
using System.Collections.Generic;

public partial class PhysicalLobby : Control {
    [Signal]
    public delegate void JoinedLobbyEventHandler();
    [Signal]
    public delegate void PokerActionEventHandler();
    public PokerGame Game;
    HBoxContainer BoardContainer;
    int LastDrawn;
    public int LocalSeat { get; private set; }
    PhysicalCard[] Board = new PhysicalCard[5];
    Button CallButton;
    Button BetButton;
    Button FoldButton;
    Button CheckButton;
    ChipsInput ChipsInput;

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

        BoardContainer = GetNode<HBoxContainer>("%BoardContainer");
        Godot.Collections.Array<Node> board = BoardContainer.GetChildren();
        for (int i = 0; i < board.Count; i++) {
            Board[i] = (PhysicalCard)board[i];
        }
        ClearBoard();
    }

    void ClearBoard() {
        foreach (PhysicalCard card in Board) {
            // card.Visible = false;
        }
        LastDrawn = 0;
    }

    public void DrawToBoard(PokerCard card) {
        Board[LastDrawn].Card = card;
        Board[LastDrawn].Visible = true;
        LastDrawn++;
    }

    void Act(Poker.Action action, int amount = 0) {
        bool success = Game.Act(action, amount);
        if (!success) {
            Assert.That(false, "Client attempted to perform an illegal action");
            return;
        }
        Global.Client.HandleAction(action, amount);
        EmitSignal(SignalName.PokerAction);
    }

    void Call() {
        Act(Poker.Action.CALL);
    }
    void Bet() {
        int amount = 50;
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
