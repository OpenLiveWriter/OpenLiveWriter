// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OpenLiveWriter.Publishing.Drafts
{
    /// <summary>
    /// File-system <see cref="IDraftStore"/> that persists one JSON file per draft
    /// under a caller-supplied directory. The directory is resolved by the host
    /// (via <c>OpenLiveWriter.Platform</c> on the app side, or a temp dir in tests),
    /// so this type never hardcodes a platform path.
    ///
    /// Robustness: a missing directory is treated as an empty store (created lazily
    /// on save); a corrupt file fails <see cref="Load"/> with <see cref="DraftStoreException"/>
    /// but is silently skipped by <see cref="List"/> so one bad file cannot break the
    /// Open Drafts list.
    /// </summary>
    public sealed class FileDraftStore : IDraftStore
    {
        // Distinct extension keeps drafts recognizable and avoids clobbering other JSON.
        private const string DraftExtension = ".oldraft.json";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private readonly string _directory;

        public FileDraftStore(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        /// <summary>The directory this store reads/writes draft files in.</summary>
        public string Directory => _directory;

        public PostDocument Save(PostDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            System.IO.Directory.CreateDirectory(_directory);

            DateTime now = DateTime.UtcNow;
            if (string.IsNullOrEmpty(document.Id))
            {
                document.Id = Guid.NewGuid().ToString("N");
                document.DateCreatedUtc = now;
            }
            else if (document.DateCreatedUtc == default)
            {
                document.DateCreatedUtc = now;
            }

            document.DateModifiedUtc = now;

            string json = JsonSerializer.Serialize(document, SerializerOptions);
            // Write atomically-ish: temp file then move, so a crash mid-write cannot
            // leave a truncated (corrupt) draft in place.
            string finalPath = PathForId(document.Id);
            string tempPath = finalPath + ".tmp";
            File.WriteAllText(tempPath, json);
            if (File.Exists(finalPath))
                File.Delete(finalPath);
            File.Move(tempPath, finalPath);

            document.IsDirty = false;
            return document;
        }

        public PostDocument Load(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            string path = PathForId(id);
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                var doc = JsonSerializer.Deserialize<PostDocument>(json, SerializerOptions);
                if (doc == null)
                    throw new DraftStoreException($"Draft '{id}' deserialized to null.", null);

                // Guard against tampering: keep the id consistent with the file name.
                doc.Id = id;
                doc.IsDirty = false;
                return doc;
            }
            catch (JsonException ex)
            {
                throw new DraftStoreException($"Draft '{id}' is corrupt and could not be read.", ex);
            }
            catch (IOException ex)
            {
                throw new DraftStoreException($"Draft '{id}' could not be read.", ex);
            }
        }

        public IReadOnlyList<DraftInfo> List()
        {
            if (!System.IO.Directory.Exists(_directory))
                return Array.Empty<DraftInfo>();

            var results = new List<DraftInfo>();
            foreach (string path in System.IO.Directory.EnumerateFiles(_directory, "*" + DraftExtension))
            {
                DraftInfo info = TryReadInfo(path);
                if (info != null)
                    results.Add(info);
            }

            // Most-recently-modified first; deterministic tie-break by title then id.
            return results
                .OrderByDescending(d => d.DateModifiedUtc)
                .ThenBy(d => d.Title, StringComparer.OrdinalIgnoreCase)
                .ThenBy(d => d.Id, StringComparer.Ordinal)
                .ToList();
        }

        public void Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            string path = PathForId(id);
            if (File.Exists(path))
                File.Delete(path);
        }

        public bool Exists(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return File.Exists(PathForId(id));
        }

        private DraftInfo TryReadInfo(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var doc = JsonSerializer.Deserialize<PostDocument>(json, SerializerOptions);
                if (doc == null) return null;

                string id = Path.GetFileName(path);
                if (id.EndsWith(DraftExtension, StringComparison.OrdinalIgnoreCase))
                    id = id.Substring(0, id.Length - DraftExtension.Length);

                return new DraftInfo(id, doc.Title, doc.DateModifiedUtc);
            }
            catch (JsonException)
            {
                // Corrupt file — skip it so it can't break the whole list.
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        private string PathForId(string id) => Path.Combine(_directory, id + DraftExtension);
    }
}
