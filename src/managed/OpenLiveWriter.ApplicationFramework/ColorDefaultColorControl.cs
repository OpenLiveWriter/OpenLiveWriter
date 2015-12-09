// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Text;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using System.Security.Permissions;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Summary description for ColorDefaultColorControl.
    /// </summary>
    public class ColorDefaultColorControl : IColorPickerSubControl
    {
        private bool selected = false;

        public ColorDefaultColorControl(ColorSelectedEventHandler colorSelected, NavigateEventHandler navigate) : base(colorSelected, navigate)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.Text = Res.Get(StringId.ColorPickerDefaultColor);
            Color = Color.Empty;
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // ColorDefaultColorControl
            //
            this.Name = "ColorDefaultColorControl";
            this.Size = new System.Drawing.Size(108, 24);
        }

        #endregion

        public override Color Color
        {
            get
            {
                return Color.Empty;
            }

            set
            {
                selected = (value == Color.Empty);
            }
        }

    }
}
