// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport(), Guid("B196B288-BAB4-101A-B69C-00AA00341D07"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleControl
    {

        [PreserveSig]
        int GetControlInfo();

        [PreserveSig]
        int OnMnemonic(
               [In]
                      ref MSG pMsg);

        [PreserveSig]
        int OnAmbientPropertyChange(

                int dispID);

        [PreserveSig]
        int FreezeEvents(

                int bFreeze);

    }
}
