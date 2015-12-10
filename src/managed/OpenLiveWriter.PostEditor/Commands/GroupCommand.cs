// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using System.Drawing;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    class GroupCommand : Command
    {
        private Command _representativeCommand;
        public GroupCommand(CommandId commandId, Command representativeCommand) : base(commandId)
        {
            Debug.Assert(representativeCommand != null, "Unexpected null command");
            _representativeCommand = representativeCommand;
            UpdateInvalidationState(PropertyKeys.SmallImage, InvalidationState.Pending);
            UpdateInvalidationState(PropertyKeys.SmallHighContrastImage, InvalidationState.Pending);
            UpdateInvalidationState(PropertyKeys.LargeImage, InvalidationState.Pending);
            UpdateInvalidationState(PropertyKeys.LargeHighContrastImage, InvalidationState.Pending);
        }

        public override Bitmap SmallImage
        {
            get { return _representativeCommand.SmallImage; }
            set { Debug.Fail("Setting properties on GroupCommand is not supported."); }
        }

        public override Bitmap SmallHighContrastImage
        {
            get { return _representativeCommand.SmallHighContrastImage; }
            set { Debug.Fail("Setting properties on GroupCommand is not supported."); }
        }

        public override Bitmap LargeImage
        {
            get { return _representativeCommand.LargeImage; }
            set { base.LargeImage = value; }
        }

        public override Bitmap LargeHighContrastImage
        {
            get { return _representativeCommand.LargeHighContrastImage; }
            set { Debug.Fail("Setting properties on GroupCommand is not supported."); }
        }
    }
}
