// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml.Mshtml_Interop;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interactive editor used for creating and publishing MHT-based reports. Embeds and customizes
    /// extensively the MSHTML editing engine.
    ///
    /// </summary>
    public class MshtmlEditor : UserControl, IDocHostUIHandler2, IHTMLChangeSink, IHTMLEditDesignerRaw
    {
        private readonly EventCounter _eventCounter = new EventCounter();

        #region Initialization/Disposal

        /// <summary>
        /// Construction. Some initialization is done here, the rest of the initialiation is
        /// deferred until the document readyState is "complete"
        /// </summary>
        public MshtmlEditor(IServiceProviderRaw serviceProvider, MshtmlOptions mshtmlOptions, bool protectFocus)
        {
            // initialize properties
            BackColor = SystemColors.Window;

            // initialize MSHTML
            InitializeMshtmlControl(serviceProvider, mshtmlOptions, protectFocus);

            _active = true;

            // watch for ReadyState == "complete" so we can finish initialization once
            // the document is loaded
            mshtmlControl.DocumentEvents.ReadyStateChanged += new EventHandler(ReadyStateChangedHandler);
        }

        private bool _active;

        /// <summary>
        /// Indicates if the editor is attached and active (i.e. not cached).
        /// </summary>
        public bool Active
        {
            get { return _active; }
            set
            {
                if (value != _active)
                {
                    _active = value;
                    OnActiveChanged(EventArgs.Empty);
                }
            }
        }

        protected virtual void OnActiveChanged(EventArgs eventArgs)
        {
            if (_active)
            {
                Debug.Assert(mshtmlControl != null, "Cached editor referencing a null mshtml control!");
                if (mshtmlControl != null)
                    mshtmlControl.InstallDocHostUIHandler(this);

                _eventCounter.Reset();
            }
            else
            {
                if (mshtmlControl != null)
                {
                    mshtmlControl.UninstallDocHostUIHandler();

                    if (mshtmlControl.DocumentIsComplete && mshtmlControl.MarkupContainer != null)
                        mshtmlControl.MarkupContainer.UnRegisterForDirtyRange(_changeSinkCookie);
                }

                // AddEditDesigner is automatically called when the document loads,
                // but we need to manually remove ourselves when we become inactive.
                RemoveEditDesigner();

                _eventCounter.AssertAllEventsAreUnhooked();
            }
        }

        public void SetServiceProvider(IServiceProviderRaw serviceProvider)
        {
            mshtmlControl.SetServiceProvider(serviceProvider);
        }

        /// <summary>
        /// Create the MSHTML control and add it to our client area
        /// </summary>
        private void InitializeMshtmlControl(IServiceProviderRaw serviceProvider, MshtmlOptions mshtmlOptions, bool protectFocus)
        {
            UpdateOptions(mshtmlOptions, true);

            // initialize mshtml control
            mshtmlControl = new MshtmlControl(serviceProvider, protectFocus);
            mshtmlControl.InstallDocHostUIHandler(this);
            mshtmlControl.SetDLControlFlags(mshtmlOptions.DLCTLOptions);
            mshtmlControl.Init();
            mshtmlControl.SetEditMode(true);

            // get a reference to the htmlDocument
            htmlDocument = mshtmlControl.HTMLDocument;

            /// Hook Mshtml document GotFocus so we can notify .NET when our control
            /// gets focus (allows Enter and Leave events to be fired correctly so
            /// that commands can be managed correctly)
            mshtmlControl.DocumentEvents.GotFocus += new EventHandler(DocumentEvents_GotFocus);

            // add the control to our client area
            mshtmlControl.Dock = DockStyle.Fill;
            Controls.Add(mshtmlControl);
        }

        /// <summary>
        /// Dispose the presentation editor
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (mshtmlControl != null)
                {
                    // detach event handlers
                    mshtmlControl.DocumentEvents.ReadyStateChanged -= new EventHandler(ReadyStateChangedHandler);
                    mshtmlControl.DocumentEvents.GotFocus -= new EventHandler(DocumentEvents_GotFocus);

                    // dispose MSHTML
                    mshtmlControl.Dispose();
                }

                _eventCounter.AssertAllEventsAreUnhooked();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Watch for document being complete to do the remainder of our
        /// initialization (some initialization can only be done once the
        /// document has loaded)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="ea">event args</param>
        private void ReadyStateChangedHandler(object sender, EventArgs ea)
        {
            try
            {
                if (mshtmlControl.DocumentIsComplete)
                {
                    IHTMLTxtRangePool.Clear(mshtmlControl.MarkupServices.MarkupServicesRaw);

                    // check for (unexpected) case of DocumentComplete ready-state being called twice
                    if (!_documentCompleteReadyStateFired)
                    {
                        // update flag indicating DocumentComplete ReadyState has been called
                        // (used as a sanity check (see immediately above) on our assumpton that
                        // this is called only once)
                        _documentCompleteReadyStateFired = true;

                        mshtmlControl.ExecuteCommand(IDM.HTMLEDITMODE, true);
                        mshtmlControl.ExecuteCommand(IDM.IME_ENABLE_RECONVERSION, true);

                        // initialize various editing options
                        InitializeDocumentEditingOptions(true);
                    }

                    // add our custom edit designer
                    AddEditDesigner();

                    // register for dirty range
                    mshtmlControl.MarkupContainer.RegisterForDirtyRange(this, out _changeSinkCookie);

                    // During performing the document complete action, *something* is causing a document change log, which causes
                    // IHtmlChangeSink.Notify to fire, making the document 'dirty' even when nothing was changed.
                    // Suppressing handling of the 'dirty' notification until after document completion actions.
                    using (SuspendNotification())
                    {
                        // perform document complete actions
                        OnDocumentComplete();
                    }
                }
            }
            catch (Exception e)
            {
                HandleUncaughtException(e);
            }
        }

        private IHTMLEditServicesRaw _htmlEditServices;

        private void AddEditDesigner()
        {
            // NOTE: workaround for a bug in MshtmlControl where it caches the EditServices interface accross
            // multiple document loads -- if we could change the source we would just invalidate the ptr
            // after a load -- instead we just do the QueryService ourselves so we can get a different
            // pointer each time

            Guid EDIT_SERVICES_SID = new Guid(0x3050f7f9, 0x98b5, 0x11cf, 0xbb, 0x82, 0x00, 0xaa, 0x00, 0xbd, 0xce, 0x0b);
            _htmlEditServices = (IHTMLEditServicesRaw)ComHelper.QueryService(HTMLDocument, EDIT_SERVICES_SID, typeof(IHTMLEditServicesRaw).GUID);
            _htmlEditServices.AddDesigner(this);
        }

        private void RemoveEditDesigner()
        {
            if (_htmlEditServices != null)
                _htmlEditServices.RemoveDesigner(this);
        }

        public event EventHandler DocumentComplete
        {
            add
            {
                DocumentCompleteEventHandler += value;
                _eventCounter.EventHooked(DocumentCompleteEventHandler);
            }
            remove
            {
                DocumentCompleteEventHandler -= value;
                _eventCounter.EventUnhooked(DocumentCompleteEventHandler);
            }
        }
        private event EventHandler DocumentCompleteEventHandler;

        /// <summary>
        /// Finish initialization when the document has completed loading
        /// </summary>
        private void OnDocumentComplete()
        {
            if (DocumentCompleteEventHandler != null)
                DocumentCompleteEventHandler(this, EventArgs.Empty);
        }

        /// <summary>
        ///  Initialize miscelleneous document editing options
        /// </summary>
        private void InitializeDocumentEditingOptions(bool updateComposeSettings)
        {
            foreach (DictionaryEntry editingOption in _mshtmlOptions.EditingOptions)
            {
                try
                {
                    if (!updateComposeSettings && (uint)editingOption.Key == IDM.COMPOSESETTINGS)
                    {
                        // WinLive 124184: Font settings in a Mail compose note don't persist.
                        // We don't want to ExecuteCommand IDM.COMPOSESETTINGS all the time, because doing so restores the default font
                        // With respect to 124184, we were restoring the default font in a selection change handler following processing the enter key.
                        // Rather than do that, we want the editor to persist whatever font style was applied when the enter key was hit.
                        continue;
                    }

                    mshtmlControl.ExecuteCommand((uint)editingOption.Key, editingOption.Value);
                }
                catch (COMException ex)
                {
                    // There is a bug in IE where the HR is incorrectly set for RESPECTVISIBILITY_INDESIGN, but
                    // the value is correctly set.  It is targetted to be fixed in IE9
                    const int E_NOTSUPPORTED = unchecked((int)0x80040100);
                    if (ex.ErrorCode == E_NOTSUPPORTED && (uint)editingOption.Key == IDM.RESPECTVISIBILITY_INDESIGN)
                    {
                        continue;
                    }

                    Trace.Fail(String.Format(CultureInfo.InvariantCulture, "Unexpected error setting IE editing option {0} to value {1}: {2}",
editingOption.Key, editingOption.Value, ex.ToString()));

                }
            }

            mshtmlControl.SetDLControlFlags(_mshtmlOptions.DLCTLOptions);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the document from the specified file name
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadFromFile(string filePath)
        {
            using (SuspendNotification())
            {
                mshtmlControl.LoadFromFile(filePath);
                SetStateForNewDocumentLoad();
            }
        }

        /// <summary>
        /// Load the document directly from a string
        /// </summary>
        public void LoadFromString(string html)
        {
            using (SuspendNotification())
            {
                mshtmlControl.LoadFromString(html);
                SetStateForNewDocumentLoad();
            }
        }

        /// <summary>
        /// Save the document to the file it was loaded from
        /// </summary>
        public void SaveToFile()
        {
            mshtmlControl.SaveToFile();
            IsDirty = false;
        }

        /// <summary>
        /// Saves the document as a new file
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveToFile(string fileName)
        {
            mshtmlControl.SaveToFile(fileName, false);
            IsDirty = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Is the document currently dirty? (in need of saving)
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
                if (_isDirty && IsDirtyEventHandler != null)
                {
                    IsDirtyEventHandler(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// This will suspend notifications from the time it is called,
        /// until the returned object is disposed, including any notification
        /// messages that are in the window message queue at the time of
        /// disposal. (This is achieved by using BeginInvoke, hopefully
        /// this turns out to be robust enough!)
        /// </summary>
        public IDisposable SuspendNotification()
        {
            ++_ignoreNotifyCount;
            return new ResumeNotificationHelper(this);
        }

        private class ResumeNotificationHelper : IDisposable
        {
            private readonly MshtmlEditor _parent;
            private bool _disposed = false;

            public ResumeNotificationHelper(MshtmlEditor _parent)
            {
                this._parent = _parent;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;

                    if (_parent.Handle != IntPtr.Zero)
                        _parent.BeginInvoke(new ThreadStart(ResumeNotification));
                }
            }

            private void ResumeNotification()
            {
                _parent._ignoreNotifyCount--;
            }
        }

        /// <summary>
        /// HTML Document we are hosted on
        /// </summary>
        public IHTMLDocument2 HTMLDocument
        {
            get
            {
                return htmlDocument;
            }
        }

        /// <summary>
        /// Get the commands exposed by the MSHTML editor (a dictionary w/
        /// keys of type MshtmlCommand and values of type IMshtmlCommand)
        /// </summary>
        public MshtmlCoreCommandSet Commands
        {
            get
            {
                return mshtmlControl.Commands;
            }
        }

        /// <summary>
        /// Get the events
        /// </summary>
        public IMshtmlDocumentEvents DocumentEvents
        {
            get
            {
                return mshtmlControl.DocumentEvents;
            }
        }

        /// <summary>
        /// Underlying mshtml control
        /// </summary>
        public MshtmlControl MshtmlControl
        {
            get
            {
                return mshtmlControl;
            }
        }

        public bool HasContiguousSelection
        {
            get
            {
                return mshtmlControl.HasContiguousSelection;
            }
        }

        /// <summary>
        /// Handler for the IDocHostUIHandler.GetDropTarget operation.
        /// </summary>
        public DropTargetUIHandler DropTargetHandler
        {
            get { return _dropTargetHandler; }
            set { _dropTargetHandler = value; }
        }
        private DropTargetUIHandler _dropTargetHandler;
        public delegate int DropTargetUIHandler(OpenLiveWriter.Interop.Com.IDropTarget pDropTarget, out OpenLiveWriter.Interop.Com.IDropTarget ppDropTarget);

        /// <summary>
        /// Register a context menu handler
        /// </summary>
        /// <param name="contextMenuHandler">handler to register</param>
        public void RegisterContextMenuHandler(ShowContextMenuHandler contextMenuHandler)
        {
            _contextMenuHandlers.Add(contextMenuHandler);
        }

        public void ClearContextMenuHandlers()
        {
            _contextMenuHandlers.Clear();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Alerts listeners that the state of the display has changed and they should
        /// therefore update the state of any connected UI (command bar, property editor, etc.)
        /// </summary>
        public event EventHandler DisplayChanged
        {
            add
            {
                DisplayChangedEventHandler += value;
                _eventCounter.EventHooked(DisplayChangedEventHandler);
            }
            remove
            {
                DisplayChangedEventHandler -= value;
                _eventCounter.EventUnhooked(DisplayChangedEventHandler);
            }
        }
        private event EventHandler DisplayChangedEventHandler;

        public event EventHandler IsDirtyEvent
        {
            add
            {
                IsDirtyEventHandler += value;
                _eventCounter.EventHooked(IsDirtyEventHandler);
            }
            remove
            {
                IsDirtyEventHandler -= value;
                _eventCounter.EventUnhooked(IsDirtyEventHandler);
            }
        }
        private event EventHandler IsDirtyEventHandler;

        /// <summary>
        /// Raise the DisplayChanged event
        /// </summary>
        /// <param name="ea"></param>
        protected virtual void OnDisplayChanged(EventArgs ea)
        {
            if (DisplayChangedEventHandler != null)
                DisplayChangedEventHandler(this, ea);
        }

        /// <summary>
        /// Occurs for command key processing.
        /// </summary>
        [
            Category("Action"),
                Description("")
        ]
        public event KeyEventHandler CommandKey
        {
            add
            {
                CommandKeyEventHandler += value;
                _eventCounter.EventHooked(CommandKeyEventHandler);
            }
            remove
            {
                CommandKeyEventHandler -= value;
                _eventCounter.EventUnhooked(CommandKeyEventHandler);
            }
        }
        private event KeyEventHandler CommandKeyEventHandler;

        /// <summary>
        /// Raises the CommandKey event.
        /// </summary>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        protected virtual void OnCommandKey(KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.F1:
                    MshtmlControl.FireHelpRequest();
                    e.Handled = true;
                    return;
                case Keys.F5: // Block F5 from mshtml so it doesnt refresh the editor
                    e.Handled = true;
                    return;
            }

            if (CommandKeyEventHandler != null)
                CommandKeyEventHandler(this, e);
        }

        /// <summary>
        /// Provide the ability to filter editor events
        /// </summary>
        public event HtmlEditDesignerEventHandler PreHandleEvent
        {
            add
            {
                preEventHandleList.Insert(0, value);

                PreHandleEventHandler += value; // Used only for debug purposes.
                _eventCounter.EventHooked(PreHandleEventHandler);
            }
            remove
            {
                preEventHandleList.Remove(value);

                PreHandleEventHandler -= value; // Used only for debug purposes.
                _eventCounter.EventUnhooked(PreHandleEventHandler);
            }
        }
        private List<HtmlEditDesignerEventHandler> preEventHandleList = new List<HtmlEditDesignerEventHandler>();
        private event HtmlEditDesignerEventHandler PreHandleEventHandler; // Used only for debug purposes.

        /// <summary>
        /// Provide the ability to override accelerator processing
        /// </summary>
        public event HtmlEditDesignerEventHandler TranslateAccelerator
        {
            add
            {
                TranslateAcceleratorEventHandler += value;
                _eventCounter.EventHooked(TranslateAcceleratorEventHandler);
            }
            remove
            {
                TranslateAcceleratorEventHandler -= value;
                _eventCounter.EventUnhooked(TranslateAcceleratorEventHandler);
            }
        }
        private event HtmlEditDesignerEventHandler TranslateAcceleratorEventHandler;

        /// <summary>
        /// Help requested via the F1 key
        /// </summary>
        public event EventHandler HelpRequest
        {
            add
            {
                MshtmlControl.HelpRequest += value;
            }
            remove
            {
                MshtmlControl.HelpRequest -= value;
            }
        }

        /// <summary>
        /// Notification that we are about to show the context menu
        /// </summary>
        public event EventHandler BeforeShowContextMenu
        {
            add
            {
                BeforeShowContextMenuEventHandler += value;
                _eventCounter.EventHooked(BeforeShowContextMenuEventHandler);
            }
            remove
            {
                BeforeShowContextMenuEventHandler -= value;
                _eventCounter.EventUnhooked(BeforeShowContextMenuEventHandler);
            }
        }
        private event EventHandler BeforeShowContextMenuEventHandler;

        #endregion

        #region Implementation of IDocHostUIHandler

        /// <summary>
        /// Override the MHTML context menu
        /// </summary>
        /// <param name="dwID">context menu id</param>
        /// <param name="ppt">point where the context click occurred</param>
        /// <param name="pcmdtReserved">reserved</param>
        /// <param name="pdispReserved">reserved</param>
        /// <returns>S_OK to indicate context menu overridden, otherwise S_FALSE</returns>
        int IDocHostUIHandler2.ShowContextMenu(int dwID, ref POINT ppt, object pcmdtReserved, object pdispReserved)
        {
            try
            {
                Point screenPoint = new Point(ppt.x, ppt.y);
                Point clientPoint = PointToClient(screenPoint);

                //If clientPoint is (0,0) then use the location of the caret to display the menu.
                if (clientPoint == Point.Empty)
                {
                    //this is a keyboard invocation of the context menu, so show the context menu
                    //at the caret location
                    IHTMLCaretRaw caret;
                    mshtmlControl.DisplayServices.GetCaret(out caret);
                    //calculate the screen point of the caret
                    //Note: we don't test for caret visibility anymore since the caret
                    //is not considered visible if there is a contiguous selection (bug 476391)
                    POINT p;
                    caret.GetLocation(out p, true);
                    screenPoint = PointToScreen(new Point(p.x, p.y));
                }

                if (ShowContextMenu(screenPoint))
                    return HRESULT.S_OK;

                return HRESULT.S_FALSE;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Unexpected error occurred during ShowContextMenu: " + ex.ToString());
                return HRESULT.S_OK; // supress menu in case of error
            }
        }

        public bool ShowContextMenu(Point screenPoint)
        {
            try
            {
                // Fire notification that we are about to show the context menu
                if (BeforeShowContextMenuEventHandler != null)
                    BeforeShowContextMenuEventHandler(this, EventArgs.Empty);

                // NOTE: The Supressing of UpdateUI notifications during display of the context menu was
                // necessary to fix Bug# 244853. The main editor hooks these notifications in order to
                // update command states. Unfortunately this notification is called quite eagerly by the
                // editor (mouse move causes it to fire), so command management was actually occuring
                // while the context menu was being created and shown. In some cases this caused the context
                // menu to flash in and out of view (still not 100% clear on why). In any event, supressing
                // UpdateUI (and therefore command management) during the showing of context menus reliably
                // fixes the problem.
                SuppressUpdateUI = true;

                // show the context menu
                IHTMLElement element = ElementAtPoint(screenPoint);

                foreach (ShowContextMenuHandler contextMenuHandler in _contextMenuHandlers)
                    if (contextMenuHandler(element, screenPoint))
                        return true;

                // no custom handler took it, show default
                return false;
            }
            finally
            {
                SuppressUpdateUI = false;
            }
        }

        /// <summary>
        /// Called by MSHTML to retrieve the user interface (UI) capabilities of the application that is hosting MSHTML
        /// </summary>
        /// <param name="pInfo">Pointer to a DOCHOSTUIINFO structure that receives the host's UI capabilities</param>
        void IDocHostUIHandler2.GetHostInfo(ref DOCHOSTUIINFO pInfo)
        {
            // Internet Explorer 6 or later. Turns off any 3-D border on the outermost frame or frameset only
            pInfo.dwFlags |= DOCHOSTUIFLAG.NO3DOUTERBORDER | DOCHOSTUIFLAG.DPI_AWARE;

            if (_mshtmlOptions.UseDivForCarriageReturn)
            {
                pInfo.dwFlags |=
                    // MSHTML inserts the div tag if a return is entered in edit mode. Without this flag,
                    // MSHTML will use the p tag.
                    DOCHOSTUIFLAG.DIV_BLOCKDEFAULT;
            }
        }

        /// <summary>
        /// Called by MSHTML to enable the host to replace MSHTML menus and toolbars
        /// </summary>
        /// <param name="dwID">DWORD that receives a DOCHOSTUITYPE value indicating the type of user interface (UI)</param>
        /// <param name="pActiveObject">Pointer to an IOleInPlaceActiveObject interface for the active object</param>
        /// <param name="pCommandTarget">Pointer to an IOleCommandTarget interface for the object</param>
        /// <param name="pFrame">Pointer to an IOleInPlaceFrame interface for the object. Menus and toolbars must use this parameter</param>
        /// <param name="pDoc">Pointer to an IOleInPlaceUIWindow interface for the object. Toolbars must use this parameter</param>
        /// <returns>S_OK -- Host displayed its own UI. MSHTML will not display its UI.
        /// S_FALSE -- Host did not display its own UI. MSHTML will display its UI</returns>
        int IDocHostUIHandler2.ShowUI(DOCHOSTUITYPE dwID, IOleInPlaceActiveObject pActiveObject, IOleCommandTarget pCommandTarget, IOleInPlaceFrame pFrame, IOleInPlaceUIWindow pDoc)
        {
            // Host did not display any UI. MSHTML will display its UI.
            return HRESULT.S_FALSE;
        }

        /// <summary>
        /// Called when MSHTML removes its menus and toolbars
        /// </summary>
        void IDocHostUIHandler2.HideUI()
        {
        }

        /// <summary>
        /// Called by MSHTML to notify the host that the command state has changed
        /// </summary>
        void IDocHostUIHandler2.UpdateUI()
        {
            try
            {
                if (!SuppressUpdateUI)
                    OnDisplayChanged(EventArgs.Empty);
            }
            catch (Exception e)
            {
                HandleUncaughtException(e);
            }
        }

        private bool SuppressUpdateUI
        {
            get
            {
                return _suppressUpdateUI;
            }
            set
            {
                if (value != _suppressUpdateUI)
                {
                    _suppressUpdateUI = value;

                    // if we are re-enabling UpdateUI then automatically
                    // call UpdateUI for a refresh
                    if (!_suppressUpdateUI)
                        ((IDocHostUIHandler2)this).UpdateUI();
                }
            }
        }
        private bool _suppressUpdateUI = false;

        /// <summary>
        /// Called by the MSHTML implementation of IOleInPlaceActiveObject::EnableModeless. Also called when MSHTML displays a modal UI
        /// </summary>
        /// <param name="fEnable">true=modeless dialogs are enabled, false=modeless dialogs are disabled</param>
        void IDocHostUIHandler2.EnableModeless(bool fEnable)
        {
        }

        /// <summary>
        /// Called by the MSHTML implementation of IOleInPlaceActiveObject::OnDocWindowActivate
        /// </summary>
        /// <param name="fActivate">true when doc window is being activated, false when deactivated</param>
        void IDocHostUIHandler2.OnDocWindowActivate(bool fActivate)
        {
        }

        /// <summary>
        /// Called by the MSHTML implementation of IOleInPlaceActiveObject::OnFrameWindowActivate
        /// </summary>
        /// <param name="fActivate">true when frame window is being activated, false when deactivated</param>
        void IDocHostUIHandler2.OnFrameWindowActivate(bool fActivate)
        {
        }

        /// <summary>
        /// Called by the MSHTML implementation of IOleInPlaceActiveObject::ResizeBorder
        /// </summary>
        /// <param name="prcBorder">pointer to a RECT for the new outer rectangle of the border</param>
        /// <param name="pUIWindow">Pointer to an IOleInPlaceUIWindow interface for the frame or document window whose border is to be changed</param>
        /// <param name="frameWindow">BOOL that is TRUE if the frame window is calling IDocHostUIHandler::ResizeBorder, or FALSE otherwise</param>
        void IDocHostUIHandler2.ResizeBorder(ref RECT prcBorder, IOleInPlaceUIWindow pUIWindow, bool frameWindow)
        {
        }

        private KeyEventArgs lastKeyEventArgs;

        /// <summary>
        /// Called by MSHTML when IOleInPlaceActiveObject::TranslateAccelerator or IOleControlSite::TranslateAccelerator is called
        /// </summary>
        /// <param name="lpMsg">Pointer to a MSG structure that specifies the message to be translated</param>
        /// <param name="pguidCmdGroup">Pointer to a GUID for the command group identifier</param>
        /// <param name="nCmdID">DWORD that specifies a command identifier</param>
        /// <returns>S_OK if the message was translated successfully, S_FALSE if the message was not translated</returns>
        int IDocHostUIHandler2.TranslateAccelerator(ref MSG lpMsg, ref Guid pguidCmdGroup, uint nCmdID)
        {
            if (lpMsg.message != WM.KEYDOWN && lpMsg.message != WM.SYSKEYDOWN)
            {
                if (lpMsg.message == WM.CHAR)
                {
                    Keys currentKeyCode = ((Keys)(int)lpMsg.wParam & Keys.KeyCode);

                    if ((Control.ModifierKeys & Keys.Control) > 0)
                    {
                        if (currentKeyCode == Keys.Space)
                        {
                            // WinLive 72691: Don't let Mail process WM_CHAR for space key when part of Ctrl-Space.
                            return HRESULT.S_OK;
                        }
                        else if (currentKeyCode == Keys.Enter)
                        {
                            // WinLive 170257 & 170271: Don't let MSHTML process WM_CHAR for Enter key when the user hits Ctrl+M or Ctrl+Shift+M.
                            return HRESULT.S_OK;
                        }
                    }
                    else if (Control.ModifierKeys == Keys.None)
                    {
                        // WinLive 219280: A WM_CHAR message is posted when a WM_KEYDOWN message is translated. The
                        // WM_CHAR message contains the character code of the key that was pressed. If we already
                        // handled the key in the WM_KEYDOWN message, we don't want to handle it again (or let MSHTML
                        // handle it). Modifier keys can cause the WM_CHAR keycode to differ from the WM_KEYDOWN
                        // keycode, so we only do the comparison if no modifier keys are being held down.
                        if (lastKeyEventArgs != null && lastKeyEventArgs.Handled && lastKeyEventArgs.KeyCode == currentKeyCode)
                        {
                            return HRESULT.S_OK;
                        }
                    }
                }

                return HRESULT.S_FALSE;
            }

            // convert the wParam into a .Net Keys value
            Keys key = (Keys)(int)lpMsg.wParam & Keys.KeyCode;

            // Convert the wParam into a .Net Keys value and combine it with
            // any currently pressed modifier keys
            Keys keyData = key | ModifierKeys;

            //	Instantiate the KeyEventArgs.
            KeyEventArgs keyEventArgs = new KeyEventArgs(keyData);

            //	Raise the CommandKey event.
            OnCommandKey(keyEventArgs);

            // Save a copy in case we get a WM_CHAR message next.
            lastKeyEventArgs = keyEventArgs;

            //	If the key was handled, return S_OK.  Otherwise, return S_FALSE.
            if (keyEventArgs.Handled)
            {
                //translated accellerator
                return HRESULT.S_OK;
            }
            else
            {
                // didn't translate accelerator
                return HRESULT.S_FALSE;
            }
        }

        /// <summary>
        /// Called by the WebBrowser Control to retrieve a registry subkey path that changes the location of the default Microsoft® Internet Explorer registry settings
        /// (typically found at HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer)
        /// </summary>
        /// <param name="pchKey">Pointer to an LPOLESTR that receives the registry subkey string where the host stores its registry settings</param>
        /// <param name="dwReserved">Reserved. Must be set to NULL</param>
        void IDocHostUIHandler2.GetOptionKeyPath(out IntPtr pchKey, uint dwReserved)
        {
            if (!String.IsNullOrEmpty(_mshtmlOptions.DocHostUIOptionKeyPath))
                // MSHTML is responsible for freeing the allocated memory
                pchKey = Marshal.StringToCoTaskMemUni(_mshtmlOptions.DocHostUIOptionKeyPath);
            else
                pchKey = IntPtr.Zero;
        }

        /// <summary>
        /// Called by the WebBrowser Control to retrieve a registry subkey path that overrides the settings found in the default Microsoft® Internet Explorer registry settings
        /// </summary>
        /// <param name="pchKey"></param>
        /// <param name="dwReserved"></param>
        void IDocHostUIHandler2.GetOverrideKeyPath(out IntPtr pchKey, uint dwReserved)
        {
            if (!String.IsNullOrEmpty(_mshtmlOptions.DocHostUIOverrideKeyPath))
                // MSHTML is responsible for freeing the allocated memory
                pchKey = Marshal.StringToCoTaskMemUni(_mshtmlOptions.DocHostUIOverrideKeyPath);
            else
                pchKey = IntPtr.Zero;
        }

        /// <summary>
        /// Called by MSHTML to obtain the host's IDispatch interface
        /// </summary>
        /// <param name="ppDispatch">Address of a pointer to a variable that receives an IDispatch interface pointer for the host application</param>
        void IDocHostUIHandler2.GetExternal(out IntPtr ppDispatch)
        {
            // no external dispatch implementaiton available
            ppDispatch = IntPtr.Zero;
        }

        /// <summary>
        /// Delegate to DragAndDropManager for GetDropTarget
        /// </summary>
        /// <param name="pDropTarget">default implementation</param>
        /// <param name="ppDropTarget">our implemetation</param>
        /// <returns>S_OK to indicate that we replaced implementation, otherwise E_NOTIMPL</returns>
        int IDocHostUIHandler2.GetDropTarget(OpenLiveWriter.Interop.Com.IDropTarget pDropTarget, out OpenLiveWriter.Interop.Com.IDropTarget ppDropTarget)
        {
            if (DropTargetHandler != null)
            {
                return DropTargetHandler(pDropTarget, out ppDropTarget);
            }
            else
            {
                ppDropTarget = null;
                return HRESULT.E_NOTIMPL;
            }
        }

        /// <summary>
        /// Called by MSHTML to give the host an opportunity to modify the URL to be loaded
        /// </summary>
        /// <param name="dwReserved">Reserved. Must be set to NULL</param>
        /// <param name="pchURLIn">Pointer to an OLECHAR that specifies the current URL for navigation</param>
        /// <param name="ppchURLOut">Address of a pointer variable that receives an OLECHAR pointer containing the new URL</param>
        /// <returns>Returns S_OK if the URL was translated, or S_FALSE if the URL was not translated</returns>
        int IDocHostUIHandler2.TranslateUrl(uint dwReserved, IntPtr pchURLIn, out IntPtr ppchURLOut)
        {
            // do not translate url
            ppchURLOut = pchURLIn;
            return HRESULT.S_FALSE;
        }

        /// <summary>
        /// Called by MSHTML to allow the host to replace the MSHTML data object
        /// </summary>
        /// <param name="pDO">Pointer to an IDataObject interface supplied by MSHTML</param>
        /// <param name="ppDORet">Address of a pointer variable that receives an IDataObject interface pointer supplied by the host</param>
        /// <returns>Returns S_OK if the data object is replaced, or S_FALSE if it's not replaced</returns>
        int IDocHostUIHandler2.FilterDataObject(IOleDataObject pDO, out IOleDataObject ppDORet)
        {
            // do not filter data object
            ppDORet = pDO;
            return HRESULT.S_FALSE;
        }

        #endregion

        #region Implementation of IHTMLEditDesigner

        /// <summary>
        /// Called by MSHTML before the MSHTML Editor processes an event, so that the designer
        /// can provide its own event handling behavior
        /// </summary>
        /// <param name="inEvtDispId">event id</param>b
        /// <param name="pIEventObj">event object</param>
        /// <returns>S_OK to indicate event is handled and should not be processed further,
        /// S_FALSE to allow it continue processing</returns>
        int IHTMLEditDesignerRaw.PreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            try
            {
                if (IsInvalidLink(inEvtDispId, pIEventObj))
                    return HRESULT.S_OK;

                if (preEventHandleList.Count != 0)
                {
                    for (int i = 0; i < preEventHandleList.Count; i++)
                    {
                        if (pIEventObj.cancelBubble)
                            break;

                        int result = preEventHandleList[i](inEvtDispId, pIEventObj);
                        if (result == HRESULT.S_OK)
                            return HRESULT.S_OK;
                    }
                }

                return HRESULT.S_FALSE;
            }
            catch (Exception e)
            {
                HandleUncaughtException(e);
                return HRESULT.S_FALSE;
            }
        }

        private static bool IsInvalidLink(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (pIEventObj.ctrlKey &&
               (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONCLICK ||
                inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN ||
                inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEUP))
            {
                IHTMLAnchorElement anchorElement = HTMLElementHelper.GetContainingAnchorElement(pIEventObj.srcElement);

                if (anchorElement != null)
                {
                    string url = ((IHTMLElement)anchorElement).getAttribute("href", 2) as string;
                    // Ignore clicks on anchor tags that don't have a valid URL in their href
                    if (!string.IsNullOrEmpty(url) && !UrlHelper.IsKnownScheme(url))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Custom processing for keyboard input
        /// </summary>
        /// <param name="inEvtDispId">keyboard event</param>
        /// <param name="pIEventObj">event object</param>
        /// <returns>S_OK to indicate keyboard input was fully handled, else S_FALSE</returns>
        int IHTMLEditDesignerRaw.TranslateAccelerator(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            try
            {
                // forward to all listeners
                bool maskFromMshtml = false;
                if (TranslateAcceleratorEventHandler != null)
                {
                    foreach (HtmlEditDesignerEventHandler handler in TranslateAcceleratorEventHandler.GetInvocationList())
                    {
                        int result = handler(inEvtDispId, pIEventObj);
                        if (result == HRESULT.S_OK)
                            maskFromMshtml = true;
                    }
                }

                // mask from mshtml if requested
                if (maskFromMshtml)
                    return HRESULT.S_OK;
                else
                    return HRESULT.S_FALSE;
            }
            catch (Exception e)
            {
                HandleUncaughtException(e);
                return HRESULT.S_FALSE;
            }
        }

        /// <summary>
        /// Notification that an event has already occurred
        /// </summary>
        /// <param name="inEvtDispId">event id</param>
        /// <param name="pIEventObj">event object</param>
        void IHTMLEditDesignerRaw.PostEditorEventNotify(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (PostEditorEventHandler != null)
                PostEditorEventHandler(this, new EditDesignerEventArgs(inEvtDispId, pIEventObj));
        }

        public delegate void EditDesignerEventHandler(object sender, EditDesignerEventArgs args);
        public class EditDesignerEventArgs
        {
            public int EventDispId;
            public IHTMLEventObj EventObj;

            public EditDesignerEventArgs(int eventDispId, IHTMLEventObj eventObj)
            {
                EventDispId = eventDispId;
                EventObj = eventObj;
            }
        }

        public event EditDesignerEventHandler PostEditorEvent
        {
            add
            {
                PostEditorEventHandler += value;
                _eventCounter.EventHooked(PostEditorEventHandler);
            }
            remove
            {
                PostEditorEventHandler -= value;
                _eventCounter.EventUnhooked(PostEditorEventHandler);
            }
        }
        private event EditDesignerEventHandler PostEditorEventHandler;

        private ushort lastSeenLangId;

        private bool trackKeyboardLanguageChange;

        /// <summary>
        /// If set to true, the KeyboardLanguageChanged event will be fired when the
        /// editor notices that the keyboard language has changed.
        /// </summary>
        public bool TrackKeyboardLanguageChanges
        {
            get
            {
                return trackKeyboardLanguageChange;
            }
            set
            {
                trackKeyboardLanguageChange = value;
                if (value)
                    lastSeenLangId = (ushort)(User32.GetKeyboardLayout(0) & 0xFFFF);
            }
        }

        /// <summary>
        /// Called by MSHTML after the MSHTML Editor processes an event, so that the designer
        /// can provide its own event handling behavior
        /// </summary>
        /// <param name="inEvtDispId">event id</param>
        /// <param name="pIEventObj">event object</param>
        /// <returns>S_OK to indicate event is handled and should not be processed further,
        /// S_FALSE to allow it to continue processing (call PostEditorEventNotify, etc.)</returns>
        int IHTMLEditDesignerRaw.PostHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (trackKeyboardLanguageChange)
            {
                ushort langId = (ushort)(User32.GetKeyboardLayout(0) & 0xFFFF);
                if (lastSeenLangId != langId)
                {
                    lastSeenLangId = langId;
                    if (KeyboardLanguageChangedEventHandler != null)
                        KeyboardLanguageChangedEventHandler(this, EventArgs.Empty);
                }
            }

            return HRESULT.S_FALSE;
        }

        public event EventHandler KeyboardLanguageChanged
        {
            add
            {
                KeyboardLanguageChangedEventHandler += value;
                _eventCounter.EventHooked(KeyboardLanguageChangedEventHandler);
            }
            remove
            {
                KeyboardLanguageChangedEventHandler -= value;
                _eventCounter.EventUnhooked(KeyboardLanguageChangedEventHandler);
            }
        }
        private event EventHandler KeyboardLanguageChangedEventHandler;

        #endregion

        #region Implemetation if IHTMLChangeSink

        /// <summary>
        /// Implement IHtmlChangeSink to get notified of changes to the document
        /// </summary>
        void IHTMLChangeSink.Notify()
        {
            if (_ignoreNotifyCount <= 0)
            {
                // notify means the document is dirty
                IsDirty = true;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Notify form that ActiveControl has changed so that Enter and
        /// Leave events are fired correctly for other controls
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void DocumentEvents_GotFocus(object sender, EventArgs e)
        {
            Form form = FindForm();
            if (form != null)
                form.ActiveControl = this;
        }

        #endregion

        #region General Purpose Private Helpers

        private void SetStateForNewDocumentLoad()
        {
            Debug.Assert(_ignoreNotifyCount > 0, "Do you really want this to result in a dirty editor?");

            IsDirty = false;
            _documentCompleteReadyStateFired = false;
        }

        /// <summary>
        /// Begin a logically grouped set of undoable operations
        /// </summary>
        private void BeginUndoUnit()
        {
            // get IMarkupServicesRaw interface to overcome incorrect mshtml pia declarations
            IMarkupServicesRaw markupServicesRaw = (IMarkupServicesRaw)mshtmlControl.MarkupServices;

            // begin undo unit
            markupServicesRaw.BeginUndoUnit(Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Complete logically grouped set of undoable operations
        /// </summary>
        private void EndUndoUnit()
        {
            mshtmlControl.MarkupServices.EndUndoUnit();
        }

        /// <summary>
        /// Execute an Undo
        /// </summary>
        private void Undo()
        {
            mshtmlControl.ExecuteCommand(IDM.UNDO);
        }

        /// <summary>
        /// Execute a Redo
        /// </summary>
        private void Redo()
        {
            mshtmlControl.ExecuteCommand(IDM.REDO);
        }

        /// <summary>
        /// Handle an uncaught exception (used within COM interop interface implementations
        /// where uncaught exceptions are basically ignored unless you catch and deal w/ them).
        /// This method should be called from all top-level COM interop entry points.
        /// </summary>
        /// <param name="e"></param>
        private void HandleUncaughtException(Exception e)
        {
            UnexpectedErrorMessage.Show(e);
        }

        /// <summary>
        /// Get the element at the specified screen point (returns null if there is no element)
        /// </summary>
        /// <param name="screenPoint">screen point to check</param>
        /// <returns>element at point (or null if none)</returns>
        private IHTMLElement ElementAtPoint(Point screenPoint)
        {
            Point clientPoint = PointToClient(screenPoint);
            if (PointIsOverDocumentArea(clientPoint.X, clientPoint.Y))
                return HTMLDocument.elementFromPoint(clientPoint.X, clientPoint.Y);
            else
                return null;
        }

        /// <summary>
        /// Check to see if the specified point is directly over the document (client rectangle
        /// excluding scrollbars if they are visible)
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns>true if it is in the client area, otherwise false</returns>
        private bool PointIsOverDocumentArea(int x, int y)
        {
            // get references to body element
            IHTMLElement body = HTMLDocument.body;
            IHTMLElement2 body2 = (IHTMLElement2)body;

            // see if the point is over one of the scrollbars
            bool pointOverVerticalScrollBar = x >= (ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth);
            bool pointOverHorizontalScrollBar = ((body.offsetWidth - body2.scrollWidth) < SystemInformation.VerticalScrollBarWidth) && (y >= ClientRectangle.Height - SystemInformation.HorizontalScrollBarHeight);

            // return true if it is not over one of the two scroll-bars
            return (!(pointOverVerticalScrollBar || pointOverHorizontalScrollBar));
        }

        #endregion

        #region Private Member Variables

        /// <summary>
        /// Underlying MSHTML editing control
        /// </summary>
        private MshtmlControl mshtmlControl;

        /// <summary>
        /// Underlying HTML document
        /// </summary>
        private IHTMLDocument2 htmlDocument;

        /// <summary>
        /// Callbacks registered for showing custom context menus
        /// </summary>
        private ArrayList _contextMenuHandlers = new ArrayList();

        /// <summary>
        /// cookie used for manipulating change sink for underlying document
        /// (used to track dirty status)
        /// </summary>
        private uint _changeSinkCookie;

        /// <summary>
        /// Is the underlying document dirty?
        /// </summary>
        private bool _isDirty = false;

        /// <summary>
        /// Flag indicating we have recieved the event for the DocumentComplete ReadyState
        /// </summary>
        private bool _documentCompleteReadyStateFired = false;

        /// <summary>
        /// Counter for calls to IgnoreNextNotify which controls whether changes
        /// to the document "count" for setting the dirty bit
        /// </summary>
        private int _ignoreNotifyCount = 0;

        private MshtmlOptions _mshtmlOptions;

        #endregion

        public void UpdateOptions(MshtmlOptions options, bool updateComposeSettings)
        {
            _mshtmlOptions = options;
            // If the document has already loaded then we need to apply the settings
            // again right away so they take effect
            if (_documentCompleteReadyStateFired)
                InitializeDocumentEditingOptions(updateComposeSettings);
        }
    }

    public delegate void HTMLElementEventHandler(object source, HTMLElementEvent e);
    public class HTMLElementEvent : EventArgs
    {
        public HTMLElementEvent(IHTMLElement element)
        {
            this.element = element;
        }

        public IHTMLElement Element
        {
            get { return element; }
            set { element = value; }
        }
        private IHTMLElement element;
    }

    public delegate bool ShowContextMenuHandler(IHTMLElement element, Point screenPoint);

    /// <summary>
    /// Delegate used for handling html document events
    /// </summary>
    public delegate int HtmlEditDesignerEventHandler(int inEvtDispId, IHTMLEventObj pIEventObj);

    public class MshtmlOptions
    {
        public MshtmlOptions()
        {
        }

        /// <summary>
        /// Set the options for how the editor will download content.  -1 will provide the default options.
        /// </summary>
        public int DLCTLOptions
        {
            get { return _DLCTLOptions; }
            set
            {
                if (value == -1)
                {
                    _DLCTLOptions = DEFAULT_DLCTL;
                    return;
                }
                _DLCTLOptions = value;
            }
        }

        public const int DEFAULT_DLCTL = DLCTL.SILENT | DLCTL.DLIMAGES | DLCTL.VIDEOS | DLCTL.NO_SCRIPTS;
        private int _DLCTLOptions = DEFAULT_DLCTL;

        public bool UseDivForCarriageReturn
        {
            get { return _useDivForCarriageReturn; }
            set { _useDivForCarriageReturn = value; }
        }
        private bool _useDivForCarriageReturn = false;

        public ListDictionary EditingOptions
        {
            get { return _editingOptions; }
        }
        private ListDictionary _editingOptions = new ListDictionary();

        /// <summary>
        /// Base registry key path that used to override the IE settings found in
        /// HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer
        /// </summary>
        public String DocHostUIOverrideKeyPath
        {
            get { return _docHostUIOverrideKeyPath; }
            set { _docHostUIOverrideKeyPath = value; }
        }
        private String _docHostUIOverrideKeyPath = null;

        /// <summary>
        /// Base registry key path that used to override the IE settings found in
        /// HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer
        /// </summary>
        public String DocHostUIOptionKeyPath
        {
            get { return _docHostUIOptionKeyPath; }
            set { _docHostUIOptionKeyPath = value; }
        }
        private String _docHostUIOptionKeyPath = null;
    }

    public class MshtmlEditingOption
    {
        public MshtmlEditingOption(uint option, bool enabled)
        {
            Option = option;
            Enabled = enabled;
        }

        public readonly uint Option;
        public readonly bool Enabled;

    }
}
