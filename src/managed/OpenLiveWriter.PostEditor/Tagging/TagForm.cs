// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tagging
{

    public class TagForm : BaseForm
    {
        public TagForm(TagContext context) : base()
        {
            InitializeComponent();

            this.buttonInsert.Text = Res.Get(StringId.InsertButton);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.Text = Res.Get(StringId.TagsInsertTags);

            tagEditor.Tags = context.Tags;
            tagEditor.SetTagProviders(context);
            tagEditor.TagProvider = context.CurrentProvider;
            tagEditor.PreviouslyUsedTags = context.PreviouslyUsedTags;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                tagEditor.NaturalizeLayout();
                LayoutHelper.FixupOKCancel(buttonInsert, buttonCancel);
            }
        }

        public string[] Tags
        {
            get
            {
                return tagEditor.Tags;
            }
        }

        public TagProvider TagProvider
        {
            get
            {
                return tagEditor.TagProvider;
            }
        }

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
            this.buttonInsert = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tagEditor = new OpenLiveWriter.PostEditor.Tagging.TagEditor();
            this.SuspendLayout();
            //
            // buttonInsert
            //
            this.buttonInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInsert.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonInsert.Location = new System.Drawing.Point(94, 179);
            this.buttonInsert.Name = "buttonInsert";
            this.buttonInsert.Size = new System.Drawing.Size(90, 27);
            this.buttonInsert.TabIndex = 2;
            this.buttonInsert.Text = "&Insert";
            this.buttonInsert.Click += new System.EventHandler(this.buttonInsert_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(190, 179);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 27);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // tagEditor
            //
            this.tagEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tagEditor.Location = new System.Drawing.Point(12, 9);
            this.tagEditor.Name = "tagEditor";
            this.tagEditor.Size = new System.Drawing.Size(264, 152);
            this.tagEditor.TabIndex = 0;
            this.tagEditor.TagProvider = null;
            this.tagEditor.Tags = new string[0];
            //
            // TagForm
            //
            this.AcceptButton = this.buttonInsert;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(288, 216);
            this.Controls.Add(this.tagEditor);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonInsert);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TagForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Insert Tags";
            this.ResumeLayout(false);

        }
        #endregion

        private Button buttonInsert;
        private Button buttonCancel;
        private Container components = null;
        private TagEditor tagEditor;

        private void buttonInsert_Click(object sender, EventArgs e)
        {
            if (tagEditor.Tags.Length == 0)
            {
                DisplayMessage.Show(MessageId.NoTags, this);
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
