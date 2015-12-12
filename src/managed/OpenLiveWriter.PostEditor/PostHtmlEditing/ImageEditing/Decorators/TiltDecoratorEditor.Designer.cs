namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    partial class TiltDecoratorEditor
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
            this.textBoxTilt = new System.Windows.Forms.TextBox();
            this.trackBarTilt = new System.Windows.Forms.TrackBar();
            this.labelTilt = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTilt)).BeginInit();
            this.SuspendLayout();
            //
            // textBoxTilt
            //
            this.textBoxTilt.AcceptsReturn = true;
            this.textBoxTilt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTilt.Location = new System.Drawing.Point(224, 18);
            this.textBoxTilt.Name = "textBoxTilt";
            this.textBoxTilt.Size = new System.Drawing.Size(32, 20);
            this.textBoxTilt.TabIndex = 10;
            this.textBoxTilt.Text = "0";
            this.textBoxTilt.TextChanged += new System.EventHandler(this.textBoxTilt_TextChanged);
            this.textBoxTilt.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxTilt_KeyDown);
            this.textBoxTilt.Leave += new System.EventHandler(this.textBoxTilt_Leave);
            //
            // trackBarTilt
            //
            this.trackBarTilt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarTilt.Location = new System.Drawing.Point(3, 18);
            this.trackBarTilt.Maximum = 20;
            this.trackBarTilt.Minimum = -20;
            this.trackBarTilt.Name = "trackBarTilt";
            this.trackBarTilt.Size = new System.Drawing.Size(215, 45);
            this.trackBarTilt.TabIndex = 9;
            this.trackBarTilt.TickFrequency = 5;
            this.trackBarTilt.ValueChanged += new System.EventHandler(this.trackBarTilt_ValueChanged);
            this.trackBarTilt.KeyUp += new System.Windows.Forms.KeyEventHandler(this.trackBarTilt_KeyUp);
            this.trackBarTilt.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBarTilt_MouseUp);
            //
            // labelTilt
            //
            this.labelTilt.Location = new System.Drawing.Point(3, 2);
            this.labelTilt.Name = "labelTilt";
            this.labelTilt.Size = new System.Drawing.Size(100, 16);
            this.labelTilt.TabIndex = 8;
            this.labelTilt.Text = "Tilt:";
            //
            // btnOK
            //
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(100, 78);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(181, 78);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            //
            // TiltDecoratorEditor
            //
            this.Controls.Add(this.textBoxTilt);
            this.Controls.Add(this.trackBarTilt);
            this.Controls.Add(this.labelTilt);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Name = "TiltDecoratorEditor";
            this.Size = new System.Drawing.Size(262, 112);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTilt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTilt;
        private System.Windows.Forms.TrackBar trackBarTilt;
        private System.Windows.Forms.Label labelTilt;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
