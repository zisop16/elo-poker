using System.Linq;
using HoldemPoker.Cards;
public class PokerCard {
	public static string SuitName(CardColor suit) {
		switch (suit) {
			case CardColor.Spade: return "s";
			case CardColor.Heart: return "h";
			case CardColor.Club: return "c";
			case CardColor.Diamond: return "d";
		}
		Assert.That(false, "Nonexistent suit: " + suit + " could not be named");
		return "";
	}
	public static string RankName(CardType rank) {
		switch (rank) {
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
		Assert.That(false, "Nonexistent rank: " + rank + " could not be named");
		return "";
	}
	public static string DetermineName(CardColor suit, CardType rank) {
		return RankName(rank) + SuitName(suit);
	}
	public const int NUM_CARDS = 52;
	public const int NUM_SUITS = 4;
	public const int NUM_RANKS = NUM_CARDS / NUM_SUITS;
	private Card Card;
	public string Name { get => DetermineName(Card.Color, Card.Type); }

	public PokerCard(int fromInt) {
		int s = fromInt / NUM_RANKS;
		int r = fromInt % NUM_RANKS;
		CardColor col = (CardColor)s;
		CardType type = (CardType)r;
		Card = new Card(type, col);
	}
	public PokerCard(CardType rank, CardColor suit) {
		CardColor col = suit;
		CardType type = rank;
		Card = new Card(type, col);
	}
	public static Card[] ToCards(PokerCard[] cards) {
		return (Card[])cards.Select(c => c.Card);
	}
}
