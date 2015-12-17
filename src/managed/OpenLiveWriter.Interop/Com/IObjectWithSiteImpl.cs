// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Generic implementation of IObjectWithSite (interface for communication
    /// between an object and its site in a container).
    /// </summary>
    public class IObjectWithSiteImpl : IObjectWithSite
    {
        /// <summary>
        ///  IUnknown pointer to the site that was set
        /// </summary>
        public object Site
        {
            get { return m_IUnkSite; }
        }
        private object m_IUnkSite = null;

        /// <summary>
        /// Provides the IUnknown site pointer (save for returning in GetSite)
        /// </summary>
        /// <param name="pUnkSite">Browser interface as IUnknown</param>
        public virtual int SetSite([In, MarshalAs(UnmanagedType.IUnknown)] Object pUnkSite)
        {
            // we are being set with a new site, release reference to existing site
            if (m_IUnkSite != null)
            {
                Marshal.ReleaseComObject(m_IUnkSite);
                m_IUnkSite = null;
            }

            // grab the new site
            m_IUnkSite = pUnkSite;

            return HRESULT.S_OK;

        }

        /// <summary>
        /// Returns the last site set with SetSite
        /// </summary>
        /// <param name="riid">The interface identifer whose pointer should be
        /// returned in ppvSite</param>
        /// <param name="ppvSite">The last site pointer passed in to SetSite</param>
        public virtual void GetSite(
            ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out Object ppvSite)
        {
            // default to returning null
            ppvSite = null;

            // if we have an BandSite, delegate to its QueryInterface method
            if (m_IUnkSite != null)
            {
                // manually call Marshal.QueryInterface to delegate the request
                IntPtr pReturned;
                IntPtr pUnk = Marshal.GetIUnknownForObject(m_IUnkSite);
                Marshal.QueryInterface(pUnk, ref riid, out pReturned);
                Marshal.Release(pUnk);

                // set site if we got an interface returned
                if (pReturned != IntPtr.Zero)
                    ppvSite = Marshal.GetObjectForIUnknown(pReturned);
                else
                {
                    ComHelper.Return(HRESULT.E_FAILED);
                }
            }
            else // GetSite called when we have no Site
            {
                ComHelper.Return(HRESULT.E_FAILED);
            }
        }
    }
}
