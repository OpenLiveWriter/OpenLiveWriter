// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public partial class TiltDecoratorEditor : ImageDecoratorEditor
    {
        private readonly Size _originalSize = new Size(0, 0);
        private int _originalTilt = 0;
        private TiltDecoratorSettings TiltSettings = null;

        public TiltDecoratorEditor()
        {
            InitializeComponent();
            _originalSize = Size;
            Text = Res.Get(StringId.TiltEditorTitle);
            CultureHelper.FixupTextboxForNumber(textBoxTilt);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ParentForm.AcceptButton = btnOK;
            ParentForm.CancelButton = btnCancel;

            btnCancel.Text = Res.Get(StringId.CancelButton);
            btnOK.Text = Res.Get(StringId.OKButtonText);
            labelTilt.Text = Res.Get(StringId.TiltEditorLabel);

            ParentForm.Closing += ParentForm_Closing;

            LayoutHelper.FixupOKCancel(btnOK, btnCancel);

            textBoxTilt.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.TiltEditorLabel));
        }

        private void ParentForm_Closing(object sender, CancelEventArgs e)
        {
            //undo changes if user hits cancel, esc, or the red X
            if (ParentForm.DialogResult == DialogResult.Cancel)
            {
                trackBarTilt.Value = _originalTilt;
                SaveSettingsAndApplyDecorator();
            }
        }

        protected override void LoadEditor()
        {
            base.LoadEditor();

            TiltSettings = new TiltDecoratorSettings(Settings);

            _originalTilt = TiltSettings.TiltDegrees;
            trackBarTilt.Value = TiltSettings.TiltDegrees;
        }

        public override Size GetPreferredSize()
        {
            return _originalSize;
        }

        protected override void OnSaveSettings()
        {
            base.OnSaveSettings();
            int newTilt = trackBarTilt.Value;
            if (newTilt != TiltSettings.TiltDegrees)
                TiltSettings.TiltDegrees = newTilt;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }

        private void trackBarTilt_ValueChanged(object sender, EventArgs e)
        {
            string text = NumberHelper.IntToString(trackBarTilt.Value);
            if (textBoxTilt.Text != text)
                textBoxTilt.Text = text;
        }

        private void trackBarTilt_MouseUp(object sender, MouseEventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }
        private void trackBarTilt_KeyUp(object sender, KeyEventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }

        private void textBoxTilt_TextChanged(object sender, EventArgs e)
        {
            int val = 0;
            try
            {
                val = Int32.Parse(textBoxTilt.Text.Trim(), CultureInfo.CurrentCulture);
                if (val > trackBarTilt.Maximum)
                {
                    val = trackBarTilt.Maximum;
                    textBoxTilt.Text = val.ToString(CultureInfo.CurrentCulture);
                }
                if (val < trackBarTilt.Minimum)
                {
                    val = trackBarTilt.Minimum;
                    textBoxTilt.Text = val.ToString(CultureInfo.CurrentCulture);
                }
                if (trackBarTilt.Value != val)
                    trackBarTilt.Value = val;
            }
            catch (Exception)
            {
                //illegal int value detected, assume the user is still typing.
            }
        }

        private void textBoxTilt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveSettingsAndApplyDecorator();
                e.Handled = true; //prevent the control from inserting a newline
            }
        }

        private void textBoxTilt_Leave(object sender, EventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }
    }
}
