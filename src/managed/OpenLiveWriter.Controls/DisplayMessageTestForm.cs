// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Summary description for DisplayMessageTestForm.
    /// </summary>
    public class DisplayMessageTestForm : BaseForm
    {
        private System.Windows.Forms.ListBox lstDisplayMessages;
        private System.Windows.Forms.GroupBox grpInfo;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.TextBox txtCaption;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnShow;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public DisplayMessageTestForm()
        {
            InitializeComponent();
        }

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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string[] messageIds = Enum.GetNames(typeof(MessageId));
            string[] messageIdsMinusNone = new string[messageIds.Length - 1];
            Array.Copy(messageIds, 1, messageIdsMinusNone, 0, messageIdsMinusNone.Length);
            lstDisplayMessages.Items.AddRange(messageIdsMinusNone);
            if (messageIdsMinusNone.Length > 0)
                lstDisplayMessages.SelectedIndex = 0;
        }

        private void lstDisplayMessages_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            DisplayMessage displayMessage = new DisplayMessage(MessageId);
            string title = displayMessage.Title;
            string text = displayMessage.Text;
            txtCaption.Text = title;
            txtMessage.Text = text;
        }

        private MessageId MessageId
        {
            get
            {
                string messageIdStr = (string)lstDisplayMessages.SelectedItem;
                return (MessageId)Enum.Parse(typeof(MessageId), messageIdStr);
            }
        }

        private void ShowSelectedMessage()
        {
            object[] dummyParams = new object[20];
            for (int i = 0; i < dummyParams.Length; i++)
                dummyParams[i] = "{" + i.ToString(CultureInfo.InvariantCulture) + "}";
            DisplayMessage.Show(MessageId, this, dummyParams);
        }

        private void btnShow_Click(object sender, System.EventArgs e)
        {
            ShowSelectedMessage();
        }

        private void lstDisplayMessages_DoubleClick(object sender, System.EventArgs e)
        {
            ShowSelectedMessage();
        }

        private void btnClose_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstDisplayMessages = new System.Windows.Forms.ListBox();
            this.grpInfo = new System.Windows.Forms.GroupBox();
            this.btnShow = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.txtCaption = new System.Windows.Forms.TextBox();
            this.lblCaption = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpInfo.SuspendLayout();
            this.SuspendLayout();
            //
            // lstDisplayMessages
            //
            this.lstDisplayMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDisplayMessages.Location = new System.Drawing.Point(8, 8);
            this.lstDisplayMessages.Name = "lstDisplayMessages";
            this.lstDisplayMessages.Size = new System.Drawing.Size(376, 212);
            this.lstDisplayMessages.TabIndex = 0;
            this.lstDisplayMessages.DoubleClick += new System.EventHandler(this.lstDisplayMessages_DoubleClick);
            this.lstDisplayMessages.SelectedIndexChanged += new System.EventHandler(this.lstDisplayMessages_SelectedIndexChanged);
            //
            // grpInfo
            //
            this.grpInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.grpInfo.Controls.Add(this.btnShow);
            this.grpInfo.Controls.Add(this.txtMessage);
            this.grpInfo.Controls.Add(this.lblMessage);
            this.grpInfo.Controls.Add(this.txtCaption);
            this.grpInfo.Controls.Add(this.lblCaption);
            this.grpInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpInfo.Location = new System.Drawing.Point(8, 224);
            this.grpInfo.Name = "grpInfo";
            this.grpInfo.Size = new System.Drawing.Size(376, 192);
            this.grpInfo.TabIndex = 1;
            this.grpInfo.TabStop = false;
            //
            // btnShow
            //
            this.btnShow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnShow.Location = new System.Drawing.Point(8, 160);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(80, 23);
            this.btnShow.TabIndex = 4;
            this.btnShow.Text = "&Show";
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            //
            // txtMessage
            //
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.Location = new System.Drawing.Point(8, 80);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMessage.Size = new System.Drawing.Size(360, 72);
            this.txtMessage.TabIndex = 3;
            this.txtMessage.Text = "";
            //
            // lblMessage
            //
            this.lblMessage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblMessage.Location = new System.Drawing.Point(8, 64);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.TabIndex = 2;
            this.lblMessage.Text = "Message";
            //
            // txtCaption
            //
            this.txtCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCaption.Location = new System.Drawing.Point(8, 32);
            this.txtCaption.Name = "txtCaption";
            this.txtCaption.ReadOnly = true;
            this.txtCaption.Size = new System.Drawing.Size(360, 20);
            this.txtCaption.TabIndex = 1;
            this.txtCaption.Text = "";
            //
            // lblCaption
            //
            this.lblCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblCaption.Location = new System.Drawing.Point(8, 16);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.TabIndex = 0;
            this.lblCaption.Text = "Caption";
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnClose.Location = new System.Drawing.Point(304, 424);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(80, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "&Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // DisplayMessageTestForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(392, 452);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.grpInfo);
            this.Controls.Add(this.lstDisplayMessages);
            this.Name = "DisplayMessageTestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Display Messages";
            this.grpInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
