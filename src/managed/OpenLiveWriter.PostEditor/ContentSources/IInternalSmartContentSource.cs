// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    /// <summary>
    /// A SmartContentSource implementing this interface indicates that it wishes to receive InternalSmartContent
    /// in calls that generate HTML.
    /// </summary>
    interface IInternalSmartContentSource
    {
        /// <summary>
        /// Returns HTML such that IDataObject.GetData(CF_UNICODETEXT,...) yields the desired plain text representation of the smart content.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="publishingContext"></param>
        /// <returns></returns>
        string GeneratePlainTextHtml(ISmartContent content, IPublishingContext publishingContext);
    }
}
