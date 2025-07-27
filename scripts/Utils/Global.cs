using System;
using Godot;
using Poker;

public static class Global {
    public static double Time { get => Godot.Time.GetTicksUsec() / 1000000.0; }
    public static NetworkClient Client;
    public static PhysicalLobby Lobby { get => Client.Lobby; }
    public static PokerGame LocalGame { get => Client.Lobby.Game; }
    public static Player LocalPlayer { get => LocalGame.Players[LocalSeat]; }
    public static int LocalSeat { get => Client.Lobby.LocalSeat; }
    public static Hand LocalHand { get => LocalGame.Players[LocalSeat].Hand; }
    public static bool ActionOnSelf { get => LocalGame.ActingPosition == LocalSeat; }
    public static bool LocallyAllin { get => LocalGame.IsAllIn(LocalSeat); }
    public static TableLayout LocalLayout { get => GD.Load<TableLayout>(TableLayout.LAYOUT_PATH); }
}