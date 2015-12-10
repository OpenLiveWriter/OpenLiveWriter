// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Project31.CoreServices;
using Project31.Interop.Windows;

namespace Project31.Controls
{
    /// <summary>
    /// Displays an information box.  This class is very similar to System.Windows.Forms.MessageBox
    /// with the exception being that InformationBox is designed to be used as a base form.
    /// </summary>
    public class InformationBox : System.Windows.Forms.Form
    {
        /// <summary>
        /// The body background bitmap.
        /// </summary>
        private static Bitmap bodyBackgroundBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.BodyBackground.png");

        /// <summary>
        /// Information box type enumeration.  Specifies which type of information box to display.
        /// </summary>
        public enum InformationBoxType
        {
            /// <summary>
            ///
            /// </summary>
            Information,

            /// <summary>
            ///
            /// </summary>
            Warning,

            /// <summary>
            ///
            /// </summary>
            Error
        }

        /// <summary>
        /// GetWindowLong and SetWindowLong. This stuff moves soon...
        /// </summary>
        private Project31.Controls.LabelControl labelControlText;
        private Project31.Controls.CheckBoxControl checkBoxControlAgain;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.Button buttonNo;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.Button buttonRetry;
        private System.Windows.Forms.Button buttonIgnore;
        private System.Windows.Forms.PictureBox pictureBoxError;
        private System.Windows.Forms.PictureBox pictureBoxWarning;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// The information box type.
        /// </summary>
        private InformationBoxType type;

        /// <summary>
        /// Gets or sets the information box type.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(false),
        Description("Specifies the the information box type.")
        ]
        public InformationBoxType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        /// <summary>
        /// The information box buttons.
        /// </summary>
        private MessageBoxButtons buttons;

