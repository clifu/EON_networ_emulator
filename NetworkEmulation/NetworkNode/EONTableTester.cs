using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkNode;

namespace NetworkingTools
{
    /// <summary>
    /// Klasa do testowania klasy EONTable
    /// </summary>
    [TestClass]
    public class EONTableTester
    {
        [TestMethod]
        public void testCheckAvailability1()
        {
            EONTable ET = new EONTable();

            short frequencyIn = 1;
            short bandIN = 2;
            short frequencyOut = 0;
            short bandOut = 3;


            EONTableRowIN rowIn = new EONTableRowIN(frequencyIn, bandIN);
            EONTableRowOut rowOut = new EONTableRowOut(frequencyOut, bandOut);

            //Pasma powinny byc teraz wolne
            Assert.IsTrue(ET.CheckAvailability(frequencyIn, bandIN, "in"));
            Assert.IsTrue(ET.CheckAvailability(frequencyOut, bandOut, "out"));
            Assert.IsTrue(ET.CheckAvailability(rowIn));
            Assert.IsTrue(ET.CheckAvailability(rowOut));

            ET.addRow(rowIn);
            ET.addRow(rowOut);

            //Po dodaniu pasma powinny byc zajete
            Assert.IsFalse(ET.CheckAvailability(frequencyIn, bandIN, "in"));
            Assert.IsFalse(ET.CheckAvailability(frequencyOut, bandOut, "out"));
            Assert.IsFalse(ET.CheckAvailability(rowIn));
            Assert.IsFalse(ET.CheckAvailability(rowOut));
        }

        [TestMethod]
        public void testCheckAvailability2()
        {
            EONTable ET = new EONTable();

            short frequencyIn = 1;
            short bandIN = 63;
            short frequencyOut = 0;
            short bandOut = 64;

            EONTableRowIN rowIn = new EONTableRowIN(frequencyIn, bandIN);
            EONTableRowOut rowOut = new EONTableRowOut(frequencyOut, bandOut);

            //Pasma powinny byc teraz wolne
            Assert.IsTrue(ET.CheckAvailability(frequencyIn, bandIN, "in"));
            Assert.IsTrue(ET.CheckAvailability(frequencyOut, bandOut, "out"));
            Assert.IsTrue(ET.CheckAvailability(rowIn));
            Assert.IsTrue(ET.CheckAvailability(rowOut));

            ET.addRow(rowIn);
            ET.addRow(rowOut);

            //Po dodaniu pasma powinny byc zajete
            Assert.IsFalse(ET.CheckAvailability(frequencyIn, bandIN, "in"));
            Assert.IsFalse(ET.CheckAvailability(frequencyOut, bandOut, "out"));
            Assert.IsFalse(ET.CheckAvailability(rowIn));
            Assert.IsFalse(ET.CheckAvailability(rowOut));
        }

        [TestMethod]
        public void testFindFrequency()
        {
            EONTable ET = new EONTable();

            //zapychamy łącze od 0 do 31
            ET.addRow(new EONTableRowIN(0, 32));
            ET.addRow(new EONTableRowIN(35, 29));

            Assert.IsTrue(ET.CheckAvailability(new EONTableRowIN(32, 3)));

            Assert.AreEqual(32, ET.FindFreeFrequency(3, "in"));
            Assert.AreEqual(-1, ET.FindFreeFrequency(4, "in"));

            //zapychamy łącze od 0 do 31
            ET.addRow(new EONTableRowOut(0, 32));
            ET.addRow(new EONTableRowOut(35, 29));

            Assert.IsTrue(ET.CheckAvailability(new EONTableRowOut(32, 3)));

            Assert.AreEqual(32, ET.FindFreeFrequency(3, "out"));
            Assert.AreEqual(-1, ET.FindFreeFrequency(4, "out"));
        }
    }
}
