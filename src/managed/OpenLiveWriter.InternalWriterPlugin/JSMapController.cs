// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.InternalWriterPlugin
{
    public interface IJSMapController
    {
        void EnqueueEvent(IMapEvent e);

        string MapStyle { get; }
        int ZoomLevel { get; }
        bool BirdsEyeAvailable { get; }
        VEBirdseyeScene BirdseyeScene { get; }
        VELatLong GetCenter();

        event EventHandler StyleChanged;
        event EventHandler ZoomLevelChanged;
        event EventHandler BirdseyeChanged;
        event MapContextMenuHandler ShowMapContextMenu;
        event MapPushpinContextMenuHandler ShowPushpinContextMenu;
    }

    public delegate void MapPropertyHandler(MapPropertyEvent e);
    public delegate void PushpinEventHandler(VEPushpin pushpin);
    public delegate void MapContextMenuHandler(MapContextMenuEvent e);
    public delegate void MapPushpinContextMenuHandler(MapContextMenuEvent e, string pushpinId);

    /// <summary>
    /// Controller object that is injected into the running Map HTMLDocument and is used to exchange
    /// data between the .NET and the HTMLDocument runtime.
    /// </summary>
    [ComVisible(true)]
    public class JSMapController : IJSMapController
    {
        private Queue _events; //list of events to be processed by the JavaScript runtime
        private VELatLong _center;

        internal JSMapController(VELatLong center, string mapStyle, int zoomLevel, VEBirdseyeScene scene)
        {
            _events = new Queue();
            _center = center;
            _birdseyeScene = scene;
            _mapStyle = mapStyle;
            _zoomLevel = zoomLevel;
        }

        void IJSMapController.EnqueueEvent(IMapEvent e)
        {
            _events.Enqueue(e);
        }

        #region Internal callbacks used by JS Runtime
        public IMapEvent NextEvent()
        {
            if (_events.Count > 0)
            {
                IMapEvent e = (IMapEvent)_events.Dequeue();
                return e;
            }
            else
                return null;
        }

        public void ShowContextMenu(int x, int y, float latitude, float longitude, string reserved, string pushpinId)
        {
            if (reserved == String.Empty)
                reserved = null;

            if (pushpinId == "")
            {
                OnShowMapContextMenu(new MapContextMenuEvent(x, y, latitude, longitude, reserved));
            }
            else
            {
                OnShowPushpinContextMenu(new MapContextMenuEvent(x, y, latitude, longitude, reserved), pushpinId);
            }
        }

        public void JSUpdateBirdsEye(string sceneId,
            string direction,
            string sceneThumbUrl
            )
        {
            BirdseyeScene = new VEBirdseyeScene(sceneId, direction);
            OnBirdseyeChanged(EventArgs.Empty);
        }

        public void SetCenter(float latitude, float longitude, String reserved)
        {
            if (reserved == String.Empty)
                reserved = null;
            _center = new VELatLong(latitude, longitude, reserved);
        }
        #endregion

        #region JS Runtime Properties/Accessors
        public VELatLong GetCenter()
        {
            return _center;
        }

        public bool BirdsEyeAvailable
        {
            get
            {
                return _birdsEyeAvailable;
            }
            set
            {
                if (_birdsEyeAvailable != value)
                {
                    _birdsEyeAvailable = value;
                    OnBirdseyeChanged(EventArgs.Empty);
                }
            }
        }
        private bool _birdsEyeAvailable;

        public VEBirdseyeScene BirdseyeScene
        {
            get
            {
                return _birdseyeScene;
            }
            set
            {
                if (_birdseyeScene == null || value == null || _birdseyeScene.SceneId != value.SceneId)
                {
                    _birdseyeScene = value;
                    OnBirdseyeChanged(EventArgs.Empty);
                }
            }
        }
        private VEBirdseyeScene _birdseyeScene;

        public string MapStyle
        {
            get
            {
                return _mapStyle;
            }
            set
            {
                if (_mapStyle != value)
                {
                    _mapStyle = value;
                    OnStyleChanged(EventArgs.Empty);
                }
            }
        }
        private string _mapStyle;

        public int ZoomLevel
        {
            get
            {
                return _zoomLevel;
            }
            set
            {
                if (_zoomLevel != value)
                {
                    _zoomLevel = value;
                    OnZoomLevelChanged(EventArgs.Empty);
                }
            }
        }
        private int _zoomLevel;
        #endregion

        #region JS Runtime Events
        public event EventHandler StyleChanged;
        protected virtual void OnStyleChanged(EventArgs e)
        {
            if (StyleChanged != null)
                StyleChanged(this, e);
        }

        public event EventHandler ZoomLevelChanged;
        protected virtual void OnZoomLevelChanged(EventArgs e)
        {
            if (ZoomLevelChanged != null)
                ZoomLevelChanged(this, e);
        }

        public event MapContextMenuHandler ShowMapContextMenu;
        protected virtual void OnShowMapContextMenu(MapContextMenuEvent e)
        {
            if (ShowMapContextMenu != null)
                ShowMapContextMenu(e);
        }

        public event MapPushpinContextMenuHandler ShowPushpinContextMenu;
        protected virtual void OnShowPushpinContextMenu(MapContextMenuEvent e, string pushpinId)
        {
            if (ShowPushpinContextMenu != null)
                ShowPushpinContextMenu(e, pushpinId);
        }

        public event EventHandler BirdseyeChanged;
        protected virtual void OnBirdseyeChanged(EventArgs e)
        {
            if (BirdseyeChanged != null)
                BirdseyeChanged(this, e);
        }
        #endregion
    }

    [ComVisible(true)]
    public class VEBirdseyeScene
    {
        public VEBirdseyeScene()
        {

        }
        public VEBirdseyeScene(string sceneId, string orientation)
        {
            _sceneId = sceneId;
            _orientation = (VEOrientation)VEOrientation.Parse(typeof(VEOrientation), orientation);
        }
        public VEBirdseyeScene(string sceneId, VEOrientation orientation)
        {
            _sceneId = sceneId;
            _orientation = orientation;
        }
        private string _sceneId;
        public string SceneId
        {
            get { return _sceneId; }
            set { _sceneId = value; }
        }

        public VEOrientation Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }
        private VEOrientation _orientation;
    }

    public class VEBirdseyeSceneThumbnail
    {
        private string _sceneId;
        private BirdseyeSceneAdjacency _adjacency;
        private string _url;
        public VEBirdseyeSceneThumbnail(string sceneId, BirdseyeSceneAdjacency adjacency, string url)
        {
            _sceneId = sceneId;
            _adjacency = adjacency;
            _url = url;
        }

        public string SceneId
        {
            get { return _sceneId; }
        }

        public BirdseyeSceneAdjacency Adjacency
        {
            get { return _adjacency; }
        }

        public string Url
        {
            get { return _url; }
        }
    }

    public enum BirdseyeSceneAdjacency { SameArea = -1, North = 0, NorthEast = 1, East = 2, SouthEast = 3, South = 4, SouthWest = 5, West = 6, NorthWest = 7 };
    public enum VEMapStyle { Road, Aerial, Hybrid, Birdseye };

    [ComVisible(true)]
    public class VELatLong
    {
        private float _latitude;
        private float _longitude;
        private string _reserved;

        public VELatLong(float latitude, float longitude, String reserved)
        {
            _latitude = latitude;
            _longitude = longitude;
            _reserved = reserved;
        }

        public float Latitude
        {
            get { return _latitude; }
        }

        public float Longitude
        {
            get { return _longitude; }
        }

        public String Reserved
        {
            get { return _reserved; }
        }

        public override bool Equals(object obj)
        {
            VELatLong veLatLong = obj as VELatLong;
            return veLatLong != null && veLatLong.Latitude == Latitude && veLatLong.Longitude == Longitude && veLatLong.Reserved == Reserved;
        }

        public override int GetHashCode()
        {
            if (Reserved != null)
                return Reserved.GetHashCode();
            else
                return Latitude.GetHashCode() | Longitude.GetHashCode();
        }
    }

    [ComVisible(true)]
    public interface IMapEvent
    {
        int Type { get; }
    }

    public enum MapEventTypes { PropertyChanged = 1, PushPinAction = 2, PanMap = 3, ZoomToPixel = 4 };

    public enum VEOrientation
    {
        North,
        South,
        East,
        West
    }

    [ComVisible(true)]
    public class MapPropertyEvent : IMapEvent
    {
        public MapPropertyEvent(string name, object val)
        {
            _name = name;
            _value = val;
        }
        private string _name;
        private object _value;
        public string Name
        {
            get { return _name; }
        }

        public object Value
        {
            get { return _value; }
        }

        public int Type
        {
            get { return (int)MapEventTypes.PropertyChanged; }
        }
    }

    [ComVisible(true)]
    public class VEPushpin
    {
        private string _pinId;
        private VELatLong _veLatLong;
        private string _imageFile;
        private string _title;
        private string _details;
        private string _photoUrl;
        private string _moreInfoUrl;

        public VEPushpin(string pinId, VELatLong veLatLong, string imageFile, string title, string details, string moreInfoUrl, string photoUrl)
        {
            _pinId = pinId;
            _veLatLong = veLatLong;
            _imageFile = imageFile;
            _title = title;
            _details = details;
            _moreInfoUrl = moreInfoUrl;
            _photoUrl = photoUrl;
        }

        public string PinId
        {
            get { return _pinId; }
        }

        public VELatLong VELatLong
        {
            get { return _veLatLong; }
        }

        public string ImageFile
        {
            get { return _imageFile; }
        }

        public string Title
        {
            get { return _title; }
        }

        public string Details
        {
            get { return _details; }
        }

        public string MoreInfoUrl
        {
            get { return _moreInfoUrl; }
        }

        public string PhotoUrl
        {
            get { return _photoUrl; }
        }
    }

    [ComVisible(true)]
    public class PushpinEvent : IMapEvent
    {
        VEPushpin _pushpin;
        internal enum PushPinAction { Add = 1, Delete = 2, DeleteAll = 3 };
        PushPinAction _action;
        internal PushpinEvent(VEPushpin pushpin, PushPinAction action)
        {
            _pushpin = pushpin;
            _action = action;
        }

        public VEPushpin Pushpin
        {
            get { return _pushpin; }
        }

        public int Action
        {
            get
            {
                return (int)_action;
            }
        }

        public int Type
        {
            get { return (int)MapEventTypes.PushPinAction; }
        }
    }

    [ComVisible(true)]
    public class PanMapEvent : IMapEvent
    {
        private int _deltaX;
        private int _deltaY;
        public PanMapEvent(int deltaX, int deltaY)
        {
            _deltaX = deltaX;
            _deltaY = deltaY;
        }

        public int DeltaX
        {
            get { return _deltaX; }
        }

        public int DeltaY
        {
            get { return _deltaY; }
        }

        public int Type
        {
            get { return (int)MapEventTypes.PanMap; }
        }
    }

    [ComVisible(true)]
    public class MapPixelZoomEvent : IMapEvent
    {
        private Point _location;
        private int _zoomLevel;
        public MapPixelZoomEvent(Point location, int zoomLevel)
        {
            _location = location;
            _zoomLevel = zoomLevel;
        }

        public int X
        {
            get { return _location.X; }
        }

        public int Y
        {
            get { return _location.Y; }
        }

        public int ZoomLevel
        {
            get { return _zoomLevel; }
        }

        public int Type
        {
            get { return (int)MapEventTypes.ZoomToPixel; }
        }
    }

    public class MapContextMenuEvent : EventArgs
    {
        int _x;
        int _y;
        float _latitude;
        float _longitude;
        string _reserved;
        internal MapContextMenuEvent(int x, int y, float latitude, float longitude, string reserved)
        {
            _x = x;
            _y = y;
            _latitude = latitude;
            _longitude = longitude;
            _reserved = reserved;
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public float Latitude
        {
            get { return _latitude; }
        }

        public float Longitude
        {
            get { return _longitude; }
        }

        public string Reserved
        {
            get { return _reserved; }
        }
    }
}
