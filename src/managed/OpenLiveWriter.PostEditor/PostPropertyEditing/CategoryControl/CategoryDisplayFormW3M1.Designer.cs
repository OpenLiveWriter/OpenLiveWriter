using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    partial class CategoryDisplayFormW3M1
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
            this.components = new System.ComponentModel.Container();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.selectorContainer = new System.Windows.Forms.Panel();
            this.lblNone = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.grpAdd = new System.Windows.Forms.GroupBox();
            this.btnDoAdd = new System.Windows.Forms.Button();
            this.cbParent = new OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl.CategoryDisplayFormW3M1.ParentCategoryComboBox();
            this.txtNewCategory = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.toolTip = new ToolTip2(this.components);
            this.grpAdd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            //
            // txtFilter
            //
            this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtFilter.Location = new System.Drawing.Point(15, 17);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(157, 14);
            this.txtFilter.TabIndex = 0;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            //
            // selectorContainer
            //
            this.selectorContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.selectorContainer.Location = new System.Drawing.Point(12, 42);
            this.selectorContainer.Name = "selectorContainer";
            this.selectorContainer.Size = new System.Drawing.Size(209, 250);
            this.selectorContainer.TabIndex = 2;
            //
            // lblNone
            //
            this.lblNone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNone.BackColor = System.Drawing.SystemColors.Window;
            this.lblNone.Location = new System.Drawing.Point(37, 105);
            this.lblNone.Name = "lblNone";
            this.lblNone.Size = new System.Drawing.Size(159, 67);
            this.lblNone.TabIndex = 4;
            this.lblNone.Text = "(No categories)";
            this.lblNone.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblNone.Visible = false;
            //
            // btnRefresh
            //
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(202, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(22, 24);
            this.btnRefresh.TabIndex = 1;
            this.toolTip.SetToolTip(this.btnRefresh, "Refresh List");
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            // grpAdd
            //
            this.grpAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAdd.Controls.Add(this.btnDoAdd);
            this.grpAdd.Controls.Add(this.cbParent);
            this.grpAdd.Controls.Add(this.txtNewCategory);
            this.grpAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpAdd.Location = new System.Drawing.Point(10, 302);
            this.grpAdd.Name = "grpAdd";
            this.grpAdd.Size = new System.Drawing.Size(213, 77);
            this.grpAdd.TabIndex = 3;
            this.grpAdd.TabStop = false;
            this.grpAdd.Text = "Add Category";
            //
            // btnDoAdd
            //
            this.btnDoAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDoAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnDoAdd.Location = new System.Drawing.Point(170, 46);
            this.btnDoAdd.Name = "btnDoAdd";
            this.btnDoAdd.Size = new System.Drawing.Size(36, 23);
            this.btnDoAdd.TabIndex = 2;
            this.btnDoAdd.Text = "&Add";
            this.btnDoAdd.UseVisualStyleBackColor = true;
            this.btnDoAdd.Click += new System.EventHandler(this.btnDoAdd_Click);
            //
            // cbParent
            //
            this.cbParent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbParent.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbParent.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbParent.FormattingEnabled = true;
            this.cbParent.IntegralHeight = false;
            this.cbParent.Location = new System.Drawing.Point(8, 47);
            this.cbParent.Name = "cbParent";
            this.cbParent.Size = new System.Drawing.Size(156, 22);
            this.cbParent.TabIndex = 1;
            //
            // txtNewCategory
            //
            this.txtNewCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNewCategory.Location = new System.Drawing.Point(8, 20);
            this.txtNewCategory.Name = "txtNewCategory";
            this.txtNewCategory.Size = new System.Drawing.Size(197, 21);
            this.txtNewCategory.TabIndex = 0;
            //
            // pictureBox1
            //
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBox1.Location = new System.Drawing.Point(179, 16);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.RightToLeft = RightToLeft.No;
            this.pictureBox1.Size = new System.Drawing.Size(15, 15);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            //
            // CategoryDisplayFormW3M1
            //
            this.ClientSize = new System.Drawing.Size(233, 391);
            this.Controls.Add(this.lblNone);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.grpAdd);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.selectorContainer);
            this.Controls.Add(this.txtFilter);
            this.MaximumSize = new System.Drawing.Size(438, 566);
            this.MinimumSize = new System.Drawing.Size(233, 300);
            this.Name = "CategoryDisplayFormW3M1";
            this.Text = "CategoryDisplayFormW3M1";
            this.grpAdd.ResumeLayout(false);
            this.grpAdd.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Panel selectorContainer;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.GroupBox grpAdd;
        private System.Windows.Forms.Button btnDoAdd;
        private ParentCategoryComboBox cbParent;
        private System.Windows.Forms.TextBox txtNewCategory;
        private System.Windows.Forms.PictureBox pictureBox1;
        private ToolTip2 toolTip;
        private Label lblNone;
    }
}