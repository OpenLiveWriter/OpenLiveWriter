// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.Video.YouTube;

namespace OpenLiveWriter.PostEditor.Video
{
    public enum VideoPlayerStyle
    {
        Automatic,
        PreviewWithLink,
        EmbeddedInPage
    }

    internal class VideoSmartContent : MediaSmartContent
    {

        public VideoSmartContent(ISmartContent content)
            : base(content)
        {
            MinSize = new Size(1, 1);
        }

        public void Initialize(Video video, string blogID)
        {
            // save video metadata
            _content.Properties.SetString(VIDEO_AUTHOR, video.Url);
            _content.Properties.SetString(VIDEO_ID, video.Id);
            _content.Properties.SetString(VIDEO_EMBED, video.Embed);
            _content.Properties.SetString(VIDEO_PROVIDER_ID, (video.Provider != null ? video.Provider.ServiceId : String.Empty));
            _content.Properties.SetString(VIDEO_URL, video.Url);
            _content.Properties.SetString(VIDEO_THUMBNAIL_URL, video.ThumbnailUrl);
            _content.Properties.SetBoolean(VIDEO_HAS_PROGRESS, video.IsUploading);
            _content.Properties.SetString(VIDEO_EDITOR_FORMAT, video.EditorFormat);
            _content.Properties.SetString(VIDEO_PERMISSION, video.Permission);
            HtmlSize = new Size(video.Width, video.Height);
            AspectRatioType = video.AspectRatioType;

            _content.Properties.SetString(VIDEO_USERNAME, video.Username);

            _provider = video.Provider;

            if (VideoHasProgress())
                SaveBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Video.Images.Progress.gif"), VIDEO_PROGRESS_PATH, ImageFormat.Gif);

            // If the video already has a snapshot, then we will want to save it, it came from the WebVideoSource
            if (video.Snapshot != null)
                UpdateVideoSnapshot(video.Snapshot);

            if (video.StatusWatcher != null)
                ((IInternalContent)_content).ObjectState = video.StatusWatcher;
        }

        // Make a new random path for the video snapshots
        private string MakeVideoFileName()
        {
            string newGuid = Guid.NewGuid().ToString();
            return "video" + newGuid.Substring(newGuid.LastIndexOf("-") + 1);
        }

        public void UpdateVideoSnapshot(Bitmap videoSnapshot)
        {
            string jpegImageFile = _content.Properties.GetString(VIDEO_PUBLISH_IMAGE, String.Empty);

            if (jpegImageFile == String.Empty)
            {
                jpegImageFile = MakeVideoFileName() + ".jpg";
                _content.Properties.SetString(VIDEO_PUBLISH_IMAGE, jpegImageFile);
            }

            SaveBitmap(videoSnapshot, jpegImageFile, ImageFormat.Jpeg);

            // save height and width of snapshot
            VideoSnapshotSize = new Size(videoSnapshot.Width, videoSnapshot.Height);
        }

        public string Id
        {
            get
            {
                return _content.Properties.GetString(VIDEO_ID, String.Empty);
            }
            set
            {
                _content.Properties.SetString(VIDEO_ID, value);
            }
        }

        public string Permission
        {
            get
            {
                return _content.Properties.GetString(VIDEO_PERMISSION, String.Empty);
            }
            set
            {
                _content.Properties.SetString(VIDEO_PERMISSION, value);
            }
        }

        public string Url
        {
            get
            {
                return _content.Properties.GetString(VIDEO_URL, String.Empty).Replace(VideoProvider.VIDEOID, Id);
            }
        }

        public string EmbedFormat
        {
            get
            {
                return _content.Properties.GetString(VIDEO_EMBED, String.Empty);
            }
        }

        public string EditorFormat
        {
            get
            {
                return VideoProvider.GenerateEmbedHtml(_content.Properties.GetString(VIDEO_EDITOR_FORMAT, String.Empty), Id, HtmlSize);
            }
            set
            {
                _content.Properties.SetString(VIDEO_EDITOR_FORMAT, value);
            }
        }

        public VideoProvider Provider
        {
            get
            {
                if (_provider == null)
                    _provider = VideoProviderManager.FindProviderFromProviderId(ProviderId);
                return _provider;
            }
        }

