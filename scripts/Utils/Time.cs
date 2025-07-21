using System;
using Godot;

public static class Global {
    public static double Time { get => Godot.Time.GetTicksUsec() / 1000000.0; }
}