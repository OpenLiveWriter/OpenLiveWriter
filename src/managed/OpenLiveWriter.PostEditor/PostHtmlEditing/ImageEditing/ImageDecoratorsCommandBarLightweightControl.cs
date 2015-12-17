// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageDecoratorsCommandBarLightweightControl.
    /// </summary>
    public class ImageDecoratorsCommandBarLightweightControl : CommandBarLightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the MindShareToolBandCommandBarLightweightControl.
        /// </summary>
        public ImageDecoratorsCommandBarLightweightControl(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

            //
            //
            //
        }

        /// <summary>
        /// Initializes a new instance of the MindShareToolBandCommandBarLightweightControl.
        /// </summary>
        public ImageDecoratorsCommandBarLightweightControl()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        /// <summary>
        /// Gets the top layout margin.
        /// </summary>
        public override int TopLayoutMargin
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the left layout margin.
        /// </summary>
        public override int LeftLayoutMargin
        {
            get
            {
                return -10;
            }
        }

        /// <summary>
        /// Gets the bottom layout margin.
        /// </summary>
        public override int BottomLayoutMargin
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the right layout margin.
        /// </summary>
        public override int RightLayoutMargin
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the separator layout margin.
        /// </summary>
        public override int SeparatorLayoutMargin
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        ///	Gets the top command bar color.
        /// </summary>
        public override Color TopColor
        {
            get
            {
                return ColorizedResources.Instance.SidebarGradientBottomColor;
            }
        }

        /// <summary>
        ///	Gets the bottom command bar color.
        /// </summary>
        public override Color BottomColor
        {
            get
            {
                return TopColor;
            }
        }

        /// <summary>
        ///	Gets the top bevel first line color.
        /// </summary>
        public override Color TopBevelFirstLineColor
        {
            get
            {
                return TopColor;
            }
        }

        /// <summary>
        ///	Gets the top bevel second line color.
        /// </summary>
        public override Color TopBevelSecondLineColor
        {
            get
            {
                return TopColor;
            }
        }

        /// <summary>
        ///	Gets the bottom bevel first line color.
        /// </summary>
        public override Color BottomBevelFirstLineColor
        {
            get
            {
                return TopColor;
            }
        }

        /// <summary>
        ///	Gets the bottom bevel second line color.
        /// </summary>
        public override Color BottomBevelSecondLineColor
        {
            get
            {
                return TopColor;
            }
        }

        /// <summary>
        ///	Gets the text color.
        /// </summary>
        public override Color TextColor
        {
            get
            {
                return !UseHighContrastMode ? SystemColors.WindowText : SystemColors.ControlText;
            }
        }

        /// <summary>
        ///	Gets the text color.
        /// </summary>
        public override Color DisabledTextColor
        {
            get
            {
                return SystemColors.GrayText;
            }
        }
    }
}
