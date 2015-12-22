namespace OpenLiveWriter.PostEditor.Autoreplace
{
    partial class AutoreplaceEditForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.textBoxPhrase = new System.Windows.Forms.TextBox();
            this.labelPhrase = new System.Windows.Forms.Label();
            this.textBoxReplace = new System.Windows.Forms.TextBox();
            this.labelReplace = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(279, 260);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 25;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(198, 260);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 20;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // textBoxPhrase
            //
            this.textBoxPhrase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPhrase.Location = new System.Drawing.Point(12, 25);
            this.textBoxPhrase.Name = "textBoxPhrase";
            this.textBoxPhrase.Size = new System.Drawing.Size(342, 21);
            this.textBoxPhrase.TabIndex = 5;
            //
            // labelPhrase
            //
            this.labelPhrase.AutoSize = true;
            this.labelPhrase.Location = new System.Drawing.Point(9, 9);
            this.labelPhrase.Name = "labelPhrase";
            this.labelPhrase.Size = new System.Drawing.Size(86, 13);
            this.labelPhrase.TabIndex = 0;
            this.labelPhrase.Text = "Word or Phrase:";
            //
            // textBoxReplace
            //
            this.textBoxReplace.AcceptsReturn = true;
            this.textBoxReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxReplace.Location = new System.Drawing.Point(12, 65);
            this.textBoxReplace.Multiline = true;
            this.textBoxReplace.Name = "textBoxReplace";
            this.textBoxReplace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxReplace.Size = new System.Drawing.Size(342, 189);
            this.textBoxReplace.TabIndex = 15;
            //
            // labelReplace
            //
            this.labelReplace.AutoSize = true;
            this.labelReplace.Location = new System.Drawing.Point(9, 49);
            this.labelReplace.Name = "labelReplace";
            this.labelReplace.Size = new System.Drawing.Size(102, 13);
            this.labelReplace.TabIndex = 10;
            this.labelReplace.Text = "Replacement value:";
            //
            // AutoreplaceEditForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(360, 290);
            this.Controls.Add(this.labelReplace);
            this.Controls.Add(this.textBoxReplace);
            this.Controls.Add(this.labelPhrase);
            this.Controls.Add(this.textBoxPhrase);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Name = "AutoreplaceEditForm";
            this.Text = "Auto Replace";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox textBoxPhrase;
        private System.Windows.Forms.Label labelPhrase;
        private System.Windows.Forms.TextBox textBoxReplace;
        private System.Windows.Forms.Label labelReplace;
    }
}