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
    public class TextContextMenuDefinition : CommandContextMenuDefinition
    {
        public TextContextMenuDefinition() : this(false)
        {
        }

        public TextContextMenuDefinition(bool clipboardCommandsOnly)
        {
            Entries.Add(CommandId.Cut, false, false);
            Entries.Add(CommandId.CopyCommand, false, false);
            Entries.Add(CommandId.Paste, false, false);
            Entries.Add(CommandId.PasteSpecial, false, false);
            if (!clipboardCommandsOnly)
            {
                Entries.Add(CommandId.SelectAll, true, true);
                Entries.Add(CommandId.InsertLink, true, true);
                MenuDefinitionEntryPlaceholder alignPlaceholder = new MenuDefinitionEntryPlaceholder();
                alignPlaceholder.MenuPath = Res.Get(StringId.AlignMenuItem);
                Entries.Add(alignPlaceholder);
                alignPlaceholder.Entries.Add(CommandId.AlignLeft, false, false);
                alignPlaceholder.Entries.Add(CommandId.AlignCenter, false, false);
                alignPlaceholder.Entries.Add(CommandId.AlignRight, false, false);
                alignPlaceholder.Entries.Add(CommandId.Justify, false, false);
                Entries.Add(CommandId.Numbers, false, false);
                Entries.Add(CommandId.Bullets, false, false);
            }
        }
    }
}
