// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.HtmlEditor
{
    public class EditorLinkNavigator : IDisposable
    {
        /// <summary>
        /// Link navigator used for navigating to links from within
        /// an editing session
        /// </summary>
        /// <param name="editorControl">presentation editor context</param>
        public EditorLinkNavigator(HtmlEditorControl editorControl, IMainFrameWindow editorFrame, IStatusBar statusBar, IMshtmlDocumentEvents events)
        {
            // save references
            _htmlEditorContext = editorControl as IHtmlEditorComponentContext;
            _htmlEditorControl = editorControl;
            _editorFrame = editorFrame;
            _statusBar = statusBar;
            _documentEvents = events;

            // sign up for editor events
            _editorFrame.Deactivate += new EventHandler(_editorFrame_Deactivate);
            _htmlEditorContext.PreHandleEvent += new HtmlEditDesignerEventHandler(presentationEditor_PreHandleEvent);
            _htmlEditorContext.TranslateAccelerator += new HtmlEditDesignerEventHandler(presentationEditor_TranslateAccelerator);
            _documentEvents.LostFocus += new EventHandler(_documentEvents_LostFocus);
        }

        public bool SuppressForImages
        {
            get { return _suppressForImages; }
            set { _suppressForImages = value; }
        }
        private bool _suppressForImages = true;

        public bool SuppressForLocalUrls
        {
            get { return _suppressForLocalUrls; }
            set { _suppressForLocalUrls = value; }
        }
        private bool _suppressForLocalUrls = true;

        public bool SuppressForNonEditableRegions
        {
            get { return _suppressForNonEditableRegions; }
            set { _suppressForNonEditableRegions = value; }
        }
        private bool _suppressForNonEditableRegions = true;

        /// <summary>
        /// Dispose embedded tooltip control
        /// </summary>
        public void Dispose()
        {
            // detach from events
            _documentEvents.LostFocus -= new EventHandler(_documentEvents_LostFocus);
            _htmlEditorContext.PreHandleEvent -= new HtmlEditDesignerEventHandler(presentationEditor_PreHandleEvent);
            _htmlEditorContext.TranslateAccelerator -= new HtmlEditDesignerEventHandler(presentationEditor_TranslateAccelerator);
            _editorFrame.Deactivate -= new EventHandler(_editorFrame_Deactivate);

            // dispose the tooltip
            if (toolTip != null)
                toolTip.Dispose();

            if (_frameActiveTimer != null)
            {
                _frameActiveTimer.Stop();
                _frameActiveTimer.Dispose();
                _frameActiveTimer = null;
            }
        }

        /// <summary>
        /// Handle editor events to do link navigation (including tooltips)
        /// </summary>
        /// <param name="inEvtDispId">event id</param>
        /// <param name="pIEventObj">event object</param>
        /// <returns>S_OK to indicate event fully handled, S_FALSE to continue default processing</returns>
        private int presentationEditor_PreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            switch (inEvtDispId)
            {
                case DISPID_HTMLELEMENTEVENTS2.ONMOUSEOVER:
                    return HandleMouseOver(pIEventObj);
                case DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN:
                    return HandleMouseDown(pIEventObj);
                default:
                    return HRESULT.S_FALSE;
            }
        }

        /// <summary>
        /// Handle mouse over event
        /// </summary>
        /// <param name="pIEventObj">event object</param>
        private int HandleMouseOver(IHTMLEventObj pIEventObj)
        {
            // see if we are over a link element
            IHTMLElement linkElement = GetLinkElement(pIEventObj.srcElement);

            // see if this represents a change in state
            if (linkElement != currentLinkElement)
            {

                // change state
                currentLinkElement = linkElement;

                // reset navigated flag
                navigatedToCurrentLinkElement = false;

                // update feedback
                UpdateFeedback();
            }

            // just to be paranoid, if there is no current link element we should
            // always update feedback (to make sure that an edge case doesn't
            // cause us to keep feedback around when it shouldn't be)
            if (currentLinkElement == null)
                UpdateFeedback();

            // in all cases continue default processing
            return HRESULT.S_FALSE;
        }

        /// <summary>
        /// Handle mouse down event
        /// </summary>
        /// <param name="pIEventObj">event object</param>
        private int HandleMouseDown(IHTMLEventObj pIEventObj)
        {
            // if the control key is down
            if (pIEventObj.ctrlKey)
            {
                // see if we are over a link element
                IHTMLElement element = GetLinkElement(pIEventObj.srcElement);
                if (element != null)
                {
                    // navigate to link
                    string href = String.Empty;
                    try
                    {
                        // set flag indicating we have navigated to the current link element
                        // and then update the tooltip
                        navigatedToCurrentLinkElement = true;
                        UpdateTooltip();

                        // get href
                        IHTMLAnchorElement anchor = element as IHTMLAnchorElement;
                        href = anchor.href;

                        // launch it using the shell
                        if (UrlHelper.IsUrl(href))
                            ShellHelper.LaunchUrl(href);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Unexpected failure to navigate to link {0}\r\n{1}", href, e.Message));
                    }

                    // suppress event (we handled it)
                    return HRESULT.S_OK;

                }
                else // not over an element
                {
                    return HRESULT.S_FALSE;
                }
            }
            else // control-key not down
            {
                // any standard mouse-down causes us to clear the current tooltip (user
                // wants to edit or show context menu)
                ClearLinkFeedback();

                // continue default processing
                return HRESULT.S_FALSE;
            }
        }

        /// <summary>
        /// Handle TranslateAccelerator to update feedback on cursor and to clear the
        /// tooltip for ALT-TAB, CTL-HOME, etc.
        /// </summary>
        /// <param name="inEvtDispId"></param>
        /// <param name="pIEventObj"></param>
        /// <returns></returns>
        private int presentationEditor_TranslateAccelerator(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            // control key up or down always results in updating the cursor
            if (pIEventObj.keyCode == (int)Keys.ControlKey)
            {
                UpdateCursor();
            }
            // all other accelerators are some type of user action -- clear feedback
            else
            {
                ClearLinkFeedback();
            }

            // continue default feedback
            return HRESULT.S_FALSE;
        }

        /// <summary>
        /// Always clear link feedback when the document loses focus
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void _documentEvents_LostFocus(object sender, EventArgs e)
        {
            ClearLinkFeedback();
        }

        /// <summary>
        /// Always clear link feedback when the frame is deactivated
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void _editorFrame_Deactivate(object sender, EventArgs e)
        {
            ClearLinkFeedback();
        }

        /// <summary>
        /// Monitors whether the frame is currently active and remove the tooltip
        /// if it is. This technically should not be necessary as we already monitor
        /// the Deactivate event for the frame however it appears as if sometimes
        /// this event is not called (not reproducible but there is no doubt that
        /// it does sometimes occur)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void frameActiveTimer_Tick(object sender, EventArgs e)
        {
            if (_frameActiveTimer == null)
                return;

            // see if the main frame has lost focus
            IntPtr parentWindow = User32.GetAncestor(_editorFrame.Handle, GA.ROOT);
            IntPtr foregroundWindow = User32.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero || !(foregroundWindow == _editorFrame.Handle || foregroundWindow == parentWindow))
            {
                if (currentLinkElement != null)
                    ClearLinkFeedback();
            }

            // stop and dispose the timer if link feedback has been cleared
            if (currentLinkElement == null)
            {
                _frameActiveTimer.Stop();
                _frameActiveTimer.Dispose();
            }
        }

        /// <summary>
        /// Clear any existing link feedback
        /// </summary>
        private void ClearLinkFeedback()
        {
            currentLinkElement = null;
            navigatedToCurrentLinkElement = false;
            UpdateFeedback();
        }

        /// <summary>
        /// Update all forms of feedback (status bar, tooltip, cursor)
        /// </summary>
        private void UpdateFeedback()
        {
            // update status bar
            UpdateStatusBar();

            // update tooltip
            UpdateTooltip();

            // update the cursor
            UpdateCursor();
        }

        /// <summary>
        /// Update the status bar
        /// </summary>
        void UpdateStatusBar()
        {
            // get rid of any existing status text
            if (statusBarTextVisible)
            {
                _statusBar.PopStatusMessage();
                statusBarTextVisible = false;
            }

            // if we are over a link then put the base href on the status bar
            if (currentLinkElement != null)
            {
                string href = (string)currentLinkElement.getAttribute("href", 2); // 2 means don't auto-escape
                if (href != null)
                {
                    // set the status text
                    _statusBar.PushStatusMessage(href);

                    // set internal flag so we know to clean it up later
                    statusBarTextVisible = true;
                }
            }
        }

        /// <summary>
        /// Update the tooltip
        /// </summary>
        void UpdateTooltip()
        {
            // always start by hiding any existing tooltip
            toolTip.SetToolTip(null);

            // see if we need to display a tooltip
            if (currentLinkElement != null && !navigatedToCurrentLinkElement)
            {
                // set timer so we only show the tooltip if the user hovers over this
                // link element for 300ms
                Timer toolTipDelayTimer = new ToolTipDelayTimer(currentLinkElement, 300);
                toolTipDelayTimer.Tick += new EventHandler(toolTipDelayTimer_Tick);
                toolTipDelayTimer.Start();
            }
        }

        /// <summary>
        /// Delayed processing for showing tooltips (only show them if the user hovers
        /// over a link for more than a specified period of time)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void toolTipDelayTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // get the delay element from the timer
                ToolTipDelayTimer toolTipDelayTimer = sender as ToolTipDelayTimer;
                IHTMLElement delayElement = toolTipDelayTimer.DelayElement;

                // stop and dispose timer
                toolTipDelayTimer.Stop();
                toolTipDelayTimer.Dispose();

                // determine the link element the mouse is currently over and check to  see
                // if the delay element is the same as the current element -- if so, show the tooltip
                IHTMLElement mouseOverElement = _htmlEditorControl.ElementAtPoint(Control.MousePosition);
                if (mouseOverElement != null && (GetLinkElement(mouseOverElement) == delayElement) && !navigatedToCurrentLinkElement && currentLinkElement != null)
                {
                    // calculate the screen coordinate of the top of the current link element
                    int elementClientY = HTMLElementHelper.GetTopRelativeToClient(mouseOverElement);
                    Point elementScreenPt = _htmlEditorControl.ClientPointToScreenPoint(new Point(0, elementClientY));

                    // set location - x-based on mouse, y-based on top of element (minus height of tooltip)
                    toolTip.Location = new Point(Control.MousePosition.X, elementScreenPt.Y - 20);

                    // set tooltip text
                    string href = (string)currentLinkElement.getAttribute("href", 2); // 2 means don't auto-escape
                    href = StringHelper.Ellipsis(href, 35);
                    toolTip.SetToolTip(String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.LinkToolTip), href));

                    // start timer which monitors whether the main frame is still active and
                    // hides the tooltip if it does not
                    _frameActiveTimer = new Timer();
                    _frameActiveTimer.Interval = 100;
                    _frameActiveTimer.Tick += new EventHandler(frameActiveTimer_Tick);
                    _frameActiveTimer.Start();
                }
            }
            catch (Exception ex)
            {
                // eat exceptions that occur in here (bizzare timing bugs can occur w/ drag and
                // drop of images -- this is harmless and the user shouldn't be burdened with
                // an error message
                Debug.Fail("Unexpected error during tool tip delay timer: " + ex.ToString());
            }
        }

        /// <summary>
        /// Update the state of the cursor
        /// </summary>
        private void UpdateCursor()
        {
            // if we over a link element with the control-key down then show hand cursor
            if (currentLinkElement != null && ((Control.ModifierKeys & Keys.Control) > 0))
            {
                _htmlEditorContext.OverrideCursor = true;
                Cursor.Current = Cursors.Hand;
            }
            else // restore default cursor
            {
                // stop overriding cursor
                _htmlEditorContext.OverrideCursor = false;

                // force cursor back to insert if we are over a link
                if (currentLinkElement != null)
                    Cursor.Current = Cursors.IBeam;
            }
        }

        /// <summary>
        /// Gets the parent link element (if any) for the passed element
        /// </summary>
        /// <param name="element">element</param>
        /// <returns>link element (or null if this element or one of its parents are not a link)</returns>
        private IHTMLElement GetLinkElement(IHTMLElement element)
        {
            // never exhibit this behavior for an image element
            if (SuppressForImages)
            {
                if (element is IHTMLImgElement)
                    return null;
            }

            // search up the parent heirarchy
            while (element != null)
            {
                // if it is an anchor that has an HREF (exclude anchors with only NAME)
                // then stop searching
                if (element is IHTMLAnchorElement)
                {
                    string href = element.getAttribute("href", 2) as string;
                    if (href != null && !(SuppressForLocalUrls && UrlHelper.IsFileUrl(href)))
                    {
                        IHTMLElement3 anchorElement3 = (IHTMLElement3)element;
                        if (!SuppressForNonEditableRegions || anchorElement3.isContentEditable)
                        {
                            return element;
                        }
                    }
                }

                // search parent
                element = element.parentElement;
            }

            // didn't find an anchor
            return null;
        }

        /// <summary>
        /// Helper class used to create a delay for showing tooltips
        /// </summary>
        private class ToolTipDelayTimer : Timer
        {
            public ToolTipDelayTimer(IHTMLElement delayElement, int delay)
            {
                DelayElement = delayElement;
                Interval = delay;
            }

            public readonly IHTMLElement DelayElement;
        }

        /// <summary>
        /// editor context we are attached to
        /// </summary>
        private IHtmlEditorComponentContext _htmlEditorContext;

        /// <summary>
        /// editor context we are attached to
        /// </summary>
        private HtmlEditorControl _htmlEditorControl;

        /// <summary>
        /// Frame we are hosted int
        /// </summary>
        private IMainFrameWindow _editorFrame;

        private IStatusBar _statusBar;

        /// <summary>
        /// Event sink for html document
        /// </summary>
        private IMshtmlDocumentEvents _documentEvents;

        /// <summary>
        /// Link element we are currently over
        /// </summary>
        private IHTMLElement currentLinkElement;

        /// <summary>
        /// State variable determining whether status bar text is currently visible
        /// </summary>
        private bool statusBarTextVisible = false;

        /// <summary>
        /// Flag indicating we have navigated to the current link element (supresses the
        /// tooltip from displaying)
        /// </summary>
        private bool navigatedToCurrentLinkElement = false;

        /// <summary>
        /// Tooltip used to notify user that they can CTRL + click to navigate
        /// </summary>
        private TrackingToolTip toolTip = new TrackingToolTip();

        /// <summary>
        /// Double checks that when the main window is deactivated, the tooltip is hidden.
        /// </summary>
        private Timer _frameActiveTimer;
    }
}
