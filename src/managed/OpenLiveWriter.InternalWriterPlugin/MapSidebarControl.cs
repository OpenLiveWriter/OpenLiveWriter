// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;

// @RIBBON TODO: Remove obsolete code

namespace OpenLiveWriter.InternalWriterPlugin
{
    internal class MapSidebarControl : SmartContentEditor
    {
        private MapSettings _mapSettings;
        private System.ComponentModel.IContainer components;
        private ISmartContentEditorSite _contentEditorSite;

        private SectionHeaderControl headerLayout;
        private System.Windows.Forms.Label labelTextWrapping;
        private HtmlContentAlignmentComboBox comboBoxTextWrapping;
        private ToolTip2 toolTip;
        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.PictureBox pictureBoxCustomizeMap;
        private System.Windows.Forms.LinkLabel linkLabelCustomizeMap;
        private System.Windows.Forms.TextBox textBoxCaption;
        private SidebarHeaderControl sidebarHeaderControl1;
        private SectionHeaderControl sectionHeaderControlOptions;
        private Label labelBottom;
        private Label labelLeft;
        private Label labelRight;
        private Label labelTop;
        private NumericUpDown numericMarginLeft;
        private NumericUpDown numericMarginBottom;
        private NumericUpDown numericMarginRight;
        private NumericUpDown numericMarginTop;
        private Panel panelCustomMargins;
        private MapSidebarControl.MarginsComboBox comboBoxMargins;
        private Label labelMargins;

        private MapOptions _mapOptions;

        public MapSidebarControl(MapOptions mapOptions, ISmartContentEditorSite contentEditorSite)
        {
            _mapOptions = mapOptions;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.labelCaption.Text = Res.Get(StringId.MapSBCaption);
            this.labelMargins.Text = Res.Get(StringId.MapSBMargins);
            this.sectionHeaderControlOptions.HeaderText = Res.Get(StringId.Options);
            this.headerLayout.HeaderText = Res.Get(StringId.MapSBLayoutHeader);
            this.labelTop.Text = Res.Get(StringId.MapSBTop);
            this.labelRight.Text = Res.Get(StringId.MapSBRight);
            this.labelLeft.Text = Res.Get(StringId.MapSBLeft);
            this.labelBottom.Text = Res.Get(StringId.MapSBBottom);
            this.labelTextWrapping.Text = Res.Get(StringId.MapSBWrapping);
            this.linkLabelCustomizeMap.Text = Res.Get(StringId.MapSBCustomize);
            linkLabelCustomizeMap.LinkColor = ColorizedResources.Instance.SidebarLinkColor;
            //linkLabelCustomizeMap.LinkArea = new LinkArea(0, linkLabelCustomizeMap.Text.Length);
            this.toolTip.SetToolTip(this.linkLabelCustomizeMap, Res.Get(StringId.MapSBCustomizeTooltip));

            SimpleTextEditorCommandHelper.UseNativeBehaviors(((ICommandManagerHost)contentEditorSite).CommandManager, numericMarginLeft, numericMarginRight, numericMarginTop, numericMarginBottom);

            sidebarHeaderControl1.LinkText = Res.Get(StringId.ViewMap);
            sidebarHeaderControl1.HeaderText = Res.Get(StringId.MapSBMapHeader);
            sidebarHeaderControl1.LinkUrl = "";
            sidebarHeaderControl1.TabIndex = 0;

            _contentEditorSite = contentEditorSite;

            InitializeCommands();

            // initialize bitmaps
            pictureBoxCustomizeMap.Image = ResourceHelper.LoadAssemblyResourceBitmap("Images.CustomizeMapIcon.png");

            // additional initializatoin of combos
            comboBoxTextWrapping.Initialize();
            comboBoxMargins.Initialize();

        }

        private void InitializeCommands()
        {
            ((ICommandManagerHost)_contentEditorSite).CommandManager.Add(CommandId.MapWebPreview, (sender, e) => ShellHelper.LaunchUrl(sidebarHeaderControl1.LinkUrl));
            ((ICommandManagerHost)_contentEditorSite).CommandManager.Add(CommandId.FormatMapEdit, (sender, e) => ShowCustomizeMapDialog());
        }

