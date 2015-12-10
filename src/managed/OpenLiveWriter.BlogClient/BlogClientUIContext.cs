// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient
{
    /// <summary>
    /// Required capabilities for a blog client ui context:
    ///		- IWin32Window for dialog ownership/parenting
    ///		- ISynchronizeInvoke for showing dialogs back on the UI thread
    /// </summary>
    public interface IBlogClientUIContext : IWin32Window, ISynchronizeInvoke
    {
    }

    /// <summary>
    /// Convenience implementation of IBlogClientUIContext
    /// </summary>
    public class BlogClientUIContextImpl : IBlogClientUIContext
    {
        public BlogClientUIContextImpl(Form form) { _dialogOwner = form; _invokeTarget = form; }
        public BlogClientUIContextImpl(IWin32Window dialogOwner, ISynchronizeInvoke invokeContext) { _dialogOwner = dialogOwner; _invokeTarget = invokeContext; }
        public IntPtr Handle { get { return _dialogOwner.Handle; } }
        public bool InvokeRequired { get { return _invokeTarget.InvokeRequired; } }
        public IAsyncResult BeginInvoke(Delegate method, object[] args) { return _invokeTarget.BeginInvoke(method, args); }
        public object EndInvoke(IAsyncResult result) { return _invokeTarget.EndInvoke(result); }
        public object Invoke(Delegate method, object[] args) { return _invokeTarget.Invoke(method, args); }

        private IWin32Window _dialogOwner;
        private ISynchronizeInvoke _invokeTarget;
    }

    /// <summary>
    /// Class used to install and remove (on dispose) the UI context for the currently
    /// running thread. To enforce the idiom of install/remove this is the ONLY
    /// suppported mechanism for manipulating the UI context. This class should be
    /// instantiated under a "using" statement on every thread that will call
    /// BlogClient code. Further, every time a new dialog box which may call blog
    /// client code (e.g. OpenPost, UpdateTemplate, etc.) should also construct
    /// and instance of this class around the showing of the dialog.
    /// </summary>
    public class BlogClientUIContextScope : IDisposable
    {
        public BlogClientUIContextScope(Form form)
            : this(new BlogClientUIContextImpl(form))
        {
        }

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
        public static DialogResult ShowDialog(Form dialogForm)
        {
            if (SilentModeForCurrentThread)
                return DialogResult.Cancel;

            if (_uiContext == null)
                throw new InvalidOperationException("Called BlogClientUIContext.ShowDialog on a thread with no context initialized!");

            // center the form relative to the owning window
            CenterForm(_uiContext, dialogForm);

            // show the form using the Desktop window as the owner (we had trouble with the UI thread hanging
            // when we used the active dialog as the actual owner)
            if (_uiContext.InvokeRequired)
            {
                Debug.Fail("WARNING: You are using a fragile codepath--form created on one thread and shown on another!");

                // make the form always-on-top so the user does not "lose" it behind other windows if they switch apps
                dialogForm.TopMost = true;

                // invoke with desktop form as parent
                object dialogResult = _uiContext.Invoke(new ShowDialogHandler(dialogForm.ShowDialog), new object[] { Win32WindowImpl.DesktopWin32Window });

                // return the result
                return (DialogResult)dialogResult;
            }
            else
            {
                return dialogForm.ShowDialog(_uiContext);
            }
        }
        private delegate DialogResult ShowDialogHandler(IWin32Window owner);

        public static DialogResult ShowDisplayMessageOnUIThread(MessageId messageId, params object[] parameters)
        {
            IBlogClientUIContext uiContext = ContextForCurrentThread;
            if (uiContext == null)
            {
                Trace.Fail("BlogClientUIContext.ShowDisplayMessageOnUIThread called without a context in scope!");
                return DialogResult.None;
            }

            DisplayMessageHelper dmh = new DisplayMessageHelper(uiContext, messageId, parameters);
            if (uiContext.InvokeRequired)
                uiContext.Invoke(new ThreadStart(dmh.Handler), null);
            else
                dmh.Handler();
            return dmh.DialogResult;
        }

        private class DisplayMessageHelper
        {
            private readonly IWin32Window _owner;
            private readonly MessageId _messageId;
            private readonly object[] _parameters;
            private DialogResult _dialogResult;

            public DisplayMessageHelper(IWin32Window owner, MessageId messageId, object[] parameters)
            {
                _owner = owner;
                _messageId = messageId;
                _parameters = parameters;
            }

            public DialogResult DialogResult
            {
                get { return _dialogResult; }
            }

            public void Handler()
            {
                _dialogResult = DisplayMessage.Show(_messageId, _owner, _parameters);
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

        private static void CenterForm(IWin32Window hRelativeToWnd, Form form)
        {
            // if the owning form is not visible then center on the desktop
            if (!User32.IsWindowVisible(hRelativeToWnd.Handle))
                hRelativeToWnd = Win32WindowImpl.DesktopWin32Window;

            // get the coordinates of the relativeToWnd
            RECT rect = new RECT();
            User32.GetWindowRect(hRelativeToWnd.Handle, ref rect);
            Rectangle centerOnRect = RectangleHelper.Convert(rect);

            // determine the center point
            Point centerPoint = new Point(centerOnRect.Left + (centerOnRect.Width / 2), centerOnRect.Top + (centerOnRect.Height / 2));

            // center the form on that point
            form.Location = new Point(centerPoint.X - (form.Width / 2), centerPoint.Y - (form.Height / 2));
        }

        [ThreadStatic]
        private static IBlogClientUIContext _uiContext;

        [ThreadStatic]
        private static bool _silentMode;
    }
}
