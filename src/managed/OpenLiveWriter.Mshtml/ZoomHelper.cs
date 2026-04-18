// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Helper class for editor zoom level calculations.
    /// </summary>
    public static class ZoomHelper
    {
        /// <summary>
        /// Standard zoom percentage levels matching browser zoom behavior.
        /// </summary>
        public static readonly int[] ZoomLevels = { 25, 33, 50, 67, 75, 80, 90, 100, 110, 125, 150, 175, 200, 250, 300, 400, 500 };

        /// <summary>
        /// Gets the next zoom level in the given direction from the predefined zoom steps.
        /// </summary>
        /// <param name="currentZoom">The current zoom percentage.</param>
        /// <param name="zoomIn">True to zoom in (increase), false to zoom out (decrease).</param>
        /// <returns>The next zoom percentage in the requested direction.</returns>
        public static int GetNextZoomLevel(int currentZoom, bool zoomIn)
        {
            if (zoomIn)
            {
                // Find the next level above current zoom
                for (int i = 0; i < ZoomLevels.Length; i++)
                {
                    if (ZoomLevels[i] > currentZoom)
                        return ZoomLevels[i];
                }
                return ZoomLevels[ZoomLevels.Length - 1]; // Already at max
            }
            else
            {
                // Find the next level below current zoom
                for (int i = ZoomLevels.Length - 1; i >= 0; i--)
                {
                    if (ZoomLevels[i] < currentZoom)
                        return ZoomLevels[i];
                }
                return ZoomLevels[0]; // Already at min
            }
        }
    }
}
