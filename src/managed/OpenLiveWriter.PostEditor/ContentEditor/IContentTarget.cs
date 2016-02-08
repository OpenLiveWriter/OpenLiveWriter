// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Represents the target type of content the canvas
    /// is editing.  This might be a blog post in
    /// Open Live Writer or an email.
    /// </summary>
    [Guid("0BACBB95-5E96-4336-B9B2-97920BD0E32C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IContentTarget
    {
        /// <summary>
        /// Value indicating whether the feature is supported
        /// by the current publishing context.
        /// </summary>
        bool SupportsFeature(ContentEditorFeature featureName);

        string ProductName { get; }
    }
}
