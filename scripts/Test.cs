using Godot;
using System;

public partial class Test : Node2D {
    PokerGame Game;
    [Export]
    TableSettings Settings;
    public override void _Ready() {
        Game = new PokerGame(Settings);
        Game.Deal();
    }
}
