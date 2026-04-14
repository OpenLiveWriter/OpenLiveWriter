// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    [SupportedOSPlatform("windows")]
    public class CabinetFileExtractor
    {
        public CabinetFileExtractor(string cabinetFilePath, string targetDirectory)
        {
            _cabinetFilePath = cabinetFilePath;
            _targetDirectory = targetDirectory;
        }

        public void Extract()
        {
            if (!SetupIterateCabinet(_cabinetFilePath, 0, Psp_File_Callback, 0))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Exception("Failed to extract cabinet file- Error code " + errorCode);
            }
        }

        private uint Psp_File_Callback(uint context, uint notification, IntPtr param1, IntPtr param2)
        {
            uint rtnValue = HRESULT.S_OK;
            switch (notification)
            {
                case SPFILENOTIFY_FILEINCABINET:
                    FILE_IN_CABINET_INFO fileInCabinetInfo = (FILE_IN_CABINET_INFO)Marshal.PtrToStructure(param1, typeof(FILE_IN_CABINET_INFO));
                    fileInCabinetInfo.FullTargetName = Path.Combine(_targetDirectory, fileInCabinetInfo.NameInCabinet);
                    Marshal.StructureToPtr(fileInCabinetInfo, param1, true);
                    rtnValue = FILEOP_DOIT;
                    break;
            }
            return rtnValue;
        }

        private const uint SPFILENOTIFY_FILEINCABINET = 17;
        private const uint FILEOP_DOIT = 1;

        [DllImport("SetupApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupIterateCabinet(string cabinetFile, uint reserved, PSP_FILE_CALLBACK callBack, uint context);

        private delegate uint PSP_FILE_CALLBACK(uint context, uint notification, IntPtr param1, IntPtr param2);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class FILE_IN_CABINET_INFO
        {
            public String NameInCabinet;
            public uint FileSize;
            public uint Win32Error;
            public ushort DosDate;
            public ushort DosTime;
            public ushort DosAttribs;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public System.String FullTargetName;
        }

        private readonly string _cabinetFilePath;
        private readonly string _targetDirectory;
    }
}
