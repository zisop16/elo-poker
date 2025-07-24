using Godot;
using System;

public partial class BoardContainer : HBoxContainer {
    PhysicalCard[] Board = new PhysicalCard[PokerGame.BOARD_SIZE];
    int NumCardsOnBoard = 0;

    public override void _Ready() {
        Godot.Collections.Array<Node> board = GetChildren();
        for (int i = 0; i < board.Count; i++) {
            Board[i] = (PhysicalCard)board[i];
        }
        CallDeferred(MethodName.AfterReady);
    }
    void AfterReady() {
        Global.Lobby.JoinedLobby += ClearBoard;
        Global.Lobby.BoardChange += OnBoardChange;
    }

    void OnBoardChange() {
        while (NumCardsOnBoard < Global.LocalGame.NumCardsOnBoard) {
            PhysicalCard currPhys = Board[NumCardsOnBoard];
            PokerCard currPoker = Global.LocalGame.Board[NumCardsOnBoard];
            currPhys.Visible = true;
            currPhys.Card = currPoker;
            NumCardsOnBoard++;
        }
    }

    void ClearBoard() {
        foreach (PhysicalCard card in Board) {
            card.Visible = false;
        }
        NumCardsOnBoard = 0;
    }
}
