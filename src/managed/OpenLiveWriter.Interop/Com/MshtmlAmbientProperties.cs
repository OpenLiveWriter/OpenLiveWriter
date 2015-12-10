// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Interop.Com
{
    public struct MSHTML_DISPID
    {
        public const int WINDOWOBJECT = (-5500);
        public const int LOCATIONOBJECT = (-5506);
        public const int HISTORYOBJECT = (-5507);
        public const int NAVIGATOROBJECT = (-5508);
        public const int SECURITYCTX = (-5511);
        public const int AMBIENT_DLCONTROL = (-5512);
        public const int AMBIENT_USERAGENT = (-5513);
        public const int SECURITYDOMAIN = (-5514);
    }

    public struct DLCTL
    {
        public const int DLIMAGES = 0x00000010;
        public const int VIDEOS = 0x00000020;
        public const int BGSOUNDS = 0x00000040;
        public const int NO_SCRIPTS = 0x00000080;
        public const int NO_JAVA = 0x00000100;
        public const int NO_RUNACTIVEXCTLS = 0x00000200;
        public const int NO_DLACTIVEXCTLS = 0x00000400;
        public const int DOWNLOADONLY = 0x00000800;
        public const int NO_FRAMEDOWNLOAD = 0x00001000;
        public const int RESYNCHRONIZE = 0x00002000;
        public const int PRAGMA_NO_CACHE = 0x00004000;
        public const int NO_BEHAVIORS = 0x00008000;
        public const int NO_METACHARSET = 0x00010000;
        public const int URL_ENCODING_DISABLE_UTF8 = 0x00020000;
        public const int URL_ENCODING_ENABLE_UTF8 = 0x00040000;
        public const int NOFRAMES = 0x00080000;
        public const int FORCEOFFLINE = 0x10000000;
        public const int NO_CLIENTPULL = 0x20000000;
        public const int SILENT = 0x40000000;
        public const int OFFLINEIFNOTCONNECTED = unchecked((int)0x80000000);
        public const int OFFLINE = OFFLINEIFNOTCONNECTED;
    }

}
