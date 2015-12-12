// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.SHDocVw;
using OpenLiveWriter.Interop.Windows;
using OLECMDEXECOPT = OpenLiveWriter.Interop.SHDocVw.OLECMDEXECOPT;
using OLECMDF = OpenLiveWriter.Interop.SHDocVw.OLECMDF;
using OLECMDID = OpenLiveWriter.Interop.SHDocVw.OLECMDID;

namespace OpenLiveWriter.BrowserControl
{
    /// <summary>
    /// Internet Explorer browser control
    /// </summary>
    public class ExplorerBrowserControl : UserControl, IBrowserControl
    {
        /// <summary>
        /// Intialize BrowserControl
        /// </summary>
        public ExplorerBrowserControl()
        {
            // Initialize the browser and add it to our client area
            InitializeBrowser();

            // Add the standard commands to our command table
            AddCommands();

            // Subscribe to events we are interested in
            SubscribeToEvents();

            // Update the UI state of the browser commands
            UpdateCommandState();
        }

        public int DownloadOptions
        {
            get
            {
                return BrowserControlClientSite.DownloadOptions;
            }
            set
            {
                BrowserControlClientSite.DownloadOptions = value;
            }
        }

        public NetworkCredential NetworkCredential
        {
            set
            {
                BrowserControlClientSite.NetworkCredential = value;
            }
        }

        public void SetCookies(string url, string cookies)
        {
            WinInet.InternetSetCookies(url, null, cookies);
        }

        private ExplorerBrowserControlClientSite BrowserControlClientSite
        {
            get
            {
                if (_browserControlClientSite == null)
                    _browserControlClientSite = new ExplorerBrowserControlClientSite(this.Browser);
                return _browserControlClientSite;
            }
        }
        private ExplorerBrowserControlClientSite _browserControlClientSite;

        /// <summary>
        /// Get a reference to the underlying web browser
        /// </summary>
        public IWebBrowser2 Browser
        {
            get
            {
                return (IWebBrowser2)m_browser.GetOcx();
            }
        }

        /// <summary>
        /// Retrieves the automation object of the active document, if any. When the
        /// active document is an HTML page, this property provides access to the
        /// IDispatch interface pointer to the HTMLDocument coclass. Callers can
        /// retrieve the IHTMLDocument2 interface (or other HTMLDocument interfaces),
        /// by casting to them from the object received from this property.
        /// When other document types are active (e.g. a Word document) this property
        /// returns the default IDispatch interface pointer for the document (e.g.
        /// the Document object in the Word object model). This property should only
        /// be accessed after the DocumentComplete event is fired.
        /// </summary>
        public object Document
        {
            get { return m_browser.Document; }
        }

        /// <summary>
        /// The name of the resource that Microsoft® Internet Explorer is currently
        /// displaying. If the resource is an HTML page on the World Wide Web, the
        /// name is the title of that page. If the resource is a folder or file on
        /// the network or local computer, the name is the full path of the folder
        /// or file in Universal Naming Convention (UNC) format.
        /// </summary>
        public string LocationName
        {
            get { return m_browser.LocationName; }
        }

        /// <summary>
        /// Retrieves the URL of the resource that Microsoft® Internet Explorer is
        /// currently displaying. If the resource is a folder or file on the
        /// network or local computer, the name is the full path of the folder or
        /// file in the Universal Naming Convention (UNC) format.
        /// </summary>
        public string LocationURL
        {
            get { return m_browser.LocationURL; }
        }

        /// <summary>
        /// Title of current document (normally use for window caption display).
        /// You should retreive/update this value whenever the TitleChanged
        /// event is fired.
        /// </summary>
        public string Title
        {
            get { return m_title; }
        }
        private string m_title;

        /// <summary>
        /// StatusText (normally displayed in status bar). You should retreive/update
        /// this value whenever the StatusTextChanged event is fired.
        /// </summary>
        public string StatusText
        {
            get { return m_statusText; }
        }
        private string m_statusText;

        /// <summary>
        /// Property indicating the encryption level of the currently displayed document.
        /// The EncryptionLevelChanged event will be fired whenever this property
        /// changes valiue.
        /// </summary>
        public EncryptionLevel EncryptionLevel
        {
            get { return m_encryptionLevel; }
        }
        private EncryptionLevel m_encryptionLevel;

