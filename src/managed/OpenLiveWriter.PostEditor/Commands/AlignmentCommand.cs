// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class AlignmentCommand : GalleryCommand<Alignment>
    {
        private Command _commandAlignmentGroup;

        public AlignmentCommand(CommandManager commandManager)
            : base(CommandId.AlignmentGallery, Alignment.None)
        {
            _commandAlignmentGroup = new GroupCommand(CommandId.AlignmentGroup, this);
            commandManager.Add(_commandAlignmentGroup);

            ExecuteWithArgs += new ExecuteEventHandler(AlignmentCommand_ExecuteWithArgs);

            // By default, we are disabled
            _commandAlignmentGroup.Enabled = false;
            base.Enabled = false;
        }

        void AlignmentCommand_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
        {
            int newSelectedIndex = args.GetInt(CommandId.ToString());
            SetSelectedItem(Items[newSelectedIndex].Cookie);

            if (AlignmentChanged != null)
                AlignmentChanged(this, EventArgs.Empty);
        }

        public event EventHandler AlignmentChanged;

        public override void LoadItems()
        {
            if (items.Count == 0)
            {
                items.Add(new TooltippedGalleryItem(Res.Get(StringId.ImgSBAlignInline), Res.Get(StringId.ImgSBAlignInline), Images.Alignment_Inline_48x48, Alignment.None));
                items.Add(new TooltippedGalleryItem(Res.Get(StringId.ImgSBAlignLeft), Res.Get(StringId.ImgSBAlignLeft), Images.Alignment_Left_48x48, Alignment.Left));
                items.Add(new TooltippedGalleryItem(Res.Get(StringId.ImgSBAlignCenter), Res.Get(StringId.ImgSBAlignCenter), Images.Alignment_Center_48x48, Alignment.Center));
                items.Add(new TooltippedGalleryItem(Res.Get(StringId.ImgSBAlignRight), Res.Get(StringId.ImgSBAlignRight), Images.Alignment_Right_48x48, Alignment.Right));
                base.LoadItems();
            }
        }

        public void SetAlignment(Alignment? alignment)
        {
            if (alignment.HasValue)
                SelectedItem = alignment.Value;
        }

        public override bool Enabled
        {
            set
            {
                if (value == false || Items.Count == 0)
                {
                    Invalidate();
                }

                _commandAlignmentGroup.Enabled = value;

                base.Enabled = value;
            }
        }
    }
}
