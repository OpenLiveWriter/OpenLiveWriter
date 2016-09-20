// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{

    public class RowPropertiesForm : ApplicationDialog
    {
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxHeight;
        private System.Windows.Forms.RadioButton radioButtonSizeToContent;
        private System.Windows.Forms.RadioButton radioButtonFixedHeight;
        private System.Windows.Forms.Label labelPixels;
        private OpenLiveWriter.Controls.NumericTextBox textBoxHeight;
        private OpenLiveWriter.PostEditor.Tables.CellPropertiesControl cellPropertiesControl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public RowPropertiesForm(RowProperties rowProperties)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.groupBoxHeight.Text = Res.Get(StringId.Height);
            this.labelPixels.Text = Res.Get(StringId.Pixels);
            this.radioButtonFixedHeight.Text = Res.Get(StringId.RowPropertiesFixedHeight);
            this.radioButtonSizeToContent.Text = Res.Get(StringId.RowPropertiesSizeToContent);
            this.Text = Res.Get(StringId.RowPropertiesTitle);

            RowHeight = rowProperties.Height;
            cellPropertiesControl.CellProperties = rowProperties.CellProperties;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (AutoGrow autoGrow = new AutoGrow(this, AnchorStyles.Right, false))
            {
                autoGrow.AllowAnchoring = true;

                using (new AutoGrow(groupBoxHeight, AnchorStyles.Right, false))
                {
                    DisplayHelper.AutoFitSystemRadioButton(radioButtonFixedHeight, 0, int.MaxValue);
                    DisplayHelper.AutoFitSystemRadioButton(radioButtonSizeToContent, 0, int.MaxValue);
                }

                cellPropertiesControl.AdjustSize();

                cellPropertiesControl.Width = groupBoxHeight.Width = Math.Max(cellPropertiesControl.Width, groupBoxHeight.Width);
            }
            LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
        }

        public RowProperties RowProperties
        {
            get
            {
                RowProperties rowProperties = new RowProperties();
                rowProperties.Height = RowHeight;
                rowProperties.CellProperties = cellPropertiesControl.CellProperties;
                return rowProperties;
            }
        }

        private int RowHeight
        {
            get
            {
                if (radioButtonSizeToContent.Checked)
                {
                    return 0;
                }
                else
                {
                    try
                    {
                        return int.Parse(textBoxHeight.Text, CultureInfo.CurrentCulture);
                    }
                    catch
                    {
                        return 0;
                    }
                }
            }
            set
            {
                if (value != 0)
                {
                    radioButtonFixedHeight.Checked = true;
                    textBoxHeight.Text = value.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    radioButtonSizeToContent.Checked = true;
                    textBoxHeight.Text = String.Empty;
                }
                ManageUIState();
            }
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            if (ValidateInput(20, 1000))
                DialogResult = DialogResult.OK;
        }

        private void radioButtonSizeToContent_CheckedChanged(object sender, System.EventArgs e)
        {
            ManageUIState();
        }

        private void radioButtonFixedHeight_CheckedChanged(object sender, System.EventArgs e)
        {
            ManageUIState();
        }

        private void ManageUIState()
        {
            textBoxHeight.Enabled = radioButtonFixedHeight.Checked;
        }

        private bool ValidateInput(int minValue, int maxValue)
        {
            if (radioButtonFixedHeight.Checked)
            {
                if (RowHeight == 0)
                {
                    DisplayMessage.Show(MessageId.UnspecifiedValue, FindForm(), Res.Get(StringId.Height));
                    textBoxHeight.Focus();
                    return false;
                }
                else if (minValue > 0 && RowHeight < minValue)
                {
                    DisplayMessage.Show(MessageId.ValueLessThanMinimum, FindForm(), minValue, Res.Get(StringId.Height));
                    textBoxHeight.Focus();
                    return false;
                }
                else if (maxValue > 0 && RowHeight >= maxValue)
                {
                    DisplayMessage.Show(MessageId.ValueExceedsMaximum, FindForm(), maxValue, Res.Get(StringId.Height));
                    textBoxHeight.Focus();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxHeight = new System.Windows.Forms.GroupBox();
            this.labelPixels = new System.Windows.Forms.Label();
            this.textBoxHeight = new OpenLiveWriter.Controls.NumericTextBox();
            this.radioButtonFixedHeight = new System.Windows.Forms.RadioButton();
            this.radioButtonSizeToContent = new System.Windows.Forms.RadioButton();
            this.cellPropertiesControl = new OpenLiveWriter.PostEditor.Tables.CellPropertiesControl();
            this.groupBoxHeight.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(97, 240);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(178, 240);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            //
            // groupBoxHeight
            //
            this.groupBoxHeight.Controls.Add(this.labelPixels);
            this.groupBoxHeight.Controls.Add(this.textBoxHeight);
            this.groupBoxHeight.Controls.Add(this.radioButtonFixedHeight);
            this.groupBoxHeight.Controls.Add(this.radioButtonSizeToContent);
            this.groupBoxHeight.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxHeight.Location = new System.Drawing.Point(9, 9);
            this.groupBoxHeight.Name = "groupBoxHeight";
            this.groupBoxHeight.Size = new System.Drawing.Size(243, 119);
            this.groupBoxHeight.TabIndex = 0;
            this.groupBoxHeight.TabStop = false;
            this.groupBoxHeight.Text = "Height";
            //
            // labelPixels
            //
            this.labelPixels.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPixels.Location = new System.Drawing.Point(105, 87);
            this.labelPixels.Name = "labelPixels";
            this.labelPixels.Size = new System.Drawing.Size(120, 15);
            this.labelPixels.TabIndex = 3;
            this.labelPixels.Text = "pixels";
            //
            // textBoxHeight
            //
            this.textBoxHeight.Location = new System.Drawing.Point(50, 84);
            this.textBoxHeight.MaxLength = 9;
            this.textBoxHeight.Name = "textBoxHeight";
            this.textBoxHeight.Size = new System.Drawing.Size(46, 21);
            this.textBoxHeight.TabIndex = 2;
            this.textBoxHeight.Text = "";
            //
            // radioButtonFixedHeight
            //
            this.radioButtonFixedHeight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonFixedHeight.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonFixedHeight.Location = new System.Drawing.Point(15, 57);
            this.radioButtonFixedHeight.Name = "radioButtonFixedHeight";
            this.radioButtonFixedHeight.Size = new System.Drawing.Size(216, 24);
            this.radioButtonFixedHeight.TabIndex = 1;
            this.radioButtonFixedHeight.Text = "&Fixed height:";
            this.radioButtonFixedHeight.CheckedChanged += new System.EventHandler(this.radioButtonFixedHeight_CheckedChanged);
            //
            // radioButtonSizeToContent
            //
            this.radioButtonSizeToContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonSizeToContent.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonSizeToContent.Location = new System.Drawing.Point(15, 25);
            this.radioButtonSizeToContent.Name = "radioButtonSizeToContent";
            this.radioButtonSizeToContent.Size = new System.Drawing.Size(212, 24);
            this.radioButtonSizeToContent.TabIndex = 0;
            this.radioButtonSizeToContent.Text = "&Automatically size to content";
            this.radioButtonSizeToContent.CheckedChanged += new System.EventHandler(this.radioButtonSizeToContent_CheckedChanged);
            //
            // cellPropertiesControl
            //
            this.cellPropertiesControl.Location = new System.Drawing.Point(9, 132);
            this.cellPropertiesControl.Name = "cellPropertiesControl";
            this.cellPropertiesControl.Size = new System.Drawing.Size(243, 98);
            this.cellPropertiesControl.TabIndex = 1;
            //
            // RowPropertiesForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(262, 271);
            this.Controls.Add(this.cellPropertiesControl);
            this.Controls.Add(this.groupBoxHeight);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RowPropertiesForm";
            this.Text = "Row Properties";
            this.groupBoxHeight.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
