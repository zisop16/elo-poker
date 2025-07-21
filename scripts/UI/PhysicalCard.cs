using Godot;
using System;

public partial class PhysicalCard : TextureRect {
    static string DeckPath = "res://images/cards/";
    PokerCard card;
    public PokerCard Card {
        get => card;
        set {
            card = value;
            string type = PokerCard.TypeName(card.Type, longForm: true);
            string color = PokerCard.ColorName(card.Color, longForm: true);
            string cardName = type + "_of_" + color + ".png";
            string cardPath = DeckPath + cardName;
            Texture2D tex = GD.Load<Texture2D>(cardPath);
            Texture = tex;
        }
    }
}
