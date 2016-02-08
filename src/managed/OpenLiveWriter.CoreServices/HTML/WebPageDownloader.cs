// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics ;
using System.Windows.Forms;
using System.Runtime.InteropServices ;

using Project31.Interop.Com ;
using Project31.Interop.Com.SHDocVw ;
using Project31.Interop.Com.StructuredStorage ;
using Project31.Interop.Com.ActiveDocuments ;
using Project31.Interop.Windows ;
using Project31.BrowserControl ;

using mshtml ;

namespace Project31.CoreServices
{
    public class WebPageDownloader : IDisposable, IOleClientSite
    {
        /// <summary>
        /// Initialize the downloader
        /// </summary>
        public WebPageDownloader()
        {
            // create the undelrying control
            browserControl = new ExplorerBrowserControl() ;

            // set us as the client site
            // http://www.codeproject.com/books/0764549146_8.asp
            // http://discuss.develop.com/archives/wa.exe?A2=ind0205A&L=DOTNET&D=0&P=15756
            IOleObject oleObject = (IOleObject)browserControl.Browser ;
            oleObject.SetClientSite( this ) ;

            // configure options
            browserControl.Browser.Silent = true ;

            // subscribe to events
            browserControl.ProgressChange +=new BrowserProgressChangeEventHandler(browserControl_ProgressChange);
            browserControl.DocumentComplete +=new BrowserDocumentEventHandler(browserControl_DocumentComplete);
            browserControl.NewWindow2 +=new Project31.BrowserControl.DWebBrowserEvents2_NewWindow2EventHandler(browserControl_NewWindow2);
        }

        /// <summary>
        /// Download from the specified URL
        /// </summary>
        /// <param name="url"></param>
        public void DownloadFromUrl( string url )
        {
            browserControl.Navigate( url, false ) ;
        }


        /// <summary>
        /// Event which indicates that the download is complete
        /// </summary>
        public event EventHandler DownloadComplete ;

        /// <summary>
        /// Event which fires when total download progress is updated
        /// </summary>
        public event ProgressUpdatedEventHandler ProgressUpdated ;

        /// <summary>
        /// Get the underlying document that the control has downloaded (only
        /// available after DownloadComplete fires)
        /// </summary>
        public IHTMLDocument2 HTMLDocument
        {
            get { return (IHTMLDocument2)browserControl.Document; }

        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            if ( browserControl != null )
                browserControl.Dispose() ;
        }

        /// <summary>
        /// Respond to the AMBIENT_DLCONTROL disp-id by returning our download-control flags
        /// </summary>
        /// <returns></returns>
        [DispId(MSHTML_DISPID.AMBIENT_DLCONTROL)]
        public int AmbientDLControl()
        {
            return	DLCTL.DOWNLOADONLY | DLCTL.NO_CLIENTPULL |
                    DLCTL.NO_JAVA | DLCTL.NO_DLACTIVEXCTLS |
                    DLCTL.NO_RUNACTIVEXCTLS | DLCTL.SILENT ;
        }

        /// <summary>
        /// Handle document complete event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void browserControl_DocumentComplete(object sender, BrowserDocumentEventArgs e)
        {
            // verify ready-state complete
            Debug.Assert( browserControl.Browser.ReadyState == tagREADYSTATE.READYSTATE_COMPLETE ) ;

            // propagate event
            if ( DownloadComplete != null )
                DownloadComplete( this, EventArgs.Empty ) ;
        }

        /// <summary>
        /// Handle new window event (prevent all pop-up windows from displaying)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void browserControl_NewWindow2(object sender, DWebBrowserEvents2_NewWindow2Event e)
        {
            // prevent pop-ups!
            e.cancel = true ;
        }

        /// <summary>
        /// Handle progress changed event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void browserControl_ProgressChange(object sender, BrowserProgressChangeEventArgs e)
        {
            if ( ProgressUpdated != null )
            {
                ProgressUpdatedEventArgs ea = new ProgressUpdatedEventArgs(
                    Convert.ToInt32(e.Progress), Convert.ToInt32(e.ProgressMax), String.Empty ) ;
                ProgressUpdated( this, ea ) ;
            }
        }

        #region IOleClientSite Members

        void IOleClientSite.SaveObject()
        {
        }

        int IOleClientSite.GetMoniker(OLEGETMONIKER dwAssign, OLEWHICHMK dwWhichMoniker, out UCOMIMoniker ppmk)
        {
            ppmk = null;
            return HRESULT.E_NOTIMPL ;
        }

        int IOleClientSite.GetContainer(out IOleContainer ppContainer)
        {
            ppContainer = null ;
            return HRESULT.E_NOINTERFACE;
        }

        void IOleClientSite.ShowObject()
        {
        }

        void IOleClientSite.OnShowWindow(bool fShow)
        {
        }

        int IOleClientSite.RequestNewObjectLayout()
        {
            return HRESULT.E_NOTIMPL ;
        }

        #endregion

        /// <summary>
        /// Embedded web browser
        /// </summary>
        private ExplorerBrowserControl browserControl = null ;
    }

}
