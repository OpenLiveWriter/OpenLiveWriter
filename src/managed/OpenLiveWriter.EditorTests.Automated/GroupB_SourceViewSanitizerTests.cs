// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Source view data-URI elision: embedded images (base64 data-URIs, potentially
    /// megabytes on a single line) must be tokenized for display so the Source pane
    /// stays usable, and faithfully re-expanded when the user pushes edited source
    /// back into the editor.
    /// </summary>
    [TestFixture]
    [Category("GroupB")]
    public class GroupB_SourceViewSanitizerTests
    {
        private static string DataUri(int payloadLength) =>
            "data:image/png;base64," + new string('A', payloadLength);

        [Test]
        public void Elide_LongDataUri_BecomesToken()
        {
            string html = $"<p>hi</p><img src=\"{DataUri(5000)}\" alt=\"photo\" />";
            var uris = new List<string>();

            string display = SourceViewSanitizer.ElideDataUris(html, uris);

            Assert.That(display, Does.Contain("src=\"data-olw-img:0\""));
            Assert.That(display, Does.Contain("alt=\"photo\""), "attributes around the src survive");
            Assert.That(display, Does.Not.Contain("AAAA"));
            Assert.That(uris, Has.Count.EqualTo(1));
            Assert.That(uris[0], Does.StartWith("data:image/png;base64,"));
        }

        [Test]
        public void Elide_ShortDataUri_StaysInline()
        {
            string small = DataUri(50);
            string html = $"<img src=\"{small}\" />";
            var uris = new List<string>();

            string display = SourceViewSanitizer.ElideDataUris(html, uris);

            Assert.That(display, Is.EqualTo(html));
            Assert.That(uris, Is.Empty);
        }

        [Test]
        public void Elide_MultipleImages_IndexedInOrder()
        {
            string html = $"<img src=\"{DataUri(300)}\" /><img src=\"{DataUri(400)}\" />";
            var uris = new List<string>();

            string display = SourceViewSanitizer.ElideDataUris(html, uris);

            Assert.That(display, Does.Contain("data-olw-img:0"));
            Assert.That(display, Does.Contain("data-olw-img:1"));
            Assert.That(uris, Has.Count.EqualTo(2));
            Assert.That(uris[0].Length, Is.LessThan(uris[1].Length));
        }

        [Test]
        public void Restore_ReexpandsTokens_Lossless()
        {
            string html = $"<p>hi</p><img src=\"{DataUri(5000)}\" alt=\"photo\" /><p>bye</p>";
            var uris = new List<string>();
            string display = SourceViewSanitizer.ElideDataUris(html, uris);

            string restored = SourceViewSanitizer.RestoreDataUris(display, uris);

            Assert.That(restored, Is.EqualTo(html));
        }

        [Test]
        public void Restore_PreservesUserEditsAroundToken()
        {
            string html = $"<p>before</p><img src=\"{DataUri(2000)}\" width=\"640\" />";
            var uris = new List<string>();
            string display = SourceViewSanitizer.ElideDataUris(html, uris);

            string edited = display.Replace("before", "AFTER").Replace("640", "800");
            string restored = SourceViewSanitizer.RestoreDataUris(edited, uris);

            Assert.That(restored, Does.Contain("AFTER"));
            Assert.That(restored, Does.Contain("width=\"800\""));
            Assert.That(restored, Does.Contain(DataUri(2000)));
        }

        [Test]
        public void Restore_UnknownTokenLeftVerbatim()
        {
            string text = "<img src=\"data-olw-img:9\" />";
            string restored = SourceViewSanitizer.RestoreDataUris(text, new List<string>());
            Assert.That(restored, Is.EqualTo(text));
        }

        [Test]
        public void Restore_NullOrEmptyInputs_Safe()
        {
            Assert.That(SourceViewSanitizer.RestoreDataUris(null, null), Is.Null);
            Assert.That(SourceViewSanitizer.RestoreDataUris("", new List<string>()), Is.EqualTo(""));
            Assert.That(SourceViewSanitizer.ElideDataUris(null, new List<string>()), Is.Null);
        }
    }
}
