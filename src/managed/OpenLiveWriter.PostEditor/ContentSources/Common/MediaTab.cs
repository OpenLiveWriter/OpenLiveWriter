using System;
using System.Collections.Generic;

using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.PostEditor.ImageInsertion;

namespace OpenLiveWriter.PostEditor.ContentSources.Common
{
    public partial class MediaTab : InsertImageTabControl
    {
        public MediaTab()
        {
            InitializeComponent();
        }

        public virtual bool ValidateSelection()
        {
            return false;
        }

        public virtual void Repaint(Size newSize)
        {
            Size = newSize;
        }

        public virtual void SaveContent(MediaSmartContent content)
        {

        }

        public virtual void TabSelected()
        {

        }

        protected string _blogId;
        public virtual void Init(MediaSmartContent content, string blogId)
        {
            _blogId = blogId;
        }

        public event EventHandler MediaSelected;

        public string BlogId
        {
            // Copyright (c) .NET Foundation. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for details.

            get { return _blogId; }
            set { _blogId = value; }
        }

        protected void OnMediaSelected()
        {
            if (MediaSelected != null)
                MediaSelected(this, EventArgs.Empty);
        }

        public virtual List<Control> GetAccessibleControls()
        {
            return new List<Control>();
        }
    }
}
