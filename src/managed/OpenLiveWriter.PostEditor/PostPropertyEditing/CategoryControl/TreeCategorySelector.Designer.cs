namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    partial class TreeCategorySelector
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
            this.components = new System.ComponentModel.Container();
            this.treeView = new DoubleClicklessTreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            //
            // treeView
            //
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView.CheckBoxes = true;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.ItemHeight = 18;
            this.treeView.Location = new System.Drawing.Point(3, 0);
            this.treeView.Name = "treeView";
            this.treeView.RightToLeftLayout = true;
            this.treeView.ShowLines = false;
            this.treeView.ShowPlusMinus = false;
            this.treeView.ShowRootLines = false;
            this.treeView.Size = new System.Drawing.Size(0, 0);
            this.treeView.TabIndex = 0;
            //
            // imageList
            //
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(1, 14);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            //
            // TreeCategorySelector
            //
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.treeView);
            this.Name = "TreeCategorySelector";
            this.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.Size = new System.Drawing.Size(3, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private DoubleClicklessTreeView treeView;
        private System.Windows.Forms.ImageList imageList;
    }
}
