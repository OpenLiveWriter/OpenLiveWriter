// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Video.VideoService
{
    public partial class VideoRequestTypeComboBox : ImageComboBox
    {
        public VideoRequestTypeComboBox() : base(new Size(20, 18))
        {
            InitializeComponent();
            DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            Name = "comboBoxVideoRequestType";
        }

        public void SetEntries(IVideoRequestType[] requestTypes)
        {
            Items.Clear();
            foreach (IVideoRequestType requestType in requestTypes)
                Items.Add(requestType);
        }

        public void SelectEntry(string typeName)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                IVideoRequestType requestType = (IVideoRequestType)Items[i];
                if (requestType.TypeName == typeName)
                {
                    SelectedIndex = i;
                    return;
                }
            }
        }

        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            base.OnSelectionChangeCommitted(e);
            OnSelectedRequestTypeChanged();
        }

        public IVideoRequestType SelectedRequestType
        {
            get
            {
                return (IVideoRequestType)SelectedItem;
            }
            set
            {
                int i = 0;
                foreach (IVideoRequestType requestType in Items)
                {
                    if (requestType == value)
                    {
                        SelectedIndex = i;
                        break;
                    }
                    i++;
                }
            }
        }

        public event EventHandler SelectedRequestTypeChanged;

        protected virtual void OnSelectedRequestTypeChanged()
        {
            if (SelectedRequestTypeChanged != null)
                SelectedRequestTypeChanged(this, EventArgs.Empty);
        }

    }

}
