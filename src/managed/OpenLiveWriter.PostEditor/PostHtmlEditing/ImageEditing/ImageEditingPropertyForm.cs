// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using Onfolio.Core.HtmlEditor;
using Onfolio.Writer.Api.ImageEditing;
using Onfolio.Writer.PostEditor.PostHtmlEditing;
using Onfolio.Writer.PostEditor.PostHtmlEditing.Commands;
using mshtml ;
using Project31.ApplicationFramework;

namespace Onfolio.Writer.PostEditor.PostHtmlEditing
{
    public class ImageEditingPropertyForm : MainFrameSatelliteWindow, IImagePropertyEditingContext
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = new Container();

        private IHtmlEditorComponentContext _editorContext ;
        private IBlogPostImageDataContext _imageDataContext;
        private Onfolio.Writer.PostEditor.PostHtmlEditing.ImagePropertyEditorControl imagePropertyEditorControl1;
        private Onfolio.Writer.PostEditor.PostHtmlEditing.ImageEditingPropertyStatusbar imageEditingPropertyStatusbar1;
        private CreateFileCallback _createFileCallback ;

        #region Initialization/Singleton

        public static void Initialize(IHtmlEditorComponentContext editorContext, IBlogPostImageDataContext dataContext, CreateFileCallback callback)
        {
            // initialize one form per-thread
            if ( _imagePropertyForm == null )
            {
                _imagePropertyForm = new ImageEditingPropertyForm() ;
                _imagePropertyForm.Init(editorContext, dataContext, callback);
            }
        }

        public static ImageEditingPropertyForm Instance
        {
            get
            {
                return _imagePropertyForm ;
            }
        }

        [ThreadStatic]
        private static ImageEditingPropertyForm _imagePropertyForm ;

        public ImageEditingPropertyForm()
        {
            InitializeComponent() ;
        }

        #endregion

        #region Interaction with base (MainFrameSatelliteWindow)

        private void Init(IHtmlEditorComponentContext editorContext, IBlogPostImageDataContext dataContext, CreateFileCallback callback)
        {
            _editorContext = editorContext ;
            _editorContext.SelectionChanged +=new EventHandler(_editorContext_SelectionChanged);
            _imageDataContext = dataContext;

            _createFileCallback = callback ;

            this.imagePropertyEditorControl1.Init(dataContext);

            base.Init(editorContext.MainFrameWindow, typeof(CommandViewImageProperties)) ;
        }

        protected override void OnBeforeShow()
        {
            base.OnBeforeShow();
            ImagePropertyHandler.RefreshView();
        }

        protected override bool AutoUpdateActivationSetting
        {
            get { return ImageEditingSettings.AutoSavePropertiesActivationState  ; }
        }

        protected override bool ShowWhenEditContextActivated
        {
            get { return ImageEditingSettings.ShowPropertiesOnImageSelection ; }
            set { ImageEditingSettings.ShowPropertiesOnImageSelection = value ; }
        }

        protected override Size MainFrameTopRightOffset
        {
            get { return ImageEditingSettings.PropertiesMainFrameTopRightOffset ; }
            set { ImageEditingSettings.PropertiesMainFrameTopRightOffset = value ; }
        }

        #endregion

        private ImageEditingTabPageControl[] TabPages
        {
            get
            {
                return this.imagePropertyEditorControl1.TabPages;
            }
        }

        private void _editorContext_SelectionChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        public IHTMLImgElement SelectedImage
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
                    Debug.Assert(SelectedImage != null, "No image selected!");
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
                imagePropertyEditorControl1.ImagePropertyChanged -= new ImagePropertyEventHandler(tabPage_ImagePropertyChanged);

                ManageCommands();

                foreach(ImageEditingTabPageControl tabPage in TabPages)
                    tabPage.ImageInfo = _imageInfo;

                imagePropertyEditorControl1.ImageInfo = _imageInfo;

