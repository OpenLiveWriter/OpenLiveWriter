// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlEditor.Controls;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.SpellChecker;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// MainFrameWindowAdapter will take a IContentEditorSite given by a hosting application and convert it into
    /// IMainFrameWindow and IBlogPostEditingSite which are used by the ContentEditor.  It's general functions
    /// are to act as a middle man to the window that is holding the canvas.
    /// </summary>
    internal class MainFrameWindowAdapter : IMainFrameWindow, IBlogPostEditingSite, IBlogContext, OpenLiveWriter.Interop.Com.IDropTarget, IDisposable, IETWProvider, IWordRangeProvider
    {
        private readonly IntPtr _parentWindowHandle;
        private readonly Control _editorHostPanel;
        private IContentEditorSite _contentEditorSite;
        private string _accountId;
        public MainFrameWindowAdapter(IntPtr parentWindowHandle, Control editorHostPanel, IContentEditorSite contentEditorSite, string accountId)
        {
            _parentWindowHandle = parentWindowHandle;
            _editorHostPanel = editorHostPanel;
            _contentEditorSite = contentEditorSite;
            editorHostPanel.LocationChanged += new EventHandler(editorHostPanel_LocationChanged);
            editorHostPanel.SizeChanged += new EventHandler(editorHostPanel_SizeChanged);
            editorHostPanel.Layout += new LayoutEventHandler(editorHostPanel_Layout);
            _accountId = accountId;
        }

        void editorHostPanel_Layout(object sender, LayoutEventArgs e)
        {
            FireLayout(e);
        }

        void editorHostPanel_SizeChanged(object sender, EventArgs e)
        {
            FireSizeChanged(_editorHostPanel.Width, _editorHostPanel.Height);
        }

        void editorHostPanel_LocationChanged(object sender, EventArgs e)
        {
            FireLocationChanged();
        }

        public IUIFramework RibbonFramework
        {
            get
            {
                return (IUIFramework)_contentEditorSite;
            }
        }

        public void OnKeyboardLanguageChanged()
        {
            _contentEditorSite.OnKeyboardLanguageChanged();
        }

        #region IWordRangeProvider Members

        public IWordRange GetSubjectSpellcheckWordRange()
        {
            return ((IWordRangeProvider)_contentEditorSite).GetSubjectSpellcheckWordRange();
        }

        public void CloseSubjectSpellcheckWordRange()
        {
            ((IWordRangeProvider)_contentEditorSite).CloseSubjectSpellcheckWordRange();
        }

        #endregion

        #region IMainFrameWindow Members

        public string Caption
        {
            set
            {

            }
        }

        public Point Location
        {
            get
            {
                WINDOWINFO info = new WINDOWINFO();
                User32.GetWindowInfo(_parentWindowHandle, ref info);
                return new Point(info.rcWindow.left, info.rcWindow.top);
            }
        }

        public Size Size
        {
            get
            {
                WINDOWINFO info = new WINDOWINFO();
                User32.GetWindowInfo(_parentWindowHandle, ref info);
                return new Size(Math.Max(info.rcWindow.Width, _width), Math.Max(info.rcWindow.Height, _height));
            }
        }

        public event EventHandler LocationChanged;
        private void FireLocationChanged()
        {
            if (LocationChanged != null)
                LocationChanged(this, EventArgs.Empty);
        }

        private int _width;
        private int _height;
        public event EventHandler SizeChanged;
        public void FireSizeChanged(int x, int y)
        {
            _width = x;
            _height = y;
            if (SizeChanged != null)
                SizeChanged(this, EventArgs.Empty);
        }

        public event EventHandler Deactivate;
        private void FireDeactivate()
        {
            if (Deactivate != null)
                Deactivate(this, EventArgs.Empty);
        }

        public event LayoutEventHandler Layout;
        private void FireLayout(LayoutEventArgs arg)
        {
            if (Layout != null)
            {
                Layout(this, arg);
            }

        }

        public void Activate()
        {
            User32.SetForegroundWindow(_parentWindowHandle);
        }

        public void Update()
        {
            User32.UpdateWindow(_parentWindowHandle);
        }

        public void SetStatusBarMessage(StatusMessage message)
        {
            _contentEditorSite.SetStatusBarMessage(message.BlogPostStatus, message.WordCountValue);
        }

        private Stack<StatusMessage> _statusMessageStack = new Stack<StatusMessage>();
        public void PushStatusBarMessage(StatusMessage message)
        {
            _statusMessageStack.Push(message);
            _contentEditorSite.SetStatusBarMessage(message.BlogPostStatus, message.WordCountValue);
        }

        public void PopStatusBarMessage()
        {
            if (_statusMessageStack.Count > 0)
                _statusMessageStack.Pop();

            if (_statusMessageStack.Count > 0)
            {
                StatusMessage message = _statusMessageStack.Peek();
                _contentEditorSite.SetStatusBarMessage(message.BlogPostStatus, message.WordCountValue);
            }
            else
            {
                _contentEditorSite.SetStatusBarMessage(null, null);
            }

        }

        public void PerformLayout()
        {
            _editorHostPanel.PerformLayout();
        }

        public void Invalidate()
        {
            User32.InvalidateWindow(_parentWindowHandle, IntPtr.Zero, false);
        }

        public void Close()
        {
            User32.SendMessage(_parentWindowHandle, WM.CLOSE, UIntPtr.Zero, IntPtr.Zero);
        }

        #endregion

        #region IWin32Window Members

        public IntPtr Handle
        {
            get { return _editorHostPanel.Handle; }
        }

        #endregion

        #region ISynchronizeInvoke Members

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            return _editorHostPanel.BeginInvoke(method, args);
        }

        public object EndInvoke(IAsyncResult result)
        {
            return _editorHostPanel.EndInvoke(result);
        }

        public object Invoke(Delegate method, object[] args)
        {
            return _editorHostPanel.Invoke(method, args);
        }

        public bool InvokeRequired
        {
            get { return _editorHostPanel.InvokeRequired; }
        }

        #endregion

        #region IMiniFormOwner Members

        public void AddOwnedForm(System.Windows.Forms.Form f)
        {
            User32.SetParent(f.Handle, _parentWindowHandle);
        }

        public void RemoveOwnedForm(System.Windows.Forms.Form f)
        {
            User32.SetParent(f.Handle, IntPtr.Zero);
        }

        #endregion

        #region IBlogContext Members

        public string CurrentAccountId
        {
            get { return null; }
            set { }
        }

        #endregion

        #region IBlogPostEditingSite Members

        public IMainFrameWindow FrameWindow
        {
            get { return this; }
        }
        // @SharedCanvas - this can all be removed once the default sidebar is taken out
        public event WeblogHandler WeblogChanged;

        public event EventHandler WeblogListChanged;

        private void InvokeWeblogChanged(string blogId)
        {
            WeblogHandler Handler = WeblogChanged;
            if (Handler != null) Handler(blogId);
        }

        public event WeblogSettingsChangedHandler WeblogSettingsChanged;

        private void InvokeWeblogSettingsChanged(string blogId, bool templateChanged)
        {
            WeblogSettingsChangedHandler Handler = WeblogSettingsChanged;
            if (Handler != null) Handler(blogId, templateChanged);
        }

        public event WeblogSettingsChangedHandler GlobalWeblogSettingsChanged;

        private void InvokeGlobalWeblogSettingsChanged(string blogId, bool templateChanged)
        {
            WeblogSettingsChangedHandler Handler = GlobalWeblogSettingsChanged;
            if (Handler != null) Handler(blogId, templateChanged);
        }

        public void NotifyWeblogSettingsChanged(bool templateChanged)
        {
            throw new NotImplementedException();
        }

        public void NotifyWeblogSettingsChanged(string blogId, bool templateChanged)
        {
            throw new NotImplementedException();
        }

        public void NotifyWeblogAccountListEdited()
        {
            throw new NotImplementedException();
        }

        public void ConfigureWeblog(string blogId, Type selectedPanel)
        {
            throw new System.NotImplementedException();
        }

        public void ConfigureWeblog(string blogId)
        {
            throw new NotImplementedException();
        }

        public void ConfigureWeblogFtpUpload(string blogId)
        {
            throw new NotImplementedException();
        }

        public bool UpdateWeblogTemplate(string blogId)
        {
            throw new NotImplementedException();
        }

        public void AddWeblog()
        {
            throw new NotImplementedException();
        }

        public void OpenLocalPost(PostInfo postInfo)
        {
            throw new NotImplementedException();
        }

        public void DeleteLocalPost(PostInfo postInfo)
        {
            throw new NotImplementedException();
        }

        public event EventHandler PostListChanged;

        private void InvokePostListChanged(EventArgs e)
        {
            EventHandler postListChangedHandler = PostListChanged;
            if (postListChangedHandler != null) postListChangedHandler(this, e);
        }

        // This only exists to please the compiler.
        private void InvokeWeblogListChanged(EventArgs e)
        {
            EventHandler weblogListChangedHandler = WeblogListChanged;
            if (weblogListChangedHandler != null) weblogListChangedHandler(this, e);
        }

        public OpenLiveWriter.HtmlEditor.Controls.IHtmlStylePicker StyleControl
        {
            // @SharedCanvas - this needs to be fixed with the ribbon.  The window should not be providing
            // a combo box for the command manager to poke up
            get { return new NullHtmlStylePicker(); }
        }

        public class NullHtmlStylePicker : IHtmlStylePicker
        {

            #region IHtmlStylePicker Members

            public event EventHandler HtmlStyleChanged;

            private void InvokeHtmlStyleChanged(EventArgs e)
            {
                EventHandler htmlStyleChangedHandler = HtmlStyleChanged;
                if (htmlStyleChangedHandler != null) htmlStyleChangedHandler(this, e);
            }

            public class NullHtmlFormattingStyle : IHtmlFormattingStyle
            {

                #region IHtmlFormattingStyle Members

                public string DisplayName
                {
                    get { return ""; }
                }

                public string ElementName
                {
                    get { return ""; }
                }

                public mshtml._ELEMENT_TAG_ID ElementTagId
                {
                    get { return mshtml._ELEMENT_TAG_ID.TAGID_SPAN; }
                }

                #endregion
            }

            public IHtmlFormattingStyle SelectedStyle
            {
                get { return new NullHtmlFormattingStyle(); }
            }

            public bool Enabled
            {
                get
                {
                    return false;
                }
                set
                {

                }
            }

            public void SelectStyleByElementName(string p)
            {

            }

            #endregion
        }

        #endregion

        #region IDropTarget Members

        public void DragEnter(OpenLiveWriter.Interop.Com.IOleDataObject pDataObj, MK grfKeyState, POINT pt, ref OpenLiveWriter.Interop.Com.DROPEFFECT pdwEffect)
        {
            _contentEditorSite.DragEnter(pDataObj, grfKeyState, pt, ref pdwEffect);
        }

        public void DragOver(MK grfKeyState, POINT pt, ref OpenLiveWriter.Interop.Com.DROPEFFECT pdwEffect)
        {
            _contentEditorSite.DragOver(grfKeyState, pt, ref pdwEffect);
        }

        public void DragLeave()
        {
            _contentEditorSite.DragLeave();
        }

        public void Drop(OpenLiveWriter.Interop.Com.IOleDataObject pDataObj, MK grfKeyState, POINT pt, ref OpenLiveWriter.Interop.Com.DROPEFFECT pdwEffect)
        {
            _contentEditorSite.Drop(pDataObj, grfKeyState, pt, ref pdwEffect);
        }

        #endregion

        #region IBlogPostEditingSite Members

        public CommandManager CommandManager
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _contentEditorSite = null;
        }

        #endregion

        public void WriteEvent(string eventName)
        {
            if (_contentEditorSite != null)
                _contentEditorSite.WriteEvent(eventName);
        }
    }
}
