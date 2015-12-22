// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;
using System.Collections.Generic;
using OpenLiveWriter.PostEditor.Video.VideoService;
using OpenLiveWriter.PostEditor.Video.YouTube;

namespace OpenLiveWriter.PostEditor.Video
{
    [WriterPlugin(VideoContentSource.ID, "Videos",
        ImagePath = "Images.InsertVideo.png",
        PublisherUrl = "http://www.microsoft.com",
        Description = "Publish videos to your weblog.")]

    [InsertableContentSource("Videos")]
    [UrlContentSourceAttribute("xxx")]
    [CustomLocalizedPlugin("Videos")]

    public class VideoContentSource : SmartContentSource, IHandlesMultipleUrls, ISupportsDragDropFile, IContentUpdateFilter, ITabbedInsertDialogContentSource
    {
        public const string ID = "0ABB7CC8-30EB-4F34-8080-22DA77ED20C3";
        public enum Tab
        {
            Web,
            File,
            Service
        }

        public override SmartContentEditor CreateEditor(ISmartContentEditorSite contentEditorSite)
        {
            return new VideoEditorControl(contentEditorSite);
        }

        public override DialogResult CreateContent(IWin32Window dialogOwner, ISmartContent content)
        {
            IBlogContext blogContext = dialogOwner as IBlogContext;

            List<MediaTab> videoSources = new List<MediaTab>();

            videoSources.Add(new WebVideoSource());

            bool youtubeEnabled = MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.YouTubeVideo);

            if (youtubeEnabled)
            {
                videoSources.Add(new VideoPublishSource(Options.GetString("Video.lastPermission", String.Empty)));

                VideoServiceVideoSource source = new VideoServiceVideoSource();
                source.RegisterServices(new IVideoService[] { new YouTubeVideoService() });

                videoSources.Add(source);
            }

            return CreateContentForm(dialogOwner, content, videoSources, 0);
        }

        public override void CreateContentFromUrl(string url, ref string title, ISmartContent content)
        {
            try
            {
                // download the video
                Video video = VideoProviderManager.FindVideo(url);
                if (video == null)
                {
                    throw new ContentCreationException(
                        Res.Get(StringId.Plugin_Video_Cannot_Parse_Url),
                        Res.Get(StringId.Plugin_Video_Cannot_Parse_Url_Message));
                }
                VideoSmartContent vsc = new VideoSmartContent(content);
                vsc.Initialize(video, null);
            }
            catch (ContentCreationException)
            {
                throw;
            }
            catch (VideoPluginException ex)
            {
                throw new ContentCreationException(ex.Title, ex.Description);
            }
            catch (Exception ex)
            {
                VideoPluginUnexpectedException exception = new VideoPluginUnexpectedException(ex);
                throw new ContentCreationException(exception.Title, exception.Description);
            }
        }

        public void CreateContentFromEmbed(string embed, ISmartContent content)
        {
            try
            {
                // download the video
                Video video = VideoProviderManager.FindVideo(embed);
                VideoSmartContent vsc = new VideoSmartContent(content);
                vsc.Initialize(video, null);
                if (video == null)
                {
                    throw new ContentCreationException(
                        "Unable to Parse Video Embed", "Unknown provider or incomplete embed.");
                }
            }
            catch (ContentCreationException)
            {
                throw;
            }
            catch (VideoPluginException ex)
            {
                throw new ContentCreationException(ex.Title, ex.Description);
            }
            catch (Exception ex)
            {
                VideoPluginUnexpectedException exception = new VideoPluginUnexpectedException(ex);
                throw new ContentCreationException(exception.Title, exception.Description);
            }
        }