        /// <summary>
        /// Size of text displayed by the browser control. Before usin g this
        /// property you should query the TextSizeSupported property to make
        /// sure that the currently displayed document supports TextSize.
        /// </summary>
        public TextSize TextSize
        {
            get
            {
                // verify that we are being called correctly
                if (TextSizeSupported)
                {
                    // setup input parameters
                    object input = null;
                    object output = null;

                    // query for the current text size
                    m_browser.ExecWB(
                        OLECMDID.OLECMDID_ZOOM,
                        OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER,
                        ref input,
                        ref output);

                    // return it
                    return (TextSize)output;
                }
                else // default behavior when TextSize not supported
                     // (we don't Assert here because that hoses the
                     // designer)
                {
                    return TextSize.Medium;
                }
            }
            set
            {
                // verify that we are being called correctly
                if (TextSizeSupported)
                {
                    // perepare Execute paramters
                    object input = value;
                    object output = null;

                    // change the TextSize
                    m_browser.ExecWB(
                        OLECMDID.OLECMDID_ZOOM,
                        OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT,
                        ref input, ref output);
                }
                else  // (we don't Assert here because that hoses the designer)
                {
                }
            }
        }

        /// <summary>
        /// Check whether the TextSize property is supported by the current
        /// document (used to determine whether to enable/disable a menu
        /// or other UI used to set TextSize)
        /// </summary>
        public bool TextSizeSupported
        {
            get
            {
                // check whether TextSize is supported by the current document
                return (m_browser.QueryStatusWB(OLECMDID.OLECMDID_ZOOM)
                    & OLECMDF.OLECMDF_SUPPORTED) > 0;
            }
        }

        /// <summary>
        /// Returns the InternetSecurityZone assign to the currently loaded document
        /// </summary>
        /// <returns></returns>
        protected InternetSecurityZone GetSecurityZone()
        {
            IOleCommandTargetWithExecParams target = (IOleCommandTargetWithExecParams)Document;
            object input = null;
            object output = null;
            target.Exec(MSHTML, GETFRAMEZONE, OpenLiveWriter.Interop.Com.OLECMDEXECOPT.DODEFAULT, ref input, ref output);

            UInt32 zoneInt = (UInt32)output;
            InternetSecurityZone zone = (InternetSecurityZone)zoneInt;
            return zone;
        }
        protected enum InternetSecurityZone { LocalMachine = 0, LocalIntranet = 1, TrustedSites = 2, Internet = 3, RestrictedSites = 4 };
        private static readonly Guid MSHTML = new Guid("DE4BA900-59CA-11CF-9592-444553540000");
        private const uint GETFRAMEZONE = 6037;

        /// <summary>
        /// Indicates whether the object is engaged in a navigation or downloading
        /// operation. If the control is busy, you can use the BrowserCommand.Stop
        /// to cancel the navigation or download operation before it is completed.
        /// </summary>
        public bool Busy
        {
            get { return m_browser.Busy; }
        }

        /// <summary>
        /// Sets or retrieves a value that indicates whether the object can show dialog boxes.
        /// </summary>
        public bool Silent
        {
            get { return m_browser.Silent; }
            set { m_browser.Silent = value; }
        }

        /// <summary>
        /// Global offline state ('Work Offline' menu in Internet Explorer)
        /// </summary>
        public bool WorkOffline
        {
            get { return WinInet.WorkOffline; }
            set { WinInet.WorkOffline = value; }
        }

        /// <summary>
        /// Navigate to the specified URL
        /// </summary>
        /// <param name="url">URL to navigate to</param>
        public void Navigate(string url)
        {
            Navigate(url, false);
        }

        public void Navigate(string url, bool newWindow)
        {
            Navigate(url, newWindow, null);
        }

        public void Post(string url, byte[] postData)
        {
            Navigate(url, false, string.Empty, postData);
        }

        /// <summary>
        /// Navigate to the specified URL using the specified options
        /// </summary>
        /// <param name="url">url to navigate to</param>
        /// <param name="newWindow">navigate using new top level window</param>
        public void Navigate(string url, bool newWindow, string headers)
        {
            Navigate(url, newWindow, headers, null);
        }

