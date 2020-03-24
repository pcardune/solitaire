using System;
using System.Collections.Generic;

public class Deck
{
    public static List<Card> GetShuffledDeck(int randomSeed)
    {
        List<Card> shuffledCards = new List<Card>();
        List<Card> cards = new List<Card>();
        foreach (var suit in (Suit[])Enum.GetValues(typeof(Suit)))
        {
            for (int value = 1; value <= 13; value++)
            {
                cards.Add(new Card(suit, value));
            }
        }
        System.Random random = new System.Random(randomSeed);
        while (cards.Count > 0)
        {
            int cardIndex = random.Next(0, cards.Count);
            shuffledCards.Add(cards[cardIndex]);
            cards.RemoveAt(cardIndex);
        }
        return shuffledCards;
    }
}