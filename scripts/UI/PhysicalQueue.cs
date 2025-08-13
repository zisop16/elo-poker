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

    void ToggleQueue() {
        if (InQueue) {
            ExitQueue();
        } else {
            JoinQueue();
        }
    }

    void JoinQueue() {
        Global.Client.JoinQueue();
    }

    public void RegisterJoin() {
        QueueLabel.Text = "In Queue";
        InQueue = true;
        GD.Print("Joined Queue");
    }

    void ExitQueue() {
        Global.Client.ExitQueue();
    }

    public void RegisterExit() {
        QueueLabel.Text = "Not In Queue";
        InQueue = false;
        GD.Print("Left Queue");
    }
}
