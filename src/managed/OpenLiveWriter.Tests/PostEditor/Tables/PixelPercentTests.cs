using System;
using System.Globalization;

using NUnit.Framework;

using OpenLiveWriter.PostEditor.Tables;

namespace OpenLiveWriter.Tests.PostEditor.Tables
{
    [TestFixture]
    public class PixelPercentTests
    {
        [Test]
        public void Default_Constructor()
        {
            // Act
            var sut = new PixelPercent();

            // Assert
            Assert.AreEqual(0, sut.Value);
            Assert.AreEqual(PixelPercentUnits.Undefined, sut.Units);
        }

        [Test]
        [TestCase(1, PixelPercentUnits.Percentage)]
        [TestCase(100, PixelPercentUnits.Percentage)]
        [TestCase(2, PixelPercentUnits.Pixels)]
        [TestCase(300, PixelPercentUnits.Pixels)]
        public void Constructor_Valid_Values(int value, PixelPercentUnits units)
        {
            // Act
            var sut = new PixelPercent(value, units);

            // Assert
            Assert.AreEqual(value, sut.Value);
            Assert.AreEqual(units, sut.Units);
        }

        [Test]
        [TestCase(-1, PixelPercentUnits.Percentage)]
        [TestCase(101, PixelPercentUnits.Percentage)]
        [TestCase(-1, PixelPercentUnits.Pixels)]
        public void Constructor_Invalid_Values(int value, PixelPercentUnits units)
        {
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new PixelPercent(value, units));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void Constructor_EmptyValues_Gives_Undefined(string text)
        {
            var sut = new PixelPercent(text, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(PixelPercentUnits.Undefined, sut.Units);
        }

        [Test]
        [TestCase("1", 1, PixelPercentUnits.Pixels)]
        [TestCase("1%", 1, PixelPercentUnits.Percentage)]
        [TestCase(" 100% ", 100, PixelPercentUnits.Percentage)]
        public void Constructor_Valid_Values(string text, int expectedValue, PixelPercentUnits expectedUnits)
        {
            // Act
            var sut = new PixelPercent(text, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expectedUnits, sut.Units);
            Assert.AreEqual(expectedValue, sut.Value);
        }

        [Test]
        [TestCase("1", 1, PixelPercentUnits.Pixels)]
        [TestCase("1", 1, PixelPercentUnits.Percentage)]
        [TestCase(" 100 ", 100, PixelPercentUnits.Percentage)]
        public void Constructor_Valid_Values_With_Units(string text, int expectedValue, PixelPercentUnits units)
        {
            // Act
            var sut = new PixelPercent(text, CultureInfo.InvariantCulture, units);

            // Assert
            Assert.AreEqual(expectedValue, sut.Value);
        }

        [Test]
        [TestCase("d", PixelPercentUnits.Pixels)]
        [TestCase("100.454 ", PixelPercentUnits.Percentage)]
        public void Constructor_Invalid_Values_With_Units(string text, PixelPercentUnits units)
        {
            // Act
            var sut = new PixelPercent(text, CultureInfo.InvariantCulture, units);

            // Assert
            Assert.AreEqual(0, sut.Value);
            Assert.AreEqual(PixelPercentUnits.Undefined, sut.Units);
        }

        [Test]
        [TestCase("0", true)]
        [TestCase("100", true)]
        [TestCase("100%", true)]
        [TestCase("x", false)]
        [TestCase("107.9", false)]
        public void IsAcceptableWidth_Values(string text, bool expected)
        {
            // Act
            var result = PixelPercent.IsAcceptableWidth(text);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}