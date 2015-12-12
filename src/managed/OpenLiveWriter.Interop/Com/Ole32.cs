// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com
{
    public class Ole32
    {
        [DllImport("Ole32.dll")]
        public static extern int OleInitialize(IntPtr pvReserved);

        /// <summary>
        /// Frees the specified storage medium (automatically calls
        /// pUnkForRelease if required).
        /// </summary>
        /// <param name="pmedium">Storage medium to be freed</param>
        [DllImport("Ole32.dll")]
        public static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

        [DllImport("Ole32.dll")]
        public static extern int CoCreateInstance(
            [In] ref Guid rclsid,
            [In] IntPtr pUnkOuter,
            [In] CLSCTX dwClsContext,
            [In] ref Guid riid,
            [Out] out IntPtr pUnknown);

        [DllImport("Ole32.dll")]
        public static extern int CoDisconnectObject(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown,
            uint dwReserved);

        [DllImport("Ole32.dll")]
        public static extern int OleRun(
            [MarshalAs(UnmanagedType.IUnknown)] object pUnknown);

        [DllImport("Ole32.dll")]
        public static extern int OleLockRunning(
            [MarshalAs(UnmanagedType.IUnknown)] object pUnknown,
            [MarshalAs(UnmanagedType.Bool)] bool fLock,
            [MarshalAs(UnmanagedType.Bool)] bool fLastUnlockCloses);

        /// <summary>
        /// Gets the CLSID for an application based upon its progId
        /// </summary>
        [DllImport("Ole32.dll")]
        public static extern int CLSIDFromProgID(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszProgID,
            out Guid pclsid
            );

        [DllImport("Ole32.dll")]
        public static extern IntPtr CoTaskMemAlloc(uint cb);

        [DllImport("Ole32.dll")]
        public static extern int CreateBindCtx(
            [In] uint reserved,
            [Out] out IBindCtx ppbc);

        [DllImport("Ole32.dll")]
        public static extern int CreateItemMoniker(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszDelim,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszItem,
            out IMoniker ppmk
            );

        [DllImport("Ole32.dll")]
        public static extern int GetRunningObjectTable(
            int reserved,
            out IRunningObjectTable pprot
            );

        /// <summary>
        /// Initiate a drag and drop operation
        /// </summary>
        [DllImport("Ole32.dll")]
        public static extern int DoDragDrop(
            IOleDataObject pDataObject,  // Pointer to the data object
            IDropSource pDropSource,	  // Pointer to the source
            DROPEFFECT dwOKEffect,       // Effects allowed by the source
            ref DROPEFFECT pdwEffect    // Pointer to effects on the source
            );

        [DllImport("Ole32.dll")]
        public static extern int RegisterDragDrop(
            IntPtr hwnd,  //Handle to a window that can accept drops
            IDropTarget pDropTarget
            //Pointer to object that is to be target of drop
            );

        [DllImport("Ole32.dll")]
        public static extern int RevokeDragDrop(
            IntPtr hwnd  //Handle to a window that can accept drops
            );

        [DllImport("Ole32.dll")]
        public static extern IntPtr OleGetIconOfFile(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszPath,
            [MarshalAs(UnmanagedType.Bool)] bool fUseFileAsLabel);

        [DllImport("Ole32.dll", PreserveSig = false)] // returns hresult
        public extern static void PropVariantCopy([Out] out PropVariant pDst, [In] ref PropVariant pSrc);

        [DllImport("Ole32.dll", PreserveSig = false)] // returns hresult
        internal extern static void PropVariantClear([In, Out] ref PropVariant pvar);
    }

    [Flags]
    public enum CLSCTX : uint
    {
        INPROC_SERVER = 0x1,
        INPROC_HANDLER = 0x2,
        LOCAL_SERVER = 0x4,
        INPROC_SERVER16 = 0x8,
        REMOTE_SERVER = 0x10,
        INPROC_HANDLER16 = 0x20,
        RESERVED1 = 0x40,
        RESERVED2 = 0x80,
        RESERVED3 = 0x100,
        RESERVED4 = 0x200,
        NO_CODE_DOWNLOAD = 0x400,
        RESERVED5 = 0x800,
        NO_CUSTOM_MARSHAL = 0x1000,
        ENABLE_CODE_DOWNLOAD = 0x2000,
        NO_FAILURE_LOG = 0x4000,
        DISABLE_AAA = 0x8000,
        ENABLE_AAA = 0x10000,
        FROM_DEFAULT_CONTEXT = 0x20000
    };

}
