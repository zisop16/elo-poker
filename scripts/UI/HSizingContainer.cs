using Godot;
using System;

[Tool]
[GlobalClass]
public partial class HSizingContainer : HBoxContainer {
    bool _preflop = false;
    [Export]
    public bool Preflop {
        get => _preflop;
        set {
            bool before = _preflop;
            _preflop = value;
            if (before == value || Settings == null) return;
            UpdateButtons();
        }
    }
    SizingSettings _settings;
    [Export]
    public SizingSettings Settings {
        get => _settings;
        set {
            _settings = value;
            if (Settings == null) return;
            UpdateButtons();
        }
    }
    public override void _Ready() {
        UpdateButtons();
        CallDeferred(MethodName.AfterReady);
    }
    void AfterReady() {
        if (Engine.IsEditorHint()) return;
        Global.Lobby.JoinedLobby += OnJoinedLobby;
        Global.Lobby.PokerAction += OnAction;
    }
    void OnJoinedLobby() {
        Preflop = true;
    }
    void OnAction() {
        if (Global.LocalGame.Street == Poker.Street.PREFLOP) {
            Preflop = true;
            return;
        } else {
            Preflop = false;
            return;
        }
    }

    void UpdateButtons() {
        foreach (Node n in GetChildren()) {
            RemoveChild(n);
        }
        int numButtons;
        Godot.Collections.Array<BetSize> sizes;
        if (Preflop) {
            numButtons = Settings.NumPreflopSizes;
            sizes = Settings.PreflopSizes;

        } else {
            numButtons = Settings.NumPostflopSizes;
            sizes = Settings.PostflopSizes;
        }
        for (int i = 0; i < numButtons; i++) {
            BetSize currSize = sizes[i];
            if (currSize == null) return;
            SizingButton currButton = new() {
                Type = currSize.Type,
                Amount = currSize.Amount
            };
            AddChild(currButton);
            if (Engine.IsEditorHint()) {
                if (GetTree() == null || GetTree().EditedSceneRoot == null) return;
                currButton.Owner = GetTree().EditedSceneRoot;
            }
        }
    }
}