        protected override void OnSelectedContentChanged()
        {
            base.OnSelectedContentChanged();

            _mapSettings = new MapSettings(SelectedContent.Properties);
            sidebarHeaderControl1.LinkUrl = _mapSettings.LiveMapUrl;

            // update the settings UI
            InitializeOptionsUI();

            // force a layout for dynamic control flow
            PerformLayout();
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            sidebarHeaderControl1.RefreshLayout();
            LayoutHelper.FitControlsBelow(0, sidebarHeaderControl1);
            DisplayHelper.AutoFitSystemLabel(linkLabelCustomizeMap, linkLabelCustomizeMap.Width,
                                             linkLabelCustomizeMap.Width);
            LayoutHelper.NaturalizeHeightAndDistribute(8, linkLabelCustomizeMap, new ControlGroup(labelCaption, textBoxCaption));
            LayoutHelper.FitControlsBelow(20, textBoxCaption);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelMargins, comboBoxMargins, panelCustomMargins);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            VirtualTransparency.VirtualPaint(this, pevent);
        }

        private void ShowCustomizeMapDialog()
        {
            using (new WaitCursor())
            {
                if (!MapForm.ValidateLiveLocalConnection(true))
                    return;

                using (MapForm mapForm = new MapForm(false, _mapOptions, ((ICommandManagerHost)_contentEditorSite).CommandManager))
                {
                    VEBirdseyeScene scene = null;
                    if (_mapSettings.MapStyle == "o")
                        scene = new VEBirdseyeScene(_mapSettings.BirdseyeSceneId, _mapSettings.BirdseyeOrientation);
                    mapForm.LoadMap(_mapSettings.Latitude, _mapSettings.Longitude, _mapSettings.Reserved, _mapSettings.MapStyle, _mapSettings.ZoomLevel, scene);
                    mapForm.Pushpins = _mapSettings.Pushpins;
                    if (mapForm.ShowDialog(this) == DialogResult.OK)
                    {
                        _mapOptions.DefaultDialogSize = mapForm.Size;
                        _mapSettings.UpdateSettings(mapForm.Latitude, mapForm.Longitude, mapForm.Reserved, mapForm.ZoomLevel, mapForm.MapStyle, mapForm.Pushpins, mapForm.BirdseyeScene);

                        SelectedContent.Files.Remove(_mapSettings.ImageFileId);
                        Size mapSize = _mapSettings.Size;
                        Debug.Assert(mapSize != Size.Empty);
                        MapContentSource.UpdateMapImage(SelectedContent, _mapSettings, mapSize);
                        _mapSettings.Size = mapSize;

                        sidebarHeaderControl1.LinkUrl = _mapSettings.LiveMapUrl;
                        sidebarHeaderControl1.RefreshLayout();

                        OnContentEdited();
                    }
                }
            }
        }