        /// <summary>
        /// Gets or sets the information box buttons.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(false),
        Description("Specifies the the information box buttons.")
        ]
        public MessageBoxButtons Buttons
        {
            get
            {
                return buttons;
            }
            set
            {
                buttons = value;
            }
        }

        /// <summary>
        /// The text to display in the title bar of the information box.
        /// </summary>
        private string informationBoxTitle;

        /// <summary>
        /// Gets or sets the text to display in the title bar of the information box.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(true),
        Description("The text to display in the title bar of the information box.")
        ]
        public string InformationBoxTitle
        {
            get
            {
                return informationBoxTitle;
            }
            set
            {
                informationBoxTitle = value;
            }
        }

        /// <summary>
        /// The text to display in the information box.
        /// </summary>
        private string informationBoxText;

        /// <summary>
        /// Gets or sets the text to display in the information box.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(true),
        Description("Specifies the text to display in the information box.")
        ]
        public string InformationBoxText
        {
            get
            {
                return informationBoxText;
            }
            set
            {
                informationBoxText = value;
            }
        }

        /// <summary>
        /// Initializes a new instances of the InformationBox class.
        /// </summary>
        public InformationBox()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //	Initialize the object.
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instances of the PropertyEditorForm class.
        /// </summary>
        private void InitializeObject()
        {
            //	Turn off CS_CLIPCHILDREN.
            User32.SetWindowLong(Handle, GWL.STYLE, User32.GetWindowLong(Handle, GWL.STYLE) & ~WS.CLIPCHILDREN);

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(InformationBox));
            this.buttonOK = new System.Windows.Forms.Button();
            this.pictureBoxError = new System.Windows.Forms.PictureBox();
            this.labelControlText = new Project31.Controls.LabelControl();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxControlAgain = new Project31.Controls.CheckBoxControl();
            this.buttonYes = new System.Windows.Forms.Button();
            this.buttonNo = new System.Windows.Forms.Button();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.buttonRetry = new System.Windows.Forms.Button();
            this.buttonIgnore = new System.Windows.Forms.Button();
            this.pictureBoxWarning = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.buttonOK.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(6, 92);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&OK";
            this.buttonOK.Visible = false;
            //
            // pictureBoxError
            //
            this.pictureBoxError.Image = ((System.Drawing.Bitmap)(resources.GetObject("pictureBoxError.Image")));
            this.pictureBoxError.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBoxError.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxError.Name = "pictureBoxError";
            this.pictureBoxError.Size = new System.Drawing.Size(39, 40);
            this.pictureBoxError.TabIndex = 2;
            this.pictureBoxError.TabStop = false;
            //
            // labelControlText
            //
            this.labelControlText.BackColor = System.Drawing.Color.Transparent;
            this.labelControlText.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.labelControlText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelControlText.Location = new System.Drawing.Point(57, 12);
            this.labelControlText.Name = "labelControlText";
            this.labelControlText.Size = new System.Drawing.Size(325, 40);
            this.labelControlText.TabIndex = 3;
            this.labelControlText.Text = "Text.";
            this.labelControlText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.buttonCancel.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(6, 115);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.Visible = false;
            //
            // checkBoxControlAgain
            //
            this.checkBoxControlAgain.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.checkBoxControlAgain.Location = new System.Drawing.Point(57, 62);
            this.checkBoxControlAgain.Name = "checkBoxControlAgain";
            this.checkBoxControlAgain.Size = new System.Drawing.Size(288, 18);
            this.checkBoxControlAgain.TabIndex = 4;
            this.checkBoxControlAgain.Text = "Again.";
            //
            // buttonYes
            //
            this.buttonYes.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.buttonYes.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.buttonYes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonYes.Location = new System.Drawing.Point(81, 92);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.TabIndex = 6;
            this.buttonYes.Text = "&Yes";
            this.buttonYes.Visible = false;
            //
            // buttonNo
            //
            this.buttonNo.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.buttonNo.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.buttonNo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonNo.Location = new System.Drawing.Point(81, 115);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.TabIndex = 7;
            this.buttonNo.Text = "&No";
            this.buttonNo.Visible = false;
            //
            // buttonAbort
            //
            this.buttonAbort.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.buttonAbort.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.buttonAbort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAbort.Location = new System.Drawing.Point(156, 92);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.TabIndex = 8;
            this.buttonAbort.Text = "&Abort";
            this.buttonAbort.Visible = false;
            //
            // buttonRetry
            //
            this.buttonRetry.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.buttonRetry.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.buttonRetry.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRetry.Location = new System.Drawing.Point(156, 115);
            this.buttonRetry.Name = "buttonRetry";
            this.buttonRetry.TabIndex = 9;
            this.buttonRetry.Text = "&Retry";
            this.buttonRetry.Visible = false;
            //
            // buttonIgnore
            //
            this.buttonIgnore.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.buttonIgnore.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.buttonIgnore.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonIgnore.Location = new System.Drawing.Point(156, 138);
            this.buttonIgnore.Name = "buttonIgnore";
            this.buttonIgnore.TabIndex = 10;
            this.buttonIgnore.Text = "&Ignore";
            this.buttonIgnore.Visible = false;
            //
            // pictureBoxWarning
            //
            this.pictureBoxWarning.Image = ((System.Drawing.Bitmap)(resources.GetObject("pictureBoxWarning.Image")));
            this.pictureBoxWarning.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBoxWarning.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxWarning.Name = "pictureBoxWarning";
            this.pictureBoxWarning.Size = new System.Drawing.Size(39, 40);
            this.pictureBoxWarning.TabIndex = 11;
            this.pictureBoxWarning.TabStop = false;
            //
            // InformationBox
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(394, 164);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.buttonIgnore,
                                                                          this.buttonRetry,
                                                                          this.buttonAbort,
                                                                          this.buttonNo,
                                                                          this.buttonYes,
                                                                          this.buttonCancel,
                                                                          this.checkBoxControlAgain,
                                                                          this.labelControlText,
                                                                          this.buttonOK,
                                                                          this.pictureBoxWarning,
                                                                          this.pictureBoxError});
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InformationBox";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Title";
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Displays an information box.
        /// </summary>
        /// <returns>One of the DialogResult values.</returns>
        public new DialogResult ShowDialog()
        {
            //	Layout the controls.
            LayoutControls();

            //	Show the dialog.
            return base.ShowDialog();
        }

        /// <summary>
        /// Displays an information box.
        /// </summary>
        /// <param name="owner">The IWin32Window the message box will display in front of.</param>
        /// <returns>One of the DialogResult values.</returns>
        public new DialogResult ShowDialog(IWin32Window owner)
        {
            //	Layout the controls.
            LayoutControls();

            //	Show the dialog.
            return base.ShowDialog(owner);
        }

        /// <summary>
        /// Raises the PaintBackground event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            //	Fill the dialog body.
            GraphicsHelper.TileFillScaledImageHorizontally(	e.Graphics,
                                                            bodyBackgroundBitmap,
                                                            ClientRectangle);
        }

        /// <summary>
        /// Layout controls.
        /// </summary>
        private void LayoutControls()
        {
            switch (Type)
            {
                case InformationBoxType.Information:
                    break;

                case InformationBoxType.Warning:
                    break;

                case InformationBoxType.Error:
                    break;
            }
        }
    }
}
