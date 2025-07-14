using System.Collections.Generic;
using System.Security.Cryptography;

public class Deck {
    private int[] Cards = new int[PokerCard.NUM_CARDS];
    int LastDrawn;
    public Deck() {
        for (int i = 0; i < PokerCard.NUM_CARDS; i++) {
            Cards[i] = i;
        }
    }
    public void Shuffle() {
        RandomNumberGenerator.Shuffle<int>(Cards);
        LastDrawn = 0;
    }
    public PokerCard Draw() {
        return new PokerCard(LastDrawn++);
    }
}