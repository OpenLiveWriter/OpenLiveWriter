// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    [TestFixture]
    public class HtmlPreserverTest
    {
        private const string OBJECT_WITH_EMBED = "<object width=\"425\" height=\"355\"><param name=\"movie\" value=\"http://www.youtube.com/v/AIObOihJDB8&rel=1\"></param><param name=\"wmode\" value=\"transparent\"></param><embed src=\"http://www.youtube.com/v/AIObOihJDB8&rel=1\" type=\"application/x-shockwave-flash\" wmode=\"transparent\" width=\"425\" height=\"355\"></embed></object>";

        private const string OBJECT_WITH_EMBED_IN_SMART_CONTENT =
            "<div class=\"wlWriterSmartContent\">" + OBJECT_WITH_EMBED + "</div>";

        private const string SCRIPT = "<script language=\"JavaScript\">document.write('foo');</script>";

        [Test]
        public void Test1()
        {
            HtmlPreserver hp = new HtmlPreserver();
            string scanned = hp.ScanAndPreserve(OBJECT_WITH_EMBED);
            Assert.That(scanned, Is.Not.EqualTo(OBJECT_WITH_EMBED));
            Assert.That(
                hp.RestorePreserved(scanned.Replace(OBJECT_WITH_EMBED, "<object></object>")),
                Is.EqualTo(OBJECT_WITH_EMBED));
        }

        [Test]
        public void IgnoreSmartContent()
        {
            HtmlPreserver hp = new HtmlPreserver();
            Assert.That(
                hp.ScanAndPreserve(OBJECT_WITH_EMBED_IN_SMART_CONTENT),
                Is.EqualTo(OBJECT_WITH_EMBED_IN_SMART_CONTENT));

            hp.Reset();
            Assert.That(
                hp.RestorePreserved(hp.ScanAndPreserve(OBJECT_WITH_EMBED_IN_SMART_CONTENT)),
                Is.EqualTo(OBJECT_WITH_EMBED_IN_SMART_CONTENT));
        }

        [Test]
        public void ScriptTest()
        {
            HtmlPreserver hp = new HtmlPreserver();
            string working = hp.ScanAndPreserve(SCRIPT);
            working = working.Replace(SCRIPT, SCRIPT + "foo");
            Assert.That(
                hp.RestorePreserved(working),
                Is.EqualTo(SCRIPT));
        }
    }
}
