// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlEditor.Linking;
using OpenLiveWriter.HtmlEditor.Linking.Commands;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.OpenPost
{
    /// <summary>
    /// Summary description for CommandRecentPost.
    /// </summary>
    public class CommandRecentPost : LinkingCommand
    {
        public CommandRecentPost() : base(CommandId.RecentPost)
        {
        }

        public override bool FindLink(string linkText, HyperlinkForm caller)
        {
            using (SelectPostLinkForm openPostForm = new SelectPostLinkForm())
            {
                if (openPostForm.ShowDialog(Win32WindowImpl.ForegroundWin32Window) == DialogResult.OK)
                {
                    if (String.Empty == caller.LinkText.Trim())
                        caller.LinkText = openPostForm.PostTitle;
                    caller.Hyperlink = openPostForm.PostLink;
                    return true;
                }
            }
            return false;
        }
    }
}
