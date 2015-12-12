// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.Video.VideoService;
using OpenLiveWriter.PostEditor.Video.YouTube;

namespace OpenLiveWriter.PostEditor.Video.VideoListBox
{
    internal class VideoListBoxControl : ListBox
    {
        #region Private Members

        private VideoThumbNailManager _thumbnailManager;

        #endregion

        #region Initialization and Cleanup

        public void Initialize()
        {
            // set options for list box
            DrawMode = DrawMode.OwnerDrawFixed;
            SelectionMode = SelectionMode.One;
            HorizontalScrollbar = false;
            IntegralHeight = false;

            // create the thumbnail manager
            _thumbnailManager = new VideoThumbNailManager(this);
            _thumbnailManager.ThumbnailDownloadCompleted += _thumbnailManager_ThumbnailDownloadCompleted;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_thumbnailManager != null)
                {
                    _thumbnailManager.ThumbnailDownloadCompleted -= _thumbnailManager_ThumbnailDownloadCompleted;
                    _thumbnailManager.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Public Interface

        public void DisplayVideos(IVideo[] videos)
        {
            Items.Clear();
            ItemHeight = CalculateItemHeight();

            // clear all pending downloads
            _thumbnailManager.ClearPendingDownloads();

            // initiate a download for each video
            foreach (IVideo video in videos)
                _thumbnailManager.DownloadThumbnail(video);

            // add the videos to the list
            foreach (IVideo video in videos)
                Items.Add(video);

            // update status text
            if (Items.Count == 0)
                DisplayNoVideosFound();
            else
                QueryStatusText = null;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string QueryStatusText
        {
            get
            {
                return QueryStatusLabel.Text;
            }
            set
            {
                if (value != null)
                {
                    QueryStatusLabel.Text = value;
                    QueryStatusLabel.Width = Width - 5;
                    LayoutHelper.NaturalizeHeight(QueryStatusLabel);
                    QueryStatusLabel.Left = Left + Width / 2 - QueryStatusLabel.Width / 2;
                    QueryStatusLabel.Top = Top + (Height / 2);
                    QueryStatusLabel.Visible = true;
                    QueryStatusLabel.BringToFront();
                    QueryStatusLabel.Update();
                }
                else
                {
                    QueryStatusLabel.Text = String.Empty;
                    QueryStatusLabel.Visible = false;
                    QueryStatusLabel.SendToBack();
                    QueryStatusLabel.Update();
                }
            }
        }

        public void DisplayGetVideosError()
        {
            QueryStatusText = Res.Get(StringId.VideoGetVideosError);
        }

        public void DisplayLoggingIn()
        {
            QueryStatusText = Res.Get(StringId.Plugin_Video_Soapbox_LoggingIn);
        }

        public void DisplayNoVideosFound()
        {
            QueryStatusText = Res.Get(StringId.Plugin_Video_Soapbox_None_Found);
        }

        public void DisplayWebError()
        {
            QueryStatusText = Res.Get(StringId.Plugin_Video_Soapbox_Web_Error);
        }

        public void DisplaySearchError()
        {
            QueryStatusText = Res.Get(StringId.Plugin_Video_Soapbox_Soap_Error);
        }

        #endregion

        #region Hyperlink Mouse Handling

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor = Cursors.Default;
            _preventSelectionPainting = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            _lastMouseLocation = new PointF(e.X, e.Y);

            if (MouseInHyperlink)
            {
                Cursor = Cursors.Hand;
                _preventSelectionPainting = true;
            }
            else
            {
                Cursor = Cursors.Default;
                _preventSelectionPainting = false;
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            _preventSelectionPainting = false;
        }

        private bool MouseInHyperlink
        {
            get
            {
                return GetMouseOverVideo() != null;
            }
        }

        private IVideo GetMouseOverVideo()
        {
            // handle no videos displayed
            if (Items.Count == 0)
                return null;

            using (Graphics g = CreateGraphics())
            {
                const int LEFT_MARGIN = HORIZONTAL_INSET + THUMBNAIL_IMAGE_WIDTH + HORIZONTAL_INSET;

                int maxItemsDisplayed = Height / ItemHeight;
                int itemsDisplayed = Math.Min(maxItemsDisplayed, Items.Count - TopIndex);

                for (int i = 0; i < itemsDisplayed; i++)
                {
                    // get the current video
                    IVideo currentVideo = Items[TopIndex + i] as IVideo;

                    // see if the mouse lies in the video's title range
                    PointF titleLocation = new Point(LEFT_MARGIN, (i * ItemHeight) + VERTICAL_INSET);
                    SizeF titleSize = g.MeasureString(currentVideo.Title, Font, Width - Convert.ToInt32(titleLocation.X));
                    RectangleF titleRectangle = new RectangleF(titleLocation, titleSize);
                    if (titleRectangle.Contains(_lastMouseLocation))
                    {
                        return currentVideo;
                    }
                };

                // wasn't in hyperlink
                return null;
            }
        }

        private PointF _lastMouseLocation = Point.Empty;
        private bool _preventSelectionPainting = false;

        #endregion

        #region Painting

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // screen invalid drawing states
            if (DesignMode || e.Index == -1)
                return;

            // get video we are rendering
            IVideo video = Items[e.Index] as IVideo;

            // determine state
            bool selected = ((e.State & DrawItemState.Selected) > 0) && !_preventSelectionPainting;

            // calculate colors
            Color textColor;
            if (selected)
            {
                if (Focused)
                {
                    textColor = SystemColors.HighlightText;
                }
                else
                {
                    textColor = SystemColors.ControlText;
                }
            }
            else
            {
                textColor = SystemColors.ControlText;
            }
            Color previewColor = Color.FromArgb(200, textColor);

            BidiGraphics g = new BidiGraphics(e.Graphics, e.Bounds);

            // setup standard string format
            TextFormatFlags ellipsesStringFormat = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.ExpandTabs | TextFormatFlags.WordEllipsis;

            // draw background
            e.DrawBackground();
            //using (SolidBrush solidBrush = new SolidBrush(backColor))
            //    g.FillRectangle(solidBrush, e.Bounds);

            // draw the thumbnail image
            Rectangle thumbnailRect = new Rectangle(e.Bounds.Left + ScaleX(HORIZONTAL_INSET), e.Bounds.Top + ScaleY(VERTICAL_INSET), THUMBNAIL_IMAGE_WIDTH, THUMBNAIL_IMAGE_HEIGHT);
            VideoThumbnail thumbnail = _thumbnailManager.GetThumbnail(video);
            thumbnail.Draw(g, e.Font, thumbnailRect);

            // calculate standard text drawing metrics
            int leftMargin = ScaleX(HORIZONTAL_INSET) + THUMBNAIL_IMAGE_WIDTH + ScaleX(HORIZONTAL_INSET);
            int topMargin = e.Bounds.Top + ScaleY(VERTICAL_INSET);
            int fontHeight = g.MeasureText(video.Title, e.Font).Height;

            // draw title and duration
            int titleWidth;
            Rectangle durationRectangle;
            using (Font hyperlinkFont = new Font(e.Font, FontStyle.Underline))
            {
                titleWidth = e.Bounds.Right - ScaleX(HORIZONTAL_INSET) - leftMargin;
                Rectangle titleRectangle = new Rectangle(leftMargin, topMargin, titleWidth, fontHeight);

                string title = video.Title ?? "";

                g.DrawText(title,
                    hyperlinkFont,
                    titleRectangle, selected ? textColor : SystemColors.HotTrack, ellipsesStringFormat);
            }

            using (Font durationFont = new Font(e.Font, FontStyle.Regular)) // was bold
            {
                durationRectangle = new Rectangle(leftMargin, topMargin + fontHeight + 3, titleWidth, fontHeight);

                string duration = String.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", video.LengthSeconds / 60, video.LengthSeconds % 60);

                g.DrawText(
                    duration,
                    durationFont,
                    durationRectangle, textColor, ellipsesStringFormat);
            }

            // draw description

            // calculate layout rectangle
            Rectangle layoutRectangle = new Rectangle(
                leftMargin,
                durationRectangle.Bottom + ScaleY(VERTICAL_INSET),
                e.Bounds.Width - leftMargin - ScaleX(HORIZONTAL_INSET),
                e.Bounds.Bottom - ScaleY(VERTICAL_INSET) - durationRectangle.Bottom - ScaleY(VERTICAL_INSET));

            // draw description
            g.DrawText(
                video.Description,
                e.Font, layoutRectangle, previewColor, ellipsesStringFormat);

            // draw bottom-line if necessary
            if (!selected)
            {
                using (Pen pen = new Pen(SystemColors.ControlLight))
                    g.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - ScaleY(1), e.Bounds.Right, e.Bounds.Bottom - ScaleY(1));
            }

            // focus rectange if necessary
            e.DrawFocusRectangle();
        }

