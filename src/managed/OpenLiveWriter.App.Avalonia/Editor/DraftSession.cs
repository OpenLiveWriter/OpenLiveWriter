// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// UI-agnostic controller for the document/draft lifecycle: it owns the
    /// <see cref="Current"/> <see cref="PostDocument"/>, tracks unsaved changes, and
    /// drives an <see cref="IDraftStore"/> for New/Save/Open/Delete. Deliberately free
    /// of any WebView/Avalonia dependency so the lifecycle is unit-testable headlessly
    /// against a temp-directory <see cref="FileDraftStore"/>; the shell (MainWindow)
    /// supplies title/body text and the actual dialog interactions.
    /// </summary>
    public class DraftSession
    {
        private readonly IDraftStore _store;

        public DraftSession(IDraftStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            Current = new PostDocument();
        }

        /// <summary>The document currently being edited.</summary>
        public PostDocument Current { get; private set; }

        /// <summary>Raised whenever <see cref="Current"/> is replaced (New/Open/Delete-of-current).</summary>
        public event EventHandler CurrentChanged;

        /// <summary>True when the current document has unsaved edits.</summary>
        public bool IsDirty => Current.IsDirty;

        /// <summary>Flags the current document as having unsaved changes.</summary>
        public void MarkDirty() => Current.IsDirty = true;

        /// <summary>Updates the title, marking dirty only when the value actually changes.</summary>
        public void UpdateTitle(string title)
        {
            title ??= string.Empty;
            if (!string.Equals(Current.Title, title, StringComparison.Ordinal))
            {
                Current.Title = title;
                Current.IsDirty = true;
            }
        }

        /// <summary>
        /// Updates the canonical body for the given format, marking dirty only when
        /// the value or format actually changes.
        /// </summary>
        public void UpdateBody(string body, ContentFormat format = ContentFormat.Html)
        {
            body ??= string.Empty;
            if (format == ContentFormat.Markdown)
            {
                if (Current.BodyFormat != ContentFormat.Markdown
                    || !string.Equals(Current.BodyMarkdown, body, StringComparison.Ordinal))
                {
                    Current.BodyFormat = ContentFormat.Markdown;
                    Current.BodyMarkdown = body;
                    Current.IsDirty = true;
                }
            }
            else
            {
                if (Current.BodyFormat != ContentFormat.Html
                    || !string.Equals(Current.BodyHtml, body, StringComparison.Ordinal))
                {
                    Current.BodyFormat = ContentFormat.Html;
                    Current.BodyHtml = body;
                    Current.IsDirty = true;
                }
            }
        }

        /// <summary>Starts a fresh, empty document (does not persist anything).</summary>
        public void NewPost(bool isPage = false)
        {
            Current = new PostDocument { IsPage = isPage };
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Persists the current document (creating a new draft or overwriting an
        /// existing one). Optionally sets the title/body first. Returns the saved doc.
        /// When <paramref name="bodyFormat"/> is Markdown, <paramref name="bodyMarkdown"/>
        /// is authoritative; otherwise <paramref name="bodyHtml"/> is authoritative.
        /// </summary>
        public PostDocument Save(
            string title = null,
            string bodyHtml = null,
            string bodyMarkdown = null,
            ContentFormat? bodyFormat = null)
        {
            if (title != null) Current.Title = title;

            ContentFormat format = bodyFormat ?? Current.BodyFormat;
            Current.BodyFormat = format;

            if (format == ContentFormat.Markdown)
            {
                if (bodyMarkdown != null)
                    Current.BodyMarkdown = bodyMarkdown;
                if (bodyHtml != null)
                    Current.BodyHtml = bodyHtml;
            }
            else if (bodyHtml != null)
            {
                Current.BodyHtml = bodyHtml;
            }

            return _store.Save(Current);
        }

        /// <summary>
        /// Loads the draft with the given id and makes it current. Returns false if no
        /// such draft exists (leaving the current document untouched).
        /// </summary>
        public bool Open(string id)
        {
            PostDocument doc = _store.Load(id);
            if (doc == null) return false;

            Current = doc;
            CurrentChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Deletes the draft with the given id. If it was the current document, the
        /// session resets to a fresh untitled document.
        /// </summary>
        public void Delete(string id)
        {
            _store.Delete(id);
            if (string.Equals(Current.Id, id, StringComparison.Ordinal))
                NewPost(Current.IsPage);
        }

        /// <summary>
        /// Adopts an externally-built document as the current one without involving the
        /// draft store — used when opening a post fetched from the blog, which is not a
        /// local draft (a later save persists it as one).
        /// </summary>
        public void OpenDocument(PostDocument document)
        {
            Current = document ?? throw new ArgumentNullException(nameof(document));
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Lists saved drafts, most-recently-modified first.</summary>
        public IReadOnlyList<DraftInfo> ListDrafts() => _store.List();
    }
}
