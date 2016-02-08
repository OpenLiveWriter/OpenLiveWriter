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

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Summary description for ExpirationForm.
    /// </summary>
    public class ExpirationForm : ApplicationDialog
    {
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelBody;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public static DialogResult ShowExpiredDialog(int daysRemaining)
        {
            ExpirationForm form = new ExpirationForm();

            string title;
            string message;
            if (daysRemaining <= 0)
            {
                title = Res.Get(StringId.BetaExpiredTitle);
                message = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.BetaExpiredMessage), ApplicationEnvironment.ProductNameQualified);
            }
            else
            {
                title = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.BetaDaysRemaining), daysRemaining);
                if (daysRemaining == 1)
                    title = Res.Get(StringId.Beta1DayRemaining);
                message = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.BetaRemainingMessage), ApplicationEnvironment.ProductNameQualified, ExpirationSettings.Expires.ToShortDateString());
            }

            form.Title = title;
            form.Text = Res.Get(StringId.BetaExpiredDialogTitle);
            form.Message = message;
            return form.ShowDialog();
        }

        public ExpirationForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            Icon = ApplicationEnvironment.ProductIcon;

            pictureBox.Image = ResourceHelper.LoadAssemblyResourceBitmap("Images.AboutBoxImageSmall.png");
            labelTitle.Font = Res.GetFont(FontSize.XXLarge, FontStyle.Regular);
            buttonOk.Text = Res.Get(StringId.BetaExpiredDownloadNow);
            buttonCancel.Text = Res.Get(StringId.BetaExpiredAskLater);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, true))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(16, labelBody, new ControlGroup(buttonOk, buttonCancel));
                LayoutHelper.FixupOKCancel(buttonOk, buttonCancel);

                if (buttonOk.Left < labelBody.Left)
                {
                    // go to over/under
                    LayoutHelper.DistributeVertically(10, false, buttonOk, buttonCancel);
                    buttonOk.Left = buttonCancel.Left = labelBody.Left;
                }
            }
        }

        public string Title
        {
            get
            {
                return labelTitle.Text;
            }
            set
            {
                labelTitle.Text = value;
            }
        }

        public string Message
        {
            get
            {
                return labelBody.Text;
            }
            set
            {
                labelBody.Text = value;
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ExpirationForm));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelBody = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // pictureBox
            //
            this.pictureBox.Location = new System.Drawing.Point(16, 22);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(48, 51);
            this.pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.RightToLeft = RightToLeft.No;
            //
            // labelTitle
            //
            this.labelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTitle.Location = new System.Drawing.Point(72, 17);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(264, 35);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "Beta Expires in 4 weeks.";
            //
            // labelBody
            //
            this.labelBody.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelBody.Location = new System.Drawing.Point(72, 43);
            this.labelBody.Name = "labelBody";
            this.labelBody.Size = new System.Drawing.Size(264, 69);
            this.labelBody.TabIndex = 2;
            this.labelBody.Text = "The current beta version of Open Live Writer will expire on <date>. Please dow" +
                "nload and install a newer version.";
            //
            // buttonOk
            //
            this.buttonOk.BackColor = System.Drawing.SystemColors.Control;
            this.buttonOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOk.Location = new System.Drawing.Point(72, 120);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(128, 25);
            this.buttonOk.TabIndex = 3;
            this.buttonOk.Text = "Download Now";
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.BackColor = System.Drawing.SystemColors.Control;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(208, 120);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(128, 25);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Ask Me Later";
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            //
            // ExpirationForm
            //
            this.AcceptButton = this.buttonOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(346, 158);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.labelBody);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.pictureBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExpirationForm";
            this.ShowInTaskbar = true;
            this.Text = "Beta Expiration";
            this.ResumeLayout(false);

        }
        #endregion

        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonOk_Click(object sender, System.EventArgs e)
        {
            ShellHelper.LaunchUrl(GLink.Instance.DownloadUpdatedWriter);
            DialogResult = DialogResult.OK;
        }
    }

    public class ExpirationSettings
    {
        public static int LastWarnDays
        {
            get
            {
                return ApplicationEnvironment.UserSettingsRoot.GetInt32(LASTWARN, -1);
            }
            set
            {
                ApplicationEnvironment.UserSettingsRoot.SetInt32(LASTWARN, value);
            }
        }
        private const string LASTWARN = "LastWarn";

        public static DateTime Expires
        {
            get
            {
                return ApplicationEnvironment.UserSettingsRoot.GetDateTime(EXPIRES, DateTime.MaxValue);
            }
            set
            {
                ApplicationEnvironment.UserSettingsRoot.SetDateTime(EXPIRES, value);
            }
        }

        private const string EXPIRES = "Expires";

        public static int DaysRemaining
        {
            get
            {
                return Expires.Subtract(DateTime.Now.Date).Days;
            }
        }
    }
}
