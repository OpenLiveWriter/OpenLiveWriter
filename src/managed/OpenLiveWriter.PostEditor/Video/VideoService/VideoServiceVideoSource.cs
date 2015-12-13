// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.PostEditor.Video.VideoListBox;

namespace OpenLiveWriter.PostEditor.Video.VideoService
{
    public class VideoServiceVideoSource : MediaTab, IRtlAware
    {
        private VideoListBoxControl _listBoxVideos;
        private SidebarListBox<IVideoService> _sidebarService;
        private LoginStatusControl _videoLoginStatusControl;
        private VideoPagingControl _videoPagingControl;
        private VideoRequestTypeComboBox _videoRequestComboBox;
        private PanelLoginControl _videoLoginControl;

        public VideoServiceVideoSource()
        {
            TabText = Res.Get(StringId.VideoFromVideoService);
            TabBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Video.VideoService.Images.VideoServiceTab.png");
            Name = "VideoServiceSource";

            _listBoxVideos = new VideoListBoxControl();
            _listBoxVideos.Initialize();

            _sidebarService = new SidebarListBox<IVideoService>();
            _sidebarService.Initialize();

            _videoLoginStatusControl = new LoginStatusControl();
            _videoLoginStatusControl.ShowLoginButton = false;

            _videoPagingControl = new VideoPagingControl();
            _videoPagingControl.RightToLeft = BidiHelper.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;

            _videoRequestComboBox = new VideoRequestTypeComboBox();
            _videoRequestComboBox.Visible = false;

            _videoLoginControl = new PanelLoginControl();

            SuspendLayout();
            Controls.Add(_listBoxVideos);
            Controls.Add(_sidebarService);
            Controls.Add(_videoLoginStatusControl);
            Controls.Add(_videoPagingControl);
            Controls.Add(_videoRequestComboBox);
            Controls.Add(_videoLoginControl);
            ResumeLayout();

            _sidebarService.AccessibleName = Res.Get(StringId.Plugin_Video_Provider_Select);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DoLayout();
        }

        public override List<Control> GetAccessibleControls()
        {
            List<Control> controls = new List<Control>();
            if (_videoLoginStatusControl.ShowLoginButton)
            {
                controls.Add(_videoLoginStatusControl);
            }
            controls.Add(_sidebarService);
            controls.Add(_videoRequestComboBox);
            controls.Add(_listBoxVideos);
            controls.AddRange(_videoLoginControl.GetAccessibleControls());
            controls.Add(_videoPagingControl);
            return controls;
        }

