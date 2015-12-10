// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// SmartContentSources can implement IActiveSmartContentEditor, which means they can for trigger the event ForceContentEdited
    /// and the editor will update their content even if the smart content is not currently selected.  Be careful when using this event
    /// that you are indeed responding to a user action because it will drive focus to the smart content.  For example, this is used
    /// when the user has focus in the attachment well but clicks on a different template for the photo album.
    /// </summary>
    public interface IActiveSmartContentEditor
    {
        event EventHandler ForceContentEdited;
    }
}
