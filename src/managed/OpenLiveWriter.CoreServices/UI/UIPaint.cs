// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.CoreServices.UI
{
    /// <summary>
    /// Summary description for UIPaint.
    /// </summary>
    public class UIPaint
    {
        private static readonly UIPaint _instance = new UIPaint();
        public static UIPaint Instance
        {
            get
            {
                return _instance;
            }
        }

        private UIPaint()
        {
        }

        public Color FrameGradientLight
        {
            get { return Color.FromArgb(236, 246, 250); }
        }
    }
}