        #region High DPI Scaling
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScale(float dx, float dy)
        {
            scale = new PointF(scale.X * dx, scale.Y * dy);
        }
        private PointF scale = new PointF(1f, 1f);

        protected int ScaleX(int x)
        {
            return (int)(x * scale.X);
        }

        protected int ScaleY(int y)
        {
            return (int)(y * scale.Y);
        }
        #endregion

        private int CalculateItemHeight()
        {
            return ScaleY(VERTICAL_INSET) + THUMBNAIL_IMAGE_HEIGHT + ScaleY(VERTICAL_INSET);
        }

        private void _thumbnailManager_ThumbnailDownloadCompleted(IVideo listBoxVideo)
        {
            // if the listBoxVideo whose download completed is visible then invalidate its rectangle

            int maxItemsDisplayed = Height / ItemHeight;
            int itemsDisplayed = Math.Min(maxItemsDisplayed, Items.Count - TopIndex);

            for (int i = 0; i < itemsDisplayed; i++)
            {
                // get the current listBoxVideo
                IVideo currentListBoxVideo = Items[TopIndex + i] as IVideo;
                if (listBoxVideo == currentListBoxVideo)
                {
                    // invalidate just the rectangle containing the listBoxVideo
                    Rectangle invalidateRect = new Rectangle(
                        HORIZONTAL_INSET,
                        (i * ItemHeight) + VERTICAL_INSET,
                        THUMBNAIL_IMAGE_WIDTH,
                        THUMBNAIL_IMAGE_HEIGHT);

                    Invalidate(invalidateRect);

                    break;
                }
            };
        }

        private Label QueryStatusLabel
        {
            get
            {
                if (_queryStatusLabel == null)
                {
                    Label label = new Label();
                    label.Size = new Size(Width - 5, 4 * Convert.ToInt32(Font.GetHeight()));
                    label.TextAlign = ContentAlignment.MiddleCenter;
                    label.BackColor = BackColor;
                    label.ForeColor = SystemColors.ControlDarkDark;
                    label.Font = Res.GetFont(FontSize.XLarge, FontStyle.Regular);
                    label.Text = String.Empty;
                    label.Visible = false;
                    //label.FlatStyle = FlatStyle.System;
                    label.TabStop = false;
                    label.Location = new Point(
                        Left + (Width / 2) - (label.Width / 2),
                        Top + (Height / 2) - (label.Height / 2) - 30);
                    Parent.Controls.Add(label);
                    _queryStatusLabel = label;
                }
                return _queryStatusLabel;
            }
        }
        private Label _queryStatusLabel;

        // item metrics
        private const int VERTICAL_INSET = 5;
        private const int HORIZONTAL_INSET = 5;
        private const int THUMBNAIL_IMAGE_WIDTH = 130;
        private const int THUMBNAIL_IMAGE_HEIGHT = 100;

        #endregion

    }

}
