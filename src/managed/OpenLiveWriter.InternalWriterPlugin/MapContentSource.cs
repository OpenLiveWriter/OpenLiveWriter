// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using HtmlScreenCapture = OpenLiveWriter.Api.HtmlScreenCapture;

namespace OpenLiveWriter.InternalWriterPlugin
{
    [WriterPlugin("C62021F8-9D77-4E84-BD14-18CE70F02159", "Live Search Maps",
         ImagePath = "Images.InsertMap.png",
         PublisherUrl = "http://local.live.com",
         Description = "Embed Live Search Maps in your blog posts.")]

    [InsertableContentSource("Live Search Map", SidebarText = "Map")]

    [CustomLocalizedPlugin("Map")]
    public class MapContentSource : SmartContentSource
    {
        public const string ID = "C62021F8-9D77-4E84-BD14-18CE70F02159";

        private MapOptions _pluginOptions;
        private readonly string clickToViewText = Res.Get(StringId.ViewMap);

        public override void Initialize(IProperties pluginOptions)
        {
            base.Initialize(pluginOptions);

            _pluginOptions = new MapOptions(pluginOptions);
        }

        public override SmartContentEditor CreateEditor(ISmartContentEditorSite contentEditorSite)
        {
            return new MapSidebarControl(_pluginOptions, contentEditorSite);
        }

        public override string GenerateEditorHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            return GenerateHtml(content, true, publishingContext.AccountId);
        }