        private void linkLabelCustomizeMap_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            ShowCustomizeMapDialog();
        }

        private void comboBoxTextWrapping_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedContent.Layout.Alignment = comboBoxTextWrapping.Alignment;
            OnContentEdited();
        }

        private void textBoxCaption_TextChanged(object sender, EventArgs e)
        {
            _mapSettings.Caption = textBoxCaption.Text;
            OnContentEdited();
        }

        private void comboBoxMargins_SelectedIndexChanged(object sender, EventArgs e)
        {
            // manage visibility of padding panel and reset margins if necessary
            if (comboBoxMargins.CustomMargins)
            {
                panelCustomMargins.Visible = true;
            }
            else
            {
                panelCustomMargins.Visible = false;

                numericMarginTop.Value = 0;
                numericMarginLeft.Value = 0;
                numericMarginBottom.Value = 0;
                numericMarginRight.Value = 0;
            }

            // update
            UpdatePadding();
        }

        private void numericMargin_ValueChanged(object sender, EventArgs e)
        {
            UpdatePadding();
        }

        private void UpdatePadding()
        {
            // update underlying values
            SelectedContent.Layout.TopMargin = Convert.ToInt32(numericMarginTop.Value);
            SelectedContent.Layout.LeftMargin = Convert.ToInt32(numericMarginLeft.Value);
            SelectedContent.Layout.BottomMargin = Convert.ToInt32(numericMarginBottom.Value);
            SelectedContent.Layout.RightMargin = Convert.ToInt32(numericMarginRight.Value);

            // invalidate
            OnContentEdited();
        }

        private void InitializeOptionsUI()
        {
            UnhookSettingsUIChangedEvents();

            // caption
            textBoxCaption.Text = _mapSettings.Caption;

            // alignment and margins
            comboBoxTextWrapping.Alignment = SelectedContent.Layout.Alignment;
            comboBoxMargins.CustomMargins = ShowCustomMargins(SelectedContent.Layout);
            panelCustomMargins.Visible = comboBoxMargins.CustomMargins;
            numericMarginTop.Value = SelectedContent.Layout.TopMargin;
            numericMarginLeft.Value = SelectedContent.Layout.LeftMargin;
            numericMarginBottom.Value = SelectedContent.Layout.BottomMargin;
            numericMarginRight.Value = SelectedContent.Layout.RightMargin;

            HookSettingsUIChangedEvents();
        }

        private void HookSettingsUIChangedEvents()
        {
            // caption
            textBoxCaption.TextChanged += new EventHandler(textBoxCaption_TextChanged);

            // alignment and margins
            comboBoxTextWrapping.SelectedIndexChanged += new EventHandler(comboBoxTextWrapping_SelectedIndexChanged);
            comboBoxMargins.SelectedIndexChanged += new EventHandler(comboBoxMargins_SelectedIndexChanged);
            numericMarginTop.ValueChanged += new EventHandler(numericMargin_ValueChanged);
            numericMarginLeft.ValueChanged += new EventHandler(numericMargin_ValueChanged);
            numericMarginBottom.ValueChanged += new EventHandler(numericMargin_ValueChanged);
            numericMarginRight.ValueChanged += new EventHandler(numericMargin_ValueChanged);
        }

        private void UnhookSettingsUIChangedEvents()
        {
            // caption
            textBoxCaption.TextChanged -= new EventHandler(textBoxCaption_TextChanged);

            // alignment and margins
            comboBoxTextWrapping.SelectedIndexChanged -= new EventHandler(comboBoxTextWrapping_SelectedIndexChanged);
            comboBoxMargins.SelectedIndexChanged -= new EventHandler(comboBoxMargins_SelectedIndexChanged);
            numericMarginTop.ValueChanged -= new EventHandler(numericMargin_ValueChanged);
            numericMarginLeft.ValueChanged -= new EventHandler(numericMargin_ValueChanged);
            numericMarginBottom.ValueChanged -= new EventHandler(numericMargin_ValueChanged);
            numericMarginRight.ValueChanged -= new EventHandler(numericMargin_ValueChanged);
        }

        private bool ShowCustomMargins(ILayoutStyle layoutStyle)
        {
            return (layoutStyle.TopMargin != 0) || (layoutStyle.RightMargin != 0) || (layoutStyle.BottomMargin != 0) || (layoutStyle.LeftMargin != 0);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private class HtmlContentAlignmentComboBox : ImageComboBox
        {
            public HtmlContentAlignmentComboBox()
                : base(new Size(33, 21))
            {
            }

            public void Initialize()
            {
                AllowMirroring = false;
                Items.Add(new ComboItem(OpenLiveWriter.Api.Alignment.None, Res.Get(StringId.MapAlignInline), "Images.AlignmentInline.png"));
                Items.Add(new ComboItem(OpenLiveWriter.Api.Alignment.Left, Res.Get(StringId.MapAlignLeft), "Images.AlignmentLeft.png"));
                Items.Add(new ComboItem(OpenLiveWriter.Api.Alignment.Right, Res.Get(StringId.MapAlignRight), "Images.AlignmentRight.png"));
                Items.Add(new ComboItem(OpenLiveWriter.Api.Alignment.Center, Res.Get(StringId.MapAlignCenter), "Images.AlignmentCenter.png"));
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Alignment Alignment
            {
                get
                {
                    return (SelectedItem as ComboItem).Alignment;
                }
                set
                {
                    ComboItem itemToSelect = null;
                    foreach (ComboItem item in Items)
                    {
                        if (item.Alignment == value)
                        {
                            itemToSelect = item;
                            break;
                        }
                    }
                    if (itemToSelect != null)
                        SelectedItem = itemToSelect;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    foreach (ComboItem comboItem in Items)
                        comboItem.Dispose();
                }

                base.Dispose(disposing);
            }

            private class ComboItem : ImageComboBox.IComboItem, IDisposable
            {
                public ComboItem(Alignment alignment, string caption, string imageResourcePath)
                {
                    _alignment = alignment;
                    _caption = caption;
                    _image = new Bitmap(typeof(MapSidebarControl), imageResourcePath);
                }

                public void Dispose()
                {
                    if (_image != null)
                        _image.Dispose();
                }

                public Alignment Alignment { get { return _alignment; } }
                private Alignment _alignment;

                public Image Image { get { return _image; } }
                private Image _image;

                public override string ToString() { return _caption; }
                private string _caption;

                public override bool Equals(object obj) { return (obj as ComboItem).Alignment == Alignment; }
                public override int GetHashCode() { return Alignment.GetHashCode(); }
            }
        }

        private class MarginsComboBox : ComboBox
        {
            public MarginsComboBox()
            {
            }

            public void Initialize()
            {
                Items.Add(NO_MARGINS);
                Items.Add(CUSTOM_MARGINS);
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool CustomMargins
            {
                get { return SelectedItem != null && SelectedItem.Equals(CUSTOM_MARGINS); }
                set { if (Items.Count > 0) SelectedItem = value ? CUSTOM_MARGINS : NO_MARGINS; }
            }

            private static readonly string NO_MARGINS = Res.Get(StringId.MapNoMargins);
            private static readonly string CUSTOM_MARGINS = Res.Get(StringId.MapCustomMargins);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.textBoxCaption = new System.Windows.Forms.TextBox();
            this.labelCaption = new System.Windows.Forms.Label();
            this.labelTextWrapping = new System.Windows.Forms.Label();
            this.comboBoxTextWrapping = new OpenLiveWriter.InternalWriterPlugin.MapSidebarControl.HtmlContentAlignmentComboBox();
            this.linkLabelCustomizeMap = new System.Windows.Forms.LinkLabel();
            this.pictureBoxCustomizeMap = new System.Windows.Forms.PictureBox();
            this.sidebarHeaderControl1 = new OpenLiveWriter.ApplicationFramework.SidebarHeaderControl();
            this.headerLayout = new OpenLiveWriter.ApplicationFramework.SectionHeaderControl();
            this.sectionHeaderControlOptions = new OpenLiveWriter.ApplicationFramework.SectionHeaderControl();
            this.labelBottom = new System.Windows.Forms.Label();
            this.labelLeft = new System.Windows.Forms.Label();
            this.labelRight = new System.Windows.Forms.Label();
            this.labelTop = new System.Windows.Forms.Label();
            this.numericMarginLeft = new System.Windows.Forms.NumericUpDown();
            this.numericMarginBottom = new System.Windows.Forms.NumericUpDown();
            this.numericMarginRight = new System.Windows.Forms.NumericUpDown();
            this.numericMarginTop = new System.Windows.Forms.NumericUpDown();
            this.panelCustomMargins = new System.Windows.Forms.Panel();
            this.comboBoxMargins = new OpenLiveWriter.InternalWriterPlugin.MapSidebarControl.MarginsComboBox();
            this.labelMargins = new System.Windows.Forms.Label();
            this.toolTip = new OpenLiveWriter.Controls.ToolTip2(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustomizeMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginTop)).BeginInit();
            this.panelCustomMargins.SuspendLayout();
            this.SuspendLayout();
            //
            // textBoxCaption
            //
            this.textBoxCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCaption.Location = new System.Drawing.Point(11, 159);
            this.textBoxCaption.Name = "textBoxCaption";
            this.textBoxCaption.Size = new System.Drawing.Size(173, 20);
            this.textBoxCaption.TabIndex = 10;
            //
            // labelCaption
            //
            this.labelCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCaption.Location = new System.Drawing.Point(12, 141);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Size = new System.Drawing.Size(161, 16);
            this.labelCaption.TabIndex = 9;
            this.labelCaption.Text = "Caption:";
            //
            // labelTextWrapping
            //
            this.labelTextWrapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTextWrapping.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTextWrapping.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelTextWrapping.Location = new System.Drawing.Point(12, 219);
            this.labelTextWrapping.Name = "labelTextWrapping";
            this.labelTextWrapping.Size = new System.Drawing.Size(171, 15);
            this.labelTextWrapping.TabIndex = 14;
            this.labelTextWrapping.Text = "Text wrapping:";
            //
            // comboBoxTextWrapping
            //
            this.comboBoxTextWrapping.AllowMirroring = true;
            this.comboBoxTextWrapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTextWrapping.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxTextWrapping.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTextWrapping.ItemHeight = 25;
            this.comboBoxTextWrapping.Location = new System.Drawing.Point(11, 237);
            this.comboBoxTextWrapping.Name = "comboBoxTextWrapping";
            this.comboBoxTextWrapping.Size = new System.Drawing.Size(173, 31);
            this.comboBoxTextWrapping.TabIndex = 15;
            //
            // linkLabelCustomizeMap
            //
            this.linkLabelCustomizeMap.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkLabelCustomizeMap.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkLabelCustomizeMap.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelCustomizeMap.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkLabelCustomizeMap.Location = new System.Drawing.Point(36, 118);
            this.linkLabelCustomizeMap.Name = "linkLabelCustomizeMap";
            this.linkLabelCustomizeMap.Size = new System.Drawing.Size(147, 17);
            this.linkLabelCustomizeMap.TabIndex = 5;
            this.linkLabelCustomizeMap.TabStop = true;
            this.linkLabelCustomizeMap.Text = "Customize Map...";
            this.toolTip.SetToolTip(this.linkLabelCustomizeMap, "Edit the Map location, style, and pushpins");
            this.linkLabelCustomizeMap.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCustomizeMap_LinkClicked);
            //
            // pictureBoxCustomizeMap
            //
            this.pictureBoxCustomizeMap.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBoxCustomizeMap.Location = new System.Drawing.Point(11, 117);
            this.pictureBoxCustomizeMap.Name = "pictureBoxCustomizeMap";
            this.pictureBoxCustomizeMap.Size = new System.Drawing.Size(23, 16);
            this.pictureBoxCustomizeMap.TabIndex = 3;
            this.pictureBoxCustomizeMap.TabStop = false;
            //
            // sidebarHeaderControl1
            //
            this.sidebarHeaderControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.sidebarHeaderControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.sidebarHeaderControl1.Location = new System.Drawing.Point(10, 2);
            this.sidebarHeaderControl1.Name = "sidebarHeaderControl1";
            this.sidebarHeaderControl1.Size = new System.Drawing.Size(184, 48);
            this.sidebarHeaderControl1.TabIndex = 31;
            //
            // headerLayout
            //
            this.headerLayout.AccessibleName = "Layout";
            this.headerLayout.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this.headerLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.headerLayout.HeaderText = "Layout";
            this.headerLayout.Location = new System.Drawing.Point(7, 199);
            this.headerLayout.Name = "headerLayout";
            this.headerLayout.Size = new System.Drawing.Size(184, 17);
            this.headerLayout.TabIndex = 3;
            this.headerLayout.TabStop = false;
            //
            // sectionHeaderControlOptions
            //
            this.sectionHeaderControlOptions.AccessibleName = "Options";
            this.sectionHeaderControlOptions.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this.sectionHeaderControlOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.sectionHeaderControlOptions.HeaderText = "Options";
            this.sectionHeaderControlOptions.Location = new System.Drawing.Point(7, 93);
            this.sectionHeaderControlOptions.Name = "sectionHeaderControlOptions";
            this.sectionHeaderControlOptions.Size = new System.Drawing.Size(184, 17);
            this.sectionHeaderControlOptions.TabIndex = 32;
            this.sectionHeaderControlOptions.TabStop = false;
            //
            // labelBottom
            //
            this.labelBottom.AutoSize = true;
            this.labelBottom.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelBottom.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelBottom.Location = new System.Drawing.Point(0, 40);
            this.labelBottom.Name = "labelBottom";
            this.labelBottom.Size = new System.Drawing.Size(40, 13);
            this.labelBottom.TabIndex = 20;
            this.labelBottom.Text = "Bottom";
            //
            // labelLeft
            //
            this.labelLeft.AutoSize = true;
            this.labelLeft.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelLeft.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelLeft.Location = new System.Drawing.Point(93, 40);
            this.labelLeft.Name = "labelLeft";
            this.labelLeft.Size = new System.Drawing.Size(25, 13);
            this.labelLeft.TabIndex = 30;
            this.labelLeft.Text = "Left";
            //
            // labelRight
            //
            this.labelRight.AutoSize = true;
            this.labelRight.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelRight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelRight.Location = new System.Drawing.Point(93, 0);
            this.labelRight.Name = "labelRight";
            this.labelRight.Size = new System.Drawing.Size(32, 13);
            this.labelRight.TabIndex = 10;
            this.labelRight.Text = "Right";
            //
            // labelTop
            //
            this.labelTop.AutoSize = true;
            this.labelTop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTop.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelTop.Location = new System.Drawing.Point(0, 0);
            this.labelTop.Name = "labelTop";
            this.labelTop.Size = new System.Drawing.Size(26, 13);
            this.labelTop.TabIndex = 0;
            this.labelTop.Text = "Top";
            //
            // numericMarginLeft
            //
            this.numericMarginLeft.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericMarginLeft.Location = new System.Drawing.Point(93, 56);
            this.numericMarginLeft.Name = "numericMarginLeft";
            this.numericMarginLeft.Size = new System.Drawing.Size(56, 20);
            this.numericMarginLeft.TabIndex = 35;
            this.numericMarginLeft.Enter += new System.EventHandler(this.numericMargin_Enter);
            //
            // numericMarginBottom
            //
            this.numericMarginBottom.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericMarginBottom.Location = new System.Drawing.Point(0, 56);
            this.numericMarginBottom.Name = "numericMarginBottom";
            this.numericMarginBottom.Size = new System.Drawing.Size(56, 20);
            this.numericMarginBottom.TabIndex = 25;
            this.numericMarginBottom.Enter += new System.EventHandler(this.numericMargin_Enter);
            //
            // numericMarginRight
            //
            this.numericMarginRight.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericMarginRight.Location = new System.Drawing.Point(93, 16);
            this.numericMarginRight.Name = "numericMarginRight";
            this.numericMarginRight.Size = new System.Drawing.Size(56, 20);
            this.numericMarginRight.TabIndex = 15;
            this.numericMarginRight.Enter += new System.EventHandler(this.numericMargin_Enter);
            //
            // numericMarginTop
            //
            this.numericMarginTop.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericMarginTop.Location = new System.Drawing.Point(0, 16);
            this.numericMarginTop.Name = "numericMarginTop";
            this.numericMarginTop.Size = new System.Drawing.Size(56, 20);
            this.numericMarginTop.TabIndex = 1;
            this.numericMarginTop.Enter += new System.EventHandler(this.numericMargin_Enter);
            //
            // panelCustomMargins
            //
            this.panelCustomMargins.Controls.Add(this.numericMarginTop);
            this.panelCustomMargins.Controls.Add(this.numericMarginRight);
            this.panelCustomMargins.Controls.Add(this.numericMarginBottom);
            this.panelCustomMargins.Controls.Add(this.numericMarginLeft);
            this.panelCustomMargins.Controls.Add(this.labelTop);
            this.panelCustomMargins.Controls.Add(this.labelRight);
            this.panelCustomMargins.Controls.Add(this.labelLeft);
            this.panelCustomMargins.Controls.Add(this.labelBottom);
            this.panelCustomMargins.Location = new System.Drawing.Point(11, 319);
            this.panelCustomMargins.Name = "panelCustomMargins";
            this.panelCustomMargins.Size = new System.Drawing.Size(180, 92);
            this.panelCustomMargins.TabIndex = 30;
            //
            // comboBoxMargins
            //
            this.comboBoxMargins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMargins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMargins.ItemHeight = 13;
            this.comboBoxMargins.Location = new System.Drawing.Point(11, 291);
            this.comboBoxMargins.Name = "comboBoxMargins";
            this.comboBoxMargins.Size = new System.Drawing.Size(173, 21);
            this.comboBoxMargins.TabIndex = 20;
            //
            // labelMargins
            //
            this.labelMargins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMargins.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelMargins.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelMargins.Location = new System.Drawing.Point(12, 273);
            this.labelMargins.Name = "labelMargins";
            this.labelMargins.Size = new System.Drawing.Size(172, 15);
            this.labelMargins.TabIndex = 19;
            this.labelMargins.Text = "Margins:";
            //
            // MapSidebarControl
            //
            this.Controls.Add(this.sectionHeaderControlOptions);
            this.Controls.Add(this.sidebarHeaderControl1);
            this.Controls.Add(this.textBoxCaption);
            this.Controls.Add(this.labelCaption);
            this.Controls.Add(this.labelMargins);
            this.Controls.Add(this.comboBoxMargins);
            this.Controls.Add(this.headerLayout);
            this.Controls.Add(this.panelCustomMargins);
            this.Controls.Add(this.labelTextWrapping);
            this.Controls.Add(this.comboBoxTextWrapping);
            this.Controls.Add(this.linkLabelCustomizeMap);
            this.Controls.Add(this.pictureBoxCustomizeMap);
            this.Name = "MapSidebarControl";
            this.Size = new System.Drawing.Size(200, 470);
            this.Enter += new System.EventHandler(this.numericMargin_Enter);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustomizeMap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMarginTop)).EndInit();
            this.panelCustomMargins.ResumeLayout(false);
            this.panelCustomMargins.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void numericMargin_Enter(object sender, System.EventArgs e)
        {
            NumericUpDown upDown = sender as NumericUpDown;
            if (upDown != null)
                upDown.Select(0, upDown.Text.Length);
        }

    }
}