        public override string GenerateEditorHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            VideoSmartContent VideoContent = new VideoSmartContent(content);
            string html = VideoContent.GenerateEditorHtml(publishingContext);
            ((IInternalContent)content).RefreshCallback = VideoContent.GetRefreshCallback();
            return html;
        }

        public override string GeneratePublishHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            VideoSmartContent VideoContent = new VideoSmartContent(content);
            return VideoContent.GeneratePublishHtml(publishingContext);
        }

        public override ResizeCapabilities ResizeCapabilities
        {
            get
            {
                return ResizeCapabilities.Resizable | ResizeCapabilities.PreserveAspectRatio;
            }
        }

        public override void OnResizeStart(ISmartContent content, ResizeOptions options)
        {
            // access content object
            VideoSmartContent VideoContent = new VideoSmartContent(content);

            // make sure we keep to the current aspect ratio
            options.AspectRatio = VideoContent.HtmlSize.Width / (double)VideoContent.HtmlSize.Height;
        }

        public override void OnResizing(ISmartContent content, Size newSize)
        {
            // access content object
            VideoSmartContent VideoContent = new VideoSmartContent(content);

            // update html size
            VideoContent.HtmlSize = newSize;
        }

        public override void OnResizeComplete(ISmartContent content, Size newSize)
        {
            // access content object
            VideoSmartContent VideoContent = new VideoSmartContent(content);

            // update html size
            VideoContent.HtmlSize = newSize;
        }

        bool IHandlesMultipleUrls.HasUrlMatch(string url)
        {
            VideoProvider provider = VideoProviderManager.FindProviderFromUrl(url);
            return (provider != null);
        }

        #region ISupportsDragDropFile Members

        public DialogResult CreateContentFromFile(IWin32Window dialogOwner, ISmartContent content, string[] files, object context)
        {
            bool youtubeEnabled = MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.YouTubeVideo);
            if (!youtubeEnabled)
            {
                return DialogResult.Cancel;
            }

            Trace.Assert(files.Length == 1, "Cannot insert more then 1 video at once through drag and drop.");

            IBlogContext blogContext = dialogOwner as IBlogContext;

            List<MediaTab> videoSources = new List<MediaTab>();

            videoSources.Add(new WebVideoSource());

            VideoPublishSource videoPublishSource = new VideoPublishSource(Options.GetString("Video.lastPermission", String.Empty));
            videoPublishSource.SelectedPath = files[0];
            videoSources.Add(videoPublishSource);

            VideoServiceVideoSource source = new VideoServiceVideoSource();
            source.RegisterServices(new IVideoService[] { new YouTubeVideoService() });

            videoSources.Add(source);

            return CreateContentForm(dialogOwner, content, videoSources, 1);
        }

        public DialogResult CreateContentFromTabbedDialog(IWin32Window dialogOwner, ISmartContent content, int selectedTab)
        {
            bool youtubeEnabled = MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.YouTubeVideo);
            if (!youtubeEnabled)
            {
                return DialogResult.Cancel;
            }

            IBlogContext blogContext = dialogOwner as IBlogContext;

            List<MediaTab> videoSources = new List<MediaTab>();

            videoSources.Add(new WebVideoSource());

            VideoPublishSource videoPublishSource = new VideoPublishSource(Options.GetString("Video.lastPermission", String.Empty));
            //videoPublishSource.SelectedPath = files[0];
            videoSources.Add(videoPublishSource);

            VideoServiceVideoSource source = new VideoServiceVideoSource();
            source.RegisterServices(new IVideoService[] { new YouTubeVideoService() });

            videoSources.Add(source);

            return CreateContentForm(dialogOwner, content, videoSources, selectedTab);
        }

        #endregion

        private DialogResult CreateContentForm(IWin32Window dialogOwner, ISmartContent content, List<MediaTab> videoSources, int selectedTab)
        {
            using (new WaitCursor())
            {
                IBlogContext blogContext = dialogOwner as IBlogContext;
                VideoSmartContent videoSmartContent = new VideoSmartContent(content);
                using (MediaInsertForm videoBrowserForm = new MediaInsertForm(videoSources, blogContext.CurrentAccountId, selectedTab, videoSmartContent, Res.Get(StringId.Plugin_Videos_Select_Video_Form), true))
                {
                    if (videoBrowserForm.ShowDialog(dialogOwner) == DialogResult.OK)
                    {
                        string VIDEO_LAST_PERMISSION = "Video.lastPermission";
                        Options.SetString(VIDEO_LAST_PERMISSION, videoSmartContent.Permission);

                        videoBrowserForm.SaveContent(videoSmartContent);

                        return DialogResult.OK;
                    }
                    else
                    {
                        return DialogResult.Cancel;
                    }
                }
            }
        }

        #region IContentUpdateFilter Members

        public bool ShouldUpdateContent(string oldHTML, string newHTML)
        {
            HtmlExtractor exOld = new HtmlExtractor(oldHTML);
            HtmlExtractor exNew = new HtmlExtractor(newHTML);

            HtmlExtractor exImgOld = exOld.Seek("<img title>");
            HtmlExtractor exImgNew = exNew.Seek("<img title>");

            if (exImgOld.Success &&
               exImgNew.Success &&
               ((BeginTag)exImgOld.Element).GetAttributeValue("title") == ((BeginTag)exImgNew.Element).GetAttributeValue("title"))
            {
                return false;
            }

            return true;
        }

        #endregion

    }
}
