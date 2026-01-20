// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// This is a compatibility shim for System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler
// which was removed in .NET Core/.NET 5+. This provides the same functionality for COM interop.

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Runtime.InteropServices.CustomMarshalers
{
    /// <summary>
    /// Marshals the COM IEnumVARIANT interface to the .NET IEnumerator interface, and vice versa.
    /// This is a compatibility shim for .NET 10 since the original was removed from .NET Core.
    /// </summary>
    public class EnumeratorToEnumVariantMarshaler : ICustomMarshaler
    {
        private static readonly EnumeratorToEnumVariantMarshaler s_instance = new EnumeratorToEnumVariantMarshaler();

        public static ICustomMarshaler GetInstance(string cookie) => s_instance;

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.Release(pNativeData);
        }

        public int GetNativeDataSize() => -1;

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            if (ManagedObj == null)
                return IntPtr.Zero;

            throw new NotSupportedException("Marshaling from managed IEnumerator to native IEnumVARIANT is not supported.");
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;

            return new EnumVariantWrapper(pNativeData);
        }

        /// <summary>
        /// Wraps a COM IEnumVARIANT as a .NET IEnumerator
        /// </summary>
        private class EnumVariantWrapper : IEnumerator, IDisposable
        {
            private IEnumVARIANT _enumVariant;
            private object _current;
            private bool _disposed;

            public EnumVariantWrapper(IntPtr pEnumVariant)
            {
                _enumVariant = (IEnumVARIANT)Marshal.GetObjectForIUnknown(pEnumVariant);
            }

            public object Current => _current;

            public bool MoveNext()
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(EnumVariantWrapper));

                object[] items = new object[1];
                int fetched = 0;

                int hr = _enumVariant.Next(1, items, ref fetched);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                if (fetched == 1)
                {
                    _current = items[0];
                    return true;
                }

                _current = null;
                return false;
            }

            public void Reset()
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(EnumVariantWrapper));

                _enumVariant.Reset();
                _current = null;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    if (_enumVariant != null)
                    {
                        Marshal.ReleaseComObject(_enumVariant);
                        _enumVariant = null;
                    }
                    _current = null;
                }
            }
        }

        /// <summary>
        /// COM IEnumVARIANT interface
        /// </summary>
        [ComImport]
        [Guid("00020404-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IEnumVARIANT
        {
            [PreserveSig]
            int Next(
                [In, MarshalAs(UnmanagedType.U4)] int celt,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] object[] rgVar,
                [In, Out] ref int pCeltFetched);

            [PreserveSig]
            int Skip([In, MarshalAs(UnmanagedType.U4)] int celt);

            void Reset();

            IEnumVARIANT Clone();
        }
    }
}
