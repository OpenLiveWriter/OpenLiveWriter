// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestFixture]
    public class DefaultBlockElementTest
    {
        [Test]
        public void ParagraphDefaultBlockElementTest()
        {
            ParagraphDefaultBlockElement paragraphDefaultBlockElement = new ParagraphDefaultBlockElement();

            string plainText = 
@"line 1
line 2

line 3


line 4



line 5




line 6





line 7";

            string html = TextHelper.GetHTMLFromText(plainText, false, paragraphDefaultBlockElement);

            Assert.AreEqual(
@"<p>line 1<br />
line 2</p>
<p>line 3</p>
<p><br />
line 4</p>
<p></p>
<p>line 5</p>
<p></p>
<p><br />
line 6</p>
<p></p>
<p></p>
<p>line 7</p>", html);
        }

        [Test]
        public void DivDefaultBlockElementTest()
        {
            DivDefaultBlockElement divDefaultBlockElement = new DivDefaultBlockElement();

            string plainText = 
@"line 1
line 2

line 3


line 4



line 5




line 6





line 7";

            string html = TextHelper.GetHTMLFromText(plainText, false, divDefaultBlockElement);

            Assert.AreEqual(
@"<div>line 1</div>
<div>line 2</div>
<div></div>
<div>line 3</div>
<div></div>
<div></div>
<div>line 4</div>
<div></div>
<div></div>
<div></div>
<div>line 5</div>
<div></div>
<div></div>
<div></div>
<div></div>
<div>line 6</div>
<div></div>
<div></div>
<div></div>
<div></div>
<div></div>
<div>line 7</div>", html);
        }
    }
}
