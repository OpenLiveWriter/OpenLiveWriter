// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.PostEditor.ImageInsertion
{
    /// <summary>
    /// Summary description for WebImageControl.
    /// </summary>
    public class WebImageSource : InsertImageSource
    {
        private Label _labelOne;
        private TextBoxWithPaste _webImageUrl;
        private BorderControl _pictureBorder;
        private PictureBox _previewBox;
        private Button _previewButton;
        private Label _labelSize;
        private Label _fileSize;
        private bool _selected = false;
        UserControl _control;
        private int _width;
        private int _height;

        private const int WebImageUrlPadding = 6;
        private const int SizeLabelPadding = 6;

        public void Init(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public string TabName
        {
            get
            {
                return Res.Get(StringId.InsertImageInsertFromWeb);
            }
        }

        public Bitmap TabBitmap
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("ImageInsertion.Images.TabInsertFromWeb.png");
            }
        }

        public UserControl ImageSelectionControls
        {
            get
            {
                if (_control == null)
                {
                    _control = new UserControl();
                    _control.Location = new Point(0, 0);
                    _control.Size = new Size(_width, _height);
                    _control.Load += new EventHandler(_control_Load);
                    _control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
                    _control.Font = Res.DefaultFont;

                    _labelOne = new Label();
                    _labelOne.FlatStyle = FlatStyle.System;
                    _labelOne.Text = Res.Get(StringId.InsertImageImageUrl);
                    _labelOne.Location = new Point(10, 13);
                    _labelOne.Size = new Size(87, 15);
                    _labelOne.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                    _webImageUrl = new TextBoxWithPaste();
                    _webImageUrl.Location = new Point(10, 31);
                    _webImageUrl.Size = new Size(377, 20);
                    _webImageUrl.Name = "imageUrl";
                    _webImageUrl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    _webImageUrl.OnPaste += new TextBoxWithPaste.OnPasteEventHandler(this._imageUrl_Paste);
                    _webImageUrl.RightToLeft = RightToLeft.No;
                    if (BidiHelper.IsRightToLeft)
                        _webImageUrl.TextAlign = HorizontalAlignment.Right;

                    _previewButton = new Button();
                    _previewButton.Location = new Point(395, 31);
                    _previewButton.Size = new Size(75, 23);
                    _previewButton.FlatStyle = FlatStyle.System;
                    _previewButton.Text = Res.Get(StringId.InsertImagePreviewButton);
                    _previewButton.Name = "previewButton";
                    _previewButton.Click += new EventHandler(_previewButton_Click);
                    _previewButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;

                    _previewBox = new PictureBox();
                    _previewBox.BackColor = Color.White;
                    _previewBox.BorderStyle = BorderStyle.None;
                    _previewBox.SizeMode = PictureBoxSizeMode.CenterImage;
                    _previewBox.Name = "previewBox";

                    _pictureBorder = new BorderControl();
                    _pictureBorder.Location = new Point(10, 59);
                    _pictureBorder.Size = new Size(460, 283);
                    _pictureBorder.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top |
                                            AnchorStyles.Bottom;
                    _pictureBorder.Control = _previewBox;

                    _labelSize = new Label();
                    _labelSize.FlatStyle = FlatStyle.System;
                    _labelSize.Text = Res.Get(StringId.InsertImageSize);
                    _labelSize.Size = new Size(35, 15);
                    _labelSize.Location = new Point(10, 347);
                    _labelSize.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

                    _fileSize = new Label();
                    _fileSize.FlatStyle = FlatStyle.System;
                    _fileSize.Text = "";
                    _fileSize.Size = new Size(438, 15);
                    _fileSize.Location = new Point(51, 347);
                    _fileSize.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

                    _control.Controls.Add(_labelOne);
                    _control.Controls.Add(_webImageUrl);
                    _control.Controls.Add(_previewButton);
                    _control.Controls.Add(_pictureBorder);
                    _control.Controls.Add(_labelSize);
                    _control.Controls.Add(_fileSize);
                    _control.Name = "WebImageSource";
                    _control.RightToLeft = BidiHelper.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
                }
                return _control;
            }
        }

        private void _control_Load(object sender, EventArgs e)
        {
            LayoutHelper.AutoFitLabels(_labelSize);

            DisplayHelper.AutoFitSystemLabel(_labelOne, _labelOne.Width, int.MaxValue);
            DisplayHelper.AutoFitSystemButton(_previewButton, 0, int.MaxValue);

            if (_control.RightToLeft == RightToLeft.Yes)
            {
                _webImageUrl.Left = _previewButton.Right + (int)DisplayHelper.ScaleX(WebImageUrlPadding);
                _webImageUrl.Width = _pictureBorder.Right - _previewButton.Right - (int)DisplayHelper.ScaleX(WebImageUrlPadding);

                _fileSize.Left = _pictureBorder.Left;
                _fileSize.Width = _labelSize.Left - _fileSize.Left - SizeLabelPadding;
            }
            else
            {
                _webImageUrl.Left = _pictureBorder.Left;
                _webImageUrl.Width = _previewButton.Left - _webImageUrl.Left - (int)DisplayHelper.ScaleX(WebImageUrlPadding);

                _fileSize.Left = _labelSize.Right + SizeLabelPadding;
                _fileSize.Width = _pictureBorder.Right - _fileSize.Left;
            }

            BidiHelper.RtlLayoutFixup(_control);

            _webImageUrl.Select();
        }

        private void _imageUrl_Paste(object sender, TextBoxWithPaste.PasteEventArgs eventArgs)
        {
            //special case...image on clipboard
            if (_webImageUrl.Text.Trim() == String.Empty)
            {
                DataObjectMeister dataObject = new DataObjectMeister(Clipboard.GetDataObject());
                if (dataObject.HTMLData != null && dataObject.HTMLData.OnlyImagePath != null)
                {
                    _webImageUrl.Text = dataObject.HTMLData.OnlyImagePath;
                }
            }
            PopulatePreviewBox();
            _webImageUrl.SelectAll();
        }

        public void Repaint(Size newSize)
        {
            _control.Size = newSize;

        }

        public string SourceImageLink
        {
            get { return FileName; }
        }

        public void TabSelected()
        {
            _webImageUrl.Focus();
        }

        public event EventHandler OnSelectionMade;

        public string FileName
        {
            get
            {
                return UrlHelper.FixUpUrl(_webImageUrl.Text);
            }
        }

        public bool ValidateSelection()
        {
            if (!UrlHelper.IsUrl(UrlHelper.FixUpUrl(_webImageUrl.Text)))
            {
                DisplayMessage.Show(MessageId.InvalidWebImage);
                _webImageUrl.Focus();
                return false;
            }
            return true;
        }

        public bool HandleEnter(int cmdId)
        {
            int previewButtonId = (int)User32.GetDlgCtrlID(_previewButton.Handle) & 0xFFFF;
            if (previewButtonId == cmdId)
            {
                _previewButton.PerformClick();
                return true;
            }
            return false;
        }

        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
            }
        }

        private void _previewButton_Click(object sender, EventArgs e)
        {
            PopulatePreviewBox();
        }

        private void PopulatePreviewBox()
        {
            string imageUrl = _webImageUrl.Text.Trim();
            if (imageUrl != String.Empty && UrlHelper.IsUrl(imageUrl))
            {
                using (new WaitCursor())
                {
                    try
                    {
                        HttpWebResponse response = HttpRequestHelper.SendRequest(UrlHelper.FixUpUrl(imageUrl));
                        Bitmap webImage;
                        using (Stream responseStream = response.GetResponseStream())
                            webImage = new Bitmap(StreamHelper.CopyToMemoryStream(responseStream));

                        SetPreviewPicture(webImage, imageUrl);

                        if (OnSelectionMade != null)
                            OnSelectionMade(this, new EventArgs());

                    }
                    catch (Exception)
                    {
                        _previewBox.Image = null;
                        DisplayMessage.Show(MessageId.NoPreviewAvailable);
                        _webImageUrl.Focus();
                    }
                }
            }
            else
            {
                _previewBox.Image = null;
                DisplayMessage.Show(MessageId.NoPreviewAvailable);
                _webImageUrl.Focus();
            }
        }

        private void SetPreviewPicture(Bitmap image, string filename)
        {
            int maxWidth = _previewBox.Width - 2;
            int maxHeight = _previewBox.Height - 2;
            bool scaled = true;
            int currentWidth = image.Width;
            int currentHeight = image.Height;
            double ratio = 1.00;
            //if height and width are too big
            if (currentWidth > maxWidth && currentHeight > maxHeight)
            {
                //size according to the one that is more off of the available area
                if ((maxWidth / currentWidth) < (maxHeight / currentHeight))
                {
                    ratio = (double)maxWidth / (double)currentWidth;
                }
                else
                {
                    ratio = (double)maxHeight / (double)currentHeight;
                }
            }
            //if just width
            else if (currentWidth > maxWidth)
            {
                ratio = (double)maxWidth / (double)currentWidth;
            }
            //if just height
            else if (currentHeight > maxHeight)
            {
                ratio = (double)maxHeight / (double)currentHeight;
            }
            //else fine
            else
            {
                scaled = false;
            }

            ImageFormat format;
            string fileExt;
            ImageHelper2.GetImageFormat(filename, out fileExt, out format);
            int newWidth = (int)(currentWidth * ratio);
            int newHeight = (int)(currentHeight * ratio);

            Bitmap newImage = ImageHelper2.CreateResizedBitmap(image, newWidth, newHeight, format);

            //if image is small enough, add border
            if (newWidth <= maxWidth && newHeight <= maxHeight)
            {
                Bitmap borderPic = new Bitmap(newWidth + 2, newHeight + 2);
                //Get a graphics object for it
                using (Graphics g = Graphics.FromImage(borderPic))
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;

                    int R = SystemColors.Control.R;
                    int G = SystemColors.Control.G;
                    int B = SystemColors.Control.B;
                    Color dark = Color.FromArgb((int)(R * 0.9), (int)(G * 0.9), (int)(B * 0.9));
                    //draw the border
                    g.FillRectangle(new SolidBrush(dark), new Rectangle(0, 0, borderPic.Width, 1));
                    g.FillRectangle(new SolidBrush(dark), new Rectangle(0, borderPic.Height - 1, borderPic.Width, 1));
                    g.FillRectangle(new SolidBrush(dark), new Rectangle(0, 0, 1, borderPic.Height));
                    g.FillRectangle(new SolidBrush(dark), new Rectangle(borderPic.Width - 1, 0, 1, borderPic.Height));

                    //draw our image back in the middle
                    g.DrawImage(newImage, new Rectangle(1, 1, newWidth, newHeight), 0, 0, newWidth, newHeight, GraphicsUnit.Pixel);
                }
                _previewBox.Image = borderPic;
            }
            else
            {
                _previewBox.Image = newImage;
            }

            string picsize;
            if (!scaled)
                picsize = MakeDimensions(currentWidth, currentHeight);
            else
            {
                picsize = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.InsertImageDimensionsFormatScaled),
                                  MakeDimensions(currentWidth, currentHeight));
            }
            _fileSize.Text = picsize;
        }

        private static string MakeDimensions(int width, int height)
        {
            return string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.InsertImageDimensionsFormat), width, height);
        }
    }
}

