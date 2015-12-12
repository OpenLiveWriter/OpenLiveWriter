// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Imports from Kernel32.dll
    /// </summary>
    public class Kernel32
    {
        /// <summary>
        /// Infinite timeout value
        /// </summary>
        public static readonly uint INFINITE = 0xFFFFFFFF;

        /// <summary>
        /// Invalid handle value
        /// </summary>
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetDllDirectory(string pathName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SetErrorMode(uint uMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void ExitProcess(
            UInt32 uExitCode
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool TerminateProcess(
            IntPtr hProcess,
            UInt32 uExitCode
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(
            IntPtr lpModuleName
            );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint GetModuleFileName(
            IntPtr hModule,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpFilename,
            uint nSize
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetProcessWorkingSetSize(
            IntPtr hProcess,
            int dwMinimumWorkingSetSize,
            int dwMaximumWorkingSetSize
            );

        [DllImport("kernel32.dll")]
        public static extern bool Beep(int frequency, int duration);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetLongPathName(
            [In] string lpFileName,
            [Out] StringBuilder lpBuffer,
            [In] int nBufferLength);

        /// <summary>
        /// Converts a potentially short filename to a long filename.
        /// If the file does not exist, <c>null</c> is returned.
        /// </summary>
        public static string GetLongPathName(string fileName)
        {
            return GetLongPathName(fileName, MAX_PATH);
        }

        protected static string GetLongPathName(string fileName, int bufferLen)
        {
            StringBuilder buffer = new StringBuilder(bufferLen);
            int requiredBuffer = GetLongPathName(fileName, buffer, bufferLen);
            if (requiredBuffer == 0)
            {
                // error... probably file does not exist
                return null;
            }
            if (requiredBuffer > bufferLen)
            {
                // The buffer we used was not long enough... try again
                return GetLongPathName(fileName, requiredBuffer);
            }
            else
            {
                return buffer.ToString();
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetShortPathName(
            [In] string lpFileName,
            [Out] StringBuilder lpBuffer,
            [In] int nBufferLength);

        /// <summary>
        /// Converts a potentially long filename to a short filename.
        /// If the file does not exist, <c>null</c> is returned.
        /// </summary>
        public static string GetShortPathName(string fileName)
        {
            return GetShortPathName(fileName, MAX_PATH);
        }

        protected static string GetShortPathName(string fileName, int bufferLen)
        {
            StringBuilder buffer = new StringBuilder(bufferLen);
            int requiredBuffer = GetShortPathName(fileName, buffer, bufferLen);
            if (requiredBuffer == 0)
            {
                // error... probably file does not exist
                return null;
            }
            if (requiredBuffer > bufferLen)
            {
                // The buffer we used was not long enough... try again
                return GetShortPathName(fileName, requiredBuffer);
            }
            else
            {
                return buffer.ToString();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetThreadLocale(int lcid);

        // used to get supported code pages
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool EnumSystemCodePages(CodePageDelegate lpCodePageEnumProc,
            uint dwFlags);

        public const uint CP_INSTALLED = 0x00000001;  // installed code page ids
        public const uint CP_SUPPORTED = 0x00000002;  // supported code page ids

        /// <summary>
        /// Delegate used for EnumSystemCodePages
        /// </summary>
        public delegate bool CodePageDelegate([MarshalAs(UnmanagedType.LPTStr)] string codePageName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree(
            IntPtr hMem
            );

        /// <summary>
        /// Get the Terminal Services/Fast User Switching session ID
        /// for a process ID.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern bool ProcessIdToSessionId(
            [In] uint dwProcessId,
            [Out] out uint pSessionId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            IntPtr lpAttributes,
            uint flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            [In, MarshalAs(UnmanagedType.LPTStr)] string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            uint dwDesiredAccess,
            uint dwFileOffsetHigh,
            uint dwFileOffsetLow,
            uint dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void ZeroMemory(
            IntPtr Destination,
            uint Length);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int UnmapViewOfFile(
            IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ReplaceFile(
            [MarshalAs(UnmanagedType.LPTStr)] string lpReplacedFileName,
            [MarshalAs(UnmanagedType.LPTStr)] string lpReplacementFileName,
            [MarshalAs(UnmanagedType.LPTStr)] string lpBackupFileName,
            uint dwReplaceFlags,
            IntPtr lpExclude,
            IntPtr lpReserved);

        /// <summary>
        /// Get an error code for the last error on this thread.
        /// IntPtr can be converted to a 32 bit Int.
        /// </summary>
        [DllImport("kernel32.dll")]
        [Obsolete("Use Marshal.GetLastWin32Error() with [DllImport(SetLastError = true)] instead!", true)]
        public static extern uint GetLastError();

        /// <summary>
        /// Locks a global memory object and returns a pointer to the first byte
        /// of the object's memory block.
        /// </summary>
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        /// <summary>
        /// Decrements the lock count associated with the HGLOBAL memory block
        /// </summary>
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern UIntPtr GlobalSize(IntPtr hMem);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = true)]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, UIntPtr Length);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateMutex(
            IntPtr lpMutexAttributes,
            int bInitialOwner,
            [MarshalAs(UnmanagedType.LPTStr)] string lpName
            );

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int ReleaseMutex(
            IntPtr hMutex
            );

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateEvent(
            IntPtr lpEventAttributes,
            int bManualReset,
            int bInitialState,
            [MarshalAs(UnmanagedType.LPTStr)] string lpName);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr OpenEvent(
            uint dwDesiredAccess,
            int bInheritHandle,
            [MarshalAs(UnmanagedType.LPTStr)] string lpName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int SetEvent(IntPtr hEvent);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int ResetEvent(IntPtr hEvent);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(
            IntPtr hHandle,
            uint dwMilliseconds
            );

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int CloseHandle(
            IntPtr hObject
            );

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern uint WaitForMultipleObjects(
            uint nCount,
            IntPtr[] pHandles,
            bool bWaitAll,
            uint dwMilliseconds
            );

        /// <summary>
        /// Get the drive type for a particular drive.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern long GetDriveType(string driveLetter);

        /// <summary>
        /// Read a string from an INI file
        /// </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpdefault,
            StringBuilder sbout,
            int nsize,
            string lpFileName);

        /// <summary>
        /// The drive types returned by GetDriveType
        /// </summary>
        public struct DRIVE
        {
            public const int UNKNOWN = 0;
            public const int NO_ROOT_DIR = 1;
            public const int REMOVABLE = 2;
            public const int FIXED = 3;
            public const int REMOTE = 4;
            public const int CDROM = 5;
            public const int RAMDISK = 6;
        }

        /// <summary>
        /// Get the unique ID of the currently executing thread
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        /// <summary>
        /// Gets a temp file using a path and a prefix string.  Note that stringbuilder
        /// should be at least kernel32.MAX_PATH size. uUnique should be 0 to make unique
        /// temp file names, non zero to not necessarily create unique temp file names.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern int GetTempFileName(
            string lpPathName,
            string lpPrefixString,
            Int32 uUnique,
            StringBuilder lpTempFileName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(
            SafeFileHandle hFile, IntPtr lpBuffer,
            uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        // The Max Path constant
        public const int MAX_PATH = 260;

        /// <summary>
        ///    The QueryPerformanceCounter function retrieves the current value of the high-resolution
        ///    performance counter.
        ///    </summary>
        [DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceCounter(ref long performanceCount);

        /// <summary>
        ///    The QueryPerformanceFrequency function retrieves the frequency of the high-resolution
        ///    performance counter, if one exists. The frequency cannot change while the system is running.
        ///    </summary>
        [DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(ref long frequency);

        /// <summary>
        /// The GetTickCount function retrieves the number of milliseconds that have elapsed since the
        /// system was started. It is limited to the resolution of the system timer. To obtain the system
        /// timer resolution, use the GetSystemTimeAdjustment function.
        /// </summary>
        /// <remarks>
        /// The elapsed time is stored as a DWORD value. Therefore, the time will wrap around to zero if
        /// the system is run continuously for 49.7 days.
        /// </remarks>
        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();

        [DllImport("kernel32.dll")]
        public static extern bool FileTimeToLocalFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime,
            out System.Runtime.InteropServices.ComTypes.FILETIME lpLocalFileTime);

        [DllImport("kernel32.dll")]
        public static extern bool LocalFileTimeToFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpLocalFileTime,
            out System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    }

    public class GlobalMemoryStatus
    {
        public GlobalMemoryStatus()
        {
            _memoryStatus.Length = Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            if (!GlobalMemoryStatusEx(ref _memoryStatus))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public int MemoryLoad { get { return _memoryStatus.MemoryLoad; } }
        public long TotalPhysical { get { return _memoryStatus.TotalPhysical; } }
        public long AvailablePhysical { get { return _memoryStatus.AvailablePhysical; } }
        public long TotalPageFile { get { return _memoryStatus.TotalPageFile; } }
        public long AvailablePageFile { get { return _memoryStatus.AvailablePageFile; } }
        public long TotalVirtual { get { return _memoryStatus.TotalVirtual; } }
        public long AvailableVirtual { get { return _memoryStatus.AvailableVirtual; } }
        public long AvailableExtendedVirtual { get { return _memoryStatus.AvailableExtendedVirtual; } }

        private struct MEMORYSTATUSEX
        {
            public int Length;
            public int MemoryLoad;
            public long TotalPhysical;
            public long AvailablePhysical;
            public long TotalPageFile;
            public long AvailablePageFile;
            public long TotalVirtual;
            public long AvailableVirtual;
            public long AvailableExtendedVirtual;
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private MEMORYSTATUSEX _memoryStatus = new MEMORYSTATUSEX();
    }

    /// <summary>
    /// Disposable wrapper class for getting access to the contents of an HGLOBAL
    /// </summary>
    public class HGlobalLock : IDisposable
    {
        /// <summary>
        /// Initialize by locking the HGLOBAL
        /// </summary>
        /// <param name="hGlobal">HGLOBAL to lock and then access</param>
        public HGlobalLock(IntPtr hGlobal)
        {
            this.hGlobal = hGlobal;
            Lock();
        }

        /// <summary>
        /// Unlock on dispose
        /// </summary>
        public void Dispose()
        {
            Unlock();
        }

        /// <summary>
        /// Underlying memory pointed to by the hGlobal
        /// </summary>
        public IntPtr Memory
        {
            get
            {
                Debug.Assert(pData != IntPtr.Zero);
                return pData;
            }
        }

        /// <summary>
        /// Get the size of of the locked global memory handle
        /// </summary>
        public UIntPtr Size
        {
            get
            {
                UIntPtr size = Kernel32.GlobalSize(hGlobal);
                if (size == UIntPtr.Zero)
                {
                    throw new Win32Exception(
                        Marshal.GetLastWin32Error(), "Unexpected error calling GlobalSize");
                }
                return size;
            }
        }

        /// <summary>
        /// Lock the HGLOBAL so as to access the underlying memory block
        /// </summary>
        public void Lock()
        {
            Debug.Assert(pData == IntPtr.Zero);
            pData = Kernel32.GlobalLock(hGlobal);
            if (pData == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "Error occurred while tyring to lock global memory");
            }
        }

        /// <summary>
        /// Unlock the HGLOBAL
        /// </summary>
        public void Unlock()
        {
            if (pData != IntPtr.Zero)
            {
                bool success = Kernel32.GlobalUnlock(hGlobal);
                int lastError = Marshal.GetLastWin32Error();
                if (!success && lastError != ERROR.SUCCESS)
                {
                    throw new Win32Exception(lastError,
                         "Unexpected error occurred calling GlobalUnlock");
                }
                pData = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Clone the underlying memory and return a new HGLOBAL that references
        /// the cloned memory (this HGLOBAL will need to be freed using GlobalFree)
        /// </summary>
        /// <returns>cloned memory block</returns>
        public IntPtr Clone()
        {
            // allocate output memory
            IntPtr hglobOut = Kernel32.GlobalAlloc(GMEM.FIXED, Size);
            if (hglobOut == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "Unexpected error occurred calling GlobalAlloc");
            }

            // got the memory, copy into it and return it
            Kernel32.CopyMemory(hglobOut, Memory, Size);
            return hglobOut;
        }

        /// <summary>
        /// Underlying HGLOBAL
        /// </summary>
        private IntPtr hGlobal = IntPtr.Zero;

        /// <summary>
        /// Pointer to data (aquired by locking the HGLOBAL)
        /// </summary>
        private IntPtr pData = IntPtr.Zero;
    }

    /// <summary>
    /// SetErrorMode flags
    /// </summary>
    public struct SEM
    {
        public const uint FAILCRITICALERRORS = 0x0001;
        public const uint NOGPFAULTERRORBOX = 0x0002;
        public const uint NOALIGNMENTFAULTEXCEPT = 0x0004;
        public const uint NOOPENFILEERRORBOX = 0x8000;
    }

    /// <summary>
    /// ReplaceFile flags
    /// </summary>
    public struct REPLACEFILE
    {
        public const uint WRITE_THROUGH = 0x00000001;
        public const uint IGNORE_MERGE_ERRORS = 0x00000002;
    }

    /// <summary>
    /// Thread priority constants
    /// </summary>
    public struct THREAD_PRIORITY
    {
        public const int NORMAL = 0;
    }

    public struct GMEM
    {
        public const uint FIXED = 0x0000;
        public const uint MOVEABLE = 0x0002;
        public const uint ZEROINIT = 0x0040;
        public const uint SHARE = 0x2000;
    }

    public struct PAGE
    {
        public const uint READONLY = 0x02;
        public const uint READWRITE = 0x04;
        public const uint WRITECOPY = 0x08;
    }

    public struct SEC
    {
        public const uint IMAGE = 0x1000000;
        public const uint RESERVE = 0x4000000;
        public const uint COMMIT = 0x8000000;
        public const uint NOCACHE = 0x10000000;
    }

    public struct FILE_MAP
    {
        public const uint COPY = 0x0001;
        public const uint WRITE = 0x0002;
        public const uint READ = 0x0004;
    }

    public struct FILE_ATTRIBUTE
    {
        public const uint READONLY = 0x00000001;
        public const uint HIDDEN = 0x00000002;
        public const uint SYSTEM = 0x00000004;
        public const uint DIRECTORY = 0x00000010;
        public const uint ARCHIVE = 0x00000020;
        public const uint DEVICE = 0x00000040;
        public const uint NORMAL = 0x00000080;
        public const uint TEMPORARY = 0x00000100;
    }

    /// <summary>
    /// The SECURITY_ATTRIBUTES structure contains the security descriptor for
    /// an object and specifies whether the handle retrieved by specifying this
    /// structure is inheritable
    /// </summary>
    public struct SECURITY_ATTRIBUTES
    {
        /// <summary>
        /// Specifies the size, in bytes, of this structure. Set this value to
        /// the size of the SECURITY_ATTRIBUTES structure.
        /// </summary>
        public uint nLength;

        /// <summary>
        /// Pointer to a security descriptor for the object that controls the
        /// sharing of it. If NULL is specified for this member, the object
        /// is assigned the default security descriptor of the calling process.
        /// </summary>
        public IntPtr lpSecurityDescriptor;

        /// <summary>
        /// Specifies whether the returned handle is inherited when a new
        /// process is created. If this member is TRUE, the new process inherits
        /// the handle.
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool bInheritHandle;
    }

    /// <summary>
    /// Wait constants used in synchronization functions
    /// </summary>
    public struct WAIT
    {
        public const uint FAILED = 0xFFFFFFFF;
        public const uint OBJECT_0 = 0x00000000;
        public const uint ABANDONED = 0x00000080;
        public const uint TIMEOUT = 258;
    }

    /// <summary>
    /// Constants used for getting access to synchronization events
    /// </summary>
    public struct EVENT
    {
        public const uint MODIFY_STATE = 0x0002;
    }
}
