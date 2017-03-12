// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlEditor.Linking;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Used to host the sidebar control for the image property editing form. Now that the ribbon has replaced the
    /// sidebar, this class has been repurposed to centralize all the picture related commands.
    /// </summary>
    public class PictureEditingManager : IImagePropertyEditingContext
    {
        private CommandManager _commandManager;
        private CreateFileCallback _createFileCallback;
        private IHtmlEditorComponentContext _editorContext;
        private IBlogPostImageEditingContext _imageEditingContext;
        private ImageEditingPropertyHandler _imagePropertyHandler;
        private bool _alignmentMarginCommandsHooked;

        // Picture related commands.
        private AlignmentCommand _alignmentCommand;
        private Command _aspectRatioLockedCommand;
        private Command _customSizeDropDownCommand;
        private Command _customSizeSmallCommand;
        private Command _customSizeMediumCommand;
        private Command _customSizeLargeCommand;
        private Command _customSizeOriginalCommand;
        private Command _customSizeDefaultsCommand;
        private Command _imageAltTextCommand;
        private ImageEffectsBorderGalleryCommand _imageBorderGalleryCommand;
        private Command _imageContrastCommand;
        private Command _imageCropCommand;
        private ImageEffectsEmbossGalleryCommand _imageEffectsEmbossGalleryCommand;
        private Command _imageEffectsGalleryCommand;
        private ImageEffectsBlurGalleryCommand _imageEffectsGaussianGalleryCommand;
        private ImageEffectsRecolorGalleryCommand _imageEffectsRecolorGalleryCommand;
        private ImageEffectsSharpenGalleryCommand _imageEffectsSharpenGalleryCommand;
        private SpinnerCommand _imageHeightCommand;
        private Command _imageLinkOptionsCommand;
        private Command _imageLinkTargetDropDownCommand;
        private Command _imageLinkToSourceCommand;
        private Command _imageLinkToUrlCommand;
        private Command _imageLinkToNoneCommand;
        private ImagePropertiesInfo _imagePropertiesInfo;
        private Command _imageResetCommand;
        private Command _imageRotateCCWCommand;
        private Command _imageRotateCWCommand;
        private Command _imageSaveDefaultsCommand;
        private Command _imageTiltCommand;
        private Command _imageWatermarkCommand;
        private SpinnerCommand _imageWidthCommand;
        private MarginCommand _marginCommand;

        public PictureEditingManager(
            IHtmlEditorComponentContext editorContext,
            IBlogPostImageEditingContext imageEditingContext,
            CreateFileCallback createFileCallback)
        {
            _editorContext = editorContext;
            _imageEditingContext = imageEditingContext;
            _createFileCallback = createFileCallback;

            InitializeCommands();
        }

        private ImageEditingPropertyHandler ImagePropertyHandler
        {
            get
            {
                if (_imagePropertyHandler == null)
                {
                    Debug.Assert(SelectedImage != null, "No image selected!");
                    _imagePropertyHandler = new ImageEditingPropertyHandler(
                        this, _createFileCallback, _imageEditingContext);
                }
                return _imagePropertyHandler;
            }
        }

        #region IImagePropertyEditingContext Members

        public IHTMLImgElement SelectedImage
        {
            get { return _editorContext.Selection.SelectedImage; }
        }

        public ImagePropertiesInfo ImagePropertiesInfo
        {
            get { return _imagePropertiesInfo; }
            set
            {
                _imagePropertiesInfo = value;
                ManageCommands();
            }
        }

        public event ImagePropertyEventHandler ImagePropertyChanged;

        #endregion

        /// <summary>
        /// Called when the ImagePropertiesInfo object has changed in order to sync the commands and decorators with
        /// the currently selected image.
        /// </summary>
        private void ManageCommands()
        {
            using (IUndoUnit undo = _editorContext.CreateInvisibleUndoUnit())
            {
                if (_imageEditingContext != null)
                {
                    for (int i = 0; i < _imageEditingContext.DecoratorsManager.ImageDecoratorGroups.Length; i++)
                    {
                        ImageDecoratorGroup imageDecoratorGroup = _imageEditingContext.DecoratorsManager.ImageDecoratorGroups[i];
                        foreach (ImageDecorator imageDecorator in imageDecoratorGroup.ImageDecorators)
                        {
                            if (_imagePropertiesInfo == null || _imagePropertiesInfo.IsEditableEmbeddedImage() || imageDecorator.IsWebImageSupported)
                                imageDecorator.Command.Enabled = true;
                            else
                                imageDecorator.Command.Enabled = false;
                        }
                    }

                    ResetCommands();

                    if (ImagePropertiesInfo != null)
                        RefreshCommands();
                }
                undo.Commit();
            }
        }

        /// <summary>
        /// Applies changes to an image by firing the ImagePropertyChanged event.
        /// </summary>
        private void ApplyImageDecorations()
        {
            ApplyImageDecorations(ImagePropertyType.Decorators, ImageDecoratorInvocationSource.ImagePropertiesEditor);
        }

        /// <summary>
        /// Applies changes to an image by firing the ImagePropertyChanged event.
        /// </summary>
        private void ApplyImageDecorations(ImagePropertyType propertyType, ImageDecoratorInvocationSource source)
        {
            if (ImagePropertyChanged != null)
                OnImagePropertyChanged(new ImagePropertyEvent(propertyType, ImagePropertiesInfo, source));
        }

        /// <summary>
        /// Forces an update of the picture commands by recreating the ImagePropertiesInfo object.
        /// </summary>
        public void UpdateView(object htmlSelection)
        {
            if (htmlSelection != null)
            {
                // Image selected.
                HookAlignmentMarginCommands();
            }
            else
            {
                // No image selected.
                UnhookAlignmentMarginCommands();
            }

            ImagePropertyHandler.RefreshView();
        }

        /// <summary>
        /// The alignment and margin commands are used in other parts of the ribbon, so they should be hooked up only
        /// when needed.
        /// </summary>
        private void HookAlignmentMarginCommands()
        {
            if (!_alignmentMarginCommandsHooked)
            {
                _alignmentMarginCommandsHooked = true;
                _alignmentCommand.AlignmentChanged += new EventHandler(alignmentCommand_AlignmentChanged);
                _marginCommand.MarginChanged += new EventHandler(marginCommand_MarginChanged);

                // These are shared with other tabs, so when an image is selected we need to enable them.
                _alignmentCommand.Enabled = true;
                _marginCommand.Enabled = true;
            }
        }

        /// <summary>
        /// The alignment and margin commands are used in other parts of the ribbon, so they should be hooked up only
        /// when needed.
        /// </summary>
        private void UnhookAlignmentMarginCommands()
        {
            if (_alignmentMarginCommandsHooked)
            {
                _alignmentMarginCommandsHooked = false;
                _alignmentCommand.AlignmentChanged -= new EventHandler(alignmentCommand_AlignmentChanged);
                _marginCommand.MarginChanged -= new EventHandler(marginCommand_MarginChanged);

                // These are shared with other tabs, so when an image is deselected we need to disable them.
                _alignmentCommand.Enabled = false;
                _marginCommand.Enabled = false;
            }
        }

        private void InitializeCommands()
        {
            _commandManager = _editorContext.CommandManager;
            _commandManager.BeginUpdate();

            InitializeSizeChunkCommands();
            InitializeAlignmentMarginChunkCommands();
            InitializeRotateChunkCommands();
            InitializeStylesChunkCommands();
            InitializePropertiesChunkCommands();
            InitializeSettingsChunkCommand();

            _commandManager.EndUpdate();
        }

        private ImageCustomSizeDropdown _imageCustomSizeDropdown;
        private void InitializeSizeChunkCommands()
        {
            _imageCropCommand = new Command(CommandId.ImageCrop);
            _imageCropCommand.Tag = _imageEditingContext.DecoratorsManager.GetImageDecorator(CropDecorator.Id);
            _imageCropCommand.Execute += new EventHandler(imageDecoratorCommand_Execute); //Note: this handles its own undo
            _commandManager.Add(_imageCropCommand);

            _imageWidthCommand = new ImageSizeSpinnerCommand(CommandId.FormatImageAdjustWidth);
            _imageWidthCommand.Tag = _imageEditingContext.DecoratorsManager.GetImageDecorator(HtmlImageResizeDecorator.Id);
            _imageWidthCommand.ExecuteWithArgs += new ExecuteEventHandler(imageSizeCommand_ExecuteWithArgs);
            _commandManager.Add(_imageWidthCommand);

            _imageHeightCommand = new ImageSizeSpinnerCommand(CommandId.FormatImageAdjustHeight);
            _imageHeightCommand.Tag = _imageEditingContext.DecoratorsManager.GetImageDecorator(HtmlImageResizeDecorator.Id);
            _imageHeightCommand.ExecuteWithArgs += new ExecuteEventHandler(imageSizeCommand_ExecuteWithArgs);
            _commandManager.Add(_imageHeightCommand);

            _customSizeDropDownCommand = new Command(CommandId.CustomSizeGallery);
            _commandManager.Add(_customSizeDropDownCommand);

            _customSizeSmallCommand = new Command(CommandId.CustomSizeSmall, ImageSizeName.Small);
            _commandManager.Add(_customSizeSmallCommand);

            _customSizeMediumCommand = new Command(CommandId.CustomSizeMedium, ImageSizeName.Medium);
            _commandManager.Add(_customSizeMediumCommand);

            _customSizeLargeCommand = new Command(CommandId.CustomSizeLarge, ImageSizeName.Large);
            _commandManager.Add(_customSizeLargeCommand);

            _customSizeOriginalCommand = new Command(CommandId.CustomSizeOriginal, ImageSizeName.Full);
            _commandManager.Add(_customSizeOriginalCommand);

            _imageCustomSizeDropdown = new ImageCustomSizeDropdown(_customSizeDropDownCommand, new[] { _customSizeSmallCommand, _customSizeMediumCommand, _customSizeLargeCommand, _customSizeOriginalCommand }, customSizeCommand_Execute);

            _customSizeDefaultsCommand = new GroupCommand(CommandId.SetCustomSizeDefaults, _customSizeDropDownCommand);
            _customSizeDefaultsCommand.Execute += new EventHandler(customSizeDefaultsCommand_Execute);
            _commandManager.Add(_customSizeDefaultsCommand);

            _aspectRatioLockedCommand = new Command(CommandId.FormatImageLockAspectRatio);
            _aspectRatioLockedCommand.Execute += new EventHandler(aspectRatioLockedCommand_Execute);
            _commandManager.Add(_aspectRatioLockedCommand);

            _commandManager.Add(new GroupCommand(CommandId.FormatImageSizeGroup, _customSizeDropDownCommand));
        }

        private void InitializeAlignmentMarginChunkCommands()
        {
            _alignmentCommand = (AlignmentCommand)_editorContext.CommandManager.Get(CommandId.AlignmentGallery);
            _marginCommand = (MarginCommand)_editorContext.CommandManager.Get(CommandId.MarginsGroup);
        }

        private void InitializeRotateChunkCommands()
        {
            _imageRotateCWCommand = new Command(CommandId.ImageRotateCW);
            _imageRotateCWCommand.Execute += new UndoableCommandHandler(new EventHandler(imageRotateCWCommand_Execute), _editorContext).ExecuteHandler;
            _commandManager.Add(_imageRotateCWCommand);

            _imageRotateCCWCommand = new Command(CommandId.ImageRotateCCW);
            _imageRotateCCWCommand.Execute += new UndoableCommandHandler(new EventHandler(imageRotateCCWCommand_Execute), _editorContext).ExecuteHandler;
            _commandManager.Add(_imageRotateCCWCommand);

            _imageTiltCommand = new Command(CommandId.ImageTilt);
            _imageTiltCommand.Tag = _imageEditingContext.DecoratorsManager.GetImageDecorator(TiltDecorator.Id);
            _imageTiltCommand.Execute += new EventHandler(imageDecoratorCommand_Execute);
            _commandManager.Add(_imageTiltCommand);

            _commandManager.Add(new GroupCommand(CommandId.FormatImageRotateGroup, _imageRotateCWCommand));
        }

        private void InitializeStylesChunkCommands()
        {
            _imageBorderGalleryCommand = new ImageEffectsBorderGalleryCommand(CommandId.ImageBorderGallery);
            _imageBorderGalleryCommand.ExecuteWithArgs += new ExecuteEventHandler(imageEffectsGalleryCommand_ExecuteWithArgs);
            _commandManager.Add(_imageBorderGalleryCommand);

            _imageEffectsGalleryCommand = new Command(CommandId.ImageEffectsGallery);
            _commandManager.Add(_imageEffectsGalleryCommand);

            _imageEffectsRecolorGalleryCommand = new ImageEffectsRecolorGalleryCommand(CommandId.ImageEffectsRecolorGallery);
            _imageEffectsRecolorGalleryCommand.ExecuteWithArgs += new ExecuteEventHandler(imageEffectsGalleryCommand_ExecuteWithArgs);
            _commandManager.Add(_imageEffectsRecolorGalleryCommand);

            _imageEffectsSharpenGalleryCommand = new ImageEffectsSharpenGalleryCommand(CommandId.ImageEffectsSharpenGallery);
            _imageEffectsSharpenGalleryCommand.ExecuteWithArgs += new ExecuteEventHandler(imageEffectsGalleryCommand_ExecuteWithArgs);
            _commandManager.Add(_imageEffectsSharpenGalleryCommand);

            _imageEffectsGaussianGalleryCommand = new ImageEffectsBlurGalleryCommand(CommandId.ImageEffectsBlurGallery);
            _imageEffectsGaussianGalleryCommand.ExecuteWithArgs += new ExecuteEventHandler(imageEffectsGalleryCommand_ExecuteWithArgs);
            _commandManager.Add(_imageEffectsGaussianGalleryCommand);

            _imageEffectsEmbossGalleryCommand = new ImageEffectsEmbossGalleryCommand(CommandId.ImageEffectsEmbossGallery);
            _imageEffectsEmbossGalleryCommand.ExecuteWithArgs += new ExecuteEventHandler(imageEffectsGalleryCommand_ExecuteWithArgs);
            _commandManager.Add(_imageEffectsEmbossGalleryCommand);

            _imageContrastCommand = new Command(CommandId.ImageContrast);
            _imageContrastCommand.Tag = _imageEditingContext.DecoratorsManager.GetImageDecorator(BrightnessDecorator.Id);
            _imageContrastCommand.Execute += new EventHandler(imageDecoratorCommand_Execute); //Note: this handles its own undo
            _commandManager.Add(_imageContrastCommand);

            _imageWatermarkCommand = new Command(CommandId.Watermark);
            _imageWatermarkCommand.Tag = _imageEditingContext.DecoratorsManager.GetImageDecorator(WatermarkDecorator.Id);
            _imageWatermarkCommand.Execute += new EventHandler(imageDecoratorCommand_Execute); //Note: this handles its own undo
            _commandManager.Add(_imageWatermarkCommand);

            _commandManager.Add(new Command(CommandId.FormatImageStyleGroup));
        }

        private ImageLinkTargetDropdown _imageLinkTargetDropdown;
        private void InitializePropertiesChunkCommands()
        {
            _imageLinkTargetDropDownCommand = new Command(CommandId.FormatImageSelectLink);
            _commandManager.Add(_imageLinkTargetDropDownCommand);

            _imageLinkToSourceCommand = new Command(CommandId.ImageLinkToSource, LinkTargetType.IMAGE);
            _commandManager.Add(_imageLinkToSourceCommand);

            _imageLinkToUrlCommand = new Command(CommandId.ImageLinkToUrl, LinkTargetType.URL);
            _commandManager.Add(_imageLinkToUrlCommand);

            _imageLinkToNoneCommand = new Command(CommandId.ImageLinkToNone, LinkTargetType.NONE);
            _commandManager.Add(_imageLinkToNoneCommand);

            _imageLinkTargetDropdown = new ImageLinkTargetDropdown(_imageLinkTargetDropDownCommand, new[] { _imageLinkToSourceCommand, _imageLinkToUrlCommand, _imageLinkToNoneCommand }, imageTargetSelectGalleryCommand_Execute);
            _imageLinkTargetDropdown.SelectTag(LinkTargetType.IMAGE);

            _imageLinkOptionsCommand = new CustomTooltipWhenDisabledCommand(CommandId.FormatImageLinkOptions, Res.Get(StringId.ImgSBLinkOptionsDisabledTooltip));
            _commandManager.Add(_imageLinkOptionsCommand, imageLinkOptionsCommand_Execute);

            _imageAltTextCommand = new Command(CommandId.FormatImageAltText);
            _imageAltTextCommand.Tag = _imageEditingContext.DecoratorsManager.GetImageDecorator(HtmlAltTextDecorator.Id);
            _imageAltTextCommand.Execute += new EventHandler(imageDecoratorCommand_Execute);
            _commandManager.Add(_imageAltTextCommand);

            _commandManager.Add(new GroupCommand(CommandId.FormatImagePropertiesGroup, _imageLinkOptionsCommand));
        }

        private void InitializeSettingsChunkCommand()
        {
            _imageResetCommand = new Command(CommandId.FormatImageRevertSettings);
            _imageResetCommand.Execute += new UndoableCommandHandler(new EventHandler(imageResetCommand_Execute), _editorContext).ExecuteHandler;
            _commandManager.Add(_imageResetCommand);

            _imageSaveDefaultsCommand = new Command(CommandId.ImageSaveDefaults);
            _imageSaveDefaultsCommand.Execute += new EventHandler(imageSaveDefaultsCommand_Execute);//no undo necessary
            _commandManager.Add(_imageSaveDefaultsCommand);

            _commandManager.Add(new GroupCommand(CommandId.FormatImageSettingsGroup, _imageSaveDefaultsCommand));
        }

        /// <summary>
        /// Initializes ribbon commands when a new image is loaded.
        /// </summary>
        private void ResetCommands()
        {
            bool isImageSelected = ImagePropertiesInfo != null;
            bool isEditableEmbeddedImage = isImageSelected && ImagePropertiesInfo.IsEditableEmbeddedImage();

            ResetSizeChunkCommands(isImageSelected, isEditableEmbeddedImage);
            ResetAlignmentChunkCommands(isImageSelected, isEditableEmbeddedImage);
            ResetMarginsChunkCommands(isImageSelected, isEditableEmbeddedImage);
            ResetStylesChunkCommands(isImageSelected, isEditableEmbeddedImage);
            ResetRotateChunkCommands(isImageSelected, isEditableEmbeddedImage);
            ResetPropertiesChunkCommands(isImageSelected, isEditableEmbeddedImage);
            ResetSettingsChunkCommands(isImageSelected, isEditableEmbeddedImage);
        }

        /// <summary>
        /// Initializes ribbon commands when a new image is loaded.
        /// </summary>
        private void ResetSizeChunkCommands(bool isImageSelected, bool isEditableEmbeddedImage)
        {
            // Set enabled state.
            _imageCropCommand.Enabled = isEditableEmbeddedImage;
            _imageWidthCommand.Enabled = isImageSelected;
            _imageHeightCommand.Enabled = isImageSelected;
            _imageCustomSizeDropdown.Enabled = isImageSelected;
            _customSizeDefaultsCommand.Enabled = isImageSelected;
            _aspectRatioLockedCommand.Enabled = isImageSelected;

            // Set any defaults.
            _imageCustomSizeDropdown.SelectTag(ImageSizeName.Custom);

            if (_aspectRatioLockedCommand.Enabled)
            {
                _aspectRatioLockedCommand.Latched = ImagePropertiesInfo.InlineImageAspectRatioLocked;
            }
        }

        /// <summary>
        /// Initializes ribbon commands when a new image is loaded.
        /// </summary>
        private void ResetAlignmentChunkCommands(bool isImageSelected, bool isEditableEmbeddedImage)
        {
            // The alignment command is shared among different contextual tabs, so we don't set the enabled state here.
            if (_alignmentCommand.Enabled)
                _alignmentCommand.SetSelectedItem(ImagePropertiesInfo.InlineImageAlignment);
        }

        /// <summary>
        /// Initializes ribbon commands when a new image is loaded.
        /// </summary>
        private void ResetMarginsChunkCommands(bool isImageSelected, bool isEditableEmbeddedImage)
        {
            // The margins command is shared among different contextual tabs, so we don't set the enabled state here.
            if (_marginCommand.Enabled)
            {
                MarginStyle marginStyle = ImagePropertiesInfo.InlineImageMargin;
                _marginCommand.Value = new Padding(marginStyle.Left, marginStyle.Top, marginStyle.Right, marginStyle.Bottom);
            }
        }

        /// <summary>
        /// Initializes ribbon commands when a new image is loaded.
        /// </summary>
        private void ResetStylesChunkCommands(bool isImageSelected, bool isEditableEmbeddedImage)
        {
            // Set enabled state.
            _imageBorderGalleryCommand.Enabled = isImageSelected;
            _imageEffectsGalleryCommand.Enabled = isEditableEmbeddedImage;
            _imageEffectsRecolorGalleryCommand.Enabled = isEditableEmbeddedImage;
            _imageEffectsSharpenGalleryCommand.Enabled = isEditableEmbeddedImage;
            _imageEffectsGaussianGalleryCommand.Enabled = isEditableEmbeddedImage;
            _imageEffectsEmbossGalleryCommand.Enabled = isEditableEmbeddedImage;
            _imageContrastCommand.Enabled = isEditableEmbeddedImage;
            _imageWatermarkCommand.Enabled = isEditableEmbeddedImage;

            // Invalidate any galleries.
            if (_imageBorderGalleryCommand.Enabled)
            {
                _imageBorderGalleryCommand.DecoratorsManager = _imageEditingContext.DecoratorsManager;
                _imageBorderGalleryCommand.Invalidate();
            }

            if (_imageEffectsGalleryCommand.Enabled)
            {
                _imageEffectsRecolorGalleryCommand.DecoratorsManager = _imageEditingContext.DecoratorsManager;
                _imageEffectsRecolorGalleryCommand.Invalidate();
            }

            if (_imageEffectsSharpenGalleryCommand.Enabled)
            {
                _imageEffectsSharpenGalleryCommand.DecoratorsManager = _imageEditingContext.DecoratorsManager;
                _imageEffectsSharpenGalleryCommand.Invalidate();
            }

            if (_imageEffectsGaussianGalleryCommand.Enabled)
            {
                _imageEffectsGaussianGalleryCommand.DecoratorsManager = _imageEditingContext.DecoratorsManager;
                _imageEffectsGaussianGalleryCommand.Invalidate();
            }

            if (_imageEffectsEmbossGalleryCommand.Enabled)
            {
                _imageEffectsEmbossGalleryCommand.DecoratorsManager = _imageEditingContext.DecoratorsManager;
                _imageEffectsEmbossGalleryCommand.Invalidate();
            }
        }

        /// <summary>
        /// Initializes ribbon commands when a new image is loaded.
        /// </summary>
        private void ResetRotateChunkCommands(bool isImageSelected, bool isEditableEmbeddedImage)
        {
            // Set enabled state.
            _imageRotateCWCommand.Enabled = isEditableEmbeddedImage;
            _imageRotateCCWCommand.Enabled = isEditableEmbeddedImage;
            _imageTiltCommand.Enabled = isEditableEmbeddedImage;
        }

        /// <summary>
        /// Initializes ribbon commands when a new image is loaded.
        /// </summary>
        private void ResetPropertiesChunkCommands(bool isImageSelected, bool isEditableEmbeddedImage)
        {
            // Set enabled state.
            _imageLinkTargetDropdown.Enabled = isImageSelected;
            _imageLinkToSourceCommand.Enabled = isImageSelected && UrlHelper.IsFileUrl(ImagePropertiesInfo.ImageSourceUri.ToString()) && GlobalEditorOptions.SupportsFeature(ContentEditorFeature.SupportsImageClickThroughs);
            _imageLinkOptionsCommand.Enabled = isImageSelected;
            _imageAltTextCommand.Enabled = isImageSelected;

            // Set any defaults.
            _imageLinkTargetDropdown.SelectTag(LinkTargetType.NONE);
        }

        /// <summary>
        /// Initializes ribbon commands when a new image is loaded.
        /// </summary>
        private void ResetSettingsChunkCommands(bool isImageSelected, bool isEditableEmbeddedImage)
        {
            // Set enabled state.
            _imageSaveDefaultsCommand.Enabled = isEditableEmbeddedImage;
            _imageResetCommand.Enabled = isEditableEmbeddedImage;
        }

        /// <summary>
        /// Refreshes ribbon commands when an image property has changed.
        /// </summary>
        private void RefreshCommands()
        {
            RefreshSizeChunkCommands();
            RefreshStylesChunkCommands();
            RefreshPropertiesChunkCommands();
        }

        /// <summary>
        /// Refreshes ribbon commands when an image property has changed.
        /// </summary>
        private void RefreshSizeChunkCommands()
        {
            _imageWidthCommand.Value = ImagePropertiesInfo.InlineImageWidth;
            _imageHeightCommand.Value = ImagePropertiesInfo.InlineImageHeight;
            _imageCustomSizeDropdown.SelectTag(ImagePropertiesInfo.InlineImageSizeName);
            _aspectRatioLockedCommand.Latched = ImagePropertiesInfo.InlineImageAspectRatioLocked;
        }

        /// <summary>
        /// Refreshes ribbon commands when an image property has changed.
        /// </summary>
        private void RefreshStylesChunkCommands()
        {
            if (ImagePropertiesInfo.ImageDecorators != null)
            {
                _imageBorderGalleryCommand.Enabled = true;
                _imageBorderGalleryCommand.SelectedItem = ImagePropertiesInfo.ImageDecorators.BorderImageDecorator.Id;

                if (ImagePropertiesInfo.IsEditableEmbeddedImage())
                {
                    _imageEffectsRecolorGalleryCommand.SelectedItem = ImagePropertiesInfo.ImageDecorators.RecolorImageDecorator.Id;
                    _imageEffectsSharpenGalleryCommand.SelectedItem = ImagePropertiesInfo.ImageDecorators.SharpenImageDecorator.Id;
                    _imageEffectsGaussianGalleryCommand.SelectedItem = ImagePropertiesInfo.ImageDecorators.BlurImageDecorator.Id;
                    _imageEffectsEmbossGalleryCommand.SelectedItem = ImagePropertiesInfo.ImageDecorators.EmbossImageDecorator.Id;
                }
            }

            _imageBorderGalleryCommand.Enabled = (ImagePropertiesInfo.ImageDecorators != null);

            bool isEditableEmbeddedImage = ImagePropertiesInfo.IsEditableEmbeddedImage();
            _imageEffectsRecolorGalleryCommand.Enabled = isEditableEmbeddedImage;
            _imageEffectsSharpenGalleryCommand.Enabled = isEditableEmbeddedImage;
            _imageEffectsGaussianGalleryCommand.Enabled = isEditableEmbeddedImage;
            _imageEffectsEmbossGalleryCommand.Enabled = isEditableEmbeddedImage;
        }

        /// <summary>
        /// Refreshes ribbon commands when an image property has changed.
        /// </summary>
        private void RefreshPropertiesChunkCommands()
        {
            _imageLinkTargetDropdown.SelectTag(ImagePropertiesInfo.LinkTarget);
            _imageLinkOptionsCommand.Enabled = (ImagePropertiesInfo.LinkTarget != LinkTargetType.NONE);
        }

        private void imageDecoratorCommand_Execute(object sender, EventArgs e)
        {
            using (IUndoUnit undo = _editorContext.CreateUndoUnit())
            {
                ImageDecorator imageDecorator = ((ImageDecorator)((Command)sender).Tag);

                //perform the execution so that the decorator is added to the list of active decorators
                imageDecorator.Command.PerformExecute();

                //since this command was invoked explicitly via a command button, display the editor dialog.
                object state = null;
                if (imageDecorator.Id == CropDecorator.Id)
                    state = ImagePropertiesInfo.Image;

                using (state as IDisposable)
                {
                    DialogResult result = ImageDecoratorHelper.ShowImageDecoratorEditorDialog(imageDecorator,
                        ImagePropertiesInfo, new ApplyDecoratorCallback(ApplyImageDecorations),
                        _editorContext, state, _imageEditingContext, _imageEditingContext.DecoratorsManager.CommandManager);

                    if (result == DialogResult.OK)
                        undo.Commit();
                }
            }
        }

        void imageSizeCommand_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
        {
            SpinnerCommand command = (SpinnerCommand)sender;
            int newValue = Convert.ToInt32(args.GetDecimal(command.CommandId.ToString()));

            using (IUndoUnit undo = _editorContext.CreateUndoUnit())
            {
                if (command.CommandId == CommandId.FormatImageAdjustWidth && newValue != ImagePropertiesInfo.InlineImageWidth)
                {
                    ImagePropertiesInfo.InlineImageWidth = newValue;
                    ApplyImageDecorations(ImagePropertyType.InlineSize, ImageDecoratorInvocationSource.Command);
                    undo.Commit();
                }
                else if (command.CommandId == CommandId.FormatImageAdjustHeight && newValue != ImagePropertiesInfo.InlineImageHeight)
                {
                    ImagePropertiesInfo.InlineImageHeight = newValue;
                    ApplyImageDecorations(ImagePropertyType.InlineSize, ImageDecoratorInvocationSource.Command);
                    undo.Commit();
                }
            }
        }

        private void customSizeCommand_Execute(object sender, EventArgs args)
        {
            Command selectedCommand = (Command)sender;

            using (IUndoUnit undo = _editorContext.CreateUndoUnit())
            {
                ImagePropertiesInfo.InlineImageSizeName = (ImageSizeName)selectedCommand.Tag;
                ApplyImageDecorations();
                undo.Commit();
            }
        }

        private void customSizeDefaultsCommand_Execute(object sender, EventArgs e)
        {
            using (new WaitCursor())
            using (EditImageSizesDialog dialog = new EditImageSizesDialog())
            {
                ImageSizeName imageSizeName = ImagePropertiesInfo.InlineImageSizeName;
                if (dialog.ShowDialog(_editorContext.MainFrameWindow) == DialogResult.OK)
                {
                    // If the current ImageSizeName's settings changed, we should resize the image.
                    ImagePropertiesInfo.InlineImageSizeName = imageSizeName;
                    ApplyImageDecorations(ImagePropertyType.InlineSize, ImageDecoratorInvocationSource.Command);
                }
            }
        }

        private void aspectRatioLockedCommand_Execute(object sender, EventArgs e)
        {
            Command command = (Command)sender;
            command.Latched = !command.Latched;
            ImagePropertiesInfo.InlineImageAspectRatioLocked = command.Latched;
            ImagePropertiesInfo.TargetAspectRatioSize = ImagePropertiesInfo.InlineImageSize;
            ApplyImageDecorations();
        }

        private void alignmentCommand_AlignmentChanged(object sender, EventArgs e)
        {
            if (ImagePropertiesInfo.InlineImageAlignment != _alignmentCommand.SelectedItem)
            {
                using (IUndoUnit undo = _editorContext.CreateUndoUnit())
                {
                    ImagePropertiesInfo.InlineImageAlignment = _alignmentCommand.SelectedItem;

                    // Center aligning an image overwrites the right and left margins.
                    if (_alignmentCommand.SelectedItem == Alignment.Center)
                    {
                        bool isImageSelected = ImagePropertiesInfo != null;
                        bool isEditableEmbeddedImage = isImageSelected && ImagePropertiesInfo.IsEditableEmbeddedImage();
                        ResetMarginsChunkCommands(isImageSelected, isEditableEmbeddedImage);
                    }

                    undo.Commit();
                }
            }
        }

        private void marginCommand_MarginChanged(object sender, EventArgs e)
        {
            using (IUndoUnit undo = _editorContext.CreateUndoUnit())
            {
                bool centered = _alignmentCommand.SelectedItem == Alignment.Center;
                if (!_marginCommand.IsZero())
                    ImagePropertiesInfo.InlineImageMargin = new MarginStyle(_marginCommand.Top, _marginCommand.Right, _marginCommand.Bottom, _marginCommand.Left, StyleSizeUnit.PX);
                else if (!centered)
                    ImagePropertiesInfo.InlineImageMargin = null;

                // Changing the right or left margins on a center aligned image resets the alignment.
                if ((_marginCommand.Right > 0 || _marginCommand.Left > 0) && centered)
                {
                    bool isImageSelected = ImagePropertiesInfo != null;
                    bool isEditableEmbeddedImage = isImageSelected && ImagePropertiesInfo.IsEditableEmbeddedImage();
                    ResetAlignmentChunkCommands(isImageSelected, isEditableEmbeddedImage);
                }

                undo.Commit();
            }
        }

        void imageEffectsGalleryCommand_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
        {
            ImageEffectsGalleryCommand galleryCommand = (ImageEffectsGalleryCommand)sender;
            int newSelectedIndex = args.GetInt(galleryCommand.CommandId.ToString());
            string newDecoratorId = galleryCommand.Items[newSelectedIndex].Cookie;
            ImageDecorator newDecorator = _imageEditingContext.DecoratorsManager.GetImageDecorator(newDecoratorId);

            Debug.Assert(newDecorator != null);

            if (galleryCommand.SelectedItem != newDecorator.Id)
            {
                ImagePropertiesInfo.ImageDecorators.AddDecorator(newDecorator);

                if (!ImagePropertiesInfo.IsEditableEmbeddedImage())
                {
                    // If this is a web image, calling ApplyImageDecorations will keep properties up to date but won't
                    // actually do any decorating, so do it manually. Only borders should be manually decorated.
                    SimpleImageDecoratorContext context = new SimpleImageDecoratorContext(ImagePropertiesInfo);
                    newDecorator.Decorate(context);
                }

                ApplyImageDecorations();
            }
        }

        private void imageRotateCWCommand_Execute(object sender, EventArgs e)
        {
            ImagePropertiesInfo.ImageRotation = ImageDecoratorUtils.GetFlipTypeRotatedCW(ImagePropertiesInfo.ImageRotation);
            ApplyImageDecorations();
        }

        private void imageRotateCCWCommand_Execute(object sender, EventArgs e)
        {
            ImagePropertiesInfo.ImageRotation = ImageDecoratorUtils.GetFlipTypeRotatedCCW(ImagePropertiesInfo.ImageRotation);
            ApplyImageDecorations();
        }

        void imageTargetSelectGalleryCommand_Execute(object sender, EventArgs args)
        {
            // By the time this is called we've already changed the dropdown
            // If user cancels then we need to restore previous dropdown selection.
            Command selectedCommand = (Command)sender;
            LinkTargetType newLinkTargetType = (LinkTargetType)selectedCommand.Tag;

            using (new WaitCursor())
            using (IUndoUnit undo = _editorContext.CreateUndoUnit())
            {

                if (newLinkTargetType == LinkTargetType.URL && EditTargetOptions(newLinkTargetType) != DialogResult.OK)
                {
                    newLinkTargetType = ImagePropertiesInfo.LinkTarget;
                    _imageLinkTargetDropdown.SelectTag(newLinkTargetType);
                }

                ImagePropertiesInfo.LinkTarget = newLinkTargetType;
                ApplyImageDecorations();
                undo.Commit();
            }
        }

        void imageLinkOptionsCommand_Execute(object sender, EventArgs e)
        {
            using (new WaitCursor())
            using (IUndoUnit undo = _editorContext.CreateUndoUnit())
            {
                if (EditTargetOptions(ImagePropertiesInfo.LinkTarget) == DialogResult.OK)
                {
                    ApplyImageDecorations();
                    undo.Commit();
                }
            }
        }

        private void imageResetCommand_Execute(object sender, EventArgs e)
        {
            // get the default settings
            DefaultImageSettings defaultImageSettings = new DefaultImageSettings(_imageEditingContext.CurrentAccountId, _imageEditingContext.DecoratorsManager);
            ImageDecoratorsList defaultDecoratorsList = defaultImageSettings.LoadDefaultImageDecoratorsList();

            // reset them
            ImagePropertiesInfo.ResetImageSettings(defaultDecoratorsList);

            // apply decorations
            ApplyImageDecorations(ImagePropertyType.Decorators, ImageDecoratorInvocationSource.Reset);
        }

        private void imageSaveDefaultsCommand_Execute(object sender, EventArgs e)
        {
            DefaultImageSettings defaultImageSettings = new DefaultImageSettings(_imageEditingContext.CurrentAccountId, _imageEditingContext.DecoratorsManager);
            defaultImageSettings.SaveAsDefault(ImagePropertiesInfo);
        }

        protected virtual void OnImagePropertyChanged(ImagePropertyEvent evt)
        {
            using (IUndoUnit undo = _editorContext.CreateUndoUnit())
            {
                if (ImagePropertyChanged != null)
                    ImagePropertyChanged(this, evt);

                undo.Commit();
            }

            switch (evt.PropertyType)
            {
                case ImagePropertyType.Decorators:
                    {
                        RefreshSizeChunkCommands();
                        RefreshStylesChunkCommands();
                        RefreshPropertiesChunkCommands();
                        break;
                    }
                case ImagePropertyType.InlineSize:
                    RefreshSizeChunkCommands();
                    break;
                case ImagePropertyType.Source:
                    RefreshPropertiesChunkCommands();
                    break;
                default:
                    RefreshCommands();
                    break;
            }

            // If this is a reset, then handle alignment/margins
            if (evt.InvocationSource == ImageDecoratorInvocationSource.Reset)
            {
                // Reset margins and alignments
                bool isImageSelected = ImagePropertiesInfo != null;
                bool isEditableEmbeddedImage = isImageSelected && ImagePropertiesInfo.IsEditableEmbeddedImage();

                ResetAlignmentChunkCommands(isImageSelected, isEditableEmbeddedImage);
                ResetMarginsChunkCommands(isImageSelected, isEditableEmbeddedImage);
            }
        }

        public void UpdateInlineImageSize(Size newSize, ImageDecoratorInvocationSource invocationSource, HtmlEditorControl currentEditor)
        {
            ImagePropertiesInfo.InlineImageSize = newSize;
            ApplyImageDecorations(ImagePropertyType.InlineSize, invocationSource);
        }

        public void UpdateImageLink(string newLink, string title, bool newWindow, string rel, ImageDecoratorInvocationSource invocationSource)
        {
            if (newLink == String.Empty)
            {
                ImagePropertiesInfo.LinkTarget = LinkTargetType.NONE;
            }
            else
            {
                if ((ImagePropertiesInfo.LinkTarget != LinkTargetType.IMAGE) || (ImagePropertiesInfo.LinkTargetUrl != newLink))
                {
                    ImagePropertiesInfo.LinkTarget = LinkTargetType.URL;
                    ImagePropertiesInfo.LinkTargetUrl = newLink;
                }
                ImagePropertiesInfo.UpdateImageLinkOptions(title, rel, newWindow);
            }

            ApplyImageDecorations(ImagePropertyType.Decorators, invocationSource);
        }

        private DialogResult EditTargetOptions(LinkTargetType linkTargetType)
        {
            using (LinkToOptionsForm linkOptionsForm = new LinkToOptionsForm())
            {
                if (linkTargetType == LinkTargetType.IMAGE)
                {
                    using (ImageTargetEditorControl editor = new ImageTargetEditorControl())
                    {
                        editor.LoadImageSize(ImagePropertiesInfo.LinkTargetImageSize, ImagePropertiesInfo.ImageSourceSize, ImagePropertiesInfo.ImageRotation);
                        editor.LinkOptions = ImagePropertiesInfo.LinkOptions;
                        editor.EditorOptions = _imageEditingContext.EditorOptions;
                        linkOptionsForm.EditorControl = editor;

                        ImagePropertiesInfo.DhtmlImageViewer = _imageEditingContext.EditorOptions.DhtmlImageViewer;

                        DialogResult result = linkOptionsForm.ShowDialog(_editorContext.MainFrameWindow);
                        if (result == DialogResult.OK)
                        {
                            ImagePropertiesInfo.LinkTargetImageSize = editor.ImageSize;
                            ImagePropertiesInfo.DhtmlImageViewer = _imageEditingContext.EditorOptions.DhtmlImageViewer;
                            ImagePropertiesInfo.LinkOptions = editor.LinkOptions;
                            ImagePropertiesInfo.LinkTargetImageSizeName = editor.ImageBoundsSize;
                        }
                        return result;
                    }
                }
                else if (linkTargetType == LinkTargetType.URL)
                {
                    using (HyperlinkForm hyperlinkForm = new HyperlinkForm(_editorContext.CommandManager, GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShowAllLinkOptions)))
                    {
                        hyperlinkForm.ContainsImage = true;
                        hyperlinkForm.EditStyle = !String.IsNullOrEmpty(ImagePropertiesInfo.LinkTargetUrl);
                        hyperlinkForm.NewWindow = ImagePropertiesInfo.LinkOptions.ShowInNewWindow;
                        if (ImagePropertiesInfo.LinkTitle != String.Empty)
                            hyperlinkForm.LinkTitle = ImagePropertiesInfo.LinkTitle;
                        if (ImagePropertiesInfo.LinkRel != String.Empty)
                            hyperlinkForm.Rel = ImagePropertiesInfo.LinkRel;
                        if (ImagePropertiesInfo.LinkTargetUrl != null && ImagePropertiesInfo.LinkTarget != LinkTargetType.IMAGE)
                        {
                            hyperlinkForm.Hyperlink = ImagePropertiesInfo.LinkTargetUrl;
                        }

                        DialogResult result = hyperlinkForm.ShowDialog(_editorContext.MainFrameWindow);
                        if (result == DialogResult.OK)
                        {
                            ImagePropertiesInfo.LinkTargetUrl = hyperlinkForm.Hyperlink;
                            ImagePropertiesInfo.UpdateImageLinkOptions(hyperlinkForm.LinkTitle, hyperlinkForm.Rel, hyperlinkForm.NewWindow);
                            ImagePropertiesInfo.LinkOptions = new LinkOptions(hyperlinkForm.NewWindow, false, null);
                        }
                        return result;
                    }
                }

                return DialogResult.Abort;
            }
        }

        private class UndoableCommandHandler
        {
            private EventHandler _commandHandler;
            private IUndoUnitFactory _undoFactory;
            public UndoableCommandHandler(EventHandler commandHandler, IUndoUnitFactory undoFactory)
            {
                _commandHandler = commandHandler;
                _undoFactory = undoFactory;
            }

            public EventHandler ExecuteHandler
            {
                get { return new EventHandler(ExecuteCommand); }
            }

            public void ExecuteCommand(object sender, EventArgs e)
            {
                using (IUndoUnit undo = _undoFactory.CreateUndoUnit())
                {
                    _commandHandler(sender, e);
                    undo.Commit();
                }
            }
        }
    }

    public class SimpleImageDecoratorContext : ImageDecoratorContext
    {
        private ImagePropertiesInfo _imageInfo;
        public SimpleImageDecoratorContext(ImagePropertiesInfo imageInfo)
        {
            _imageInfo = imageInfo;
        }

        #region ImageDecoratorContext Members

        public Bitmap Image
        {
            get
            {
                return _imageInfo.Image;
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public ImageBorderMargin BorderMargin
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public IProperties Settings
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public IEditorOptions EditorOptions
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public ImageEmbedType ImageEmbedType
        {
            get { return ImageEmbedType.Linked; }
        }

        public IHTMLElement ImgElement
        {
            get { return _imageInfo.ImgElement; }
        }

        public ImageDecoratorInvocationSource InvocationSource
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public RotateFlipType ImageRotation
        {
            get { return _imageInfo.ImageRotation; }
        }

        public Uri SourceImageUri
        {
            get { return _imageInfo.ImageSourceUri; }
        }

        public float? EnforcedAspectRatio
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