        public string ProviderId
        {
            get
            {
                return _content.Properties.GetString(VIDEO_PROVIDER_ID, String.Empty);
            }
        }

        public bool OpenInNewWindow
        {
            get
            {
                return _content.Properties.GetBoolean(NEW_WINDOW, true);
            }
            set
            {
                _content.Properties.SetBoolean(NEW_WINDOW, value);
            }
        }

        public string Caption
        {
            get
            {
                return _content.Properties.GetString(VIDEO_CAPTION, String.Empty);
            }
            set
            {
                _content.Properties.SetString(VIDEO_CAPTION, value);
            }

        }

        public VideoAspectRatioType AspectRatioType
        {
            get
            {
                VideoAspectRatioType aspectRatioType = VideoAspectRatioType.Unknown;
                string aspectRatioString = _content.Properties.GetString(VIDEO_ASPECT_RATIO, String.Empty);

                if (Enum.IsDefined(typeof(VideoAspectRatioType), aspectRatioString))
                {
                    aspectRatioType = (VideoAspectRatioType)Enum.Parse(typeof(VideoAspectRatioType), aspectRatioString);
                }

                return aspectRatioType;
            }
            set
            {
                _content.Properties.SetString(VIDEO_ASPECT_RATIO, value.ToString("G"));

                // Resize the smart content to fit in the new aspect ratio.
                if (value != VideoAspectRatioType.Unknown)
                {
                    HtmlSize = VideoAspectRatioHelper.ResizeHeightToAspectRatio(value, HtmlSize);
                }
            }
        }

        public ILayoutStyle LayoutStyle
        {
            get { return _content.Layout; }
        }

        public const string VideoThumbnailId = "VideoThumbnail";

        public static void DrawErrorMockPlayer(Bitmap img, BidiGraphics bidiGraphics, string message)
        {
            int LEFT_PADDING = 30;
            int RIGHT_PADDING = 10;
            int ERROR_IMAGE_WIDTH = 32;
            int ERROR_IMAGE_HEIGHT = 32;
            int IMAGE_TO_TEXT_PADDING = 7;
            int PLACEHOLDER_HEIGHT = img.Height;
            int PLACEHOLDER_WIDTH = img.Width;
            int VERTICAL_OFFSET = 30;
            int LINE_BREAK_PADDING = 4;

            Size titleSize = bidiGraphics.MeasureText(Res.Get(StringId.VideoError),
                                          Res.GetFont(FontSize.XXLarge, FontStyle.Bold),
                                          new Size(PLACEHOLDER_WIDTH - LEFT_PADDING - ERROR_IMAGE_WIDTH - IMAGE_TO_TEXT_PADDING - RIGHT_PADDING, PLACEHOLDER_HEIGHT - VERTICAL_OFFSET),
                                          TextFormatFlags.WordBreak);

            bidiGraphics.DrawText(Res.Get(StringId.VideoError),
                                  Res.GetFont(FontSize.XXLarge,
                                  FontStyle.Bold),
                                  new Rectangle(LEFT_PADDING + ERROR_IMAGE_WIDTH + IMAGE_TO_TEXT_PADDING,
                                                VERTICAL_OFFSET + (ERROR_IMAGE_HEIGHT / 2) - (titleSize.Height / 2),
                                                PLACEHOLDER_WIDTH - LEFT_PADDING - ERROR_IMAGE_WIDTH - IMAGE_TO_TEXT_PADDING - RIGHT_PADDING,
                                                PLACEHOLDER_HEIGHT - VERTICAL_OFFSET),
                                  Color.White,
                                  TextFormatFlags.WordBreak);

            bidiGraphics.DrawText(message,
                                  Res.GetFont(FontSize.XLarge,
                                  FontStyle.Regular),
                                  new Rectangle(LEFT_PADDING + ERROR_IMAGE_WIDTH + IMAGE_TO_TEXT_PADDING,
                                                VERTICAL_OFFSET + (ERROR_IMAGE_HEIGHT / 2) - (titleSize.Height / 2) + titleSize.Height + LINE_BREAK_PADDING,
                                                PLACEHOLDER_WIDTH - LEFT_PADDING - ERROR_IMAGE_WIDTH - IMAGE_TO_TEXT_PADDING - RIGHT_PADDING,
                                                PLACEHOLDER_HEIGHT - (VERTICAL_OFFSET + titleSize.Height + LINE_BREAK_PADDING)),
                                  Color.White,
                                  TextFormatFlags.WordBreak);

            bidiGraphics.DrawImage(false,
                       ResourceHelper.LoadAssemblyResourceBitmap("Video.Images.Error.gif"),
                       LEFT_PADDING,
                       VERTICAL_OFFSET);
        }

