// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Threading;

namespace OpenLiveWriter.Mshtml
{
    public class DisplayServices
    {
        public static void TraceMoveToMarkupPointer(IDisplayPointerRaw displayPointer, MarkupPointer markupPointer)
        {
            try
            {
                if (displayPointer == null)
                    throw new ArgumentException("Unexpected null display pointer.");

                if (markupPointer == null)
                    throw new ArgumentException("Unexpected null markup pointer.");

                //position a display pointer on the same line as the markup pointer
                displayPointer.MoveToMarkupPointer(markupPointer.PointerRaw, null);
            }
            catch (Exception e)
            {
                Trace.Fail("Unexpected exception in TraceMoveToMarkupPointer: " + e.ToString());
                throw;
            }
        }
    }
}