        public void Navigate(string url, bool newWindow, string headers, byte[] postData)
        {
            object objFlags;
            if (newWindow)
                objFlags = BrowserNavConstants.navOpenInNewWindow;
            else
                objFlags = m; // missing parameter

            object objPostData;
            if (postData != null)
                objPostData = postData;
            else
                objPostData = m; // missing parameter

            object headersObj = headers;

            m_browser.Navigate(url, ref objFlags, ref m, ref objPostData, ref headersObj);
        }

        /// <summary>
        /// Determine if a command is enabled
        /// </summary>
        /// <param name="command">unique ID of command</param>
        /// <returns>true if the command is enabled, otherwise false</returns>
        public bool IsEnabled(BrowserCommand command)
        {
            // verify that the command exists
            Debug.Assert(m_commands.Contains(command),
                "Attempted to QueryStatus on a command that doesn't exist");

            // query status
            IBrowserCommand cmdStatus = (IBrowserCommand)m_commands[command];
            if (cmdStatus != null)
                return cmdStatus.Enabled;
            else
                return false;
        }

        /// <summary>
        /// Execute a command
        /// </summary>
        /// <param name="command">unique ID of command</param>
        public void Execute(BrowserCommand command)
        {
            // verify that the command exists
            Debug.Assert(m_commands.Contains(command),
                "Attempted to Execute a command that doesn't exist");

            // execute
            IBrowserCommand cmdExecute = (IBrowserCommand)m_commands[command];
            if (cmdExecute != null)
                cmdExecute.Execute();
        }

        /// <summary>
        /// Force refresh of the state of the browser commands
        /// </summary>
        public void UpdateCommandState()
        {
            m_browser.ExecWB(
                OLECMDID.OLECMDID_UPDATECOMMANDS,
                OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER,
                ref m, ref m);
        }

        public event BrowserNavigateComplete2EventHandler NavigateComplete2;

        protected virtual void OnNavigateComplete2(BrowserNavigateComplete2EventArgs e)
        {
            if (NavigateComplete2 != null)
                NavigateComplete2(this, e);
        }

        public event BrowserBeforeNavigate2EventHandler BeforeNavigate2Document;

        protected virtual void OnBeforeNavigate2Document(BrowserBeforeNavigate2EventArgs e)
        {
            if (BeforeNavigate2Document != null)
                BeforeNavigate2Document(this, e);
        }

        public event BrowserBeforeNavigate2EventHandler BeforeNavigate2Frame;

        protected virtual void OnBeforeNavigate2Frame(BrowserBeforeNavigate2EventArgs e)
        {
            if (BeforeNavigate2Frame != null)
                BeforeNavigate2Frame(this, e);
        }

        public event BrowserNavigateErrorEventHandler NavigateError;

        protected virtual void OnNavigateError(BrowserNavigateErrorEventArgs e)
        {
            if (NavigateError != null)
                NavigateError(this, e);
        }

        /// <summary>
        /// Event that fires when a new document is navigated to and has completed
        /// downloading. For standard pages, this event will fire when the
        /// page has been downloaded. For framesets, this event will fire when
        /// the entire frameset has been downloaded (FrameComplete will be fired
        /// for each individual frame as they are downloaded).
        /// </summary>
        public event BrowserDocumentEventHandler DocumentComplete;

        /// <summary>
        /// Raises the DocumentComplete event.
        /// </summary>
        /// <param name="e">An event args that contains the event data.</param>
        protected virtual void OnDocumentComplete(BrowserDocumentEventArgs e)
        {
            //loading docs in LocalMachine security zone is generally a no-no since it is either much more
            //restrictive, or much less restrictive than the Internet security zone.  This assertion is
            //in here to help catch bugs where we accidentally load code in the LocalMachine zone.  If it
            //turns out there is a real need to support this, we can remove it.
            //Debug.Assert(GetSecurityZone() != InternetSecurityZone.LocalMachine, "Warning!, document was loaded in the LocalMachine security zone");

            if (DocumentComplete != null)
                DocumentComplete(this, e);
        }

        /// <summary>
        /// Event that fires when a frame within a frameset has completed downloading
        /// </summary>
        public event BrowserDocumentEventHandler FrameComplete;

