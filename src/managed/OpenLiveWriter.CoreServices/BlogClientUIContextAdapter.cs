// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Adapts a WinForms IWin32Window to the cross-platform IBlogClientUIContext interface.
    /// Used by Windows-specific code that needs to call cross-platform API methods.
    /// </summary>
    public class BlogClientUIContextAdapter : OpenLiveWriter.Platform.IBlogClientUIContext
    {
        private readonly IWin32Window _window;
        private readonly ISynchronizeInvoke _invokeTarget;

        /// <summary>
        /// Create an adapter from an IWin32Window. If the window also implements
        /// ISynchronizeInvoke (e.g. a Form or Control), invoke operations are supported.
        /// </summary>
        public BlogClientUIContextAdapter(IWin32Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _invokeTarget = window as ISynchronizeInvoke;
        }

        /// <summary>
        /// Create an adapter from an IWin32Window and an explicit ISynchronizeInvoke target.
        /// </summary>
        public BlogClientUIContextAdapter(IWin32Window window, ISynchronizeInvoke invokeTarget)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _invokeTarget = invokeTarget;
        }

        public IntPtr NativeWindowHandle => _window.Handle;

        public bool InvokeRequired => _invokeTarget?.InvokeRequired ?? false;

        public object Invoke(Delegate method, object[] args)
        {
            if (_invokeTarget != null)
                return _invokeTarget.Invoke(method, args);
            return method.DynamicInvoke(args);
        }

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            if (_invokeTarget != null)
                return _invokeTarget.BeginInvoke(method, args);
            throw new InvalidOperationException("Asynchronous invoke is not supported without an ISynchronizeInvoke target.");
        }

        public object EndInvoke(IAsyncResult result)
        {
            if (_invokeTarget != null)
                return _invokeTarget.EndInvoke(result);
            throw new InvalidOperationException("Asynchronous invoke is not supported without an ISynchronizeInvoke target.");
        }
    }
}
