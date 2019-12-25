using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SolitaireTestSuite
    {
        // A Test behaves as an ordinary method
        // [Test]
        // public void SolitaireTestSuiteSimplePasses()
        // {
        //     var bytes = SolitairePacker.Pack(new Solitaire(1));
        //     var s = SolitairePacker.Unpack(bytes);
        //     var bytes2 = SolitairePacker.Pack(s);
        //     for (int i = 0; i < bytes.Length; i++)
        //     {
        //         Assert.Equals(bytes[i], bytes2[i]);
        //     }
        // }

        [Test]
        public void CanPackPlainSolitaire()
        {
            var s = new Solitaire(1);
            foreach (var move in s.Deal())
            {
                // don't do anything
            }
            var bytes = SolitairePacker.Pack(s);
        }

        [Test]
        public void CanUnpackPlainSolitaire()
        {
            var s = new Solitaire(1);
            foreach (var move in s.Deal())
            {
                // don't do anything
            }
            var bytes = SolitairePacker.Pack(s);

            SolitairePacker.Unpack(bytes);
        }

        [Test]
        public void UnpackingThenPackingYieldsEqualBytes()
        {
            var s = new Solitaire(1);
            foreach (var move in s.Deal())
            {
                // don't do anything
            }
            var bytes = SolitairePacker.Pack(s);

            var bytes2 = SolitairePacker.Pack(SolitairePacker.Unpack(bytes));
            Assert.That(bytes2, Is.EqualTo(bytes));
        }

        [Test]
        public void UnpackingThenPackingAComplicatedSolitaireStateYieldsEqualBytes()
        {
            var s = new Solitaire(1);
            foreach (var move in s.Deal()) { }
            var random = new System.Random(1);
            var lastMoveBytes = SolitairePacker.Pack(s);
            for (int i = 0; i < 50; i++)
            {
                s.PerformMove(s.GetRandomMove(random));
                var bytes = SolitairePacker.Pack(s);
                var bytes2 = SolitairePacker.Pack(SolitairePacker.Unpack(bytes));
                Assert.That(bytes2, Is.EqualTo(bytes));

                Assert.That(bytes, Is.Not.EqualTo(lastMoveBytes));
                lastMoveBytes = bytes;
            }
        }

        [Test]
        public void MoveFromStockToWaste()
        {
            var s = new Solitaire(1);
            s.DealAll();

            Assert.That(s.stockPile.waste, Has.Count.EqualTo(0), "Waste pile starts off with no cards");
            Assert.That(s.stockPile.stock, Has.Count.EqualTo(24), "Stock pile should have 28 cards after initial deal out");
            s.PerformMove(new CardMovement(s.stockPile.stock.Peek(), s.stockPile.waste.GetDropCardLocation()));
            Assert.That(s.stockPile.waste, Has.Count.EqualTo(1), "Waste pile should now have one card");
            Assert.That(s.stockPile.stock, Has.Count.EqualTo(23), "Stock pile should have one less card");
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator SolitaireTestSuiteWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
