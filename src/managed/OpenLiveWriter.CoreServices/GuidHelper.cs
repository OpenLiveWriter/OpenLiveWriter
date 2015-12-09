// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public class GuidHelper
    {
        /// <summary>
        /// This actually has nothing to do with GUIDs.
        /// </summary>
        /// <returns>A random 8-digit hex number</returns>
        public static string GetVeryShortGuid()
        {
            return ((uint)new Random().Next()).ToString("X8", CultureInfo.InvariantCulture);
        }
    }
}
