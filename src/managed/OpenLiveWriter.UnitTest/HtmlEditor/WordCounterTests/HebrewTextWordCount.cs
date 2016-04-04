using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.UnitTest.HtmlEditor.WordCounterTests
{
    [TestClass]
    public class HebrewTextWordCount
    {
        private static void CountText(string text, int expectedCount, int? expectedCharCount = null)
        {
            var wordCounter = new WordCounter(text);
            Assert.AreEqual(expectedCount, wordCounter.Words);

            if (expectedCharCount.HasValue)
            {
                Assert.AreEqual(expectedCharCount, wordCounter.Chars);
            }
        }

        [TestMethod]
        public void EmptyText_ReturnZero()
        {
            CountText(string.Empty, 0, 0);
        }

        [TestMethod]
        public void SanityEnglishText()
        {
            CountText("Simple English Text", 3, 19);
        }

        [TestMethod]
        public void CyrillicText()
        {
            CountText("ДЖem", 1, 4);
        }
        
        [TestMethod]
        public void OneHebrewWordText()
        {
            CountText("עברית", 1, 5);
        }

        [TestMethod]
        public void SimpleHebrewText()
        {
            CountText("משפט עם חמש מילים בעברית", 5);
        }

        [TestMethod]
        public void MixedHebrewEnglishWords()
        {
            CountText("מילה בעברית and an english word", 6);
        }

        [TestMethod]
        public void MixedHebrewEnglishChars()
        {
            CountText("עבריתenglish", 1, 12);
        }

        [TestMethod]
        public void OneArabicWordText()
        {
            CountText("عربي", 1, 4);
        }

        [TestMethod]
        public void SimpleArabicText()
        {
            CountText("اللغة العربية هي أكثر اللغات تحدثاً", 6);
        }




    }
}
