// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Controls
{
    public class DisplayableExceptionDisplayForm : BaseForm
    {
        private Bitmap errorBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.ErrorLogoSmall.png");

        private PictureBox pictureBoxIcon;
        private Label labelMessage;
        private TextBox textBoxDetails;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private Button buttonOK;

        /// <summary>
        /// Initialize w/ required values
        /// </summary>
        public DisplayableExceptionDisplayForm(Exception exception)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.labelMessage.Font = Res.GetFont(FontSize.Large, FontStyle.Bold);
            this.buttonOK.Text = Res.Get(StringId.OKButtonText);

            // initialize controls
            Icon = ApplicationEnvironment.ProductIcon;
            pictureBoxIcon.Image = errorBitmap;

            DisplayableException displayableException = exception as DisplayableException;
            if (displayableException != null)
            {
                Text = EnsureStringValueProvided(displayableException.Title);
                labelMessage.Text = EnsureStringValueProvided(displayableException.Title);
                textBoxDetails.Text = EnsureStringValueProvided(displayableException.Text);

                // log the error
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "DisplayableException occurred: {0}", displayableException.ToString()));
            }
            else
            {
                // log error
                Trace.WriteLine("Non DisplayableException-derived exception thrown. Subsystems need to handle these exceptions and convert them to WriterExceptions:\r\n" + exception.ToString());

                // give the user a full data dump
                Text = Res.Get(StringId.UnexpectedErrorTitle);
                labelMessage.Text = exception.GetType().Name;
                textBoxDetails.Text = exception.ToString();
            }
        }

        public static void Show(IWin32Window owner, Exception ex)
        {
            using (DisplayableExceptionDisplayForm form = new DisplayableExceptionDisplayForm(ex))
                form.ShowDialog(owner);
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

        /// <summary>
        /// Ensure that we are warned in debug mode if a string value is not provided but
        /// fail gracefully in release mode
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string EnsureStringValueProvided(string value)
        {
            if (value == null || value.Length == 0)
            {
                Debug.Fail("Required string value not passed to DisplayableExceptionDisplayForm constructor");
                return String.Empty;
            }
            else
            {
                return value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using (new AutoGrow(this, AnchorStyles.Bottom, true))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(8, labelMessage, textBoxDetails, buttonOK);
                DisplayHelper.AutoFitSystemButton(buttonOK, buttonOK.Width, Int32.MaxValue);
                buttonOK.Left = (Width / 2) - (buttonOK.Width / 2);
            }

        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.labelMessage = new System.Windows.Forms.Label();
            this.textBoxDetails = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.SuspendLayout();
            //
            // pictureBoxIcon
            //
            this.pictureBoxIcon.Location = new System.Drawing.Point(9, 9);
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(39, 40);
            this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxIcon.TabIndex = 0;
            this.pictureBoxIcon.TabStop = false;
            //
            // labelMessage
            //
            this.labelMessage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelMessage.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.labelMessage.Location = new System.Drawing.Point(55, 19);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(258, 28);
            this.labelMessage.TabIndex = 1;
            this.labelMessage.Text = "Unable to connect to FTP server";
            //
            // textBoxDetails
            //
            this.textBoxDetails.Location = new System.Drawing.Point(55, 50);
            this.textBoxDetails.Multiline = true;
            this.textBoxDetails.Name = "textBoxDetails";
            this.textBoxDetails.ReadOnly = true;
            this.textBoxDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDetails.Size = new System.Drawing.Size(277, 115);
            this.textBoxDetails.TabIndex = 2;
            //
            // buttonOK
            //
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(135, 180);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            //
            // DisplayableExceptionDisplayForm
            //
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonOK;
            this.ClientSize = new System.Drawing.Size(349, 214);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxDetails);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.pictureBoxIcon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DisplayableExceptionDisplayForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Error Communicating with Weblog";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
