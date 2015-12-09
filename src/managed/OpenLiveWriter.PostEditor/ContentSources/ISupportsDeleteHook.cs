// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    internal interface ISupportsDeleteHook
    {
        void OnSmartContentDelete(ISmartContent smartContent);
    }
}
