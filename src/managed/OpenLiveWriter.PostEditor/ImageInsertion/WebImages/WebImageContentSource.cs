// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Web;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.ImageInsertion.WebImages
{
    [WriterPlugin(WebImageContentSource.ID, "Web Image",
    ImagePath = "Images.TabInsertFromWeb.png",
     PublisherUrl = "http://local.live.com",
     Description = "Add images from the Internet to your post.")]

    [InsertableContentSource("Web Image", SidebarText = "Web Image")]

    [CustomLocalizedPlugin("WebImage")]
    public class WebImageContentSource : ContentSource
    {
        public const string ID = "78047914-B039-4B15-B2FA-90D4DBDF7C51";

        public override System.Windows.Forms.DialogResult CreateContent(System.Windows.Forms.IWin32Window dialogOwner, ref string content)
        {
            using (WebImageForm form = new WebImageForm())
            {
                if (form.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty(form.ImageUrl))
                {
                    content = String.Format(CultureInfo.InvariantCulture, @"<img wlApplyDefaultMargins=""true"" src=""{0}"" />", HttpUtility.HtmlEncode(form.ImageUrl));
                }

                return form.DialogResult;
            }

        }

    }
}
