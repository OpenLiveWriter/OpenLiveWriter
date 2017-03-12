// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls.MarginUtil;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{

    public class HtmlMarginEditor : ImageDecoratorEditor
    {
        private IContainer components = null;
        private Panel panelCustomMargin;
        private NumericUpDown numericMarginTop;
        private NumericUpDown numericMarginRight;
        private NumericUpDown numericMarginBottom;
        private NumericUpDown numericMarginLeft;
        private Label label3;
        private Label label4;
        private Label label5;
        private MarginsComboBox comboBoxMargins;
        private Label label6;

        private MarginCommand marginCommand;

        public HtmlMarginEditor(CommandManager commandManager)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            marginCommand = (MarginCommand)commandManager.Get(CommandId.MarginsGroup);

            this.label3.Text = Res.Get(StringId.ImgSBMarginTop);
            this.label4.Text = Res.Get(StringId.ImgSBMarginRight);
            this.label5.Text = Res.Get(StringId.ImgSBMarginLeft);
            this.label6.Text = Res.Get(StringId.ImgSBMarginBottom);

            comboBoxMargins.Initialize();
            comboBoxMargins.SelectedIndexChanged += new EventHandler(comboBoxMargins_SelectedIndexChanged);
            comboBoxMargins.SelectedIndex = 0;
            panelCustomMargin.VisibleChanged += new EventHandler(panelCustomMargin_VisibleChanged);

            comboBoxMargins.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.ImgSBMargins));

            marginCommand.MarginChanged += new EventHandler(marginCommand_MarginChanged);
        }

        void marginCommand_MarginChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        public event EventHandler CustomMarginPanelVisibleChanged;
        public bool IsCustomMarginPanelVisible
        {
            get { return panelCustomMargin.Visible; }
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

                marginCommand.MarginChanged -= new EventHandler(marginCommand_MarginChanged);
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
            this.panelCustomMargin = new System.Windows.Forms.Panel();
            this.numericMarginTop = new System.Windows.Forms.NumericUpDown();
            this.numericMarginRight = new System.Windows.Forms.NumericUpDown();
            this.numericMarginBottom = new System.Windows.Forms.NumericUpDown();
            this.numericMarginLeft = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxMargins = new OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators.HtmlMarginEditor.MarginsComboBox();
            this.panelCustomMargin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginLeft)).BeginInit();
            this.SuspendLayout();
            //
            // panelCustomMargin
            //
            this.panelCustomMargin.Controls.Add(this.numericMarginTop);
            this.panelCustomMargin.Controls.Add(this.numericMarginRight);
            this.panelCustomMargin.Controls.Add(this.numericMarginBottom);
            this.panelCustomMargin.Controls.Add(this.numericMarginLeft);
            this.panelCustomMargin.Controls.Add(this.label3);
            this.panelCustomMargin.Controls.Add(this.label4);
            this.panelCustomMargin.Controls.Add(this.label5);
            this.panelCustomMargin.Controls.Add(this.label6);
            this.panelCustomMargin.Enabled = false;
            this.panelCustomMargin.Location = new System.Drawing.Point(0, 24);
            this.panelCustomMargin.Name = "panelCustomMargin";
            this.panelCustomMargin.Size = new System.Drawing.Size(180, 83);
            this.panelCustomMargin.TabIndex = 5;
            //
            // numericMarginTop
            //
            this.numericMarginTop.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericMarginTop.Location = new System.Drawing.Point(0, 16);
            this.numericMarginTop.Name = "numericMarginTop";
            this.numericMarginTop.Size = new System.Drawing.Size(60, 20);
            this.numericMarginTop.TabIndex = 10;
            this.numericMarginTop.ValueChanged += new System.EventHandler(this.numericMargin_ValueChanged);
            this.numericMarginTop.Enter += new System.EventHandler(this.numericMargin_Enter);
            //
            // numericMarginRight
            //
            this.numericMarginRight.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericMarginRight.Location = new System.Drawing.Point(93, 16);
            this.numericMarginRight.Name = "numericMarginRight";
            this.numericMarginRight.Size = new System.Drawing.Size(60, 20);
            this.numericMarginRight.TabIndex = 20;
            this.numericMarginRight.ValueChanged += new System.EventHandler(this.numericMargin_ValueChanged);
            this.numericMarginRight.Enter += new System.EventHandler(this.numericMargin_Enter);
            //
            // numericMarginBottom
            //
            this.numericMarginBottom.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericMarginBottom.Location = new System.Drawing.Point(0, 56);
            this.numericMarginBottom.Name = "numericMarginBottom";
            this.numericMarginBottom.Size = new System.Drawing.Size(60, 20);
            this.numericMarginBottom.TabIndex = 30;
            this.numericMarginBottom.ValueChanged += new System.EventHandler(this.numericMargin_ValueChanged);
            this.numericMarginBottom.Enter += new System.EventHandler(this.numericMargin_Enter);
            //
            // numericMarginLeft
            //
            this.numericMarginLeft.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericMarginLeft.Location = new System.Drawing.Point(93, 56);
            this.numericMarginLeft.Name = "numericMarginLeft";
            this.numericMarginLeft.Size = new System.Drawing.Size(60, 20);
            this.numericMarginLeft.TabIndex = 40;
            this.numericMarginLeft.ValueChanged += new System.EventHandler(this.numericMargin_ValueChanged);
            this.numericMarginLeft.Enter += new System.EventHandler(this.numericMargin_Enter);
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "T&op";
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label4.Location = new System.Drawing.Point(93, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "&Right";
            //
            // label5
            //
            this.label5.AutoSize = true;
            this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label5.Location = new System.Drawing.Point(93, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(25, 13);
            this.label5.TabIndex = 35;
            this.label5.Text = "&Left";
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label6.Location = new System.Drawing.Point(0, 40);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 25;
            this.label6.Text = "&Bottom";
            //
            // comboBoxMargins
            //
            this.comboBoxMargins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMargins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMargins.Location = new System.Drawing.Point(0, 0);
            this.comboBoxMargins.Name = "comboBoxMargins";
            this.comboBoxMargins.Size = new System.Drawing.Size(204, 21);
            this.comboBoxMargins.TabIndex = 0;
            //
            // HtmlMarginEditor
            //
            this.Controls.Add(this.comboBoxMargins);
            this.Controls.Add(this.panelCustomMargin);
            this.Name = "HtmlMarginEditor";
            this.Size = new System.Drawing.Size(204, 107);
            this.panelCustomMargin.ResumeLayout(false);
            this.panelCustomMargin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginLeft)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        protected override void LoadEditor()
        {
            base.LoadEditor();
            HtmlMarginSettings = new HtmlMarginDecoratorSettings(Settings, EditorContext.ImgElement);

            comboBoxMargins.CustomMargins = HtmlMarginSettings.HasCustomMargin;
            MarginStyle marginStyle = HtmlMarginSettings.Margin;

            numericMarginTop.Value = marginStyle.Top;
            numericMarginRight.Value = marginStyle.Right;
            numericMarginBottom.Value = marginStyle.Bottom;
            numericMarginLeft.Value = marginStyle.Left;

            marginCommand.Value = new Padding(marginStyle.Left, marginStyle.Top, marginStyle.Right, marginStyle.Bottom);

        }
        private HtmlMarginDecoratorSettings HtmlMarginSettings;

        public override Size GetPreferredSize()
        {
            return new Size(204, 68);
        }

        protected override void OnSaveSettings()
        {
            base.OnSaveSettings();

            if (marginCommand.IsZero() == false)
                HtmlMarginSettings.Margin = new MarginStyle(marginCommand.Top, marginCommand.Right, marginCommand.Bottom, marginCommand.Left, StyleSizeUnit.PX);
            else
                HtmlMarginSettings.Margin = null;

            //  Taking out this call because applying
            //   decorators is very slow and doesn't seem to do anything
            //   in this case. I don't know of any side effects to taking
            //   out this call but that doesn't mean they don't exist.
            //   (Bug 672770: Evident lag in applying margins to images, especially if effects are applied to the image)
            //
            // EditorContext.ApplyDecorator();

            FireMarginChanged();
        }

        public event EventHandler HtmlMarginChanged;
        protected void FireMarginChanged()
        {
            if (HtmlMarginChanged != null)
                HtmlMarginChanged(this, EventArgs.Empty);
        }

        private void numericMargin_ValueChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void comboBoxMargins_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelCustomMargin.Visible = comboBoxMargins.CustomMargins;
            panelCustomMargin.Enabled = comboBoxMargins.CustomMargins;
            SaveSettings();
        }

        private class MarginsComboBox : ComboBox
        {
            public MarginsComboBox()
            {
            }

            public void Initialize()
            {
                Items.Add(NO_MARGINS);
                Items.Add(CUSTOM_MARGINS);
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool CustomMargins
            {
                get { return CUSTOM_MARGINS.Equals(SelectedItem); }
                set { if (Items.Count > 0) SelectedItem = value ? CUSTOM_MARGINS : NO_MARGINS; }
            }

            private static readonly MarginComboItem NO_MARGINS = new MarginComboItem(Res.Get(StringId.ImgSBMarginNoMargins), MarginType.NoMargins);
            private static readonly MarginComboItem CUSTOM_MARGINS = new MarginComboItem(Res.Get(StringId.ImgSBMarginCustomMargins), MarginType.CustomMargins);
        }

        private void panelCustomMargin_VisibleChanged(object sender, EventArgs e)
        {
            if (CustomMarginPanelVisibleChanged != null)
                CustomMarginPanelVisibleChanged(this, EventArgs.Empty);
        }

        private void numericMargin_Enter(object sender, System.EventArgs e)
        {
            NumericUpDown numericControl = sender as NumericUpDown;
            if (numericControl != null)
                numericControl.Select(0, numericControl.Text.Length);

        }
    }
}

