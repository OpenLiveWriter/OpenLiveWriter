// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageEditingPropertyStatusbar.
    /// </summary>
    internal class ImageEditingPropertyStatusbar : UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ImageEditingPropertyStatusbar()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // enable double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // set height
            Height = SystemInformation.ToolWindowCaptionHeight ;

            // does not accept tab
            TabStop = false ;

            _stringFormat = new StringFormat(StringFormatFlags.NoWrap);
            _stringFormat.Trimming = StringTrimming.EllipsisPath;

            // set the background color
            //BackColor = ApplicationManager.ApplicationStyle.ActiveTabTopColor ;
            ApplicationStyleManager.ApplicationStyleChanged += new EventHandler(ApplicationManager_ApplicationStyleChanged);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        internal enum IMAGE_TYPE { LOCAL_IMAGE, WEB_IMAGE };
        public void SetImageStatus(string imagePath, IMAGE_TYPE imageType)
        {
            imagePath = Path.GetFileName(imagePath.Replace("/", "\\"));
            _statusText = String.Format("{0}:  {1}", imageType == IMAGE_TYPE.LOCAL_IMAGE ? "Local image": "Web image", imagePath);
            PerformLayout();
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout (levent);

            // calculate the rectangle we will paint within
            _controlRectangle = new Rectangle(0,0,Width, Height) ;

            _image = localImage;
            int IMAGE_TOP_OFFSET = TOP_OFFSET ;
            int IMAGE_LEFT_OFFSET = TOP_OFFSET ;
            _imageRectangle = new Rectangle(IMAGE_LEFT_OFFSET, IMAGE_TOP_OFFSET, _image.Width, _image.Height );

            //calculate the text rectangle
            int TEXT_TOP_OFFSET = TOP_OFFSET+2 ;
            int TEXT_LEFT_MARGIN = 0 ;
            int TEXT_LEFT_OFFSET = _imageRectangle.Right + TEXT_LEFT_MARGIN ;

            _textRectangle = new Rectangle(TEXT_LEFT_OFFSET, TEXT_TOP_OFFSET, _controlRectangle.Width-TEXT_LEFT_OFFSET,_controlRectangle.Height-TEXT_TOP_OFFSET);

            // make sure the control is repainted
            Invalidate() ;
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            Color backgroundColor = ApplicationManager.ApplicationStyle.ActiveTabBottomColor ;
            using ( Brush brush = new SolidBrush(backgroundColor) )
                e.Graphics.FillRectangle( brush, _controlRectangle ) ;

            // draw the border
            using ( Pen pen = new Pen(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor) )
            {
                e.Graphics.DrawLine(pen, _controlRectangle.Left, _controlRectangle.Top, _controlRectangle.Width, _controlRectangle.Top);
                e.Graphics.DrawLine(pen, _controlRectangle.Left, _controlRectangle.Bottom, _controlRectangle.Width, _controlRectangle.Bottom);
                //e.Graphics.DrawRectangle( pen, _controlRectangle ) ;
            }

            // draw the image
            e.Graphics.DrawImage( _image, _imageRectangle.Left, _imageRectangle.Top ) ;

            // draw the text
            using ( Brush brush = new SolidBrush(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTextColor) )
                e.Graphics.DrawString( _statusText ,ApplicationManager.ApplicationStyle.NormalApplicationFont, brush,
                    new RectangleF(_textRectangle.X, _textRectangle.Y, _textRectangle.Width, _textRectangle.Height),
                                       _stringFormat);
        }
        private string _statusText = "Local image: ";
        private StringFormat _stringFormat;
        private Rectangle _controlRectangle ;
        private Rectangle _imageRectangle ;
        private Rectangle _textRectangle ;
        private Image _image;
        private static readonly int TOP_OFFSET = SatelliteApplicationForm.WorkspaceInset;

        //status bar image
        private const string IMAGE_PATH = "PostHtmlEditing.ImageEditing.Images." ;
        private Bitmap localImage = ResourceHelper.LoadAssemblyResourceBitmap( IMAGE_PATH + "LocalImage.png") ;
        private Bitmap webImage = ResourceHelper.LoadAssemblyResourceBitmap( IMAGE_PATH + "WebImage.png") ;

        private void UpdateAppearance()
        {
            PerformLayout();
            Invalidate();
        }

        private void ApplicationManager_ApplicationStyleChanged(object sender, EventArgs e)
        {
            UpdateAppearance();
        }
    }
}
