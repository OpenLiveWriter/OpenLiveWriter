// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System.Drawing;

    /// <summary>
    /// Event handler that is fired when a content is resized.
    /// </summary>
    /// <param name="newSize">The new size of the content</param>
    /// <param name="completed">Whether the sizing operation has completed or not</param>
    public delegate void ContentResizedEventHandler(Size newSize, bool completed);
}