using Godot;
using System;

public partial class PokerUI : Control {
    [Signal]
    public delegate void PokerActionEventHandler(Poker.Action action, uint amount);

    HBoxContainer BoardContainer;
    int LastDrawn;
    PhysicalCard[] Board = new PhysicalCard[5];
    Button CallButton;
    Button BetButton;
    Button FoldButton;
    Button CheckButton;

    public override void _Ready() {
        CallButton = GetNode<Button>("%CallButton");
        CallButton.ButtonDown += Call;
        BetButton = GetNode<Button>("%BetButton");
        BetButton.ButtonDown += Bet;
        FoldButton = GetNode<Button>("%FoldButton");
        FoldButton.ButtonDown += Fold;
        CheckButton = GetNode<Button>("%CheckButton");
        CheckButton.ButtonDown += Check;

        BoardContainer = GetNode<HBoxContainer>("BoardContainer");
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
        EmitSignal(SignalName.PokerAction, (byte)Poker.Action.CALL, 0);
    }
    void Bet() {
        uint amount = 50;
        EmitSignal(SignalName.PokerAction, (byte)Poker.Action.BET, amount);
    }
    void Check() {
        EmitSignal(SignalName.PokerAction, (byte)Poker.Action.CHECK, 0);
    }
    void Fold() {
        EmitSignal(SignalName.PokerAction, (byte)Poker.Action.FOLD, 0);
    }
}
