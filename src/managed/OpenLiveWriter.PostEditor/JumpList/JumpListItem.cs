// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.JumpList
{
    /// <summary>
    /// Represents a jump list item.
    /// </summary>
    public class JumpListItem : IJumpListItem, IDisposable
    {
        /// <summary>
        /// Creates a jump list item with the specified path.
        /// </summary>
        /// <param name="path">The path to the jump list item.</param>
        /// <remarks>The file type should associate the given file
        /// with the calling application.</remarks>
        public JumpListItem(string path)
        {
            // Get the absolute path
            string absPath = ShellHelper.GetAbsolutePath(path);

            // Make sure this is valid
            if (!File.Exists(absPath))
                throw new FileNotFoundException(string.Format("The given path does not exist ({0})", path));

            ParsingName = absPath;
        }

        /// <summary>
        /// Parsing name for this Object e.g. c:\Windows\file.txt,
        /// or ::{Some Guid}
        /// </summary>
        private string internalParsingName = null;

        /// <summary>
        /// Gets the parsing name for this ShellItem.
        /// </summary>
        virtual public string ParsingName
        {
            get
            {
                if (internalParsingName == null && nativeShellItem != null)
                {
                    internalParsingName = GetParsingName(nativeShellItem);
                }
                return internalParsingName;
            }
            protected set
            {
                this.internalParsingName = value;
            }
        }

        private static string GetParsingName(IShellItem shellItem)
        {
            if (shellItem == null)
                return null;

            string path = null;

            IntPtr pszPath = IntPtr.Zero;
            int hr = shellItem.GetDisplayName(Shell32.SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out pszPath);

            if (false ==
                    (hr == HRESULT.S_OK ||
                     hr == HRESULT.E_INVALIDARG))
                throw new COMException("GetParsingName", (int)hr);

            if (pszPath != IntPtr.Zero)
            {
                path = Marshal.PtrToStringAuto(pszPath);
                Marshal.FreeCoTaskMem(pszPath);
                pszPath = IntPtr.Zero;
            }

            return path;

        }

        #region IJumpListItem Members

        /// <summary>
        /// Gets or sets the target path for this jump list item.
        /// </summary>
        public string Path
        {
            get
            {
                return ParsingName;
            }
            set
            {
                ParsingName = value;
            }
        }

        /// <summary>
        /// Internal member to keep track of the native IShellItem2
        /// </summary>
        internal IShellItem2 nativeShellItem;

        /// <summary>
        /// Return the native ShellFolder object as newer IShellItem2
        /// </summary>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">If the native object cannot be created.
        /// The ErrorCode member will contain the external error code.</exception>
        virtual internal IShellItem2 NativeShellItem2
        {
            get
            {
                if (nativeShellItem == null && ParsingName != null)
                {
                    Guid guid = new Guid(Shell32.IShellItem2);
                    int retCode = Shell32.SHCreateItemFromParsingName(ParsingName, IntPtr.Zero, ref guid, out nativeShellItem);

                    if (nativeShellItem == null || !ComHelper.SUCCEEDED(retCode))
                    {
                        throw new ExternalException("Shell item could not be created.", Marshal.GetExceptionForHR(retCode));
                    }
                }
                return nativeShellItem;
            }
        }

        /// <summary>
        /// Return the native ShellFolder object
        /// </summary>
        public virtual IShellItem NativeShellItem
        {
            get
            {
                return NativeShellItem2;
            }
        }

        #endregion

        /// <summary>
        /// Release the native and managed objects
        /// </summary>
        /// <param name="disposing">Indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                internalParsingName = null;
            }

            if (nativeShellItem != null)
            {
                Marshal.ReleaseComObject(nativeShellItem);
                nativeShellItem = null;
            }
        }

        /// <summary>
        /// Release the native objects.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement the finalizer.
        /// </summary>
        ~JumpListItem()
        {
            Dispose(false);
        }
    }
}
