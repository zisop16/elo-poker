using System.Linq;
using HoldemPoker.Cards;
public struct PokerCard {
	public static string ColorName(CardColor color) {
		switch (color) {
			case CardColor.Spade:
				return "s";
			case CardColor.Heart:
				return "h";
			case CardColor.Club:
				return "c";
			case CardColor.Diamond:
				return "d";
		}
		Assert.That(false, "Nonexistent suit: " + color + " could not be named");
		return "";
	}
	public static string TypeName(CardType type) {
		switch (type) {
			case CardType.Deuce: return "2";
			case CardType.Three: return "3";
			case CardType.Four: return "4";
			case CardType.Five: return "5";
			case CardType.Six: return "6";
			case CardType.Seven: return "7";
			case CardType.Eight: return "8";
			case CardType.Nine: return "9";
			case CardType.Ten: return "T";
			case CardType.Jack: return "J";
			case CardType.Queen: return "Q";
			case CardType.King: return "K";
			case CardType.Ace: return "A";
		}
		Assert.That(false, "Nonexistent rank: " + type + " could not be named");
		return "";
	}
	public static string DetermineName(CardColor suit, CardType rank) {
		return TypeName(rank) + ColorName(suit);
	}
	public const int NUM_CARDS = 52;
	public const int NUM_SUITS = 4;
	public const int NUM_RANKS = NUM_CARDS / NUM_SUITS;
	private readonly Card Card;
	public string Name { get => DetermineName(Card.Color, Card.Type); }
	public CardType Type { get => Card.Type; }
	public CardColor Color { get => Card.Color; }
	public static implicit operator byte(PokerCard c)
    {
        return (byte)c;
    }
	public static explicit operator PokerCard(byte b) {
		return new PokerCard((Card)b);
	}

	public PokerCard(Card c) {
		Card = c;
	}

	public PokerCard(int fromInt) {
		int s = fromInt / NUM_RANKS;
		int r = fromInt % NUM_RANKS;
		CardColor col = (CardColor)s;
		CardType type = (CardType)r;
		Card = new Card(type, col);
	}
	public PokerCard(CardType t, CardColor c) {
		Card = new Card(t, c);
	}
	public static Card[] ToCards(PokerCard[] cards) {
		return [.. cards.Select(c => c.Card).ToArray()];
	}
    public override string ToString() {
		return Name;
    }

}
