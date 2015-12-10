// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.CoreServices;
using System.Threading;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class BrightnessEditor : ImageDecoratorEditor
    {
        private TextBox textBoxBrightness;
        private TrackBar trackBarBrightness;
        private Label label1;
        private TextBox textBoxContrast;
        private TrackBar trackBarContrast;
        private Label label2;
        private Button btnCancel;
        private Button btnOK;
        private int _originalBrightness;
        private int _originalContrast;
        private Size _originalSize;

        private IContainer components = null;

        public BrightnessEditor()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.label1.Text = Res.Get(StringId.BrightnessBrightness);
            this.label2.Text = Res.Get(StringId.BrightnessContrast);
            this.btnCancel.Text = Res.Get(StringId.CancelButton);
            this.btnOK.Text = Res.Get(StringId.OKButtonText);
            Text = Res.Get(StringId.BrightnessAdjust);
            _originalSize = Size;

            CultureHelper.FixupTextboxForNumber(textBoxBrightness);
            CultureHelper.FixupTextboxForNumber(textBoxContrast);

            textBoxBrightness.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.BrightnessBrightness));
            textBoxContrast.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.BrightnessContrast));

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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxBrightness = new System.Windows.Forms.TextBox();
            this.trackBarBrightness = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxContrast = new System.Windows.Forms.TextBox();
            this.trackBarContrast = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarContrast)).BeginInit();
            this.SuspendLayout();
            //
            // textBoxBrightness
            //
            this.textBoxBrightness.AcceptsReturn = true;
            this.textBoxBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBrightness.Location = new System.Drawing.Point(228, 28);
            this.textBoxBrightness.Name = "textBoxBrightness";
            this.textBoxBrightness.Size = new System.Drawing.Size(32, 20);
            this.textBoxBrightness.TabIndex = 2;
            this.textBoxBrightness.Text = "0";
            this.textBoxBrightness.KeyDown += new System.Windows.Forms.KeyEventHandler(this.trackBarTextBox_KeyDown);
            this.textBoxBrightness.TextChanged += new System.EventHandler(this.textBoxBrightness_TextChanged);
            this.textBoxBrightness.Leave += new System.EventHandler(this.trackBarTextBox_Leave);
            this.textBoxBrightness.Enter += new System.EventHandler(this.textBox_Enter);
            //
            // trackBarBrightness
            //
            this.trackBarBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarBrightness.Location = new System.Drawing.Point(8, 24);
            this.trackBarBrightness.Maximum = 100;
            this.trackBarBrightness.Minimum = -100;
            this.trackBarBrightness.Name = "trackBarBrightness";
            this.trackBarBrightness.Size = new System.Drawing.Size(212, 45);
            this.trackBarBrightness.TabIndex = 1;
            this.trackBarBrightness.TickFrequency = 50;
            this.trackBarBrightness.KeyUp += new System.Windows.Forms.KeyEventHandler(this.trackBar_KeyUp);
            this.trackBarBrightness.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBar_MouseUp);
            this.trackBarBrightness.ValueChanged += new System.EventHandler(this.trackBarBrightness_ValueChanged);
            //
            // label1
            //
            this.label1.FlatStyle = FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Brightness:";
            //
            // textBoxContrast
            //
            this.textBoxContrast.AcceptsReturn = true;
            this.textBoxContrast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxContrast.Location = new System.Drawing.Point(228, 92);
            this.textBoxContrast.Name = "textBoxContrast";
            this.textBoxContrast.Size = new System.Drawing.Size(32, 20);
            this.textBoxContrast.TabIndex = 5;
            this.textBoxContrast.Text = "0";
            this.textBoxContrast.KeyDown += new System.Windows.Forms.KeyEventHandler(this.trackBarTextBox_KeyDown);
            this.textBoxContrast.TextChanged += new System.EventHandler(this.textBoxContrast_TextChanged);
            this.textBoxContrast.Leave += new System.EventHandler(this.trackBarTextBox_Leave);
            this.textBoxContrast.Enter += new System.EventHandler(this.textBox_Enter);
            //
            // trackBarContrast
            //
            this.trackBarContrast.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarContrast.Location = new System.Drawing.Point(8, 88);
            this.trackBarContrast.Maximum = 100;
            this.trackBarContrast.Minimum = -100;
            this.trackBarContrast.Name = "trackBarContrast";
            this.trackBarContrast.Size = new System.Drawing.Size(212, 45);
            this.trackBarContrast.TabIndex = 4;
            this.trackBarContrast.TickFrequency = 50;
            this.trackBarContrast.KeyUp += new System.Windows.Forms.KeyEventHandler(this.trackBar_KeyUp);
            this.trackBarContrast.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBar_MouseUp);
            this.trackBarContrast.ValueChanged += new System.EventHandler(this.trackBarContrast_ValueChanged);
            //
            // label2
            //
            this.label2.FlatStyle = FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(8, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Contrast:";
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(186, 148);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            //
            // btnOK
            //
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(105, 148);
            this.btnOK.Name = "btnOK";
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            //
            // BrightnessEditor
            //
            this.Controls.Add(this.textBoxContrast);
            this.Controls.Add(this.trackBarContrast);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxBrightness);
            this.Controls.Add(this.trackBarBrightness);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Name = "BrightnessEditor";
            this.Size = new System.Drawing.Size(268, 174);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarContrast)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        protected override void LoadEditor()
        {
            base.LoadEditor();
            BrightnessSettings = new BrightnessDecoratorSettings(Settings);

            float brightness = BrightnessSettings.Brightness * 100;
            trackBarBrightness.Value = (int)brightness;
            _originalBrightness = (int)brightness;

            float contrast = (100.0f * BrightnessSettings.Contrast) - 100;
            trackBarContrast.Value = (int)contrast;
            _originalContrast = (int)contrast;
        }
        private BrightnessDecoratorSettings BrightnessSettings;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ParentForm.AcceptButton = this.btnOK;
            ParentForm.CancelButton = this.btnCancel;
            ParentForm.Closing += new CancelEventHandler(ParentForm_Closing);

            LayoutHelper.FixupOKCancel(btnOK, btnCancel);
            LayoutHelper.NaturalizeHeight(label1, label2);
        }

        private void ParentForm_Closing(object sender, CancelEventArgs e)
        {
            //undo changes if user hits cancel, esc, or the red X
            if (ParentForm.DialogResult == DialogResult.Cancel)
            {
                trackBarBrightness.Value = _originalBrightness;
                trackBarContrast.Value = _originalContrast;
                SaveSettingsAndApplyDecorator();
            }
        }

        public override Size GetPreferredSize()
        {
            return _originalSize;
        }

        private void trackBar_MouseUp(object sender, MouseEventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }
        private void trackBar_KeyUp(object sender, KeyEventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }

        protected override void OnSaveSettings()
        {
            base.OnSaveSettings();
            float newBrightness = trackBarBrightness.Value / 100f;
            if (BrightnessSettings.Brightness != newBrightness)
            {
                BrightnessSettings.Brightness = newBrightness;
            }

            float newContrast = (float)(100.0 + trackBarContrast.Value) / 100.0f;
            if (BrightnessSettings.Contrast != newContrast)
            {
                BrightnessSettings.Contrast = newContrast;
            }
        }

        private void trackBarBrightness_ValueChanged(object sender, EventArgs e)
        {
            string text = NumberHelper.IntToString(trackBarBrightness.Value);
            if (textBoxBrightness.Text != text)
                textBoxBrightness.Text = text;
        }

        private void trackBarContrast_ValueChanged(object sender, EventArgs e)
        {
            string text = NumberHelper.IntToString(trackBarContrast.Value);
            if (textBoxContrast.Text != text)
                textBoxContrast.Text = text;
        }

        private void textBoxBrightness_TextChanged(object sender, EventArgs e)
        {
            trackbarTextBox_TextChanged(textBoxBrightness, trackBarBrightness);
        }

        private void textBoxContrast_TextChanged(object sender, EventArgs e)
        {
            trackbarTextBox_TextChanged(textBoxContrast, trackBarContrast);
        }

        private void trackbarTextBox_TextChanged(TextBox textBox, TrackBar trackbar)
        {
            int val = 0;
            try
            {
                val = Int32.Parse(textBox.Text.Trim(), CultureInfo.CurrentCulture);
                if (val > 100)
                {
                    val = 100;
                    textBox.Text = val.ToString(CultureInfo.CurrentCulture);
                }
                if (val < -100)
                {
                    val = -100;
                    textBox.Text = val.ToString(CultureInfo.CurrentCulture);
                }
                if (trackbar.Value != val)
                    trackbar.Value = val;
            }
            catch (Exception)
            {
                //illegal int value detected, assume the user is still typing.
            }
        }

        private void trackBarTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveSettingsAndApplyDecorator();
                e.Handled = true; //prevent the control from inserting a newline
            }
        }

        private void trackBarTextBox_Leave(object sender, EventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }

        private void textBox_Enter(object sender, System.EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }

    }
}

