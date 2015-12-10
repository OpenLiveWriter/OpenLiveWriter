// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Api.ImageEditing;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class ImageEditingPropertySidebar : Panel, IImagePropertyEditingContext
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = new Container();

        private IHtmlEditorComponentContext _editorContext ;
        private IBlogPostImageDataContext _imageDataContext;
        private ImageEditingPropertyTitlebar imageEditingPropertyTitlebar;
        private ImagePropertyEditorControl imagePropertyEditorControl;
        private ImageEditingPropertyStatusbar imageEditingPropertyStatusbar;
        private CreateFileCallback _createFileCallback ;

        #region Initialization/Singleton

        public static void Initialize(IHtmlEditorComponentContext editorContext, IBlogPostImageDataContext dataContext, CreateFileCallback callback)
        {
            // initialize one form per-thread
            if ( _imagePropertySidebar == null )
            {
                _imagePropertySidebar = new ImageEditingPropertySidebar() ;
                _imagePropertySidebar.Init(editorContext, dataContext, callback);
            }
        }

        public static ImageEditingPropertySidebar Instance
        {
            get
            {
                return _imagePropertySidebar ;
            }
        }

        [ThreadStatic]
        private static ImageEditingPropertySidebar _imagePropertySidebar ;

        public ImageEditingPropertySidebar()
        {
            InitializeDockPadding() ;
            InitializeControls();
            AdjustLayoutForLargeFonts() ;
        }

        private void InitializeControls()
        {
            SuspendLayout() ;

            Size = new Size(236, 400) ;

            imageEditingPropertyTitlebar = new ImageEditingPropertyTitlebar();
            imageEditingPropertyTitlebar.Dock = DockStyle.Top;
            imageEditingPropertyTitlebar.TabIndex = 0;
            imageEditingPropertyTitlebar.TabStop = false;
            imageEditingPropertyTitlebar.HideTitleBarClicked +=new EventHandler(imageEditingPropertyTitlebar_HideTitleBarClicked);

            imagePropertyEditorControl = new ImagePropertyEditorControl();
            imagePropertyEditorControl.AllowDragDropAutoScroll = false;
            imagePropertyEditorControl.AllPaintingInWmPaint = true;
            imagePropertyEditorControl.Dock = DockStyle.Fill ;
            imagePropertyEditorControl.ImageInfo = null;
            imagePropertyEditorControl.Location = new Point(0, 25);
            imagePropertyEditorControl.Size = new Size(236, 335);
            imagePropertyEditorControl.TabIndex = 1;
            imagePropertyEditorControl.TopLayoutMargin = 2;

            imageEditingPropertyStatusbar = new ImageEditingPropertyStatusbar();
            imageEditingPropertyStatusbar.Dock = DockStyle.Bottom;
            imageEditingPropertyStatusbar.Size = new Size(236, 24);
            imageEditingPropertyStatusbar.TabIndex = 2;
            imageEditingPropertyStatusbar.TabStop = false;

            Controls.Add(imagePropertyEditorControl);
            Controls.Add(imageEditingPropertyTitlebar);
            Controls.Add(imageEditingPropertyStatusbar);

            ResumeLayout(false);
        }


        #endregion

        #region Public Interface

        /// <summary>
        /// Make the form visible / update its appearance based on current selection
        /// </summary>
        public void ShowSidebar()
        {
            // refresh the view
            RefreshImage();

            // manage UI
            ManageVisibilityOfEditingUI() ;

            if ( !Visible )
            {
                // show the form
                Show() ;
            }
        }

        private void _editorContext_SelectionChanged(object sender, EventArgs e)
        {
            RefreshImage();

            // manage UI
            ManageVisibilityOfEditingUI() ;
        }

        private void ManageVisibilityOfEditingUI()
        {
            // show/hide property UI as appropriate
            SuspendLayout() ;
            imagePropertyEditorControl.Visible = SelectionIsImage ;
            imageEditingPropertyStatusbar.Visible = SelectionIsImage ;
            ResumeLayout(true) ;
        }

        public void HideSidebar()
        {
            Visible = false ;
        }

        private void imageEditingPropertyTitlebar_HideTitleBarClicked(object sender, EventArgs e)
        {
            HideSidebar() ;
        }

        #endregion

        private void Init(IHtmlEditorComponentContext editorContext, IBlogPostImageDataContext dataContext, CreateFileCallback callback)
        {
            _editorContext = editorContext ;
            _editorContext.SelectionChanged +=new EventHandler(_editorContext_SelectionChanged);
            _imageDataContext = dataContext;

            _createFileCallback = callback ;

            this.imagePropertyEditorControl.Init(dataContext);
        }

        private ImageEditingTabPageControl[] TabPages
        {
            get
            {
                return this.imagePropertyEditorControl.TabPages;
            }
        }


        IHTMLImgElement IImagePropertyEditingContext.SelectedImage
        {
            get { return _selectedImage; }
        }
        private IHTMLImgElement _selectedImage ;

        private bool SelectionIsImage
        {
            get
            {
                return _selectedImage != null ;
            }
        }

        private ImageEditingPropertyHandler ImagePropertyHandler
        {
            get
            {
                if (_imagePropertyHandler == null)
                {
                    Debug.Assert(_selectedImage != null, "No image selected!");
                    _imagePropertyHandler = new ImageEditingPropertyHandler(
                        this, _createFileCallback, _imageDataContext );
                }
                return _imagePropertyHandler ;
            }
        }
        private ImageEditingPropertyHandler _imagePropertyHandler ;

        public ImagePropertiesInfo ImagePropertiesInfo
        {
            get
            {
                return _imageInfo;
            }
            set
            {
                _imageInfo = value;

                //unsubscribe from change event while the image info is being updated.
                foreach(ImageEditingTabPageControl tabPage in TabPages)
                    tabPage.ImagePropertyChanged -= new ImagePropertyEventHandler(tabPage_ImagePropertyChanged);
                imagePropertyEditorControl.ImagePropertyChanged -= new ImagePropertyEventHandler(tabPage_ImagePropertyChanged);

                ManageCommands();

                foreach(ImageEditingTabPageControl tabPage in TabPages)
                    tabPage.ImageInfo = _imageInfo;

                imagePropertyEditorControl.ImageInfo = _imageInfo;

                //re-subscribe to change events so they can be rebroadcast
                foreach(ImageEditingTabPageControl tabPage in TabPages)
                    tabPage.ImagePropertyChanged += new ImagePropertyEventHandler(tabPage_ImagePropertyChanged);
                imagePropertyEditorControl.ImagePropertyChanged += new ImagePropertyEventHandler(tabPage_ImagePropertyChanged);

                imageEditingPropertyStatusbar.SetImageStatus(_imageInfo.ImageSourceUri.ToString(),
                    _imageInfo.ImageSourceUri.IsFile ? ImageEditingPropertyStatusbar.IMAGE_TYPE.LOCAL_IMAGE :
                        ImageEditingPropertyStatusbar.IMAGE_TYPE.WEB_IMAGE
                    );

            }
        }
        private ImagePropertiesInfo _imageInfo;

        /// <summary>
        /// Manages the commands assocatiated with image editing.
        /// </summary>
        private void ManageCommands()
        {
            if(_imageDataContext != null)
            {
                for(int i=0; i<_imageDataContext.DecoratorsManager.ImageDecoratorGroups.Length; i++)
                {
                    ImageDecoratorGroup imageDecoratorGroup = _imageDataContext.DecoratorsManager.ImageDecoratorGroups[i];
                    foreach(ImageDecorator imageDecorator in imageDecoratorGroup.ImageDecorators)
                    {
                        if(_imageInfo == null || _imageInfo.ImageSourceUri.IsFile || imageDecorator.IsWebImageSupported)
                            imageDecorator.Command.Enabled = true;
                        else
                            imageDecorator.Command.Enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Forces a reload of the image's properties. This is useful for refreshing the form when the image
        /// properties have been changed externally.
        /// </summary>
        private void RefreshImage()
        {
            // update selected image (null if none is selected)
            _selectedImage = _editorContext.SelectedImage ;

            // refresh the view
            if ( SelectionIsImage )
                ImagePropertyHandler.RefreshView();
        }

        public event EventHandler SaveDefaultsRequested
        {
            add
            {
                imagePropertyEditorControl.SaveDefaultsRequested += value;
            }
            remove
            {
                imagePropertyEditorControl.SaveDefaultsRequested -= value;
            }
        }
        public event EventHandler ResetToDefaultsRequested
        {
            add
            {
                imagePropertyEditorControl.ResetToDefaultsRequested += value;
            }
            remove
            {
                imagePropertyEditorControl.ResetToDefaultsRequested -= value;
            }
        }

        public event ImagePropertyEventHandler ImagePropertyChanged;
        protected virtual void OnImagePropertyChanged(ImagePropertyEvent evt)
        {
            if(ImagePropertyChanged != null)
            {
                ImagePropertyChanged(this, evt);
            }

            //notify the tabs that the image properties have changed so that they can update themselves.
            foreach(ImageEditingTabPageControl tabPage in TabPages)
            {
                tabPage.HandleImagePropertyChangedEvent(evt);
            }
        }

        private void tabPage_ImagePropertyChanged(object source, ImagePropertyEvent evt)
        {
            OnImagePropertyChanged(evt);
        }

        public void UpdateInlineImageSize(Size newSize, ImageDecoratorInvocationSource invocationSource)
        {
            ImagePropertiesInfo imagePropertiesInfo = (this as IImagePropertyEditingContext).ImagePropertiesInfo ;
            imagePropertiesInfo.InlineImageSize = newSize;
            ImagePropertyChanged(this, new ImagePropertyEvent(ImagePropertyType.InlineSize, imagePropertiesInfo, invocationSource));
            RefreshImage();
        }

        private const int TOP_INSET = 1 ;
        private const int LEFT_INSET = 5 ;
        private const int RIGHT_INSET = 1 ;
        private const int BOTTOM_INSET = 1 ;

        private void InitializeDockPadding()
        {
            DockPadding.Top = TOP_INSET ;
            DockPadding.Left = LEFT_INSET ;
            DockPadding.Right = RIGHT_INSET ;
            DockPadding.Bottom = BOTTOM_INSET ;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);

            // draw the border
            Rectangle borderRectangle = new Rectangle(
                    ClientRectangle.Left + LEFT_INSET - 1,
                    ClientRectangle.Top + TOP_INSET - 1,
                    ClientRectangle.Width - LEFT_INSET - RIGHT_INSET + 1,
                    ClientRectangle.Height - TOP_INSET - BOTTOM_INSET + 1) ;
            using ( Pen pen = new Pen(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor) )
                e.Graphics.DrawRectangle( pen, borderRectangle ) ;

            // draw the no image selected message
            Font textFont = ApplicationManager.ApplicationStyle.NormalApplicationFont ;
            string noImageSelected = "No image selected" ;
            SizeF textSize = e.Graphics.MeasureString(noImageSelected, textFont) ;
            Color textColor = Color.FromArgb(200, ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTextColor);
            using ( Brush brush = new SolidBrush(textColor) )
                e.Graphics.DrawString( "No image selected", textFont, brush, new PointF((Width/2)-(textSize.Width/2),100));
        }

        private void AdjustLayoutForLargeFonts()
        {
            // see if we need to adjust our width for non-standard DPI (large fonts)
            const double DESIGNTIME_DPI = 96 ;
            double dpiX = Convert.ToDouble(DisplayHelper.PixelsPerLogicalInchX) ;
            if ( dpiX > DESIGNTIME_DPI )
            {
                // adjust scale ration for percentage of toolbar containing text
                const double TEXT_PERCENT = 0.0 ;  // currently has no text
                double scaleRatio = (1-TEXT_PERCENT) + ((TEXT_PERCENT*dpiX)/DESIGNTIME_DPI) ;

                // change width as appropriate
                Width = Convert.ToInt32( Convert.ToDouble(Width) * scaleRatio ) ;
            }
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

    }
}
