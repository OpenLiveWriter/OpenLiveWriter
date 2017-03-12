// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.InternalWriterPlugin
{
    /// <summary>
    /// Summary description for MapSettings.
    /// </summary>
    public class MapSettings
    {
        private const string IMAGE_FILE_ID = "map.imageFileId";
        //private const string LIVE_MAP_URL = "map.liveUrl";
        private const string MAP_WIDTH = "map.width";
        private const string MAP_HEIGHT = "map.height";
        private const string MAP_ZOOMLEVEL = "map.zoomLevel";
        private const string MAP_STYLE = "map.style";
        private const string MAP_ID = "map.id";
        private const string MAP_LATITUDE = "map.latitude";
        private const string MAP_LONGITUDE = "map.longitude";
        private const string MAP_RESERVED = "map.latlongReserved";
        private const string MAP_CAPTION = "map.caption";
        private const string PUBLISH_TARGET = "publish.targetId";
        private const string MAP_IMAGE_INVALIDATED = "map.invalidated";

        private IProperties _settings;
        public MapSettings(IProperties settings)
        {
            _settings = settings;
        }

        public void UpdateSettings(float latitude, float longitude, string reserved, int zoomLevel, string style, VEPushpin[] pushpins, VEBirdseyeScene birdseyeScene)
        {
            Latitude = latitude;
            Longitude = longitude;
            Reserved = reserved;
            ZoomLevel = zoomLevel;
            MapStyle = style;
            Pushpins = pushpins;
            if (birdseyeScene != null)
            {
                BirdseyeSceneId = birdseyeScene.SceneId;
                BirdseyeOrientation = birdseyeScene.Orientation.ToString();
            }
            else
            {
                BirdseyeSceneId = null;
                BirdseyeOrientation = null;
            }
            MapImageInvalidated = true;
        }

        public string ImageFileId
        {
            get { return _settings.GetString(IMAGE_FILE_ID, "map.jpg"); }
            set
            {
                _settings.SetString(IMAGE_FILE_ID, value);
            }
        }

        public string MapId
        {
            get
            {
                string mapId = _settings.GetString(MAP_ID, null);
                if (mapId == null)
                {
                    mapId = Guid.NewGuid().ToString();
                    _settings.SetString(MAP_ID, mapId);
                }
                return mapId;
            }
            set
            {
                _settings.SetString(MAP_ID, value);
            }
        }

        public bool MapImageInvalidated
        {
            get
            {
                return _settings.GetBoolean(MAP_IMAGE_INVALIDATED, true);
            }
            set
            {
                _settings.GetBoolean(MAP_IMAGE_INVALIDATED, value);
            }
        }

        public string LiveMapUrl
        {
            get
            {
                string sceneId = BirdseyeSceneId;
                VEBirdseyeScene scene = sceneId != null ? new VEBirdseyeScene(sceneId, BirdseyeOrientation) : null;
                return MapUrlHelper.CreateLiveUrl(Latitude, Longitude, Reserved, MapStyle, ZoomLevel, Pushpins, scene);
            }
        }

        public Size Size
        {
            get
            {
                int width = _settings.GetInt(MAP_WIDTH, 0);
                int height = _settings.GetInt(MAP_HEIGHT, 0);
                if (width != 0 || height != 0)
                    return EnsurePositiveSize(new Size(width, height), MapOptions.DefaultMapSize);
                else
                    return MapOptions.DefaultMapSize;
            }
            set
            {
                _settings.SetInt(MAP_WIDTH, value.Width);
                _settings.SetInt(MAP_HEIGHT, value.Height);
            }
        }

        public int ZoomLevel
        {
            get
            {
                return _settings.GetInt(MAP_ZOOMLEVEL, 4);
            }
            set
            {
                _settings.SetInt(MAP_ZOOMLEVEL, value);
            }
        }

        public float Latitude
        {
            get
            {
                return _settings.GetFloat(MAP_LATITUDE, 39.4022446f);
            }
            set
            {
                _settings.SetFloat(MAP_LATITUDE, value);
            }
        }

        public float Longitude
        {
            get
            {
                return _settings.GetFloat(MAP_LONGITUDE, -97.734375f);
            }
            set
            {
                _settings.SetFloat(MAP_LONGITUDE, value);
            }
        }

        public string Reserved
        {
            get
            {
                return _settings.GetString(MAP_RESERVED, null);
            }
            set
            {
                _settings.SetString(MAP_RESERVED, value);
            }
        }

        public string MapStyle
        {
            get
            {
                return _settings.GetString(MAP_STYLE, "r");
            }
            set
            {
                _settings.SetString(MAP_STYLE, value);
            }
        }

        public string Caption
        {
            get
            {
                return _settings.GetString(MAP_CAPTION, String.Empty);
            }
            set
            {
                _settings.SetString(MAP_CAPTION, value);
            }
        }

        public string PublishTargetId
        {
            get
            {
                return _settings.GetString(PUBLISH_TARGET, String.Empty);
            }
            set
            {
                _settings.SetString(PUBLISH_TARGET, value);
            }
        }

        public VEPushpin[] Pushpins
        {
            get
            {
                IProperties pushpinSettings = _settings.GetSubProperties(PUSHPINS);
                string[] pinIds = pushpinSettings.SubPropertyNames;
                VEPushpin[] pushpins = new VEPushpin[pinIds.Length];
                for (int i = 0; i < pinIds.Length; i++)
                {
                    string pinId = pinIds[i];
                    IProperties pinProps = pushpinSettings.GetSubProperties(pinId);
                    float latitude = pinProps.GetFloat(PUSHPIN_LATITUDE, 0);
                    float longitude = pinProps.GetFloat(PUSHPIN_LONGITUDE, 0);
                    string reserved = pinProps.GetString(PUSHPIN_RESERVED, null);
                    string title = pinProps.GetString(PUSHPIN_TITLE, "");
                    string details = pinProps.GetString(PUSHPIN_DETAILS, "");
                    string imageUrl = pinProps.GetString(PUSHPIN_IMAGE_URL, "");
                    string moreInfoUrl = pinProps.GetString(PUSHPIN_MORE_INFO_URL, "");
                    string photoUrl = pinProps.GetString(PUSHPIN_PHOTO_URL, "");
                    pushpins[i] = new VEPushpin(pinId, new VELatLong(latitude, longitude, reserved), imageUrl, title, details, moreInfoUrl, photoUrl);
                }
                return pushpins;
            }

            set
            {
                _settings.RemoveSubProperties(PUSHPINS);
                foreach (VEPushpin pushpin in value)
                {
                    AddPushpinSettings(pushpin);
                }
            }
        }

        public string BirdseyeSceneId
        {
            get
            {
                return _settings.GetString(BIRDSEYE_SCENE_ID, null);
            }
            set
            {
                _settings.SetString(BIRDSEYE_SCENE_ID, value);
            }
        }

        public string BirdseyeOrientation
        {
            get
            {
                return _settings.GetString(BIRDSEYE_ORIENTATION, VEOrientation.North.ToString());
            }
            set
            {
                _settings.SetString(BIRDSEYE_ORIENTATION, value);
            }
        }

        private void AddPushpinSettings(VEPushpin pushpin)
        {
            IProperties pushpinSettings = _settings.GetSubProperties(PUSHPINS).GetSubProperties(pushpin.PinId);
            pushpinSettings.SetFloat(PUSHPIN_LATITUDE, pushpin.VELatLong.Latitude);
            pushpinSettings.SetFloat(PUSHPIN_LONGITUDE, pushpin.VELatLong.Longitude);
            pushpinSettings.SetString(PUSHPIN_TITLE, pushpin.Title);
            pushpinSettings.SetString(PUSHPIN_IMAGE_URL, pushpin.ImageFile);
            pushpinSettings.SetString(PUSHPIN_DETAILS, pushpin.Details);
            pushpinSettings.SetString(PUSHPIN_MORE_INFO_URL, pushpin.MoreInfoUrl);
            pushpinSettings.SetString(PUSHPIN_PHOTO_URL, pushpin.PhotoUrl);
        }

        /// <summary>
        /// Converts the specified size into a default size if any of its dimensions are less than or equal to zero.
        /// </summary>
        /// <returns></returns>
        internal static Size EnsurePositiveSize(Size size, Size defaultSize)
        {
            if (size.Width <= 0 || size.Height <= 0)
                return defaultSize;
            return size;
        }

        private const string BIRDSEYE_SCENE_ID = "birdseyeSceneId";
        private const string BIRDSEYE_ORIENTATION = "birdseyeOrientation";
        private const string PUSHPINS = "pushpins";
        private const string PUSHPIN_LATITUDE = "latitude";
        private const string PUSHPIN_LONGITUDE = "longitude";
        private const string PUSHPIN_RESERVED = "latlongReserved";
        private const string PUSHPIN_TITLE = "title";
        private const string PUSHPIN_IMAGE_URL = "imageUrl";
        private const string PUSHPIN_DETAILS = "details";
        private const string PUSHPIN_MORE_INFO_URL = "moreInfo";
        private const string PUSHPIN_PHOTO_URL = "photoUrl";
    }
}
