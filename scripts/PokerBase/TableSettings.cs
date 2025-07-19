using Godot;
using System;

[GlobalClass, Icon("res://icons/table_icon.png")]
public partial class TableSettings : Resource {
    [Export(PropertyHint.Range, "2,6,1")]
    public int NumPlayers = 2;
    [Export(PropertyHint.Range, "1, 100, 1")]
    public int BigBlind = 2;
    [Export(PropertyHint.Range, "0, 1, .01")]
    private double smallBlind = .5;
    public int SmallBlind {
        get => (int)Math.Round(BigBlind * smallBlind);
        set => smallBlind = (double)value / BigBlind;
    }
    [Export(PropertyHint.Range, "0, 2, .01")]
    /// <summary>
    /// Percent of big blind as the TOTAL ANTE, summed across all players
    /// </summary>
    private double combinedAnte = 0;
    /// <summary>
    /// Number of chips each player must invest as the ante
    /// </summary>
    public int Ante {
        get => (int)Math.Round(combinedAnte * BigBlind / NumPlayers);
        set => combinedAnte = (double)value / BigBlind / NumPlayers;
    }
    [Export(PropertyHint.Range, "1, 200, .01")]
    /// <summary>
    /// Starting stack in big blinds
    /// </summary>
    private double startStack;
    public int StartStack {
        get => (int)Math.Round(startStack * BigBlind);
        set => startStack = (double)value / BigBlind;
    }
    [Export]
    public bool Timer = true;
    [Export(PropertyHint.Range, "4, 10, .1")]
    public double DecisionTime;
    [Export(PropertyHint.Range, "0, 30, .1")]
    public double TimeBank;
    [Export(PropertyHint.Range, "0, 30, .1")]
    public double TimeBankAdd;
    
    public GameType Type = GameType.HOLDEM;


    public bool ValidateSettings() {
        if (BigBlind < 1) { return false; }
        if (SmallBlind > 1 || SmallBlind < 0) { return false; }
        if (combinedAnte < 0 || combinedAnte > 2) { return false; }

        if (DecisionTime < 4) { return false; }
        if (TimeBank < 0) { return false; }
        if (TimeBankAdd < 0) { return false; }

        return true;
    }
}
