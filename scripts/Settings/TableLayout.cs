using Godot;
using System;

[Tool]
[GlobalClass, Icon("res://images/icons/poker_table.png")]
public partial class TableLayout : Resource {
    public enum CardbackColor : byte {
        AQUA, BLACK, BLUE, FUCHSIA, GRAY, GREEN, LIGHTBLUE, LIME, MAROON,
        NAVY, OLIVE, PURPLE, RED, SILVER, TEAL, YELLOW
    };
    enum DeckType : byte {
        DEFAULT
    };
    public const string LAYOUT_PATH = "res://resources/player_settings/layout.tres";
    const string CARD_BACK_PATH = "res://images/card_backs/";
    const string DEFAULT_PATH = "res://images/deck/";
    static Texture2D GetCardBack(CardbackColor col) {
        string backName = "back-";
        switch (col) {
            case CardbackColor.AQUA:
                backName += "aqua";
                break;
            case CardbackColor.BLACK:
                backName += "black";
                break;
            case CardbackColor.BLUE:
                backName += "blue";
                break;
            case CardbackColor.FUCHSIA:
                backName += "fuchsia";
                break;
            case CardbackColor.GRAY:
                backName += "gray";
                break;
            case CardbackColor.GREEN:
                backName += "green";
                break;
            case CardbackColor.LIGHTBLUE:
                backName += "lightblue";
                break;
            case CardbackColor.LIME:
                backName += "lime";
                break;
            case CardbackColor.MAROON:
                backName += "maroon";
                break;
            case CardbackColor.NAVY:
                backName += "navy";
                break;
            case CardbackColor.OLIVE:
                backName += "olive";
                break;
            case CardbackColor.PURPLE:
                backName += "purple";
                break;
            case CardbackColor.RED:
                backName += "red";
                break;
            case CardbackColor.SILVER:
                backName += "silver";
                break;
            case CardbackColor.TEAL:
                backName += "teal";
                break;
            case CardbackColor.YELLOW:
                backName += "yellow";
                break;
        }
        backName += ".png";
        return GD.Load<Texture2D>(CARD_BACK_PATH + backName);
    }
    [Export]
    private CardbackColor _cardBack = CardbackColor.LIGHTBLUE;
    public Texture2D CardBack { get => GetCardBack(_cardBack); }
    [Export]
    private DeckType Type = DeckType.DEFAULT;
    public Texture2D GetCard(PokerCard card) {
        string cardPath = "";
        switch (Type) {
            case DeckType.DEFAULT:
                cardPath = DEFAULT_PATH;
                break;
        }
        if (cardPath == "") {
            Assert.That(false, "Table layout deck type not identified");
        }
        cardPath += card.Name + ".png";
        return GD.Load<Texture2D>(cardPath);
    }
}
