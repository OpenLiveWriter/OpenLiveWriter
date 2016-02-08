// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.InternalWriterPlugin
{
    /// <summary>
    /// Bridge for exchanging map data between .NET and a running Map HTMLDocument.
    /// </summary>
    public class MapActiveObject
    {
        private Hashtable _pushpinTable;
        private ArrayList _pushpinList;

        private IJSMapController _jsMapController;

        public MapActiveObject(VELatLong center, string mapStyle, int zoomLevel, VEBirdseyeScene birdseyeScene)
        {
            _jsMapController = new JSMapController(center, mapStyle, zoomLevel, birdseyeScene);
            _pushpinTable = new Hashtable();
            _pushpinList = new ArrayList();
        }

        public void AttachToMapDocument(IHTMLDocument2 document)
        {
            HTMLDocumentHelper.InjectObjectIntoScriptingEnvironment(document, "jsMapController", _jsMapController);
        }

        #region Public Methods
        public void FindLocation(string location)
        {
            OnJSPropertyChanged("location", location);
        }

        public VELatLong GetCenter()
        {
            return _jsMapController.GetCenter();
        }

        public void PanMap(int deltaX, int deltaY)
        {
            OnJSPanMap(deltaX, deltaY);
        }

        public void ZoomToLocation(Point p, int zoomLevel)
        {
            OnJSZoomToPoint(p, zoomLevel);
        }

        #endregion

        #region Public Properties
        public string MapStyle
        {
            get
            {
                return _jsMapController.MapStyle;
            }
            set
            {
                OnJSPropertyChanged("mapStyle", value);
            }
        }

        public VEMapStyle VEMapStyle
        {
            get
            {
                switch (_jsMapController.MapStyle)
                {
                    case "r":
                        return VEMapStyle.Road;
                    case "a":
                        return VEMapStyle.Aerial;
                    case "h":
                        return VEMapStyle.Hybrid;
                    case "o":
                        return VEMapStyle.Birdseye;
                    default:
                        throw new ArgumentException("unsupported map style: " + _jsMapController.MapStyle);
                }
            }
            set
            {
                string mapStyle;
                switch (value)
                {
                    case VEMapStyle.Road:
                        mapStyle = "r";
                        break;
                    case VEMapStyle.Aerial:
                        mapStyle = "a";
                        break;
                    case VEMapStyle.Hybrid:
                        mapStyle = "h";
                        break;
                    case VEMapStyle.Birdseye:
                        mapStyle = "o";
                        break;
                    default:
                        throw new ArgumentException("unsupported VEMapStyle detected: " + value);
                }
                MapStyle = mapStyle;
            }
        }

        public int ZoomLevel
        {
            get
            {
                return _jsMapController.ZoomLevel;
            }
            set
            {
                OnJSPropertyChanged("zoomLevel", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool BirdsEyeAvailable
        {
            get
            {
                return _jsMapController.BirdsEyeAvailable;
            }
        }

        public VEBirdseyeScene BirdseyeScene
        {
            get
            {
                return _jsMapController.BirdseyeScene;
            }
            set
            {
                OnJSPropertyChanged("birdseyeScene", value.SceneId);
            }
        }

        public VEOrientation BirdseyeOrientation
        {
            get { return BirdseyeScene.Orientation; }
            set
            {
                OnJSPropertyChanged("orientation", value.ToString());
            }
        }
        #endregion

        #region Pushpins
        public void AddPushpin(VEPushpin pushpin)
        {
            if (_pushpinTable.ContainsKey(pushpin.PinId))
                UpdatePushpin(pushpin);
            else
            {
                _pushpinTable[pushpin.PinId] = pushpin;
                _pushpinList.Add(pushpin);
                EnqueueEvent(new PushpinEvent(pushpin, PushpinEvent.PushPinAction.Add));
            }
        }

        public VEPushpin GetPushpin(string pushpinId)
        {
            return (VEPushpin)_pushpinTable[pushpinId];
        }

        public void UpdatePushpin(VEPushpin pushpin)
        {
            DeletePushpin(pushpin.PinId);
            AddPushpin(pushpin);
        }

        public void DeletePushpin(string pushpinId)
        {
            VEPushpin pushpin = GetPushpin(pushpinId);
            if (pushpin != null)
            {
                _pushpinTable.Remove(pushpinId);
                _pushpinList.Remove(pushpin);
                EnqueueEvent(new PushpinEvent(pushpin, PushpinEvent.PushPinAction.Delete));
                OnPushpinRemoved(pushpin);
            }
        }

        public void DeleteAllPushpins()
        {
            VEPushpin[] pushpins = GetPushpins();
            _pushpinTable.Clear();
            _pushpinList.Clear();
            EnqueueEvent(new PushpinEvent(null, PushpinEvent.PushPinAction.DeleteAll));

            foreach (VEPushpin pushpin in pushpins)
                OnPushpinRemoved(pushpin);
        }

        public int PushpinCount
        {
            get
            {
                return _pushpinList.Count;
            }
        }

        public VEPushpin[] GetPushpins()
        {
            return (VEPushpin[])_pushpinList.ToArray(typeof(VEPushpin));
        }

        #endregion

        #region JS to .NET Events

        public event EventHandler StyleChanged
        {
            add { _jsMapController.StyleChanged += value; }
            remove { _jsMapController.StyleChanged -= value; }
        }

        public event EventHandler ZoomLevelChanged
        {
            add { _jsMapController.ZoomLevelChanged += value; }
            remove { _jsMapController.ZoomLevelChanged -= value; }
        }

        public event MapPushpinContextMenuHandler ShowPushpinContextMenu
        {
            add { _jsMapController.ShowPushpinContextMenu += value; }
            remove { _jsMapController.ShowPushpinContextMenu -= value; }
        }

        public event MapContextMenuHandler ShowMapContextMenu
        {
            add { _jsMapController.ShowMapContextMenu += value; }
            remove { _jsMapController.ShowMapContextMenu -= value; }
        }

        public event EventHandler BirdseyeChanged
        {
            add { _jsMapController.BirdseyeChanged += value; }
            remove { _jsMapController.BirdseyeChanged -= value; }
        }

        public event PushpinEventHandler PushpinAdded;
        protected virtual void OnPushpinAdded(VEPushpin pushpin)
        {
            if (PushpinAdded != null)
                PushpinAdded(pushpin);
        }

        public event PushpinEventHandler PushpinRemoved;
        protected virtual void OnPushpinRemoved(VEPushpin pushpin)
        {
            if (PushpinRemoved != null)
                PushpinRemoved(pushpin);
        }

        #endregion

        #region .NET to JS Events
        private void EnqueueEvent(IMapEvent e)
        {
            _jsMapController.EnqueueEvent(e);
        }

        private void OnJSPropertyChanged(string propertyName, object newValue)
        {
            MapPropertyEvent e = new MapPropertyEvent(propertyName, newValue);
            EnqueueEvent(e);
        }

        private void OnJSPanMap(int deltaX, int deltaY)
        {
            PanMapEvent e = new PanMapEvent(deltaX, deltaY);
            EnqueueEvent(e);
        }

        private void OnJSZoomToPoint(Point p, int zoomLevel)
        {
            MapPixelZoomEvent e = new MapPixelZoomEvent(p, zoomLevel);
            EnqueueEvent(e);
        }
        #endregion
    }
}
