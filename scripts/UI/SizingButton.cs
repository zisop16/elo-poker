using Godot;
using System;

[Tool]
[GlobalClass]
public partial class SizingButton : Button {
    BetSize.SizeType _type;
    [Export]
    public BetSize.SizeType Type {
        get => _type;
        set {
            _type = value;
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
        if (Type == BetSize.SizeType.PERCENT) {
            Text = formattedAmount + "%";
        } else {
            Text = formattedAmount + "BB";
        }
    }
    public override void _Ready() {
        ButtonDown += OnClick;
    }
    void OnClick() {
        Global.Lobby.ChipsInput.HandleTextSubmit(Text);
    }
}
