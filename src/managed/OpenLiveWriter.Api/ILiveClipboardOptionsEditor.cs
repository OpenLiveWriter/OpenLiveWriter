// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;

namespace OpenLiveWriter.Api
{
    public interface ILiveClipboardOptionsEditor
    {
        void EditLiveClipboardOptions(IWin32Window dialogOwner) ;
    }
}
