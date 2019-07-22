namespace OpenLiveWriter.PostEditor.Configuration.StaticSite
{
    partial class StaticSiteAdvancedConfigForm
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.applyBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.okBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Location = new System.Drawing.Point(2, 2);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(245, 270);
            this.tabControl.TabIndex = 0;
            // 
            // applyBtn
            // 
            this.applyBtn.Location = new System.Drawing.Point(355, 481);
            this.applyBtn.Name = "applyBtn";
            this.applyBtn.Size = new System.Drawing.Size(75, 23);
            this.applyBtn.TabIndex = 1;
            this.applyBtn.Text = "Apply";
            this.applyBtn.UseVisualStyleBackColor = true;
            this.applyBtn.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(274, 481);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 2;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(193, 481);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 3;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // StaticSiteAdvancedConfigForm
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(450, 550);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.applyBtn);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StaticSiteAdvancedConfigForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Static Site Advanced Configuration";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Button applyBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button okBtn;
    }
}