using NUnit.Framework;
using OpenLiveWriter.HtmlEditor;
using System;

namespace OpenLiveWriter.UnitTest.HtmlEditor.WordCounterTests
{
    [TestFixture]
    public class HebrewTextWordCount
    {
        private static void CountText(string text, int expectedCount, int? expectedCharCount = null)
        {
            var wordCounter = new WordCounter(text);
            Assert.That(wordCounter.Words, Is.EqualTo(expectedCount));

            if (expectedCharCount.HasValue)
            {
                Assert.That(wordCounter.Chars, Is.EqualTo(expectedCharCount));
            }
        }

        [Test]
        public void EmptyText_ReturnZero()
        {
            CountText(string.Empty, 0, 0);
        }

        [Test]
        public void SanityEnglishText()
        {
            CountText("Simple English Text", 3, 19);
        }

        [Test]
        public void SanityEnglishTextEndsWithPunctuation()
        {
            CountText("Simple English Text.", 3, 20);
        }


        [Test]
        public void SanityEnglishMultiline()
        {
            CountText("This is a " + Environment.NewLine + "multiline text", 5);
        }

        [Test]
        public void EnglishSeparatedBy()
        {
            CountText("This is a " + Environment.NewLine + "multiline text", 5);
        }



        [Test]
        public void CyrillicText()
        {
            CountText("ДЖem", 1, 4);
        }
        
        [Test]
        public void OneHebrewWordText()
        {
            CountText("עברית", 1, 5);
        }

        [Test]
        public void SimpleHebrewText()
        {
            CountText("משפט עם חמש מילים בעברית", 5);
        }

        [Test]
        public void HebrewMultiline()
        {
            CountText("משפט עם חמש " + "\n" + "מילים בעברית", 5);
        }


        [Test]
        public void MixedHebrewEnglishWords()
        {
            CountText("מילה בעברית and an english word", 6);            

        }

        [Test]
        public void MixedHebrewEnglishChars()
        {
            CountText("עבריתenglish", 1, 12);
        }

        [Test]
        public void OneArabicWordText()
        {
            CountText("عربي", 1, 4);
        }

        [Test]
        public void SimpleArabicText()
        {
            CountText("اللغة العربية هي أكثر اللغات تحدثاً", 6);
        }




    }
}
