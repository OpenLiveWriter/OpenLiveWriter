// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Platform.Mac
{
    public class MacDisplayHelper : IDisplayHelper
    {
        private const int TWIPS_PER_INCH = 1440;

        // macOS uses 72 points per inch as the logical DPI
        public int DefaultDpi => 72;

        public float TwipsToPixelsX(int twips) => (float)twips * DefaultDpi / TWIPS_PER_INCH;
        public float TwipsToPixelsY(int twips) => (float)twips * DefaultDpi / TWIPS_PER_INCH;

        // macOS always uses compositing (Quartz)
        public bool IsCompositionEnabled() => true;
    }
}
