// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class AnchorContextMenuDefinition : CommandContextMenuDefinition
    {
        public AnchorContextMenuDefinition(bool supportsAddToGlossary)
        {
            Entries.Add(CommandId.OpenLink, false, false);
            Entries.Add(CommandId.EditLink, false, false);
            Entries.Add(CommandId.RemoveLink, false, false);
            if (supportsAddToGlossary)
                Entries.Add(CommandId.AddToGlossary, true, false);
        }
    }
}
