// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml.Mshtml_Interop;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Base implementation of interface used for customizing the UI of MSHTML.
    /// Provides correct default behavior for all methods and allows selective
    /// overrides for targeted customization.
    /// </summary>
    public class IDocHostUIHandlerBaseImpl : IDocHostUIHandler, IDocHostUIHandler2
    {
        public virtual int ShowContextMenu(int dwID, ref POINT ppt, object pcmdtReserved, object pdispReserved)
        {
            // Host did not display any UI. MSHTML will display its UI.
            return HRESULT.S_FALSE;
        }

        public virtual void GetHostInfo(ref DOCHOSTUIINFO pInfo)
        {
        }

        public virtual int ShowUI(DOCHOSTUITYPE dwID, IOleInPlaceActiveObject pActiveObject, IOleCommandTarget pCommandTarget, IOleInPlaceFrame pFrame, IOleInPlaceUIWindow pDoc)
        {
            // Host did not display any UI. MSHTML will display its UI.
            return HRESULT.S_FALSE;
        }

        public virtual void HideUI()
        {
        }

        public virtual void UpdateUI()
        {
        }

        public virtual void EnableModeless(bool fEnable)
        {
        }

        public virtual void OnDocWindowActivate(bool fActivate)
        {
        }

        public virtual void OnFrameWindowActivate(bool fActivate)
        {
        }

        public virtual void ResizeBorder(ref RECT prcBorder, IOleInPlaceUIWindow pUIWindow, bool frameWindow)
        {
        }

        public virtual int TranslateAccelerator(ref MSG lpMsg, ref Guid pguidCmdGroup, uint nCmdID)
        {
            return HRESULT.S_FALSE;
        }

        public virtual void GetOptionKeyPath(out IntPtr pchKey, uint dwReserved)
        {
            // use default option key path
            pchKey = IntPtr.Zero;
        }

        public virtual void GetOverrideKeyPath(out IntPtr pchKey, uint dwReserved)
        {
            // use default override key path
            pchKey = IntPtr.Zero;
        }

        public virtual int GetDropTarget(IDropTarget pDropTarget, out IDropTarget ppDropTarget)
        {
            // use the default drop target implementation
            ppDropTarget = null;
            return HRESULT.E_NOTIMPL;
        }

        public virtual void GetExternal(out IntPtr ppDispatch)
        {
            // no external dispatch implementation available
            ppDispatch = IntPtr.Zero;
        }

        public virtual int TranslateUrl(uint dwReserved, IntPtr pchURLIn, out IntPtr ppchURLOut)
        {
            // do not translate url
            ppchURLOut = pchURLIn;
            return HRESULT.S_FALSE;
        }

        public virtual int FilterDataObject(IOleDataObject pDO, out IOleDataObject ppDORet)
        {
            // do not filter data object
            ppDORet = pDO;
            return HRESULT.S_FALSE;
        }
    }
}
