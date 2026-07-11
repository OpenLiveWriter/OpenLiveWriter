// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OpenLiveWriter.Publishing.Accounts
{
    /// <summary>
    /// File-system <see cref="IAccountStore"/> that persists one JSON file per account
    /// under a caller-supplied directory (resolved by the host via
    /// <c>OpenLiveWriter.Platform</c>, or a temp dir in tests — never hardcoded here).
    /// The current-selection pointer is a small sibling JSON file.
    ///
    /// Robustness mirrors <c>FileDraftStore</c>: a missing directory is an empty store
    /// (created lazily on save); a corrupt account file fails <see cref="Load"/> with
    /// <see cref="AccountStoreException"/> but is skipped by <see cref="List"/>; a corrupt
    /// selection file resolves to "no current account".
    ///
    /// Only non-secret metadata is written here — the password is stored separately in
    /// the credential store.
    /// </summary>
    public sealed class FileAccountStore : IAccountStore
    {
        private const string AccountExtension = ".olaccount.json";
        private const string CurrentFileName = "current.json";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private readonly string _directory;

        public FileAccountStore(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        /// <summary>The directory this store reads/writes account files in.</summary>
        public string Directory => _directory;

        public BlogAccount Save(BlogAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            System.IO.Directory.CreateDirectory(_directory);

            if (string.IsNullOrEmpty(account.Id))
                account.Id = Guid.NewGuid().ToString("N");

            string json = JsonSerializer.Serialize(account, SerializerOptions);
            WriteAtomic(PathForId(account.Id), json);
            return account;
        }

        public BlogAccount Load(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            string path = PathForId(id);
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                var account = JsonSerializer.Deserialize<BlogAccount>(json, SerializerOptions);
                if (account == null)
                    throw new AccountStoreException($"Account '{id}' deserialized to null.", null);

                // Keep the id consistent with the file name (guard against tampering).
                account.Id = id;
                return account;
            }
            catch (JsonException ex)
            {
                throw new AccountStoreException($"Account '{id}' is corrupt and could not be read.", ex);
            }
            catch (IOException ex)
            {
                throw new AccountStoreException($"Account '{id}' could not be read.", ex);
            }
        }

        public IReadOnlyList<BlogAccount> List()
        {
            if (!System.IO.Directory.Exists(_directory))
                return Array.Empty<BlogAccount>();

            var results = new List<BlogAccount>();
            foreach (string path in System.IO.Directory.EnumerateFiles(_directory, "*" + AccountExtension))
            {
                BlogAccount account = TryRead(path);
                if (account != null)
                    results.Add(account);
            }

            // Deterministic order by display label then id.
            return results
                .OrderBy(a => a.DisplayLabel, StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.Id, StringComparer.Ordinal)
                .ToList();
        }

        public void Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            string path = PathForId(id);
            if (File.Exists(path))
                File.Delete(path);

            // Clear the current pointer if it referenced the deleted account.
            if (string.Equals(CurrentAccountId, id, StringComparison.Ordinal))
                CurrentAccountId = null;
        }

        public bool Exists(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return File.Exists(PathForId(id));
        }

        public string CurrentAccountId
        {
            get
            {
                string path = CurrentPath();
                if (!File.Exists(path)) return null;
                try
                {
                    string json = File.ReadAllText(path);
                    var pointer = JsonSerializer.Deserialize<CurrentPointer>(json, SerializerOptions);
                    return pointer?.CurrentAccountId;
                }
                catch (JsonException)
                {
                    // Corrupt selection file — treat as "no current account".
                    return null;
                }
                catch (IOException)
                {
                    return null;
                }
            }
            set
            {
                System.IO.Directory.CreateDirectory(_directory);
                string json = JsonSerializer.Serialize(new CurrentPointer { CurrentAccountId = value }, SerializerOptions);
                WriteAtomic(CurrentPath(), json);
            }
        }

        private BlogAccount TryRead(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var account = JsonSerializer.Deserialize<BlogAccount>(json, SerializerOptions);
                if (account == null) return null;

                string id = Path.GetFileName(path);
                if (id.EndsWith(AccountExtension, StringComparison.OrdinalIgnoreCase))
                    id = id.Substring(0, id.Length - AccountExtension.Length);
                account.Id = id;
                return account;
            }
            catch (JsonException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static void WriteAtomic(string finalPath, string contents)
        {
            string tempPath = finalPath + ".tmp";
            File.WriteAllText(tempPath, contents);
            if (File.Exists(finalPath))
                File.Delete(finalPath);
            File.Move(tempPath, finalPath);
        }

        private string PathForId(string id) => Path.Combine(_directory, id + AccountExtension);
        private string CurrentPath() => Path.Combine(_directory, CurrentFileName);

        private sealed class CurrentPointer
        {
            public string CurrentAccountId { get; set; }
        }
    }
}
