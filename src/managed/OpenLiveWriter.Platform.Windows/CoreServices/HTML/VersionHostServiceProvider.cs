// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using mshtml;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;

namespace OpenLiveWriter.CoreServices
{
    public class VersionHostServiceProvider : IServiceProviderRaw, IVersionHost, IOleClientSite
    {
        public static readonly Guid VersionHostServiceId = new Guid("371ea634-dc5c-11d1-ba57-00c04fc2040e");

        private List<IVersionHost> _versionHosts;

        public VersionHostServiceProvider(params IVersionHost[] versionHosts)
            : this((IEnumerable<IVersionHost>)versionHosts)
        {
        }

        public VersionHostServiceProvider(IEnumerable<IVersionHost> versionHosts)
        {
            _versionHosts = new List<IVersionHost>(versionHosts);
        }

        #region IOleClientSite Members

        public void SaveObject()
        {
            // No-op.
        }

        public int GetMoniker(OLEGETMONIKER dwAssign, OLEWHICHMK dwWhichMoniker, out IMoniker ppmk)
        {
            ppmk = null;
            return HRESULT.E_NOTIMPL;
        }

        public int GetContainer(out IOleContainer ppContainer)
        {
            ppContainer = null;
            return HRESULT.E_NOINTERFACE;
        }

        public void ShowObject()
        {
            // No-op.
        }

        public void OnShowWindow(bool fShow)
        {
            // No-op.
        }

        public int RequestNewObjectLayout()
        {
            return HRESULT.E_NOTIMPL;
        }

        #endregion

        #region IServiceProviderRaw Members

        public int QueryService(ref Guid guid, ref Guid riid, out IntPtr service)
        {
            if (guid == VersionHostServiceId && riid == typeof(IVersionHost).GUID)
            {
                service = Marshal.GetComInterfaceForObject(this, typeof(IVersionHost));
                return HRESULT.S_OK;
            }

            service = IntPtr.Zero;
            return HRESULT.E_NOINTERFACE;
        }

        #endregion

        #region IVersionHost Members

        public void QueryUseLocalVersionVector(out bool pfUseLocal)
        {
            // This will force MSHTML to call our implementation of QueryVersionVector.
            pfUseLocal = true;
        }

        public void QueryVersionVector(IVersionVector pVersion)
        {
            _versionHosts.ForEach(host => host.QueryVersionVector(pVersion));
        }

        #endregion
    }
}
