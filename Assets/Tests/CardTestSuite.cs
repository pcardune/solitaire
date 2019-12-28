using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class CardTestSuite
    {

        [Test]
        public void ColorAttribute()
        {
            Assert.That(new Card(Suit.Clubs, 5).Color, Is.EqualTo(CardColor.BLACK));
            Assert.That(new Card(Suit.Spades, 5).Color, Is.EqualTo(CardColor.BLACK));
            Assert.That(new Card(Suit.Hearts, 5).Color, Is.EqualTo(CardColor.RED));
            Assert.That(new Card(Suit.Diamonds, 5).Color, Is.EqualTo(CardColor.RED));
        }

        [Test]
        public void TestToByte()
        {
            Assert.That(new Card(Suit.Clubs, 1).ToByte(), Is.EqualTo(1));
            Assert.That(new Card(Suit.Clubs, 5).ToByte(), Is.EqualTo(5));
            Assert.That(new Card(Suit.Diamonds, 5).ToByte(), Is.EqualTo(31));
        }

        [Test]
        public void TestFromByte()
        {
            Assert.That(Card.FromByte(31).Suit, Is.EqualTo(Suit.Diamonds));
            Assert.That(Card.FromByte(31).Rank, Is.EqualTo(5));

            Assert.That(() => Card.FromByte(0), Throws.Exception);
        }

        [Test]
        public void TestEquality()
        {
            Assert.That(new Card(Suit.Clubs, 5), Is.EqualTo(new Card(Suit.Clubs, 5)));
            Assert.That(new Card(Suit.Clubs, 5), Is.Not.EqualTo(new Card(Suit.Spades, 5)));
        }

    }
}
