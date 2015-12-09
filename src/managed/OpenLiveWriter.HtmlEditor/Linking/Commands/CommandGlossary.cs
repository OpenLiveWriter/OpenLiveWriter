// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.HtmlEditor.Linking.Commands
{
    /// <summary>
    /// Summary description for CommandGlossary.
    /// </summary>
    public class CommandGlossary : LinkingCommand
    {
        public CommandGlossary() : base(CommandId.Glossary)
        {
        }

        public override bool FindLink(string linkText, HyperlinkForm caller)
        {
            using (SelectGlossaryLinkForm glossaryForm = new SelectGlossaryLinkForm())
            {
                if (linkText != String.Empty)
                {
                    glossaryForm.SetSelected(linkText);
                }
                if (DialogResult.OK == glossaryForm.ShowDialog())
                {
                    GlossaryLinkItem chosen = glossaryForm.SelectedEntry;
                    caller.LinkText = chosen.Text;
                    caller.Hyperlink = chosen.Url;
                    caller.LinkTitle = chosen.Title;
                    caller.IsInGlossary = true;
                    return true;
                }
            }
            return false;
        }
    }
}
