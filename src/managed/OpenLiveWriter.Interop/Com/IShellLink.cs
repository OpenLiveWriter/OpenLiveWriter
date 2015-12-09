// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Interface used to process accelerators and change UI activation for a band object
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    public interface IShellLink
    {
        void GetPath(
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cch,
            IntPtr pfd,
            SLGP fFlags);

        void GetIDList();
        void SetIDList();
        void GetDescription();
        void SetDescription();
        void GetWorkingDirectory();
        void SetWorkingDirectory();
        void GetArguments();
        void SetArguments();
        void GetHotkey();
        void SetHotkey();
        void GetShowCmd();
        void SetShowCmd();
        void GetIconLocation();
        void SetIconLocation();
        void SetRelativePath();

        void Resolve(
            IntPtr hwnd,
            SLR fFlags);

        void SetPath(string path);
    }

    [Flags]
    public enum SLGP : uint
    {
        SHORTPATH = 0x1,
        UNCPRIORITY = 0x2,
        RAWPATH = 0x4
    }

    [Flags]
    public enum SLR : uint
    {
        DEFAULT = 0x0,
        NO_UI = 0x1,
        ANY_MATCH = 0x2,
        UPDATE = 0x4,
        NOUPDATE = 0x8,
        NOSEARCH = 0x10,
        NOTRACK = 0x20,
        NOLINKINFO = 0x40,
        INVOKE_MSI = 0x80,
        NO_UI_WITH_MSG_PUMP = 0x101
    }
}
