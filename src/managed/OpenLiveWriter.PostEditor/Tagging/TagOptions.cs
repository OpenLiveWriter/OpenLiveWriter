// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tagging
{
    /// <summary>
    /// Summary description for TagOptions.
    /// </summary>
    public class TagOptions : BaseForm
    {
        private ListBox listBoxOptions;
        private Label labelTagProviders;
        private Button buttonClose;
        private Button buttonAdd;
        private Button buttonEdit;
        private Button buttonDelete;
        private Button buttonRestoreDefaults;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public TagOptions()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonClose.Text = Res.Get(StringId.CloseButton);
            this.buttonAdd.Text = Res.Get(StringId.AddButton);
            this.buttonEdit.Text = Res.Get(StringId.EditButton);
            this.buttonDelete.Text = Res.Get(StringId.DeleteButton);
            this.labelTagProviders.Text = Res.Get(StringId.TagsTagProvidersLabel);
            this.buttonRestoreDefaults.Text = Res.Get(StringId.TagsRestoreDefaults);
            this.Text = Res.Get(StringId.TagsTagOptions);
            listBoxOptions.KeyUp += TagOptions_KeyUp;
            KeyUp += TagOptions_KeyUp;
        }

        void TagOptions_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteProvider();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Right, false))
            {
                DisplayHelper.AutoFitSystemButton(buttonRestoreDefaults);
                if (buttonRestoreDefaults.Right > listBoxOptions.Right)
                {
                    int oldWidth = listBoxOptions.Width;
                    listBoxOptions.Width = buttonRestoreDefaults.Width;
                    buttonClose.Left =
                        buttonEdit.Left = buttonDelete.Left = buttonAdd.Left = buttonAdd.Left + listBoxOptions.Width - oldWidth;
                }

                LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Left, buttonClose.Width, int.MaxValue,
                    buttonAdd, buttonEdit, buttonDelete, buttonClose);

            }
        }

        public void Initialize(TagProviderManager manager)
        {
            _manager = manager;
            RefreshProviders(true);
        }
        private TagProviderManager _manager;

        public void SetContext(TagContext context)
        {
            _context = context;
        }
        private TagContext _context;

        private void RefreshProviders(bool defaultSelect)
        {
            if (listBoxOptions != null)
            {
                listBoxOptions.Items.Clear();
                listBoxOptions.Items.AddRange(_manager.TagProviders);
                listBoxOptions.Sorted = true;
                if (defaultSelect)
                    listBoxOptions.SelectedIndex = 0;

                buttonDelete.Enabled = listBoxOptions.Items.Count > 1;
            }
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxOptions = new System.Windows.Forms.ListBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.labelTagProviders = new System.Windows.Forms.Label();
            this.buttonRestoreDefaults = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // listBoxOptions
            //
            this.listBoxOptions.Location = new System.Drawing.Point(8, 24);
            this.listBoxOptions.Name = "listBoxOptions";
            this.listBoxOptions.Size = new System.Drawing.Size(200, 134);
            this.listBoxOptions.TabIndex = 4;
            //
            // buttonClose
            //
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonClose.Location = new System.Drawing.Point(216, 176);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.TabIndex = 6;
            this.buttonClose.Text = "&Close";
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            //
            // buttonAdd
            //
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(216, 24);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.TabIndex = 0;
            this.buttonAdd.Text = "A&dd...";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            //
            // buttonEdit
            //
            this.buttonEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonEdit.Location = new System.Drawing.Point(216, 56);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.TabIndex = 1;
            this.buttonEdit.Text = "&Edit...";
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            //
            // buttonDelete
            //
            this.buttonDelete.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonDelete.Location = new System.Drawing.Point(216, 136);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.TabIndex = 2;
            this.buttonDelete.Text = "&Delete";
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            //
            // labelTagProviders
            //
            this.labelTagProviders.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTagProviders.Location = new System.Drawing.Point(8, 8);
            this.labelTagProviders.Name = "labelTagProviders";
            this.labelTagProviders.Size = new System.Drawing.Size(224, 16);
            this.labelTagProviders.TabIndex = 3;
            this.labelTagProviders.Text = "&Tag providers:";
            //
            // buttonRestoreDefaults
            //
            this.buttonRestoreDefaults.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonRestoreDefaults.Location = new System.Drawing.Point(8, 176);
            this.buttonRestoreDefaults.Name = "buttonRestoreDefaults";
            this.buttonRestoreDefaults.Size = new System.Drawing.Size(128, 23);
            this.buttonRestoreDefaults.TabIndex = 5;
            this.buttonRestoreDefaults.Text = "&Restore Defaults";
            this.buttonRestoreDefaults.Click += new System.EventHandler(this.buttonRestoreDefaults_Click);
            //
            // TagOptions
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(299, 214);
            this.Controls.Add(this.buttonRestoreDefaults);
            this.Controls.Add(this.labelTagProviders);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonEdit);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.listBoxOptions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TagOptions";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tag Options";
            this.ResumeLayout(false);

        }
        #endregion

        private void buttonClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            EditTagForm tagForm = new EditTagForm();
            tagForm.Provider = new TagProvider();
            DialogResult result = tagForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                TagProvider provider = tagForm.Provider;
                _manager.Save(provider);
                RefreshProviders(false);
                listBoxOptions.SelectedItem = provider;

                if (_context != null)
                    _context.CurrentProvider = provider;
            }

        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (!IsValid())
                return;

            TagProvider provider = (TagProvider)listBoxOptions.SelectedItem;
            EditTagForm tagForm = new EditTagForm();
            tagForm.Provider = provider;
            DialogResult result = tagForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                _manager.Save(tagForm.Provider);
                RefreshProviders(false);
                listBoxOptions.SelectedItem = tagForm.Provider;
            }
        }

        private void DeleteProvider()
        {
            if (!IsValid())
                return;

            TagProvider provider = (TagProvider)listBoxOptions.SelectedItem;
            DialogResult result = DisplayMessage.Show(MessageId.TagConfirmDeleteProvider, provider.Name);
            if (result == DialogResult.Yes)
            {
                _manager.Delete(provider);
                RefreshProviders(true);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            DeleteProvider();
        }

        private bool IsValid()
        {
            if (listBoxOptions.SelectedIndex == -1)
            {
                DisplayMessage.Show(MessageId.TagSelectProvider);
                return false;
            }

            return true;
        }

        private void buttonRestoreDefaults_Click(object sender, EventArgs e)
        {

            DialogResult result = DisplayMessage.Show(MessageId.TagConfirmRestoreProviders);
            if (result == DialogResult.Yes)
                _manager.RestoreDefaults();
            RefreshProviders(true);
        }

    }
}
