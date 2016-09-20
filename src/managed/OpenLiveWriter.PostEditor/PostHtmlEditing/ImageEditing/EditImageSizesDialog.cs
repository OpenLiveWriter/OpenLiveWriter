// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for EditImageSizesDialog.
    /// </summary>
    public class EditImageSizesDialog : BaseForm
    {
        private NumericUpDown numericSmallWidth;
        private Label label1;
        private Label label2;
        private NumericUpDown numericSmallHeight;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;
        private Label label11;
        private Label label12;
        private NumericUpDown numericMediumHeight;
        private NumericUpDown numericMediumWidth;
        private NumericUpDown numericLargeHeight;
        private NumericUpDown numericLargeWidth;
        private Button buttonCancel;
        private Button buttonOK;
        private GroupLabelControl labelImageLarge;
        private GroupLabelControl labelImageMedium;
        private GroupLabelControl labelImageSmall;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public EditImageSizesDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            numericSmallHeight.Maximum = ImageSizeHelper.MAX_HEIGHT;
            numericSmallWidth.Maximum = ImageSizeHelper.MAX_WIDTH;
            numericMediumHeight.Maximum = ImageSizeHelper.MAX_HEIGHT;
            numericMediumWidth.Maximum = ImageSizeHelper.MAX_WIDTH;
            numericLargeHeight.Maximum = ImageSizeHelper.MAX_HEIGHT;
            numericLargeWidth.Maximum = ImageSizeHelper.MAX_WIDTH;

            numericSmallHeight.Minimum = 1;
            numericSmallWidth.Minimum = 1;
            numericMediumHeight.Minimum = 1;
            numericMediumWidth.Minimum = 1;
            numericLargeHeight.Minimum = 1;
            numericLargeWidth.Minimum = 1;

            numericSmallHeight.Leave += new EventHandler(numeric_Leave);
            numericSmallWidth.Leave += new EventHandler(numeric_Leave);
            numericMediumHeight.Leave += new EventHandler(numeric_Leave);
            numericMediumWidth.Leave += new EventHandler(numeric_Leave);
            numericLargeHeight.Leave += new EventHandler(numeric_Leave);
            numericLargeWidth.Leave += new EventHandler(numeric_Leave);

            numericSmallHeight.Enter += new EventHandler(numeric_Enter);
            numericSmallWidth.Enter += new EventHandler(numeric_Enter);
            numericMediumHeight.Enter += new EventHandler(numeric_Enter);
            numericMediumWidth.Enter += new EventHandler(numeric_Enter);
            numericLargeHeight.Enter += new EventHandler(numeric_Enter);
            numericLargeWidth.Enter += new EventHandler(numeric_Enter);

            this.label4.Text = Res.Get(StringId.Pixels);
            this.label3.Text = Res.Get(StringId.Pixels);
            this.label2.Text = Res.Get(StringId.ImgSBMaximumHeightLabel1);
            this.label1.Text = Res.Get(StringId.ImgSBMaximumWidthLabel1);
            this.label5.Text = Res.Get(StringId.Pixels);
            this.label6.Text = Res.Get(StringId.Pixels);
            this.label7.Text = Res.Get(StringId.ImgSBMaximumHeightLabel2);
            this.label8.Text = Res.Get(StringId.ImgSBMaximumWidthLabel2);
            this.label9.Text = Res.Get(StringId.Pixels);
            this.label10.Text = Res.Get(StringId.Pixels);
            this.label11.Text = Res.Get(StringId.ImgSBMaximumHeightLabel3);
            this.label12.Text = Res.Get(StringId.ImgSBMaximumWidthLabel3);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.labelImageLarge.Text = Res.Get(StringId.ImgSBLargeImage);
            this.labelImageMedium.Text = Res.Get(StringId.ImgSBMediumImage);
            this.labelImageSmall.Text = Res.Get(StringId.ImgSBSmallImage);
            this.Text = Res.Get(StringId.ImgSBDefaultImageSizes);

            numericSmallWidth.Value = ImageEditingSettings.DefaultImageSizeSmall.Width;
            numericSmallHeight.Value = ImageEditingSettings.DefaultImageSizeSmall.Height;
            numericMediumWidth.Value = ImageEditingSettings.DefaultImageSizeMedium.Width;
            numericMediumHeight.Value = ImageEditingSettings.DefaultImageSizeMedium.Height;
            numericLargeWidth.Value = ImageEditingSettings.DefaultImageSizeLarge.Width;
            numericLargeHeight.Value = ImageEditingSettings.DefaultImageSizeLarge.Height;

            numericSmallWidth.AccessibleName = Res.Get(StringId.MaxSmWidth);
            numericSmallHeight.AccessibleName = Res.Get(StringId.MaxSmHeight);
            numericMediumWidth.AccessibleName = Res.Get(StringId.MaxMdWidth);
            numericMediumHeight.AccessibleName = Res.Get(StringId.MaxMdHeight);
            numericLargeWidth.AccessibleName = Res.Get(StringId.MaxLgWidth);
            numericLargeHeight.AccessibleName = Res.Get(StringId.MaxLgHeight);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Label[] leftLabels = { label1, label2, label8, label7, label12, label11 };
            NumericUpDown[] values = { numericSmallWidth, numericSmallHeight, numericMediumWidth, numericMediumHeight, numericLargeWidth, numericLargeHeight };
            Label[] rightLabels = { label3, label4, label6, label5, label10, label9 };

            using (AutoGrow autoGrow = new AutoGrow(this, AnchorStyles.Right, false))
            {
                autoGrow.AllowAnchoring = true;

                int deltaX = LayoutHelper.AutoFitLabels(leftLabels);
                new ControlGroup(values).Left += deltaX;
                new ControlGroup(rightLabels).Left += deltaX;
                LayoutHelper.AutoFitLabels(rightLabels);

                LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
            }
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.numericSmallWidth = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericSmallHeight = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.numericMediumHeight = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.numericMediumWidth = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.numericLargeHeight = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.numericLargeWidth = new System.Windows.Forms.NumericUpDown();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelImageLarge = new OpenLiveWriter.Controls.GroupLabelControl();
            this.labelImageMedium = new OpenLiveWriter.Controls.GroupLabelControl();
            this.labelImageSmall = new OpenLiveWriter.Controls.GroupLabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.numericSmallWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSmallHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMediumHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMediumWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLargeHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLargeWidth)).BeginInit();
            this.SuspendLayout();
            //
            // numericSmallWidth
            //
            this.numericSmallWidth.Location = new System.Drawing.Point(115, 28);
            this.numericSmallWidth.Maximum = new System.Decimal(new int[] {
                                                                              4096,
                                                                              0,
                                                                              0,
                                                                              0});
            this.numericSmallWidth.Minimum = new System.Decimal(new int[] {
                                                                              16,
                                                                              0,
                                                                              0,
                                                                              0});
            this.numericSmallWidth.Name = "numericSmallWidth";
            this.numericSmallWidth.Size = new System.Drawing.Size(56, 21);
            this.numericSmallWidth.TabIndex = 2;
            this.numericSmallWidth.Value = new System.Decimal(new int[] {
                                                                            16,
                                                                            0,
                                                                            0,
                                                                            0});
            //
            // label4
            //
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label4.Location = new System.Drawing.Point(180, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 16);
            this.label4.TabIndex = 20;
            this.label4.Text = "pixels";
            //
            // label3
            //
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label3.Location = new System.Drawing.Point(180, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 16);
            this.label3.TabIndex = 19;
            this.label3.Text = "pixels";
            //
            // label2
            //
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(20, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Maximum &Height:";
            //
            // numericSmallHeight
            //
            this.numericSmallHeight.Location = new System.Drawing.Point(115, 52);
            this.numericSmallHeight.Maximum = new System.Decimal(new int[] {
                                                                               3072,
                                                                               0,
                                                                               0,
                                                                               0});
            this.numericSmallHeight.Minimum = new System.Decimal(new int[] {
                                                                               16,
                                                                               0,
                                                                               0,
                                                                               0});
            this.numericSmallHeight.Name = "numericSmallHeight";
            this.numericSmallHeight.Size = new System.Drawing.Size(56, 21);
            this.numericSmallHeight.TabIndex = 4;
            this.numericSmallHeight.Value = new System.Decimal(new int[] {
                                                                             16,
                                                                             0,
                                                                             0,
                                                                             0});
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(20, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Maximum &Width:";
            //
            // label5
            //
            this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label5.Location = new System.Drawing.Point(180, 128);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 16);
            this.label5.TabIndex = 18;
            this.label5.Text = "pixels";
            //
            // label6
            //
            this.label6.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label6.Location = new System.Drawing.Point(180, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 16);
            this.label6.TabIndex = 17;
            this.label6.Text = "pixels";
            //
            // label7
            //
            this.label7.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label7.Location = new System.Drawing.Point(20, 128);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(85, 16);
            this.label7.TabIndex = 7;
            this.label7.Text = "Maximum H&eight:";
            //
            // numericMediumHeight
            //
            this.numericMediumHeight.Location = new System.Drawing.Point(115, 128);
            this.numericMediumHeight.Maximum = new System.Decimal(new int[] {
                                                                                3072,
                                                                                0,
                                                                                0,
                                                                                0});
            this.numericMediumHeight.Minimum = new System.Decimal(new int[] {
                                                                                16,
                                                                                0,
                                                                                0,
                                                                                0});
            this.numericMediumHeight.Name = "numericMediumHeight";
            this.numericMediumHeight.Size = new System.Drawing.Size(56, 21);
            this.numericMediumHeight.TabIndex = 8;
            this.numericMediumHeight.Value = new System.Decimal(new int[] {
                                                                              16,
                                                                              0,
                                                                              0,
                                                                              0});
            //
            // label8
            //
            this.label8.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label8.Location = new System.Drawing.Point(20, 104);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(85, 16);
            this.label8.TabIndex = 5;
            this.label8.Text = "Maximum W&idth:";
            //
            // numericMediumWidth
            //
            this.numericMediumWidth.Location = new System.Drawing.Point(115, 104);
            this.numericMediumWidth.Maximum = new System.Decimal(new int[] {
                                                                               4096,
                                                                               0,
                                                                               0,
                                                                               0});
            this.numericMediumWidth.Minimum = new System.Decimal(new int[] {
                                                                               16,
                                                                               0,
                                                                               0,
                                                                               0});
            this.numericMediumWidth.Name = "numericMediumWidth";
            this.numericMediumWidth.Size = new System.Drawing.Size(56, 21);
            this.numericMediumWidth.TabIndex = 6;
            this.numericMediumWidth.Value = new System.Decimal(new int[] {
                                                                             16,
                                                                             0,
                                                                             0,
                                                                             0});
            //
            // label9
            //
            this.label9.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label9.Location = new System.Drawing.Point(180, 204);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(48, 16);
            this.label9.TabIndex = 16;
            this.label9.Text = "pixels";
            //
            // label10
            //
            this.label10.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label10.Location = new System.Drawing.Point(180, 180);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 16);
            this.label10.TabIndex = 15;
            this.label10.Text = "pixels";
            //
            // label11
            //
            this.label11.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label11.Location = new System.Drawing.Point(20, 204);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 16);
            this.label11.TabIndex = 11;
            this.label11.Text = "Maximum Hei&ght:";
            //
            // numericLargeHeight
            //
            this.numericLargeHeight.Location = new System.Drawing.Point(115, 200);
            this.numericLargeHeight.Maximum = new System.Decimal(new int[] {
                                                                               3072,
                                                                               0,
                                                                               0,
                                                                               0});
            this.numericLargeHeight.Minimum = new System.Decimal(new int[] {
                                                                               16,
                                                                               0,
                                                                               0,
                                                                               0});
            this.numericLargeHeight.Name = "numericLargeHeight";
            this.numericLargeHeight.Size = new System.Drawing.Size(56, 21);
            this.numericLargeHeight.TabIndex = 12;
            this.numericLargeHeight.Value = new System.Decimal(new int[] {
                                                                             16,
                                                                             0,
                                                                             0,
                                                                             0});
            //
            // label12
            //
            this.label12.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label12.Location = new System.Drawing.Point(20, 180);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(85, 16);
            this.label12.TabIndex = 9;
            this.label12.Text = "Maximum Wi&dth:";
            //
            // numericLargeWidth
            //
            this.numericLargeWidth.Location = new System.Drawing.Point(115, 176);
            this.numericLargeWidth.Maximum = new System.Decimal(new int[] {
                                                                              4096,
                                                                              0,
                                                                              0,
                                                                              0});
            this.numericLargeWidth.Minimum = new System.Decimal(new int[] {
                                                                              16,
                                                                              0,
                                                                              0,
                                                                              0});
            this.numericLargeWidth.Name = "numericLargeWidth";
            this.numericLargeWidth.Size = new System.Drawing.Size(56, 21);
            this.numericLargeWidth.TabIndex = 10;
            this.numericLargeWidth.Value = new System.Decimal(new int[] {
                                                                            16,
                                                                            0,
                                                                            0,
                                                                            0});
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(172, 236);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 14;
            this.buttonCancel.Text = "Cancel";
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(92, 236);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 13;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // labelImageLarge
            //
            this.labelImageLarge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labelImageLarge.Location = new System.Drawing.Point(10, 160);
            this.labelImageLarge.MultiLine = false;
            this.labelImageLarge.Name = "labelImageLarge";
            //this.labelImageLarge.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelImageLarge.Size = new System.Drawing.Size(236, 16);
            this.labelImageLarge.TabIndex = 2;
            this.labelImageLarge.Text = "Large image:";
            //
            // labelImageMedium
            //
            this.labelImageMedium.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labelImageMedium.Location = new System.Drawing.Point(10, 84);
            this.labelImageMedium.MultiLine = false;
            this.labelImageMedium.Name = "labelImageMedium";
            //this.labelImageMedium.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelImageMedium.Size = new System.Drawing.Size(236, 16);
            this.labelImageMedium.TabIndex = 1;
            this.labelImageMedium.Text = "Medium image:";
            //
            // labelImageSmall
            //
            this.labelImageSmall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labelImageSmall.Location = new System.Drawing.Point(10, 12);
            this.labelImageSmall.MultiLine = false;
            this.labelImageSmall.Name = "labelImageSmall";
            //this.labelImageSmall.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelImageSmall.Size = new System.Drawing.Size(236, 16);
            this.labelImageSmall.TabIndex = 0;
            this.labelImageSmall.Text = "Small image:";
            //
            // EditImageSizesDialog
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(256, 268);
            this.Controls.Add(this.labelImageSmall);
            this.Controls.Add(this.labelImageMedium);
            this.Controls.Add(this.labelImageLarge);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.numericLargeWidth);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.numericLargeHeight);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numericMediumHeight);
            this.Controls.Add(this.numericMediumWidth);
            this.Controls.Add(this.numericSmallWidth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericSmallHeight);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditImageSizesDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Default Image Sizes";
            ((System.ComponentModel.ISupportInitialize)(this.numericSmallWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSmallHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMediumHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMediumWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLargeHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLargeWidth)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void buttonOK_Click(object sender, EventArgs e)
        {
            ImageEditingSettings.DefaultImageSizeSmall = new Size((int)numericSmallWidth.Value, (int)numericSmallHeight.Value);
            ImageEditingSettings.DefaultImageSizeMedium = new Size((int)numericMediumWidth.Value, (int)numericMediumHeight.Value);
            ImageEditingSettings.DefaultImageSizeLarge = new Size((int)numericLargeWidth.Value, (int)numericLargeHeight.Value);
        }

        private void numeric_Leave(object sender, EventArgs e)
        {
            NumericUpDown upDownPicker = (NumericUpDown)sender;
            if (upDownPicker.Value > upDownPicker.Maximum)
                upDownPicker.Value = upDownPicker.Maximum;
        }

        private void numeric_Enter(object sender, EventArgs e)
        {
            NumericUpDown upDownPicker = (NumericUpDown)sender;
            upDownPicker.Select(0, upDownPicker.Text.Length);
        }
    }
}
