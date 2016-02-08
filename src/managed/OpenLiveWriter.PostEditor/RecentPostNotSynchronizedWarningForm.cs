// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Summary description for RecentPostNotSynchronizedWarningForm.
    /// </summary>
    public class RecentPostNotSynchronizedWarningForm : ApplicationDialog
    {
        private PictureBox pictureBoxWarning;
        private Label labelUnableToRetrieve;
        private Label labelTitle;
        private Label labelCanStillEditLocally;
        private Button buttonOK;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public RecentPostNotSynchronizedWarningForm(string entityName)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.labelUnableToRetrieve.Text = Res.Get(StringId.RecentPostNotSyncText);
            this.labelTitle.Text = Res.Get(StringId.RecentPostNotSyncCaption);
            this.labelCanStillEditLocally.Text = Res.Get(StringId.RecentPostNotSyncText2);
            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.Text = Res.Get(StringId.RecentPostNotSyncTitle);

            this.labelTitle.Font = Res.GetFont(FontSize.XLarge, FontStyle.Bold);
            Text = String.Format(CultureInfo.CurrentCulture, this.Text, entityName);
            labelTitle.Text = String.Format(CultureInfo.CurrentCulture, labelTitle.Text, entityName);
            labelUnableToRetrieve.Text = String.Format(CultureInfo.CurrentCulture, labelUnableToRetrieve.Text, ApplicationEnvironment.ProductNameQualified, entityName.ToLower(CultureInfo.CurrentCulture));
            labelCanStillEditLocally.Text = String.Format(CultureInfo.CurrentCulture, labelCanStillEditLocally.Text, entityName.ToLower(CultureInfo.CurrentCulture));

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, true))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(8, labelTitle, labelUnableToRetrieve, labelCanStillEditLocally);
                LayoutHelper.DistributeVertically(12, false, labelCanStillEditLocally, buttonOK);
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(RecentPostNotSynchronizedWarningForm));
            this.pictureBoxWarning = new System.Windows.Forms.PictureBox();
            this.labelUnableToRetrieve = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelCanStillEditLocally = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // pictureBoxWarning
            //
            this.pictureBoxWarning.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxWarning.Image")));
            this.pictureBoxWarning.Location = new System.Drawing.Point(10, 10);
            this.pictureBoxWarning.Name = "pictureBoxWarning";
            this.pictureBoxWarning.Size = new System.Drawing.Size(39, 40);
            this.pictureBoxWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxWarning.TabIndex = 0;
            this.pictureBoxWarning.TabStop = false;
            //
            // labelUnableToRetrieve
            //
            this.labelUnableToRetrieve.FlatStyle = FlatStyle.System;
            this.labelUnableToRetrieve.Location = new System.Drawing.Point(61, 52);
            this.labelUnableToRetrieve.Name = "labelUnableToRetrieve";
            this.labelUnableToRetrieve.Size = new System.Drawing.Size(260, 46);
            this.labelUnableToRetrieve.TabIndex = 1;
            this.labelUnableToRetrieve.Text = "{0} was unable to retrieve an up to date copy of the {1} from the weblog server (" +
                "an error occurred or the retrieve was cancelled).";
            //
            // labelTitle
            //
            this.labelTitle.FlatStyle = FlatStyle.System;
            this.labelTitle.Font = Res.GetFont(FontSize.XLarge, FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(61, 21);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(237, 23);
            this.labelTitle.TabIndex = 2;
            this.labelTitle.Text = "Unable to Retrieve {0} from Server";
            //
            // labelCanStillEditLocally
            //
            this.labelCanStillEditLocally.FlatStyle = FlatStyle.System;
            this.labelCanStillEditLocally.Location = new System.Drawing.Point(61, 103);
            this.labelCanStillEditLocally.Name = "labelCanStillEditLocally";
            this.labelCanStillEditLocally.Size = new System.Drawing.Size(260, 46);
            this.labelCanStillEditLocally.TabIndex = 3;
            this.labelCanStillEditLocally.Text = "You may still edit the {0} using the local copy stored on this computer. However " +
                "if you have changed the {0} online the copy you are editing is out of date.";
            //
            // buttonOK
            //
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(130, 159);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            //
            // RecentPostNotSynchronizedWarningForm
            //
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(335, 196);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelCanStillEditLocally);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelUnableToRetrieve);
            this.Controls.Add(this.pictureBoxWarning);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RecentPostNotSynchronizedWarningForm";
            this.Text = "Unable to Retrieve {0} from Server";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