        public static void DrawStatusMockPlayer(Bitmap img, BidiGraphics bidiGraphics, string message)
        {
            bidiGraphics.DrawText(message, Res.GetFont(FontSize.XXLarge, FontStyle.Regular), new Rectangle(10, img.Height / 2, img.Width - 20, img.Height / 2), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
        }

        /// <summary>
        /// When publishing a video, this will create the image that put in the placeholder spot for the video.
        /// </summary>
        /// <param name="message">A message that will be drawn on the image</param>
        /// <returns></returns>
        private Bitmap CreateMockPlayer(string message)
        {
            // Draw the status on the image
            Bitmap img = ImageHelper2.CreateResizedBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Video.Images.MockPlayer.png"),
                                                          HtmlSize.Width,
                                                          HtmlSize.Height,
                                                          ImageFormat.Png);

            try
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    BidiGraphics bidiGraphics = new BidiGraphics(g, img.Size);
                    if (VideoHasError())
                    {
                        DrawErrorMockPlayer(img, bidiGraphics, message);
                    }
                    else
                    {
                        DrawStatusMockPlayer(img, bidiGraphics, message);
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error while creating a placeholder for a video: " + ex);
                return null;
            }

            return img;
        }

        public override string GenerateEditorHtml(IPublishingContext publishingContext)
        {
            string status = GenerateStatus(((IContentSourceSite)publishingContext).DialogOwner, ((IContentSourceSite)publishingContext).AccountId);

            // If the status has changed we need to show it to the user
            if (status != String.Empty && status != _content.Properties.GetString(VIDEO_STATUS, null))
            {
                using (Bitmap image = CreateMockPlayer(status))
                {
                    if (image != null)
                    {
                        // Save this new status
                        _content.Properties.SetString(VIDEO_STATUS, status);
                        UpdateVideoSnapshot(image);
                    }
                }
            }

            StringBuilder html = new StringBuilder();
            html.Append(GenerateEditorVideoHtml());
            html.Append(GenerateMetadataHtml(((IEditingMode)publishingContext).CurrentEditingMode == EditingMode.Wysiwyg));

            return html.ToString();
        }

        public override string GeneratePublishHtml(IPublishingContext publishingContext)
        {
            VideoContext context = new VideoContext(publishingContext);
            StringBuilder html = new StringBuilder();
            html.Append(GeneratePublishVideoHtml(context));
            html.Append(GenerateMetadataHtml(false));
            return html.ToString();
        }

        /// <summary>
        /// Easy way to figure out if we are tracking the progress of the video
        /// which would happen if we are publishing the video
        /// </summary>
        /// <returns></returns>
        private bool VideoHasProgress()
        {
            return _content.Properties.GetBoolean(VIDEO_HAS_PROGRESS, false);
        }

        private bool VideoHasError()
        {
            return _content.Properties.GetBoolean(VIDEO_HAS_ERROR, false);
        }

