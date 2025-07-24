using System;
using Godot;
using Poker;

public static class Global {
    public static double Time { get => Godot.Time.GetTicksUsec() / 1000000.0; }
    public static NetworkClient Client;
    public static PhysicalLobby Lobby { get => Client.Lobby; }
    public static PokerGame LocalGame { get => Client.Lobby.Game; }
    public static int LocalSeat { get => Client.Lobby.LocalSeat; }
    public static Hand LocalHand { get => LocalGame.Players[LocalSeat].Hand; }
}