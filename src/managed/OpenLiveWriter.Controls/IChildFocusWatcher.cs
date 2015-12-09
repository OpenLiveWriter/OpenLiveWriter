// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Interface implemented by a ContainerControl that wishes to be notified when one of its
    /// child controls gains or loses focus.
    /// </summary>
    public interface IChildFocusWatcher
    {
        void ChildGotFocus(Control control);
        void ChildLostFocus(Control control);
    }
}
