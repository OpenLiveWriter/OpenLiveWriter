// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group F — Insert Emoticon. The emoji catalog + insertion payload are pure
    /// (<see cref="EmoticonGallery"/>), so they are asserted headlessly. Inserting a
    /// Unicode emoji character replaces the Windows emoticon-image approach.
    /// </summary>
    [TestFixture]
    [Category("GroupF")]
    public class GroupF_EmoticonTests
    {
        [Test]
        public void Emoticon_CatalogIsNonEmptyAndUsesUnicode()
        {
            Assert.That(EmoticonGallery.Items, Is.Not.Empty);
            foreach (var item in EmoticonGallery.Items)
            {
                Assert.That(item.Character, Is.Not.Null.And.Not.Empty, item.Name);
                Assert.That(item.Name, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void Emoticon_BuildInsertionReturnsTheEmojiCharacter()
        {
            var first = EmoticonGallery.Items.First();
            Assert.That(EmoticonGallery.BuildInsertion(first.Character), Is.EqualTo(first.Character));
        }

        [Test]
        public void Emoticon_EveryCatalogEntryBuildsItsOwnInsertion()
        {
            foreach (var item in EmoticonGallery.Items)
                Assert.That(EmoticonGallery.BuildInsertion(item.Character), Is.EqualTo(item.Character));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("not-an-emoji")]
        public void Emoticon_BuildInsertionRejectsUnknown(string input)
        {
            Assert.That(EmoticonGallery.BuildInsertion(input), Is.Null);
        }

        [Test]
        public void Emoticon_ContainsClassicSmiley()
        {
            // U+1F600 GRINNING FACE should be present.
            Assert.That(EmoticonGallery.Items.Any(i => i.Character == "\U0001F600"), Is.True);
        }

        [Test]
        public void Emoticon_CharactersAreValidNonAsciiPayloads()
        {
            foreach (var item in EmoticonGallery.Items)
            {
                // Emoji payloads are outside the ASCII range (need no HTML escaping).
                Assert.That(item.Character.Any(c => c > 127), Is.True,
                    $"'{item.Name}' should be a non-ASCII emoji");
                Assert.That(item.Character, Does.Not.Contain("<").And.Not.Contain("&"));
            }
        }
    }
}
