// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Summary description for HtmlSourceEditorFindTextForm.
    /// </summary>
    public class HtmlSourceEditorFindTextForm : ApplicationDialog
    {
        private System.Windows.Forms.Label labelFindWhat;
        private System.Windows.Forms.TextBox textBoxFindWhat;
        private System.Windows.Forms.Button buttonFindNext;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.GroupBox groupBoxDirection;
        private System.Windows.Forms.RadioButton radioButtonUp;
        private System.Windows.Forms.RadioButton radioButtonDown;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private TextBox _targetTextBox;

        public HtmlSourceEditorFindTextForm(TextBox targetTextBox)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.labelFindWhat.Text = Res.Get(StringId.FindWhatLabel);
            this.buttonFindNext.Text = Res.Get(StringId.FindNextButton);
            this.buttonClose.Text = Res.Get(StringId.CloseButton);
            this.groupBoxDirection.Text = Res.Get(StringId.FindDirection);
            this.radioButtonDown.Text = Res.Get(StringId.FindDirectionDown);
            this.radioButtonUp.Text = Res.Get(StringId.FindDirectionUp);
            this.Text = Res.Get(StringId.FindTitle);

            buttonFindNext.Enabled = false;

            // save referenced to text text box
            _targetTextBox = targetTextBox;

            // initialize search text
            textBoxFindWhat.Text = targetTextBox.SelectedText;

            // initialize direction
            radioButtonDown.Checked = targetTextBox.SelectionStart < (targetTextBox.Text.Length / 2);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int distance = radioButtonDown.Left - radioButtonUp.Right;
            DisplayHelper.AutoFitSystemRadioButton(radioButtonDown, 0, int.MaxValue);
            DisplayHelper.AutoFitSystemRadioButton(radioButtonUp, 0, int.MaxValue);
            radioButtonDown.Left = radioButtonUp.Right + distance;

            using (new AutoGrow(this, AnchorStyles.Bottom | AnchorStyles.Right, false))
            {
                int oldTop = radioButtonDown.Top;
                radioButtonDown.Top = radioButtonUp.Top = Res.DefaultFont.Height + 3;
                groupBoxDirection.Height += Math.Max(radioButtonDown.Top - oldTop, 0);
                LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Left, buttonClose.Width, int.MaxValue, buttonFindNext, buttonClose);
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

        private void textBoxFindWhat_TextChanged(object sender, System.EventArgs e)
        {
            buttonFindNext.Enabled = textBoxFindWhat.Text.Length != 0;
        }

        private void buttonFindNext_Click(object sender, System.EventArgs e)
        {
            try
            {
                string findText = textBoxFindWhat.Text.Trim();
                int nextOccurance = -1;
                if (radioButtonDown.Checked)
                {
                    nextOccurance = _targetTextBox.Text.IndexOf(findText,
                        _targetTextBox.SelectionStart + _targetTextBox.SelectionLength);
                }
                else
                {
                    int start = _targetTextBox.SelectionStart;
                    if (_targetTextBox.SelectionLength == 1 && start > 0)
                        start = start - 1;
                    nextOccurance = _targetTextBox.Text.LastIndexOf(findText, start);
                }

                if (nextOccurance != -1)
                {
                    _targetTextBox.Select(nextOccurance, findText.Length);
                    _targetTextBox.ScrollToCaret();
                }
                else
                {
                    DisplayMessage.Show(MessageId.FinishedSearchingDocument, this);
                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(this, ex);
            }
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelFindWhat = new System.Windows.Forms.Label();
            this.textBoxFindWhat = new System.Windows.Forms.TextBox();
            this.buttonFindNext = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.groupBoxDirection = new System.Windows.Forms.GroupBox();
            this.radioButtonDown = new System.Windows.Forms.RadioButton();
            this.radioButtonUp = new System.Windows.Forms.RadioButton();
            this.groupBoxDirection.SuspendLayout();
            this.SuspendLayout();
            //
            // labelFindWhat
            //
            this.labelFindWhat.AutoSize = true;
            this.labelFindWhat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelFindWhat.Location = new System.Drawing.Point(10, 7);
            this.labelFindWhat.Name = "labelFindWhat";
            this.labelFindWhat.Size = new System.Drawing.Size(62, 15);
            this.labelFindWhat.TabIndex = 0;
            this.labelFindWhat.Text = "Fi&nd what:";
            //
            // textBoxFindWhat
            //
            this.textBoxFindWhat.Location = new System.Drawing.Point(10, 25);
            this.textBoxFindWhat.Name = "textBoxFindWhat";
            this.textBoxFindWhat.Size = new System.Drawing.Size(297, 23);
            this.textBoxFindWhat.TabIndex = 1;
            this.textBoxFindWhat.TextChanged += new System.EventHandler(this.textBoxFindWhat_TextChanged);
            //
            // buttonFindNext
            //
            this.buttonFindNext.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonFindNext.Location = new System.Drawing.Point(317, 25);
            this.buttonFindNext.Name = "buttonFindNext";
            this.buttonFindNext.Size = new System.Drawing.Size(90, 26);
            this.buttonFindNext.TabIndex = 2;
            this.buttonFindNext.Text = "&Find Next";
            this.buttonFindNext.Click += new System.EventHandler(this.buttonFindNext_Click);
            //
            // buttonClose
            //
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonClose.Location = new System.Drawing.Point(317, 57);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(90, 26);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "Close";
            //
            // groupBoxDirection
            //
            this.groupBoxDirection.Controls.Add(this.radioButtonDown);
            this.groupBoxDirection.Controls.Add(this.radioButtonUp);
            this.groupBoxDirection.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxDirection.Location = new System.Drawing.Point(10, 53);
            this.groupBoxDirection.Name = "groupBoxDirection";
            this.groupBoxDirection.Size = new System.Drawing.Size(297, 45);
            this.groupBoxDirection.TabIndex = 4;
            this.groupBoxDirection.TabStop = false;
            this.groupBoxDirection.Text = "Direction";
            //
            // radioButtonDown
            //
            this.radioButtonDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonDown.Location = new System.Drawing.Point(86, 15);
            this.radioButtonDown.Name = "radioButtonDown";
            this.radioButtonDown.Size = new System.Drawing.Size(64, 27);
            this.radioButtonDown.TabIndex = 1;
            this.radioButtonDown.Text = "&Down";
            //
            // radioButtonUp
            //
            this.radioButtonUp.Checked = true;
            this.radioButtonUp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.radioButtonUp.Location = new System.Drawing.Point(19, 15);
            this.radioButtonUp.Name = "radioButtonUp";
            this.radioButtonUp.Size = new System.Drawing.Size(58, 27);
            this.radioButtonUp.TabIndex = 0;
            this.radioButtonUp.TabStop = true;
            this.radioButtonUp.Text = "&Up";
            //
            // HtmlSourceEditorFindTextForm
            //
            this.AcceptButton = this.buttonFindNext;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(416, 105);
            this.Controls.Add(this.groupBoxDirection);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonFindNext);
            this.Controls.Add(this.textBoxFindWhat);
            this.Controls.Add(this.labelFindWhat);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HtmlSourceEditorFindTextForm";
            this.Text = "Find";
            this.groupBoxDirection.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

    }
}