        void _sidebarService_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_sidebarService.SelectedValue != CurrentService)
            {
                CurrentService = _sidebarService.SelectedValue;

            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_listBoxVideos != null)
            {
                _listBoxVideos.Dispose();
                _listBoxVideos = null;
            }
            base.Dispose(disposing);
        }

        private void DoLayout()
        {
            SuspendLayout();

            _videoLoginStatusControl.Top = ContentSourceLayout.Y_MARGIN;
            _videoLoginStatusControl.Height = (int)DisplayHelper.ScaleY(32);
            _videoLoginStatusControl.Width = (Width - (ContentSourceLayout.X_MARGIN * 2)) / 2 - ContentSourceLayout.X_SPACING;
            _videoLoginStatusControl.TabIndex = 0;
            _videoLoginStatusControl.Left = ContentSourceLayout.X_MARGIN;

            _videoRequestComboBox.Top = _videoLoginStatusControl.Top;
            _videoRequestComboBox.Height = _videoLoginStatusControl.Height - ContentSourceLayout.Y_SPACING;
            _videoRequestComboBox.ItemHeight = _videoLoginStatusControl.Height - ContentSourceLayout.Y_SPACING;
            _videoRequestComboBox.IntegralHeight = true;
            _videoRequestComboBox.TabIndex = 10;
            _videoRequestComboBox.Left = _videoLoginStatusControl.Right + ContentSourceLayout.X_SPACING;
            _videoRequestComboBox.Width = Width - _videoRequestComboBox.Left - ContentSourceLayout.X_MARGIN;

            _videoPagingControl.Height = (int)DisplayHelper.ScaleY(40);
            _videoPagingControl.Top = Bottom - _videoPagingControl.Height + ContentSourceLayout.Y_SPACING;
            _videoPagingControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _videoPagingControl.TabIndex = 40;

            _sidebarService.Top = _videoLoginStatusControl.Bottom + ContentSourceLayout.Y_SPACING;
            _sidebarService.Height = Height - _sidebarService.Top - _videoPagingControl.Height;
            _sidebarService.Width = (int)DisplayHelper.ScaleX(101);
            _sidebarService.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            _sidebarService.Name = "videoProviderListBox";
            _sidebarService.TabIndex = 20;
            _sidebarService.Left = _videoLoginStatusControl.Left + LISTBOX_PADDING;

            _listBoxVideos.Top = _sidebarService.Top;
            _listBoxVideos.Left = _sidebarService.Right + ContentSourceLayout.X_SPACING;

            _listBoxVideos.Height = _sidebarService.Height;
            _listBoxVideos.Width = _videoRequestComboBox.Right - _sidebarService.Width - 2 * MARGIN_X - LISTBOX_PADDING;
            _listBoxVideos.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
            _listBoxVideos.TabIndex = 30;
            _listBoxVideos.Name = "listBoxVideos";

            _videoPagingControl.Width = _videoRequestComboBox.Width;
            _videoPagingControl.Left = _videoRequestComboBox.Left;

            _videoLoginControl.Location = _listBoxVideos.Location;
            _videoLoginControl.Size = _listBoxVideos.Size;
            _videoLoginControl.Anchor = _listBoxVideos.Anchor;
            _videoLoginControl.Visible = true;
            _videoLoginControl.TabIndex = 30;

            BidiHelper.RtlLayoutFixup(this, true, true, Controls);

            ResumeLayout();
        }

        private readonly static int MARGIN_X = (int)DisplayHelper.ScaleX(8);
        private readonly static int LISTBOX_PADDING = 2;

        private IVideoService CurrentService
        {
            get
            {
                return _currentService;
            }
            set
            {
                _currentService = value;

                _videoLoginStatusControl.Auth = _currentService.Auth;
                _videoRequestComboBox.SetEntries(_currentService.SupportedRequests);

                VideoServiceSettings serviceSettings = VideoSettings.GetServiceSettings(_currentService.Id);
                _videoRequestComboBox.SelectEntry(serviceSettings.SelectedRequestType);

                _videoLoginControl.ShowCreateMicrosoftAccountID = false;
                _videoLoginControl.SetService(_currentService);

                RefreshListBox();
            }
        }

        private IVideoService _currentService;

        public void RegisterServices(IVideoService[] services)
        {
            foreach (IVideoService service in services)
                _sidebarService.SetEntry(service, service.Image, service.ServiceName);

            _sidebarService.SelectedIndexChanged += _sidebarService_SelectedIndexChanged;
            _videoRequestComboBox.SelectedRequestTypeChanged += _videoRequestComboBox_SelectedRequestTypeChanged;
            _videoPagingControl.RefreshRequested += _videoPagingControl_RefreshRequested;
            _videoLoginStatusControl.LoginStatusChanged += _videoLoginStatusControl_LoginStatusChanged;
            _listBoxVideos.DoubleClick += _listBoxVideos_DoubleClick;
        }

        void _listBoxVideos_DoubleClick(object sender, EventArgs e)
        {
            if (_listBoxVideos.SelectedItems.Count > 0)
                OnMediaSelected();
        }

        void _videoLoginStatusControl_LoginStatusChanged(object sender, EventArgs e)
        {
            _videoPagingControl.Reset();
            RefreshListBox();
        }

        private void _videoPagingControl_RefreshRequested(object sender, EventArgs e)
        {
            RefreshListBox();
        }

        void _videoRequestComboBox_SelectedRequestTypeChanged(object sender, EventArgs e)
        {
            _videoPagingControl.Reset();
            RefreshListBox();
        }

        private void RefreshListBox()
        {
            IVideoService service = CurrentService;

            if (service.Auth.IsLoggedIn)
            {
                _videoRequestComboBox.Visible = true;
                if (_videoRequestComboBox.SelectedIndex == -1)
                    _videoRequestComboBox.SelectedIndex = 0;

                _listBoxVideos.Visible = true;
                _videoLoginControl.Visible = false;

                _listBoxVideos.Items.Clear();
                _listBoxVideos.QueryStatusText = Res.Get(StringId.Plugin_Video_Soapbox_Retrieve_Msg);
                _listBoxVideos.Update();

                int videosAvailable;
                try
                {
                    IVideo[] videos = service.GetVideos(_videoRequestComboBox.SelectedRequestType, 10000,
                                                        _videoPagingControl.NumberOfVideosPerPage,
                                                        _videoPagingControl.CurrentPage,
                                                        out videosAvailable);

                    _videoPagingControl.NumberOfVideos = videosAvailable;
                    _listBoxVideos.DisplayVideos(videos);
                    if (_listBoxVideos.Items.Count > 0)
                        _listBoxVideos.SelectedIndex = 0;
                }
                catch (Exception e)
                {
                    Trace.Write("Failed getting videos: " + e.Message);
                    _listBoxVideos.DisplayGetVideosError();
                    _listBoxVideos.Update();
                }
            }
            else
            {
                _videoRequestComboBox.Visible = false;
                _listBoxVideos.Visible = false;
                _videoLoginControl.Visible = true;
                _listBoxVideos.QueryStatusText = "";
                _videoPagingControl.Reset();
                _videoLoginControl.Clear();
            }
        }

        public override bool ValidateSelection()
        {
            VideoSettings.selectedServiceName = _sidebarService.SelectedValue.ServiceName;
            VideoServiceSettings serviceSettings = VideoSettings.GetServiceSettings(_sidebarService.SelectedValue.Id);

            if (!CurrentService.Auth.IsLoggedIn)
            {
                DisplayMessage.Show(MessageId.VideoLoginRequired, this);
                return false;
            }

            if (_listBoxVideos.SelectedItems.Count == 0)
            {
                DisplayMessage.Show(MessageId.NoSoapboxVideoSelected, this);
                return false;
            }

            serviceSettings.SelectedRequestType = _videoRequestComboBox.SelectedRequestType.TypeName;
            serviceSettings.Username = CurrentService.Auth.Username;

            return true;
        }

        public override void SaveContent(MediaSmartContent content)
        {
            IVideo video = (IVideo)_listBoxVideos.SelectedItem;
            Video contentVideo = video.GetVideo();
            ((VideoSmartContent)content).Initialize(contentVideo, _blogId);
        }

        public override void TabSelected()
        {
            if (_sidebarService.Items.Count > 0 & !_setOnce)
            {
                if (!_sidebarService.SelectEntry(VideoSettings.selectedServiceName))
                {
                    _sidebarService.SelectedIndex = 0;
                    Application.DoEvents();
                }
                _setOnce = true;
            }

            if (!CurrentService.Auth.IsLoggedIn)
            {
                _videoLoginControl.Visible = false;
                _listBoxVideos.Visible = true;
                _listBoxVideos.Items.Clear();
                _listBoxVideos.Update();
                _listBoxVideos.DisplayLoggingIn();
                _videoLoginStatusControl.UpdateStatus();
                _listBoxVideos.QueryStatusText = "";
                RefreshListBox();
            }

            if (_videoLoginControl.Visible)
                _videoLoginControl.Focus();

            _videoLoginControl.SelectUserName();
        }

        private bool _setOnce = false;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // VideoServiceVideoSource
            //
            this.Name = "VideoServiceVideoSource";
            this.ResumeLayout(false);

        }

        void IRtlAware.Layout()
        {
        }
    }
}
