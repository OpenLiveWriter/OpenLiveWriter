// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Optional seam for the Windows-style two-stage image upload on publish: when
    /// an inserted image is displayed smaller than its natural size, the publish
    /// pipeline uploads BOTH a resized display copy (becomes the <c>&lt;img src&gt;</c>)
    /// and the original full-size bytes (becomes the click-through link target).
    ///
    /// The Publishing layer has no image codec, so the two operations are injected:
    /// the Avalonia shell wires these to SkiaSharp (<c>ImageEditorService</c>);
    /// tests can inject fakes. A null <see cref="PublishImageResizer"/> disables
    /// resizing entirely (single-upload behavior).
    /// </summary>
    public sealed class PublishImageResizer
    {
        /// <summary>
        /// Creates the seam. <paramref name="probeNaturalSize"/> returns the image's
        /// natural pixel dimensions (null when the bytes are not a decodable image —
        /// the image then publishes without resizing). <paramref name="resize"/>
        /// re-encodes the image at the requested display size and MUST return PNG
        /// bytes (the resized copy is uploaded as <c>image/png</c>).
        /// </summary>
        public PublishImageResizer(
            Func<byte[], ValueTuple<int, int>?> probeNaturalSize,
            Func<byte[], int, int, byte[]> resize)
        {
            ProbeNaturalSize = probeNaturalSize ?? throw new ArgumentNullException(nameof(probeNaturalSize));
            Resize = resize ?? throw new ArgumentNullException(nameof(resize));
        }

        /// <summary>Natural pixel dimensions of the encoded bytes; null when undecodable.</summary>
        public Func<byte[], ValueTuple<int, int>?> ProbeNaturalSize { get; }

        /// <summary>Re-encodes the image at (width, height); returns PNG bytes.</summary>
        public Func<byte[], int, int, byte[]> Resize { get; }
    }
}
