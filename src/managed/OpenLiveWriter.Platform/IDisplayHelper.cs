// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Display/DPI helper abstraction.
    /// </summary>
    public interface IDisplayHelper
    {
        int DefaultDpi { get; }
        float TwipsToPixelsX(int twips);
        float TwipsToPixelsY(int twips);
        bool IsCompositionEnabled();
    }
}