        public override string GeneratePublishHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            return GenerateHtml(content, false, publishingContext.AccountId);
        }

        public override DialogResult CreateContent(IWin32Window dialogOwner, ISmartContent content)
        {
            using (new WaitCursor())
            {
                if (!MapForm.ValidateLiveLocalConnection(true))
                    return DialogResult.Cancel;

                using (MapForm mapForm = new MapForm(true, _pluginOptions, ((ICommandManagerHost)dialogOwner).CommandManager))
                {
                    mapForm.LoadMap(-0.3515603f, 0.3515625f, null, "r", 1, null);
                    if (mapForm.ShowDialog(dialogOwner) == DialogResult.OK)
                    {
                        MapSettings settings = new MapSettings(content.Properties);
                        settings.MapId = "map-" + Guid.NewGuid().ToString();
                        settings.UpdateSettings(mapForm.Latitude, mapForm.Longitude, mapForm.Reserved, mapForm.ZoomLevel, mapForm.MapStyle, mapForm.Pushpins, mapForm.BirdseyeScene);
                        _pluginOptions.DefaultDialogSize = mapForm.Size;

                        return DialogResult.OK;
                    }
                    else
                    {
                        return DialogResult.Cancel;
                    }
                }
            }
        }

        private string GenerateHtml(ISmartContent content, bool editor, string blogId)
        {
            MapSettings settings = new MapSettings(content.Properties);
            settings.PublishTargetId = blogId;

            Uri imageUri = content.Files.GetUri(settings.ImageFileId);

            if (imageUri == null)
            {
                //try to regenerate the image using the default size for blog
                Size mapSize = _pluginOptions.GetDefaultMapSize(blogId);
                UpdateMapImage(content, settings, mapSize);
                settings.Size = mapSize;

                imageUri = content.Files.GetUri(settings.ImageFileId);
            }

            string imgAltText = Res.Get(StringId.MapImageAltText);
            if (settings.Caption != String.Empty)
                imgAltText = settings.Caption;

            string imageHtml = settings.Caption;
            if (imageUri != null)
            {
                Size mapSize = settings.Size;
                string sizeAttrs = mapSize != Size.Empty
                                       ? String.Format(CultureInfo.InvariantCulture, " width=\"{0}\" height=\"{1}\"", mapSize.Width, mapSize.Height) : "";
                imageHtml = String.Format(CultureInfo.InvariantCulture, "<img src=\"{0}\"{1} alt=\"{2}\">", HtmlServices.HtmlEncode(imageUri.ToString()), sizeAttrs, HtmlServices.HtmlEncode(imgAltText));
            }
            else
            {
                if (imageHtml.Equals(String.Empty))
                {
                    imageHtml = Res.Get(StringId.ViewMap);
                }
            }

            string clickToViewAttr = editor ? "" : String.Format(CultureInfo.InvariantCulture, " alt=\"{0}\" title=\"{0}\"", clickToViewText);
            string mapHtml = String.Format(CultureInfo.InvariantCulture, "<a href=\"{0}\" id=\"{2}\"{3}>{1}</a>",
                HtmlServices.HtmlEncode(settings.LiveMapUrl), imageHtml, settings.MapId, clickToViewAttr);
            if (imageUri != null && settings.Caption != String.Empty)
            {
                //append the caption HTML
                mapHtml += String.Format(CultureInfo.InvariantCulture, _pluginOptions.CaptionHtmlFormat, HtmlServices.HtmlEncode(settings.Caption), settings.MapId);
            }
            return mapHtml;
        }

        internal static string CreateInnerAnchor(string imageUri, string mapAddress)
        {
            string anchorInnerHtml;
            if (imageUri != null)
            {
                anchorInnerHtml = "<img src=\"" + HtmlServices.HtmlEncode(imageUri) + "\" />";
            }
            else
            {
                anchorInnerHtml = mapAddress;
            }
            return anchorInnerHtml;
        }

        #region IResizableContentSource Members

        public override ResizeCapabilities ResizeCapabilities
        {
            get
            {
                return ResizeCapabilities.Resizable;
            }
        }

        public override void OnResizeStart(ISmartContent content, ResizeOptions options)
        {
            //force the resize operation to scale based on the size of the map image.
            MapSettings settings = new MapSettings(content.Properties);
            options.ResizeableElementId = settings.MapId;
        }

        public override void OnResizing(ISmartContent content, Size newSize)
        {
            MapSettings settings = new MapSettings(content.Properties);
            settings.Size = newSize;
        }

        public override void OnResizeComplete(ISmartContent content, Size newSize)
        {
            //don't allow the size to be smaller than the min size (fixes bug 398580)
            newSize.Width = Math.Max(newSize.Width, MIN_MAP_SIZE.Width);
            newSize.Height = Math.Max(newSize.Height, MIN_MAP_SIZE.Height);

            MapSettings settings = new MapSettings(content.Properties);
            UpdateMapImage(content, settings, newSize);
            settings.Size = newSize;

            if (settings.PublishTargetId != null)
                _pluginOptions.SetDefaultMapSize(settings.PublishTargetId, newSize);
        }
        private static readonly Size MIN_MAP_SIZE = new Size(20, 20);

        internal static void UpdateMapImage(ISmartContent content, MapSettings settings, Size newSize)
        {
            if (settings.MapImageInvalidated)
            {
                settings.MapImageInvalidated = false;
                float latitude = settings.Latitude;
                float longitude = settings.Longitude;
                string reserved = settings.Reserved;
                int zoomLevel = settings.ZoomLevel;
                string style = settings.MapStyle;
                string sceneId = settings.BirdseyeSceneId;
                VEBirdseyeScene scene = sceneId != null ? new VEBirdseyeScene(sceneId, settings.BirdseyeOrientation) : null;
                string previewUrl = MapUrlHelper.CreateMapUrl(LocalMapPreviewUrl, latitude, longitude, reserved, style, zoomLevel, settings.Pushpins, scene);

#pragma warning disable 612, 618
                HtmlScreenCapture screenCapture = new HtmlScreenCapture(new Uri(previewUrl, true), newSize.Width);
#pragma warning restore 612, 618

                screenCapture.HtmlDocumentAvailable += new OpenLiveWriter.Api.HtmlDocumentAvailableHandler(screenCapture_HtmlDocumentAvailable);
                screenCapture.MaximumHeight = newSize.Height;
                Bitmap bitmap = screenCapture.CaptureHtml(45000);
                if (bitmap != null)
                {
                    try
                    {
                        if (content.Files.Contains(settings.ImageFileId))
                            content.Files.Remove(settings.ImageFileId);
                    }
                    catch (Exception e) { Debug.Fail(e.ToString()); }

                    //add the new map image (note that we use a new name to a bug in IE7 that prevents the editor
                    //from refreshing the image displayed in the browser (bug 287563)
                    string newGuid = Guid.NewGuid().ToString();
                    string newImageFileId = "map-" + newGuid.Substring(newGuid.LastIndexOf("-", StringComparison.OrdinalIgnoreCase) + 1) + ".jpg";
                    settings.ImageFileId = newImageFileId;
                    content.Files.AddImage(settings.ImageFileId, bitmap, ImageFormat.Jpeg);
                }
                else
                {
                    Debug.WriteLine("Map image could not be regenerated.");
                }
            }
        }

        private static void screenCapture_HtmlDocumentAvailable(object sender, OpenLiveWriter.Api.HtmlDocumentAvailableEventArgs e)
        {
            IHTMLDocument3 document = (IHTMLDocument3)e.Document;
            IHTMLElement mapElement = document.getElementById("map");
            if (mapElement == null || mapElement.getAttribute("loaded", 2) == System.DBNull.Value)
                e.DocumentReady = false;
            else
                e.DocumentReady = true;
        }

        private static string LocalMapPreviewUrl
        {
            get
            {
                string previewHtmlFile = Path.Combine(ApplicationEnvironment.InstallationDirectory, @"html\map-preview.html");
                return UrlHelper.CreateUrlFromPath(MapUrlHelper.FixedUpMapHtml(previewHtmlFile));
            }
        }

        #endregion

        #region IWriterPluginOptionsEditor Members

        /// <summary>
        /// Edit options: note, this is not currently supported because live.com can't take
        /// our custom pushpin URL. To re-enabled just add the HasEditableOptions property
        /// to the WriterPlugin attribute
        /// </summary>
        /// <param name="dialogOwner"></param>
        public override void EditOptions(IWin32Window dialogOwner)
        {
            using (MapOptionsDialog mapOptions = new MapOptionsDialog(_pluginOptions))
                mapOptions.ShowDialog(dialogOwner);
        }

        #endregion
    }
}
