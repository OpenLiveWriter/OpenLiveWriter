// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    /// <summary>
    /// Summary description for SelectGlossaryLinkDialog.
    /// </summary>
    public class SelectGlossaryLinkForm : ApplicationDialog
    {
        private GlossaryListView listViewGlossary;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnCancel;
        private System.ComponentModel.IContainer components = new Container();

        public SelectGlossaryLinkForm()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.btnSelect.Text = Res.Get(StringId.OKButtonText);
            this.btnCancel.Text = Res.Get(StringId.CancelButton);
            this.Text = Res.Get(StringId.GlossarySelectLink);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            LayoutHelper.FixupOKCancel(btnSelect, btnCancel);
        }

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
            this.components = new System.ComponentModel.Container();
            this.listViewGlossary = new OpenLiveWriter.HtmlEditor.Linking.GlossaryListView(this.components, 347 - 20);
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // listViewGlossary
            //
            this.listViewGlossary.AutoArrange = false;
            this.listViewGlossary.FullRowSelect = true;
            this.listViewGlossary.HideSelection = false;
            this.listViewGlossary.LabelWrap = false;
            this.listViewGlossary.Location = new System.Drawing.Point(4, 5);
            this.listViewGlossary.MultiSelect = false;
            this.listViewGlossary.Name = "listViewGlossary";
            this.listViewGlossary.Size = new System.Drawing.Size(347, 282);
            this.listViewGlossary.TabIndex = 0;
            this.listViewGlossary.View = System.Windows.Forms.View.Details;
            this.listViewGlossary.DoubleClick += new System.EventHandler(this.listViewGlossary_DoubleClick);
            //
            // btnSelect
            //
            this.btnSelect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSelect.Location = new System.Drawing.Point(191, 295);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.TabIndex = 1;
            this.btnSelect.Text = "OK";
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(274, 295);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            //
            // SelectGlossaryLinkForm
            //
            this.AcceptButton = this.btnSelect;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(354, 325);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.listViewGlossary);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectGlossaryLinkForm";
            this.Text = "Select Link from Glossary";
            this.ResumeLayout(false);

        }
        #endregion

        private void btnSelect_Click(object sender, System.EventArgs e)
        {
            if (SelectedEntry == null)
            {
                DisplayMessage.Show(MessageId.SelectEntry);
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void listViewGlossary_DoubleClick(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        public GlossaryLinkItem SelectedEntry
        {
            get
            {
                if (listViewGlossary.SelectedItems.Count == 0)
                    return null;
                return (GlossaryLinkItem)listViewGlossary.SelectedItems[0].Tag;
            }
        }

        public void SetSelected(string initialText)
        {
            ListViewItem initialItem = listViewGlossary.Find(initialText, null);
            if (initialItem != null)
            {
                initialItem.Selected = true;
            }
        }

    }
}
