// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// The emoji catalog offered by the Insert-tab emoticons gallery. The Windows
    /// build inserted small emoticon images; the modern cross-platform approach
    /// inserts a Unicode emoji character at the caret (no image assets, renders with
    /// the system emoji font and survives publish as plain text). Pure/deterministic
    /// so the catalog and insertion payload are testable without a live WebView.
    /// </summary>
    public static class EmoticonGallery
    {
        /// <summary>An emoji entry: the Unicode character and a short display name.</summary>
        public readonly struct Emoticon
        {
            public Emoticon(string character, string name)
            {
                Character = character;
                Name = name;
            }

            public string Character { get; }
            public string Name { get; }
        }

        /// <summary>
        /// A curated set of common blogging emoji (a superset of the classic
        /// smiley set). Order is display order in the gallery flyout.
        /// </summary>
        public static readonly IReadOnlyList<Emoticon> Items = new[]
        {
            new Emoticon("\U0001F600", "Grinning"),
            new Emoticon("\U0001F601", "Beaming"),
            new Emoticon("\U0001F602", "Tears of joy"),
            new Emoticon("\U0001F603", "Smiling"),
            new Emoticon("\U0001F604", "Happy"),
            new Emoticon("\U0001F609", "Winking"),
            new Emoticon("\U0001F60A", "Blushing"),
            new Emoticon("\U0001F60D", "Heart eyes"),
            new Emoticon("\U0001F618", "Blowing a kiss"),
            new Emoticon("\U0001F617", "Kissing"),
            new Emoticon("\U0001F60E", "Cool"),
            new Emoticon("\U0001F914", "Thinking"),
            new Emoticon("\U0001F610", "Neutral"),
            new Emoticon("\U0001F612", "Unamused"),
            new Emoticon("\U0001F61E", "Disappointed"),
            new Emoticon("\U0001F622", "Crying"),
            new Emoticon("\U0001F62D", "Sobbing"),
            new Emoticon("\U0001F620", "Angry"),
            new Emoticon("\U0001F631", "Screaming"),
            new Emoticon("\U0001F632", "Astonished"),
            new Emoticon("\U0001F609", "Wink"),
            new Emoticon("\U0001F44D", "Thumbs up"),
            new Emoticon("\U0001F44E", "Thumbs down"),
            new Emoticon("\U0001F44F", "Clapping"),
            new Emoticon("\U0001F64C", "Raising hands"),
            new Emoticon("\U0001F64F", "Folded hands"),
            new Emoticon("\u2764\uFE0F", "Red heart"),
            new Emoticon("\U0001F494", "Broken heart"),
            new Emoticon("\U0001F525", "Fire"),
            new Emoticon("\u2B50", "Star"),
            new Emoticon("\U0001F389", "Party popper"),
            new Emoticon("\U0001F44C", "OK hand"),
            new Emoticon("\u2705", "Check mark"),
            new Emoticon("\u274C", "Cross mark"),
            new Emoticon("\U0001F680", "Rocket"),
            new Emoticon("\U0001F4A1", "Light bulb"),
        };

        /// <summary>
        /// Builds the insertion payload for the chosen emoji. Inserting an emoji is
        /// just inserting its Unicode character; a value that is not a known emoji
        /// yields null so the caller can ignore it.
        /// </summary>
        public static string BuildInsertion(string emoji)
        {
            if (string.IsNullOrEmpty(emoji))
                return null;
            foreach (var item in Items)
            {
                if (item.Character == emoji)
                    return item.Character;
            }
            return null;
        }
    }
}
