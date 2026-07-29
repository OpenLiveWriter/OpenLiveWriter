// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    /// <summary>
    /// Regression tests for the AutoSave crash caused by a markup-laden post
    /// title: on .NET (Core) Path.GetInvalidPathChars() no longer includes
    /// " &lt; &gt; as .NET Framework did, so FileHelper must list them
    /// explicitly or HTML flows into file names verbatim.
    /// </summary>
    [TestFixture]
    public class FileHelperValidFileNameTest
    {
        [Test]
        public void GetValidFileName_StripsAngleBracketsAndQuotes()
        {
            string result = FileHelper.GetValidFileName("<span style=\"font-weight: normal;\">Title</span>");
            Assert.IsFalse(result.Contains("<"), "file name must not contain '<': " + result);
            Assert.IsFalse(result.Contains(">"), "file name must not contain '>': " + result);
            Assert.IsFalse(result.Contains("\""), "file name must not contain a quote: " + result);
            Assert.IsTrue(result.Contains("Title"), "file name should keep the title text: " + result);
        }

        [Test]
        public void IsValidFileName_RejectsMarkupChars()
        {
            Assert.IsFalse(FileHelper.IsValidFileName("<b>Title</b>"));
            Assert.IsFalse(FileHelper.IsValidFileName("say \"hi\""));
        }
    }
}
