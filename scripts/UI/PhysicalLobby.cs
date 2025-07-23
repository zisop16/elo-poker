using Godot;
using Poker;
using System;
using System.Collections.Generic;

public partial class PhysicalLobby : Control {

    NetworkClient Client { get => GetParent<NetworkClient>(); }

    HBoxContainer BoardContainer;
    int LastDrawn;
    PhysicalCard[] Board = new PhysicalCard[5];
    Button CallButton;
    Button BetButton;
    Button FoldButton;
    Button CheckButton;
    TextEdit ChipsInput;

    public override void _Ready() {
        CallButton = GetNode<Button>("%CallButton");
        CallButton.ButtonDown += Call;
        BetButton = GetNode<Button>("%BetButton");
        BetButton.ButtonDown += Bet;
        FoldButton = GetNode<Button>("%FoldButton");
        FoldButton.ButtonDown += Fold;
        CheckButton = GetNode<Button>("%CheckButton");
        CheckButton.ButtonDown += Check;
        ChipsInput = GetNode<TextEdit>("%ChipsInput");

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
        Client.HandleAction(Poker.Action.CALL);
    }
    void Bet() {
        int amount = 50;
        Client.HandleAction(Poker.Action.BET, amount);
    }
    void Check() {
        Client.HandleAction(Poker.Action.CHECK);
    }
    void Fold() {
        Client.HandleAction(Poker.Action.FOLD);
    }

    public void HandleLobbyStart(LobbyStartPacket pack) {
        Hand[] hands = pack.Hands;
        int[] ids = pack.PlayerIDs;
        TableSettings settings = pack.Settings;
        PokerGame game = new(settings, ids);
        GD.Print("Joined Lobby");
    }
}
