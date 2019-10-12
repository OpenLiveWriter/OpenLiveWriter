// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BrowserControl;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.PostEditor.ImageInsertion;
using mshtml;

namespace OpenLiveWriter.PostEditor.Video
{
    /// <summary>
    /// Summary description for WebVideoSource.
    /// </summary>
    public class WebVideoSource : MediaTab, IRtlAware
    {
        private Video _video = null;
        private bool firstRun = true;
        private string htmlPath = null;

        private TextBoxWithPaste videoCode;
        private BorderControl pictureBorder;
        private WebBrowser previewBox;
        private Button previewButton;
        private Label lblSize;
        private Label lblService;
        private System.Windows.Forms.Label lblVideoCode;

        public WebVideoSource()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            previewButton.Text = Res.Get(StringId.Plugin_Video_Web_Video_Preview_Button);
            lblSize.Text = Res.Get(StringId.Plugin_Video_Web_Video_Size_Blank);
            lblService.Text = Res.Get(StringId.Plugin_Video_Web_Video_Service_Blank);
            lblVideoCode.Text = Res.Get(StringId.Plugin_Video_Web_Video_Enter_Prompt);

            videoCode.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.Plugin_Video_Web_Video_Enter_Prompt));

            this.TabText = Res.Get(StringId.Plugin_Video_Web_Video_Tab_Name);
            this.TabBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Video.Images.InsertVideoFromWebTabIcon.png", false);
            this.BackColor = SystemColors.Control;

            previewBox.ScriptErrorsSuppressed = true;
            previewBox.ScrollBarsEnabled = false;
            previewBox.IsWebBrowserContextMenuEnabled = false;
            previewBox.WebBrowserShortcutsEnabled = false;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int delta = -previewButton.Width + DisplayHelper.AutoFitSystemButton(previewButton, previewButton.Width, int.MaxValue);
            videoCode.Width -= delta;
            videoCode.RightToLeft = System.Windows.Forms.RightToLeft.No;

            if (BidiHelper.IsRightToLeft)
            {
                lblService.SizeChanged += new EventHandler(lblService_SizeChanged);
                videoCode.TextAlign = HorizontalAlignment.Right;
            }
            else
                lblSize.SizeChanged += new EventHandler(lblSize_SizeChanged);

            int oldTop = pictureBorder.Top;
            pictureBorder.Top = videoCode.Bottom + 6;
            pictureBorder.Height -= pictureBorder.Top - oldTop;

            BidiHelper.RtlLayoutFixup(this, true, true, Controls);
        }

        public override List<Control> GetAccessibleControls()
        {
            List<Control> controls = new List<Control>();
            controls.Add(videoCode);
            controls.Add(previewButton);
            controls.Add(previewBox);
            return controls;
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.videoCode = new OpenLiveWriter.Controls.TextBoxWithPaste();
            this.previewButton = new System.Windows.Forms.Button();
            this.previewBox = new System.Windows.Forms.WebBrowser();
            this.pictureBorder = new OpenLiveWriter.ApplicationFramework.BorderControl();
            this.lblSize = new System.Windows.Forms.Label();
            this.lblService = new System.Windows.Forms.Label();
            this.lblVideoCode = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // videoCode
            //
            this.videoCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.videoCode.Location = new System.Drawing.Point(8, 26);
            this.videoCode.Name = "videoCode";
            this.videoCode.Size = new System.Drawing.Size(185, 20);
            this.videoCode.TabIndex = 15;
            this.videoCode.DoubleClick += new System.EventHandler(this.VideoCode_Enter);
            this.videoCode.TextChanged += new System.EventHandler(this.videoCode_TextChanged);
            this.videoCode.Enter += new System.EventHandler(this.VideoCode_Enter);
            //
            // previewButton
            //
            this.previewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.previewButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.previewButton.Location = new System.Drawing.Point(197, 26);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(75, 23);
            this.previewButton.TabIndex = 16;
            this.previewButton.Text = "&Preview";
            this.previewButton.Click += new System.EventHandler(this._previewButton_Click);
            //
            // previewBox
            //
            this.previewBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.previewBox.Location = new System.Drawing.Point(0, 0);
            this.previewBox.Name = "previewBox";
            this.previewBox.Size = new System.Drawing.Size(259, 204);
            this.previewBox.TabIndex = 0;
            this.previewBox.TabStop = false;
            //
            // pictureBorder
            //
            this.pictureBorder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBorder.AutoHeight = false;
            this.pictureBorder.BottomInset = 0;
            this.pictureBorder.Control = this.previewBox;
            this.pictureBorder.LeftInset = 0;
            this.pictureBorder.Location = new System.Drawing.Point(8, 50);
            this.pictureBorder.Name = "pictureBorder";
            this.pictureBorder.RightInset = 0;
            this.pictureBorder.Size = new System.Drawing.Size(263, 208);
            this.pictureBorder.SuppressBottomBorder = false;
            this.pictureBorder.TabIndex = 18;
            this.pictureBorder.TabStop = false;
            this.pictureBorder.ThemeBorder = false;
            this.pictureBorder.TopInset = 0;
            //
            // lblSize
            //
            this.lblSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSize.AutoSize = true;
            this.lblSize.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSize.Location = new System.Drawing.Point(8, 262);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(30, 13);
            this.lblSize.TabIndex = 2;
            this.lblSize.Text = "Size:";
            //
            // lblService
            //
            this.lblService.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblService.AutoSize = true;
            this.lblService.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblService.Location = new System.Drawing.Point(112, 262);
            this.lblService.Name = "lblService";
            this.lblService.Size = new System.Drawing.Size(46, 13);
            this.lblService.TabIndex = 1;
            this.lblService.Text = "Service:";
            //
            // lblVideoCode
            //
            this.lblVideoCode.Location = new System.Drawing.Point(8, 8);
            this.lblVideoCode.Name = "lblVideoCode";
            this.lblVideoCode.Size = new System.Drawing.Size(349, 15);
            this.lblVideoCode.TabIndex = 0;
            this.lblVideoCode.Text = "&Video URL or Embed:";
            //
            // WebVideoSource
            //
            this.Controls.Add(this.lblVideoCode);
            this.Controls.Add(this.lblService);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.videoCode);
            this.Controls.Add(this.previewButton);
            this.Controls.Add(this.pictureBorder);
            this.Name = "WebVideoSource";
            this.Size = new System.Drawing.Size(279, 279);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public void Reset()
        {
            videoCode.Text = String.Empty;
            lblSize.Text = Res.Get(StringId.Plugin_Video_Web_Video_Size_Blank);
            lblService.Text = Res.Get(StringId.Plugin_Video_Web_Video_Service_Blank);
            videoCode.Focus();
            _video = null;
            firstRun = false;
            previewBox.Navigate("about:blank");
        }

        private void videoCode_TextChanged(object sender, EventArgs e)
        {
            PopulatePreviewBox();
        }

        public override void TabSelected()
        {
            if (firstRun)
                Reset();
            videoCode.Select();
        }

        public override bool ValidateSelection()
        {
            if (_video == null)
            {
                string input = videoCode.Text.Trim();
                try
                {
                    _video = VideoProviderManager.FindVideo(input);
                }
                catch (VideoUrlConvertException)
                {
                    DisplayHtml(Res.Get(StringId.VideoUrlConvertError), CreateErrorHtml);
                    return false;
                }

                if (_video == null)
                {
                    DisplayHtml(Res.Get(StringId.Plugin_Video_Cannot_Parse_Url_Message), CreateErrorHtml);
                    return false;
                }
            }

            IViewObject element = GetIViewObjectElement(previewBox.Document.Body);

            // The object doesnt cant have a snapshot taken of it, but we should still allow it to
            // be inserted, though on some providers this means it might be stripped.
            // NOTE: We skip this behavior on IE9+ because of WinLive 589461.
            if (element == null || ApplicationEnvironment.BrowserVersion.Major >= 9)
            {
                _video.Snapshot = null;
                return true;
            }

            try
            {
                _video.Snapshot = HtmlScreenCaptureCore.TakeSnapshot(element, _video.Width, _video.Height);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to take video snapshot: " + ex);
                _video.Snapshot = null;
            }

            return true;
        }

        private static IViewObject GetIViewObjectElement(HtmlElement htmlElement)
        {
            HtmlElementCollection collection = htmlElement.GetElementsByTagName("embed");

            Trace.Assert(collection.Count == 0 || collection.Count == 1, "More then one embed or object found: " + htmlElement.InnerHtml);

            if (collection.Count > 0)
            {
                IViewObject element = collection[0].DomElement as IViewObject;
                if (element != null)
                    return element;
            }

            collection = htmlElement.GetElementsByTagName("object");

            Trace.Assert(collection.Count == 0 || collection.Count == 1, "More then one embed or object found: " + htmlElement.InnerHtml);

            if (collection.Count > 0)
            {
                return collection[0].DomElement as IViewObject;
            }

            return null;
        }

        private void _previewButton_Click(object sender, EventArgs e)
        {
            previewBox.Refresh(WebBrowserRefreshOption.Completely);
        }

        public override void SaveContent(MediaSmartContent content)
        {
            ((VideoSmartContent)content).Initialize(_video, _blogId);
        }

        private void PopulatePreviewBox()
        {
            _video = null;
            string input = videoCode.Text.Trim();

            try
            {
                _video = VideoProviderManager.FindVideo(input);
            }
            catch (VideoUrlConvertException)
            {
                DisplayHtml(Res.Get(StringId.VideoUrlConvertError), CreateErrorHtml);
                return;
            }

            if (_video != null)
            {
                lblSize.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.Plugin_Video_Web_Video_Size), _video.Width, _video.Height);
                lblService.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.Plugin_Video_Web_Video_Provider),
                                                _video.Provider != null ? _video.Provider.ServiceName : Res.Get(StringId.Plugin_Video_Unknown_Provider));

                DisplayHtml(
                    VideoProvider.GenerateEmbedHtml(_video.EditorFormat, _video.Id,
                                                    new Size(_video.Width, _video.Height)), CreateEmbedHtml);
            }
            else
            {
                DisplayHtml(Res.Get(StringId.Plugin_Video_Cannot_Parse_Url_Message), CreateErrorHtml);
                lblSize.Text = Res.Get(StringId.Plugin_Video_Web_Video_Size_Blank);
                lblService.Text = Res.Get(StringId.Plugin_Video_Web_Video_Service_Blank);
            }
        }

        private void DisplayHtml(string html, Formatter formatter)
        {
            if (htmlPath == null)
                htmlPath = TempFileManager.Instance.CreateTempFile("video.html");

            html = formatter(html);

            html = HTMLDocumentHelper.AddMarkOfTheWeb(html, "about:internet");

            FileHelper.WriteFile(htmlPath, html, false, Encoding.UTF8);

            previewBox.Navigate(htmlPath);
        }

        private delegate string Formatter(string t);

        private string CreateErrorHtml(string text)
        {
            string direction = "ltr";
            if (BidiHelper.IsRightToLeft)
                direction = "rtl";

            return CreateHtml(String.Format(CultureInfo.InvariantCulture, "<div dir='{3}' style='text-align: center;height: {1}px;position: relative;'><div style='position: relative;top: 50%;'><font face=\"{2}\">{0}</font></div></div>", text, previewBox.Height, Res.DefaultFont.Name, direction), "FFFFFF");
        }

        private string CreateEmbedHtml(string embed)
        {
            return CreateHtml(String.Format(CultureInfo.InvariantCulture, "<div style='position: relative;top: {1}px;left: {2}px;'>{0}</div>", embed, (previewBox.Height - _video.Height) / 2, (previewBox.Width - _video.Width) / 2), "999999");
        }

        private static string CreateHtml(string html, string color)
        {
            return String.Format(CultureInfo.InvariantCulture, "<!DOCTYPE html><html><head><meta content='IE=Edge' http-equiv='X-UA-Compatible'/></head><body style='margin: 0; padding: 0;background-color: #{1}'>{0}</body></html>", html, color);
        }

        private void lblService_SizeChanged(object sender, EventArgs e)
        {
            lblSize.Left = Right - (int)DisplayHelper.ScaleX(8) - lblSize.Width;
            lblService.Left = lblSize.Left - (int)DisplayHelper.ScaleX(8) - lblService.Width;
        }

        private void lblSize_SizeChanged(object sender, EventArgs e)
        {
            lblService.Left = lblSize.Right + (int)DisplayHelper.ScaleX(8);
        }

        private void VideoCode_Enter(object sender, EventArgs e)
        {
            videoCode.SelectAll();
        }

        void IRtlAware.Layout()
        {
        }
    }
}