                //re-subscribe to change events so they can be rebroadcast
                foreach(ImageEditingTabPageControl tabPage in TabPages)
                    tabPage.ImagePropertyChanged += new ImagePropertyEventHandler(tabPage_ImagePropertyChanged);
                imagePropertyEditorControl1.ImagePropertyChanged += new ImagePropertyEventHandler(tabPage_ImagePropertyChanged);

                imageEditingPropertyStatusbar1.SetImageStatus(_imageInfo.ImageSourceUri.ToString(),
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
        public void RefreshImage()
        {
            // update selected image (null if none is selected)
            _selectedImage = _editorContext.SelectedImage ;

            // update display
            EditContextActivated = SelectionIsImage ;
        }

        public event EventHandler SaveDefaultsRequested
        {
            add
            {
                imagePropertyEditorControl1.SaveDefaultsRequested += value;
            }
            remove
            {
                imagePropertyEditorControl1.SaveDefaultsRequested -= value;
            }
        }
        public event EventHandler ResetToDefaultsRequested
        {
            add
            {
                imagePropertyEditorControl1.ResetToDefaultsRequested += value;
            }
            remove
            {
                imagePropertyEditorControl1.ResetToDefaultsRequested -= value;
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
            if(!Visible)
            {
                //refresh the ImagePropertiesInfo object since it doesn't get refreshed automatically when
                //the form is hidden.
                ImagePropertyHandler.RefreshView();
            }

            ImagePropertiesInfo.InlineImageSize = newSize;
            ImagePropertyChanged(this, new ImagePropertyEvent(ImagePropertyType.InlineSize, ImagePropertiesInfo, invocationSource));
            RefreshImage();
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

        #region Designer Generated Code

        private void InitializeComponent()
        {
            this.imagePropertyEditorControl1 = new Onfolio.Writer.PostEditor.PostHtmlEditing.ImagePropertyEditorControl();
            this.imageEditingPropertyStatusbar1 = new Onfolio.Writer.PostEditor.PostHtmlEditing.ImageEditingPropertyStatusbar();
            ((System.ComponentModel.ISupportInitialize)(this.imagePropertyEditorControl1)).BeginInit();
            this.SuspendLayout();
            //
            // imagePropertyEditorControl1
            //
            this.imagePropertyEditorControl1.AllowDragDropAutoScroll = false;
            this.imagePropertyEditorControl1.AllPaintingInWmPaint = true;
            this.imagePropertyEditorControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.imagePropertyEditorControl1.BackColor = System.Drawing.SystemColors.Control;
            this.imagePropertyEditorControl1.ImageInfo = null;
            this.imagePropertyEditorControl1.Location = new System.Drawing.Point(0, 0);
            this.imagePropertyEditorControl1.Name = "imagePropertyEditorControl1";
            this.imagePropertyEditorControl1.Size = new System.Drawing.Size(236, 335);
            this.imagePropertyEditorControl1.TabIndex = 0;
            this.imagePropertyEditorControl1.TopLayoutMargin = 2;
            //
            // imageEditingPropertyStatusbar1
            //
            this.imageEditingPropertyStatusbar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.imageEditingPropertyStatusbar1.Location = new System.Drawing.Point(0, 334);
            this.imageEditingPropertyStatusbar1.Name = "imageEditingPropertyStatusbar1";
            this.imageEditingPropertyStatusbar1.Size = new System.Drawing.Size(236, 24);
            this.imageEditingPropertyStatusbar1.TabIndex = 1;
            this.imageEditingPropertyStatusbar1.TabStop = false;
            //
            // ImageEditingPropertyForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(236, 358);
            this.Controls.Add(this.imageEditingPropertyStatusbar1);
            this.Controls.Add(this.imagePropertyEditorControl1);
            this.Name = "ImageEditingPropertyForm";
            this.Text = "Image Properties";
            ((System.ComponentModel.ISupportInitialize)(this.imagePropertyEditorControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