        /// <summary>
        /// Gets the status of a video.  In the event the video is already published to a service
        /// this function should return "".  When it is being published by WLW we will get the status
        /// from the publisher.  It needs a window so that it can prompt the user for the
        /// username/password if a status watcher needs to be created.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        private string GenerateStatus(IWin32Window window, string blogId)
        {
            if (VideoHasProgress())
            {
                IStatusWatcher publisher = (IStatusWatcher)((IInternalContent)_content).ObjectState;

                // check to see if we need to create a status watcher, this happens during a post load
                if (publisher == null)
                {
                    // Make a video for the publisher to create a watcher for
                    Video video = new Video(_content.Properties.GetString(VIDEO_ID, Guid.Empty.ToString()),
                                            _content.Properties.GetString(VIDEO_URL, String.Empty),
                                            EmbedFormat,
                                            EditorFormat,
                                            null,
                                            HtmlSize.Width,
                                            HtmlSize.Height,
                                            VideoAspectRatioType.Unknown);
                    video.Username = _content.Properties.GetString(VIDEO_USERNAME, null);

                    IVideoPublisher newPublisher;
                    if (Provider.IsYouTube)
                    {
                        newPublisher = new YouTubeVideoPublisher();
                    }
                    else
                    {
                        newPublisher = null;
                        Trace.Fail("Unknown video publisher has been found. ID: " + Provider.ServiceId);
                    }

                    newPublisher.Init(null, window, blogId);

                    // Try to create a new watcher that can be used
                    publisher = newPublisher.CreateStatusWatcher(video);
                    ((IInternalContent)_content).ObjectState = publisher;
                    ((IInternalContent)_content).RefreshCallback = DateTimeHelper.UtcNow;
                }

                // If the publisher couldnt make a new status watcher
                // we just try to take a snap shot and then pretend it is completed
                if (publisher == null)
                {
                    StopProgress(false);
                    return VideoPublishStatus.Completed.ToString();
                }

                // Check to see the status and if it is completed we can stop tracking the progress
                PublishStatus publishStatus = publisher.Status;

                Id = publishStatus.Id;

                if (publishStatus.Status == VideoPublishStatus.Completed)
                {
                    StopProgress(false);
                    return VideoPublishStatus.Completed.ToString();
                }

                if (publishStatus.Status == VideoPublishStatus.Error)
                {
                    StopProgress(true);
                    return publishStatus.DisplayMessage;
                }

                // return the status message
                return publishStatus.DisplayMessage;
            }
            return String.Empty;
        }

        /// <summary>
        /// When a video is finished or has an error it should no longer
        /// have settings that say it has progress.  Clears those out, and make
        /// sure that we arent going to be called back anymore.
        /// </summary>
        /// <param name="message">The last known status that the video had</param>
        private void StopProgress(bool hasError)
        {
            IStatusWatcher sw = ((IInternalContent)_content).ObjectState as IStatusWatcher;

            if (sw != null)
                sw.Dispose();

            // Remove the place holder on success
            if (!hasError)
            {
                RemoveBitmap(_content.Properties.GetString(VIDEO_PUBLISH_IMAGE, String.Empty));
                _content.Properties.Remove(VIDEO_PUBLISH_IMAGE);
            }

            RemoveBitmap(VIDEO_PROGRESS_PATH);
            _content.Properties.SetBoolean(VIDEO_HAS_PROGRESS, false);
            _content.Properties.SetBoolean(VIDEO_HAS_ERROR, hasError);
        }

        public string GenerateMetadataHtml(bool editMode)
        {

            StringBuilder metadataHtml = new StringBuilder();

            string editModeAttributes = string.Format(CultureInfo.InvariantCulture, " contentEditable=\"true\" maxCharactersAccepted=\"{0}\" class=\"{1}\" defaultText=\"{2}\" wlPropertyPath=\"{3}\" ",
                Res.Get(StringId.VideoCaptionMaxCharactersAccepted),
                InlineEditField.EDIT_FIELD,
                Res.Get(StringId.VideoCaptionDefaultText),
                HtmlUtils.EscapeEntities(VIDEO_CAPTION)
            );

            if (Caption.Trim() != String.Empty)
            {
                metadataHtml.Append(string.Format(CultureInfo.InvariantCulture, "<div style=\"width:{0}px;clear:both;font-size:{1}\"{2}>", HtmlSize.Width, Res.Get(StringId.Plugin_Video_Caption_Size), editMode ? editModeAttributes : string.Empty));
                metadataHtml.Append(Caption.Trim());
                metadataHtml.Append("</div>");

                return metadataHtml.ToString();
            }

            if (editMode)
            {
                metadataHtml.Append(string.Format(CultureInfo.InvariantCulture, "<div style=\"width:{0}px;clear:both;font-size:{1};\"{2}></div>", HtmlSize.Width, Res.Get(StringId.Plugin_Video_Caption_Size), editModeAttributes));
            }

            return metadataHtml.ToString();
        }

