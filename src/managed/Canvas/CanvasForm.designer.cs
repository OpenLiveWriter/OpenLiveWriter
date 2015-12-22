namespace OpenLiveWriter.Test
{
    partial class CanvasForm
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
            this.panelCanvas = new System.Windows.Forms.Panel();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.buttonSource = new System.Windows.Forms.Button();
            this.buttonPlain = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // panelCanvas
            //
            this.panelCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCanvas.Location = new System.Drawing.Point(-1, 1);
            this.panelCanvas.Name = "panelCanvas";
            this.panelCanvas.Size = new System.Drawing.Size(439, 392);
            this.panelCanvas.TabIndex = 0;
            //
            // buttonEdit
            //
            this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEdit.Location = new System.Drawing.Point(459, 12);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(75, 23);
            this.buttonEdit.TabIndex = 1;
            this.buttonEdit.Text = "Edit";
            this.buttonEdit.UseVisualStyleBackColor = true;
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            //
            // buttonPreview
            //
            this.buttonPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPreview.Location = new System.Drawing.Point(459, 42);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(75, 23);
            this.buttonPreview.TabIndex = 2;
            this.buttonPreview.Text = "Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.buttonPreview_Click);
            //
            // buttonSource
            //
            this.buttonSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSource.Location = new System.Drawing.Point(459, 72);
            this.buttonSource.Name = "buttonSource";
            this.buttonSource.Size = new System.Drawing.Size(75, 23);
            this.buttonSource.TabIndex = 3;
            this.buttonSource.Text = "Source";
            this.buttonSource.UseVisualStyleBackColor = true;
            this.buttonSource.Click += new System.EventHandler(this.buttonSource_Click);
            //
            // buttonPlain
            //
            this.buttonPlain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPlain.Location = new System.Drawing.Point(459, 101);
            this.buttonPlain.Name = "buttonPlain";
            this.buttonPlain.Size = new System.Drawing.Size(75, 23);
            this.buttonPlain.TabIndex = 4;
            this.buttonPlain.Text = "Plain";
            this.buttonPlain.UseVisualStyleBackColor = true;
            this.buttonPlain.Click += new System.EventHandler(this.buttonPlain_Click);
            //
            // CanvasForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 390);
            this.Controls.Add(this.buttonPlain);
            this.Controls.Add(this.buttonSource);
            this.Controls.Add(this.buttonPreview);
            this.Controls.Add(this.buttonEdit);
            this.Controls.Add(this.panelCanvas);
            this.Name = "CanvasForm";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelCanvas;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.Button buttonPreview;
        private System.Windows.Forms.Button buttonSource;
        private System.Windows.Forms.Button buttonPlain;
    }
}

