// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.InternalWriterPlugin.Controls;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.InternalWriterPlugin
{
    /// <summary>
    /// Summary description for MapForm.
    /// </summary>
    public class MapForm : ApplicationDialog, IMiniFormOwner
    {
        private Panel panel1;
        private MapControl mapControl;
        private Button buttonOK;
        private Button buttonCancel;
        private TextBox textBoxAddress;
        private Label label1;
        private TrackBar trackBarZoom;
        private MapViewComboBox comboBoxStyle;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private MapBirdsEyeButton buttonGotoBirdseye;
        private MapSearchButton buttonSearch;
        private CheckBox cbShowLabels;
        private Panel panelNormal;
        private Panel panelBirdseye;
        private MapBirdsEyeZoomControl mapBirdsEyeZoomControl;
        private MapBirdsEyeDirectionControl mapBirdsEyeDirectionControl;
        private Label mapTipControl;
        private MapScrollControl mapScrollControl;
        private MapLogoControl mapLogoControl;
        private MapZoomPlusButton mapZoomPlusButton;

        private bool _suspendZoom = false;
        private const int PAN_OFFSET = 50;
        private MapZoomMinusButton buttonZoomMinus;

        private MapOptions _mapOptions;

        internal static bool ValidateLiveLocalConnection(bool showUI)
        {
            bool connected = PluginHttpRequest.InternetConnectionAvailable;

            if (!connected && showUI)
            {
                DisplayMessage.Show(MessageId.MapsNoInternetConnection);
            }

            return connected;
        }

        private readonly CommandManager CommandManager;
        internal MapForm(bool insertNewMap, MapOptions mapOptions, CommandManager commandManager)
        {
            CommandManager = commandManager;
            _mapOptions = mapOptions;

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //set accessibility names
            comboBoxStyle.Initialize(VEMapStyle.Road);
            comboBoxStyle.AccessibleName = Res.Get(StringId.MapStyle);
            mapZoomPlusButton.AccessibleName = Res.Get(StringId.MapZoomIn);
            buttonZoomMinus.AccessibleName = Res.Get(StringId.MapZoomOut);
            trackBarZoom.AccessibleName = Res.Get(StringId.MapZoomSlider);
            buttonSearch.AccessibleName = Res.Get(StringId.MapSearch);
            buttonGotoBirdseye.AccessibleName = Res.Get(StringId.MapBirdseyeButton);

            label1.Text = Res.Get(StringId.MapFindAddress);
            buttonCancel.Text = Res.Get(StringId.CancelButton);
            buttonGotoBirdseye.Text = Res.Get(StringId.MapBirdseyeLabel);
            cbShowLabels.Text = Res.Get(StringId.MapShowLabel);
            mapTipControl.Text = Res.Get(StringId.MapPushpinTip);

            if (insertNewMap)
            {
                Text = Res.Get(StringId.InsertMap);
                buttonOK.Text = Res.Get(StringId.InsertButton);
            }
            else
            {
                Text = Res.Get(StringId.CustomizeMap);
                buttonOK.Text = Res.Get(StringId.OKButtonText);
            }

            if (!DesignMode)
                Icon = ApplicationEnvironment.ProductIcon;

            comboBoxStyle.SelectedIndexChanged += new EventHandler(comboBoxStyle_SelectedIndexChanged);
            mapControl.MapStyleChanged += new EventHandler(mapControl_MapStyleChanged);
            mapControl.ZoomLevelChanged += new EventHandler(mapControl_ZoomLevelChanged);
            mapControl.BirdseyeChanged += new EventHandler(mapControl_BirdseyeChanged);
            mapControl.ShowMapContextMenu += new MapContextMenuHandler(mapControl_ShowMapContextMenu);
            mapControl.ShowPushpinContextMenu += new MapPushpinContextMenuHandler(mapControl_ShowPushpinContextMenu);

            mapScrollControl.DirectionalButtonClicked +=
                new DirectionalButtonClickedHandler(mapScrollControl_DirectionalButtonClicked);
            mapBirdsEyeZoomControl.ZoomLevelChanged += new EventHandler(mapBirdsEyeZoomControl_ZoomLevelChanged);
            mapBirdsEyeDirectionControl.DirectionChanged += new EventHandler(mapBirdsEyeDirectionControl_DirectionChanged);

            trackBarZoom.Maximum = MapControl.MAX_ZOOM;
            trackBarZoom.Minimum = MapControl.MIN_ZOOM;

            trackBarZoom.AutoSize = false;
            trackBarZoom.Height = 30;
            cbShowLabels.Top = trackBarZoom.Bottom;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Size = _mapOptions.DefaultDialogSize;

            using (LayoutHelper.SuspendAnchoring(mapControl, mapTipControl, buttonOK, buttonCancel))
            {
                LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
                mapTipControl.Width = buttonOK.Left - mapTipControl.Left - (int)DisplayHelper.ScaleX(8f);
                LayoutHelper.NaturalizeHeight(mapTipControl);

                int oldTop = buttonCancel.Top;
                if (mapTipControl.Bottom > buttonOK.Bottom)
                {
                    buttonOK.Top = buttonCancel.Top = mapTipControl.Bottom - buttonOK.Height;

                }

                Height += Math.Max(0, buttonCancel.Top - oldTop);
            }

            bool visible = cbShowLabels.Visible;
            cbShowLabels.Visible = true;
            cbShowLabels.Width = mapControl.Left - cbShowLabels.Left - 3;
            LayoutHelper.NaturalizeHeight(cbShowLabels);
            cbShowLabels.Visible = visible;

            buttonGotoBirdseye.Top = Math.Max(cbShowLabels.Bottom, mapBirdsEyeDirectionControl.Bottom) + 12;

            textBoxAddress.Focus();

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (mapControl != null)
                {
                    mapControl.MapStyleChanged -= new EventHandler(mapControl_MapStyleChanged);
                    mapControl.ZoomLevelChanged -= new EventHandler(mapControl_ZoomLevelChanged);
                    mapControl.BirdseyeChanged -= new EventHandler(mapControl_BirdseyeChanged);
                    mapControl.ShowMapContextMenu -= new MapContextMenuHandler(mapControl_ShowMapContextMenu);
                    mapControl.ShowPushpinContextMenu -= new MapPushpinContextMenuHandler(mapControl_ShowPushpinContextMenu);
                    mapControl = null;
                }
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MapForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonSearch = new OpenLiveWriter.InternalWriterPlugin.Controls.MapSearchButton();
            this.mapLogoControl = new OpenLiveWriter.InternalWriterPlugin.Controls.MapLogoControl();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxAddress = new System.Windows.Forms.TextBox();
            this.mapControl = new OpenLiveWriter.InternalWriterPlugin.MapControl();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonGotoBirdseye = new OpenLiveWriter.InternalWriterPlugin.Controls.MapBirdsEyeButton();
            this.trackBarZoom = new System.Windows.Forms.TrackBar();
            this.comboBoxStyle = new OpenLiveWriter.InternalWriterPlugin.Controls.MapViewComboBox();
            this.mapScrollControl = new OpenLiveWriter.InternalWriterPlugin.Controls.MapScrollControl();
            this.cbShowLabels = new System.Windows.Forms.CheckBox();
            this.panelNormal = new System.Windows.Forms.Panel();
            this.mapZoomPlusButton = new OpenLiveWriter.InternalWriterPlugin.Controls.MapZoomPlusButton();
            this.buttonZoomMinus = new OpenLiveWriter.InternalWriterPlugin.Controls.MapZoomMinusButton();
            this.panelBirdseye = new System.Windows.Forms.Panel();
            this.mapBirdsEyeDirectionControl = new OpenLiveWriter.InternalWriterPlugin.Controls.MapBirdsEyeDirectionControl();
            this.mapBirdsEyeZoomControl = new OpenLiveWriter.InternalWriterPlugin.Controls.MapBirdsEyeZoomControl();
            this.mapTipControl = new Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).BeginInit();
            this.panelNormal.SuspendLayout();
            this.panelBirdseye.SuspendLayout();
            this.SuspendLayout();
            //
            // panel1
            //
            this.panel1.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                   | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.buttonSearch);
            this.panel1.Controls.Add(this.mapLogoControl);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.textBoxAddress);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(642, 52);
            this.panel1.TabIndex = 0;
            //
            // buttonSearch
            //
            this.buttonSearch.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearch.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSearch.Location = new System.Drawing.Point(584, 27);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(24, 24);
            this.buttonSearch.TabIndex = 2;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            //
            // mapLogoControl
            //
            this.mapLogoControl.Location = new System.Drawing.Point(0, 2);
            this.mapLogoControl.Name = "mapLogoControl";
            this.mapLogoControl.Size = new System.Drawing.Size(184, 51);
            this.mapLogoControl.TabIndex = 3;
            this.mapLogoControl.TabStop = false;
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(188, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(203, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Find &address:";
            //
            // textBoxAddress
            //
            this.textBoxAddress.AcceptsReturn = true;
            this.textBoxAddress.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                   | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAddress.Location = new System.Drawing.Point(188, 28);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(390, 21);
            this.textBoxAddress.TabIndex = 1;
            this.textBoxAddress.Text = "";
            this.textBoxAddress.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxAddress_KeyDown);
            //
            // mapControl
            //
            this.mapControl.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                   | System.Windows.Forms.AnchorStyles.Right)));
            this.mapControl.AutoHeight = false;
            this.mapControl.BottomInset = 0;
            this.mapControl.Control = null;
            this.mapControl.DockPadding.All = 1;
            this.mapControl.LeftInset = 0;
            this.mapControl.Location = new System.Drawing.Point(188, 56);
            this.mapControl.Name = "mapControl";
            this.mapControl.RightInset = 0;
            this.mapControl.Size = new System.Drawing.Size(420, 378);
            this.mapControl.TabIndex = 1;
            this.mapControl.TabStop = false;
            this.mapControl.ThemeBorder = false;
            this.mapControl.TopInset = 0;
            //
            // buttonOK
            //
            this.buttonOK.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(456, 441);
            this.buttonOK.Name = "buttonInsert";
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&Insert";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(534, 441);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            //
            // buttonGotoBirdseye
            //
            this.buttonGotoBirdseye.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonGotoBirdseye.AutoSizeHeight = true;
            this.buttonGotoBirdseye.AutoSizeWidth = false;
            this.buttonGotoBirdseye.BackColor = System.Drawing.Color.Transparent;

            this.buttonGotoBirdseye.BitmapDisabled = null;
            this.buttonGotoBirdseye.BitmapEnabled =
                ResourceHelper.LoadAssemblyResourceBitmap("Images.GoBirdsEyeEnabled.png", true);
            this.buttonGotoBirdseye.BitmapPushed =
                ResourceHelper.LoadAssemblyResourceBitmap("Images.GoBirdsEyePressed.png", true);
            this.buttonGotoBirdseye.BitmapSelected =
                ResourceHelper.LoadAssemblyResourceBitmap("Images.GoBirdsEyeSelected.png", true);
            this.buttonGotoBirdseye.ButtonStyle = OpenLiveWriter.Controls.ButtonStyle.Bitmap;
            this.buttonGotoBirdseye.Location = new System.Drawing.Point(5, 64);
            this.buttonGotoBirdseye.Name = "buttonGotoBirdseye";
            this.buttonGotoBirdseye.Size = new System.Drawing.Size(170, 60);
            this.buttonGotoBirdseye.TabIndex = 5;
            this.buttonGotoBirdseye.Text = "&Birdseye";
            this.buttonGotoBirdseye.ToolTip = "";
            this.buttonGotoBirdseye.Visible = false;
            this.buttonGotoBirdseye.Click += new System.EventHandler(this.buttonGotoBirdseye_Click);
            //
            // trackBarZoom
            //
            this.trackBarZoom.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.trackBarZoom.LargeChange = 3;
            this.trackBarZoom.Location = new System.Drawing.Point(70, 3);
            this.trackBarZoom.Maximum = 19;
            this.trackBarZoom.Minimum = 1;
            this.trackBarZoom.Name = "trackBarZoom";
            this.trackBarZoom.Size = new System.Drawing.Size(99, 18);
            this.trackBarZoom.TabIndex = 2;
            this.trackBarZoom.TickFrequency = 5;
            this.trackBarZoom.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarZoom.Value = 1;
            this.trackBarZoom.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBarZoom_MouseUp);
            this.trackBarZoom.ValueChanged += new System.EventHandler(this.trackBarZoom_ValueChanged);
            this.trackBarZoom.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trackBarZoom_MouseDown);
            //
            // comboBoxStyle
            //
            this.comboBoxStyle.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStyle.ItemHeight = 21;
            this.comboBoxStyle.Location = new System.Drawing.Point(6, 56);
            this.comboBoxStyle.Name = "comboBoxStyle";
            this.comboBoxStyle.Size = new System.Drawing.Size(175, 27);
            this.comboBoxStyle.TabIndex = 1;
            //
            // mapScrollControl
            //
            this.mapScrollControl.Location = new System.Drawing.Point(0, 0);
            this.mapScrollControl.Name = "mapScrollControl";
            this.mapScrollControl.Size = new System.Drawing.Size(56, 56);
            this.mapScrollControl.TabIndex = 0;
            //
            // cbShowLabels
            //
            this.cbShowLabels.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbShowLabels.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbShowLabels.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cbShowLabels.Location = new System.Drawing.Point(78, 27);
            this.cbShowLabels.Name = "cbShowLabels";
            this.cbShowLabels.Size = new System.Drawing.Size(92, 31);
            this.cbShowLabels.TabIndex = 4;
            this.cbShowLabels.Text = "Show &labels";
            this.cbShowLabels.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbShowLabels.CheckedChanged += new System.EventHandler(this.cbShowLabels_CheckedChanged);
            //
            // panelNormal
            //
            this.panelNormal.Controls.Add(this.mapZoomPlusButton);
            this.panelNormal.Controls.Add(this.buttonZoomMinus);
            this.panelNormal.Controls.Add(this.mapScrollControl);
            this.panelNormal.Controls.Add(this.cbShowLabels);
            this.panelNormal.Controls.Add(this.trackBarZoom);
            this.panelNormal.Controls.Add(this.buttonGotoBirdseye);
            this.panelNormal.Location = new System.Drawing.Point(6, 90);
            this.panelNormal.Name = "panelNormal";
            this.panelNormal.Size = new System.Drawing.Size(176, 132);
            this.panelNormal.TabIndex = 2;
            //
            // mapZoomPlusButton
            //
            this.mapZoomPlusButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.mapZoomPlusButton.AutoSizeHeight = true;
            this.mapZoomPlusButton.AutoSizeWidth = true;
            this.mapZoomPlusButton.BackColor = System.Drawing.Color.Transparent;
            this.mapZoomPlusButton.BitmapDisabled =
                ((System.Drawing.Bitmap)(resources.GetObject("mapZoomPlusButton.BitmapDisabled")));
            this.mapZoomPlusButton.BitmapEnabled =
                ((System.Drawing.Bitmap)(resources.GetObject("mapZoomPlusButton.BitmapEnabled")));
            this.mapZoomPlusButton.BitmapPushed =
                ((System.Drawing.Bitmap)(resources.GetObject("mapZoomPlusButton.BitmapPushed")));
            this.mapZoomPlusButton.BitmapSelected =
                ((System.Drawing.Bitmap)(resources.GetObject("mapZoomPlusButton.BitmapSelected")));
            this.mapZoomPlusButton.ButtonStyle = OpenLiveWriter.Controls.ButtonStyle.Bitmap;
            this.mapZoomPlusButton.Location = new System.Drawing.Point(162, 8);
            this.mapZoomPlusButton.Name = "mapZoomPlusButton";
            this.mapZoomPlusButton.Size = new System.Drawing.Size(12, 12);
            this.mapZoomPlusButton.TabIndex = 3;
            this.mapZoomPlusButton.Text = "button1";
            this.mapZoomPlusButton.ToolTip = "";
            this.mapZoomPlusButton.Click += new System.EventHandler(this.mapZoomPlusButton_Click);
            //
            // buttonZoomMinus
            //
            this.buttonZoomMinus.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonZoomMinus.AutoSizeHeight = true;
            this.buttonZoomMinus.AutoSizeWidth = true;
            this.buttonZoomMinus.BackColor = System.Drawing.Color.Transparent;
            this.buttonZoomMinus.BitmapDisabled =
                ((System.Drawing.Bitmap)(resources.GetObject("buttonZoomMinus.BitmapDisabled")));
            this.buttonZoomMinus.BitmapEnabled = ((System.Drawing.Bitmap)(resources.GetObject("buttonZoomMinus.BitmapEnabled")));
            this.buttonZoomMinus.BitmapPushed = ((System.Drawing.Bitmap)(resources.GetObject("buttonZoomMinus.BitmapPushed")));
            this.buttonZoomMinus.BitmapSelected =
                ((System.Drawing.Bitmap)(resources.GetObject("buttonZoomMinus.BitmapSelected")));
            this.buttonZoomMinus.ButtonStyle = OpenLiveWriter.Controls.ButtonStyle.Bitmap;
            this.buttonZoomMinus.Location = new System.Drawing.Point(64, 9);
            this.buttonZoomMinus.Name = "buttonZoomMinus";
            this.buttonZoomMinus.Size = new System.Drawing.Size(12, 12);
            this.buttonZoomMinus.TabIndex = 1;
            this.buttonZoomMinus.Text = "button1";
            this.buttonZoomMinus.ToolTip = "";
            this.buttonZoomMinus.Click += new System.EventHandler(this.buttonZoomMinus_Click);
            //
            // panelBirdseye
            //
            this.panelBirdseye.Controls.Add(this.mapBirdsEyeDirectionControl);
            this.panelBirdseye.Controls.Add(this.mapBirdsEyeZoomControl);
            this.panelBirdseye.Location = new System.Drawing.Point(7, 224);
            this.panelBirdseye.Name = "panelBirdseye";
            this.panelBirdseye.Size = new System.Drawing.Size(176, 247);
            this.panelBirdseye.TabIndex = 3;
            this.panelBirdseye.Visible = false;
            //
            // mapBirdsEyeDirectionControl
            //
            this.mapBirdsEyeDirectionControl.Location = new System.Drawing.Point(0, 0);
            this.mapBirdsEyeDirectionControl.Name = "mapBirdsEyeDirectionControl";
            this.mapBirdsEyeDirectionControl.Size = new System.Drawing.Size(62, 57);
            this.mapBirdsEyeDirectionControl.TabIndex = 0;
            //
            // mapBirdsEyeZoomControl
            //
            this.mapBirdsEyeZoomControl.Location = new System.Drawing.Point(66, 0);
            this.mapBirdsEyeZoomControl.Name = "mapBirdsEyeZoomControl";
            this.mapBirdsEyeZoomControl.Size = new System.Drawing.Size(109, 61);
            this.mapBirdsEyeZoomControl.TabIndex = 1;
            this.mapBirdsEyeZoomControl.TabStop = false;
            this.mapBirdsEyeZoomControl.ZoomLevel = OpenLiveWriter.InternalWriterPlugin.Controls.BirdsEyeZoomLevel.Small;
            //
            // mapTipControl
            //
            this.mapTipControl.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                   | System.Windows.Forms.AnchorStyles.Right)));
            this.mapTipControl.Location = new System.Drawing.Point(188, 441);
            this.mapTipControl.Name = "mapTipControl";
            this.mapTipControl.Size = new System.Drawing.Size(256, 30);
            this.mapTipControl.TabIndex = 10;
            this.mapTipControl.TabStop = false;
            this.mapTipControl.ForeColor = SystemColors.ControlDarkDark;
            this.mapTipControl.Text = "Tip: Right-click to add a pushpin";
            //
            // MapForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(618, 474);
            this.Controls.Add(this.mapTipControl);
            this.Controls.Add(this.panelBirdseye);
            this.Controls.Add(this.panelNormal);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.mapControl);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.comboBoxStyle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(554, 453);
            this.Name = "MapForm";
            this.Text = "Insert Map";
            this.Resize += new System.EventHandler(this.MapForm_Resize);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).EndInit();
            this.panelNormal.ResumeLayout(false);
            this.panelBirdseye.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Address
        {
            get { return textBoxAddress.Text.Trim(); }
            set { textBoxAddress.Text = value; }
        }

        public float Latitude
        {
            get { return mapControl.Latitude; }
        }

        public float Longitude
        {
            get { return mapControl.Longitude; }
        }

        public string Reserved
        {
            get { return mapControl.Reserved; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MapStyle
        {
            get { return mapControl.Style; }
            set { if (mapControl != null) mapControl.Style = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VEPushpin[] Pushpins
        {
            get { return mapControl.Pushpins; }
            set { if (mapControl != null) mapControl.Pushpins = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VEBirdseyeScene BirdseyeScene
        {
            get { return mapControl.BirdseyeScene; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VEOrientation BirdseyeOrientation
        {
            get { return mapControl.BirdseyeOrientation; }
            set { if (mapControl != null) mapControl.BirdseyeOrientation = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ZoomLevel
        {
            get { return mapControl.ZoomLevel; }
            set { if (mapControl != null) mapControl.ZoomLevel = value; }
        }

        public void LoadMap(float latitude, float longitude, string reserved, string style, int zoomLevel, VEBirdseyeScene scene)
        {
            mapControl.LoadMap(latitude, longitude, reserved, style, zoomLevel, scene);
            UpdateMapStyle();
            UpdateMapZoomLevel();
        }

        public Size MapSize
        {
            get { return mapControl.Size; }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter && textBoxAddress.Focused)
            {
                handleLocationSearch();
                return true;
            }
            else
            {
                return base.ProcessDialogKey(keyData);
            }
        }

        private void textBoxAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                handleLocationSearch();
                e.Handled = true;
            }
        }

        private void comboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            string mapStyle = "r";
            switch (comboBoxStyle.MapStyle)
            {
                case VEMapStyle.Road:
                    mapStyle = "r";
                    break;
                case VEMapStyle.Aerial:
                    if (ShowLabels)
                        mapStyle = "h";
                    else
                        mapStyle = "a";
                    break;
                case VEMapStyle.Birdseye:
                    mapStyle = "o";
                    break;
                default:
                    Debug.Fail("Unknown mapStyle: " + comboBoxStyle.MapStyle.ToString());
                    break;
            }
            ManageMapStyleControls();
            mapControl.Style = mapStyle;
        }

        public bool ShowLabels
        {
            get { return cbShowLabels.Checked; }
            set { cbShowLabels.Checked = value; }
        }

        private void UpdateMapStyle()
        {
            switch (mapControl.Style)
            {
                case "r":
                    comboBoxStyle.MapStyle = VEMapStyle.Road;
                    cbShowLabels.Visible = false;
                    break;
                case "a":
                    ShowLabels = false;
                    comboBoxStyle.MapStyle = VEMapStyle.Aerial;
                    cbShowLabels.Visible = true;
                    break;
                case "h":
                    ShowLabels = true;
                    comboBoxStyle.MapStyle = VEMapStyle.Aerial;
                    cbShowLabels.Visible = true;
                    break;
                case "o":
                    comboBoxStyle.MapStyle = VEMapStyle.Birdseye;
                    break;
            }

            ManageMapStyleControls();
        }

        /// <summary>
        /// Manages the display state for any controls that are dependent on the map style.
        /// </summary>
        private void ManageMapStyleControls()
        {
            mapTipControl.Visible = comboBoxStyle.MapStyle != VEMapStyle.Birdseye;
        }

        private void UpdateMapZoomLevel()
        {
            int zoomLevel = mapControl.ZoomLevel;
            trackBarZoom.Value = zoomLevel;
            if (mapControl.Style == "o")
            {
                panelBirdseye.Location = panelNormal.Location;
                panelBirdseye.Visible = true;
                panelNormal.Visible = false;
                if (zoomLevel == 2)
                    mapBirdsEyeZoomControl.ZoomLevel = BirdsEyeZoomLevel.Large;
                else
                    mapBirdsEyeZoomControl.ZoomLevel = BirdsEyeZoomLevel.Small;
                ;
            }
            else
            {
                panelNormal.Visible = true;
                panelBirdseye.Visible = false;
            }
        }

        private void mapControl_MapStyleChanged(object sender, EventArgs e)
        {
            UpdateMapStyle();
        }

        private void mapControl_ZoomLevelChanged(object sender, EventArgs e)
        {
            UpdateMapZoomLevel();
        }

        private void trackBarZoom_ValueChanged(object sender, EventArgs e)
        {
            if (!_suspendZoom && mapControl.ZoomLevel != trackBarZoom.Value)
                mapControl.ZoomLevel = trackBarZoom.Value;
        }

        private void trackBarZoom_MouseUp(object sender, MouseEventArgs e)
        {
            _suspendZoom = false;
            if (mapControl.ZoomLevel != trackBarZoom.Value)
                mapControl.ZoomLevel = trackBarZoom.Value;
        }

        private void trackBarZoom_MouseDown(object sender, MouseEventArgs e)
        {
            //suspend the zoom event handling while the slider is being adjusted.
            _suspendZoom = true;
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            handleLocationSearch();
        }

        private void handleLocationSearch()
        {
            if (Address.Trim() != String.Empty)
                mapControl.LoadMap(Address);
            else
            {
                DisplayMessage.Show(MessageId.NoMapAddressSpecified, this);
            }
        }

        private void buttonGotoBirdseye_Click(object sender, EventArgs e)
        {
            mapControl.Style = "o";
        }

        private void cbShowLabels_CheckedChanged(object sender, EventArgs e)
        {
            string newStyle = cbShowLabels.Checked ? "h" : "a";
            if (mapControl.Style != newStyle)
                mapControl.Style = newStyle;
        }

        private void mapControl_BirdseyeChanged(object sender, EventArgs e)
        {
            if (mapControl.BirdsEyeAvailable)
            {
                comboBoxStyle.ShowBirdsEye();
                buttonGotoBirdseye.Visible = true;
            }
            else
            {
                if (comboBoxStyle.MapStyle == VEMapStyle.Birdseye)
                {
                    comboBoxStyle.MapStyle = VEMapStyle.Aerial;
                }
                comboBoxStyle.HideBirdsEye();
                buttonGotoBirdseye.Visible = false;
            }
            if (comboBoxStyle.MapStyle == VEMapStyle.Birdseye)
            {
                UpdateBirdseyeControls();
            }
        }

        private void UpdateBirdseyeControls()
        {
            VEOrientation orientation = mapControl.BirdseyeOrientation;

            if (mapBirdsEyeDirectionControl.Direction != orientation)
                mapBirdsEyeDirectionControl.Direction = orientation;
        }

        private void mapBirdsEyeZoomControl_ZoomLevelChanged(object sender, EventArgs e)
        {
            if (mapBirdsEyeZoomControl.ZoomLevel == BirdsEyeZoomLevel.Large)
                mapControl.ZoomLevel = 2;
            else
                mapControl.ZoomLevel = 1;
        }

        private void mapBirdsEyeDirectionControl_DirectionChanged(object sender, EventArgs e)
        {
            mapControl.BirdseyeOrientation = mapBirdsEyeDirectionControl.Direction;
        }

        private void mapScrollControl_DirectionalButtonClicked(object sender, DirectionalButtonClickedEventArgs ea)
        {
            switch (ea.Direction)
            {
                case VEOrientation.North:
                    mapControl.PanMap(0, -PAN_OFFSET);
                    break;
                case VEOrientation.East:
                    mapControl.PanMap(PAN_OFFSET, 0);
                    break;
                case VEOrientation.South:
                    mapControl.PanMap(0, PAN_OFFSET);
                    break;
                case VEOrientation.West:
                    mapControl.PanMap(-PAN_OFFSET, 0);
                    break;
            }
        }

        private void mapControl_ShowPushpinContextMenu(MapContextMenuEvent e, string pushpinId)
        {
            new PushpinContextMenuHandler(this, mapControl, _mapOptions, e, pushpinId).ShowContextMenu(CommandManager);
        }

        private void mapControl_ShowMapContextMenu(MapContextMenuEvent e)
        {
            new ContextMenuHandler(this, mapControl, _mapOptions, e).ShowContextMenu(CommandManager);
        }

        private void MapForm_Resize(object sender, EventArgs e)
        {
            mapTipControl.Size = new Size(Math.Max(0, buttonOK.Location.X - mapTipControl.Location.X - 5), mapTipControl.Height);
        }

        private void buttonZoomMinus_Click(object sender, EventArgs e)
        {
            mapControl.ZoomLevel = Math.Max(MapControl.MIN_ZOOM, mapControl.ZoomLevel - 1);
        }

        private void mapZoomPlusButton_Click(object sender, EventArgs e)
        {
            mapControl.ZoomLevel = Math.Min(MapControl.MAX_ZOOM, mapControl.ZoomLevel + 1);
        }

        private class ContextMenuHandler
        {
            private MapContextMenuEvent _event;
            private MapControl _mapControl;
            private MapOptions _mapOptions;
            private MapForm _mapForm;
            private const int StreetLevel = 16;
            private const int CityLevel = 11;
            private const int RegionLevel = 6;

            internal ContextMenuHandler(MapForm parentFrame, MapControl mapControl, MapOptions mapOptions, MapContextMenuEvent e)
            {
                _mapForm = parentFrame;
                _mapControl = mapControl;
                _mapOptions = mapOptions;
                _event = e;
            }

            public void ShowContextMenu(CommandManager commandManager)
            {
                MapContextCommand command =
                    MapContextMenu.ShowMapContextMenu(_mapControl, _mapControl.PointToScreen(new Point(_event.X, _event.Y)),
                                                      CalculateExcludedCommands(), commandManager);
                switch (command)
                {
                    case MapContextCommand.AddPushpin:
                        AddPushpin();
                        break;
                    case MapContextCommand.CenterMap:
                        CenterMap();
                        break;
                    case MapContextCommand.ZoomCityLevel:
                        ZoomCityLevel();
                        break;
                    case MapContextCommand.ZoomStreetLevel:
                        ZoomStreetLevel();
                        break;
                    case MapContextCommand.ZoomRegionLevel:
                        ZoomRegionLevel();
                        break;
                    case MapContextCommand.None:
                        break;
                    default:
                        Debug.Fail("Unknown MapContextCommand: " + command.ToString());
                        break;
                }
            }

            private MapContextCommand[] CalculateExcludedCommands()
            {
                MapContextCommand excludedCommand = MapContextCommand.ZoomRegionLevel;
                int zoomLevel = _mapControl.ZoomLevel;
                if (zoomLevel == StreetLevel)
                    excludedCommand = MapContextCommand.ZoomStreetLevel;
                else if (zoomLevel == CityLevel)
                    excludedCommand = MapContextCommand.ZoomCityLevel;
                else if (zoomLevel < RegionLevel)
                    excludedCommand = MapContextCommand.ZoomStreetLevel;

                return new MapContextCommand[] { excludedCommand };
            }

            private void AddPushpin()
            {
                MapPushpinForm pushpinForm =
                    new MapPushpinForm(_mapForm, _mapControl.PointToScreen(new Point(_event.X, _event.Y)),
                                       new MapPushpinEditedHandler(_mapPushpinAddedHandler));
                pushpinForm.Show();
                pushpinForm.FloatAboveOwner(_mapForm);
            }

            private void CenterMap()
            {
                _mapControl.ZoomToLocation(new Point(_event.X, _event.Y), _mapControl.ZoomLevel);
            }

            private void ZoomCityLevel()
            {
                _mapControl.ZoomToLocation(new Point(_event.X, _event.Y), CityLevel);
            }

            private void ZoomStreetLevel()
            {
                _mapControl.ZoomToLocation(new Point(_event.X, _event.Y), StreetLevel);
            }

            private void ZoomRegionLevel()
            {
                _mapControl.ZoomToLocation(new Point(_event.X, _event.Y), RegionLevel);
            }

            private void _mapPushpinAddedHandler(MapPushpinInfo pushpinInfo)
            {
                int nextPinId = 1;
                for (nextPinId = 1; _mapControl.GetPushpin(nextPinId.ToString(CultureInfo.InvariantCulture)) != null; nextPinId++) ;

                _mapControl.AddPushpin(
                    new VEPushpin(nextPinId.ToString(CultureInfo.InvariantCulture), new VELatLong(_event.Latitude, _event.Longitude, _event.Reserved),
                                  _mapOptions.PushpinUrl, pushpinInfo.Title, pushpinInfo.Notes, pushpinInfo.MoreInfoUrl,
                                  pushpinInfo.PhotoUrl));
            }
        }

        private class PushpinContextMenuHandler
        {
            private MapForm _mapForm;
            private MapContextMenuEvent _event;
            private MapControl _mapControl;
            private MapOptions _mapOptions;
            private string _pushpinId;

            internal PushpinContextMenuHandler(MapForm parentFrame, MapControl mapControl, MapOptions mapOptions,
                                               MapContextMenuEvent e, string pushpinId)
            {
                _mapForm = parentFrame;
                _mapControl = mapControl;
                _mapOptions = mapOptions;
                _event = e;
                _pushpinId = pushpinId;
            }

            public void ShowContextMenu(CommandManager commandManager)
            {
                PushpinContextCommand command =
                    MapContextMenu.ShowPushpinContextMenu(_mapControl, _mapControl.PointToScreen(new Point(_event.X, _event.Y)), commandManager);
                switch (command)
                {
                    case PushpinContextCommand.DeletePushpin:
                        DeletePushpin();
                        break;
                    case PushpinContextCommand.EditPushpin:
                        EditPushpin();
                        break;
                    case PushpinContextCommand.None:
                        break;
                    default:
                        Debug.Fail("Unknown PushpinContextCommand: " + command.ToString());
                        break;
                }
            }

            private void DeletePushpin()
            {
                _mapControl.DeletePushpin(_pushpinId);
            }

            private void EditPushpin()
            {
                VEPushpin pushpin = _mapControl.GetPushpin(_pushpinId);
                if (pushpin != null)
                {
                    MapPushpinInfo pushpinInfo = new MapPushpinInfo(pushpin.Title);
                    pushpinInfo.Notes = pushpin.Details;
                    pushpinInfo.MoreInfoUrl = pushpin.MoreInfoUrl;
                    pushpinInfo.PhotoUrl = pushpin.PhotoUrl;
                    MapPushpinForm pushpinForm =
                        new MapPushpinForm(_mapForm, _mapControl.PointToScreen(new Point(_event.X, _event.Y)),
                                           new MapPushpinEditedHandler(_mapPushpinEditedHandler), pushpinInfo);
                    pushpinForm.Show();
                    pushpinForm.FloatAboveOwner(_mapForm);
                }
            }

            private void _mapPushpinEditedHandler(MapPushpinInfo pushpinInfo)
            {
                VEPushpin oldPushpin = _mapControl.GetPushpin(_pushpinId);
                if (oldPushpin != null)
                    _mapControl.UpdatePushpin(
                        new VEPushpin(_pushpinId, oldPushpin.VELatLong, oldPushpin.ImageFile, pushpinInfo.Title, pushpinInfo.Notes,
                                      pushpinInfo.MoreInfoUrl, pushpinInfo.PhotoUrl));
            }
        }

    }
}
