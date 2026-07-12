// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Cross-platform port of the Windows
    /// <c>OpenLiveWriter.Extensibility.BlogClient.BlogPostCategory</c> — a blog category
    /// as returned by <c>metaWeblog.getCategories</c>. Carries the server id, the display
    /// name, and (optionally) a parent id for hierarchical providers. The publish path
    /// sends the category <see cref="Name"/> in the inline <c>categories</c> array, matching
    /// the Windows MetaWeblog behavior.
    /// </summary>
    public sealed class BlogPostCategory
    {
        public BlogPostCategory(string id, string name, string parent = "")
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            Parent = parent ?? string.Empty;
        }

        /// <summary>Server-side category id (falls back to the name when absent).</summary>
        public string Id { get; }

        /// <summary>Human-readable category name (what the user selects and what is published).</summary>
        public string Name { get; }

        /// <summary>Parent category id for hierarchical providers; empty when top-level.</summary>
        public string Parent { get; }

        public override string ToString() => Name;

        public override bool Equals(object obj) =>
            obj is BlogPostCategory other &&
            string.Equals(Id, other.Id, StringComparison.Ordinal) &&
            string.Equals(Name, other.Name, StringComparison.Ordinal);

        public override int GetHashCode() =>
            (Id?.GetHashCode() ?? 0) ^ (Name?.GetHashCode() ?? 0);
    }
}
