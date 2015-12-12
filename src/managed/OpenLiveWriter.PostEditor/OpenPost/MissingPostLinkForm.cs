// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls ;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor ;

namespace OpenLiveWriter.PostEditor.OpenPost
{
    /// <summary>
    /// Summary description for MissingPostLinkForm.
    /// </summary>
    public class MissingPostLinkForm : ApplicationDialog
    {
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.PictureBox pictureBoxError;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelExplanation;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public MissingPostLinkForm(PostInfo postInfo)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.labelTitle.Text = Res.Get(StringId.MissingPostLinkCaption);
            this.labelExplanation.Text = Res.Get(StringId.MissingPostLinkExplanation);
            this.Text = Res.Get(StringId.MissingPostLinkTitle);

            this.labelTitle.Font = Res.GetFont(FontSize.XLarge, FontStyle.Bold);
            string entityName = postInfo.IsPage ? Res.Get(StringId.Page) : Res.Get(StringId.Post) ;
            string entityNameLower = postInfo.IsPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower) ;

            labelTitle.Text = String.Format(CultureInfo.CurrentCulture, labelTitle.Text, entityName ) ;
            labelExplanation.Text = String.Format(CultureInfo.CurrentCulture, labelExplanation.Text, entityNameLower, ApplicationEnvironment.ProductName) ;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);

            using (new AutoGrow(this, AnchorStyles.Bottom, true))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(8, labelTitle, labelExplanation);
                LayoutHelper.FitControlsBelow(12, labelExplanation);
            }
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MissingPostLinkForm));
            this.buttonOK = new System.Windows.Forms.Button();
            this.pictureBoxError = new System.Windows.Forms.PictureBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelExplanation = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(130, 152);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            //
            // pictureBoxError
            //
            this.pictureBoxError.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxError.Image")));
            this.pictureBoxError.Location = new System.Drawing.Point(10, 10);
            this.pictureBoxError.Name = "pictureBoxError";
            this.pictureBoxError.Size = new System.Drawing.Size(39, 40);
            this.pictureBoxError.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxError.TabIndex = 5;
            this.pictureBoxError.TabStop = false;
            //
            // labelTitle
            //
            this.labelTitle.Font = Res.GetFont(FontSize.XLarge, FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(61, 21);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(237, 23);
            this.labelTitle.TabIndex = 6;
            this.labelTitle.Text = "No Link Available for {0}";
            //
            // labelExplanation
            //
            this.labelExplanation.Location = new System.Drawing.Point(61, 48);
            this.labelExplanation.Name = "labelExplanation";
            this.labelExplanation.Size = new System.Drawing.Size(260, 96);
            this.labelExplanation.TabIndex = 7;
            this.labelExplanation.Text = @"There is no link available for the selected {0}. This may be because your weblog service does not make {0} links available to {1}. You may be able to resolve this problem by redetecting your weblog account configuration using the Accounts panel of the Weblog Settings dialog.";
            //
            // MissingPostLinkForm
            //
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(335, 184);
            this.ControlBox = false;
            this.Controls.Add(this.labelExplanation);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.pictureBoxError);
            this.Controls.Add(this.buttonOK);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MissingPostLinkForm";
            this.Text = "No Link Available";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
