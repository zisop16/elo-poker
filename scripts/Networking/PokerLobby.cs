using System.Collections.Generic;
using Godot;

public partial class PokerLobby : Node {
    TableSettings Settings;
    PokerGame Game;
    int[] ConnectedPlayers;

    public PokerLobby(TableSettings settings, int[] players) {
        Settings = settings;
        Game = new PokerGame(Settings);
        ConnectedPlayers = players;
    }
    
}