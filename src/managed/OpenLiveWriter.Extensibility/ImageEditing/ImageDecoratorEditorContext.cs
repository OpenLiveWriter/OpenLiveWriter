// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.Extensibility.ImageEditing
{
    /// <summary>
    /// Provides runtime callbacks for ImageDecoratorEditors to use when editing an image's decoration settings.
    /// </summary>
    public interface ImageDecoratorEditorContext
    {
        /// <summary>
        /// Callback to tell the editor that the image decorators should be reapplied.
        /// </summary>
        void ApplyDecorator();

        /// <summary>
        /// The settings for the decorator.
        /// </summary>
        IProperties Settings { get; }

        /// <summary>
        /// The size of the original image.
        /// </summary>
        Size SourceImageSize { get; }

        /// <summary>
        /// The uri of the source image.
        /// </summary>
        Uri SourceImageUri { get; }

        /// <summary>
        /// The rotation of the image with respect to the original source image.
        /// </summary>
        RotateFlipType ImageRotation { get; set; }

        /// <summary>
        /// The img HTML element associated with current image.
        /// </summary>
        IHTMLElement ImgElement { get; }

        float? EnforcedAspectRatio { get; }

        /// <summary>
        /// Creates an undo unit that tracks editor changes so they can be pushed onto the undo/redo stack.
        /// </summary>
        /// <returns></returns>
        IImageDecoratorUndoUnit CreateUndoUnit();

        CommandManager CommandManager { get; }
    }
}
