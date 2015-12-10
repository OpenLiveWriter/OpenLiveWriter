// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using System.Runtime.InteropServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls.Wizard
{

    /// <summary>
    /// Base Form for creating wizard dialogs.
    /// Wizards should subclass this class and add wizard steps.
    /// </summary>
    public class WizardForm : BaseForm
    {
        private IContainer components = null;
        protected Button buttonCancel;
        protected Button buttonBack;
        protected Button buttonNext;
        protected static string BUTTON_TEXT_NEXT = Res.Get(StringId.WizardNext);
        protected static string BUTTON_TEXT_FINISH = Res.Get(StringId.WizardFinish);

        private Panel wizardBody;
        private Panel panelHeader;
        protected Panel panelFooter;
        private WizardController _controller;
        private IntPtr _hSystemMenu;

        public WizardForm(WizardController controller)
        {
            _controller = controller;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            buttonBack.Text = Res.Get(StringId.WizardBack);
            buttonNext.Text = BUTTON_TEXT_NEXT;
            buttonCancel.Text = Res.Get(StringId.CancelButton);
            _controller.WizardControl = this;

            this.FormClosing += new FormClosingEventHandler(WizardForm_FormClosing);
        }

        /// <summary>
        /// Let the controller know the wizard UI is closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WizardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this._controller.OnClosing();
        }

        public bool NextEnabled
        {
            get { return buttonNext.Enabled; }
            set { buttonNext.Enabled = value; }
        }

        public bool CancelEnabled
        {
            get
            {
                return buttonCancel.Enabled;
            }
            set
            {
                // cancel button
                buttonCancel.Enabled = value;

                // close menu
                User32.EnableMenuItem(_hSystemMenu, SC.CLOSE.ToUInt32(),
                    MF.BYCOMMAND | (value ? MF.ENABLED : MF.DISABLED));
            }
        }

        public void FocusNext()
        {
            if (buttonNext.Enabled)
                buttonNext.Focus();
        }

        protected override void OnLoad(EventArgs e)
        {
            // sync top-most state (in case we are running in Deskbar)
            Form owner = this.Owner;
            if (owner != null)
                this.TopMost = owner.TopMost;

            // save a reference to the system menu
            _hSystemMenu = User32.GetSystemMenu(Handle, false);
            Trace.Assert(_hSystemMenu != IntPtr.Zero);

            base.OnLoad(e); // This needs to be called before _controller.OnWizardLoad so that scaling happens before the panel is added. (bugs 599809, 606698)

            _controller.OnWizardLoad();

            LayoutHelper.EqualizeButtonWidthsHoriz(AnchorStyles.Right, buttonCancel.Width, int.MaxValue,
                                                   buttonBack, buttonNext, buttonCancel);
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
            this.buttonBack = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.wizardBody = new System.Windows.Forms.Panel();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            //
            // panelFooter
            //
            this.panelFooter.BackColor = System.Drawing.SystemColors.Control;
            this.panelFooter.Controls.Add(this.buttonBack);
            this.panelFooter.Controls.Add(this.buttonNext);
            this.panelFooter.Controls.Add(this.buttonCancel);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 275);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(390, 47);
            this.panelFooter.TabIndex = 10;
            this.panelFooter.Paint += new System.Windows.Forms.PaintEventHandler(this.panelFooter_Paint);
            //
            // buttonBack
            //
            this.buttonBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBack.Enabled = false;
            this.buttonBack.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonBack.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonBack.Location = new System.Drawing.Point(137, 12);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.TabIndex = 1;
            this.buttonBack.Text = "< &Back";
            this.buttonBack.Visible = false;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(303, 12);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            //
            // buttonNext
            //
            this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNext.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonNext.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonNext.Location = new System.Drawing.Point(218, 12);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.TabIndex = 2;
            this.buttonNext.Text = "&Next >";
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            //
            // wizardBody
            //
            this.wizardBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.wizardBody.Dock = DockStyle.Fill;
            this.wizardBody.Location = new System.Drawing.Point(8, 48);
            this.wizardBody.Name = "wizardBody";
            this.wizardBody.Size = new System.Drawing.Size(376, 270);
            this.wizardBody.TabIndex = 0;
            //
            // panelHeader
            //
            this.panelHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(400, 40);
            this.panelHeader.TabIndex = 4;
            //
            // WizardForm
            //
            this.AcceptButton = this.buttonNext;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(394, 358);
            this.Controls.Add(this.wizardBody);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.panelFooter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WizardForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Wizard Dialog";
            this.ResumeLayout(false);

        }
        #endregion

        private void panelFooter_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(SystemPens.ControlDark, 0, 0, panelFooter.ClientSize.Width, 0);
        }

        public StringId? Header
        {
            set
            {
                Control headerControl = CreateHeaderFromString(value);

                panelHeader.Controls.Clear();
                if (headerControl != null)
                {
                    panelHeader.Controls.Add(headerControl);
                    headerControl.Size = new Size(panelHeader.Width, panelHeader.Height);
                    headerControl.Location = new Point(0, 0);

                    headerControl.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                        | AnchorStyles.Left)
                        | AnchorStyles.Right)));
                }

                if (headerControl == null)
                {
                    if (this.wizardBody.Location.Y >= panelHeader.Location.Y + panelHeader.Height)
                    {
                        int yShift = panelHeader.Height;
                        this.wizardBody.Location = new Point(this.wizardBody.Location.X, this.wizardBody.Location.Y - yShift);
                        this.wizardBody.Height += yShift;
                        panelHeader.Visible = false;
                    }
                }
                else
                {
                    if (this.wizardBody.Location.Y < panelHeader.Location.Y + panelHeader.Height)
                    {
                        int yShift = panelHeader.Height;
                        this.wizardBody.Location = new Point(this.wizardBody.Location.X, this.wizardBody.Location.Y + yShift);
                        this.wizardBody.Height -= yShift;
                        panelHeader.Visible = true;
                    }
                }
            }
        }

        protected virtual Control CreateHeaderFromString(StringId? value)
        {
            return null;
        }

        protected virtual void SetWizardBody(Control control)
        {
            wizardBody.Controls.Clear();
            wizardBody.Controls.Add(control);

            control.Location = new Point(0, 0);
            control.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                | AnchorStyles.Left)
                | AnchorStyles.Right)));

            if (!_scaledControls.Contains(control)) //only scale controls one time
            {
                DisplayHelper.Scale(control);
                OnAddControl(control);
                _scaledControls.Add(control);
            }
            control.Size = new Size(wizardBody.Width, wizardBody.Height);

            // This may get called multiple times per control, and that's OK, they are designed for it.
            BidiHelper.RtlLayoutFixup(control);
        }
        private readonly List<Control> _scaledControls = new List<Control>();

        protected virtual void OnAddControl(Control c) { }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            try
            {
                _controller.back();
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(this, ex);
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            try
            {
                _controller.next();
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(this, ex);
            }
        }

        /// <summary>
        /// Displays the current wizard step.
        /// </summary>
        internal protected void DisplayWizardStep(WizardStep step, int currentStep, int totalSteps)
        {
            SuspendLayout();
            try
            {
                //step.Control.Visible = true;
                SetWizardBody(step.Control);

                this.Header = step.Header;
                if (step.NextButtonLabel != null)
                {
                    NextButtonText = step.NextButtonLabel;
                }
                else
                {
                    if (currentStep == totalSteps - 1)
                    {
                        NextButtonText = BUTTON_TEXT_FINISH;
                    }
                    else
                    {
                        NextButtonText = BUTTON_TEXT_NEXT;
                    }
                }

                if (currentStep == 0)
                {
                    buttonBack.Enabled = false;
                }
                else
                {
                    buttonBack.Enabled = true;
                }

                this.buttonBack.Visible = totalSteps > 1;

                //drive focus to the new body control (only if the step control didn't manually drive
                //focus to one of its sub-controls)
                if (step.Control.ActiveControl == null)
                {
                    if (step.WantsFocus)
                    {
                        wizardBody.Select();
                        wizardBody.SelectNextControl(wizardBody, true, true, true, true);
                    }
                    else
                    {
                        buttonNext.Select();
                    }
                }
            }
            finally
            {
                ResumeLayout();
            }
        }

        /// <summary>
        /// Cancel and close the wizard.
        /// </summary>
        internal void Cancel()
        {
            this.Close();
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Sets the visibility of the wizard buttons.
        /// </summary>
        protected bool ButtonsVisible
        {
            set
            {
                this.buttonNext.Visible = value;
                this.buttonCancel.Visible = value;
                this.buttonBack.Visible = value;
            }
        }

        internal void FocusBackButton()
        {
            buttonBack.Focus();
        }

        /// <summary>
        /// Gets/Sets the text of the next button.
        /// </summary>
        protected internal string NextButtonText
        {
            get
            {
                return this.buttonNext.Text;
            }
            set
            {
                this.buttonNext.Text = value;
            }
        }

        /// <summary>
        /// Overridable method that lets subclasses handle the completion of the Wizard.
        /// </summary>
        virtual protected internal void Finish()
        {
            this.DialogResult = DialogResult.OK;
        }

    }
}
