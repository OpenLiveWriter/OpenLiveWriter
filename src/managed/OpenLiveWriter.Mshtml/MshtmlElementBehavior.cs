// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Mshtml
{

    /// <summary>
    /// Implementation of MSHTML element behavior interfaces (IElementBehavior
    /// and IHTMLPainter) that serve as as a base class for custom behaviors.
    /// </summary>
    public abstract class MshtmlElementBehavior : IElementBehaviorRaw, IHTMLPainterRaw, IHTMLPainterEventInfoRaw, IDisposable
    {
        #region Initialization/Disposal
        /// <summary>
        /// Attach the behavior to an HTML element
        /// </summary>
        /// <param name="element">element to attach to</param>
        public void AttachToElement(IHTMLElement element)
        {
            if (_htmlElement != null)
                throw new InvalidOperationException("Attempted to attach element to already initialized behavior");

            // attach this object to the element as a rendering behavior
            _htmlElement = element;
            IHTMLElement2 elementTarget = (IHTMLElement2)_htmlElement;
            object itemRenderingBehaviorFactory = new ElementBehaviorFactoryForExistingBehavior(this);
            _behaviorCookie = elementTarget.addBehavior(null, ref itemRenderingBehaviorFactory);

            // note that we are manually attached
            _manuallyAttached = true;
        }

        /// <summary>
        /// Detach the behavior from the HTML element
        /// </summary>
        public void DetachFromElement()
        {
            if (!_manuallyAttached)
                throw new InvalidOperationException("Attempted to detach behavior from element where behavior was not manually attached.");

            // remove behavior (will result in call to IElementBehaviorRaw.Detach)
            if (_htmlElement != null)
            {
                IHTMLElement2 element2 = (IHTMLElement2)_htmlElement;
                element2.removeBehavior(_behaviorCookie);
            }
        }

        /// <summary>
        /// Access HTML element and HTML document properties, etc.
        /// </summary>
        protected virtual void OnElementAttached()
        {
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Get the HTML element
        /// </summary>
        public IHTMLElement HTMLElement
        {
            get
            {
                Debug.Assert(_attached, "Getting HTMLElement from detached behavior!");
                if (_attached)
                {
                    // From the MSDN docs: Use the IElementBehaviorSite.GetElement method to get a pointer to the
                    // element that the DHTML behavior is attached to. Note that the pointer returned by a call to the
                    // IElementBehaviorSite.GetElement method should not be cached, because caching prevents the
                    // behavior from receiving a detach call.
                    IHTMLElement element = _elementBehaviorSite.GetElement();
                    Debug.Assert(element != null, "IElementBehaviorSite.GetElement returned null while behavior was attached!");

                    return element;
                }

                return null;
            }
        }

        #endregion

        #region Protected Helper Methods

        protected IHTMLPaintSiteRaw HTMLPaintSite
        {
            get
            {
                return _htmlPaintSite;
            }
        }

        /// <summary>
        /// Behaviors often have a scope and lifetime beyond the HTML elements they are attached to, so it's critical
        /// that methods that may be called after a behavior has been detached (i.e. event handlers) check to make
        /// sure the behavior is still attached and the associated HTMLElement is not null before executing.
        /// </summary>
        public bool Attached
        {
            get
            {
                return _attached;
            }
        }

        /// <summary>
        /// Convenience method to invalidate the entire element
        /// </summary>
        public void Invalidate()
        {
            IHTMLPaintSiteInvalidateAll paintSiteInvalidator = (IHTMLPaintSiteInvalidateAll)_htmlPaintSite;
            if (paintSiteInvalidator != null)
            {
                paintSiteInvalidator.InvalidateRect(IntPtr.Zero);
                paintSiteInvalidator = null;
            }
        }

        /// <summary>
        /// Invalidate a rectangular area of the rectangle
        /// </summary>
        /// <param name="rect"></param>
        public void Invalidate(Rectangle rect)
        {
            try
            {
                if (_htmlPaintSite == null)
                    return; //avoid exceptions that may occur if invalidates are triggered during initialization

                RECT invalidRect = RectangleHelper.Convert(rect);
                _htmlPaintSite.InvalidateRect(ref invalidRect);
            }
            catch (Exception)
            {
                //TODO: An exception gets thrown here when dragging or deleting images
                //and causes the image behavior to completely break. Is this exception OK to ignore, or
                //is there some protection logic we can put here?
                //Debug.Fail("An exception occured during invalidate", e.ToString());
            }
        }

        #endregion

        #region IElementBehavior Implementation

        /// <summary>
        /// IElementBehavior.Init -- Notifies the Dynamic HTML (DHTML) behavior that it has been instantiated
        /// </summary>
        /// <param name="pBehaviorSite">Pointer to the IElementBehaviorSite interface through
        /// which the DHTML behavior communicates with MSHTML </param>
        void IElementBehaviorRaw.Init(IElementBehaviorSite pBehaviorSite)
        {
            // retain references to key interfaces
            _elementBehaviorSite = pBehaviorSite;
            _htmlPaintSite = (IHTMLPaintSiteRaw)_elementBehaviorSite;
        }

        /// <summary>
        /// IElementBehavior.Notify -- Notifies the Dynamic HTML (DHTML) behavior about the
        /// progress of parsing the document and the element to which the behavior is attached
        /// </summary>
        /// <param name="lEvent">event code</param>
        /// <param name="pVar">unused (reserved) parameter</param>
        void IElementBehaviorRaw.Notify(int lEvent, IntPtr pVar)
        {
            try
            {
                // initialize references to key interfaces
                if (lEvent == (int)_BEHAVIOR_EVENT.BEHAVIOREVENT_DOCUMENTREADY)
                {
                    if (!_attached)
                    {
                        _attached = true;
                        OnElementAttached();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail("Unexpected error during MshtmlElementBehavior.Notify: " + ex.ToString());
            }
        }

        /// <summary>
        /// IElementBehavior.Detach -- Notifies the Dynamic HTML (DHTML) behavior that it is
        /// being detached from an element
        /// </summary>
        void IElementBehaviorRaw.Detach()
        {
            Dispose();
        }

        #endregion

        #region IHTMLPainter Implementation

        /// <summary>
        /// IHTMLPainter.GetPainterInfo -- Called by MSHTML to retrieve information about the needs
        /// and functionality of a rendering behavior
        /// </summary>
        /// <param name="pInfo">Pointer to a variable of type HTML_PAINTER_INFO that receives
        /// information the behavior needs to pass to MSHTML</param>
        public abstract void GetPainterInfo(ref _HTML_PAINTER_INFO pInfo);

        /// <summary>
        /// IHTMLPainter.HitTestPoint -- Called by MSHTML to retrieve a value that specifies
        /// whether a point is contained in a rendering behavior
        /// </summary>
        /// <param name="pt">POINT structure that specifies the point clicked relative to the
        /// top-left corner of the element to which the behavior is attached</param>
        /// <param name="pbHit">Pointer to a variable of type BOOL that receives TRUE if the
        /// point is contained in the element to which the rendering behavior is attached, or
        /// FALSE otherwise</param>
        /// <param name="plPartID">Pointer to a variable of type LONG that receives a number
        /// identifying which part of the behavior has been hit</param>
        /// <returns></returns>
        public virtual int HitTestPoint(POINT pt, ref bool pbHit, ref int plPartID)
        {
            plPartID = 0;
            pbHit = false;
            return HRESULT.S_OK;
        }

        /// <summary>
        /// IHTMLPainter.Resize -- Called by MSHTML when an element containing a rendering behavior is resized
        /// </summary>
        /// <param name="size">SIZE structure that specifies the new width and height for the element,
        /// including any expanded region</param>
        public virtual void OnResize(SIZE size)
        {
        }

        /// <summary>
        /// IHTMLPainter.Draw -- Called by MSHTML to render a behavior in the browser's client area
        /// </summary>
        /// <param name="rcBounds">RECT that specifies the bounds of the element to which the behavior is attached</param>
        /// <param name="rcUpdate">RECT that specifies a bounding rectangle for the region that needs to be redrawn</param>
        /// <param name="lDrawFlags">HTML_PAINT_DRAW_FLAGS enumeration that specifies options to use while drawing</param>
        /// <param name="hdc">HDC that specifies a Microsoft® Windows® Graphics Device Interface (GDI) device context for the behavior to use while drawing</param>
        /// <param name="pvDrawObject">Pointer to a drawing object, such as a Microsoft DirectDraw® surface, for the behavior to use while drawing</param>
        public virtual void Draw(RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
        }

        public virtual void GetEventInfoFlags(out HTML_PAINT_EVENT_FLAGS plEventInfoFlags)
        {
            plEventInfoFlags = 0;
        }

        public virtual void GetEventTarget(out IHTMLElement ppElement)
        {
            ppElement = HTMLElement;
        }

        public virtual void SetCursor(int lPartID)
        {
        }

        public virtual void StringFromPartID(int lPartID, out IntPtr ppElement)
        {
            ppElement = Marshal.StringToBSTR(lPartID.ToString(CultureInfo.InvariantCulture));
        }

        #endregion

        #region IDisposable Members

        public event EventHandler Disposed;

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            // Make sure subclasses assume they are detached from here on out.
            _attached = false;
            Dispose(true);
        }

        /// <summary>
        /// Because we cannot depend on IElementBehavior.Detach being called at the correct time, subclasses that have
        /// events hooked or references to unmanaged resources should override this method and clean themselves up.
        /// Note that we *cannot* assume that we will be attached by the time Dispose is called and therefore the
        /// Attached property will always return false during the call to Dispose!
        /// </summary>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            Debug.Assert(disposeManagedResources, "Behavior was not disposed correctly.");

            if (!_disposed)
            {
                _attached = false;
                _htmlElement = null;
                _elementBehaviorSite = null;
                _htmlPaintSite = null;

                _disposed = true;

                if (disposeManagedResources)
                {
                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }
        }

        ~MshtmlElementBehavior()
        {
            // Make sure subclasses assume they are detached from here on out.
            _attached = false;
            Dispose(false);
        }

        #endregion

        #region Private Implementation Data

        /// <summary>
        /// Flag used to track if this element behavior was attached through a call to IHTMLElement2.addBehavior.
        /// </summary>
        private bool _manuallyAttached;

        /// <summary>
        /// Cookie that can be used to detach the behavior from the element.
        /// </summary>
        private int _behaviorCookie;

        /// <summary>
        /// HTML element we are attached to
        /// </summary>
        private IHTMLElement _htmlElement;

        /// <summary>
        /// Flag used to track whether this elment behavior is currently attached.
        /// </summary>
        private bool _attached;

        /// <summary>
        /// Callback interface for behavior.
        /// </summary>
        private IElementBehaviorSite _elementBehaviorSite;

        /// <summary>
        /// Call back interface for paint site.
        /// </summary>
        private IHTMLPaintSiteRaw _htmlPaintSite;

        /// <summary>
        /// Flag used to track whether this behavior has been manually disposed.
        /// </summary>
        private bool _disposed;

        #endregion
    }
}

