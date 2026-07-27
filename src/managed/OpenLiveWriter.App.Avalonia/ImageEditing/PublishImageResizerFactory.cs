// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.App.Avalonia.ImageEditing
{
    /// <summary>
    /// Wires the SkiaSharp image operations (<see cref="ImageEditorService"/>) into
    /// the Publishing layer's two-stage image-upload seam (<see cref="PublishImageResizer"/>):
    /// natural dimensions come from a header-only Skia decode, and the resized
    /// display copy is produced by the same Mitchell-cubic <see cref="ImageEditorService.Resize"/>
    /// Picture Tools uses, emitting PNG. Publishing itself stays codec-free so the
    /// decision logic remains unit-testable with fakes.
    /// </summary>
    public static class PublishImageResizerFactory
    {
        /// <summary>Creates the resizer seam the shell passes to the publish pipeline.</summary>
        public static PublishImageResizer Create() =>
            new PublishImageResizer(ProbeNaturalSize, ImageEditorService.Resize);

        private static System.ValueTuple<int, int>? ProbeNaturalSize(byte[] imageBytes) =>
            ImageEditorService.TryGetDimensions(imageBytes, out int width, out int height)
                ? (width, height)
                : (System.ValueTuple<int, int>?)null;
    }
}
