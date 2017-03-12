using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    partial class SidebarHeaderControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelHeading = new System.Windows.Forms.Label();
            this.linkLabel = new System.Windows.Forms.LinkLabel();
            this.linkLabelOptional = new System.Windows.Forms.LinkLabel();
            this.separatorControl = new OpenLiveWriter.ApplicationFramework.SeparatorControl();
            this.SuspendLayout();
            //
            // labelHeading
            //
            this.labelHeading.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelHeading.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeading.Location = new System.Drawing.Point(0, 0);
            this.labelHeading.Name = "labelHeading";
            this.labelHeading.Size = new System.Drawing.Size(192, 19);
            this.labelHeading.UseMnemonic = false;
            this.labelHeading.TabIndex = 1;
            //
            // linkLabel
            //
            this.linkLabel.Location = new System.Drawing.Point(1, 19);
            this.linkLabel.Name = "linkLabel";
            this.linkLabel.Size = new System.Drawing.Size(192, 16);
            this.linkLabel.TabIndex = 2;
            this.linkLabel.LinkColor = SystemColors.HotTrack;
            this.linkLabel.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabel.UseMnemonic = false;
            this.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.linkLabel.UseCompatibleTextRendering = false;
            //
            // linkLabelOptional
            //
            this.linkLabelOptional.Location = new System.Drawing.Point(1, 35);
            this.linkLabelOptional.Name = "linkLabelOptional";
            this.linkLabelOptional.Size = new System.Drawing.Size(192, 15);
            this.linkLabelOptional.TabIndex = 4;
            this.linkLabelOptional.LinkColor = SystemColors.HotTrack;
            this.linkLabelOptional.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabelOptional.UseMnemonic = false;
            this.linkLabelOptional.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOptional_LinkClicked);
            this.linkLabelOptional.UseCompatibleTextRendering = false;
            //
            // separatorControl
            //
            this.separatorControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left;
            this.separatorControl.Location = new System.Drawing.Point(0, 69);
            this.separatorControl.Name = "separatorControl";
            this.separatorControl.Size = new System.Drawing.Size(186, 4);
            this.separatorControl.TabIndex = 3;
            //
            // SidebarHeaderControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.linkLabelOptional);
            this.Controls.Add(this.separatorControl);
            this.Controls.Add(this.linkLabel);
            this.Controls.Add(this.labelHeading);
            this.Name = "SidebarHeaderControl";
            this.Size = new System.Drawing.Size(192, 93);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelHeading;
        private System.Windows.Forms.LinkLabel linkLabel;
        private SeparatorControl separatorControl;
        private System.Windows.Forms.LinkLabel linkLabelOptional;
    }
}