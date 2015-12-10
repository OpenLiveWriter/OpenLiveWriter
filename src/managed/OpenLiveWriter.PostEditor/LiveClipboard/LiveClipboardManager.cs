// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.Resources;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Api;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.LiveClipboard
{
    internal class LiveClipboardFormatHandler
    {
        public LiveClipboardFormatHandler(LiveClipboardContentSourceAttribute lcAttribute, ContentSourceInfo contentSource)
        {
            ResourceManager resMan = new ResourceManager(contentSource.GetType());
            string resourceNamePrefix = "LiveClipboardContentSource." + lcAttribute.Name + ".";

            _format = new LiveClipboardFormat(lcAttribute.ContentType, lcAttribute.Type);
            _formatName = LoadResourcedString(resMan, resourceNamePrefix + "Name", lcAttribute.Name);
            _formatDescription = LoadResourcedString(resMan, resourceNamePrefix + "Description", lcAttribute.Description);
            _formatImagePath = lcAttribute.ImagePath;
            _contentSource = contentSource;
        }

        private string LoadResourcedString(ResourceManager resMan, string name, string defaultValue)
        {
            try
            {
                string strValue = resMan.GetString(name);
                if (strValue == null)
                    return defaultValue;
                else
                    return strValue;
            }
            catch (MissingManifestResourceException)
            {
                return defaultValue;
            }
        }

        public LiveClipboardFormat Format
        {
            get { return _format; }
        }
        private LiveClipboardFormat _format;

        public string FormatName
        {
            get { return _formatName; }
        }
        private string _formatName;

        public string FormatDescription
        {
            get
            {
                if (_formatDescription != String.Empty)
                    return _formatDescription;
                else
                    return String.Format(CultureInfo.CurrentCulture, "{0} format", FormatName);
            }
        }
        private string _formatDescription;

        public Image FormatImage
        {
            get
            {
                try
                {
                    Image formatImage = ResourceHelper.LoadAssemblyResourceBitmap(ContentSource.Type.Assembly, ContentSource.Type.Namespace, _formatImagePath, false);
                    if (formatImage != null)
                        return formatImage;
                    else
                        return ContentSource.Image;
                }
                catch (Exception ex)
                // catch should not be necessary but we are adding this code late
                // so include it to be safe
                {
                    Trace.Fail("Unexpected exception thrown while trying to retreive live clipboard format image: " + ex.ToString());
                    return ContentSource.Image;
                }

            }
        }
        private string _formatImagePath;

        public string FriendlyContentType
        {
            get
            {
                if (Format.Type == String.Empty)
                    return Format.ContentType;
                else
                    return String.Format(CultureInfo.CurrentCulture, "{0} ({1})", Format.Type, Format.ContentType);
            }
        }

        public ContentSourceInfo ContentSource
        {
            get { return _contentSource; }
        }
        private ContentSourceInfo _contentSource;

        public class NameComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return (x as LiveClipboardFormatHandler).FormatName.CompareTo((y as LiveClipboardFormatHandler).FormatName);
            }
        }

    }

    internal sealed class LiveClipboardManager
    {
        public static ContentSourceInfo FindContentSourceForLiveClipboard(LiveClipboardFormat[] formats)
        {
            // formats are supposed to be arranged in order of richness/desirability,
            // iterate through the formats in this order searching for a content-source
            // that can handle them
            foreach (LiveClipboardFormat format in formats)
            {
                ContentSourceInfo contentSource = FindContentSourceForLiveClipboardFormat(format);
                if (contentSource != null)
                    return contentSource;
            }

            // didn't find a match
            return null;
        }

        public static ContentSourceInfo FindContentSourceForLiveClipboardFormat(LiveClipboardFormat format)
        {
            // see if the preferred content source is noted in the registry
            string contentSourceId = GetContentSourceIdForFormat(format);
            if (contentSourceId != null)
            {
                // if the content-source is still installed and active then return it
                ContentSourceInfo contentSourceInfo = ContentSourceManager.FindContentSource(contentSourceId);
                if (contentSourceInfo != null)
                    return contentSourceInfo;
            }

            // didn't find a valid preconfigured entry, scan all sources to see if we've got one
            foreach (ContentSourceInfo contentSourceInfo in ContentSourceManager.ActiveContentSources)
            {
                foreach (LiveClipboardFormatHandler csFormatHandler in contentSourceInfo.LiveClipboardFormatHandlers)
                {
                    if (csFormatHandler.Format.Equals(format))
                    {
                        // note that this is now our default
                        SetContentSourceForFormat(format, contentSourceInfo.Id);

                        // return the source
                        return contentSourceInfo;
                    }
                }
            }

            // no match found
            return null;
        }

        /// <summary>
        /// Return all supported formats in alphabetical order by format des
        /// </summary>
        public static LiveClipboardFormat[] SupportedLiveClipboardFormats
        {
            get
            {
                // build a list of the unique formats supported by the system
                ArrayList supportedFormats = new ArrayList();
                foreach (ContentSourceInfo contentSourceInfo in ContentSourceManager.ActiveContentSources)
                {
                    foreach (LiveClipboardFormatHandler formatHandler in contentSourceInfo.LiveClipboardFormatHandlers)
                        if (!supportedFormats.Contains(formatHandler.Format))
                            supportedFormats.Add(formatHandler.Format);
                }

                return supportedFormats.ToArray(typeof(LiveClipboardFormat)) as LiveClipboardFormat[];
            }
        }

        public static LiveClipboardFormatHandler[] LiveClipboardFormatHandlers
        {
            get
            {
                ArrayList formatHandlers = new ArrayList();
                foreach (LiveClipboardFormat format in SupportedLiveClipboardFormats)
                {
                    // find the content source for this format
                    ContentSourceInfo contentSource = FindContentSourceForLiveClipboardFormat(format);
                    if (contentSource != null)
                    {
                        // still need to scan all of the sources supported formats for a match
                        foreach (LiveClipboardFormatHandler formatHandler in contentSource.LiveClipboardFormatHandlers)
                            if (formatHandler.Format.Equals(format))
                                formatHandlers.Add(formatHandler);
                    }
                    else
                    {
                        Trace.Fail("Expected to find content source for format!!!!");
                    }
                }

                // sort them by name as a convenience to the caller
                formatHandlers.Sort(new LiveClipboardFormatHandler.NameComparer());

                // return the list
                return formatHandlers.ToArray(typeof(LiveClipboardFormatHandler)) as LiveClipboardFormatHandler[];
            }
        }

        public static ContentSourceInfo[] GetContentSourcesForFormat(LiveClipboardFormat format)
        {
            ArrayList contentSources = new ArrayList();

            foreach (ContentSourceInfo contentSourceInfo in ContentSourceManager.ActiveContentSources)
            {
                foreach (LiveClipboardFormatHandler supportedFormatHandler in contentSourceInfo.LiveClipboardFormatHandlers)
                    if (supportedFormatHandler.Format.Equals(format))
                        contentSources.Add(contentSourceInfo);
            }
            return contentSources.ToArray(typeof(ContentSourceInfo)) as ContentSourceInfo[];
        }

        public static void SetContentSourceForFormat(LiveClipboardFormat format, string contentSourceId)
        {
            using (SettingsPersisterHelper formatSettings = _formatHandlerSettings.GetSubSettings(format.Id))
                formatSettings.SetString(CONTENT_SOURCE_ID, contentSourceId);
        }

        private static string GetContentSourceIdForFormat(LiveClipboardFormat format)
        {
            using (SettingsPersisterHelper formatSettings = _formatHandlerSettings.GetSubSettings(format.Id))
                return formatSettings.GetString(CONTENT_SOURCE_ID, null);
        }

        private static SettingsPersisterHelper _liveClipboardSettings = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("LiveClipboard");
        private static SettingsPersisterHelper _formatHandlerSettings = _liveClipboardSettings.GetSubSettings("Formats");
        private const string CONTENT_SOURCE_ID = "ContentSourceId";

    }

    internal class LiveClipboardComponentDisplay
    {
        public LiveClipboardComponentDisplay(ContentSourceInfo contentSource)
        {
            // if this format handler is "built-in" then override the content-source to
            // list it as "windows-live writer"
            if (ContentSourceManager.ContentSourceIsPlugin(contentSource.Id))
            {
                _icon = contentSource.Image;
                _name = contentSource.Name;
            }
            else
            {
                _icon = _writerLogoBitmap;
                _name = ApplicationEnvironment.ProductName;
            }
        }

        public Image Icon { get { return _icon; } }
        private Image _icon;
        public string Name { get { return _name; } }
        private string _name;

        private Bitmap _writerLogoBitmap = ResourceHelper.LoadAssemblyResourceBitmap("LiveClipboard.Images.HandledByOpenLiveWriter.png");

    }
}

