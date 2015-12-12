// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;

namespace OpenLiveWriter.PostEditor.Video
{
    internal class VideoWebPreviewCommand : Command
    {
        private bool _publishCompleted = false;
        public bool PublishCompleted
        {
            get { return _publishCompleted; }

            set
            {
                if (_publishCompleted != value)
                {
                    _publishCompleted = value;
                    UpdateInvalidationState(PropertyKeys.Label, InvalidationState.Pending);
                    UpdateInvalidationState(PropertyKeys.LabelDescription, InvalidationState.Pending);
                    Invalidate();
                }
            }

        }

        internal VideoWebPreviewCommand()
            : base(CommandId.VideoWebPreview)
        {
        }

        public override string LabelTitle
        {
            get
            {
                return _publishCompleted ? base.LabelTitle : Res.Get(StringId.Plugin_Video_Editor_Cancel);
            }
        }

        public override string TooltipDescription
        {
            get
            {
                return _publishCompleted ? base.TooltipDescription : String.Empty;
            }
        }
        // @RIBBON TODO: Other overrides for image/string props.
        // Have cancel publish use a placeholder icon for now.
    }

    internal class VideoEditorControl : AlignmentMarginContentEditor
    {
        private System.ComponentModel.IContainer components;

        private VideoSmartContent _VideoContent;
        private ISmartContentEditorSite _contentEditorSite;

        public VideoEditorControl(ISmartContentEditorSite contentEditorSite)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            components = null;

            _contentEditorSite = contentEditorSite;

            InitializeCommands();
        }

        private VideoWebPreviewCommand commandVideoWebPreview;
        private Command commandVideoWidescreenAspectRatio;
        private Command commandVideoStandardAspectRatio;

        protected void InitializeCommands()
        {
            CommandManager cm = ((ICommandManagerHost)_contentEditorSite).CommandManager;

            commandVideoWebPreview = new VideoWebPreviewCommand();
            commandVideoWebPreview.Execute += VideoWebPreview_Execute;
            cm.Add(commandVideoWebPreview);

            commandVideoWidescreenAspectRatio = new Command(CommandId.VideoWidescreenAspectRatio);
            commandVideoWidescreenAspectRatio.Execute += VideoAspectRatio_Execute;
            commandVideoWidescreenAspectRatio.Tag = VideoAspectRatioType.Widescreen;
            cm.Add(commandVideoWidescreenAspectRatio);

            commandVideoStandardAspectRatio = new Command(CommandId.VideoStandardAspectRatio);
            commandVideoStandardAspectRatio.Execute += VideoAspectRatio_Execute;
            commandVideoStandardAspectRatio.Tag = VideoAspectRatioType.Standard;
            cm.Add(commandVideoStandardAspectRatio);

            InitializeAlignmentMarginCommands(((ICommandManagerHost)_contentEditorSite).CommandManager);
            cm.Add(new GroupCommand(CommandId.FormatVideoGroup, commandVideoWebPreview));
        }

        private void VideoWebPreview_Execute(object sender, EventArgs e)
        {
            if (commandVideoWebPreview.PublishCompleted)
            {
                ShellHelper.LaunchUrl(_VideoContent.Url ?? String.Empty);
            }
            else
            {
                CancelPublish();
            }
        }

        private void CancelPublish()
        {
            if (((IInternalContent)_VideoContent.SmartContent).ObjectState is IStatusWatcher)
            {
                ((IStatusWatcher)((IInternalContent)_VideoContent.SmartContent).ObjectState).CancelPublish();
            }

            _VideoContent.Delete();
            ((ContentSourceSidebarControl)_contentEditorSite).RemoveSelectedContent();
        }

        private void VideoAspectRatio_Execute(object sender, EventArgs e)
        {
            Command commandExecuted = (Command)sender;
            VideoAspectRatioType newAspectRatioType = (VideoAspectRatioType)commandExecuted.Tag;

            if (_VideoContent.AspectRatioType != newAspectRatioType)
            {
                _VideoContent.AspectRatioType = newAspectRatioType;

                OnContentEdited();
            }

            UpdateAspectRatio();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            VirtualTransparency.VirtualPaint(this, pevent);
        }

