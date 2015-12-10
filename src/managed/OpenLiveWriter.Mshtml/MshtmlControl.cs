// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.Com.StructuredStorage;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml.Mshtml_Interop;
using IPersistFile = OpenLiveWriter.Interop.Com.IPersistFile;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// MshtmlControl provides an HTML viewing and editing control. It does this by
    /// hosting the MSHTML ActiveDocument server (the core underlying component
    /// used by Internet Explorer to implement web browsing). Hosting an ActiveDocument
    /// is a painfully overcomplicated and underdocumented chore. Our implementation of
    /// ActiveDocument hosting is based on the best example of it we could find which
    /// was the Microsoft FramerEx sample ("SAMPLE: FramerEx.exe Is an MDI ActiveX
    /// Document Container Sample Written in Visual C++"). This sample is described
    /// in MSFT KB Article 268470 at:
    ///   http://support.microsoft.com/default.aspx?scid=kb;EN-US;268470
    ///
    /// Another more elemental sample of an ActiveDocument container implementation
    /// is the "Framer" sample from MSDN. You can download this sample at:
    ///   http://msdn.microsoft.com/downloads/samples/internet/default.asp?url=/downloads/samples/internet/components/framer/default.asp
    ///
    /// Other documentation on ActiveDocument containers can be found in the MSFT
    /// Visual C++ documentation article "ActiveDocument Containers" at:
    ///   http://msdn.microsoft.com/library/default.asp?url=/library/en-us/vccore/html/_core_activex_document_containers.asp
    ///
    /// Kraig Brockschmidt's "Inside OLE" book is another good source of information
    /// on creating OLE containers.
    ///
    /// Implementations of methods within MshtmlControl that are based heavily upon the
    /// FramerEx sample will be called out within inline comments so that the process of
    /// troubleshooting them can make reference to the original implementation.
    /// </summary>
    [ComVisible(true)]
    public class MshtmlControl : UserControl,
        IOleDocumentSite,
        IOleClientSite, IOleInPlaceSite, IOleCommandTarget,
        IOleWindow, IOleInPlaceUIWindow, IOleInPlaceFrame,
        IDocHostUIHandler, IDocHostUIHandler2, IDocHostShowUI, IServiceProviderRaw,
        IProtectFocus
    {
        private readonly EventCounter _eventCounter = new EventCounter();

        #region Creation / Disposal

        /// <summary>
        /// Create mshtml control. To fully initialize the control, call the additional
        /// initialization methods in the sequence they are presented below:
        ///		(1) Constructor
        ///		(2) InstallDocHostUIHandler, InstallShowUIHandler
        ///		(3) SetDLControlFlags (must call this w/ desired options or page won't load/display
        ///		    as you expect!)
        ///		(4) Init
        ///		(5) SetEditMode
        ///		(6) Hook DocumentEvents.ReadyStateChanged so you can watch for
        ///		    DocumentIsComplete == true after calling LoadFrom asynch-method
        ///		(7) LoadFromXXX (Url, File, or String)
        /// </summary>
        public MshtmlControl()
            : this(null, false)
        {
        }

        public MshtmlControl(IServiceProviderRaw serviceProvider, bool protectFocus)
        {
            _serviceProvider = serviceProvider;
            ProtectFocus = protectFocus;
        }

        public void SetServiceProvider(IServiceProviderRaw serviceProvider)
        {
            // Everytime we switch the service provider we need to clear out who
            // the editor thinks it's site is.  This will force it to ask for the
            // the service provider again.  If you don't do this, any editor that is reused for
            // multiple windows in Mail will contain a stale pointer to objects that were
            // quired through the service provider.  The most obvious being behaviors won't work
            // because it has cached behavior manager will not have access to the new editor resulting
            // in smart content not bring selected correctly or images not being resamples when resized.
            _serviceProvider = serviceProvider;
            oleObject.SetClientSite(null);
            oleObject.SetClientSite((IOleClientSite)this);
        }

        /// <summary>
        /// Install a custom IDocHostUIHandler
        /// </summary>
        /// <param name="handler">handler to install</param>
        public void InstallDocHostUIHandler(IDocHostUIHandler2 handler)
        {
            docHostUIHandler = handler;
        }

        /// <summary>
        /// Uninstalls the custom IDocHostUIHandler by replacing it with a dummy handler
        /// </summary>
        public void UninstallDocHostUIHandler()
        {
            docHostUIHandler = new IDocHostUIHandlerBaseImpl();
        }

        /// <summary>
        /// Install a custom IDocHostShowUIHandler
        /// </summary>
        /// <param name="handler">handler to install</param>
        public void InstallShowUIHandler(IDocHostShowUI handler)
        {
            showUIHandler = handler;
        }

        /// <summary>
        /// Set flags used to control downloading
        /// </summary>
        /// <param name="flags">dl control flags (see DLCTL enumeration)</param>
        public void SetDLControlFlags(int flags)
        {
            dlctlFlags = flags;
            IOleControl control = HTMLDocument as IOleControl;
            if (control != null)
            {
                control.OnAmbientPropertyChange(MSHTML_DISPID.AMBIENT_DLCONTROL);
            }

            OnDLControlFlagsChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Call this method after you have installed handlers, configured behavior, etc.
        /// </summary>
        public void Init()
        {
            InitializeMSHTML();

            _eventCounter.Reset();
        }

        /// <summary>
        /// Indicate whether the mshtml control should be in edit mode. You may
        /// only call this method after you have called Init()
        /// </summary>
        /// <param name="editMode"></param>
        public void SetEditMode(bool editMode)
        {
            Debug.Assert(htmlDocument != null, "Must call Init before SetEditMode");
            if (editMode)
                htmlDocument.designMode = "On";
            else
                htmlDocument.designMode = "Off";

            //cache the edit mode to prevent performance hits when commands query for editmode.
            _editMode = editMode;
        }

        /// <summary>
        /// Load the contents of the Mshtml control from a URL
        /// </summary>
        /// <param name="url">url to load from</param>
        public void LoadFromUrl(string url)
        {
            // create moniker for url
            IMoniker urlMoniker;
            int hr = UrlMon.CreateURLMoniker(null, url, out urlMoniker);
            if (hr != HRESULT.S_OK)
                throw new COMException("Error creating url moniker for " + url, hr);

            // create a binding context
            IBindCtx bindCtx;
            hr = Ole32.CreateBindCtx(0, out bindCtx);
            if (hr != HRESULT.S_OK)
                throw new COMException("Error creating binding context for " + url, hr);

            // load the document from the url moniker
            IPersistMoniker persistMoniker = (IPersistMoniker)htmlDocument;
            persistMoniker.Load(false, urlMoniker, bindCtx, STGM.READ);
        }

        /// <summary>
        /// Load the contents of the Mshtml control from a file
        /// </summary>
        /// <param name="filePath"></param>
        public void LoadFromFile(string filePath)
        {
            IPersistFile persistFile = (IPersistFile)htmlDocument;
            persistFile.Load(filePath, STGM.READWRITE);
        }

        /// <summary>
        /// Saves the the file from which the control was loaded from. Note that
        /// this method only works if the MSHTML control was originally loaded
        /// from a file.
        /// </summary>
        public void SaveToFile()
        {
            SaveToFile(null, false);
        }

        public void SaveToFile(string fileName, bool asCopy)
        {
            IPersistFile persistFile = (IPersistFile)htmlDocument;
            persistFile.Save(fileName, !asCopy);
            persistFile.SaveCompleted(fileName);
        }

        /// <summary>
        /// Load the HTML document from a string
        /// </summary>
        /// <param name="html">HTML to load</param>
        public void LoadFromString(string html)
        {
            // declare unmanged resources
            IntPtr hHTML = IntPtr.Zero;
            IStream stream = null;

            try
            {
                // copy the string to an unmanaged global memory block
                hHTML = Marshal.StringToHGlobalUni(html);

                // get a stream interface to the unmanged memory block
                int result = Ole32Storage.CreateStreamOnHGlobal(hHTML, 0, out stream);
                if (result != HRESULT.S_OK)
                    throw new COMException("Unexpected failure to create html stream", result);

                // initialize the document using IPersistStreamInit
                IPersistStreamInit persistStreamInit = (IPersistStreamInit)htmlDocument;
                persistStreamInit.InitNew();
                persistStreamInit.Load(stream);
                persistStreamInit = null;
            }
            finally
            {
                // release the stream
                if (stream != null)
                    Marshal.ReleaseComObject(stream);

                // release the global memory
                if (hHTML != IntPtr.Zero)
                    Marshal.FreeHGlobal(hHTML);
            }
        }

        /// <summary>
        /// Dispose the MSHTML control
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // dispose our resources
            if (disposing)
            {
                ReleaseMSHTML();

                _eventCounter.AssertAllEventsAreUnhooked();
            }

            // allow base to dispose
            base.Dispose(disposing);
        }

        #endregion

        #region Propreties

        /// <summary>
        /// Window handle for the underlying MSHTML window
        /// </summary>
        public IntPtr MshtmlWindowHandle
        {
            get
            {
                if (hWnd == IntPtr.Zero)
                {
                    IOleWindow window = (IOleWindow)htmlDocument;
                    window.GetWindow(out hWnd);
                }
                return hWnd;
            }
        }
        private IntPtr hWnd = IntPtr.Zero;

        /// <summary>
        /// Indicates whether design mode is on
        /// </summary>
        public bool EditMode
        {
            get
            {
                return _editMode;
            }
        }
        bool _editMode = false;

        /// <summary>
        /// Get or set the HTML text contents of the control
        /// </summary>
        public string HtmlText
        {
            get
            {
                return htmlDocument.body.innerHTML;
            }
        }

        /// <summary>
        /// Get the DOM for the contents of the control
        /// </summary>
        public IHTMLDocument2 HTMLDocument
        {
            get
            {
                return htmlDocument as IHTMLDocument2;
            }
        }

        /// <summary>
        /// Checks whether the document has completely loaded
        /// </summary>
        public bool DocumentIsComplete
        {
            get
            {
                return htmlDocument.readyState == "complete";
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
                return standardCommandSet;
            }
        }

        /// <summary>
        /// Get the events
        /// </summary>
        public IMshtmlDocumentEvents DocumentEvents
        {
            get
            {
                return documentEventRepeater;
            }
        }

        /// <summary>
        /// Get a reference to the IHTMLEditServices interface used to add
        /// edit designers and access other advanced editing functionality
        /// </summary>
        public IHTMLEditServicesRaw HTMLEditServices
        {
            get
            {
                if (htmlEditServices == null)
                {
                    // get the IHTMLEditServices
                    htmlEditServices = (IHTMLEditServicesRaw)ComHelper.QueryService(
                        htmlDocument, EDIT_SERVICES_SID, typeof(IHTMLEditServicesRaw).GUID);
                }
                return htmlEditServices;
            }
        }

        public IOleUndoManager OleUndoManager
        {
            get
            {
                if (oleUndoManager == null)
                {
                    oleUndoManager = (IOleUndoManager)ComHelper.QueryService(
                        htmlDocument, typeof(IOleUndoManager).GUID, typeof(IOleUndoManager).GUID);
                }
                return oleUndoManager;
            }
        }

        /// <summary>
        /// Selection services for document (must wait until document IsComplete to get this)
        /// </summary>
        public ISelectionServicesRaw SelectionServices
        {
            get
            {
                if (selectionServices == null)
                {
                    HTMLEditServices.GetSelectionServices(
                        MarkupContainer as IMarkupContainerRaw, out selectionServices);
                }
                return selectionServices;
            }
        }

        /// <summary>
        /// Markup services for document (must wait until document IsComplete to get this)
        /// </summary>
        public MshtmlMarkupServices MarkupServices
        {
            get
            {
                if (markupServices == null)
                {
                    markupServices = new MshtmlMarkupServices(htmlDocument as IMarkupServicesRaw);
                }
                return markupServices;
            }
        }

        public IMarkupServicesRaw MarkupServicesRaw
        {
            get
            {
                if (markupServicesRaw == null)
                {
                    markupServicesRaw = htmlDocument as IMarkupServicesRaw;
                }
                return markupServicesRaw;
            }
        }

        /// <summary>
        /// Display services for document (must wait until document IsComplete to get this)
        /// </summary>
        public IDisplayServicesRaw DisplayServices
        {
            get
            {
                if (displayServices == null)
                {
                    displayServices = htmlDocument as IDisplayServicesRaw;
                }
                return displayServices;
            }
        }

        /// <summary>
        /// Markup container for document (must wait until document IsComplete to get this)
        /// </summary>
        public IMarkupContainer2Raw MarkupContainer
        {
            get
            {
                if (markupContainer == null)
                {
                    markupContainer = htmlDocument as IMarkupContainer2Raw;
                }
                return markupContainer;
            }
        }

        /// <summary>
        /// Hightlight rendering services for document ((must wait until document IsComplete to get this)
        /// </summary>
        public IHighlightRenderingServicesRaw HighlightRenderingServices
        {
            get
            {
                if (highlightRenderingServices == null)
                {
                    highlightRenderingServices = (IHighlightRenderingServicesRaw)htmlDocument;
                }
                return highlightRenderingServices;
            }
        }

        public bool HasContiguousSelection
        {
            get
            {
                try
                {
                    // get selection and text range
                    IHTMLSelectionObject selection = this.HTMLDocument.selection;
                    // protect against null (not expected but you never know)
                    if (selection == null || selection.type == null)
                    {
                        Debug.Fail("Unexpected null value for selection type");
                        return false;
                    }

                    // check selection type
                    string type = selection.type.ToUpperInvariant();
                    switch (type)
                    {
                        case "NONE":
                            return false;

                        case "TEXT":
                            // protect against MSHTML incorrectly reporting a text selection
                            // BEP: but, returning false prevents double click from working...
                            IHTMLTxtRange textRange = (IHTMLTxtRange)selection.createRange();
                            if (textRange.text == null)
                                return true;
                            //return false ;
                            else
                                return true;

                        case "CONTROL":
                            return true;

                        default:
                            Debug.Fail(String.Format(CultureInfo.InvariantCulture, "Unexpected selection type: {0}", type));
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    // yikes! how could this happen
                    Debug.Fail("Unexpected exception occurred while determining selection type: " + ex.Message);

                    // default to return no selection in relase mode
                    return false;
                }
            }
        }

        public bool ProtectFocus { get; set; }
        #endregion

        #region Events

        /// <summary>
        /// Help requested via the F1 key
        /// </summary>
        public event EventHandler HelpRequest
        {
            add
            {
                HelpRequestEventHandler += value;
                _eventCounter.EventHooked(HelpRequestEventHandler);
            }
            remove
            {
                HelpRequestEventHandler -= value;
                _eventCounter.EventUnhooked(HelpRequestEventHandler);
            }
        }
        private event EventHandler HelpRequestEventHandler;

        internal void FireHelpRequest()
        {
            if (HelpRequestEventHandler != null)
                HelpRequestEventHandler(this, EventArgs.Empty);
        }

        public event EventHandler DLControlFlagsChanged
        {
            add
            {
                DLControlFlagsChangedEventHandler += value;
                _eventCounter.EventHooked(DLControlFlagsChangedEventHandler);
            }
            remove
            {
                DLControlFlagsChangedEventHandler -= value;
                _eventCounter.EventUnhooked(DLControlFlagsChangedEventHandler);
            }
        }
        private event EventHandler DLControlFlagsChangedEventHandler;

        private void OnDLControlFlagsChanged(EventArgs e)
        {
            if (DLControlFlagsChangedEventHandler != null)
                DLControlFlagsChangedEventHandler(this, e);
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Execute an MSHTML command
        /// </summary>
        /// <param name="cmdID">command-d</param>
        public void ExecuteCommand(uint cmdID)
        {
            ExecuteCommand(cmdID, null);
        }

        /// <summary>
        /// Execute an MSHTML command
        /// </summary>
        /// <param name="cmdID">command id</param>
        /// <param name="input">input parameter</param>
        public void ExecuteCommand(uint cmdID, object input)
        {
            object output = null;
            ExecuteCommand(cmdID, input, ref output);
        }

        /// <summary>
        /// Execute an MSHTML command
        /// </summary>
        /// <param name="cmdID">command id</param>
        /// <param name="input">input parameter</param>
        /// <param name="output">out parameter</param>
        public void ExecuteCommand(uint cmdID, object input, ref object output)
        {
            IMshtmlCommand command = Commands[cmdID] as IMshtmlCommand;
            if (command != null)
            {
                command.Execute(input, ref output);
            }
            else
            {
                Debug.Fail("Invalid Mshtml command id");
            }
        }

        #endregion

        #region Helpers for Initialization and Release of MSHTML

        /// <summary>
        /// Initialize MSHTML by creating a new MSHTML instance, running it, and
        /// in-place activating it. The call to IOleObject.DoVerb will result in
        /// a call to our IOleDocumentSite.ActivateMe method where we will conclude
        /// the initialization sequence.
        ///
        /// This method is based on the CFramerDocument::CreateNewDocObject method
        /// from the FramerEx sample (referenced above in the class comment).
        /// Differences in our implementation include the fact that we don't use
        /// IPersistStorage or IPersistFile to load  the contents of the document,
        /// we don't call IOleObject.SetHostNames, and we don't call IOleObject.Advise
        /// to set up an IAdviseSink (not necessary since we don't need notifications
        /// of changes to the data or view).
        /// </summary>
        private void InitializeMSHTML()
        {
            // Create the html document
            htmlDocument = (IHTMLDocument2)new HTMLDocumentClass();

            // Query for its IOleObject interface (core OLE embedded object interface)
            oleObject = (IOleObject)htmlDocument;

            // Put the compound document object into the running state
            int result = Ole32.OleRun(oleObject);
            if (result != HRESULT.S_OK)
                Marshal.ThrowExceptionForHR(result);

            // Inform the embedded object of its "client-site" (i.e. container)
            oleObject.SetClientSite((IOleClientSite)(this));

            // Lock the OleObject into it's running state (we will unlock it during Dispose)
            result = Ole32.OleLockRunning(oleObject, true, false);
            if (result != HRESULT.S_OK)
                Marshal.ThrowExceptionForHR(result);

            // Get the boundaries of the container
            RECT containerRect;
            GetContainerRect(out containerRect);

            // In-Place Activate the MSHTML ActiveDocument (will result in a call to
            // IOleDocumentSite.ActivateMe where initialization will continue)
            result = oleObject.DoVerb(
                OLEIVERB.INPLACEACTIVATE,	// request in-place activation
                IntPtr.Zero,				// MSG for event that invoked the verb (none)
                (IOleClientSite)(this),		// client site for activation
                0,							// reserved (must be 0)
                this.Handle,				// parent-window for active object
                ref containerRect);		// bounding rectangle in parent window

            // check for DoVerb error
            if (result != HRESULT.S_OK)
                Marshal.ThrowExceptionForHR(result);

            // hookup to document events
            documentEventRepeater = new HtmlDocumentEventRepeater(htmlDocument);

            // If FindForm() is null, this editor is not hosted in a
            // managed window, thus, we must subclass the window to wait for
            // control key press to push through to PreProcessMessage
            // that would normally come through the managed message pump
            if (FindForm() == null)
            {
                NativeWindow nw = new NativeWindow();
                nw.AssignHandle(MshtmlWindowHandle);
                windowSubClasser = new WindowSubClasser(nw, new WndProcDelegate(WndProcDelegate));
                windowSubClasser.Install();
            }
        }

        public IntPtr WndProcDelegate(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            Message msg = new Message();
            msg.LParam = lParam;
            msg.WParam = wParam;
            msg.Msg = (int)uMsg;
            msg.HWnd = hWnd;
            bool handled = PreProcessMessage(ref msg);

            if (msg.Result != IntPtr.Zero || handled)
                return msg.Result;

            return windowSubClasser.CallBaseWindowProc(hWnd, uMsg, wParam, lParam);
        }

        /// <summary>
        /// Asks a document site to activate the document making the call as a
        /// document object rather than an in-place-active object and, optionally,
        /// specifies which view of the object document to activate. This is based
        /// on the CFramerDocument::ActivateMe method from the FramerEx sample
        /// (referenced above in the class comment). The only difference is that
        /// we don't call IOleDocumentView.SetRect after the call to UIActivate
        /// since we don't allow additional of toolbars, etc. to the container.
        /// </summary>
        /// <param name="pViewToActivate"> Pointer to the document view to be used
        /// in activating the document object. Can be NULL according to the
        /// specification, however MSHTML should always pass a valid view when
        /// requesting activation.</param>
        void IOleDocumentSite.ActivateMe(IOleDocumentView pViewToActivate)
        {
            // log access to method
            LOG("IOleDocumentSite", "ActivateMe");

            // MSHTML should always pass us a view
            if (pViewToActivate == null)
            {
                Debug.Fail("MSHMTL should always pass us a view!");
                ComHelper.Return(HRESULT.E_FAILED);
            }

            // Get a reference to the document's view
            oleDocumentView = pViewToActivate;

            // Associate our site with the document view
            pViewToActivate.SetInPlaceSite((IOleInPlaceSite)(this));

            // Get the command target for the view
            oleCommandTarget = (IOleCommandTargetWithExecParams)oleDocumentView;

            // hookup extra-feature accessor to the command target
            standardCommandSet = new MshtmlCoreCommandSet(oleCommandTarget);

            // UI Activate the document view
            oleDocumentView.UIActivate(true);

            // Show the view!
            oleDocumentView.Show(true);
        }

        /// <summary>
        /// Helper method to close / release MSHTML. This method is based on the
        /// CFramerDocument::CloseDocWindow method from the FramerEx sample
        /// (referenced above in the class comment). Differences include the fact
        /// that we don't call IOleObject.Unadvise (since we don't call Advise
        /// during initialization) and we don't release storage or moniker interfaces
        /// because we don't make use of them in the first place.
        /// </summary>
        private void ReleaseMSHTML()
        {
            if (htmlDocument != null)
            {
                if (windowSubClasser != null)
                    windowSubClasser.Remove();

                // Detach from document events
                documentEventRepeater.Detach();

                // Release the IOleCommandTarget that we got from the IOleDocumentView
                if (oleCommandTarget != null)
                    Marshal.ReleaseComObject(oleCommandTarget);

                // Deactivate, hide, close, detach, and release the the IOleDocumentView
                // that was passed to us in ActivateMe
                if (oleDocumentView != null)
                {
                    oleDocumentView.UIActivate(false);
                    oleDocumentView.Show(false);
                    oleDocumentView.Close(0);
                    oleDocumentView.SetInPlaceSite(null);
                    Marshal.ReleaseComObject(oleDocumentView);
                }

                // Close, detach, and release the IOleObject that we got from the HTMLDocumentClass
                if (oleObject != null)
                {
                    oleObject.Close(OLECLOSE.NOSAVE);
                    oleObject.SetClientSite(null);
                    Ole32.OleLockRunning(oleObject, false, false);
                    Marshal.ReleaseComObject(oleObject);
                }

                // Release the core HTMLDocumentClass
                if (htmlDocument != null)
                    Marshal.ReleaseComObject(htmlDocument);

                // Sever any remaining connections to our site. Note that we probabaly
                // don't need to call this since it designed to sever out-of-process
                // connections to our site -- typically the way we would get these is
                // if MSHTML embedded an OLE object implemented as an out-of-process
                // server and this out-of-process then got a reference to our
                // IOleClientSite. I can't see how this would happen but if it ever
                // does it is nice to know that we will cleanly disconnect!
                Ole32.CoDisconnectObject((IOleClientSite)this, 0);
            }
        }

        #endregion

        #region Download Control Ambient Property

        /// <summary>
        /// Respond to the AMBIENT_DLCONTROL disp-id by returning our download-control flags
        /// </summary>
        /// <returns></returns>
        [DispId(MSHTML_DISPID.AMBIENT_DLCONTROL)]
        public int AmbientDLControl
        {
            get
            {
                return dlctlFlags;
            }
        }

        #endregion

        #region IOleInPlaceSite Members

        /// <summary>
        /// Get the HWND of the in place site
        /// </summary>
        /// <param name="phwnd">out parameter for HWND</param>
        void IOleInPlaceSite.GetWindow(out IntPtr phwnd)
        {
            // log access to method
            LOG("IOleInPlaceSite", "GetWindow");

            // provide our window-handle
            phwnd = this.Handle;
        }

        /// <summary>
        /// Request to enter context sensitive help mode (not implemented)
        /// </summary>
        /// <param name="fEnterMode">enter or exit mode</param>
        void IOleInPlaceSite.ContextSensitiveHelp(bool fEnterMode)
        {
            // log access to method
            LOG_UN("IOleInPlaceSite", "ContextSensitiveHelp");

            // not-implemented
            ComHelper.Return(HRESULT.E_NOTIMPL);
        }

        /// <summary>
        /// Determines whether or not the container can activate the object in place.
        /// </summary>
        /// <returns>S_OK to indicate activation is allowed</returns>
        public int CanInPlaceActivate()
        {
            // log access to method
            LOG("IOleInPlaceSite", "CanInPlaceActivate");

            // in-place activation supported
            return HRESULT.S_OK;
        }

        /// <summary>
        /// Notifies us that the object is in place activated. At this point
        /// we can query for the IOleInPlaceObject interface that is normally
        /// used to manage activation / deactivation of in-place objects. Since
        /// we will only ever have 1 in-place active object we don't bother
        /// saving this pointer.
        /// </summary>
        public void OnInPlaceActivate()
        {
            // log access to method
            LOG("IOleInPlaceSite", "OnInPlaceActivate");

            // no-op, see above comment
        }

        /// <summary>
        /// Notifies the container that the object is about to be activated in
        /// place and that the object is going to replace the container's main
        /// menu with an in-place composite menu. Since MSHTML won't actually
        /// do any of this we don't need this notification to 'get ready'
        /// for UI activation so we do nothing
        /// </summary>
        public void OnUIActivate()
        {
            // log access to method
            LOG("IOleInPlaceSite", "OnUIActivate");

            // no-op, see above comment
        }

        /// <summary>
        /// Enables the in-place object to retrieve the window interfaces that form
        /// the window object hierarchy, and the position in the parent window where
        /// the object's in-place activation window should be placed.
        /// </summary>
        /// <param name="ppFrame">IOleInPlaceFrame</param>
        /// <param name="ppDoc">IOleInPlaceUIWindow</param>
        /// <param name="lprcPosRect">Rectangle to position object within</param>
        /// <param name="lprcClipRect">Clipping rectangle for object</param>
        /// <param name="lpFrameInfo">Pointer to an OLEINPLACEFRAMEINFO structure the
        /// container is to fill in with appropriate data</param>
        public void GetWindowContext(out IOleInPlaceFrame ppFrame, out IOleInPlaceUIWindow ppDoc,
            out RECT lprcPosRect, out RECT lprcClipRect, ref OLEINPLACEFRAMEINFO lpFrameInfo)
        {
            // log access to method
            LOG("IOleInPlaceSite", "GetWindowContext");

            // provide pointers to our frame and in-place UI window (the user-control
            // provides the implemenation of both)
            ppFrame = (IOleInPlaceFrame)this;
            ppDoc = (IOleInPlaceUIWindow)this;

            // Get our window-rect
            RECT containerRect;
            GetContainerRect(out containerRect);

            // Set position within the container (allow it to fill the container)
            lprcPosRect = containerRect;

            // Set clipping rectangle (allow painting in entire container)
            lprcClipRect = containerRect;

            // Set frame info (no accelerator table provided, we don't need to
            // since MSHTML is an in-process server we will get first crack at
            // all acclerators).
            lpFrameInfo.cb = (uint)Marshal.SizeOf(typeof(OLEINPLACEFRAMEINFO));
            lpFrameInfo.fMDIApp = false;
            lpFrameInfo.hwndFrame = this.Handle;
            lpFrameInfo.haccel = IntPtr.Zero;
            lpFrameInfo.cAccelEntries = 0;
        }

        /// <summary>
        /// Requests that the container scroll to get the object in view (will never
        /// be called for a Document Object / MSHTML since by definition document
        /// object's take up the entire container window)
        /// </summary>
        /// <param name="scrollExtant"></param>
        void IOleInPlaceSite.Scroll(SIZE scrollExtant)
        {
            // log access to method
            LOG_UN("IOleInPlaceSite", "Scroll");

            // not-implemented
            ComHelper.Return(HRESULT.E_NOTIMPL);
        }

        /// <summary>
        /// Notification that the object is deactivating. Will occur during our
        /// implementation of Dispose.
        /// </summary>
        /// <param name="fUndoable"></param>
        public void OnUIDeactivate(bool fUndoable)
        {
            // log access to method
            LOG("IOleInPlaceSite", "OnUIDeactivate");

            // take no special action
        }

        /// <summary>
        /// Notifies the container that the object is no longer active in place.
        /// Will occur during our implmentation of Dispose.
        /// </summary>
        public void OnInPlaceDeactivate()
        {
            // log access to method
            LOG("IOleInPlaceSite", "OnInPlaceDeactivate");

            // take no special action
        }

        /// <summary>
        /// DiscardUndoState -- No-op since undo notifications not supported by MSHTML.
        /// </summary>
        public void DiscardUndoState()
        {
            // log access to method
            LOG_UN("IOleInPlaceSite", "DiscardUndoState");

            // no-op (undo not supported)
        }

        /// <summary>
        /// DeactivateAndUndo -- No-op since undo notifications not supported by MSHTML.
        /// </summary>
        public void DeactivateAndUndo()
        {
            // log access to method
            LOG_UN("IOleInPlaceSite", "DeactivateAndUndo");

            // no-op (undo not supported)
        }

        /// <summary>
        /// Not used by document objects (thier RECT always occupies the whole window)
        /// </summary>
        /// <param name="lprcPosRect"></param>
        public void OnPosRectChange(ref RECT lprcPosRect)
        {
            // log access to method
            LOG_UN("IOleInPlaceSite", "OnPosRectChanged");

            // not-implemented
            ComHelper.Return(HRESULT.E_NOTIMPL);
        }

        #endregion

        #region IOleInPlaceFrame Members

        /// <summary>
        /// Get the HWND of the frame
        /// </summary>
        /// <param name="phwnd">out parameter for HWND</param>
        public void GetWindow(out IntPtr phwnd)
        {
            // log access to method
            LOG("IOleInPlaceFrame", "GetWindow");

            // provide our window handle
            phwnd = this.Handle;
        }

        /// <summary>
        /// Request to enter context sensitive help mode( not implemented)
        /// </summary>
        /// <param name="fEnterMode">enter or exit mode</param>
        public void ContextSensitiveHelp(bool fEnterMode)
        {
            // log access to method
            LOG_UN("IOleInPlaceFrame", "ContextSensitiveHelp");

            // not-implemented
            ComHelper.Return(HRESULT.E_NOTIMPL);
        }

        /// <summary>
        /// Returns a RECT structure in which the object can put toolbars and similar
        /// controls while active in place (we don't support toolbars so we return
        /// the appropriate error code)
        /// </summary>
        /// <param name="lprectBorder">border rect</param>
        /// <returns>E_NOTOOLSPACE to indicate toolbars are not supported</returns>
        public int GetBorder(out RECT lprectBorder)
        {
            // log access to method
            LOG_UN("IOleInPlaceFrame", "GetBorder");

            // set RECT to empty
            lprectBorder.left = 0;
            lprectBorder.top = 0;
            lprectBorder.right = 0;
            lprectBorder.bottom = 0;

            // return no toolspace
            return HRESULT.E_NOTOOLSPACE;
        }

        /// <summary>
        /// Determines if there is available space for tools to be installed around the
        /// object's window frame while the object is active in place. We don't support
        /// toolbars so we return the appropriate error code.
        /// </summary>
        /// <param name="pborderwidths">requested space for toolbars</param>
        /// <returns>E_NOTOOLSPACE to indicate toolbars are not supported</returns>
        public int RequestBorderSpace(ref RECT pborderwidths)
        {
            // log access to method
            LOG_UN("IOleInPlaceFrame", "RequestBorderSpace");

            // return no toolspace
            return HRESULT.E_NOTOOLSPACE;
        }

        /// <summary>
        /// Allocates space for the border requested in the call to IOleInPlaceUIWindow
        /// RequestBorderSpace. No-op since we don't support toolbars.
        /// </summary>
        /// <param name="pborderwidths">border space to use for tools</param>
        //public void SetBorderSpace(ref RECT pborderwidths)
        public void SetBorderSpace(ref RECT pborderwidths)
        {
            // log access to method
            LOG("IOleInPlaceFrame", "SetBorderSpace");

            // no-op (see above, toolbars not supported)
            // (MSHTML calls this but passes NULL to indicate no border space)
        }

        /// <summary>
        /// In-place object notification that it is going active or inactive (opportunity for
        /// us to save a reference to the IOleInPlaceActiveObject and/or release references
        /// to previously active objects
        /// </summary>
        /// <param name="pActiveObject">in-place object becoming active</param>
        /// <param name="pszObjName">display name of object becoming active</param>
        public void SetActiveObject(IOleInPlaceActiveObject pActiveObject, string pszObjName)
        {
            // log access to method
            string args = pActiveObject != null ? "object" : "null";
            LOG("IOleInPlaceFrame", "SetActiveObject(" + args + ")");

            // set the new object only if it is truely new
            if (oleInPlaceActiveObject != pActiveObject)
            {
                // release existing object if necessary
                if (oleInPlaceActiveObject != null)
                    Marshal.ReleaseComObject(oleInPlaceActiveObject);

                // set new object
                oleInPlaceActiveObject = pActiveObject;
            }
        }

        /// <summary>
        /// Allows the container to insert its menu groups into the composite menu to be used
        /// during the in-place session (not supported, return E_NOTIMPL)
        /// </summary>
        /// <param name="hmenuShared"></param>
        /// <param name="lpMenuWidths"></param>
        public void InsertMenus(IntPtr hmenuShared, ref OLEMENUGROUPWIDTHS lpMenuWidths)
        {
            // log access to method
            LOG_UN("IOleInPlaceFrame", "InsertMenus");

            // reutrn not-implemented
            ComHelper.Return(HRESULT.E_NOTIMPL);
        }

        /// <summary>
        /// Installs the composite menu in the window frame containing the object being
        /// activated in place (not supported, return E_NOTIMPL).
        /// </summary>
        /// <param name="hmenuShared"></param>
        /// <param name="holemenu"></param>
        /// <param name="hwndActiveObject"></param>
        public void SetMenu(IntPtr hmenuShared, IntPtr holemenu, IntPtr hwndActiveObject)
        {
            // log access to method
            LOG_UN("IOleInPlaceFrame", "SetMenu");

            // return not-implemented
            ComHelper.Return(HRESULT.E_NOTIMPL);
        }

        /// <summary>
        /// Gives the container a chance to remove its menu elements from the in-place
        /// composite menu. (not supported, return E_NOTIMPL).
        /// </summary>
        /// <param name="hmenuShared"></param>
        public void RemoveMenus(IntPtr hmenuShared)
        {
            // log access to method
            LOG_UN("IOleInPlaceFrame", "RemoveMenus");

            // return not-implemented
            ComHelper.Return(HRESULT.E_NOTIMPL);
        }

        /// <summary>
        /// Sets and displays status text about the in-place object in the container's
        /// frame window status line.
        /// </summary>
        /// <param name="pszStatusText"></param>
        public void SetStatusText(string pszStatusText)
        {
            // NOTE: don't log access to this method because it is called so
            // so frequently that it generates excessive 'noise' in the log output

            // Currently we ignore this notification since our use of MSHTML
            // is to embed it as a UI component (rather than as the entire UI)
            // In this case there is no reason to disaply it's status text.
            // In the future we could expose a StatusText property and StatusTextChanged
            // event if users of the control wanted access to this.
        }

        /// <summary>
        /// Notifies us that the object is about to display modal UI so that we can
        /// disable any modeless UI we have. We ignore this notification (see
        /// comment below for reasons why)
        /// </summary>
        public void EnableModeless(bool fEnable)
        {
            // log access to method
            LOG("IOleInPlaceFrame", "EnableModeless(" + fEnable.ToString() + ")");

            // Currently we ignore this notification because we always host
            // MSHTML in-process (i.e. we share a message pump). As a result
            // it's modal UI effectively precludes access to any of our
            // modeless UI -- so there isn't really anything to do in
            // response to this message
        }

        /// <summary>
        /// Translates accelerator keystrokes intended for the container's frame
        /// while an object is active in place. Since MSHTML is an in-process
        /// object this method should never be called.
        /// </summary>
        public int TranslateAccelerator(ref MSG lpmsg, ushort wID)
        {
            // log access to method
            LOG_UN("IOleInPlaceFrame", "TranslateAccelerator");

            // didn't handle accelerator
            return HRESULT.S_FALSE;
        }

        #endregion

        #region IOleClientSite Members

        /// <summary>
        /// Request by the embedded object to be saved (results from the user
        /// choosing the 'save' menu within the embedded object editing session).
        /// We ignore this because we manage all user-interaction related to saving.
        /// </summary>
        public void SaveObject()
        {
            // log access to method
            LOG_UN("IOleClientSite", "SaveObject");

            // no-op (see above, MSHTML does not initiate save requests)
        }

        /// <summary>
        /// Returns a moniker to an object's client site. Used for OLE linking
        /// so we don't support it since MSHTML will not initiate any OLE linking
        /// </summary>
        public int GetMoniker(OLEGETMONIKER dwAssign, OLEWHICHMK dwWhichMoniker, out IMoniker ppmk)
        {
            // log access to method
            LOG_UN("IOleClientSite", "GetMoniker");

            ppmk = null;
            return HRESULT.E_NOTIMPL;
        }

        /// <summary>
        /// Returns a pointer to the container's IOleContainer interface. Used
        /// primarily for OLE linking so we don't support it since MSHTML will
        /// not initiate any OLE linking.
        /// </summary>
        public int GetContainer(out IOleContainer ppContainer)
        {
            // log access to method
            LOG("IOleClientSite", "GetContainer");

            ppContainer = null;
            return HRESULT.E_NOINTERFACE;
        }

        /// <summary>
        /// Request to make the embedded object's presentation area visible
        /// within the compound document. Since we are hosting MSHTML in a
        /// UserControl it is always visible so we ignore this request
        /// </summary>
        public void ShowObject()
        {
            // log access to method
            LOG_UN("IOleClientSite", "ShowObject");

            // no-op, see above
        }

        /// <summary>
        /// Notifies us that an embedded object's UI has become visible. Normally
        /// this applies to opening embedded object's in their own application --
        /// this notification is then used to draw a hatched/shaded look around
        /// the embedded object. Again, this doesn't apply to our use of MSHTML
        /// so we ignore it.
        /// </summary>
        public void OnShowWindow(bool fShow)
        {
            // log access to method
            LOG_UN("IOleClientSite", "OnShowWindow");

            // no-op, see above
        }

        /// <summary>
        /// Not used in OLE Documents (used in OLE Controls). Not implemented.
        /// </summary>
        /// <returns></returns>
        public int RequestNewObjectLayout()
        {
            // log access to method
            LOG_UN("IOleClientSite", "RequestNewObjectLayout");

            return HRESULT.E_NOTIMPL;
        }

        #endregion

        #region IDocHostUIHandler Members

        ///////////////////////////////////////////////////////////////////////////
        /// Delegate to embedded IDocHostUIHandler implementation
        ///

        public void GetOptionKeyPath(out IntPtr pchKey, uint dwReserved)
        {
            // log access to method
            LOG("IDocHostUIHandler", "GetOptionKeyPath");

            docHostUIHandler.GetOptionKeyPath(out pchKey, dwReserved);
        }

        public void GetOverrideKeyPath(out IntPtr pchKey, uint dwReserved)
        {
            // log access to method
            LOG("IDocHostUIHandler2", "GetOverrideKeyPath");

            docHostUIHandler.GetOverrideKeyPath(out pchKey, dwReserved);
        }

        public int TranslateAccelerator(ref MSG lpMsg, ref Guid pguidCmdGroup, uint nCmdID)
        {
            // log access to method
            LOG("IDocHostUIHandler", "TranslateAccelerator");

            return docHostUIHandler.TranslateAccelerator(ref lpMsg, ref pguidCmdGroup, nCmdID);
        }

        public int FilterDataObject(IOleDataObject pDO, out IOleDataObject ppDORet)
        {
            // log access to method
            LOG("IDocHostUIHandler", "FilterDataObject");

            return docHostUIHandler.FilterDataObject(pDO, out ppDORet);
        }

        public void OnFrameWindowActivate(bool fActivate)
        {
            // log access to method
            LOG("IDocHostUIHandler", "OnFrameWindowActivate");

            docHostUIHandler.OnFrameWindowActivate(fActivate);
        }

        public void UpdateUI()
        {
            // log access to method
            LOG("IDocHostUIHandler", "UpdateUI");

            //Fix bug 1957: notify the DocumentEvents object that the focus may have changed
            //This internal hook is used to provide IE6-compatible focus events on IE 5.5.
            (DocumentEvents as HtmlDocumentEventRepeater).NotifyDocumentDisplayChanged();

            //notify listeners that the UI has changed
            docHostUIHandler.UpdateUI();
        }

        public int ShowContextMenu(int dwID, ref POINT ppt, object pcmdtReserved, object pdispReserved)
        {
            // log access to method
            LOG("IDocHostUIHandler", "ShowContextMenu");

            return docHostUIHandler.ShowContextMenu(dwID, ref ppt, pcmdtReserved, pdispReserved);
        }

        public int TranslateUrl(uint dwReserved, IntPtr pchURLIn, out IntPtr ppchURLOut)
        {
            // log access to method
            LOG("IDocHostUIHandler", "TranslateUrl");

            return docHostUIHandler.TranslateUrl(dwReserved, pchURLIn, out ppchURLOut);
        }

        public int ShowUI(DOCHOSTUITYPE dwID, IOleInPlaceActiveObject pActiveObject, IOleCommandTarget pCommandTarget, IOleInPlaceFrame pFrame, IOleInPlaceUIWindow pDoc)
        {
            // log access to method
            LOG("IDocHostUIHandler", "ShowUI");

            return docHostUIHandler.ShowUI(dwID, pActiveObject, pCommandTarget, pFrame, pDoc);
        }

        public void GetExternal(out IntPtr ppDispatch)
        {
            // log access to method
            LOG("IDocHostUIHandler", "GetExternal");

            docHostUIHandler.GetExternal(out ppDispatch);
        }

        public void ResizeBorder(ref RECT prcBorder, IOleInPlaceUIWindow pUIWindow, bool frameWindow)
        {
            // log access to method
            LOG("IDocHostUIHandler", "ResizeBorder");

            docHostUIHandler.ResizeBorder(ref prcBorder, pUIWindow, frameWindow);
        }

        public int GetDropTarget(OpenLiveWriter.Interop.Com.IDropTarget pDropTarget, out OpenLiveWriter.Interop.Com.IDropTarget ppDropTarget)
        {
            // log access to method
            LOG("IDocHostUIHandler", "GetDropTarget");

            return docHostUIHandler.GetDropTarget(pDropTarget, out ppDropTarget);
        }

        public void GetHostInfo(ref DOCHOSTUIINFO pInfo)
        {
            // log access to method
            LOG("IDocHostUIHandler", "GetHostInfo");

            docHostUIHandler.GetHostInfo(ref pInfo);
        }

        public void HideUI()
        {
            // log access to method
            LOG("IDocHostUIHandler", "HideUI");

            docHostUIHandler.HideUI();
        }

        public void OnDocWindowActivate(bool fActivate)
        {
            // log access to method
            LOG("IDocHostUIHandler", "OnDocWindowActivate");

            docHostUIHandler.OnDocWindowActivate(fActivate);
        }

        #endregion

        #region IDocHostShowUI Members

        ///////////////////////////////////////////////////////////////////////////
        /// Delegate to embedded IDocHostShowUI implementation
        ///

        public int ShowHelp(IntPtr hwnd, string lpstrHelpFile, uint uCommand, uint dwData, POINT ptMouse, IntPtr pDispatchObjectHit)
        {
            // log access to method
            LOG("IDocHostShowUI", "ShowHelp");

            // we handle help directly via the F1 keybaord hook so supress native help
            return HRESULT.S_OK;
        }

        public int ShowMessage(IntPtr hwnd, string lpstrText, string lpstrCaption, uint dwType, string lpstrHelpFile, uint dwHelpContext, out int plResult)
        {
            // log access to method
            LOG("IDocHostShowUI", "ShowMessage");

            return showUIHandler.ShowMessage(hwnd, lpstrText, lpstrCaption, dwType, lpstrHelpFile, dwHelpContext, out plResult);
        }

        #endregion

        #region IServiceProviderRaw Members

        int IServiceProviderRaw.QueryService(ref Guid guid, ref Guid riid, out IntPtr service)
        {
            // default to no service
            service = IntPtr.Zero;

            // If IProtectFocus is being queried, we will handle it ourselves, pass ourselves
            Guid SID_SProtectFocus = new Guid(0xd81f90a3, 0x8156, 0x44f7, 0xad, 0x28, 0x5a, 0xbb, 0x87, 0x00, 0x32, 0x74);
            if (guid == SID_SProtectFocus && riid == typeof(IProtectFocus).GUID)
            {
                service = Marshal.GetComInterfaceForObject(this, typeof(IProtectFocus));
                return HRESULT.S_OK;
            }

            if (_serviceProvider != null)
                return _serviceProvider.QueryService(ref guid, ref riid, out service);
            else
                return HRESULT.E_NOINTERFACE;
        }

        #endregion

        #region Control Overrides / Event Handlers

        /// <summary>
        /// Pre-process messages to allow MSHTML to translate/handle accelerator keys
        /// </summary>
        /// <param name="msg">message to pre-process</param>
        /// <returns>true if the message was handled, else false</returns>
        public override bool PreProcessMessage(ref Message msg)
        {
            MSG message = new MSG();
            message.hwnd = msg.HWnd;
            message.message = (uint)msg.Msg;
            message.wParam = (uint)msg.WParam.ToInt32();
            message.lParam = msg.LParam.ToInt32();
            message.time = 0;
            message.pt = new POINT();

            // see if MSHTML wants to handle the input
            // @SharedCanvas - if shortcut keys dont work, first look here
            if (oleInPlaceActiveObject != null && oleInPlaceActiveObject.TranslateAccelerator(ref message) == HRESULT.S_OK)
            {
                LOG("IOleInPlaceActiveObject", "TranslateAccelerator");
                return true;
            }
            // not handled, do default processing
            else
            {
                return base.PreProcessMessage(ref msg);
            }
        }

        /// <summary>
        /// Override IsInputKey to allow MSHTML to get access to the enter key
        /// </summary>
        /// <param name="keyData">key data (key + modifier)</param>
        /// <returns>true if it is an input key, else false</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            // if the enter key is pressed and we are in design mode
            // then indicate that we want the control to handle the key
            if (keyData == Keys.Enter && EditMode == false)
                return true;
            else
            // default base processing
            {
                return base.IsInputKey(keyData);
            }

        }

        /// <summary>
        /// Handle OnGotFocus so that when the parent UserControl receives
        /// input focus via the keyboard we can forward the focus on to the
        /// embedded MSHTML ActiveDocument
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(EventArgs e)
        {
            // log access to method
            LOG("MshtmlControl", "OnGotFocus");

            // call base
            base.OnGotFocus(e);

            // if we've got an in-place active object then forward the focus to it
            if (oleInPlaceActiveObject != null)
            {
                IntPtr hWndActiveObject;

                // if we are exiting the application sometimes this method
                // returns a COM error, in this case punt
                try { oleInPlaceActiveObject.GetWindow(out hWndActiveObject); }
                catch (COMException) { return; }

                User32.SetFocus(hWndActiveObject);
            }
        }

        /// <summary>
        /// Handle OnEnter so that we can notify the in-place active object
        /// that its container ("DocWindow") is becoming active
        /// </summary>
        /// <param name="e"></param>
        protected override void OnEnter(EventArgs e)
        {
            // log access to method
            LOG("MshtmlControl", "OnEnter");

            // call base
            base.OnEnter(e);

            // notify in-place active object
            if (oleInPlaceActiveObject != null)
            {
                oleInPlaceActiveObject.OnFrameWindowActivate(true);
                oleInPlaceActiveObject.OnDocWindowActivate(true);
            }
        }

        /// Handle OnLeave so that we can notify the in-place active object
        /// that its container ("DocWindow") is becoming inactive
        protected override void OnLeave(EventArgs e)
        {
            // log access to method
            LOG("MshtmlControl", "OnLeave");

            // call base
            base.OnLeave(e);

            // notify in-place active object
            if (oleInPlaceActiveObject != null)
            {
                oleInPlaceActiveObject.OnDocWindowActivate(false);
                oleInPlaceActiveObject.OnFrameWindowActivate(false);
            }
        }

        /// <summary>
        /// Handle OnSizeChanged by updating the OleDocumentView
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            // call base
            base.OnSizeChanged(e);

            // update the RECT of the view if our size changes
            if (oleDocumentView != null)
            {
                RECT containerRect;
                GetContainerRect(out containerRect);
                try
                {
                    oleDocumentView.SetRect(ref containerRect);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Unexpected exception occurred setting ole document view RECT: " + ex.ToString());
                }
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Helper method to get the rectangle that defines the container
        /// </summary>
        /// <param name="containerRect">out parameter for returning the container rect</param>
        private void GetContainerRect(out RECT containerRect)
        {
            containerRect = new RECT();
            containerRect.left = 0;
            containerRect.top = 0;
            containerRect.right = this.Width;
            containerRect.bottom = this.Height;
        }

        #endregion

        #region Debug / Logging Utility Methods

        /// <summary>
        /// Log access to an interface method
        /// </summary>
        /// <param name="iface">name of interface</param>
        /// <param name="method">name of method</param>
        [Conditional("DEBUG")]
        private static void LOG(string iface, string method)
        {
            //Debug.WriteLine( String.Format( "{0}.{1}", iface, method ) ) ;
        }

        /// <summary>
        /// Log an unexpected access to an interface method (during active
        /// development this will assert to notify us of a new use of our
        /// interface implementations)
        /// </summary>
        /// <param name="iface">name of interface</param>
        /// <param name="method">name of method</param>
        [Conditional("DEBUG")]
        private static void LOG_UN(string iface, string method)
        {
            Debug.Fail(
                String.Format(CultureInfo.InvariantCulture, "Unexpected call to {0}.{1}", iface, method));
            LOG(iface, method);
        }

        #endregion

        #region Private member variables

        /// <summary>
        /// Core HTML document (created during initialization)
        /// </summary>
        private IHTMLDocument2 htmlDocument = null;

        /// <summary>
        /// Core embedding interface (queried from hmtlDocument)
        /// </summary>
        private IOleObject oleObject = null;

        /// <summary>
        /// View of the embedded document (passed to us in ActivateMe)
        /// </summary>
        private IOleDocumentView oleDocumentView = null;

        /// <summary>
        /// Command target for view (quieried from oleDocumentView)
        /// </summary>
        private IOleCommandTargetWithExecParams oleCommandTarget = null;

        /// <summary>
        /// Standard MSHTML command set
        /// </summary>
        private MshtmlCoreCommandSet standardCommandSet = null;

        /// <summary>
        /// Interface to activated object (passed to us in SetActiveObject)
        /// </summary>
        private IOleInPlaceActiveObject oleInPlaceActiveObject = null;

        /// <summary>
        /// Service provider that we delegate to
        /// </summary>
        private IServiceProviderRaw _serviceProvider;

        /// <summary>
        /// Advisory connection for sinking to HTML document events
        /// </summary>
        private HtmlDocumentEventRepeater documentEventRepeater = new HtmlDocumentEventRepeater();

        /// <summary>
        /// Optional IDocHostUIHandler that we delegate to for UI customizations
        /// of MSHTML. Set using the constructor.
        /// </summary>
        private IDocHostUIHandler2 docHostUIHandler = new IDocHostUIHandlerBaseImpl();

        /// <summary>
        /// Optional IDocHostShowUI that we delegate to for UI customizations
        /// of MSHTML. Set using the constructor.
        /// </summary>
        private IDocHostShowUI showUIHandler = new IDocHostShowUIBaseImpl();

        /// <summary>
        /// Download control flags (from DLCTL enumeration)
        /// </summary>
        private int dlctlFlags = 0;

        /// <summary>
        /// Reference to the IHTMLEditServices interface used to add
        /// edit designers and access other advanced editing functionality
        /// </summary>
        private IHTMLEditServicesRaw htmlEditServices = null;

        /// <summary>
        /// Referenced to the interface used for advanced interaction
        /// with the undo system
        /// </summary>
        private IOleUndoManager oleUndoManager = null;

        /// <summary>
        /// Reference to IMarkupContainer interface for the document
        /// </summary>
        private IMarkupContainer2Raw markupContainer;

        /// <summary>
        /// Reference to ISelectionServices for the document
        /// </summary>
        private ISelectionServicesRaw selectionServices;

        /// <summary>
        /// Reference to IHightlightRenderingServices for the document
        /// </summary>
        private IHighlightRenderingServicesRaw highlightRenderingServices;

        /// <summary>
        /// Reference to IMarkupServices for the document
        /// </summary>
        private MshtmlMarkupServices markupServices;

        /// <summary>
        /// Reference to IMarkupServicesRaw for the document
        /// </summary>
        private IMarkupServicesRaw markupServicesRaw;

        /// <summary>
        /// Reference to IDisplayServices for the document
        /// </summary>
        private IDisplayServicesRaw displayServices;

        private WindowSubClasser windowSubClasser;

        #endregion

        #region Private Static Constants

        // edit services SID
        private static readonly Guid EDIT_SERVICES_SID = new Guid(0x3050f7f9, 0x98b5, 0x11cf, 0xbb, 0x82, 0x00, 0xaa, 0x00, 0xbd, 0xce, 0x0b);

        #endregion

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            for (int i = 0; i < prgCmds.Length; i++)
            {
                // These command IDs don't ever get passed in to
                // QueryStatus, as far as I can see, but better safe than sorry.
                if (prgCmds[i].cmdID == OLECMDID.SHOWSCRIPTERROR || prgCmds[i].cmdID == OLECMDID.SHOWMESSAGE)
                    prgCmds[i].cmdf = OLECMDF.SUPPORTED;
            }

            return HRESULT.S_OK;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, OLECMDEXECOPT nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // Supress window.alert and script errors
            if (nCmdID == OLECMDID.SHOWSCRIPTERROR || nCmdID == OLECMDID.SHOWMESSAGE)
                return HRESULT.S_OK;

            return HRESULT.E_NOTIMPL;
        }

        #region IProtectFocus Members

        public bool AllowFocusChange()
        {
            // Called by mshtml when it wants to change/steal focus
            // Allow focus to change if we are not interested in protecting the focus
            return (ProtectFocus == false);
        }

        #endregion
    }
}
