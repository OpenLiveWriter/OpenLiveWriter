// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tagging
{

    /// <summary>
    /// Summary description for TagProviderComboBox.
    /// </summary>
    public class TagProviderComboBox : ComboBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public TagProviderComboBox()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public void SetTagProviders(TagProvider[] providers)
        {
            Items.Clear();
            Items.AddRange(providers);
            Items.Add(new TagOptionsProvider());
        }

        public TagProvider SelectedTagProvider
        {
            get
            {
                return SelectedItem as TagProvider;
            }
            set
            {
                SelectedItem = value;
            }
        }

        public event EventHandler ManageProviders;

        protected void OnManageProviders()
        {
            if (ManageProviders != null)
                ManageProviders(this, EventArgs.Empty);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            _selectedIndex = SelectedIndex;
            base.OnSelectedIndexChanged(e);
        }
        private int _selectedIndex = -1;

        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            if (SelectedItem is TagOptionsProvider)
            {
                OnManageProviders();
                if (_selectedIndex > -1 && _selectedIndex < Items.Count)
                    SelectedIndex = _selectedIndex;
            }
            base.OnSelectionChangeCommitted(e);
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
            components = new System.ComponentModel.Container();
        }
        #endregion

        private class TagOptionsProvider
        {
            public override string ToString()
            {
                return Res.Get(StringId.TagsCustomizeProviders);
            }

        }
    }

}