        internal DateTime? GetRefreshCallback()
        {
            if (_content.Properties.GetBoolean(VIDEO_HAS_PROGRESS, false))
            {
                return DateTimeHelper.UtcNow.AddSeconds(5);
            }
            //FIXME
            //((IStatusWatcher)((IInternalContent) _content).ObjectState).Dispose();
            //((IInternalContent) _content).ObjectState = null;
            return null;
        }

        internal ISmartContent SmartContent
        {
            get { return _content; }
        }

        private string GenerateEditorVideoHtml()
        {
            StringBuilder videoHtml = new StringBuilder();

            if (VideoHasError() || VideoHasProgress())
            {
                string videoImageFile = _content.Files.GetUri(_content.Properties.GetString(VIDEO_PUBLISH_IMAGE, String.Empty)).ToString();

                // Add the snapshot of the video
                videoHtml.Append(String.Format(CultureInfo.InvariantCulture,
                                             "<div id=\"{3}\" style=\"float:left;width:{0}px;height:{1}px;background-image:url({2})\">",
                                             HtmlSize.Width,
                                             HtmlSize.Height,
                                             HtmlUtils.EscapeEntities(videoImageFile),
                                             HtmlUtils.EscapeEntities(VideoThumbnailId)));

                Uri uriProgressPath = _content.Files.GetUri(VIDEO_PROGRESS_PATH);

                if (uriProgressPath != null)
                {
                    // If the video is being published, show the progress animation on the placeholder, the gif is 32x32
                    videoHtml.Append(String.Format(CultureInfo.InvariantCulture, @"<img style=""padding:0;margin:0;border-style:none"" title=""{2}"" src=""{0}"" alt=""{1}"" hspace=""{3}"" vspace=""{4}"" >",
                                     HtmlUtils.EscapeEntities(uriProgressPath.ToString()),
                                     HtmlUtils.EscapeEntities(Res.Get(StringId.Plugin_Video_Alt_Text)),
                                     _content.Properties.GetString(VIDEO_STATUS, null),
                                     (HtmlSize.Width / 2) - 16,
                                     (HtmlSize.Height / 2) - 32 - 15));
                }

                videoHtml.Append(("</div>"));

            }
            else
            {
                if (string.IsNullOrEmpty(EditorFormat))
                {
                    EditorFormat = Provider.EditorFormat;
                }

                // Add the snapshot of the video
                videoHtml.Append(EditorFormat);
            }

            return videoHtml.ToString();
        }

        private string GetSnapshotPathIfNeeded(HtmlType htmlType, VideoContext context)
        {
            // NOTE: We skip this behavior on IE9+ because of WinLive 589461.
            if (htmlType == HtmlType.ObjectHtml || ApplicationEnvironment.BrowserVersion.Major >= 9)
            {
                return null;
            }

            try
            {
                IHTMLElement element = context.GetElement(((IInternalContent)_content).Id);

                if (element != null)
                {
                    element = ((IHTMLDOMNode)element).firstChild as IHTMLElement;
                }

                if (element != null && (element is IViewObject))
                {
                    Bitmap snapshot = HtmlScreenCaptureCore.TakeSnapshot((IViewObject)element, element.offsetWidth, element.offsetHeight);
                    UpdateVideoSnapshot(snapshot);
                }

                Uri uri = _content.Files.GetUri(_content.Properties.GetString(VIDEO_PUBLISH_IMAGE, String.Empty));

                if (uri != null)
                    return uri.ToString();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to take video snapshot: " + ex);
            }

            return null;
        }

