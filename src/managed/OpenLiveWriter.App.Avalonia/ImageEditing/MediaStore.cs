// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.App.Avalonia.ImageEditing
{
    /// <summary>
    /// Per-draft media folder for inserted images, the way the Windows editor
    /// worked: an unpublished image lives on disk and its <c>&lt;img src&gt;</c> is
    /// a <c>file://</c> reference; the publish pipeline (ImagePublisher) uploads it
    /// and rewrites the src to the hosted URL.
    ///
    /// Layout mirrors the draft store: drafts live under the app-data directory in
    /// <c>Drafts/</c>; media lives in a sibling <c>Media/{mediaId}/</c> folder keyed
    /// by the document's <c>PostDocument.MediaId</c> (assigned at creation, so an
    /// image can be filed before the draft is first saved). The root directory is
    /// injected so tests can use a temp dir, like <c>FileDraftStore</c>.
    /// </summary>
    public class MediaStore
    {
        public const string MediaFolderName = "Media";

        private readonly string _rootDirectory;

        /// <param name="rootDirectory">The app-data directory (media goes in its "Media" child).</param>
        public MediaStore(string rootDirectory)
        {
            _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        }

        /// <summary>Creates the default media store rooted at the platform app-data directory.</summary>
        public static MediaStore CreateDefault()
        {
            PlatformContext.EnsureInitialized();
            return new MediaStore(PlatformContext.Services.GetApplicationDataDirectory());
        }

        /// <summary>The media folder for the given document media id (not created on demand).</summary>
        public string GetMediaDirectory(string mediaId)
        {
            ValidateMediaId(mediaId);
            return Path.Combine(_rootDirectory, MediaFolderName, mediaId);
        }

        /// <summary>
        /// Copies <paramref name="sourceFilePath"/> into the document's media folder
        /// under its original file name (deduped as <c>name-2.png</c>, <c>name-3.png</c>…
        /// on collision) and returns the <c>file://</c> URI for the copy.
        /// </summary>
        public string AddImage(string mediaId, string sourceFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
                throw new FileNotFoundException("The image file to insert was not found.", sourceFilePath);

            string mediaDir = GetMediaDirectory(mediaId);
            Directory.CreateDirectory(mediaDir);

            string destPath = DedupePath(mediaDir, Path.GetFileName(sourceFilePath));
            File.Copy(sourceFilePath, destPath);
            return BuildFileUri(destPath);
        }

        /// <summary>Deletes the document's media folder (best-effort when absent).</summary>
        public void DeleteMedia(string mediaId)
        {
            if (string.IsNullOrEmpty(mediaId))
                return;
            string mediaDir = GetMediaDirectory(mediaId);
            if (Directory.Exists(mediaDir))
                Directory.Delete(mediaDir, recursive: true);
        }

        // The media id becomes a single path segment; reject anything that could escape it.
        private static void ValidateMediaId(string mediaId)
        {
            if (string.IsNullOrEmpty(mediaId))
                throw new ArgumentException("A document media id is required.", nameof(mediaId));
            if (mediaId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException($"The media id '{mediaId}' is not a valid folder name.", nameof(mediaId));
        }

        private static string DedupePath(string directory, string fileName)
        {
            string candidate = Path.Combine(directory, fileName);
            if (!File.Exists(candidate))
                return candidate;

            string stem = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            for (int i = 2; ; i++)
            {
                candidate = Path.Combine(directory, $"{stem}-{i}{ext}");
                if (!File.Exists(candidate))
                    return candidate;
            }
        }

        // Uri.AbsoluteUri produces a properly escaped file:///… URI for the path.
        internal static string BuildFileUri(string path) => new Uri(path).AbsoluteUri;
    }
}
