// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    public partial class ComboBoxRel : ComboBox
    {
        public ComboBoxRel()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                Items.Clear();
                Items.AddRange(new object[] { "", "tag", "enclosure", "license", "nofollow" });
            }
        }

        public string Rel
        {
            get
            {
                return Text;
            }
            set
            {
                int index = FindString(value);
                if (index < 0)
                    Text = value;
                else
                    SelectedIndex = index;
            }
        }

    }
}