        /// <summary>
        /// Raises the FrameComplete event.
        /// </summary>
        /// <param name="e">An event args that contains the event data.</param>
        protected virtual void OnFrameComplete(BrowserDocumentEventArgs e)
        {
            if (FrameComplete != null)
                FrameComplete(this, e);
        }

        /// <summary>
        /// Event that fires when a download operation commences. Applications should
        /// use this event to update their visual 'busy' indicator.
        /// </summary>
        public event EventHandler DownloadBegin;

        /// <summary>
        /// Raises the DownloadBegin event.
        /// </summary>
        /// <param name="e">An event args that contains the event data.</param>
        protected virtual void OnDownloadBegin(EventArgs e)
        {
            if (DownloadBegin != null)
                DownloadBegin(this, e);
        }

        /// <summary>
        /// Event that fires when a download operation is completed. Applications shoudl
        /// use this event to update thier visual 'busy' indicator.
        /// </summary>
        public event EventHandler DownloadComplete;

        /// <summary>
        /// Raises the DownloadComplete event.
        /// </summary>
        /// <param name="e">An event args that contains the event data.</param>
        protected virtual void OnDownloadComplete(EventArgs e)
        {
            if (DownloadComplete != null)
                DownloadComplete(this, e);
        }

        /// <summary>
        /// Event that fires when document download progress changes. Applications
        /// should use this event to update progress bars or other visual indicators
        /// of download progress.
        /// </summary>
        public event BrowserProgressChangeEventHandler ProgressChange;

        /// <summary>
        /// Raises the ProgressChange event.
        /// </summary>
        /// <param name="e">An event args that contains the event data.</param>
        protected virtual void OnProgressChange(BrowserProgressChangeEventArgs e)
        {
            if (ProgressChange != null)
                ProgressChange(this, e);
        }

        /// <summary>
        /// Event that fires when the Title property changes
        /// </summary>
        public event EventHandler TitleChanged;

        /// <summary>
        /// Raises the TitleChanged event.
        /// </summary>
        /// <param name="e">An event args that contains the event data.</param>
        protected virtual void OnTitleChanged(EventArgs e)
        {
            if (TitleChanged != null)
                TitleChanged(this, e);
        }

        /// <summary>
        /// Event the fires when the StatusText property changes
        /// </summary>
        public event EventHandler StatusTextChanged;

        /// <summary>
        /// Raises the StatusTextChanged event.
        /// </summary>
        /// <param name="e">An event args that contains the event data.</param>
        protected virtual void OnStatusTextChanged(EventArgs e)
        {
            if (StatusTextChanged != null)
                StatusTextChanged(this, e);
        }

        /// <summary>
        /// Event that fires when the EncryptionLevel property changes
        /// </summary>
        public event EventHandler EncryptionLevelChanged;

        /// <summary>
        /// Raises the EncryptionLevelChanged event.
        /// </summary>
        /// <param name="e">An event args that contains the event data.</param>
        protected virtual void OnEncryptionLevelChanged(EventArgs e)
        {
            if (EncryptionLevelChanged != null)
                EncryptionLevelChanged(this, e);
        }

        /// <summary>
        /// Event that fires when the state of the browser commands has changed.
        /// Applies generally to all command except for GoBack and GoForward which
        /// have special events defined for them (because they change so frequently).
        /// </summary>
        public event EventHandler CommandStateChanged;

        /// <summary>
        /// Raises the CommandStateChanged event.
        /// </summary>
        /// <param name="e">An event args that contains the event data.</param>
        protected virtual void OnCommandStateChanged(EventArgs e)
        {
            if (CommandStateChanged != null)
                CommandStateChanged(this, e);
        }

        /// <summary>
        /// Event that fires when a script or user action attempts to create
        /// a new browser window
        /// </summary>
        public event DWebBrowserEvents2_NewWindow2EventHandler NewWindow2;

