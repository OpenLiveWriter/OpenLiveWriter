// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    public interface IInternalContent
    {
        DateTime? RefreshCallback { get; set; }
        Object ObjectState { get; set; }
        String Id { get; }
    }
}
