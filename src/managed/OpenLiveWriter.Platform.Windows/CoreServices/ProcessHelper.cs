// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for ProcessHelper.
    /// </summary>
    public sealed class ProcessHelper
    {
        public static string GetCurrentProcessName()
        {
            // allocate buffer for getting process name
            uint bufferSize = Kernel32.MAX_PATH * 4;
            StringBuilder buffer = new StringBuilder((int)bufferSize);

            // get the module handle
            IntPtr hModule = Kernel32.GetModuleHandle(IntPtr.Zero);
            Debug.Assert(hModule != IntPtr.Zero);

            // get the module file name
            uint result = Kernel32.GetModuleFileName(hModule, buffer, bufferSize);
            Debug.Assert(result != 0);

            // get the base name of the process and see if it matches IE
            return Path.GetFileName(buffer.ToString());
        }

        public static string GetProcessName(int processId)
        {
            using (Process p = Process.GetProcessById(processId))
            {
                return GetProcessName(p.Handle);
            }
        }

        public static int[] GetProcessIdsByName(string processName)
        {
            if (processName != null)
                processName = processName.ToUpperInvariant();

            Process[] processes = Process.GetProcesses();
            ArrayList list = new ArrayList(processes.Length);
            try
            {
                foreach (Process p in processes)
                {
                    try
                    {
                        string pName = GetProcessName(p.Handle);
                        if (processName == pName || (pName != null && processName == pName.ToUpperInvariant()))
                            list.Add(p.Id);
                    }
                    catch
                    {
                        // Certain processes, like Idle, cause security exceptions when we
                        // try to access them.  Maybe processes in other sessions too?
                    }
                }

                return (int[])list.ToArray(typeof(int));
            }
            finally
            {
                foreach (Process p in processes)
                    try
                    {
                        p.Dispose();
                    }
                    catch { }
            }
        }

        /// <summary>
        /// Faster than the version that takes a pid.
        /// </summary>
        public static string GetProcessName(IntPtr hProcess)
        {
            // Get the handle to the first module in the process.
            IntPtr[] modules = new IntPtr[1];
            uint needed = 0;
            if (!Psapi.EnumProcessModules(hProcess, modules, (uint)IntPtr.Size, ref needed))
                return null;
            if (needed == 0)
                return null;
            IntPtr firstModule = modules[0];

            // Get the filename for the module.
            uint bufsize = Kernel32.MAX_PATH * 4;
            StringBuilder filename = new StringBuilder((int)bufsize);
            uint charsReturned = Psapi.GetModuleFileNameEx(hProcess, firstModule, filename, bufsize);
            if (charsReturned == 0)
                return null;

            return Path.GetFileName(filename.ToString());
        }

        public static bool TrimWorkingSet()
        {
            using (Process p = Process.GetCurrentProcess())
            {
                // Use (IntPtr)(-1) for x64 compatibility - special value to empty working set
                return Kernel32.SetProcessWorkingSetSize(p.Handle, (IntPtr)(-1), (IntPtr)(-1));
            }
        }

        public static void TrimWorkingSet(int delayMilliseconds)
        {
            new DelayTrimWorkingSetHelper(delayMilliseconds);
        }

        internal class DelayTrimWorkingSetHelper
        {
            /// <summary>
            /// Trims the working set after a given delay
            /// </summary>
            /// <param name="delayMs">The delay prior to trimming the working set</param>
            /// <returns></returns>
            public DelayTrimWorkingSetHelper(int delayMs)
            {
                m_trimTimer = new Timer(new TimerCallback(TrimTimerCallBack), null, delayMs, Timeout.Infinite);
            }
            private Timer m_trimTimer = null;

            private void TrimTimerCallBack(object o)
            {
                TrimWorkingSet();
                if (m_trimTimer != null)
                {
                    m_trimTimer.Dispose();
                    m_trimTimer = null;
                }
            }
        }
    }
}
