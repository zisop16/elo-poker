using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Godot;
using HoldemPoker.Cards;
using RandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;

public class Deck {
    private int[] Cards = new int[PokerCard.NUM_CARDS];
    int LastDrawn;
    public Deck() {
        for (int i = 0; i < PokerCard.NUM_CARDS; i++) {
            Cards[i] = i;
        }
    }
    public void Shuffle() {
        int n = Cards.Length;
        while (n > 1) {
            int k = RandomNumberGenerator.GetInt32(n--);
            int temp = Cards[n];
            Cards[n] = Cards[k];
            Cards[k] = temp;
        }
        LastDrawn = 0;
    }
    public PokerCard Draw() {
        return new PokerCard(Cards[LastDrawn++]);
    }
    public override string ToString() {
        string deckOrder = "";
        foreach (int c in Cards) {
            deckOrder += new PokerCard(c) + ", ";
        }
        return deckOrder;
    }
}