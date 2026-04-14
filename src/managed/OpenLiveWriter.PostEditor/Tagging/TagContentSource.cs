// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.PostEditor.Tagging
{
    [WriterPlugin(TagContentSource.ID, "Tags",
         ImagePath = "Images.InsertTag.png",
         PublisherUrl = "http://www.microsoft.com",
         Description = "Add tags to your posts using popular tagging sites or using custom HTML.",
         HasEditableOptions = true)]

    [InsertableContentSource("Tags")]

    [CustomLocalizedPlugin("Tags")]
    public class TagContentSource : SmartContentSource
    {
        public const string ID = "77ECF5F8-D252-44F5-B4EB-D463C5396A79";

        public override string GenerateEditorHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            _currentBlogId = publishingContext.AccountId;
            return FormTagLinks(content);
        }

        public override string GeneratePublishHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            _currentBlogId = publishingContext.AccountId;
            return FormTagLinks(content);
        }

        public override OpenLiveWriter.Api.DialogResult CreateContent(IBlogClientUIContext dialogOwner, ISmartContent newContent)
        {
            IBlogContext blogContext = dialogOwner as IBlogContext;
            if (blogContext != null)
                _currentBlogId = blogContext.CurrentAccountId;

            var ownerWindow = new Win32WindowImpl(dialogOwner.NativeWindowHandle);
            TagContext context = new TagContext(newContent, Options, _currentBlogId);
            using (TagForm form = new TagForm(context))
            {
                System.Windows.Forms.DialogResult result = form.ShowDialog(ownerWindow);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    context.Tags = form.Tags;
                    context.CurrentProvider = form.TagProvider;
                    context.AddTagsToHistory(form.Tags);
                }
                return result == System.Windows.Forms.DialogResult.OK ? OpenLiveWriter.Api.DialogResult.OK : OpenLiveWriter.Api.DialogResult.Cancel;
            }
        }

        private string _currentBlogId = null;

        public override ISmartContentEditor CreateEditor(ISmartContentEditorSite editorSite)
        {
            return new TagSmartContentEditor(Options, new CurrentBlogId(GetCurrentBlogId));
        }

        public delegate string CurrentBlogId();

        public string GetCurrentBlogId()
        {
            return _currentBlogId;
        }

        public override ResizeCapabilities ResizeCapabilities
        {
            get
            {
                return ResizeCapabilities.None;
            }
        }

        private string FormTagLinks(ISmartContent smartContent)
        {
            TagContext context = new TagContext(smartContent, Options, _currentBlogId);
            return context.CurrentProvider.GenerateHtmlForTags(context.Tags);
        }

        public override void EditOptions(IBlogClientUIContext dialogOwner)
        {
            var ownerWindow = new Win32WindowImpl(dialogOwner.NativeWindowHandle);
            TagOptions options = new TagOptions();
            options.Initialize(new TagProviderManager(Options));
            options.ShowDialog(ownerWindow);
        }

    }
}
