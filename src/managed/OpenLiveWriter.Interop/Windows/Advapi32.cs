// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OpenLiveWriter.Interop.Windows
{

    public class Advapi32
    {
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RegOpenKeyEx(
            UIntPtr hKey,
            [MarshalAs(UnmanagedType.LPTStr)] string lpSubKey,
            uint ulOptions,
            uint samDesired,
            out UIntPtr phkResult
            );

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(
            UIntPtr hKey
            );

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern int RegNotifyChangeKeyValue(
            UIntPtr hKey,
            bool bWatchSubtree,
            uint dwNotifyFilter,
            SafeWaitHandle hEvent,
            bool fAsynchronous
            );

    }

    public struct STANDARD_RIGHTS
    {
        public const uint READ = 0x00020000;
        public const uint SYNCHRONIZE = 0x00100000;
    }

    public struct KEY
    {
        public const uint QUERY_VALUE = 0x0001;
        public const uint ENUMERATE_SUB_KEYS = 0x0008;
        public const uint NOTIFY = 0x0010;
        public const uint READ = (STANDARD_RIGHTS.READ | KEY.QUERY_VALUE | KEY.ENUMERATE_SUB_KEYS | KEY.NOTIFY) & ~(STANDARD_RIGHTS.SYNCHRONIZE);
    }

    //from WinReg.h
    public struct HKEY
    {
        public static readonly UIntPtr CLASSES_ROOT = new UIntPtr(0x80000000);
        public static readonly UIntPtr CURRENT_USER = new UIntPtr(0x80000001);
        public static readonly UIntPtr LOCAL_MACHINE = new UIntPtr(0x80000002);
        public static readonly UIntPtr USERS = new UIntPtr(0x80000003);
        public static readonly UIntPtr PERFORMANCE_DATA = new UIntPtr(0x80000004);
        public static readonly UIntPtr CURRENT_CONFIG = new UIntPtr(0x80000005);
        public static readonly UIntPtr DYN_DATA = new UIntPtr(0x80000006);
    }

    public struct REG_NOTIFY_CHANGE
    {
        public const uint NAME = 0x00000001;
        public const uint ATTRIBUTES = 0x00000002;
        public const uint LAST_SET = 0x00000004;
        public const uint SECURITY = 0x00000008;
    }
}
