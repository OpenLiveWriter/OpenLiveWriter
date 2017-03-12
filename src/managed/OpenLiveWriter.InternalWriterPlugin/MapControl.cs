// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.BrowserControl;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.InternalWriterPlugin
{
    /// <summary>
    /// Summary description for MapControl.
    /// </summary>
    public class MapControl : BorderControl
    {
        public static int MIN_ZOOM = 1;
        public static int MAX_ZOOM = 19;

        private ExplorerBrowserControl explorerBrowserControl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private MapActiveObject _activeObject;
        private string _address;

        public MapControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            explorerBrowserControl.DocumentComplete += new BrowserDocumentEventHandler(explorerBrowserControl_DocumentComplete);

            this.RightToLeft = RightToLeft.No;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (explorerBrowserControl != null)
                {
                    explorerBrowserControl.DocumentComplete -= new BrowserDocumentEventHandler(explorerBrowserControl_DocumentComplete);
                    explorerBrowserControl.Dispose();
                    explorerBrowserControl = null;
                }
                if (MapActiveObject != null)
                {
                    MapActiveObject = null;
                }
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.explorerBrowserControl = new OpenLiveWriter.BrowserControl.ExplorerBrowserControl();
            this.SuspendLayout();
            //
            // explorerBrowserControl
            //
            this.explorerBrowserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.explorerBrowserControl.Location = new System.Drawing.Point(1, 1);
            this.explorerBrowserControl.Name = "explorerBrowserControl";
            this.explorerBrowserControl.Silent = true;
            this.explorerBrowserControl.Size = new System.Drawing.Size(326, 278);
            this.explorerBrowserControl.TabIndex = 0;
            this.explorerBrowserControl.TextSize = OpenLiveWriter.BrowserControl.TextSize.Medium;
            this.explorerBrowserControl.WorkOffline = false;
            //
            // MapControl
            //
            this.Controls.Add(this.explorerBrowserControl);
            this.DockPadding.All = 1;
            this.Name = "MapControl";
            this.Size = new System.Drawing.Size(328, 280);
            this.Controls.SetChildIndex(this.explorerBrowserControl, 0);
            this.ResumeLayout(false);

        }
        #endregion

        #region Events

        public event EventHandler MapStyleChanged;
        protected virtual void OnMapStyleChanged(EventArgs e)
        {
            if (MapStyleChanged != null)
                MapStyleChanged(this, e);
        }

        public event EventHandler ZoomLevelChanged;
        protected virtual void OnZoomLevelChanged(EventArgs e)
        {
            if (ZoomLevelChanged != null)
                ZoomLevelChanged(this, e);
        }

        public delegate void PushpinEventHandler(VEPushpin pushpin);
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

        public event EventHandler BirdseyeChanged;
        protected virtual void OnBirdseyeChanged(EventArgs e)
        {
            if (BirdseyeChanged != null)
                BirdseyeChanged(this, e);
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
        #endregion

        private void explorerBrowserControl_DocumentComplete(object sender, BrowserDocumentEventArgs e)
        {
            IHTMLDocument2 document = (IHTMLDocument2)explorerBrowserControl.Document;

            // turn off borders
            (document.body as IHTMLElement).style.borderStyle = "none";

            //MapActiveObject.ClearEvents();
            if (_address != null)
            {
                MapActiveObject.FindLocation(_address);
                _address = null;
            }

            MapActiveObject.AttachToMapDocument(document);
        }

        /// <summary>
        /// Initializes the map to a center on a set of coordinates.
        /// </summary>
        /// <param name="address"></param>
        public void LoadMap(string address)
        {
            MapActiveObject.FindLocation(address);
        }

        /// <summary>
        /// Initializes the map to a center on a set of coordinates.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="mapStyle"></param>
        /// <param name="zoomLevel"></param>
        public void LoadMap(float latitude, float longitude, string reserved, string mapStyle, int zoomLevel)
        {
            LoadMap(latitude, longitude, reserved, mapStyle, zoomLevel, null);
        }

        /// <summary>
        /// Initializes the map to a center on a set of coordinates.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="mapStyle"></param>
        /// <param name="zoomLevel"></param>
        public void LoadMap(float latitude, float longitude, string reserved, string mapStyle, int zoomLevel, VEBirdseyeScene scene)
        {
            MapActiveObject = new MapActiveObject(new VELatLong(latitude, longitude, reserved), mapStyle, zoomLevel, scene);

            //string url = MapUrlHelper.CreateMapUrl(LocalMapUrl, latitude, longitude, mapStyle, zoomLevel);
            string url = LocalMapUrl;
            url = new Uri(url).ToString();
            explorerBrowserControl.Navigate(url);
        }

        public void PanMap(int deltaX, int deltaY)
        {
            _activeObject.PanMap(deltaX, deltaY);
        }

        public void AddPushpin(VEPushpin pushpin)
        {
            _activeObject.AddPushpin(pushpin);
        }

        public VEPushpin GetPushpin(string pushpinId)
        {
            return _activeObject.GetPushpin(pushpinId);
        }

        public void UpdatePushpin(VEPushpin pushpin)
        {
            _activeObject.UpdatePushpin(pushpin);
        }

        public void DeletePushpin(string pushpinId)
        {
            _activeObject.DeletePushpin(pushpinId);
        }

        public void DeleteAllPushpins()
        {
            _activeObject.DeleteAllPushpins();
        }

        public void ZoomToLocation(Point p, int zoomLevel)
        {
            _activeObject.ZoomToLocation(p, zoomLevel);
        }

        public float Latitude
        {
            get
            {
                return MapActiveObject.GetCenter().Latitude;
            }
        }

        public float Longitude
        {
            get
            {
                return MapActiveObject.GetCenter().Longitude;
            }
        }

        public string Reserved
        {
            get
            {
                return MapActiveObject.GetCenter().Reserved;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Style
        {
            get
            {
                return MapActiveObject.MapStyle;
            }
            set
            {
                if (MapActiveObject != null) MapActiveObject.MapStyle = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ZoomLevel
        {
            get
            {
                return MapActiveObject.ZoomLevel;
            }
            set
            {
                if (MapActiveObject != null) MapActiveObject.ZoomLevel = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VEPushpin[] Pushpins
        {
            get
            {
                return MapActiveObject.GetPushpins();
            }
            set
            {
                if (MapActiveObject != null)
                {
                    MapActiveObject.DeleteAllPushpins();
                    foreach (VEPushpin pushpin in value)
                        MapActiveObject.AddPushpin(pushpin);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool BirdsEyeAvailable
        {
            get { return _activeObject.BirdsEyeAvailable; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VEBirdseyeScene BirdseyeScene
        {
            get { return _activeObject.BirdseyeScene; }
            set { if (_activeObject != null) _activeObject.BirdseyeScene = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VEOrientation BirdseyeOrientation
        {
            get { return _activeObject.BirdseyeOrientation; }
            set { if (_activeObject != null) _activeObject.BirdseyeOrientation = value; }
        }

        private static string LocalMapUrl
        {
            get
            {
                string mapHtmlFile = Path.Combine(ApplicationEnvironment.InstallationDirectory, @"html\map.html");
                return UrlHelper.CreateUrlFromPath(MapUrlHelper.FixedUpMapHtml(mapHtmlFile));
            }
        }

        private MapActiveObject MapActiveObject
        {
            get { return _activeObject; }
            set
            {
                if (_activeObject != value)
                {
                    if (_activeObject != null)
                    {
                        _activeObject.StyleChanged -= new EventHandler(_activeObject_StyleChanged);
                        _activeObject.ZoomLevelChanged -= new EventHandler(_activeObject_ZoomLevelChanged);
                        _activeObject.PushpinAdded -= new OpenLiveWriter.InternalWriterPlugin.PushpinEventHandler(_activeObject_PushpinAdded);
                        _activeObject.PushpinRemoved -= new OpenLiveWriter.InternalWriterPlugin.PushpinEventHandler(_activeObject_PushpinRemoved);
                        _activeObject.BirdseyeChanged -= new EventHandler(_activeObject_BirdseyeChanged);
                        _activeObject.ShowMapContextMenu -= new MapContextMenuHandler(_activeObject_ShowMapContextMenu);
                        _activeObject.ShowPushpinContextMenu -= new MapPushpinContextMenuHandler(_activeObject_ShowPushpinContextMenu);
                    }
                    _activeObject = value;
                    if (_activeObject != null)
                    {
                        _activeObject.StyleChanged += new EventHandler(_activeObject_StyleChanged);
                        _activeObject.ZoomLevelChanged += new EventHandler(_activeObject_ZoomLevelChanged);
                        _activeObject.PushpinAdded += new OpenLiveWriter.InternalWriterPlugin.PushpinEventHandler(_activeObject_PushpinAdded);
                        _activeObject.PushpinRemoved += new OpenLiveWriter.InternalWriterPlugin.PushpinEventHandler(_activeObject_PushpinRemoved);
                        _activeObject.BirdseyeChanged += new EventHandler(_activeObject_BirdseyeChanged);
                        _activeObject.ShowMapContextMenu += new MapContextMenuHandler(_activeObject_ShowMapContextMenu);
                        _activeObject.ShowPushpinContextMenu += new MapPushpinContextMenuHandler(_activeObject_ShowPushpinContextMenu);
                    }
                }
            }
        }

        private void _activeObject_StyleChanged(object sender, EventArgs e)
        {
            OnMapStyleChanged(e);
        }

        private void _activeObject_ZoomLevelChanged(object sender, EventArgs e)
        {
            OnZoomLevelChanged(e);
        }

        private void _activeObject_PushpinAdded(VEPushpin pushpin)
        {
            OnPushpinAdded(pushpin);
        }

        private void _activeObject_PushpinRemoved(VEPushpin pushpin)
        {
            OnPushpinRemoved(pushpin);
        }

        private void _activeObject_BirdseyeChanged(object sender, EventArgs e)
        {
            OnBirdseyeChanged(e);
        }

        private void _activeObject_ShowMapContextMenu(MapContextMenuEvent e)
        {
            OnShowMapContextMenu(e);
        }

        private void _activeObject_ShowPushpinContextMenu(MapContextMenuEvent e, string pushpinId)
        {
            OnShowPushpinContextMenu(e, pushpinId);
        }
    }
}
