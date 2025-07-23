using Godot;
using System;

public partial class PhysicalQueue : Control {
    Label QueueLabel;
    Button JoinButton;
    bool InQueue = false;
    public override void _Ready() {
        JoinButton = GetNode<Button>("%JoinButton");
        QueueLabel = GetNode<Label>("%QueueLabel");
        JoinButton.ButtonDown += JoinQueue;
    }

    void JoinQueue() {
        if (InQueue) return;
        Global.Client.JoinQueue();
        QueueLabel.Text = "In Queue";
        InQueue = true;
        GD.Print("Joined Queue");
    }
}
