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
            Assert.That(Card.FromByte(1).Suit, Is.EqualTo(Suit.Clubs));
            Assert.That(Card.FromByte(13).Suit, Is.EqualTo(Suit.Clubs));
        }

        [Test]
        public void TestToAndFromByte()
        {
            for (byte i = 1; i <= 52; i++)
            {
                Assert.That(Card.FromByte(i).ToByte(), Is.EqualTo(i));
            }
        }

        [Test]
        public void TestEquality()
        {
            Assert.That(new Card(Suit.Clubs, 5), Is.EqualTo(new Card(Suit.Clubs, 5)));
            Assert.That(new Card(Suit.Clubs, 5), Is.Not.EqualTo(new Card(Suit.Spades, 5)));
        }

        [Test]
        public void TestId()
        {
            Assert.That(new Card(Suit.Clubs, 5).Id, Is.EqualTo("5-0"));
        }

        [Test]
        public void TestToString()
        {
            Assert.That(new Card(Suit.Clubs, 5).ToString(), Is.EqualTo("5 of Clubs"));
        }

    }
}
