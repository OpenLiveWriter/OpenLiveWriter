using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    partial class SeparatorControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SeparatorControl));
            this.pictureBoxSeparator = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSeparator)).BeginInit();
            this.SuspendLayout();
            //
            // pictureBoxSeparator
            //
            this.pictureBoxSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSeparator.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxSeparator.Image")));
            this.pictureBoxSeparator.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxSeparator.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxSeparator.Name = "pictureBoxSeparator";
            this.pictureBoxSeparator.Size = new System.Drawing.Size(186, 4);
            this.pictureBoxSeparator.TabIndex = 0;
            this.pictureBoxSeparator.TabStop = false;
            this.pictureBoxSeparator.SizeMode = PictureBoxSizeMode.StretchImage;
            //
            // SeparatorControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.pictureBoxSeparator);
            this.Name = "SeparatorControl";
            this.Size = new System.Drawing.Size(186, 4);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSeparator)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxSeparator;
    }
}