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
            Assert.That(sut.Value, Is.EqualTo(0));
            Assert.That(sut.Units, Is.EqualTo(PixelPercentUnits.Undefined));
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
            Assert.That(sut.Value, Is.EqualTo(value));
            Assert.That(sut.Units, Is.EqualTo(units));
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
            Assert.That(sut.Units, Is.EqualTo(PixelPercentUnits.Undefined));
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
            Assert.That(sut.Units, Is.EqualTo(expectedUnits));
            Assert.That(sut.Value, Is.EqualTo(expectedValue));
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
            Assert.That(sut.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase("d", PixelPercentUnits.Pixels)]
        [TestCase("100.454 ", PixelPercentUnits.Percentage)]
        public void Constructor_Invalid_Values_With_Units(string text, PixelPercentUnits units)
        {
            // Act
            var sut = new PixelPercent(text, CultureInfo.InvariantCulture, units);

            // Assert
            Assert.That(sut.Value, Is.EqualTo(0));
            Assert.That(sut.Units, Is.EqualTo(PixelPercentUnits.Undefined));
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
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}