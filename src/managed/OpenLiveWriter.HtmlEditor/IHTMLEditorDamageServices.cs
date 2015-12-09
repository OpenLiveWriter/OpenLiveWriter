// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Operations for notifying the document that damage has occurred.
    /// </summary>
    public interface IHTMLEditorDamageServices
    {
        IDisposable CreateDamageTracker(MarkupRange range, bool includeAdjacentWords);
        IDisposable CreateDamageTracker(MarkupPointer start, MarkupPointer end, bool includeAdjacentWords);
        IDisposable CreateDeleteDamageTracker(MarkupRange range);
        IDisposable CreateIgnoreDamage();
        void AddDamage(MarkupRange range);
        void AddDamage(MarkupRange range, bool includeAdjacentWords);
    }
}
