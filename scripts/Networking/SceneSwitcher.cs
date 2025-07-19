using Godot;
using System;

public partial class SceneSwitcher : Node {
    [Export]
    PackedScene ServerScene;
    [Export]
    PackedScene ClientScene;

    public override void _Process(double delta) {
        if (OS.HasFeature("dedicated_server")) {
            GetTree().ChangeSceneToPacked(ServerScene);
        } else {
            GetTree().ChangeSceneToPacked(ClientScene);
        }
    }
}
