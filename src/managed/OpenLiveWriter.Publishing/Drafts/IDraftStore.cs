// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace OpenLiveWriter.Publishing.Drafts
{
    /// <summary>
    /// Persistence abstraction for local post drafts. The Windows editor stores
    /// drafts as <c>.wpost</c> OLE structured storage; on macOS/cross-platform we
    /// use a simpler file-per-draft store (see <see cref="FileDraftStore"/>).
    /// The interface hides the on-disk format so it can change (JSON today) without
    /// touching callers.
    /// </summary>
    public interface IDraftStore
    {
        /// <summary>
        /// Persists <paramref name="document"/>. New documents (empty
        /// <see cref="PostDocument.Id"/>) are assigned an id and creation timestamp;
        /// existing documents are overwritten in place. The modified timestamp is
        /// refreshed and the document's dirty flag cleared. Returns the same instance.
        /// </summary>
        PostDocument Save(PostDocument document);

        /// <summary>
        /// Loads the draft with the given id, or returns <c>null</c> if no such draft
        /// exists. Throws <see cref="DraftStoreException"/> if the draft file is present
        /// but corrupt/unreadable.
        /// </summary>
        PostDocument Load(string id);

        /// <summary>
        /// Lists saved drafts, most-recently-modified first (the ordering used for the
        /// Open Drafts list and the draft MRU). Missing store directory yields an empty
        /// list; individual corrupt files are skipped rather than failing the whole list.
        /// </summary>
        IReadOnlyList<DraftInfo> List();

        /// <summary>Deletes the draft with the given id. Missing draft is a no-op.</summary>
        void Delete(string id);

        /// <summary>True if a draft with the given id exists.</summary>
        bool Exists(string id);
    }

    /// <summary>Thrown when a draft file exists but cannot be read/parsed.</summary>
    public class DraftStoreException : Exception
    {
        public DraftStoreException(string message, Exception inner) : base(message, inner) { }
    }
}
