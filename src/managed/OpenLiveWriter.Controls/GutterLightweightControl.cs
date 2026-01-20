// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// NOTE: This class was referenced but never implemented in the original codebase.
// This is a stub implementation to allow the .NET 10 migration to proceed.

using System.ComponentModel;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Stub implementation of GutterLightweightControl.
    /// This class was referenced but never implemented in the original codebase.
    /// </summary>
    public class GutterLightweightControl : LightweightControl
    {
        public GutterLightweightControl()
        {
        }

        public GutterLightweightControl(IContainer container)
        {
            container?.Add(this);
        }
    }
}