        private string GeneratePublishVideoHtml(VideoContext context)
        {
            //check for special cases--whitelist blog providers and such
            string output;
            if (VideoProviderManager.CheckForWhitelist(context.BlogProviderId, ProviderId, Id, HtmlSize, out output))
            {
                return String.Format(CultureInfo.InvariantCulture, "<div>{0}</div>", output);
            }

            // generate 'smart' html based on the user's preference
            AdaptiveHtmlObject adaptiveHtmlObject = new AdaptiveHtmlObject(VideoProvider.GenerateEmbedHtml(EmbedFormat, Id, HtmlSize), Url);
            if (HtmlSize != VideoSnapshotSize)
                adaptiveHtmlObject.PreviewImageSize = HtmlSize;
            adaptiveHtmlObject.OpenPreviewInNewWindow = OpenInNewWindow;
            HtmlType htmlType;

            //determine player style
            VideoPlayerStyle playerStyle = context.DetermineAppropriatePlayer(Url != String.Empty);
            switch (playerStyle)
            {
                case VideoPlayerStyle.Automatic:
                    htmlType = HtmlType.AdaptiveHtml;
                    break;

                case VideoPlayerStyle.PreviewWithLink:
                    htmlType = HtmlType.PreviewHtml;
                    break;

                case VideoPlayerStyle.EmbeddedInPage:
                    htmlType = HtmlType.ObjectHtml;
                    break;

                default:
                    Trace.Fail("Unexpected PlayerStyle: " + playerStyle.ToString());
                    htmlType = HtmlType.PreviewHtml;
                    break;
            }

            string path = GetSnapshotPathIfNeeded(htmlType, context);

            if (!string.IsNullOrEmpty(path))
            {
                adaptiveHtmlObject.PreviewImageSrc = path;
            }
            else
            {
                htmlType = HtmlType.ObjectHtml;
            }

            return adaptiveHtmlObject.GenerateHtml(htmlType);
        }

        internal Size VideoSnapshotSize
        {
            get
            {
                // get the size
                return new Size(_content.Properties.GetInt(VIDEO_WIDTH, DEFAULT_WIDTH), _content.Properties.GetInt(VIDEO_HEIGHT, DEFAULT_WIDTH));
            }
            set
            {
                // set the size
                _content.Properties.SetInt(VIDEO_WIDTH, value.Width);
                _content.Properties.SetInt(VIDEO_HEIGHT, value.Height);

                // update the html size to be the same
                HtmlSize = value;
            }
        }

        private string VIDEO_PROGRESS_PATH = "ProgressImage.gif";
        private string VIDEO_STATUS = "Video.status";
        private string VIDEO_HAS_ERROR = "Video.hasError";
        private string VIDEO_HAS_PROGRESS = "Video.videoHasProgress";
        private string VIDEO_AUTHOR = "Video.videoAuthor";
        private string VIDEO_ID = "Video.videoId";
        private string VIDEO_URL = "Video.videoUrl";
        private string VIDEO_THUMBNAIL_URL = "Video.videoThumbnailUrl";
        private string VIDEO_EMBED = "Video.videoEmbed";
        private string VIDEO_EDITOR_FORMAT = "Video.videoEditorFormat";
        private string VIDEO_PROVIDER_ID = "Video.videoProviderId";

        private string VIDEO_PUBLISH_IMAGE = "Video.videoPublishImage";

        private string NEW_WINDOW = "Video.new_window";
        private string VIDEO_WIDTH = "Video.video_width";
        private string VIDEO_HEIGHT = "Video.video_height";

        private string VIDEO_CAPTION = "Video.caption";
        private string VIDEO_USERNAME = "Video.userName";
        private string VIDEO_PERMISSION = "Video.permission";
        private string VIDEO_ASPECT_RATIO = "Video.aspectRatio";

        private VideoProvider _provider = null;

        internal void Delete()
        {
            // FIXME:It would be nice if this was copied to all the old versions
            // of this file in the SupportingFile store

            using (Bitmap img = CreateMockPlayer(Res.Get(StringId.VideoError)))
            {
                // Show the player as in an error state, stop tracking its progress
                UpdateVideoSnapshot(img);
            }

            _content.Properties.SetBoolean(VIDEO_HAS_PROGRESS, false);
            _content.Properties.SetString(VIDEO_STATUS, Res.Get(StringId.VideoError));

            //FIXME: The problem with doing this is that once you undo a delete file,
            // there is no way to turn it back on for call backs

            // If it is a video and has a watcher make sure it is taken care of
            IStatusWatcher statusWatcher = ((IInternalContent)_content).ObjectState as IStatusWatcher;

            if (statusWatcher == null)
                return;

            statusWatcher.CancelPublish();
            statusWatcher.Dispose();
            ((IInternalContent)_content).ObjectState = null;

        }
    }

    public class AdaptiveHtmlException : Exception
    {

    }
}
