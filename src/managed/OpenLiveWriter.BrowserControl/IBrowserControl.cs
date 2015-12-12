// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.BrowserControl
{
    // Generic interface to an embedded browser control
    public interface IBrowserControl
    {
        /// <summary>
        /// The name of the resource that Microsoft® Internet Explorer is currently
        /// displaying. If the resource is an HTML page on the World Wide Web, the
        /// name is the title of that page. If the resource is a folder or file on
        /// the network or local computer, the name is the full path of the folder
        /// or file in Universal Naming Convention (UNC) format.
        /// </summary>
        string LocationName { get; }

        /// <summary>
        /// Retrieves the URL of the resource that Microsoft® Internet Explorer is
        /// currently displaying. If the resource is a folder or file on the
        /// network or local computer, the name is the full path of the folder or
        /// file in the Universal Naming Convention (UNC) format.
        /// </summary>
        string LocationURL { get; }

        /// <summary>
        /// Title of current document (normally use for window caption display).
        /// You should retreive/update this value whenever the TitleChanged
        /// event is fired.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// StatusText (normally displayed in status bar). You should retreive/update
        /// this value whenever the StatusTextChanged event is fired.
        /// </summary>
        string StatusText { get; }

        /// <summary>
        /// Property indicating the encryption level of the currently displayed document.
        /// The EncryptionLevelChanged event will be fired whenever this property
        /// changes valiue.
        /// </summary>
        EncryptionLevel EncryptionLevel { get; }

        /// <summary>
        /// Size of text displayed by the browser control. Before usin g this
        /// property you should query the TextSizeSupported property to make
        /// sure that the currently displayed document supports TextSize.
        /// </summary>
        TextSize TextSize { get; set; }

        /// <summary>
        /// Check whether the TextSize property is supported by the current
        /// document (used to determine whether to enable/disable a menu
        /// or other UI used to set TextSize)
        /// </summary>
        bool TextSizeSupported { get; }

        /// <summary>
        /// Indicates whether the object is engaged in a navigation or downloading
        /// operation. If the control is busy, you can use the BrowserCommand.Stop
        /// to cancel the navigation or download operation before it is completed.
        /// </summary>
        bool Busy { get; }

        // Sets or retrieves a value that indicates whether the object can show dialog boxes.
        bool Silent { get; set; }

        /// <summary>
        /// Global offline state ('Work Offline' menu in Internet Explorer)
        /// </summary>
        bool WorkOffline { get; set; }

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
        object Document { get; }

        /// <summary>
        /// Navigate to the specified URL
        /// </summary>
        /// <param name="url">URL to navigate to</param>
        void Navigate(string url);

        /// <summary>
        /// Navigate to the specified URL using the specified options
        /// </summary>
        /// <param name="url">url to navigate to</param>
        /// <param name="newWindow">navigate using new top level window</param>
        void Navigate(string url, bool newWindow);

        /// <summary>
        /// Determine if a command is enabled
        /// </summary>
        /// <param name="command">unique ID of command</param>
        /// <returns>true if the command is enabled, otherwise false</returns>
        bool IsEnabled(BrowserCommand command);

        /// <summary>
        /// Execute a command
        /// </summary>
        /// <param name="command">unique ID of command</param>
        void Execute(BrowserCommand command);

        /// <summary>
        /// Force refresh of the state of the browser commands
        /// </summary>
        void UpdateCommandState();

        /// <summary>
        /// Fires after a navigation to a link is completed on either a window or
        /// frameSet element. The document might still be downloading (and in the
        /// case of HTML, images might still be downloading), but at least part
        /// of the document has been received from the server, and the viewer
        /// for the document has been created.
        ///
        /// In Internet Explorer 6 or later, the Navigate2 event fires only after
        /// the first navigation made in code. It does not fire when a user clicks
        /// a link on a Web page.
        /// </summary>
        event BrowserNavigateComplete2EventHandler NavigateComplete2;

        /// <summary>
        /// Event that fires when a new document is navigated to and has completed
        /// downloading. For standard pages, this event will fire when the
        /// page has been downloaded. For framesets, this event will fire when
        /// the entire frameset has been downloaded (FrameComplete will be fired
        /// for each individual frame as they are downloaded).
        /// </summary>
        event BrowserDocumentEventHandler DocumentComplete;

        /// <summary>
        /// Event that fires when a frame within a frameset has completed downloading
        /// </summary>
        event BrowserDocumentEventHandler FrameComplete;

        /// <summary>
        /// Event that fires when a download operation commences. Applications should
        /// use this event to update their visual 'busy' indicator.
        /// </summary>
        event EventHandler DownloadBegin;

        /// <summary>
        /// Event that fires when a download operation is completed. Applications shoudl
        /// use this event to update thier visual 'busy' indicator.
        /// </summary>
        event EventHandler DownloadComplete;

        /// <summary>
        /// Event that fires when document download progress changes. Applications
        /// should use this event to update progress bars or other visual indicators
        /// of download progress.
        /// </summary>
        event BrowserProgressChangeEventHandler ProgressChange;

        /// <summary>
        /// Event that fires when the Title property changes
        /// </summary>
        event EventHandler TitleChanged;

        /// <summary>
        /// Event the fires when the StatusText property changes
        /// </summary>
        event EventHandler StatusTextChanged;

        /// <summary>
        /// Event that fires when the EncryptionLevel property changes
        /// </summary>
        event EventHandler EncryptionLevelChanged;

        /// <summary>
        /// Event that fires when the state of the browser commands has changed.
        /// Applies generally to all command except for GoBack and GoForward which
        /// have special events defined for them (because they change so frequently).
        /// </summary>
        event EventHandler CommandStateChanged;
    }

    /// <summary>
    /// EncryptionLevel (maps to the IE SecureLockIconConstants enumeration)
    /// </summary>
    public enum EncryptionLevel : int
    {
        Unsecure = 0,
        Mixed = 1,
        UnknownBits = 2,
        FortyBit = 3,
        FiftySixBit = 4,
        Fortezza = 5,
        OneHundredTwentyEightBit = 6
    }

    /// <summary>
    /// Enumeration used with TextSize property
    /// </summary>
    public enum TextSize : int
    {
        Smallest = 0,
        Smaller = 1,
        Medium = 2,
        Larger = 3,
        Largest = 4
    }

    /// <summary>
    /// Event arguments for events that communicate state about browser documents
    /// </summary>
    public class BrowserDocumentEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize event args
        /// </summary>
        /// <param name="url">URL of browser document</param>
        /// <param name="document">IDispatch of browser document</param>
        internal BrowserDocumentEventArgs(string url, object document)
        {
            m_url = url;
            m_document = document;
        }

        /// <summary>
        /// URL of browser document
        /// </summary>
        public string URL
        {
            get { return m_url; }
        }
        private string m_url;

        /// <summary>
        /// IDispatch of browser document. To check whether this document implements
        /// a given interface (e.g. IHTMLDocument2 or Word.Document) simply query
        /// the Document using the 'is' operator.
        /// </summary>
        public object Document
        {
            get { return m_document; }
        }
        private object m_document;
    }

    public class BrowserNavigateErrorEventArgs : EventArgs
    {
        public object StatusCode
        {
            get { return _statusCode; }
        }
        private object _statusCode;

        public object Url
        {
            get { return _url; }
        }
        private object _url;

        internal BrowserNavigateErrorEventArgs(DWebBrowserEvents2_NavigateErrorEvent e)
        {
            _statusCode = e.statusCode;
            _url = e.uRL;
        }
    }

    public class BrowserBeforeNavigate2EventArgs : EventArgs
    {
        internal BrowserBeforeNavigate2EventArgs(DWebBrowserEvents2_BeforeNavigate2Event e)
        {
            this.pDisp = e.pDisp;
            this.uRL = e.uRL;
            this.flags = e.flags;
            this.targetFrameName = e.targetFrameName;
            this.postData = e.postData;
            this.headers = e.headers;
            this.cancel = e.cancel;
        }

        public void PutBack(DWebBrowserEvents2_BeforeNavigate2Event e)
        {
            e.pDisp = this.pDisp;
            e.uRL = this.uRL;
            e.flags = this.flags;
            e.targetFrameName = this.targetFrameName;
            e.postData = this.postData;
            e.headers = this.headers;
            e.cancel = this.cancel;
        }

        private object pDisp;
        public object PDisp
        {
            get { return pDisp; }
            set { pDisp = value; }
        }

        private object uRL;
        public object URL
        {
            get { return uRL; }
            set { uRL = value; }
        }

        private object flags;
        public object Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        private object targetFrameName;
        public object TargetFrameName
        {
            get { return targetFrameName; }
            set { targetFrameName = value; }
        }

        private object postData;
        public object PostData
        {
            get { return postData; }
            set { postData = value; }
        }

        private object headers;
        public object Headers
        {
            get { return headers; }
            set { headers = value; }
        }

        private bool cancel;
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }
    }

    /// <summary>
    /// Delegate used for communicating BrowserDocumentEvents
    /// </summary>
    public delegate void BrowserDocumentEventHandler(object sender, BrowserDocumentEventArgs e);

    public class BrowserNavigateComplete2EventArgs : EventArgs
    {
        internal BrowserNavigateComplete2EventArgs(object pDisp, string url)
        {
            m_pDisp = pDisp;
            m_url = url;
        }

        private object m_pDisp;
        public object PDisp
        {
            get { return m_pDisp; }
        }

        private string m_url;
        public string Url
        {
            get { return m_url; }
        }

    }

    public delegate void BrowserNavigateComplete2EventHandler(object sender, BrowserNavigateComplete2EventArgs e);

    public delegate void BrowserBeforeNavigate2EventHandler(object sender, BrowserBeforeNavigate2EventArgs e);

    public delegate void BrowserNavigateErrorEventHandler(object sender, BrowserNavigateErrorEventArgs e);

    /// <summary>
    /// Event arguments for ProgressChange event
    /// </summary>
    public class BrowserProgressChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize event args
        /// </summary>
        /// <param name="progress">progress (bytes)</param>
        /// <param name="progressMax">progress max (bytes)</param>
        internal BrowserProgressChangeEventArgs(long progress, long progressMax)
        {
            // copy values
            m_progress = progress;
            m_progressMax = progressMax;

            // calculate percent complete
            if (progress == -1 || progressMax == 0)
                m_percentComplete = 100;
            else
                m_percentComplete = (progress / progressMax) * 100;
        }

        /// <summary>
        /// Determine whether the operation is complete
        /// </summary>
        public bool Completed
        {
            get { return PercentComplete == 100; }
        }

        /// <summary>
        /// Percent of operation that is complete
        /// </summary>
        public long PercentComplete
        {
            get { return m_percentComplete; }
        }
        private long m_percentComplete;

        /// <summary>
        /// Total progress so far (normally in bytes). This value is valid
        /// only if Completed is false.
        /// </summary>
        public long Progress
        {
            get { return m_progress; }
        }
        private long m_progress;

        /// <summary>
        /// Maximum progress value (normally in bytes). This value is valid
        /// only if Completed is false.
        /// </summary>
        public long ProgressMax
        {
            get { return m_progressMax; }
        }
        private long m_progressMax;
    }

    /// <summary>
    /// Delegate used for ProgressChange events
    /// </summary>
    public delegate void BrowserProgressChangeEventHandler(object sender, BrowserProgressChangeEventArgs e);

}
