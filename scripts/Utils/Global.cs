using System;
using Godot;

public static class Global {
    public static double Time { get => Godot.Time.GetTicksUsec() / 1000000.0; }
    public static NetworkClient Client;
    public static int SeatNumber { get => Client.Lobby.SeatNumber; }
}