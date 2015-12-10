// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.CoreServices.Layout;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageTargetEditorControl.
    /// </summary>
    public class ImageTargetEditorControl : UserControl, ILinkOptionsEditor
    {
        private ImageSizeControl imageSizeControl1;
        private LinkOptionsEditorControl linkOptionsControl1;
        private Label label1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ImageTargetEditorControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.label1.Text = Res.Get(StringId.ImgSBImageSizeLabel);
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
            this.imageSizeControl1 = new OpenLiveWriter.PostEditor.PostHtmlEditing.ImageSizeControl();
            this.linkOptionsControl1 = new OpenLiveWriter.PostEditor.PostHtmlEditing.LinkOptionsEditorControl();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // imageSizeControl1
            //
            this.imageSizeControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.imageSizeControl1.Location = new System.Drawing.Point(4, 16);
            this.imageSizeControl1.Name = "imageSizeControl1";
            this.imageSizeControl1.Size = new System.Drawing.Size(240, 100);
            this.imageSizeControl1.TabIndex = 1;
            this.imageSizeControl1.ImageSizeChanged += new System.EventHandler(this.imageSizeControl1_ImageSizeChanged);
            //
            // linkOptionsControl1
            //
            this.linkOptionsControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.linkOptionsControl1.Location = new System.Drawing.Point(4, 116);
            this.linkOptionsControl1.Name = "linkOptionsControl1";
            this.linkOptionsControl1.Size = new System.Drawing.Size(240, 66);
            this.linkOptionsControl1.TabIndex = 2;
            this.linkOptionsControl1.LinkOptionsChanged += new System.EventHandler(this.linkOptionsControl1_LinkOptionsChanged);
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Image size:";
            //
            // ImageTargetEditorControl
            //
            this.Controls.Add(this.label1);
            this.Controls.Add(this.linkOptionsControl1);
            this.Controls.Add(this.imageSizeControl1);
            this.Name = "ImageTargetEditorControl";
            this.Size = new System.Drawing.Size(248, 190);
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                base.OnLoad(e);
                imageSizeControl1.Height = imageSizeControl1.GetPreferredSize(new Size(imageSizeControl1.Width, Int32.MaxValue)).Height;
                linkOptionsControl1.Top = imageSizeControl1.Bottom;
            }

        }

        private void linkOptionsControl1_LinkOptionsChanged(object sender, EventArgs e)
        {
            OnImageOptionsChanged(EventArgs.Empty);
        }

        private void imageSizeControl1_ImageSizeChanged(object sender, EventArgs e)
        {
            OnImageOptionsChanged(EventArgs.Empty);
        }
        public event EventHandler ImageOptionsChanged;
        protected virtual void OnImageOptionsChanged(EventArgs evt)
        {
            if (ImageOptionsChanged != null)
                ImageOptionsChanged(this, evt);
        }

        internal void LoadImageSize(Size imageSize, Size fullImageSize, RotateFlipType rotation)
        {
            this.imageSizeControl1.LoadImageSize(imageSize, fullImageSize, rotation);
        }

        public Size ImageSize
        {
            get
            {
                return imageSizeControl1.ImageSize;
            }
        }

        public ImageSizeName ImageBoundsSize
        {
            get
            {
                return imageSizeControl1.ImageBoundsSize;
            }
        }

        public ILinkOptions LinkOptions
        {
            get { return linkOptionsControl1.LinkOptions; }
            set { linkOptionsControl1.LinkOptions = value; }
        }

        public IEditorOptions EditorOptions
        {
            set { linkOptionsControl1.EditorOptions = value; }
        }
    }
}
