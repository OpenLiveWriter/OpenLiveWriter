// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    /// <summary>
    /// This should only be implemented by SmartContentSource.  The purpose of this
    /// is to allow the source to decide if they want to update the editor based on on the current editors html
    /// that will be replaced and by the new html that the editor just got from the content source.
    /// </summary>
    internal interface IContentUpdateFilter
    {
        bool ShouldUpdateContent(string oldHTML, string newHTML);
    }
}
