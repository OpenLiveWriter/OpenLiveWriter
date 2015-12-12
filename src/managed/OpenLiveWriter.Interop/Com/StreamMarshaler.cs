// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Provides a custom marshaller for working with .NET stream and COM IStreams.
    /// For a detailed explanation of the methods, see the documentation
    /// for ICustomerMarshaler.
    ///
    /// Based upon the StreamMarshaler implementation example found in
    /// Adam Nathan's ".NET and COM: The Complete Iteroperability Guide" Page 901.
    /// </summary>
    public class StreamMarshaler : ICustomMarshaler
    {

        /// <summary>
        /// Returns an instance of the custom marshaled
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static ICustomMarshaler GetInstance(string cookie)
        {
            // Always return the same instance
            if (marshaler == null)
                marshaler = new StreamMarshaler();
            return marshaler;
        }
        static StreamMarshaler marshaler = null;

        // Marshal Native to Managed
        /// <summary>
        /// Marshall native data to managed data
        /// </summary>
        /// <param name="pNativeData"></param>
        /// <returns></returns>
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero) return null;

            Object rcw = Marshal.GetObjectForIUnknown(pNativeData);
            if (!(rcw is IStream))
                throw new MarshalDirectiveException("The object must implement IStream.");

            return new ComStream((IStream)rcw);
        }

        /// <summary>
        /// Cleans up any native data
        /// </summary>
        /// <param name="pNativeData"></param>
        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.Release(pNativeData);
        }

        /// <summary>
        /// The size of the native data to be marshalled.  Always returns -1.
        /// </summary>
        /// <returns>Reference types always return -1.</returns>
        public int GetNativeDataSize()
        {
            return -1;
        }

        // Marshal Managed to Native
        /// <summary>
        /// Marshals managed data to native
        /// </summary>
        /// <param name="ManagedObj"></param>
        /// <returns></returns>
        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            const string MESSAGE = "Marshalling .NET Streams to COM Streams is not supported.";
            Debug.Fail(MESSAGE);
            throw new NotSupportedException(MESSAGE);
        }

        /// <summary>
        /// Cleans up managed data.
        /// </summary>
        /// <param name="ManagedObj"></param>
        public void CleanUpManagedData(object ManagedObj)
        {
            // Nothing to do
        }

    }
}
