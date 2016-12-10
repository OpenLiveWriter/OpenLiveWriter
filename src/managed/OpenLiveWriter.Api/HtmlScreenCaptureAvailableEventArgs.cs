// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.Api
{
    using System;
    using System.Drawing;

    using JetBrains.Annotations;

    /// <summary>
    /// Provides data for the HtmlScreenCaptureAvailable event.
    /// </summary>
    public class HtmlScreenCaptureAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlScreenCaptureAvailableEventArgs"/> class.
        /// </summary>
        /// <param name="bitmap">
        /// Currently available HTML screen shot.
        /// </param>
        public HtmlScreenCaptureAvailableEventArgs([NotNull] Bitmap bitmap)
        {
            this.Bitmap = bitmap;
        }

        /// <summary>
        /// Gets the currently available HTML screen shot.
        /// </summary>
        [NotNull]
        public Bitmap Bitmap { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the screen capture has been completed. Set this value to
        /// false to indicate that the screen capture is not yet completed. This property is useful
        /// in the case where the content to be captured has a secondary loading step (such as
        /// a media player loading a video) which must occur before the screen capture is completed.
        /// </summary>
        public bool CaptureCompleted { get; set; } = true;
    }
}