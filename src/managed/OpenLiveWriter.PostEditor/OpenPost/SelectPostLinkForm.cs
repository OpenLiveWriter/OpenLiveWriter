// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.OpenPost
{
    public class SelectPostLinkForm : OpenPostForm
    {
        private System.ComponentModel.IContainer components = null;

        public SelectPostLinkForm()
            : base(OpenMode.RecentPosts)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
            this.Text = Res.Get(StringId.LinkToPost);

            IncludeDrafts = false;
            AllowDelete = false;

            // Subscribe to validation event
            ValidatePost += new ValidatePostEventHandler(SelectPostLinkForm_ValidatePost);
        }

        public string PostLink
        {
            get { return _postLink; }
        }
        private string _postLink = String.Empty;

        public string PostTitle
        {
            get { return _postTitle; }
        }
        private string _postTitle = String.Empty;

        private void SelectPostLinkForm_ValidatePost(object sender, ValidatePostEventArgs ea)
        {
            PostInfo postInfo = ea.PostInfo;
            if (postInfo.Permalink == String.Empty)
            {
                // see if we can do a direct fetch from the weblog
                using (new WaitCursor())
                {
                    BlogPost blogPost = PostHelper.SafeRetrievePost(postInfo, 10000);
                    if (blogPost != null)
                    {
                        _postLink = blogPost.Permalink;
                        _postTitle = blogPost.Title;
                    }
                }

                // if we still don't have it then display an error message
                if (_postLink == String.Empty)
                {
                    // saving the company a few dollars by not putting this one
                    // in DisplayMessages.xml, since it's already been translated
                    // in Strings.resx.
                    DisplayMessage message = new DisplayMessage(MessageId.None);
                    message.Title = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.MissingPostLinkCaption),
                                                  postInfo.IsPage ? Res.Get(StringId.Page) : Res.Get(StringId.Post));
                    message.Text = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.MissingPostLinkExplanation),
                                                 postInfo.IsPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower),
                                                 ApplicationEnvironment.ProductNameQualified);
                    message.Type = MessageBoxIcon.Exclamation;
                    message.Buttons = MessageBoxButtons.OK;
                    message.Show(ea.OpenPostDialog);

                    // fail validation
                    ea.PostIsValid = false;
                }
            }
            else
            {
                _postLink = postInfo.Permalink;
                _postTitle = postInfo.Title;
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // SelectPostLinkForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(619, 458);
            this.Name = "SelectPostLinkForm";
            this.Text = "Link to Post";

        }
        #endregion

    }
}

