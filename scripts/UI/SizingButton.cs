using Godot;
using System;

[Tool]
[GlobalClass]
public partial class SizingButton : Button {
    bool _falseBBTruePercent;
    [Export]
    bool FalseBBTruePercent {
        get => _falseBBTruePercent;
        set {
            _falseBBTruePercent = value;
            UpdateText();
        }
    }
    double _amount;
    [Export(PropertyHint.Range, "1,500,0.1")]
    public double Amount {
        get => _amount;
        set {
            _amount = value;
            UpdateText();
        }
    }
    void UpdateText() {
        string formattedAmount = string.Format("{0:0.#}", _amount);
        if (FalseBBTruePercent) {
            Text = formattedAmount + "%";
        } else {
            Text = formattedAmount + "BB";
        }
    }
    public override void _Ready() {
        ButtonDown += OnClick;
    }
    void OnClick() {
        Global.Lobby.ChipsInput.HandleTextSizing(Text);
    }
}
