using Godot;
using System;

[Tool]
[GlobalClass, Icon("res://images/icons/poker_chip.png")]
public partial class SizingSettings : Resource {
    int _numPreflopSizes;
    [Export(PropertyHint.Range, "0, 12")]
    public int NumPreflopSizes {
        get => _numPreflopSizes;
        set {
            _numPreflopSizes = value;
            PreflopSizes.Resize(value);
        }
    }
    int _numPostflopSizes;
    [Export(PropertyHint.Range, "0, 12")]
    public int NumPostflopSizes {
        get => _numPostflopSizes;
        set {
            _numPostflopSizes = value;
            PostflopSizes.Resize(value);
        }
    }
    [Export]
    public Godot.Collections.Array<BetSize> PreflopSizes = [];
    [Export]
    public Godot.Collections.Array<BetSize> PostflopSizes = [];
}
