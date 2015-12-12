namespace OpenLiveWriter.HtmlEditor
{
    partial class WordCountForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.gbTableHeader = new System.Windows.Forms.GroupBox();
            this.labelParagraphsValue = new System.Windows.Forms.Label();
            this.labelParagraphs = new System.Windows.Forms.Label();
            this.labelCharsNoSpacesValue = new System.Windows.Forms.Label();
            this.labelCharsValue = new System.Windows.Forms.Label();
            this.labelWordCountValue = new System.Windows.Forms.Label();
            this.labelCharsNoSpaces = new System.Windows.Forms.Label();
            this.labelChars = new System.Windows.Forms.Label();
            this.labelWordCount = new System.Windows.Forms.Label();
            this.gbTableHeader.SuspendLayout();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Selection Statistics";
            //
            // buttonClose
            //
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonClose.Location = new System.Drawing.Point(143, 133);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 7;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // gbTableHeader
            //
            this.gbTableHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTableHeader.Controls.Add(this.labelParagraphsValue);
            this.gbTableHeader.Controls.Add(this.labelParagraphs);
            this.gbTableHeader.Controls.Add(this.labelCharsNoSpacesValue);
            this.gbTableHeader.Controls.Add(this.labelCharsValue);
            this.gbTableHeader.Controls.Add(this.labelWordCountValue);
            this.gbTableHeader.Controls.Add(this.labelCharsNoSpaces);
            this.gbTableHeader.Controls.Add(this.labelChars);
            this.gbTableHeader.Controls.Add(this.labelWordCount);
            this.gbTableHeader.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbTableHeader.Location = new System.Drawing.Point(12, 12);
            this.gbTableHeader.Name = "gbTableHeader";
            this.gbTableHeader.Size = new System.Drawing.Size(206, 104);
            this.gbTableHeader.TabIndex = 8;
            this.gbTableHeader.TabStop = false;
            this.gbTableHeader.Text = "Selection Statistics";
            //
            // labelParagraphsValue
            //
            this.labelParagraphsValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelParagraphsValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelParagraphsValue.Location = new System.Drawing.Point(157, 81);
            this.labelParagraphsValue.Name = "labelParagraphsValue";
            this.labelParagraphsValue.Size = new System.Drawing.Size(43, 13);
            this.labelParagraphsValue.TabIndex = 14;
            this.labelParagraphsValue.Text = "999999";
            this.labelParagraphsValue.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            //
            // labelParagraphs
            //
            this.labelParagraphs.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelParagraphs.Location = new System.Drawing.Point(6, 81);
            this.labelParagraphs.Name = "labelParagraphs";
            this.labelParagraphs.Size = new System.Drawing.Size(62, 13);
            this.labelParagraphs.TabIndex = 13;
            this.labelParagraphs.Text = "Paragraphs";
            this.labelParagraphs.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // labelCharsNoSpacesValue
            //
            this.labelCharsNoSpacesValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCharsNoSpacesValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCharsNoSpacesValue.Location = new System.Drawing.Point(157, 61);
            this.labelCharsNoSpacesValue.Name = "labelCharsNoSpacesValue";
            this.labelCharsNoSpacesValue.Size = new System.Drawing.Size(43, 13);
            this.labelCharsNoSpacesValue.TabIndex = 12;
            this.labelCharsNoSpacesValue.Text = "1180";
            this.labelCharsNoSpacesValue.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            //
            // labelCharsValue
            //
            this.labelCharsValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCharsValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCharsValue.Location = new System.Drawing.Point(157, 41);
            this.labelCharsValue.Name = "labelCharsValue";
            this.labelCharsValue.Size = new System.Drawing.Size(43, 13);
            this.labelCharsValue.TabIndex = 11;
            this.labelCharsValue.Text = "1012";
            this.labelCharsValue.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            //
            // labelWordCountValue
            //
            this.labelWordCountValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWordCountValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelWordCountValue.Location = new System.Drawing.Point(157, 22);
            this.labelWordCountValue.Name = "labelWordCountValue";
            this.labelWordCountValue.Size = new System.Drawing.Size(43, 13);
            this.labelWordCountValue.TabIndex = 10;
            this.labelWordCountValue.Text = "181";
            this.labelWordCountValue.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            //
            // labelCharsNoSpaces
            //
            this.labelCharsNoSpaces.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCharsNoSpaces.Location = new System.Drawing.Point(6, 61);
            this.labelCharsNoSpaces.Name = "labelCharsNoSpaces";
            this.labelCharsNoSpaces.Size = new System.Drawing.Size(127, 13);
            this.labelCharsNoSpaces.TabIndex = 9;
            this.labelCharsNoSpaces.Text = "Characters (with spaces)";
            this.labelCharsNoSpaces.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // labelChars
            //
            this.labelChars.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelChars.Location = new System.Drawing.Point(6, 41);
            this.labelChars.Name = "labelChars";
            this.labelChars.Size = new System.Drawing.Size(119, 13);
            this.labelChars.TabIndex = 8;
            this.labelChars.Text = "Characters (no spaces)";
            this.labelChars.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // labelWordCount
            //
            this.labelWordCount.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelWordCount.Location = new System.Drawing.Point(6, 22);
            this.labelWordCount.Name = "labelWordCount";
            this.labelWordCount.Size = new System.Drawing.Size(38, 13);
            this.labelWordCount.TabIndex = 7;
            this.labelWordCount.Text = "Words";
            this.labelWordCount.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // WordCountForm
            //
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(230, 168);
            this.Controls.Add(this.gbTableHeader);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WordCountForm";
            this.Text = "Word Count";
            this.gbTableHeader.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.GroupBox gbTableHeader;
        private System.Windows.Forms.Label labelCharsNoSpacesValue;
        private System.Windows.Forms.Label labelCharsValue;
        private System.Windows.Forms.Label labelWordCountValue;
        private System.Windows.Forms.Label labelCharsNoSpaces;
        private System.Windows.Forms.Label labelChars;
        private System.Windows.Forms.Label labelWordCount;
        private System.Windows.Forms.Label labelParagraphsValue;
        private System.Windows.Forms.Label labelParagraphs;

    }
}