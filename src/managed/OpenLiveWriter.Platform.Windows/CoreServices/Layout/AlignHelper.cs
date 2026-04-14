// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices.Layout
{
    public class AlignHelper
    {
        public static void AlignBottom(params Control[] controls)
        {
            int maxBottom = int.MinValue;
            foreach (Control c in controls)
            {
                maxBottom = Math.Max(maxBottom, c.Bottom);
            }

            foreach (Control c in controls)
            {
                c.Top = maxBottom - c.Height;
            }
        }
    }
}
