// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Mshtml
{

    /// <summary>
    /// Interface used for relaying events from Mshtml
    /// </summary>
    public interface IMshtmlDocumentEvents
    {
        /// <summary>
        /// Event raised when the mouse is clicked.
        /// </summary>
        event HtmlEventHandler Click;

        /// <summary>
        /// Event raised when the mouse is double clicked.
        /// </summary>
        event HtmlEventHandler DoubleClick;

        /// <summary>
        /// Event raised when the mouse button is down.
        /// </summary>
        event HtmlEventHandler MouseDown;

        /// <summary>
        /// Event raised when the mouse button is up.
        /// </summary>
        event HtmlEventHandler MouseUp;

        /// <summary>
        /// Event raised when the selection has changed
        /// </summary>
        event EventHandler SelectionChanged;

        /// <summary>
        /// Event raised when an alphanumeric key has been pressed
        /// </summary>
        event HtmlEventHandler KeyPress;

        /// <summary>
        /// Event raised when *any* key has been pressed
        /// </summary>
        event HtmlEventHandler KeyDown;

        /// <summary>
        /// Event raised when *any* key has been released
        /// </summary>
        event HtmlEventHandler KeyUp;

        /// <summary>
        /// Notification that a property on the page has changed
        /// </summary>
        event HtmlEventHandler PropertyChange;

        /// <summary>
        /// Event raised when the document "ReadyState" property changes
        /// </summary>
        event EventHandler ReadyStateChanged;

        /// <summary>
        /// Event raised when the document gets focus
        /// </summary>
        event EventHandler GotFocus;

        /// <summary>
        /// Event raised when the document loses focus
        /// </summary>
        event EventHandler LostFocus;
    }

    /// <summary>
    /// Defines the Event Handler interface for dealing with events from the MshtmlControl.
    /// </summary>
    public delegate void HtmlEventHandler(object o, HtmlEventArgs e);

    /// <summary>
    /// Event that wraps the raw IHTMLEventObj fired by the document.
    /// </summary>
    public class HtmlEventArgs : EventArgs
    {
        private readonly IHTMLEventObj _htmlEvt;
        private readonly IHTMLEventObj2 _htmlEvt2;

        public HtmlEventArgs(IHTMLEventObj evt)
        {
            _htmlEvt = evt;
            _htmlEvt2 = evt as IHTMLEventObj2;
        }

        public IHTMLEventObj htmlEvt
        {
            get
            {
                return _htmlEvt;
            }
        }
        public IHTMLEventObj2 htmlEvt2
        {
            get
            {
                return _htmlEvt2;
            }
        }

        /// <summary>
        /// Cancels the event and kills its propogation.
        /// </summary>
        public void Cancel()
        {
            //set the cancelBubble value
            htmlEvt.cancelBubble = true;

            //the returnValue controls the propogation of the event.
            htmlEvt.returnValue = false;

            // update state
            _wasCancelled = true;
        }

        public bool WasCancelled
        {
            get
            {
                return _wasCancelled;
            }
        }
        private bool _wasCancelled = false;

    }

    /// <summary>
    /// Class which implements HTMLDocumentEvents2 to provide an event repeater to
    /// .NET clients. This enables .NET to directly sink to the events of an
    /// existing IHTMLDocument2 instance (i.e. one that was not created as part
    /// of an ActiveX control and therefore doesn't have built in .NET event
    /// handling). Note that we are only implementing repeaters as needed so many
    /// of these event handlers are simply no-ops.
    /// </summary>
    public class HtmlDocumentEventRepeater : IMshtmlDocumentEvents, HTMLDocumentEvents2
    {
        private readonly EventCounter _eventCounter = new EventCounter();

        /// <summary>
        /// Initialize without attaching to a document instance
        /// </summary>
        public HtmlDocumentEventRepeater()
        {
        }

        /// <summary>
        /// Initialize by attaching to a document instance
        /// </summary>
        /// <param name="webBrowser">browser to attach to</param>
        public HtmlDocumentEventRepeater(IHTMLDocument2 document)
        {
            Attach(document);
        }

        /// <summary>
        /// Attach the event repeater to the specified document instance. Call Detach
        /// when you no longer want to receive events from the document.
        /// </summary>
        /// <param name="webBrowser"></param>
        public void Attach(IHTMLDocument2 document)
        {
            // confirm pre-conditions
            Debug.Assert(document != null);
            Debug.Assert(connectionPoint == null);

            // query for the IConnectionPointContainer interface
            IConnectionPointContainer cpContainer = (IConnectionPointContainer)document;

            // find the HTMLDocumentEvents2 connection point
            cpContainer.FindConnectionPoint(ref iidHTMLDocumentEvents2, out connectionPoint);

            // attach to the event interface
            connectionPoint.Advise(this, out connectionPointCookie);

            //cache a handle to the document
            HtmlDocument = document;

            _eventCounter.Reset();
        }

        /// <summary>
        /// Detach the event repeater from the document instance
        /// </summary>
        public void Detach()
        {
            // confirm preconditions
            Debug.Assert(connectionPoint != null);

            // detach and set internal references to null
            connectionPoint.Unadvise(connectionPointCookie);
            connectionPoint = null;
            HtmlDocument = null;

            _eventCounter.AssertAllEventsAreUnhooked();
        }

        /// <summary>
        /// Event raised when the mouse is clicked.
        /// </summary>
        public event HtmlEventHandler Click
        {
            add
            {
                ClickEventHandler += value;
                _eventCounter.EventHooked(ClickEventHandler);
            }
            remove
            {
                ClickEventHandler -= value;
                _eventCounter.EventUnhooked(ClickEventHandler);
            }
        }
        private event HtmlEventHandler ClickEventHandler;

        /// <summary>
        /// Event raised when the mouse is double clicked.
        /// </summary>
        public event HtmlEventHandler DoubleClick
        {
            add
            {
                DoubleClickEventHandler += value;
                _eventCounter.EventHooked(DoubleClickEventHandler);
            }
            remove
            {
                DoubleClickEventHandler -= value;
                _eventCounter.EventUnhooked(DoubleClickEventHandler);
            }
        }
        private event HtmlEventHandler DoubleClickEventHandler;

        /// <summary>
        /// Event raised when the mouse is pressed down.
        /// </summary>
        public event HtmlEventHandler MouseDown
        {
            add
            {
                MouseDownEventHandler += value;
                _eventCounter.EventHooked(MouseDownEventHandler);
            }
            remove
            {
                MouseDownEventHandler -= value;
                _eventCounter.EventUnhooked(MouseDownEventHandler);
            }
        }
        private event HtmlEventHandler MouseDownEventHandler;

        /// <summary>
        /// Event raised when the mouse is raised up.
        /// </summary>
        public event HtmlEventHandler MouseUp
        {
            add
            {
                MouseUpEventHandler += value;
                _eventCounter.EventHooked(MouseUpEventHandler);
            }
            remove
            {
                MouseUpEventHandler -= value;
                _eventCounter.EventUnhooked(MouseUpEventHandler);
            }
        }
        private event HtmlEventHandler MouseUpEventHandler;

        /// <summary>
        /// Event raised when the selection has changed
        /// </summary>
        public event EventHandler SelectionChanged
        {
            add
            {
                SelectionChangedEventHandler += value;
                _eventCounter.EventHooked(SelectionChangedEventHandler);
            }
            remove
            {
                SelectionChangedEventHandler -= value;
                _eventCounter.EventUnhooked(SelectionChangedEventHandler);
            }
        }
        private event EventHandler SelectionChangedEventHandler;

        /// <summary>
        /// Event raised when an alphanumeric key has been pressed
        /// </summary>
        public event HtmlEventHandler KeyPress
        {
            add
            {
                KeyPressEventHandler += value;
                _eventCounter.EventHooked(KeyPressEventHandler);
            }
            remove
            {
                KeyPressEventHandler -= value;
                _eventCounter.EventUnhooked(KeyPressEventHandler);
            }
        }
        private event HtmlEventHandler KeyPressEventHandler;

        /// <summary>
        /// Event raised when any key has been pressed (including
        /// delete, function, and symbols keys, etc)
        /// </summary>
        public event HtmlEventHandler KeyDown
        {
            add
            {
                KeyDownEventHandler += value;
                _eventCounter.EventHooked(KeyDownEventHandler);
            }
            remove
            {
                KeyDownEventHandler -= value;
                _eventCounter.EventUnhooked(KeyDownEventHandler);
            }
        }
        private event HtmlEventHandler KeyDownEventHandler;

        /// <summary>
        /// Event raised when any key has been released (including
        /// delete, function, and symbols keys, etc)
        /// </summary>
        public event HtmlEventHandler KeyUp
        {
            add
            {
                KeyUpEventHandler += value;
                _eventCounter.EventHooked(KeyUpEventHandler);
            }
            remove
            {
                KeyUpEventHandler -= value;
                _eventCounter.EventUnhooked(KeyUpEventHandler);
            }
        }
        private event HtmlEventHandler KeyUpEventHandler;

        /// <summary>
        /// Event raised when a property on the page changes
        /// </summary>
        public event HtmlEventHandler PropertyChange
        {
            add
            {
                PropertyChangeEventHandler += value;
                _eventCounter.EventHooked(PropertyChangeEventHandler);
            }
            remove
            {
                PropertyChangeEventHandler -= value;
                _eventCounter.EventUnhooked(PropertyChangeEventHandler);
            }
        }
        private event HtmlEventHandler PropertyChangeEventHandler;

        /// <summary>
        /// Event raised when the document "ReadyState" property changes
        /// </summary>
        public event EventHandler ReadyStateChanged
        {
            add
            {
                ReadyStateChangedEventHandler += value;
                _eventCounter.EventHooked(ReadyStateChangedEventHandler);
            }
            remove
            {
                ReadyStateChangedEventHandler -= value;
                _eventCounter.EventUnhooked(ReadyStateChangedEventHandler);
            }
        }
        private event EventHandler ReadyStateChangedEventHandler;

        /// <summary>
        /// Event raised when the document gets focus
        /// </summary>
        public event EventHandler GotFocus
        {
            add
            {
                GotFocusEventHandler += value;
                _eventCounter.EventHooked(GotFocusEventHandler);
            }
            remove
            {
                GotFocusEventHandler -= value;
                _eventCounter.EventUnhooked(GotFocusEventHandler);
            }
        }
        private event EventHandler GotFocusEventHandler;

        /// <summary>
        /// Event raised when the document loses focus
        /// </summary>
        public event EventHandler LostFocus
        {
            add
            {
                LostFocusEventHandler += value;
                _eventCounter.EventHooked(LostFocusEventHandler);
            }
            remove
            {
                LostFocusEventHandler -= value;
                _eventCounter.EventUnhooked(LostFocusEventHandler);
            }
        }
        private event EventHandler LostFocusEventHandler;

        /////////////////////////////////////////////////////////////////////////////////
        // HTMLDocumentEvents2 implemented event handlers -- These event handlers are
        // used to 'repeat' events to outside listeners.
        //

        void HTMLDocumentEvents2.onselectionchange(IHTMLEventObj pEvtObj)
        {
            if (SelectionChangedEventHandler != null)
                SelectionChangedEventHandler(this, new HtmlEventArgs(pEvtObj));
        }

        void HTMLDocumentEvents2.onreadystatechange(IHTMLEventObj pEvtObj)
        {
            if (ReadyStateChangedEventHandler != null)
                ReadyStateChangedEventHandler(this, EventArgs.Empty);
        }

        bool HTMLDocumentEvents2.onkeypress(IHTMLEventObj pEvtObj)
        {
            if (KeyPressEventHandler != null)
                KeyPressEventHandler(this, new HtmlEventArgs(pEvtObj));

            return !pEvtObj.cancelBubble;
        }

        void HTMLDocumentEvents2.onfocusin(IHTMLEventObj pEvtObj)
        {
            if (GotFocusEventHandler != null)
                GotFocusEventHandler(this, EventArgs.Empty);
        }

        void HTMLDocumentEvents2.onfocusout(IHTMLEventObj pEvtObj)
        {
            if (LostFocusEventHandler != null)
                LostFocusEventHandler(this, EventArgs.Empty);
        }

        /////////////////////////////////////////////////////////////////////////////////
        // HTMLDocumentEvents2 no-op event handlers -- As we need to handle the various
        // events we will fill in the implementations. NOTE that even though the MSDN
        // documentation asserts that we should return FALSE from these event handlers
        // to allow event bubbling the reverse of this is in fact in the case. You
        // actually must return TRUE to allow events to propagate. See the extended
        // comment at the bottom of this file for more info.
        //

        void HTMLDocumentEvents2.ondataavailable(IHTMLEventObj pEvtObj)
        {
        }

        bool HTMLDocumentEvents2.onbeforedeactivate(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        bool HTMLDocumentEvents2.onstop(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        void HTMLDocumentEvents2.onrowsinserted(IHTMLEventObj pEvtObj)
        {
        }

        bool HTMLDocumentEvents2.onselectstart(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        bool HTMLDocumentEvents2.onhelp(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        void HTMLDocumentEvents2.onpropertychange(IHTMLEventObj pEvtObj)
        {
            if (PropertyChangeEventHandler != null)
                PropertyChangeEventHandler(this, new HtmlEventArgs(pEvtObj));
        }

        void HTMLDocumentEvents2.oncellchange(IHTMLEventObj pEvtObj)
        {
        }

        bool HTMLDocumentEvents2.oncontextmenu(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        bool HTMLDocumentEvents2.ondblclick(IHTMLEventObj pEvtObj)
        {
            if (DoubleClickEventHandler != null)
                DoubleClickEventHandler(this, new HtmlEventArgs(pEvtObj));

            return !pEvtObj.cancelBubble;
        }

        void HTMLDocumentEvents2.ondatasetcomplete(IHTMLEventObj pEvtObj)
        {
        }

        void HTMLDocumentEvents2.onbeforeeditfocus(IHTMLEventObj pEvtObj)
        {
        }

        bool HTMLDocumentEvents2.ondragstart(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        bool HTMLDocumentEvents2.oncontrolselect(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        void HTMLDocumentEvents2.onactivate(IHTMLEventObj pEvtObj)
        {
        }

        void HTMLDocumentEvents2.onmouseup(IHTMLEventObj pEvtObj)
        {
            if (MouseUpEventHandler != null)
                MouseUpEventHandler(this, new HtmlEventArgs(pEvtObj));
        }

        bool HTMLDocumentEvents2.onbeforeactivate(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        void HTMLDocumentEvents2.onkeydown(IHTMLEventObj pEvtObj)
        {
            if (KeyDownEventHandler != null)
                KeyDownEventHandler(this, new HtmlEventArgs(pEvtObj));
        }

        void HTMLDocumentEvents2.onkeyup(IHTMLEventObj pEvtObj)
        {
            if (KeyUpEventHandler != null)
                KeyUpEventHandler(this, new HtmlEventArgs(pEvtObj));
        }

        bool HTMLDocumentEvents2.onrowexit(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        bool HTMLDocumentEvents2.onbeforeupdate(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        void HTMLDocumentEvents2.onrowsdelete(IHTMLEventObj pEvtObj)
        {
        }

        void HTMLDocumentEvents2.onmousemove(IHTMLEventObj pEvtObj)
        {
        }

        void HTMLDocumentEvents2.onrowenter(IHTMLEventObj pEvtObj)
        {
        }

        void HTMLDocumentEvents2.onafterupdate(IHTMLEventObj pEvtObj)
        {
        }

        void HTMLDocumentEvents2.ondeactivate(IHTMLEventObj pEvtObj)
        {
        }

        void HTMLDocumentEvents2.ondatasetchanged(IHTMLEventObj pEvtObj)
        {
        }

        void HTMLDocumentEvents2.onmouseover(IHTMLEventObj pEvtObj)
        {
        }

        bool HTMLDocumentEvents2.onmousewheel(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        bool HTMLDocumentEvents2.onerrorupdate(IHTMLEventObj pEvtObj)
        {
            return true;
        }

        void HTMLDocumentEvents2.onmouseout(IHTMLEventObj pEvtObj)
        {
        }

        void HTMLDocumentEvents2.onmousedown(IHTMLEventObj pEvtObj)
        {
            if (MouseDownEventHandler != null)
                MouseDownEventHandler(this, new HtmlEventArgs(pEvtObj));
        }

        bool HTMLDocumentEvents2.onclick(IHTMLEventObj pEvtObj)
        {
            if (ClickEventHandler != null)
                ClickEventHandler(this, new HtmlEventArgs(pEvtObj));

            return !pEvtObj.cancelBubble;
        }

        // interface id for document events
        private Guid iidHTMLDocumentEvents2 = typeof(HTMLDocumentEvents2).GUID;

        // connection point cookie used for call to IConnectionPoint.Unadvise
        private int connectionPointCookie = 0;

        // connection point that we attach our event interface to
        private IConnectionPoint connectionPoint = null;

        /// <summary>
        /// document that this object repeats events for.
        /// </summary>
        public IHTMLDocument2 HtmlDocument;

        /// <summary>
        /// Internal utility for providing IE6-compatible focus events on IE 5.5.
        /// his method keeps track of focus states and fires focus events if a focus change has occured.
        /// </summary>
        internal void NotifyDocumentDisplayChanged()
        {
            if (HtmlDocument != null && !SupportsIE6Events)
            {
                //IE 5.5 doesn't fire focus events, so keep track of the focus state and fire the event
                //when a change is detected (bug 1957).
                bool hasFocus = (HtmlDocument as IHTMLDocument4).hasFocus();
                if (wasFocused != hasFocus)
                {
                    if (hasFocus)
                    {
                        if (GotFocusEventHandler != null)
                            GotFocusEventHandler(this, EventArgs.Empty);
                    }
                    else
                    {
                        if (LostFocusEventHandler != null)
                            LostFocusEventHandler(this, EventArgs.Empty);
                    }
                    wasFocused = hasFocus;
                }
            }
        }
        bool wasFocused;

        /// <summary>
        /// Returns true if the control is running with an IE6-event-compatible browser.
        /// </summary>
        private bool SupportsIE6Events
        {
            get
            {
                if (supportsIE6Events == -1 && HtmlDocument != null)
                {
                    try
                    {
                        //attempt to use a method that is only supported by IE6 and later. If this throws an
                        //exception, then we know that we know that IE6 events are not supported.
                        supportsIE6Events = (HtmlDocument as IHTMLDocument5).onfocusin != null ? 1 : 0;
                    }
                    catch (Exception)
                    {
                        supportsIE6Events = 0;
                    }
                    if (supportsIE6Events == 0)
                        Trace.WriteLine("Using IE6 event compatibility mode");
                }
                return supportsIE6Events == 1;
            }
        }
        private int supportsIE6Events = -1; //use an int so that we can have 3 states(unknown/false/true)
    }
}

/* INFORMATION ON HTMLDocumentEvents2 Workaround

Google Groups Search: DotNet HTMLDocumentEvents2

URL for newsgroup posting on workaround:

http://groups.google.com/groups?q=DotNet+HTMLDocumentEvents2&hl=en&lr=&ie=UTF-8&oe=UTF-8&selm=9YiIa.12479%24Xj1.5184%40fe04.atl2.webusenet.com&rnum=2

Contents on newsgroup posting:

Okay, now that we've all felt the pain of watching "+=" event handler
registration kill all OTHER (default/existing) event handling in our
MSHTML-related code, here's a workaround of sorts.  I say "of sorts",
because it doesn't allow you to selectively (on a per-event basis) cancel or
enable event bubbling.

Blame for the C# code goes to me, credit to the MSIE support team for the
trick of using the "wrong" return values from the event handlers so that
other (existing) event handlers get invoked (the thing that has killed us
all when we use "+=" to register an onmousemove, only to watch keyboard
handling disappear, etc.).

Rumor has it that there will be a KB article with this workaround in the
next month or so.

1) Declare that your class implements HTMLDocumentEvents2

 public class YourClass : mshtml.HTMLDocumentEvents2[, other interfaces
implemented by your class]

2) Declare the equivalent of the following class members:

  private UCOMIConnectionPoint m_ConnectionPoint;  // connection point for
advisory notifications
  private Int32 m_ConnectionPointCookie = -1;  // the cookie val that idents
the connection instance

3) Implement all the members of that interface, coding your
"specializations" where desired:

 NOTE: The Platform SDK documentation for the following members that return
boolean values states they should return "false" to allow other (registered)
event handlers to continue processing the event, or "true" to PREVENT other
handlers from handling the event. The current interop code does NOT function
this way - "false" prevents other event handlers from processing the event,
"true" ALLOWS them to process the event. Rumor has it that returning "false"
actually cancels subsequent event handling in an intermittent fashion, but I
have not seen this behavior.

#region HTMLDocumentEvents2 interface members
  public void onactivate(mshtml.IHTMLEventObj evtObj )
  {
  }

  public void onafterupdate(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool onbeforeactivate(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public bool onbeforedeactivate(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public void onbeforeeditfocus(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool onbeforeupdate(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public void oncellchange(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool onclick(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public bool oncontextmenu(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public bool oncontrolselect(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public void ondataavailable(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void ondatasetchanged(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void ondatasetcomplete(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool ondblclick(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public void ondeactivate(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool ondragstart(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public bool onerrorupdate(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public void onfocusin(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onfocusout(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool onhelp(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public void onkeydown(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool onkeypress(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public void onkeyup(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onmousedown(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onmousemove(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onmouseout(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onmouseover(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onmouseup(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool onmousewheel(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public void onpropertychange(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onreadystatechange(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onrowenter(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool onrowexit(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public void onrowsdelete(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onrowsinserted(mshtml.IHTMLEventObj evtObj)
  {
  }

  public void onselectionchange(mshtml.IHTMLEventObj evtObj)
  {
  }

  public bool onselectstart(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

  public bool onstop(mshtml.IHTMLEventObj evtObj)
  {
   return true;
  }

#endregion

4)In your DocumentComplete handler (or some other point where the document
state is "Ready"),

    // find the source interface GUID for HTMLDocumentEvents2
    // HKEY_CLASSES_ROOT\Interface\{3050F60F-98B5-11CF-BB82-00AA00BDCE0B}
    Guid guid = typeof(mshtml.HTMLDocumentEvents2).GUID;

    // // m_HtmlDoc is the HTML doc; in my case, mshtml.IHTMLDocument2

    UCOMIConnectionPointContainer icpc =
(UCOMIConnectionPointContainer)m_HtmlDoc;
    icpc.FindConnectionPoint(ref guid, out m_ConnectionPoint);
    m_ConnectionPoint.Advise(this, out m_ConnectionPointCookie);

5) Unregister your event sink in your BeforeNavigate2 handler (or other
point where you will no longer want to receive HTMLDocumentEvents2
notifications):
    if (this.m_ConnectionPointCookie != -1)
     m_ConnectionPoint.Unadvise( this.m_ConnectionPointCookie);

BOL

--
Regards,

Jim Allison
jwallison.1@bellsouth.net
(de-mung by removing '.1')

*/

