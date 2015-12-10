// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.HtmlEditor.Linking.Commands
{
    /// <summary>
    /// Summary description for LinkingCommand.
    /// </summary>
    public class LinkingCommand : Command
    {
        public LinkingCommand(CommandId commandId) : base(commandId)
        {
        }

        public virtual bool FindLink(string linkText, HyperlinkForm caller)
        {
            throw new NotImplementedException();
        }
    }
}
