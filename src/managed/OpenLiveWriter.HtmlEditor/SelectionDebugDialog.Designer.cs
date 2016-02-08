namespace OpenLiveWriter.HtmlEditor
{
    partial class SelectionDebugDialog
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
            this.listBoxSelection = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            //
            // listBoxSelection
            //
            this.listBoxSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxSelection.FormattingEnabled = true;
            this.listBoxSelection.Location = new System.Drawing.Point(0, 0);
            this.listBoxSelection.Name = "listBoxSelection";
            this.listBoxSelection.Size = new System.Drawing.Size(571, 420);
            this.listBoxSelection.TabIndex = 0;
            this.listBoxSelection.SelectedIndexChanged += new System.EventHandler(this.listBoxSelection_SelectedIndexChanged);
            //
            // SelectionDebugDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 420);
            this.Controls.Add(this.listBoxSelection);
            this.Name = "SelectionDebugDialog";
            this.Text = "SelectionDebugDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxSelection;
    }
}