        /// <summary>
        /// Raises the NewWindow2 event
        /// </summary>
        /// <param name="e">event args</param>
        protected virtual void OnNewWindow2(DWebBrowserEvents2_NewWindow2Event e)
        {
            if (NewWindow2 != null)
                NewWindow2(this, e);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose of the browser instance
                m_browser.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Helper function to initialize the browser and add it to the
        /// client area of the UserControl (it fills the entire area)
        /// </summary>
        private void InitializeBrowser()
        {
            // create browser instance
            m_browser = new AxWebBrowser();

            // begin initialization
            SuspendLayout();
            m_browser.BeginInit();

            // add browser to out client area
            m_browser.Enabled = true;
            m_browser.Dock = DockStyle.Fill;
            m_browser.TabIndex = 0;
            Controls.Add(m_browser);

            // end initialization
            m_browser.EndInit();
            ResumeLayout(false);
        }

        /// <summary>
        /// Add the standard command set to our command table
        /// </summary>
        private void AddCommands()
        {
            // save away references to selected DirectInvoke commands prior to adding them
            m_goBackCommand = new GoBackBrowserCommand(m_browser);
            m_goForwardCommand = new GoForwardBrowserCommand(m_browser);
            m_stopCommand = new StopBrowserCommand(m_browser);

            // standard file menu commands (figure out how to do New Window and Open)
            AddCommand(BrowserCommand.NewWindow, new NewWindowBrowserCommand(m_browser));
            AddCommand(BrowserCommand.SaveAs, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_SAVEAS));
            AddCommand(BrowserCommand.PageSetup, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_PAGESETUP));
            AddCommand(BrowserCommand.Print, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_PRINT));
            AddCommand(BrowserCommand.PrintPreview, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_PRINTPREVIEW));
            AddCommand(BrowserCommand.Properties, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_PROPERTIES));

            // standard edit menu commands
            AddCommand(BrowserCommand.Cut, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_CUT));
            AddCommand(BrowserCommand.Copy, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_COPY));
            AddCommand(BrowserCommand.Paste, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_PASTE));
            AddCommand(BrowserCommand.SelectAll, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_SELECTALL));
            AddCommand(BrowserCommand.Find, new PrivateBrowserCommand(m_browser, PrivateBrowserCommand.Find));

            // standard view menu commands
            AddCommand(BrowserCommand.GoBack, m_goBackCommand);
            AddCommand(BrowserCommand.GoForward, m_goForwardCommand);
            AddCommand(BrowserCommand.Stop, m_stopCommand);
            AddCommand(BrowserCommand.Refresh, new StandardBrowserCommand(m_browser, OLECMDID.OLECMDID_REFRESH));
            AddCommand(BrowserCommand.GoHome, new GoHomeBrowserCommand(m_browser));
            AddCommand(BrowserCommand.GoSearch, new GoSearchBrowserCommand(m_browser));
            AddCommand(BrowserCommand.ViewSource, new PrivateBrowserCommand(m_browser, PrivateBrowserCommand.ViewSource));
            AddCommand(BrowserCommand.Languages, new LanguagesBrowserCommand(m_browser));

            // standard favorites menu commands
            AddCommand(BrowserCommand.AddFavorite, new AddFavoriteBrowserCommand(m_browser));
            AddCommand(BrowserCommand.OrganizeFavorites, new OrganizeFavoritesBrowserCommand(m_browser));

            // standard tools menu commands
            AddCommand(BrowserCommand.InternetOptions, new PrivateBrowserCommand(m_browser, PrivateBrowserCommand.InternetOptions));
        }

        /// <summary>
        /// Add a command to the command table
        /// </summary>
        /// <param name="command">unique ID of command</param>
        /// <param name="command">command implementation</param>
        private void AddCommand(BrowserCommand commandID, IBrowserCommand command)
        {
            // verify we haven't already added this command
            Debug.Assert(!m_commands.Contains(commandID),
                "Added a command that is already part of the command table");

            // insert the command into the table
            m_commands[commandID] = command;
        }

        /// <summary>
        /// Remove a command from the command table
        /// </summary>
        /// <param name="command">unique ID of command</param>
        private void RemoveCommand(BrowserCommand commandID)
        {
            // verify we aren't trying to remove a command that doesn't exist
            Debug.Assert(m_commands.Contains(commandID),
                "Attempted to remove a command that doesn't exist");

            // remove the command from the table
            m_commands.Remove(commandID);
        }

        /// <summary>
        /// Helper function used to subscribe to events we are interested in
        /// </summary>
        private void SubscribeToEvents()
        {
            // hookup event handler for document status
            m_browser.DownloadBegin += new EventHandler(AxWebBrowser_DownloadBegin);
            m_browser.ProgressChange += new DWebBrowserEvents2_ProgressChangeEventHandler(AxWebBrowser_ProgressChange);
            m_browser.DownloadComplete += new EventHandler(AxWebBrowser_DownloadComplete);
            m_browser.DocumentComplete += new DWebBrowserEvents2_DocumentCompleteEventHandler(AxWebBrowser_DocumentComplete);

            // hookup event handlers for monitoring title and status bar changes
            m_browser.TitleChange += new DWebBrowserEvents2_TitleChangeEventHandler(AxWebBrowser_TitleChange);
            m_browser.StatusTextChange += new DWebBrowserEvents2_StatusTextChangeEventHandler(AxWebBrowser_StatusTextChange);
            m_browser.SetSecureLockIcon += new DWebBrowserEvents2_SetSecureLockIconEventHandler(AxWebBrowser_SetSecureLockIcon);

            // hookup event handler for monitoring command state changes
            m_browser.CommandStateChange += new DWebBrowserEvents2_CommandStateChangeEventHandler(AxWebBrowser_CommandStateChange);
            m_browser.NavigateComplete2 += new DWebBrowserEvents2_NavigateComplete2EventHandler(AxWebBrowser_NavigateComplete2);
            m_browser.BeforeNavigate2 += new DWebBrowserEvents2_BeforeNavigate2EventHandler(AxWebBrowser_BeforeNavigate2);
            m_browser.NavigateError += new DWebBrowserEvents2_NavigateErrorEventHandler(AxWebBrowser_NavigateError);

            // hookup event handler for monitoring new window creation
            m_browser.NewWindow2 += new DWebBrowserEvents2_NewWindow2EventHandler(AxWebBrowser_NewWindow2);
        }

        /// <summary>
        /// Forward DownloadBegin event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void AxWebBrowser_DownloadBegin(object sender, EventArgs e)
        {
            OnDownloadBegin(EventArgs.Empty);
        }

        /// <summary>
        /// Forward DownloadComplete event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void AxWebBrowser_DownloadComplete(object sender, EventArgs e)
        {
            OnDownloadComplete(EventArgs.Empty);
        }

        /// <summary>
        /// ProgressChange event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void AxWebBrowser_ProgressChange(object sender, DWebBrowserEvents2_ProgressChangeEvent e)
        {
            OnProgressChange(new BrowserProgressChangeEventArgs(e.progress, e.progressMax));
        }

        private void AxWebBrowser_NavigateComplete2(object sender, DWebBrowserEvents2_NavigateComplete2Event e)
        {
            BrowserNavigateComplete2EventArgs args = new BrowserNavigateComplete2EventArgs(e.pDisp, (string)e.uRL);
            OnNavigateComplete2(args);
        }

        private void AxWebBrowser_BeforeNavigate2(object sender, DWebBrowserEvents2_BeforeNavigate2Event e)
        {
            BrowserBeforeNavigate2EventArgs args = new BrowserBeforeNavigate2EventArgs(e);

            IntPtr thisPtr = Marshal.GetIDispatchForObject(e.pDisp);
            IntPtr browserPtr = Marshal.GetIDispatchForObject(m_browser.GetOcx());
            bool isDocument = (thisPtr == browserPtr);
            Marshal.Release(thisPtr);
            Marshal.Release(browserPtr);

            if (isDocument)
                OnBeforeNavigate2Document(args);
            else
                OnBeforeNavigate2Frame(args);

            args.PutBack(e);
        }

        private void AxWebBrowser_NavigateError(object sender, DWebBrowserEvents2_NavigateErrorEvent e)
        {
            BrowserNavigateErrorEventArgs args = new BrowserNavigateErrorEventArgs(e);
            OnNavigateError(args);
        }

        /// <summary>
        /// Event handler for DocumentComplete. We fire 2 events out of this event handler:
        ///		FrameComplete -- frame within a frameset completes
        ///		DocumentComplete -- entire document (page or frameset) completes
        ///	Both of these events provide the URL and the IDispatch of the document
        ///	object as part of their event arguments.
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event parameters</param>
        private void AxWebBrowser_DocumentComplete(
            object sender, DWebBrowserEvents2_DocumentCompleteEvent e)
        {
            // Create native event args based on the passed event args
            BrowserDocumentEventArgs evtArgs =
                new BrowserDocumentEventArgs(e.uRL.ToString(), e.pDisp);

            // Check to see if this is the full document (page or frameset). This
            // technique is based on the article "HOWTO: Determine When a Page Is
            // Done Loading in WebBrowser Control" at:
            //  http://support.microsoft.com/default.aspx?scid=KB;en-us;q180366
            IntPtr thisPtr = Marshal.GetIDispatchForObject(e.pDisp);
            IntPtr browserPtr = Marshal.GetIDispatchForObject(m_browser.GetOcx());
            bool isDocument = (thisPtr == browserPtr);
            Marshal.Release(thisPtr);
            Marshal.Release(browserPtr);

            if (isDocument)
            {
                // TODO: update address bar on DocumentComplete

                // fire to listeners
                OnDocumentComplete(evtArgs);
            }
            else // this was a frame within a frameset
            {
                // fire to listeners
                OnFrameComplete(evtArgs);
            }
        }

        /// <summary>
        /// Event handler for web browser title changes
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void AxWebBrowser_TitleChange(
            object sender, DWebBrowserEvents2_TitleChangeEvent e)
        {
            // update our title
            m_title = e.text;

            // notify listeners
            OnTitleChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Event handler for web browser status text changes
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void AxWebBrowser_StatusTextChange(
            object sender, DWebBrowserEvents2_StatusTextChangeEvent e)
        {
            // update the status text
            m_statusText = e.text;

            // notify listeners
            OnStatusTextChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Forward the SetSecureLockIcon event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void AxWebBrowser_SetSecureLockIcon(
            object sender, DWebBrowserEvents2_SetSecureLockIconEvent e)
        {
            // set value
            m_encryptionLevel = (EncryptionLevel)e.secureLockIcon;

            // fire event
            OnEncryptionLevelChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Event handler for CommandStateChange -- update our internal command states
        /// then re-fire this event to listeners.
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event parameters</param>
        private void AxWebBrowser_CommandStateChange(
            object sender, DWebBrowserEvents2_CommandStateChangeEvent e)
        {
            // constatns representing possible command state changes
            const int CSC_UPDATECOMMANDS = -1;
            const int CSC_NAVIGATEFORWARD = 1;
            const int CSC_NAVIGATEBACK = 2;

            // Back button state changed
            if (e.command == CSC_NAVIGATEBACK)
            {
                m_goBackCommand.SetEnabled(e.enable);
            }
            // Forward button state changed
            else if (e.command == CSC_NAVIGATEFORWARD)
            {
                m_goForwardCommand.SetEnabled(e.enable);
            }
            // General command state change
            else if (e.command == CSC_UPDATECOMMANDS)
            {
                // Do manual update on m_stopCommand
                m_stopCommand.SetEnabled(m_browser.Busy);

                // No update needed for m_refreshCommand or m_goHomeCommand
                // (both are always enabled)
            }

            // fire the CommandStateChanged event so listeners can update their UI
            OnCommandStateChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Handle the browser NewWindow2 event (forward on to listeners)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void AxWebBrowser_NewWindow2(object sender, DWebBrowserEvents2_NewWindow2Event e)
        {
            OnNewWindow2(e);
        }

        /// <summary>
        /// Web browser instance we contain
        /// </summary>
        private AxWebBrowser m_browser;

        /// <summary>
        /// Table of commands supported by the WebBrowser. Key = BrowserCommand (int),
        /// Value = BrowserCommand.
        /// </summary>
        private Hashtable m_commands = new Hashtable();

        // Instances of DirectInvokeBrowserCommand that we need references to so
        // we can update their status when the CommandStateChange event is fired
        private DirectInvokeBrowserCommand m_goBackCommand = null;
        private DirectInvokeBrowserCommand m_goForwardCommand = null;
        private DirectInvokeBrowserCommand m_stopCommand = null;

        /// <summary>
        /// Alias used to omit optional parameters ('missing' parameter)
        /// </summary>
        private object m = Type.Missing;
    }

    [ComVisible(true)]
    public class ExplorerBrowserControlClientSite : IOleClientSite, IServiceProviderRaw, IAuthenticate
    {
        public ExplorerBrowserControlClientSite(object browser)
        {
            // Set us as the client site
            //
            // It seems a bit dodgy to set ourselves as the client site since we don't
            // implement all the interfaces that may be exposed by the provided client site.  It
            // appears, however, that none of those interfaces are called once the browserControl
            // has been loaded, so this appears safe.  See the description in the links below for
            // additional discussion.
            //
            // http://www.codeproject.com/books/0764549146_8.asp
            // http://discuss.develop.com/archives/wa.exe?A2=ind0205A&L=DOTNET&D=0&P=15756
            IOleObject oleObject = (IOleObject)browser;
            oleObject.SetClientSite(this);
        }

        #region Download Control

        public int DownloadOptions
        {
            get { return _downloadOptions; }
            set { _downloadOptions = value; }
        }
        private int _downloadOptions;

        /// <summary>
        /// Respond to the AMBIENT_DLCONTROL disp-id by returning our download-control flags
        /// </summary>
        /// <returns></returns>
        [DispId(MSHTML_DISPID.AMBIENT_DLCONTROL)]
        public int AmbientDLControl()
        {
            return DownloadOptions;
        }

        #endregion

        #region Authentication

        public NetworkCredential NetworkCredential
        {
            set
            {
                _networkCredential = value;
            }
        }
        private NetworkCredential _networkCredential = null;

        int IServiceProviderRaw.QueryService(ref Guid guid, ref Guid riid, out IntPtr service)
        {
            // Authentication service
            Guid SID_SAuthenticate = typeof(IAuthenticate).GUID;
            if (guid == SID_SAuthenticate && riid == typeof(IAuthenticate).GUID)
            {
                if (_networkCredential != null)
                {
                    service = Marshal.GetComInterfaceForObject(this, typeof(IAuthenticate));
                    return HRESULT.S_OK;
                }
            }

            // default if no service found
            service = IntPtr.Zero;
            return HRESULT.E_NOINTERFACE;
        }

        int IAuthenticate.Authenticate(out IntPtr phwnd, out IntPtr pszUsername, out IntPtr pszPassword)
        {
            if (_networkCredential != null)
            {
                phwnd = IntPtr.Zero; // no ui
                pszUsername = Marshal.StringToCoTaskMemUni(_networkCredential.UserName);
                pszPassword = Marshal.StringToCoTaskMemUni(_networkCredential.Password);
                return HRESULT.S_OK;
            }
            else
            {
                phwnd = IntPtr.Zero;
                pszUsername = IntPtr.Zero;
                pszPassword = IntPtr.Zero;
                return HRESULT.E_ACCESSDENIED;
            }
        }

        #endregion

        #region IOleClientSite Members

        /// <summary>
        /// Saves the object associated with the client site.  This should not be called.
        /// </summary>
        void IOleClientSite.SaveObject()
        {
        }

        /// <summary>
        /// Returns a moniker to the object's client site.  Not implemented, but may be called.
        /// </summary>
        int IOleClientSite.GetMoniker(OLEGETMONIKER dwAssign, OLEWHICHMK dwWhichMoniker, out IMoniker ppmk)
        {
            ppmk = null;
            return HRESULT.E_NOTIMPL;
        }

        /// <summary>
        /// Returns the object's IOleContainer.  Indicate container doesn't support this interface.
        /// </summary>
        int IOleClientSite.GetContainer(out IOleContainer ppContainer)
        {
            ppContainer = null;
            return HRESULT.E_NOINTERFACE;
        }

        /// <summary>
        /// Notifies the object to make itself visible to the user.  Should not be called.
        /// </summary>
        void IOleClientSite.ShowObject()
        {
        }

        /// <summary>
        /// Notifies the object when the window should be shown or hidden.  Should not be called.
        /// </summary>
        void IOleClientSite.OnShowWindow(bool fShow)
        {
        }

        /// <summary>
        /// Asks container to allocate more or less space for displaying an embedded object.
        /// Should not be called.
        /// </summary>
        int IOleClientSite.RequestNewObjectLayout()
        {
            return HRESULT.E_NOTIMPL;
        }

        #endregion

    }

}
