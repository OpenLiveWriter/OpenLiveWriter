// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Publishing.Drafts
{
    /// <summary>
    /// Lightweight listing entry for a saved draft — enough to populate the Open
    /// Drafts picker and the draft MRU without loading the full body HTML.
    /// </summary>
    public sealed class DraftInfo
    {
        public DraftInfo(string id, string title, DateTime dateModifiedUtc)
        {
            Id = id;
            Title = title;
            DateModifiedUtc = dateModifiedUtc;
        }

        /// <summary>Draft identifier (matches <see cref="PostDocument.Id"/>).</summary>
        public string Id { get; }

        /// <summary>Draft title, or a friendly placeholder when the post is untitled.</summary>
        public string Title { get; }

        /// <summary>UTC last-modified time, used for MRU/list ordering.</summary>
        public DateTime DateModifiedUtc { get; }

        /// <summary>A non-empty display title, substituting a placeholder if blank.</summary>
        public string DisplayTitle =>
            string.IsNullOrWhiteSpace(Title) ? "(untitled post)" : Title;
    }
}
