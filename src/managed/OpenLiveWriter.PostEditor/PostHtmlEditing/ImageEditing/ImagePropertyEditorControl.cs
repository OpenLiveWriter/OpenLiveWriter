// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Api.Sidebar;
using OpenLiveWriter.Api.ImageEditing;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Commands;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Hosts the tab control for the image property editing form.
    /// </summary>
    public class ImagePropertyEditorControl : PrimaryWorkspaceControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private PrimaryWorkspaceWorkspaceCommandBarLightweightControl commandBarLightweightControl;

        private TabLightweightControl tabLightweightControl;
        private ImageTabPageImageControl imageTabPageImage;
        private ImageTabPageLayoutControl imageTabPageLayout;
        private ImageTabPageEffectsControl imageTabPageEffects;
        private ImageTabPageUploadControl imageTabPageUpload;

        public ImagePropertyEditorControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // configure look and behavior
            AllowDragDropAutoScroll = false;
            AllPaintingInWmPaint = true;
            TopLayoutMargin = 2;

            ApplicationStyleManager.ApplicationStyleChanged += new EventHandler(ApplicationManager_ApplicationStyleChanged);

            // initialize tab lightweight control
            tabLightweightControl = new TabLightweightControl(this.components) ;
            tabLightweightControl.DrawSideAndBottomTabPageBorders = false ;
            tabLightweightControl.SmallTabs = true ;
        }

        public void Init(IBlogPostImageDataContext dataContext)
        {

            _imageDataContext = dataContext;
            // initialization constants
            const int TOP_INSET = 2;

            imageTabPageImage = new ImageTabPageImageControl() ;
            imageTabPageLayout = new ImageTabPageLayoutControl() ;
            imageTabPageEffects = new ImageTabPageEffectsControl() ;
            imageTabPageUpload = new ImageTabPageUploadControl() ;

            _tabPages = new ImageEditingTabPageControl[]{imageTabPageLayout, imageTabPageImage, imageTabPageEffects, imageTabPageUpload};
            for(int i=0; i<_tabPages.Length; i++)
            {
                ImageEditingTabPageControl tabPage = _tabPages[i];
                tabPage.DecoratorsManager = dataContext.DecoratorsManager;
                tabPage.TabStop = false ;
                tabPage.TabIndex = i ;
                Controls.Add( tabPage ) ;
                tabLightweightControl.SetTab( i, tabPage ) ;
            }

            // initial appearance of editor
            tabLightweightControl.SelectedTabNumber = 0 ;

            InitializeCommands();
            InitializeToolbar();

            _imageDataContext.DecoratorsManager.GetImageDecorator(BrightnessDecorator.Id).Command.StateChanged += new EventHandler(Command_StateChanged);

            // configure primary workspace
            // configure primary workspace
            SuspendLayout() ;
            TopLayoutMargin = TOP_INSET;
            LeftColumn.UpperPane.LightweightControl = tabLightweightControl;
            CenterColumn.Visible = false;
            RightColumn.Visible = false;
            ResumeLayout() ;
        }
        IBlogPostImageDataContext _imageDataContext;

        private void InitializeCommands()
        {
            commandContextManager = new CommandContextManager(ApplicationManager.CommandManager);
            commandContextManager.BeginUpdate() ;

            commandImageBrightness = new CommandImageBrightness(components) ;
            commandImageBrightness.Tag = _imageDataContext.DecoratorsManager.GetImageDecorator(BrightnessDecorator.Id);
            commandImageBrightness.Execute += new EventHandler(commandImageDecorator_Execute);
            commandContextManager.AddCommand( commandImageBrightness, CommandContext.Normal) ;

            commandImageRotate = new CommandImageRotate(components) ;
            commandImageRotate.Execute += new EventHandler(commandImageRotate_Execute);
            commandContextManager.AddCommand( commandImageRotate, CommandContext.Normal) ;

            commandImageReset = new CommandImageReset(components) ;
            commandImageReset.Execute += new EventHandler(commandImageReset_Execute);
            commandContextManager.AddCommand( commandImageReset, CommandContext.Normal) ;

            commandImageSaveDefaults = new CommandImageSaveDefaults(components) ;
            commandImageSaveDefaults.Execute += new EventHandler(commandImageSaveDefaults_Execute);
            commandContextManager.AddCommand( commandImageSaveDefaults, CommandContext.Normal) ;

            commandContextManager.EndUpdate() ;
        }
        CommandContextManager commandContextManager;
        Command commandImageBrightness;
        Command commandImageRotate;
        Command commandImageReset;
        Command commandImageSaveDefaults;

        private void InitializeToolbar()
        {
            CommandBarButtonEntry commandBarButtonEntryImageBrightness = new CommandBarButtonEntry(components) ;
            commandBarButtonEntryImageBrightness.CommandIdentifier = commandImageBrightness.Identifier ;

            CommandBarButtonEntry commandBarButtonEntryImageRotate = new CommandBarButtonEntry(components) ;
            commandBarButtonEntryImageRotate.CommandIdentifier = commandImageRotate.Identifier ;

            CommandBarButtonEntry commandBarButtonEntryImageReset = new CommandBarButtonEntry(components) ;
            commandBarButtonEntryImageReset.CommandIdentifier = commandImageReset.Identifier ;

            CommandBarButtonEntry commandBarButtonEntryImageSaveDefaults = new CommandBarButtonEntry(components) ;
            commandBarButtonEntryImageSaveDefaults.CommandIdentifier = commandImageSaveDefaults.Identifier ;

            CommandBarDefinition commandBarDefinition = new CommandBarDefinition(components) ;
            commandBarDefinition.LeftCommandBarEntries.Add( commandBarButtonEntryImageRotate ) ;
            commandBarDefinition.LeftCommandBarEntries.Add( commandBarButtonEntryImageBrightness ) ;

            commandBarDefinition.RightCommandBarEntries.Add( commandBarButtonEntryImageReset ) ;
            commandBarDefinition.RightCommandBarEntries.Add( commandBarButtonEntryImageSaveDefaults ) ;

            commandBarLightweightControl = new PrimaryWorkspaceWorkspaceCommandBarLightweightControl(components) ;
            commandBarLightweightControl.LightweightControlContainerControl = this ;

            commandBarLightweightControl.CommandManager = ApplicationManager.CommandManager ;
            commandBarLightweightControl.CommandBarDefinition = commandBarDefinition ;
        }

        public override CommandBarLightweightControl FirstCommandBarLightweightControl
        {
            get
            {
                return commandBarLightweightControl;
            }
        }

        public event EventHandler SaveDefaultsRequested;
        public event EventHandler ResetToDefaultsRequested;

        public event ImagePropertyEventHandler ImagePropertyChanged;
        private void ApplyImageDecorations()
        {
            ApplyImageDecorations(ImageDecoratorInvocationSource.ImagePropertiesEditor);
        }
        private void ApplyImageDecorations(ImageDecoratorInvocationSource source)
        {
            if(ImagePropertyChanged != null)
            {
                ImagePropertyChanged(this, new ImagePropertyEvent(ImagePropertyType.Decorators, this.ImageInfo, source));
            }
        }
        public ImagePropertiesInfo ImageInfo
        {
            get
            {
                return imageInfo;
            }
            set
            {
                if(imageInfo != value)
                {
                    imageInfo = value;

                    //disable the image rotate command if the image is not a local image.
                    commandImageRotate.Enabled = imageInfo != null && imageInfo.ImageSourceUri.IsFile;

                    imageTabPageUpload.Visible = (this._imageDataContext != null) && (this._imageDataContext.ImageServiceId != null);
                }
            }
        }
        public ImagePropertiesInfo imageInfo;

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

                ApplicationStyleManager.ApplicationStyleChanged -= new EventHandler(ApplicationManager_ApplicationStyleChanged);
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

        internal ImageEditingTabPageControl[] TabPages
        {
            get
            {
                return _tabPages;
            }
        }
        private ImageEditingTabPageControl[] _tabPages = new ImageEditingTabPageControl[0];

        private void UpdateAppearance()
        {
            PerformLayout();
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            using ( Brush brush = new SolidBrush(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomColor) )
                pevent.Graphics.FillRectangle( brush, ClientRectangle ) ;
        }

        /// <summary>
        /// Handle appearance preference changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplicationManager_ApplicationStyleChanged(object sender, EventArgs e)
        {
            UpdateAppearance() ;
        }

        private void commandImageDecorator_Execute(object sender, EventArgs e)
        {
            ImageDecorator imageDecorator = ((ImageDecorator)((Command)sender).Tag);

            //perform the execution so that the decorator is added to the list of active decorators
            imageDecorator.Command.PerformExecute();

            //since this command was invoked explicitly via a command button, display the editor dialog.
            ImageDecoratorHelper.ShowImageDecoratorEditorDialog( FindForm(), imageDecorator, ImageInfo, new ApplyDecoratorCallback(ApplyImageDecorations) );
        }

        private void commandImageRotate_Execute(object sender, EventArgs e)
        {
            ImageInfo.ImageRotation = ImageDecoratorUtils.GetFlipTypeRotated90(ImageInfo.ImageRotation);
            ApplyImageDecorations();
        }

        private void commandImageReset_Execute(object sender, EventArgs e)
        {
            if(ResetToDefaultsRequested != null)
            {
                ResetToDefaultsRequested(this, EventArgs.Empty);
            }
            ApplyImageDecorations(ImageDecoratorInvocationSource.Reset);
        }

        private void commandImageSaveDefaults_Execute(object sender, EventArgs e)
        {
            if(SaveDefaultsRequested != null)
            {
                SaveDefaultsRequested(this, EventArgs.Empty);
            }
        }

        private void Command_StateChanged(object sender, EventArgs e)
        {
            commandImageBrightness.Enabled = _imageDataContext.DecoratorsManager.GetImageDecorator(BrightnessDecorator.Id).Command.Enabled;
        }
    }
}
