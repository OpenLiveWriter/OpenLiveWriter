// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f6df-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IHTMLPainterEventInfoRaw
    {
        void GetEventInfoFlags(out HTML_PAINT_EVENT_FLAGS plEventInfoFlags);

        void GetEventTarget(out IHTMLElement ppElement);

        void SetCursor(int lPartID);

        void StringFromPartID(
            int lPartID,
            out IntPtr ppElement);
    }

    /// <summary>
    /// Enum for flags returned from GetEventInfoFlags
    /// </summary>
    [Flags]
    public enum HTML_PAINT_EVENT_FLAGS : int
    {
        HTMLPAINT_EVENT_TARGET = 0x0001,
        HTMLPAINT_EVENT_SETCURSOR = 0x0002
    };

}
