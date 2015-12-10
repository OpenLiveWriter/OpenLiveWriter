// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.Extensibility.BlogClient
{
    public interface IBlogProviderButtonDescription
    {
        // id (scoped to blog)
        string Id { get; }

        // tooltip text
        string Description { get; }

        // icons
        string ImageUrl { get; }
        Bitmap Image { get; }

        // supports simple click gesture
        bool SupportsClick { get; }

        // url to navigate to when clicking the button
        string ClickUrl { get; }

        // supports display of content from a drop-down
        bool SupportsContent { get; }

        // url to poll for new content
        string ContentUrl { get; }

        // size for content window
        Size ContentDisplaySize { get; }

        // supports polling for a notification image
        bool SupportsNotification { get; }

        // url to poll for notifications
        string NotificationUrl { get; }
    }

    public interface IBlogProviderButtonNotification
    {
        // interval until next notification check
        TimeSpan PollingInterval { get; }

        // url for custom button image
        Bitmap NotificationImage { get; }

        // text to use in tooltip
        string NotificationText { get; }

        // clear notification image on click?
        bool ClearNotificationOnClick { get; }
    }

}
