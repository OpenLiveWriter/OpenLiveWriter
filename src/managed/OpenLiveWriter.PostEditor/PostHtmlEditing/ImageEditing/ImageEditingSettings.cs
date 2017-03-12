// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public sealed class ImageEditingSettings
    {

        public static Size DefaultImageSizeLarge
        {
            get { return SettingsKey.GetSize(LARGE_IMAGE_SIZE, new Size(1024, 768)); }
            set { SettingsKey.SetSize(LARGE_IMAGE_SIZE, value); }
        }
        private const string LARGE_IMAGE_SIZE = "ImageSizeLarge";

        public static Size DefaultImageSizeMedium
        {
            get { return SettingsKey.GetSize(MEDIUM_IMAGE_SIZE, new Size(640, 480)); }
            set { SettingsKey.SetSize(MEDIUM_IMAGE_SIZE, value); }
        }
        private const string MEDIUM_IMAGE_SIZE = "ImageSizeMedium";

        public static Size DefaultImageSizeSmall
        {
            get { return SettingsKey.GetSize(SMALL_IMAGE_SIZE, new Size(240, 240)); }
            set { SettingsKey.SetSize(SMALL_IMAGE_SIZE, value); }
        }
        private const string SMALL_IMAGE_SIZE = "ImageSizeSmall";

        internal static SettingsPersisterHelper SettingsKey = HtmlEditorSettings.SettingsKey.GetSubSettings("ImageEditing");
    }
    public enum ImageBorderType { None, DropShadow, Photo };
}
