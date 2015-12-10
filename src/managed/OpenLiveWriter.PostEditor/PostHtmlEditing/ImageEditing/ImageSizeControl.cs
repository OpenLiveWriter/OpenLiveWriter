// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageSizeControl.
    /// </summary>
    public class ImageSizeControl : UserControl
    {
        private TextBox textBoxWidth;
        private TextBox textBoxHeight;
        private Label label1;
        private Label label2;
        private CheckBox cbConstrainRatio;
        private ImageSizePickerControl imageSizePickerControl1;
        private CustomizeButton buttonCustomize;
        private ToolTip2 toolTip;
        private System.ComponentModel.IContainer components;

        public ImageSizeControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.label1.Text = Res.Get(StringId.WidthColon);
            this.label2.Text = Res.Get(StringId.HeightColon);
            this.cbConstrainRatio.Text = Res.Get(StringId.ImgSBLockRatio);
            this.toolTip.SetToolTip(buttonCustomize, Res.Get(StringId.ImgSBCustomize));
            buttonCustomize.AccessibleName = Res.Get(StringId.ImgSBCustomize);
            buttonCustomize.AccessibleDescription = Res.Get(StringId.ImgSBCustomize);
            this.imageSizePickerControl1.Resynchronize(new GetScaledImageSizeDelegate(GetScaledImageSize));
        }

        protected override void OnValidating(CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxWidth.Text) || string.IsNullOrEmpty(textBoxHeight.Text))
            {
                if (string.IsNullOrEmpty(textBoxWidth.Text))
                    textBoxWidth.Select();
                else
                    textBoxHeight.Select();

                DisplayMessage.Show(MessageId.WidthHeightRequired, this);
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Removes mnemonic hotkeys from the the control labels.
        /// </summary>
        public void RemoveMnemonics()
        {
            label1.Text = label1.Text.Replace("&", "");
            label2.Text = label2.Text.Replace("&", "");
            cbConstrainRatio.Text = cbConstrainRatio.Text.Replace("&", "");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LayoutHelper.AutoFitLabels(label1, label2);
            LayoutHelper.DistributeHorizontally(8, label1, label2);
            LayoutHelper.DistributeHorizontally(8, textBoxWidth, textBoxHeight);
            if (label2.Left > textBoxHeight.Left)
                textBoxHeight.Left = label2.Left;
            else
                label2.Left = textBoxHeight.Left;

            textBoxWidth.Left = label1.Left;
            textBoxHeight.Left = label2.Left;

            LayoutHelper.NaturalizeHeightAndDistribute(3, new ControlGroup(imageSizePickerControl1, buttonCustomize), new ControlGroup(label1, label2));
            LayoutHelper.NaturalizeHeightAndDistribute(3, new ControlGroup(label1, label2), new ControlGroup(textBoxHeight, textBoxWidth), cbConstrainRatio);

        }

        public int PreferredHeight
        {
            get
            {
                return cbConstrainRatio.Bottom;
            }
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageSizeControl));
            this.textBoxWidth = new System.Windows.Forms.TextBox();
            this.textBoxHeight = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbConstrainRatio = new System.Windows.Forms.CheckBox();
            this.imageSizePickerControl1 = new OpenLiveWriter.PostEditor.PostHtmlEditing.ImageSizePickerControl();
            this.buttonCustomize = new OpenLiveWriter.PostEditor.PostHtmlEditing.CustomizeButton();
            this.toolTip = new OpenLiveWriter.Controls.ToolTip2(this.components);
            this.SuspendLayout();
            //
            // textBoxWidth
            //
            this.textBoxWidth.Location = new System.Drawing.Point(0, 48);
            this.textBoxWidth.Name = "textBoxWidth";
            this.textBoxWidth.Size = new System.Drawing.Size(67, 20);
            this.textBoxWidth.TabIndex = 3;
            this.textBoxWidth.TextChanged += new System.EventHandler(this.textBoxWidth_TextChanged);
            this.textBoxWidth.Leave += new EventHandler(textBoxWidth_Leave);
            this.textBoxWidth.KeyDown += new KeyEventHandler(textBoxWidthHeight_KeyDown);
            //
            // textBoxHeight
            //
            this.textBoxHeight.Location = new System.Drawing.Point(81, 48);
            this.textBoxHeight.Name = "textBoxHeight";
            this.textBoxHeight.Size = new System.Drawing.Size(67, 20);
            this.textBoxHeight.TabIndex = 5;
            this.textBoxHeight.TextChanged += new System.EventHandler(this.textBoxHeight_TextChanged);
            this.textBoxHeight.Leave += new EventHandler(textBoxHeight_Leave);
            this.textBoxHeight.KeyDown += new KeyEventHandler(textBoxWidthHeight_KeyDown);
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Location = new System.Drawing.Point(0, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "&Width:";
            //
            // label2
            //
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Location = new System.Drawing.Point(81, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "&Height:";
            //
            // cbConstrainRatio
            //
            this.cbConstrainRatio.Checked = false;
            this.cbConstrainRatio.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbConstrainRatio.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbConstrainRatio.Location = new System.Drawing.Point(0, 72);
            this.cbConstrainRatio.Name = "cbConstrainRatio";
            this.cbConstrainRatio.Size = new System.Drawing.Size(177, 28);
            this.cbConstrainRatio.TabIndex = 6;
            this.cbConstrainRatio.Text = "Lock &ratio";
            this.cbConstrainRatio.CheckedChanged += new System.EventHandler(this.cbConstrainRatio_CheckedChanged);
            //
            // imageSizePickerControl1
            //
            this.imageSizePickerControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.imageSizePickerControl1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("imageSizePickerControl1.BackgroundImage")));
            this.imageSizePickerControl1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageSizePickerControl1.Location = new System.Drawing.Point(0, 5);
            this.imageSizePickerControl1.Name = "imageSizePickerControl1";
            this.imageSizePickerControl1.SelectedImageSize = OpenLiveWriter.PostEditor.PostHtmlEditing.ImageSize.Unknown;
            this.imageSizePickerControl1.Size = new System.Drawing.Size(216, 21);
            this.imageSizePickerControl1.TabIndex = 0;
            this.imageSizePickerControl1.SelectedImageSizeChanged += new System.EventHandler(this.imageSizePickerControl1_SelectedImageSizeChanged);
            //
            // buttonCustomize
            //
            this.buttonCustomize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCustomize.Image = ((System.Drawing.Image)(resources.GetObject("buttonCustomize.Image")));
            this.buttonCustomize.Location = new System.Drawing.Point(219, 4);
            this.buttonCustomize.Name = "buttonCustomize";
            this.buttonCustomize.Size = new System.Drawing.Size(28, 23);
            this.buttonCustomize.TabIndex = 1;
            this.buttonCustomize.Click += new System.EventHandler(this.buttonCustomize_Click);
            //
            // ImageSizeControl
            //
            this.Controls.Add(this.buttonCustomize);
            this.Controls.Add(this.imageSizePickerControl1);
            this.Controls.Add(this.cbConstrainRatio);
            this.Controls.Add(this.textBoxWidth);
            this.Controls.Add(this.textBoxHeight);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Name = "ImageSizeControl";
            this.Size = new System.Drawing.Size(249, 101);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        void textBoxWidth_Leave(object sender, EventArgs e)
        {
            OnWidthHeightLeave(textBoxWidth, _imageSize.Width);
        }

        void textBoxHeight_Leave(object sender, EventArgs e)
        {
            OnWidthHeightLeave(textBoxHeight, _imageSize.Height);
        }

        private void OnWidthHeightLeave(TextBox textBox, int defaultValue)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                // Fix bug 779659: Writer hangs on entering invalid/no values for height and width of an image
                textBox.Text = defaultValue.ToString(CultureInfo.CurrentCulture);
                return;
            }

            if (_sizeIsDirty)
                OnImageSizeChanged(EventArgs.Empty);
        }

        void textBoxWidthHeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _sizeIsDirty)
            {
                OnImageSizeChanged(EventArgs.Empty);
                e.Handled = true;
            }
        }

        public void LoadImageSize(Size imageSize, Size fullImageSize, RotateFlipType rotation)
        {
            _fullImageSize = fullImageSize;
            _rotation = rotation;

            SetImageSize(imageSize, false);

            if (imageSizePickerControl1.SelectedImageSize == PostHtmlEditing.ImageSize.Unknown)
            {
                int lockedHeight = ImageUtils.GetScaledImageSize(imageSize.Width, Int32.MaxValue, fullImageSize).Height;
                cbConstrainRatio.Checked = imageSize.Height == lockedHeight;
            }
            else
            {
                cbConstrainRatio.Checked = true;
            }

            UpdateSizeName();

            _sizeIsDirty = false;
        }
        private RotateFlipType _rotation;

        public Size ImageSize
        {
            get
            {
                return _imageSize;
            }
        }

        public ImageSizeName ImageBoundsSize
        {
            get
            {
                switch (imageSizePickerControl1.SelectedImageSize)
                {
                    case PostHtmlEditing.ImageSize.Small:
                        return ImageSizeName.Small;
                    case PostHtmlEditing.ImageSize.Medium:
                        return ImageSizeName.Medium;
                    case PostHtmlEditing.ImageSize.Large:
                        return ImageSizeName.Large;
                    case PostHtmlEditing.ImageSize.Original:
                        return ImageSizeName.Full;
                    case PostHtmlEditing.ImageSize.Unknown:
                        return ImageSizeName.Custom;
                    default:
                        return ImageSizeName.Custom;
                }
            }
        }
        private Size _imageSize;
        private Size _fullImageSize;
        private bool _sizeIsDirty = false;
        private bool SetImageSize(Size size, bool custom)
        {
            _ignoreTextChanges++;
            try
            {
                if (_imageSize != size)
                {
                    _imageSize = size;
                    if (size != new Size(Int32.MaxValue, Int32.MaxValue))
                    {
                        textBoxHeight.Text = _imageSize.Height.ToString(CultureInfo.CurrentCulture);
                        textBoxWidth.Text = _imageSize.Width.ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        textBoxHeight.Text = "";
                        textBoxWidth.Text = "";
                    }

                    //don't force to known size when user may be typing
                    if (!custom)
                        UpdateSizeName();

                    _sizeIsDirty = true;
                    //OnImageSizeChanged(EventArgs.Empty);
                    return true;
                }
                return false;
            }
            finally
            {
                _ignoreTextChanges--;
            }
        }

        private void UpdateSizeName()
        {
            Size fullSizeWithRotation = GetFullImageSizeWithRotation();
            if (_imageSize == fullSizeWithRotation)
                imageSizePickerControl1.SelectedImageSize = PostHtmlEditing.ImageSize.Original;
            else if (ImageSizeNameMatches(ImageSizeName.Large, fullSizeWithRotation, _imageSize))
                imageSizePickerControl1.SelectedImageSize = PostHtmlEditing.ImageSize.Large;
            else if (ImageSizeNameMatches(ImageSizeName.Medium, fullSizeWithRotation, _imageSize))
                imageSizePickerControl1.SelectedImageSize = PostHtmlEditing.ImageSize.Medium;
            else if (ImageSizeNameMatches(ImageSizeName.Small, fullSizeWithRotation, _imageSize))
                imageSizePickerControl1.SelectedImageSize = PostHtmlEditing.ImageSize.Small;
            else
                imageSizePickerControl1.SelectedImageSize = PostHtmlEditing.ImageSize.Unknown;
        }

        private Size GetFullImageSizeWithRotation()
        {
            if (ImageUtils.IsRotated90(_rotation))
                return new Size(_fullImageSize.Height, _fullImageSize.Width);
            else
                return _fullImageSize;
        }

        private bool ImageSizeNameMatches(ImageSizeName sizeName, Size orginalSize, Size currentSize)
        {
            Size constrainedSize = ImageSizeHelper.GetSizeConstraints(sizeName);
            return currentSize == ImageUtils.GetScaledImageSize(constrainedSize.Width, constrainedSize.Height, orginalSize);
        }

        public event EventHandler ImageSizeChanged;
        protected virtual void OnImageSizeChanged(EventArgs evt)
        {
            _sizeIsDirty = false;
            if (ImageSizeChanged != null)
                ImageSizeChanged(this, evt);
        }

        private void SetSelectedImageSize()
        {
            if (imageSizePickerControl1.SelectedImageSize != PostHtmlEditing.ImageSize.Unknown)
            {
                Size scaledImageSize = GetScaledImageSize(imageSizePickerControl1.SelectedImageSize);
                if (SetImageSize(scaledImageSize, false))
                    OnImageSizeChanged(EventArgs.Empty);
            }
        }

        private Size GetScaledImageSize(ImageSize baseSize)
        {
            Size imageSize;
            Size scaledImageSize;
            if (baseSize == PostHtmlEditing.ImageSize.Original)
            {
                imageSize = GetFullImageSizeWithRotation();
                scaledImageSize = imageSize;
            }
            else
            {
                imageSize = ImageSizeHelper.GetSizeConstraints(ConvertToImageSizeName(baseSize));

                //if the image is rotated, so create a scaled size based on the rotation within the size bounds
                if (ImageUtils.IsRotated90(_rotation))
                {
                    scaledImageSize = ImageUtils.GetScaledImageSize(imageSize.Height, imageSize.Width, _fullImageSize);
                    scaledImageSize = new Size(scaledImageSize.Height, scaledImageSize.Width);
                }
                else
                {
                    scaledImageSize = ImageUtils.GetScaledImageSize(imageSize.Width, imageSize.Height, _fullImageSize);
                }
            }
            return scaledImageSize;
        }

        private ImageSizeName ConvertToImageSizeName(ImageSize size)
        {
            switch (size)
            {
                case PostHtmlEditing.ImageSize.Small:
                    return ImageSizeName.Small;
                case PostHtmlEditing.ImageSize.Medium:
                    return ImageSizeName.Medium;
                case PostHtmlEditing.ImageSize.Large:
                    return ImageSizeName.Large;
                case PostHtmlEditing.ImageSize.Original:
                    return ImageSizeName.Full;
                case PostHtmlEditing.ImageSize.Unknown:
                    return ImageSizeName.Custom;
                default:
                    Debug.Fail("Unknown image size: " + size.ToString());
                    return ImageSizeName.Full;
            }
        }

        private int _ignoreTextChanges = 0;

        private void textBoxWidth_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreTextChanges <= 0)
                ApplyCustomSize(true);
        }

        private void textBoxHeight_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreTextChanges <= 0)
                ApplyCustomSize(false);
        }

        private bool ApplyCustomSize(bool anchorWidth)
        {
            _ignoreTextChanges++;
            try
            {
                if (textBoxWidth.Text == String.Empty || textBoxHeight.Text == String.Empty)
                {
                    //there is only a partial value entered, so don't apply the current custom size
                    return false;
                }

                try
                {
                    int width = Int32.Parse(textBoxWidth.Text.Trim(), CultureInfo.CurrentCulture);
                    int height = Int32.Parse(textBoxHeight.Text.Trim(), CultureInfo.CurrentCulture);

                    if (width <= 0 || height <= 0 || width > ImageSizeHelper.MAX_WIDTH || height > ImageSizeHelper.MAX_HEIGHT)
                    {
                        //don't apply a custom size, revert back to the old sizes
                        textBoxHeight.Text = ImageSize.Height.ToString(CultureInfo.CurrentCulture);
                        textBoxWidth.Text = ImageSize.Width.ToString(CultureInfo.CurrentCulture);
                        return false;
                    }

                    //clear the currently selected named size
                    imageSizePickerControl1.SelectedImageSize = PostHtmlEditing.ImageSize.Unknown;

                    Size customSize = new Size(width, height);
                    if (cbConstrainRatio.Checked)
                    {
                        if (anchorWidth)
                        {
                            customSize.Height =
                                ImageUtils.GetScaledImageSize(customSize.Width, Int32.MaxValue, _fullImageSize).Height;
                        }
                        else
                        {
                            customSize.Width =
                                ImageUtils.GetScaledImageSize(Int32.MaxValue, customSize.Height, _fullImageSize).Width;
                        }
                    }
                    return SetImageSize(customSize, true);
                }
                catch (Exception)
                {
                    //don't apply a custom size, revert back to the old sizes
                    textBoxHeight.Text = ImageSize.Height.ToString(CultureInfo.CurrentCulture);
                    textBoxWidth.Text = ImageSize.Width.ToString(CultureInfo.CurrentCulture);
                    return false;
                }
            }
            finally
            {
                _ignoreTextChanges--;
            }
        }

        private void cbConstrainRatio_CheckedChanged(object sender, EventArgs e)
        {
            if (cbConstrainRatio.Checked)
            {
                if (ApplyCustomSize(true))
                    OnImageSizeChanged(EventArgs.Empty);
            }
        }

        private void imageSizePickerControl1_SelectedImageSizeChanged(object sender, EventArgs e)
        {
            SetSelectedImageSize();
        }

        private void buttonCustomize_Click(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                using (EditImageSizesDialog dialog = new EditImageSizesDialog())
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        imageSizePickerControl1.Resynchronize(new GetScaledImageSizeDelegate(GetScaledImageSize));
                    }
                }
            }
        }
    }

    internal class CustomizeButton : XPBitmapButton
    {
        public CustomizeButton()
        {
            Bitmap buttonFace = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.ImageEditing.Images.CustomizeImageSizeButton.png");
            Initialize(buttonFace, buttonFace);
        }
    }
}
