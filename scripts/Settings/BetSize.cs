using Godot;
using System;

public enum SizeType { BIG_BLINDS, PERCENT };
[Tool]
[GlobalClass]
public partial class BetSize : Resource {
    [Export]
    public SizeType Type;
    [Export(PropertyHint.Range, "0,500,0.1")]
    public double Amount;
}
