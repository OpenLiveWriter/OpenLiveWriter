// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Threading;
using OpenLiveWriter.Platform;

// Use the platform-agnostic IBlogClientUIContext from OpenLiveWriter.Platform
using IBlogClientUIContext = OpenLiveWriter.Platform.IBlogClientUIContext;

namespace OpenLiveWriter.BlogClient
{
    /// <summary>
    /// Class used to install and remove (on dispose) the UI context for the currently
    /// running thread. To enforce the idiom of install/remove this is the ONLY
    /// supported mechanism for manipulating the UI context. This class should be
    /// instantiated under a "using" statement on every thread that will call
    /// BlogClient code. Further, every time a new dialog box which may call blog
    /// client code (e.g. OpenPost, UpdateTemplate, etc.) should also construct
    /// an instance of this class around the showing of the dialog.
    /// </summary>
    public class BlogClientUIContextScope : IDisposable
    {
        public BlogClientUIContextScope(IBlogClientUIContext uiContext)
        {
            _previousUIContext = BlogClientUIContext.ContextForCurrentThread;
            BlogClientUIContext.ContextForCurrentThread = uiContext;
        }

        public void Dispose()
        {
            BlogClientUIContext.ContextForCurrentThread = _previousUIContext;
        }

        private IBlogClientUIContext _previousUIContext;
    }

    public class BlogClientUIContextSilentMode : IDisposable
    {
        public BlogClientUIContextSilentMode()
        {
            _previousSilentMode = BlogClientUIContext.SilentModeForCurrentThread;
            BlogClientUIContext.SilentModeForCurrentThread = true;
        }

        public void Dispose()
        {
            BlogClientUIContext.SilentModeForCurrentThread = _previousSilentMode;
        }

        private bool _previousSilentMode;
    }


    /// <summary>
    /// Class which allows blog-client code at any level in the stack and on
    /// any thread to show a modal dialog on the main UI thread. In order to
    /// use this class a BlogClientUIContextScope must have been created (and
    /// not yet disposed) on the currently executing thread.
    /// </summary>
    public class BlogClientUIContext
    {
        /// <summary>
        /// Show a display message on the UI thread using the platform dialog service.
        /// Returns a platform-agnostic DialogResultValue.
        /// </summary>
        public static DialogResultValue ShowDisplayMessageOnUIThread(string messageId, params object[] parameters)
        {
            IBlogClientUIContext uiContext = ContextForCurrentThread;
            if (uiContext == null)
            {
                Trace.Fail("BlogClientUIContext.ShowDisplayMessageOnUIThread called without a context in scope!");
                return DialogResultValue.None;
            }

            var dialogService = PlatformContext.DialogService;
            if (dialogService == null)
            {
                Trace.Fail("No dialog service registered in PlatformContext!");
                return DialogResultValue.None;
            }

            if (uiContext.InvokeRequired)
            {
                DialogResultValue result = DialogResultValue.None;
                uiContext.Invoke(new ThreadStart(() =>
                {
                    result = dialogService.ShowMessage(messageId, uiContext.NativeWindowHandle, parameters);
                }), null);
                return result;
            }
            else
            {
                return dialogService.ShowMessage(messageId, uiContext.NativeWindowHandle, parameters);
            }
        }

        internal static IBlogClientUIContext ContextForCurrentThread
        {
            get { return _uiContext; }
            set { _uiContext = value; }
        }

        internal static bool SilentModeForCurrentThread
        {
            get { return _silentMode; }
            set { _silentMode = value; }
        }

        [ThreadStatic]
        private static IBlogClientUIContext _uiContext;

        [ThreadStatic]
        private static bool _silentMode;
    }
}
