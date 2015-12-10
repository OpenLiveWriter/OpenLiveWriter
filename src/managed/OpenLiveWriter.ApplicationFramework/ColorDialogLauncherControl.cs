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
using OpenLiveWriter.CoreServices;
using System.Security.Permissions;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Summary description for ColorDialogLauncherControl.
    /// </summary>
    public class ColorDialogLauncherControl : IColorPickerSubControl
    {
        public event EventHandler Close;

        public ColorDialogLauncherControl(ColorSelectedEventHandler colorSelected, NavigateEventHandler navigate) : base(colorSelected, navigate)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.Text = Res.Get(StringId.ColorPickerMoreColors);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // ColorDialogLauncherControl
            //
            this.Name = "ColorDialogLauncherControl";
            this.Size = new System.Drawing.Size(108, 24);
            this.Text = "&More Colors…";
        }
        #endregion

        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessMnemonic(char charCode)
        {
            if (IsMnemonic(charCode, this.Text))
            {
                ShowColorDialog();
                return true;
            }
            return base.ProcessMnemonic(charCode);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ShowColorDialog();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' | e.KeyChar == ' ')
            {
                ShowColorDialog();
                e.Handled = true;
            }
        }

        private void ShowColorDialog()
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.FullOpen = true;
                colorDialog.CustomColors = ApplicationEnvironment.CustomColors;
                if (Color != Color.Empty)
                    colorDialog.Color = Color;
                else
                    colorDialog.Color = Color.Black;

                Close(this, EventArgs.Empty);
                DialogResult result = colorDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    ApplicationEnvironment.CustomColors = colorDialog.CustomColors;
                    ColorSelected(new ColorSelectedEventArgs(colorDialog.Color));
                }
            }
        }
    }
}
