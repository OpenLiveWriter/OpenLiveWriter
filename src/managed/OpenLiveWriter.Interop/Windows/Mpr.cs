// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows
{
    public class Mpr
    {
        public const int UNIVERSAL_NAME_INFO_LEVEL =   0x00000001;

        [DllImport("mpr.dll", CharSet=CharSet.Auto)]
        public static extern int WNetGetUniversalName(
            [In, MarshalAs(UnmanagedType.LPTStr)] string lpLocalPath,
            [In] int dwInfoLevel,
            [In] IntPtr buffer,
            [In, Out] ref int bufferSize);

        public struct UNIVERSAL_NAME_INFO
        {
            [MarshalAs(UnmanagedType.LPTStr)] public string lpUniversalName;
        }

        /// <summary>
        /// Get the UNC path of a file/dir on a locally mounted network share.
        /// Returns <c>null</c> if the path cannot be mapped to UNC for any reason.
        /// </summary>
        public static string GetUniversalName(string path)
        {
            return GetUniversalName(path, 100);
        }

        protected static string GetUniversalName(string path, int bufferSize)
        {
            IntPtr buffer = IntPtr.Zero ;
            try
            {
                buffer = Marshal.AllocHGlobal(bufferSize) ;

                int oldBufferSize = bufferSize;

                int errorCode =
                    WNetGetUniversalName(path, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref bufferSize);

                if (errorCode == ERROR.SUCCESS)
                {
                    // all clear
                    UNIVERSAL_NAME_INFO uni =
                        (UNIVERSAL_NAME_INFO) Marshal.PtrToStructure( buffer, typeof(UNIVERSAL_NAME_INFO) ) ;

                    return uni.lpUniversalName ;
                }
                else if (errorCode == ERROR.MORE_DATA)
                {
                    Debug.Assert( bufferSize > oldBufferSize ) ;

                    // more data avilable....
                    return GetUniversalName(path,bufferSize);
                }
                else
                {
                    // error occurred... probably invalid path or device went away

                    // Call Debug.Fail on unexpected errors
                    switch (errorCode)
                    {
                            // these two errors are expected
                        case ERROR.BAD_DEVICE:
                        case ERROR.NOT_CONNECTED:
                            break;
                        default:
                            Debug.WriteLine("Encountered error " + errorCode + " while trying to get universal name for " + path);
                            break;
                    }

                    return null;
                }
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
