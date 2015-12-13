// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net;
using System.Web;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.BlogProviderButtons
{
    public class BlogProviderButton : IDisposable
    {
        public BlogProviderButton(string blogId, string hostBlogId, string homepageUrl, string postApiUrl, string buttonId)
        {
            _blogId = blogId;
            _hostBlogId = hostBlogId;
            _homepageUrl = UrlHelper.InsureTrailingSlash(homepageUrl);
            _postApiUrl = postApiUrl;
            _buttonId = buttonId;
            _settingsKey = BlogSettings.GetProviderButtonsSettingsKey(blogId).GetSubSettings(buttonId);
            _buttonDescription = new BlogProviderButtonDescriptionFromSettings(_settingsKey);
        }

        public void Dispose()
        {
            if (_settingsKey != null)
                _settingsKey.Dispose();
        }

        public string BlogId
        {
            get
            {
                return _blogId;
            }
        }

        // id
        public string Id
        {
            get
            {
                return _buttonDescription.Id;
            }
        }

        // tooltip text
        public string Description
        {
            get
            {
                return _buttonDescription.Description;
            }
        }

        // url to navigate to when clicking the button
        public string ClickUrl
        {
            get
            {
                return FormatUrl(_buttonDescription.ClickUrl);
            }
        }

        public bool SupportsClick
        {
            get
            {
                return _buttonDescription.SupportsClick;
            }
        }

        public string ContentUrl
        {
            get
            {
                return FormatUrl(_buttonDescription.ContentUrl);
            }
        }

        public string ContentQueryUrl
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}?blog_id={1}&button_id={2}", ContentUrl, HttpUtility.UrlEncode(_hostBlogId), HttpUtility.UrlEncode(_buttonId));
            }
        }

        // supports display of content from a drop-down
        public bool SupportsContent
        {
            get
            {
                return _buttonDescription.SupportsContent;
            }
        }

        // the current image
        public Bitmap CurrentImage
        {
            get
            {
                if (SupportsNotification && ShowNotificationImage)
                {
                    return SafeGetNotificationImage();
                }
                else
                {
                    return Image;
                }
            }
        }

        public string CurrentText
        {
            get
            {
                if (SupportsNotification && ShowNotificationText)
                    return NotificationText;
                else
                    return Description;
            }
        }

        /// <summary>
        /// Initial connection to application frame. Supress notification image and force
        /// an immediate polling for notification status
        /// </summary>
        public void ConnectToFrame()
        {
            ShowNotificationImage = false;
            _settingsKey.SetDateTime(NOTIFICATION_POLLING_TIME, DateTimeHelper.UtcNow);
        }

        public Size ContentDisplaySize
        {
            get
            {
                Size displaySize = _settingsKey.GetSize(CONTENT_DISPLAY_SIZE, Size.Empty);
                if (displaySize != Size.Empty)
                    return displaySize;
                else
                    return DefaultContentSize;
            }
        }
        private const string CONTENT_DISPLAY_SIZE = "ContentDisplaySize";
        private readonly Size DefaultContentSize = new Size(300, 350);

        public void RecordButtonClicked()
        {
            if (ClearNotificationOnClick)
            {
                ShowNotificationImage = false;

                NotificationText = String.Empty;

                // fire notification events
                BlogProviderButtonNotificationSink.FireNotificationEvent(BlogId, Id);
            }
        }

        public void CheckForNotification()
        {
            try
            {
                if ((DateTimeHelper.UtcNow >= NotificationPollingTime) && WinInet.InternetConnectionAvailable)
                {
                    // poll for notification
                    IBlogProviderButtonNotification buttonNotification = null;
                    using (new BlogClientUIContextSilentMode())
                        buttonNotification = GetButtonNotification();

                    // update notification text under control of the apply updates lock (the lock along
                    // with the check for a valid blog-id immediately below ensures that we a background
                    // notification never creates a "crufty" blog-id by writing to a BlogSettings key
                    // that has already been deleted).
                    using (BlogSettings.ApplyUpdatesLock(BlogId))
                    {
                        if (BlogSettings.BlogIdIsValid(BlogId))
                        {
                            NotificationText = buttonNotification.NotificationText;

                            // update notification image
                            ShowNotificationImage = SafeUpdateNotificationImage(buttonNotification.NotificationImage);

                            // update clear notification flag
                            ClearNotificationOnClick = buttonNotification.ClearNotificationOnClick;

                            // set next polling time
                            UpdateNotificationPollingTime(buttonNotification.PollingInterval);
                        }
                        else
                        {
                            throw new InvalidOperationException("Attempted update notification data for invalid blog-id");
                        }
                    }

                    // fire notification events
                    BlogProviderButtonNotificationSink.FireNotificationEvent(BlogId, Id);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error occurred polling for button notification: " + ex.ToString());
            }
        }

        private string NotificationUrl
        {
            get
            {
                return FormatUrl(_buttonDescription.NotificationUrl);
            }
        }

        private IBlogProviderButtonNotification GetButtonNotification()
        {
            string notificationUrl = String.Format(
                CultureInfo.InvariantCulture,
                "{0}?blog_id={1}&button_id={2}&image_url={3}",
                NotificationUrl,
                HttpUtility.UrlEncode(_hostBlogId),
                HttpUtility.UrlEncode(_buttonId),
                HttpUtility.UrlEncode(ImageUrl));

            // get the content
            HttpWebResponse response = null;
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                using (Blog blog = new Blog(_blogId))
                    response = blog.SendAuthenticatedHttpRequest(notificationUrl, 10000);

                // parse the results
                xmlDocument.Load(response.GetResponseStream());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            // create namespace manager
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
            nsmgr.AddNamespace("n", "http://schemas.microsoft.com/wlw/buttons/notification");

            // throw if the root element is not manifest
            if (xmlDocument.DocumentElement.LocalName.ToLower(CultureInfo.InvariantCulture) != "notification")
                throw new ArgumentException("Not a valid writer button notification");

            // polling interval
            int checkAgainMinutes = XmlHelper.NodeInt(xmlDocument.SelectSingleNode("//n:checkAgainMinutes", nsmgr), 0);
            if (checkAgainMinutes == 0)
            {
                throw new ArgumentException("You must specify a value for checkAgainMinutes");
            }
            TimeSpan pollingInterval = TimeSpan.FromMinutes(checkAgainMinutes);

            // notification text
            string notificationText = XmlHelper.NodeText(xmlDocument.SelectSingleNode("//n:text", nsmgr));

            // notification image
            Bitmap notificationImage = null;
            string notificationImageUrl = XmlHelper.NodeText(xmlDocument.SelectSingleNode("//n:imageUrl", nsmgr));
            if (notificationImageUrl != String.Empty)
            {
                // compute the absolute url then allow parameter substitution
                notificationImageUrl = BlogClientHelper.GetAbsoluteUrl(notificationImageUrl, NotificationUrl);
                notificationImageUrl = BlogClientHelper.FormatUrl(notificationImageUrl, _homepageUrl, _postApiUrl, _hostBlogId);

                // try to download it (will use the cache if available)
                // note that failing to download it is a recoverable error, we simply won't show a notification image
                try
                {
                    // try to get a credentials context for the download
                    WinInetCredentialsContext credentialsContext = null;
                    try
                    {
                        credentialsContext = BlogClientHelper.GetCredentialsContext(_blogId, notificationImageUrl);
                    }
                    catch (BlogClientOperationCancelledException)
                    {
                    }

                    // execute the download
                    notificationImage = ImageHelper.DownloadBitmap(notificationImageUrl, credentialsContext);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error downloading notification image: " + ex.ToString());
                }
            }

            // clear notification on click
            bool clearNotificationOnClick = XmlHelper.NodeBool(xmlDocument.SelectSingleNode("//n:resetOnClick", nsmgr), true);

            // return the notification
            return new BlogProviderButtonNotification(pollingInterval, notificationText, notificationImage, clearNotificationOnClick);
        }

        // icons
        private string ImageUrl { get { return _buttonDescription.ImageUrl; } }

        private Bitmap Image { get { return _buttonDescription.Image; } }

        // supports polling for a notification image
        private bool SupportsNotification { get { return _buttonDescription.SupportsNotification; } }

        private DateTime NotificationPollingTime
        {
            get { return _settingsKey.GetDateTime(NOTIFICATION_POLLING_TIME, DateTimeHelper.UtcNow); }
        }

        private void UpdateNotificationPollingTime(TimeSpan pollingInterval)
        {
            // enforce minimum polling interval of one minute
            pollingInterval = pollingInterval >= TimeSpan.FromMinutes(1) ? pollingInterval : TimeSpan.FromMinutes(1);

            // set the next polling time
            _settingsKey.SetDateTime(NOTIFICATION_POLLING_TIME, DateTimeHelper.UtcNow.Add(pollingInterval));
        }
        private const string NOTIFICATION_POLLING_TIME = "NotificationPollingTime";

        private bool SafeUpdateNotificationImage(Bitmap notificationImage)
        {
            try
            {
                if (notificationImage == null)
                    return false;

                _settingsKey.SetByteArray(NOTIFICATION_IMAGE, ImageHelper.GetBitmapBytes(notificationImage, new Size(24, 24)));
                return true;
            }
            catch (Exception ex)
            {
                Trace.Fail(String.Format(CultureInfo.InvariantCulture, "Unexpected exception occurred updating notification image: {0}", ex.ToString()));
                return false;
            }
        }
        private const string NOTIFICATION_IMAGE = "NotificationImage";

        private Bitmap SafeGetNotificationImage()
        {
            try
            {
                byte[] notificationImageBytes = _settingsKey.GetByteArray(NOTIFICATION_IMAGE, null);
                return new Bitmap(new MemoryStream(notificationImageBytes));
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception loading notification image: " + ex.ToString());
                return Image;
            }
        }

        private string NotificationText
        {
            get { return _settingsKey.GetString(NOTIFICATION_TEXT, String.Empty); }
            set { _settingsKey.SetString(NOTIFICATION_TEXT, value); }
        }
        private const string NOTIFICATION_TEXT = "NotificationText";

        private bool ShowNotificationText
        {
            get { return NotificationText != String.Empty; }
        }
        private const string SHOW_NOTIFICATION_TEXT = "ShowNotificationText";

        private bool ShowNotificationImage
        {
            get { return _settingsKey.GetBoolean(SHOW_NOTIFICATION_IMAGE, false); }
            set { _settingsKey.SetBoolean(SHOW_NOTIFICATION_IMAGE, value); }
        }
        private const string SHOW_NOTIFICATION_IMAGE = "ShowNotificationImage";

        private bool ClearNotificationOnClick
        {
            get { return _settingsKey.GetBoolean(CLEAR_NOTIFICATION_ON_CLICK, true); }
            set { _settingsKey.SetBoolean(CLEAR_NOTIFICATION_ON_CLICK, value); }
        }
        private const string CLEAR_NOTIFICATION_ON_CLICK = "ClearNotificationOnClick";

        internal string FormatUrl(string url)
        {
            return BlogClientHelper.FormatUrl(url, _homepageUrl, _postApiUrl, _hostBlogId);
        }

        private readonly string _blogId;
        private readonly string _hostBlogId;
        private readonly string _homepageUrl;
        private readonly string _postApiUrl;
        private readonly string _buttonId;
        private readonly SettingsPersisterHelper _settingsKey;
        private readonly BlogProviderButtonDescriptionFromSettings _buttonDescription;
    }
}

