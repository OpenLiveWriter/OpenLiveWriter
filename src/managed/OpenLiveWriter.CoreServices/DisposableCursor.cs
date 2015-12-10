// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Helper class that enables you to easily install and guarantee the removal
    /// of a cursor within a using statement
    /// </summary>
    /// <example>
    ///  using ( new DisposableCursor(Cursors.WaitCursor) )
    ///		PerformLongRunningOperation() ;
    /// </example>
    public class DisposableCursor : IDisposable
    {
        public DisposableCursor(Cursor cursor)
        {
            previousCursor = Cursor.Current;
            Cursor.Current = cursor;
        }

        public void Dispose()
        {
            Cursor.Current = previousCursor;
            previousCursor = null;
        }

        /// <summary>
        /// A backup of the cursor that was active before this cursor was assigned (allows for proper nesting
        /// of cursors).
        /// </summary>
        Cursor previousCursor;
    }

    // subclasses for commonly used cursor types
    public class WaitCursor : DisposableCursor { public WaitCursor() : base(Cursors.WaitCursor) { } }
    public class AppStartingCursor : DisposableCursor { public AppStartingCursor() : base(Cursors.AppStarting) { } }

}
