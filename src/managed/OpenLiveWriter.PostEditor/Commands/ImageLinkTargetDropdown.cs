// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class ImageLinkTargetDropdown : MutuallyExclusiveDropdown<LinkTargetType>
    {
        public ImageLinkTargetDropdown(Command dropDownCommand, Command[] commands, EventHandler executeHandler)
            : base(dropDownCommand, commands, executeHandler)
        {
        }

        private CommandId _lastSelectedCommandId = CommandId.None;
        protected override void OnExecute(Command executedCommand)
        {
            _lastSelectedCommandId = executedCommand.CommandId;
            base.OnExecute(executedCommand);
        }

        protected override void SelectCommand(CommandId commandId)
        {
            _lastSelectedCommandId = commandId;
            base.SelectCommand(commandId);
        }

        public override void UpdateDropdown(Command selectedCommand)
        {
            Debug.Assert(selectedCommand.Tag is LinkTargetType);
            LinkTargetType linkTargetType = (LinkTargetType)selectedCommand.Tag;

            switch (linkTargetType)
            {
                case LinkTargetType.IMAGE:
                    DropDownCommand.LabelTitle = Res.Get(StringId.LinkToSourceLabelTitle);
                    break;
                case LinkTargetType.URL:
                    DropDownCommand.LabelTitle = Res.Get(StringId.LinkToURLLabelTitle);
                    break;
                case LinkTargetType.NONE:
                    DropDownCommand.LabelTitle = Res.Get(StringId.LinkToNoneLabelTitle);
                    break;
                default:
                    Debug.Fail("Unexpected LinkTargetType.");
                    break;
            }
        }
    }
}
