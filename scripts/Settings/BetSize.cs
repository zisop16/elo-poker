using Godot;
using System;

[Tool]
[GlobalClass]
public partial class BetSize : Resource {
    public enum SizeType { BIG_BLINDS, PERCENT };
    [Export]
    public SizeType Type;
    [Export(PropertyHint.Range, "0,500,0.1")]
    public double Amount;
}
