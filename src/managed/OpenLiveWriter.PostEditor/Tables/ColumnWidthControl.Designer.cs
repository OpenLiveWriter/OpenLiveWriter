// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{

    public partial class ColumnWidthControl
    {
        private OpenLiveWriter.Controls.NumericTextBox textBoxWidth;
        private System.Windows.Forms.Label labelWidth;
        private RadioButton rbPixels;
        private RadioButton rbPercent;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelWidth = new System.Windows.Forms.Label();
            this.rbPixels = new System.Windows.Forms.RadioButton();
            this.rbPercent = new System.Windows.Forms.RadioButton();
            this.textBoxWidth = new OpenLiveWriter.Controls.NumericTextBox();
            this.SuspendLayout();
            // 
            // labelWidth
            // 
            this.labelWidth.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelWidth.Location = new System.Drawing.Point(0, 3);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(64, 15);
            this.labelWidth.TabIndex = 0;
            this.labelWidth.Text = "&Width:";
            // 
            // rbPixels
            // 
            this.rbPixels.AutoSize = true;
            this.rbPixels.Checked = true;
            this.rbPixels.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rbPixels.Location = new System.Drawing.Point(124, 3);
            this.rbPixels.Name = "rbPixels";
            this.rbPixels.Size = new System.Drawing.Size(67, 22);
            this.rbPixels.TabIndex = 4;
            this.rbPixels.TabStop = true;
            this.rbPixels.Text = "pixels";
            this.rbPixels.UseVisualStyleBackColor = true;
            // 
            // rbPercent
            // 
            this.rbPercent.AutoSize = true;
            this.rbPercent.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rbPercent.Location = new System.Drawing.Point(194, 3);
            this.rbPercent.Name = "rbPercent";
            this.rbPercent.Size = new System.Drawing.Size(80, 22);
            this.rbPercent.TabIndex = 5;
            this.rbPercent.Text = "percent";
            this.rbPercent.UseVisualStyleBackColor = true;
            // 
            // textBoxWidth
            // 
            this.textBoxWidth.Location = new System.Drawing.Point(72, 0);
            this.textBoxWidth.MaxLength = 9;
            this.textBoxWidth.Name = "textBoxWidth";
            this.textBoxWidth.Size = new System.Drawing.Size(46, 22);
            this.textBoxWidth.TabIndex = 2;
            // 
            // ColumnWidthControl
            // 
            this.Controls.Add(this.rbPercent);
            this.Controls.Add(this.rbPixels);
            this.Controls.Add(this.textBoxWidth);
            this.Controls.Add(this.labelWidth);
            this.Name = "ColumnWidthControl";
            this.Size = new System.Drawing.Size(284, 23);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }


}
