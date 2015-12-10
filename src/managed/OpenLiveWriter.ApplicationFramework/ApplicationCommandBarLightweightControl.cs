// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// A command bar styled for the top of the app.
    /// </summary>
    public class ApplicationCommandBarLightweightControl : CommandBarLightweightControl
    {
        private Bitmap _contextMenuArrowBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.WhiteDropArrow.png");
        private Bitmap _contextMenuArrowBitmapDisabled = ImageHelper.MakeDisabled(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.WhiteDropArrow.png"));

        public ApplicationCommandBarLightweightControl(IContainer container) : base(container)
        {
        }

        public ApplicationCommandBarLightweightControl()
        {
        }

        public ApplicationCommandBarLightweightControl(CommandManager commandManager)
            : this()
        {
            CommandManager = commandManager;
        }

        public override ButtonMargins? GetButtonMargins(ButtonFeatures features, bool rightAligned)
        {
            if (!rightAligned)
            {
                switch (features)
                {
                    case ButtonFeatures.Image:
                        return new ButtonMargins(6, 0, 0, 6, 0);
                    case ButtonFeatures.Text:
                        return new ButtonMargins(0, 15, 0, 15, 0);
                    case ButtonFeatures.Image | ButtonFeatures.Text:
                        return new ButtonMargins(15, 7, 0, 15, 0);
                    case ButtonFeatures.Text | ButtonFeatures.Menu:
                        return new ButtonMargins(0, 15, 5, 15, 0);
                    case ButtonFeatures.Text | ButtonFeatures.SplitMenu:
                        return new ButtonMargins(0, 15, 9, 5, 10);
                    case ButtonFeatures.Image | ButtonFeatures.Menu:
                        return new ButtonMargins(6, 0, 5, 6, 0);
                    default:
                        return null;
                }
            }
            else
            {
                switch (features)
                {
                    case ButtonFeatures.Text | ButtonFeatures.Menu:
                        return new ButtonMargins(0, 10, 5, 10, 0);
                    case ButtonFeatures.Image:
                        return new ButtonMargins(6, 0, 0, 6, 0);
                    case ButtonFeatures.Image | ButtonFeatures.Menu:
                        return new ButtonMargins(6, 0, 5, 6, 0);
                    default:
                        return null;
                }
            }
        }

        public override Color TextColor
        {
            get
            {
                return !UseHighContrastMode ? Color.FromArgb(53, 90, 136) : SystemColors.ControlText;
            }
        }

        public override int LeftLayoutMargin
        {
            get
            {
                return 0;
            }
        }

        public override int RightLayoutMargin
        {
            get
            {
                return 3;
            }
        }

        public override Color TopBevelFirstLineColor
        {
            get { return Color.Transparent; }
        }

        public override Color TopBevelSecondLineColor
        {
            get { return Color.Transparent; }
        }

        public override Color TopColor
        {
            get { return Color.Transparent; }
        }

        public override Color BottomColor
        {
            get { return Color.Transparent; }
        }

        public override Color BottomBevelFirstLineColor
        {
            get
            {
                return Color.Transparent;
            }
        }

        public override Color BottomBevelSecondLineColor
        {
            get
            {
                return Color.Transparent;
            }
        }

        public override Bitmap ContextMenuArrowBitmap
        {
            get { return _contextMenuArrowBitmap; }
        }

        public override Bitmap ContextMenuArrowBitmapDisabled
        {
            get { return _contextMenuArrowBitmapDisabled; }
        }
    }
}
