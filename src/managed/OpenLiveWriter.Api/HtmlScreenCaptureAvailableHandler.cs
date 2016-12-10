// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.Api
{
    using JetBrains.Annotations;

    /// <summary>
    /// Represents the method that will handle the HtmlScreenCaptureAvailable event of the HtmlScreenCapture class.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="HtmlScreenCaptureAvailableEventArgs"/> instance containing the event data.</param>
    public delegate void HtmlScreenCaptureAvailableHandler(
        [NotNull] object sender,
        [NotNull] HtmlScreenCaptureAvailableEventArgs e);
}