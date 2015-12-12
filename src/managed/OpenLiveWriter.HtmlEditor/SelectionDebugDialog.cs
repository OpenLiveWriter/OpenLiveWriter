// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    public partial class SelectionDebugDialog : Form
    {
        public SelectionDebugDialog()
        {
            InitializeComponent();
        }

        internal void Add(OpenLiveWriter.Mshtml.MarkupRange SelectedMarkupRange)
        {
            listBoxSelection.Items.Insert(0, new SelectionItem(SelectedMarkupRange));
        }

        public class SelectionItem
        {
            public readonly MarkupRange Range;
            public SelectionItem(MarkupRange range)
            {
                Range = range;
            }

            public override string ToString()
            {
                return Range.Start.PositionDetail + " ---> " + Range.End.PositionDetail;
            }
        }

        private void listBoxSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectionItem item = listBoxSelection.SelectedItem as SelectionItem;
            if (item != null)
            {
                MessageBox.Show(item.Range.Start.GetDocument().body.outerHTML);
            }
        }

    }
}
