// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Base implemetnation of interface used for customizing the UI of MSHTML.
    /// Provides correct default behavior for all methods and allows selective
    /// overrides for targeted customization.
    /// </summary>
    public class IDocHostShowUIBaseImpl : IDocHostShowUI
    {
        public virtual int ShowMessage(IntPtr hwnd, string lpstrText, string lpstrCaption, uint dwType, string lpstrHelpFile, uint dwHelpContext, out int plResult)
        {
            // return false to allow MSHTML to show its own UI (no customization)
            plResult = 0;
            return HRESULT.S_FALSE;
        }

        public virtual int ShowHelp(IntPtr hwnd, string lpstrHelpFile, uint uCommand, uint dwData, POINT ptMouse, IntPtr pDispatchObjectHit)
        {
            // return HRESULT.S_FALSE to allow default processing
            return HRESULT.S_FALSE;
        }
    }

}

