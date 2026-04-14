// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using mshtml;
using OpenLiveWriter.Interop.Com;

namespace OpenLiveWriter.CoreServices
{
    public class DisableVmlVersionHost : IVersionHost
    {
        #region IVersionHost Members

        public void QueryUseLocalVersionVector(out bool pfUseLocal)
        {
            pfUseLocal = true;
        }

        public void QueryVersionVector(IVersionVector pVersion)
        {
            if (pVersion == null)
            {
                throw new ArgumentNullException("pVersion");
            }

            // This tells MSHTML that we don't want to support VML.
            pVersion.SetVersion("VML", null);
        }

        #endregion
    }
}
