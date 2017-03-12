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
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for CopyrightEditor.
    /// </summary>
    public class WatermarkEditor : ImageDecoratorEditor
    {
        private string _fontFamilyOriginal;
        private int _fontSizeOriginal;
        private WatermarkDecorator.WatermarkPosition _positionOriginal;
        private string _textOriginal;
        private Label labelText;
        private TextBox textBoxText;
        private ComboBox comboBoxFontFamily;
        private Label labelFontFamily;
        private ComboBox comboBoxSize;
        private Label labelSize;
        private ComboBox comboBoxPosition;
        private Label labelPosition;
        private Button buttonOK;
        private Button buttonCancel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WatermarkEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.RightToLeft = BidiHelper.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;

            this.labelText.Text = Res.Get(StringId.WatermarkTextLabel);
            this.labelFontFamily.Text = Res.Get(StringId.WatermarkFontFamilyLabel);
            this.labelSize.Text = Res.Get(StringId.WatermarkSizeLabel);
            this.labelPosition.Text = Res.Get(StringId.WatermarkPositionLabel);

            AddSizes();
            AddFonts();
            AddPositions();

            textBoxText.LostFocus += new EventHandler(textBoxText_LostFocus);
            textBoxText.KeyDown += new KeyEventHandler(textBoxText_KeyDown);

            keyTimer = new Timer();
            keyTimer.Interval = 100;
            keyTimer.Tick += new EventHandler(keyTimer_Tick);
            keyTimer.Start();

            Text = Res.Get(StringId.WatermarkDialogTitle);

            buttonOK.Text = Res.Get(StringId.OKButtonText);
            buttonCancel.Text = Res.Get(StringId.CancelButton);
            buttonCancel.Click += new EventHandler(buttonCancel_Click);
        }

        void buttonCancel_Click(object sender, EventArgs e)
        {
            this.textBoxText.Text = _textOriginal;
            SelectInCombo(comboBoxSize, _fontSizeOriginal.ToString(CultureInfo.CurrentCulture));
            SelectInCombo(comboBoxPosition, GetPrettyName(_positionOriginal));
            SelectInCombo(comboBoxFontFamily, _fontFamilyOriginal);
            SaveSettingsAndApplyDecorator(true);
        }

        protected override void LoadEditor()
        {
            base.LoadEditor();
            WatermarkSettings = new WatermarkDecorator.WatermarkDecoratorSettings(Settings);

            _fontFamilyOriginal = WatermarkSettings.FontFamily;
            _fontSizeOriginal = WatermarkSettings.FontSize;
            _positionOriginal = WatermarkSettings.Position;
            _textOriginal = WatermarkSettings.Text;

            textBoxText.Text = string.IsNullOrEmpty(WatermarkSettings.Text) ? String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.WatermarkDefaultText), DateTime.Now) : WatermarkSettings.Text;

            SelectInCombo(comboBoxSize, WatermarkSettings.FontSize.ToString(CultureInfo.CurrentCulture));
            SelectInCombo(comboBoxPosition, GetPrettyName(WatermarkSettings.Position));
            SelectInCombo(comboBoxFontFamily, WatermarkSettings.FontFamily);
        }

        protected override void OnEditorLoaded()
        {
            SaveSettingsAndApplyDecorator();
        }

        private void AddSizes()
        {
            comboBoxSize.Items.AddRange(new object[] { 8, 10, 12, 14, 18, 24, 36 });
        }

        private void AddFonts()
        {
            foreach (FontFamily family in FontFamily.Families)
                if (DoesFontWork(family.Name))
                    comboBoxFontFamily.Items.Add(family.Name);
        }

        private static bool DoesFontWork(string font)
        {
            string[] brokenFonts = new string[] { "Brush Script MT", "Aharoni", "Berlin Sans FB Demi", "Harlow Solid Italic", "Magneto", "Monotype Corsiva", "Palace Script MT", "Vivaldi" };
            foreach (string fontName in brokenFonts)
                if (fontName == font)
                    return false;
            return true;
        }

        private void AddPositions()
        {
            this.comboBoxPosition.Items.Add(GetPrettyName(WatermarkDecorator.WatermarkPosition.BottomLeft));
            this.comboBoxPosition.Items.Add(GetPrettyName(WatermarkDecorator.WatermarkPosition.BottomRight));
            this.comboBoxPosition.Items.Add(GetPrettyName(WatermarkDecorator.WatermarkPosition.Centered));
            this.comboBoxPosition.Items.Add(GetPrettyName(WatermarkDecorator.WatermarkPosition.TopLeft));
            this.comboBoxPosition.Items.Add(GetPrettyName(WatermarkDecorator.WatermarkPosition.TopRight));
        }

        private string GetPrettyName(WatermarkDecorator.WatermarkPosition position)
        {
            switch (position)
            {
                case (WatermarkDecorator.WatermarkPosition.BottomLeft):
                    return Res.Get(StringId.WatermarkAlignBottomLeft);
                case (WatermarkDecorator.WatermarkPosition.BottomRight):
                    return Res.Get(StringId.WatermarkAlignBottomRight);
                case (WatermarkDecorator.WatermarkPosition.Centered):
                    return Res.Get(StringId.WatermarkAlignCentered);
                case (WatermarkDecorator.WatermarkPosition.TopLeft):
                    return Res.Get(StringId.WatermarkAlignTopLeft);
                case (WatermarkDecorator.WatermarkPosition.TopRight):
                    return Res.Get(StringId.WatermarkAlignTopRight);
            }
            return Res.Get(StringId.WatermarkAlignUnknown);
        }

        private WatermarkDecorator.WatermarkPosition GetEnumFromName(string name)
        {
            if (name == Res.Get(StringId.WatermarkAlignBottomLeft))
                return WatermarkDecorator.WatermarkPosition.BottomLeft;
            else if (name == Res.Get(StringId.WatermarkAlignBottomRight))
                return WatermarkDecorator.WatermarkPosition.BottomRight;
            else if (name == Res.Get(StringId.WatermarkAlignTopLeft))
                return WatermarkDecorator.WatermarkPosition.TopLeft;
            else if (name == Res.Get(StringId.WatermarkAlignTopRight))
                return WatermarkDecorator.WatermarkPosition.TopRight;
            else if (name == Res.Get(StringId.WatermarkAlignCentered))
                return WatermarkDecorator.WatermarkPosition.Centered;
            return WatermarkDecorator.WatermarkPosition.BottomRight;
        }

        private void SelectInCombo(ComboBox combo, string text)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (text == combo.GetItemText(combo.Items[i]))
                {
                    combo.SelectedIndex = i;
                    break;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Form form = FindForm();
            form.AcceptButton = buttonOK;
            form.CancelButton = buttonCancel;

            LayoutHelper.NaturalizeHeightAndDistribute(3, labelText, textBoxText);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelFontFamily, comboBoxFontFamily);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelSize, comboBoxSize);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelPosition, comboBoxPosition);
            LayoutHelper.NaturalizeHeightAndDistribute(10, new ControlGroup(labelText, textBoxText), new ControlGroup(labelFontFamily, comboBoxFontFamily), new ControlGroup(labelSize, comboBoxSize), new ControlGroup(labelPosition, comboBoxPosition));

            LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
        }

        private WatermarkDecorator.WatermarkDecoratorSettings WatermarkSettings;

        public override Size GetPreferredSize()
        {
            return this.Size;
        }

        protected override void OnSaveSettings()
        {
            base.OnSaveSettings();
            WatermarkSettings.Text = textBoxText.Text;
            WatermarkSettings.FontSize = Int32.Parse(comboBoxSize.GetItemText(comboBoxSize.SelectedItem), CultureInfo.CurrentCulture);
            WatermarkSettings.Position = GetEnumFromName(comboBoxPosition.GetItemText(comboBoxPosition.SelectedItem));
            WatermarkSettings.FontFamily = comboBoxFontFamily.GetItemText(comboBoxFontFamily.SelectedItem);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            keyTimer.Tick -= new EventHandler(keyTimer_Tick);
            keyTimer.Dispose();
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
            this.labelText = new System.Windows.Forms.Label();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.comboBoxFontFamily = new System.Windows.Forms.ComboBox();
            this.labelFontFamily = new System.Windows.Forms.Label();
            this.comboBoxSize = new System.Windows.Forms.ComboBox();
            this.comboBoxPosition = new System.Windows.Forms.ComboBox();
            this.labelSize = new System.Windows.Forms.Label();
            this.labelPosition = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // labelText
            //
            this.labelText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelText.Location = new System.Drawing.Point(8, 2);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(193, 13);
            this.labelText.TabIndex = 0;
            this.labelText.Text = "&Watermark text:";
            //
            // textBoxText
            //
            this.textBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxText.Location = new System.Drawing.Point(8, 20);
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(193, 20);
            this.textBoxText.TabIndex = 5;
            //
            // comboBoxFontFamily
            //
            this.comboBoxFontFamily.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontFamily.Location = new System.Drawing.Point(8, 70);
            this.comboBoxFontFamily.Name = "comboBoxFontFamily";
            this.comboBoxFontFamily.Size = new System.Drawing.Size(193, 21);
            this.comboBoxFontFamily.TabIndex = 8;
            this.comboBoxFontFamily.SelectedIndexChanged += new System.EventHandler(this.comboBoxFontFamily_SelectedIndexChanged);
            //
            // labelFontFamily
            //
            this.labelFontFamily.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelFontFamily.Location = new System.Drawing.Point(8, 51);
            this.labelFontFamily.Name = "labelFontFamily";
            this.labelFontFamily.Size = new System.Drawing.Size(204, 16);
            this.labelFontFamily.TabIndex = 7;
            this.labelFontFamily.Text = "&Font family:";
            //
            // comboBoxSize
            //
            this.comboBoxSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSize.Location = new System.Drawing.Point(8, 121);
            this.comboBoxSize.Name = "comboBoxSize";
            this.comboBoxSize.Size = new System.Drawing.Size(50, 21);
            this.comboBoxSize.TabIndex = 12;
            this.comboBoxSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxSize_SelectedIndexChanged);
            //
            // comboBoxPosition
            //
            this.comboBoxPosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPosition.Location = new System.Drawing.Point(8, 170);
            this.comboBoxPosition.Name = "comboBoxPosition";
            this.comboBoxPosition.Size = new System.Drawing.Size(193, 21);
            this.comboBoxPosition.TabIndex = 15;
            this.comboBoxPosition.SelectedIndexChanged += new System.EventHandler(this.comboBoxPosition_SelectedIndexChanged);
            //
            // labelSize
            //
            this.labelSize.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSize.Location = new System.Drawing.Point(8, 103);
            this.labelSize.Name = "labelSize";
            this.labelSize.Size = new System.Drawing.Size(193, 13);
            this.labelSize.TabIndex = 11;
            this.labelSize.Text = "&Size:";
            //
            // labelPosition
            //
            this.labelPosition.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPosition.Location = new System.Drawing.Point(8, 153);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(193, 13);
            this.labelPosition.TabIndex = 13;
            this.labelPosition.Text = "&Position:";
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(47, 208);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 16;
            this.buttonOK.Text = "button1";
            this.buttonOK.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(126, 208);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 17;
            this.buttonCancel.Text = "button2";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // WatermarkEditor
            //
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelPosition);
            this.Controls.Add(this.labelSize);
            this.Controls.Add(this.comboBoxPosition);
            this.Controls.Add(this.comboBoxSize);
            this.Controls.Add(this.labelFontFamily);
            this.Controls.Add(this.comboBoxFontFamily);
            this.Controls.Add(this.textBoxText);
            this.Controls.Add(this.labelText);
            this.Name = "WatermarkEditor";
            this.Size = new System.Drawing.Size(208, 238);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void textBoxText_LostFocus(object sender, EventArgs e)
        {
            if (textBoxText.Text != WatermarkSettings.Text)
                SaveSettingsAndApplyDecorator();
        }

        private void textBoxText_KeyDown(object sender, KeyEventArgs e)
        {
            nextRefresh = DateTime.UtcNow.AddMilliseconds(500);
            if (e.KeyCode == Keys.Enter && textBoxText.Text != WatermarkSettings.Text)
                SaveSettingsAndApplyDecorator();
        }
        private DateTime nextRefresh = DateTime.MaxValue;

        private void comboBoxSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }

        private void comboBoxPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }

        private void comboBoxFontFamily_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }

        private Timer keyTimer;

        private void keyTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.UtcNow > nextRefresh)
            {
                SaveSettingsAndApplyDecorator();
                nextRefresh = DateTime.MaxValue;
            }
        }
    }
}
