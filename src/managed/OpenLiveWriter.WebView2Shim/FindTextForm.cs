// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.WebView2Shim
{
    /// <summary>
    /// Simple Find dialog for WebView2 editor.
    /// </summary>
    public class FindTextForm : Form
    {
        private TextBox _textBoxFindWhat;
        private Button _buttonFindNext;
        private Button _buttonClose;
        private CheckBox _checkBoxMatchCase;
        private GroupBox _groupBoxDirection;
        private RadioButton _radioButtonUp;
        private RadioButton _radioButtonDown;
        private Label _labelFindWhat;

        /// <summary>
        /// Gets the search text entered by the user.
        /// </summary>
        public string SearchText => _textBoxFindWhat?.Text ?? string.Empty;

        /// <summary>
        /// Gets whether to match case.
        /// </summary>
        public bool MatchCase => _checkBoxMatchCase?.Checked ?? false;

        /// <summary>
        /// Gets whether to search backward.
        /// </summary>
        public bool SearchBackward => _radioButtonUp?.Checked ?? false;

        public FindTextForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _labelFindWhat = new Label();
            _textBoxFindWhat = new TextBox();
            _buttonFindNext = new Button();
            _buttonClose = new Button();
            _checkBoxMatchCase = new CheckBox();
            _groupBoxDirection = new GroupBox();
            _radioButtonUp = new RadioButton();
            _radioButtonDown = new RadioButton();

            _groupBoxDirection.SuspendLayout();
            SuspendLayout();

            // labelFindWhat
            _labelFindWhat.AutoSize = true;
            _labelFindWhat.Location = new Point(12, 15);
            _labelFindWhat.Name = "labelFindWhat";
            _labelFindWhat.Size = new Size(56, 13);
            _labelFindWhat.Text = "Fi&nd what:";

            // textBoxFindWhat
            _textBoxFindWhat.Location = new Point(12, 33);
            _textBoxFindWhat.Name = "textBoxFindWhat";
            _textBoxFindWhat.Size = new Size(270, 20);
            _textBoxFindWhat.TabIndex = 0;
            _textBoxFindWhat.TextChanged += TextBoxFindWhat_TextChanged;

            // buttonFindNext
            _buttonFindNext.Location = new Point(295, 31);
            _buttonFindNext.Name = "buttonFindNext";
            _buttonFindNext.Size = new Size(85, 23);
            _buttonFindNext.TabIndex = 1;
            _buttonFindNext.Text = "&Find Next";
            _buttonFindNext.UseVisualStyleBackColor = true;
            _buttonFindNext.Enabled = false;
            _buttonFindNext.Click += ButtonFindNext_Click;

            // buttonClose
            _buttonClose.DialogResult = DialogResult.Cancel;
            _buttonClose.Location = new Point(295, 60);
            _buttonClose.Name = "buttonClose";
            _buttonClose.Size = new Size(85, 23);
            _buttonClose.TabIndex = 2;
            _buttonClose.Text = "Close";
            _buttonClose.UseVisualStyleBackColor = true;

            // checkBoxMatchCase
            _checkBoxMatchCase.AutoSize = true;
            _checkBoxMatchCase.Location = new Point(12, 60);
            _checkBoxMatchCase.Name = "checkBoxMatchCase";
            _checkBoxMatchCase.Size = new Size(82, 17);
            _checkBoxMatchCase.TabIndex = 3;
            _checkBoxMatchCase.Text = "Match &case";
            _checkBoxMatchCase.UseVisualStyleBackColor = true;

            // groupBoxDirection
            _groupBoxDirection.Controls.Add(_radioButtonDown);
            _groupBoxDirection.Controls.Add(_radioButtonUp);
            _groupBoxDirection.Location = new Point(110, 55);
            _groupBoxDirection.Name = "groupBoxDirection";
            _groupBoxDirection.Size = new Size(172, 40);
            _groupBoxDirection.TabIndex = 4;
            _groupBoxDirection.TabStop = false;
            _groupBoxDirection.Text = "Direction";

            // radioButtonUp
            _radioButtonUp.AutoSize = true;
            _radioButtonUp.Location = new Point(10, 16);
            _radioButtonUp.Name = "radioButtonUp";
            _radioButtonUp.Size = new Size(39, 17);
            _radioButtonUp.TabIndex = 0;
            _radioButtonUp.Text = "&Up";
            _radioButtonUp.UseVisualStyleBackColor = true;

            // radioButtonDown
            _radioButtonDown.AutoSize = true;
            _radioButtonDown.Checked = true;
            _radioButtonDown.Location = new Point(65, 16);
            _radioButtonDown.Name = "radioButtonDown";
            _radioButtonDown.Size = new Size(53, 17);
            _radioButtonDown.TabIndex = 1;
            _radioButtonDown.TabStop = true;
            _radioButtonDown.Text = "&Down";
            _radioButtonDown.UseVisualStyleBackColor = true;

            // FindTextForm
            AcceptButton = _buttonFindNext;
            CancelButton = _buttonClose;
            ClientSize = new Size(392, 105);
            Controls.Add(_groupBoxDirection);
            Controls.Add(_checkBoxMatchCase);
            Controls.Add(_buttonClose);
            Controls.Add(_buttonFindNext);
            Controls.Add(_textBoxFindWhat);
            Controls.Add(_labelFindWhat);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FindTextForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Find";

            _groupBoxDirection.ResumeLayout(false);
            _groupBoxDirection.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private void TextBoxFindWhat_TextChanged(object sender, EventArgs e)
        {
            _buttonFindNext.Enabled = !string.IsNullOrEmpty(_textBoxFindWhat.Text);
        }

        private void ButtonFindNext_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_textBoxFindWhat.Text))
            {
                DialogResult = DialogResult.OK;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _labelFindWhat?.Dispose();
                _textBoxFindWhat?.Dispose();
                _buttonFindNext?.Dispose();
                _buttonClose?.Dispose();
                _checkBoxMatchCase?.Dispose();
                _radioButtonUp?.Dispose();
                _radioButtonDown?.Dispose();
                _groupBoxDirection?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
