// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Text;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com
{

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("BB2E617C-0920-11d1-9A0B-00C04FC2D6C1")]
    public interface IExtractImage
    {
        void GetLocation(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPathBuffer,
            uint cch,
            out uint pdwPriority,
            [In] ref SIZE prgSize,
            uint dwRecClrDepth,
            [In, Out] ref uint pdwFlags);

        void Extract(
             [Out] out IntPtr phBmpThumbnail);
    }

    public struct IEIFLAG
    {
        public const uint ASYNC = 0x0001;      // ask the extractor if it supports ASYNC extract (free threaded)
        public const uint CACHE = 0x0002;      // returned from the extractor if it does NOT cache the thumbnail
        public const uint ASPECT = 0x0004;      // passed to the extractor to beg it to render to the aspect ratio of the supplied rect
        public const uint OFFLINE = 0x0008;      // if the extractor shouldn't hit the net to get any content neede for the rendering
        public const uint GLEAM = 0x0010;      // does the image have a gleam ? this will be returned if it does
        public const uint SCREEN = 0x0020;      // render as if for the screen  (this is exlusive with IEIFLAG_ASPECT )
        public const uint ORIGSIZE = 0x0040;      // render to the approx size passed, but crop if neccessary
        public const uint NOSTAMP = 0x0080;      // returned from the extractor if it does NOT want an icon stamp on the thumbnail
        public const uint NOBORDER = 0x0100;      // returned from the extractor if it does NOT want an a border around the thumbnail
        public const uint QUALITY = 0x0200;      // passed to the Extract method to indicate that a slower, higher quality image is desired, re-compute the thumbnail
        public const uint REFRESH = 0x0400;      // returned from the extractor if it would like to have Refresh Thumbnail available
    }

}


