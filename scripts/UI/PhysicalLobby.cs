using Godot;
using Poker;
using System;
using System.Collections.Generic;

public partial class PhysicalLobby : Control {
    [Signal]
    public delegate void JoinedLobbyEventHandler();
    public PokerGame Game;
    HBoxContainer BoardContainer;
    int LastDrawn;
    public int SeatNumber { get; private set; }
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
            card.Visible = false;
        }
        LastDrawn = 0;
    }

    public void DrawToBoard(PokerCard card) {
        Board[LastDrawn].Card = card;
        Board[LastDrawn].Visible = true;
        LastDrawn++;
    }

    void Call() {
        Global.Client.HandleAction(Poker.Action.CALL);
    }
    void Bet() {
        int amount = 50;
        Global.Client.HandleAction(Poker.Action.BET, amount);
    }
    void Check() {
        Global.Client.HandleAction(Poker.Action.CHECK);
    }
    void Fold() {
        Global.Client.HandleAction(Poker.Action.FOLD);
    }

    public void HandleLobbyStart(LobbyStartPacket pack) {
        Hand hand = pack.Hand;
        int[] ids = pack.PlayerIDs;
        TableSettings settings = pack.Settings;
        SeatNumber = pack.SeatNumber;
        Game = new(settings, ids);
        GD.Print("Joined Lobby");
        EmitSignal(SignalName.JoinedLobby);
    }
}
