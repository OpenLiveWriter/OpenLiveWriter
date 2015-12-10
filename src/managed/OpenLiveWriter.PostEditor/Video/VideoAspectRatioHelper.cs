// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.Video
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Defines the available video aspect ratios.
    /// </summary>
    public enum VideoAspectRatioType
    {
        /// <summary>
        /// Used when the aspect ratio is unknown and we don't want to force a specific aspect ratio.
        /// </summary>
        Unknown,

        /// <summary>
        /// Represents a standard 4:3 aspect ratio.
        /// </summary>
        Standard,

        /// <summary>
        /// Represents a widescreen 16:9 ratio.
        /// </summary>
        Widescreen
    }

    /// <summary>
    /// Provides helper methods and data for dealing with video aspect ratios.
    /// </summary>
    public static class VideoAspectRatioHelper
    {
        /// <summary>
        /// The standard 4:3 aspect ratio.
        /// </summary>
        public const float StandardAspectRatio = 4f / 3f;

        /// <summary>
        /// The widescreen 16:9 aspect ratio.
        /// </summary>
        public const float WidescreenAspectRatio = 16f / 9f;

        /// <summary>
        /// Computes a new size by keeping the width the same and resizing the height to fit the provided aspect ratio
        /// type.
        /// </summary>
        /// <param name="videoAspectRatioType">The aspect ratio type that bounds the height.</param>
        /// <param name="currentSize">The current size of the video.</param>
        /// <returns>A new size with the same width but a new height that conforms to the provided aspect
        /// ratio type.</returns>
        public static Size ResizeHeightToAspectRatio(VideoAspectRatioType videoAspectRatioType, Size currentSize)
        {
            if (videoAspectRatioType == VideoAspectRatioType.Unknown)
            {
                throw new ArgumentException("videoAspectRatioType cannot be VideoAspectRatioType.Unknown");
            }

            return ResizeHeightToAspectRatio(
                videoAspectRatioType == VideoAspectRatioType.Standard ? StandardAspectRatio : WidescreenAspectRatio,
                currentSize);
        }

        /// <summary>
        /// Computes a new size by keeping the width the same and resizing the height to fit the provided aspect ratio.
        /// </summary>
        /// <param name="newAspectRatio">The aspect ratio that bounds the height.</param>
        /// <param name="currentSize">The current size of the video.</param>
        /// <returns>A new size with the same width but a new height that conforms to the provided aspect
        /// ratio.</returns>
        public static Size ResizeHeightToAspectRatio(float newAspectRatio, Size currentSize)
        {
            float newHeight = currentSize.Width / newAspectRatio;
            return new Size(currentSize.Width, (int)Math.Round(newHeight));
        }
    }
}
