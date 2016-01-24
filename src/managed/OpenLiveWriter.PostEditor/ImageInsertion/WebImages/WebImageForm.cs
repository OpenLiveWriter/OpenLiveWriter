// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.ImageInsertion.WebImages
{
    public partial class WebImageForm : BaseForm
    {
        readonly WebImageSource _source = new WebImageSource();
        public WebImageForm()
        {
            InitializeComponent();

            _source.Init(panelLayout.Width, panelLayout.Height);
            UserControl uc = _source.ImageSelectionControls;
            panelLayout.Controls.Add(uc);
            buttonInsert.Text = Res.Get(StringId.InsertImageButton);
            buttonCancel.Text = Res.Get(StringId.CancelButton);
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Text = Res.Get(StringId.Plugin_WebImage_Title);
            Icon = ApplicationEnvironment.ProductIcon;

            LayoutHelper.FixupOKCancel(buttonInsert, buttonCancel);
            BidiHelper.RtlLayoutFixup(this);
        }

        public string ImageUrl
        {
            get
            {
                return _source.SourceImageLink;
            }
        }

        private void buttonInsert_Click(object sender, EventArgs e)
        {
            if (!UrlHelper.IsUrl(_source.SourceImageLink))
            {
                //Ask if they want to go back and correct the link or just abort it altogether
                if (DisplayMessage.Show(MessageId.InputIsNotUrl, this, _source.SourceImageLink) == System.Windows.Forms.DialogResult.Yes)
                {
                    _source.TabSelected();
                    return;
                }
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

    }
}
