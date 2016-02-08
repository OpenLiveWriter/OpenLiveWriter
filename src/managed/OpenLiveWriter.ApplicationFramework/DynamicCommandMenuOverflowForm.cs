// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Summary description for DynamicCommandMenuOverflowForm.
    /// </summary>
    public class DynamicCommandMenuOverflowForm : ApplicationDialog
    {
        private Button buttonOK;
        private Button buttonCancel;
        private ListBox listBoxCommands;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public DynamicCommandMenuOverflowForm(IMenuCommandObject[] menuCommandObjects)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);

            // initialize the listbox
            foreach (IMenuCommandObject menuCommandObject in menuCommandObjects)
                listBoxCommands.Items.Add(new MenuCommandObjectListBoxAdapter(menuCommandObject));

            // select the top item in the list box
            if (listBoxCommands.Items.Count > 0)
                listBoxCommands.SelectedIndex = 0;
        }

        public IMenuCommandObject SelectedObject
        {
            get
            {
                if (listBoxCommands.SelectedIndex != -1)
                    return (listBoxCommands.SelectedItem as MenuCommandObjectListBoxAdapter).MenuCommandObject;
                else
                    return null;
            }
        }

        private void listBoxCommands_DoubleClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private class MenuCommandObjectListBoxAdapter
        {
            public MenuCommandObjectListBoxAdapter(IMenuCommandObject menuCommandObject)
            {
                _menuCommandObject = menuCommandObject;
            }

            public override string ToString()
            {
                return _menuCommandObject.CaptionNoMnemonic;
            }

            public IMenuCommandObject MenuCommandObject { get { return _menuCommandObject; } }
            private IMenuCommandObject _menuCommandObject;

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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.listBoxCommands = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(88, 228);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            //
            // buttonCancel
            //
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(171, 228);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            //
            // listBoxCommands
            //
            this.listBoxCommands.Location = new System.Drawing.Point(8, 8);
            this.listBoxCommands.Name = "listBoxCommands";
            this.listBoxCommands.Size = new System.Drawing.Size(238, 212);
            this.listBoxCommands.TabIndex = 2;
            this.listBoxCommands.DoubleClick += new System.EventHandler(this.listBoxCommands_DoubleClick);
            //
            // DynamicCommandMenuOverflowForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(256, 260);
            this.Controls.Add(this.listBoxCommands);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DynamicCommandMenuOverflowForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DynamicCommandMenuOverflowForm";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
