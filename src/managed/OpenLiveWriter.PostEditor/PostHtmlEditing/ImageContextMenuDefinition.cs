// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for TextContextMenuDefinition.
    /// </summary>
    public class ImageContextMenuDefinition : CommandContextMenuDefinition
    {
        public ImageContextMenuDefinition()
        {
            Entries.Add(CommandId.Cut, false, false);
            Entries.Add(CommandId.CopyCommand, false, false);
            Entries.Add(CommandId.Paste, false, false);
            Entries.Add(CommandId.InsertLink, true, false);
        }
    }
}
