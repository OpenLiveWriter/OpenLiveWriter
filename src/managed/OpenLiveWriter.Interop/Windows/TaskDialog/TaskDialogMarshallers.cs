// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenLiveWriter.Interop.Windows.TaskDialog
{
    internal class StringMarshaller : IDisposable
    {
        private IntPtr pwsz;

        public StringMarshaller(string str)
        {
            if (str != null)
                pwsz = Marshal.StringToHGlobalUni(str);
        }

        public IntPtr Value
        {
            get { return pwsz; }
        }

        public void Dispose()
        {
            IntPtr temp = Interlocked.CompareExchange(ref pwsz, IntPtr.Zero, pwsz);
            if (temp != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pwsz);
            }
        }
    }

    internal class StructArrayMarshaller<T> : IDisposable
    {
        private int elementSize;
        private IntPtr buffer;
        private int cButtons;

        public unsafe StructArrayMarshaller(params T[] buttons)
        {
            if (buttons != null && buttons.Length > 0)
            {
                elementSize = Marshal.SizeOf(buttons[0]);
                buffer = Marshal.AllocHGlobal(elementSize * buttons.Length);
                cButtons = buttons.Length;

                for (int i = 0; i < buttons.Length; i++)
                {
                    IntPtr dest = new IntPtr(buffer.ToInt64() + i * elementSize);
                    Marshal.StructureToPtr(buttons[i], dest, false);
                }
            }
        }

        public IntPtr Buffer
        {
            get { return buffer; }
        }

        public void Dispose()
        {
            IntPtr temp = Interlocked.CompareExchange(ref buffer, IntPtr.Zero, buffer);
            if (temp != IntPtr.Zero)
            {
                for (int i = 0; i < cButtons; i++)
                {
                    Marshal.DestroyStructure(
                        new IntPtr(temp.ToInt64() + i * elementSize),
                        typeof(T));
                }
                Marshal.FreeHGlobal(temp);
            }
        }
    }
}
