// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Configuration.Settings;
using OpenLiveWriter.PostEditor.Configuration.Accounts;
using OpenLiveWriter.PostEditor.Configuration.Wizard;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.Configuration.Accounts
{
    /// <summary>
    /// Summary description for WeblogAccountManagementControl.
    /// </summary>
    public class WeblogAccountManagementControl : UserControl
    {
        private WeblogAccountListView listViewWeblogs;
        private Button buttonAdd;
        private Button buttonEdit;
        private Button buttonDelete;
        private Button buttonView;
        private System.ComponentModel.IContainer components = new Container();

        public WeblogAccountManagementControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.buttonAdd.Text = Res.Get(StringId.AddButton);
            this.buttonEdit.Text = Res.Get(StringId.EditButton);
            this.buttonDelete.Text = Res.Get(StringId.DeleteButton);
            this.buttonView.Text = Res.Get(StringId.ViewButton);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int gap = buttonAdd.Left - listViewWeblogs.Right;
            LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Right, buttonDelete.Width, int.MaxValue,
                buttonAdd, buttonEdit, buttonDelete, buttonView);
            listViewWeblogs.Width = buttonAdd.Left - gap - listViewWeblogs.Left;
        }

        public IBlogPostEditingSite EditingSite
        {
            get
            {
                return _editingSite;
            }
            set
            {
                _editingSite = value;
            }
        }
        private IBlogPostEditingSite _editingSite;

        private void listViewWeblogs_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonEnabledStates();
        }

        private void UpdateButtonEnabledStates()
        {
            // manage availability of buttons
            bool weblogSelected = listViewWeblogs.SelectedItems.Count > 0;
            buttonEdit.Enabled = weblogSelected;
            buttonView.Enabled = weblogSelected;
            buttonDelete.Enabled = weblogSelected && DeleteCommandEnabled;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                using (new WaitCursor())
                {
                    string newBlogId = WeblogConfigurationWizardController.Add(FindForm(), false);
                    if (newBlogId != null)
                    {
                        // add the weblog
                        using (BlogSettings blogSettings = BlogSettings.ForBlogId(newBlogId))
                        {
                            ListViewItem item = listViewWeblogs.AddWeblogItem(blogSettings);

                            // select the item that was added
                            listViewWeblogs.SelectedItems.Clear();
                            item.Selected = true;

                            // set focus to the list
                            listViewWeblogs.Focus();
                        }
                    }
                    return;

                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            EditSelectedWeblog();
        }

        private void listViewWeblogs_DoubleClick(object sender, EventArgs e)
        {
            EditSelectedWeblog();
        }

        private void buttonView_Click(object sender, EventArgs e)
        {
            try
            {
                ShellHelper.LaunchUrl(SelectedWeblog.HomepageUrl);
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }
        }

        private bool DeleteCommandEnabled
        {
            // don't allow deleting of the last weblog in the list
            get { return listViewWeblogs.Items.Count > 1; }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedWeblog();
        }

        /// <summary>
        /// Process keyboard accelerators
        /// </summary>
        /// <param name="msg">message</param>
        /// <param name="keyData">key data</param>
        /// <returns>true if processed</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // delete key deletes items
            if (keyData == Keys.Delete && DeleteCommandEnabled)
            {
                DeleteSelectedWeblog();
                return true;
            }
            else // delegate to base
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void EditSelectedWeblog()
        {
            if (SelectedWeblog == null)
                return;

            try
            {
                using (new WaitCursor())
                {
                    WeblogSettingsManager.EditSettings(FindForm(), SelectedWeblog.Id, typeof(AccountPanel));

                    // refresh contents of list-view item
                    listViewWeblogs.EditWeblogItem(listViewWeblogs.SelectedItems[0], SelectedWeblog);

                    // set focus to the list
                    listViewWeblogs.Focus();

                    // if we have an editing site then notify it that we
                    // just edited weblog settings
                    if (EditingSite != null)
                        EditingSite.NotifyWeblogSettingsChanged(SelectedWeblog.Id, true);

                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }

        }

        private void DeleteSelectedWeblog()
        {
            if (SelectedWeblog == null)
                return;

            try
            {
                if (DisplayMessage.Show(MessageId.ConfirmRemoveWeblog) == DialogResult.Yes)
                {
                    // delete the weblog
                    listViewWeblogs.DeleteSelectedWeblog();

                    // set focus to the list
                    listViewWeblogs.Focus();

                    // update button enabled states
                    UpdateButtonEnabledStates();
                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }
        }

        private BlogSettings SelectedWeblog
        {
            get
            {
                return listViewWeblogs.SelectedWeblog;
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

        public IBlogSettingsEditor[] BlogSettingsEditors
        {
            get
            {
                return _blogSettingsEditors;
            }
            set
            {
                _blogSettingsEditors = value;
            }
        }
        private IBlogSettingsEditor[] _blogSettingsEditors;

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listViewWeblogs = new OpenLiveWriter.PostEditor.Configuration.Accounts.WeblogAccountListView(this.components);
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonView = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // listViewWeblogs
            //
            this.listViewWeblogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewWeblogs.AutoArrange = false;
            this.listViewWeblogs.FullRowSelect = true;
            this.listViewWeblogs.HideSelection = false;
            this.listViewWeblogs.LabelWrap = false;
            this.listViewWeblogs.Location = new System.Drawing.Point(0, 0);
            this.listViewWeblogs.MultiSelect = false;
            this.listViewWeblogs.Name = "listViewWeblogs";
            this.listViewWeblogs.Size = new System.Drawing.Size(254, 245);
            this.listViewWeblogs.TabIndex = 0;
            this.listViewWeblogs.View = System.Windows.Forms.View.Details;
            this.listViewWeblogs.DoubleClick += new System.EventHandler(this.listViewWeblogs_DoubleClick);
            this.listViewWeblogs.SelectedIndexChanged += new System.EventHandler(this.listViewWeblogs_SelectedIndexChanged);
            //
            // buttonAdd
            //
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(261, 0);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(84, 23);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "&Add...";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            //
            // buttonEdit
            //
            this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonEdit.Location = new System.Drawing.Point(261, 30);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(84, 23);
            this.buttonEdit.TabIndex = 2;
            this.buttonEdit.Text = "&Edit...";
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            //
            // buttonDelete
            //
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDelete.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonDelete.Location = new System.Drawing.Point(261, 90);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(84, 23);
            this.buttonDelete.TabIndex = 4;
            this.buttonDelete.Text = "&Delete";
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            //
            // buttonView
            //
            this.buttonView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonView.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonView.Location = new System.Drawing.Point(261, 60);
            this.buttonView.Name = "buttonView";
            this.buttonView.Size = new System.Drawing.Size(84, 23);
            this.buttonView.TabIndex = 3;
            this.buttonView.Text = "&View";
            this.buttonView.Click += new System.EventHandler(this.buttonView_Click);
            //
            // WeblogAccountManagementControl
            //
            this.Controls.Add(this.buttonView);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonEdit);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listViewWeblogs);
            this.Name = "WeblogAccountManagementControl";
            this.Size = new System.Drawing.Size(345, 245);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Hook that allows for processing just after a new blog has been created.
        /// </summary>

    }
}