        protected override void OnSelectedContentChanged()
        {
            // make connection to settings and elements
            _VideoContent = new VideoSmartContent(SelectedContent);

            base.OnSelectedContentChanged();

            // update the settings UI
            InitializeSettingsUI();

            // force a layout for dynamic control flow
            PerformLayout();

            UpdatePadding();
            // WinLive 121985: Cannot select the 'Enter Video Caption Here' edit control.
            //OnContentEdited();

            UpdateAspectRatio();
        }

        public override bool ContentEnabled
        {
            set
            {
                base.ContentEnabled = value;
                commandVideoWebPreview.Enabled = value;
                commandVideoWidescreenAspectRatio.Enabled = value;
                commandVideoStandardAspectRatio.Enabled = value;

                if (value == false)
                {
                    commandVideoWidescreenAspectRatio.Latched = false;
                    commandVideoStandardAspectRatio.Latched = false;
                }
            }
        }

        private void UpdateVideoSizeDisplay()
        {
            _contentEditorSite.UpdateStatusBar(String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.Plugin_Videos_Video_Size), _VideoContent.HtmlSize.Width, _VideoContent.HtmlSize.Height));
        }

        private void UpdateAspectRatio()
        {
            // Update the aspect ratio buttons.
            commandVideoWidescreenAspectRatio.Latched = _VideoContent.AspectRatioType == VideoAspectRatioType.Widescreen;
            commandVideoStandardAspectRatio.Latched = _VideoContent.AspectRatioType == VideoAspectRatioType.Standard;

            // The ribbon keeps internal state about if the buttons should be latched or not, so we invalidate the
            // state to make sure it calls back into us to get the current values.
            commandVideoWidescreenAspectRatio.Invalidate(new[] { PropertyKeys.BooleanValue });
            commandVideoStandardAspectRatio.Invalidate(new[] { PropertyKeys.BooleanValue });
        }

        private void UpdatePadding()
        {
            // update underlying values
            Padding margin = ContentMargin;
            bool paddingChanged = _VideoContent.LayoutStyle.TopMargin != Convert.ToInt32(margin.Top) |
                                  _VideoContent.LayoutStyle.LeftMargin != Convert.ToInt32(margin.Left) |
                                  _VideoContent.LayoutStyle.BottomMargin != Convert.ToInt32(margin.Bottom) |
                                  _VideoContent.LayoutStyle.RightMargin != Convert.ToInt32(margin.Right);

            _VideoContent.LayoutStyle.TopMargin = Convert.ToInt32(margin.Top);
            _VideoContent.LayoutStyle.LeftMargin = Convert.ToInt32(margin.Left);
            _VideoContent.LayoutStyle.BottomMargin = Convert.ToInt32(margin.Bottom);
            _VideoContent.LayoutStyle.RightMargin = Convert.ToInt32(margin.Right);

            // invalidate
            if (paddingChanged)
                OnContentEdited();
        }

        private void InitializeSettingsUI()
        {
            Padding margin = new Padding(_VideoContent.LayoutStyle.LeftMargin,
                                               _VideoContent.LayoutStyle.TopMargin,
                                               _VideoContent.LayoutStyle.RightMargin,
                                               _VideoContent.LayoutStyle.BottomMargin);

            SetAlignmentMargin(margin, _VideoContent.LayoutStyle.Alignment);

            // video size in status-bar
            UpdateVideoSizeDisplay();

            IStatusWatcher statusWatcher = ((IInternalContent)_VideoContent.SmartContent).ObjectState as IStatusWatcher;
            if (statusWatcher != null && statusWatcher.IsCancelable)
            {
                commandVideoWebPreview.PublishCompleted = false;
            }
            else
            {
                commandVideoWebPreview.PublishCompleted = true;
            }
        }

        protected override void OnMarginChanged(object sender, EventArgs e)
        {
            UpdatePadding();
            base.OnMarginChanged(sender, e);
            OnContentEdited();
        }

        protected override void OnAlignmentChanged(object sender, EventArgs e)
        {
            _VideoContent.LayoutStyle.Alignment = ContentAlignment;
            base.OnAlignmentChanged(sender, e);
            OnContentEdited();
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion
    }
}
