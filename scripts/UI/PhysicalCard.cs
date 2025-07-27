using Godot;
using HoldemPoker.Cards;
using System;

[Tool]
[GlobalClass]
public partial class PhysicalCard : TextureRect {
    bool _backFacing = false;
    public bool BackFacing {
        get => _backFacing;
        set {
            _backFacing = value;
            UpdateTexture();
        }
    }
    public PokerCard Card {
        get {
            return new PokerCard(Type, Color);
        }
        set {
            _color = value.Color;
            _type = value.Type;
            UpdateTexture();
        }
    }
    CardColor _color = CardColor.Spade;
    [Export]
    public CardColor Color {
        get => _color;
        set {
            _color = value;
            UpdateTexture();
        }
    }
    CardType _type = CardType.Ace;
    [Export]
    public CardType Type {
        get => _type;
        set {
            _type = value;
            UpdateTexture();
        }
    }
    void UpdateTexture() {
        if (_backFacing) {
            Texture2D tex = Global.LocalLayout.CardBack;
            Texture = tex;
        } else {
            Texture2D tex = Global.LocalLayout.GetCard(Card);
            Texture = tex;
        }
    }
}
