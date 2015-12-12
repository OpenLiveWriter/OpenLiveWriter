// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Drawing;
using System.Text;
using OpenLiveWriter.Api;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Manages the loading and storing of the default image settings for a specified context.
    /// </summary>
    internal class DefaultImageSettings
    {
        string _contextId;
        ImageDecoratorsManager _decoratorsManager;
        public DefaultImageSettings(string contextId, ImageDecoratorsManager decoratorsManager)
        {
            _contextId = contextId;
            _decoratorsManager = decoratorsManager;
        }

        public void SaveAsDefault(ImagePropertiesInfo imageInfo)
        {
            ImageDecoratorsList decorators = imageInfo.ImageDecorators;
            //delete the existing image settings
            DeleteImageSettings(_contextId);

            using (SettingsPersisterHelper contextImageSettings = GetImageSettingsPersister(_contextId, true))
            {
                //loop over the image decorators and save the settings of any decorators that are defaultable.
                StringBuilder sb = new StringBuilder();
                foreach (string decoratorId in decorators.GetImageDecoratorIds())
                {
                    ImageDecorator decorator = decorators.GetImageDecorator(decoratorId);
                    if (decorator.IsDefaultable)
                    {
                        if (sb.Length > 0)
                            sb.Append(",");
                        sb.Append(decorator.Id);
                        using (SettingsPersisterHelper decoratorDefaultSettings = contextImageSettings.GetSubSettings(decoratorId))
                        {
                            PluginSettingsAdaptor settings = new PluginSettingsAdaptor(decoratorDefaultSettings);
                            IProperties decoratorCurrentSettings = decorators.GetImageDecoratorSettings(decoratorId);
                            CopySettings(decoratorCurrentSettings, settings);
                            ImageDecoratorEditorContext editorContext =
                                new ImageDecoratorEditorContextImpl(decoratorCurrentSettings, null, imageInfo, new NoOpUndoUnitFactory(), _decoratorsManager.CommandManager);
                            decorator.ApplyCustomizeDefaultSettingsHook(editorContext, settings);
                        }
                    }
                }
                contextImageSettings.SetString(ImageDecoratorsListKey, sb.ToString());
            }
        }

        private const string ImageDefaultsKey = "ImageDefaults";
        private const string ImageDecoratorsListKey = "Decorators";
        private void CopySettings(IProperties sourceSettings, IProperties targetSettings)
        {
            foreach (string key in sourceSettings.Names)
            {
                targetSettings.SetString(key, sourceSettings.GetString(key, null));
            }
        }

        /// <summary>
        /// Load the default image decorators for the current image context.
        /// </summary>
        /// <returns></returns>
        public ImageDecoratorsList LoadDefaultImageDecoratorsList()
        {
            using (SettingsPersisterHelper contextImageSettings = GetImageSettingsPersister(_contextId, false))
            {
                ImageDecoratorsList decoratorsList;
                if (contextImageSettings == null)
                {
                    decoratorsList = GetInitialLocalImageDecoratorsList();
                }
                else
                {
                    decoratorsList = new ImageDecoratorsList(_decoratorsManager, new BlogPostSettingsBag(), false);
                    string[] decoratorIds = contextImageSettings.GetString(ImageDecoratorsListKey, "").Split(',');
                    foreach (string decoratorId in decoratorIds)
                    {
                        ImageDecorator imageDecorator = _decoratorsManager.GetImageDecorator(decoratorId);
                        if (imageDecorator != null) //can be null if the decorator is no longer valid
                        {
                            decoratorsList.AddDecorator(imageDecorator);
                            using (SettingsPersisterHelper decoratorDefaultSettings = contextImageSettings.GetSubSettings(decoratorId))
                            {
                                PluginSettingsAdaptor settings = new PluginSettingsAdaptor(decoratorDefaultSettings);
                                CopySettings(settings, decoratorsList.GetImageDecoratorSettings(decoratorId));
                            }
                        }
                    }
                }
                //now add the implicit decorators IFF they aren't already in the list
                decoratorsList.MergeDecorators(GetImplicitLocalImageDecorators());
                return decoratorsList;
            }
        }

        /// <summary>
        /// Returns the default image size for inline images.
        /// </summary>
        /// <returns></returns>
        public Size GetDefaultInlineImageSize()
        {
            ImageDecoratorsList decoratorsList = LoadDefaultImageDecoratorsList();
            IProperties props = decoratorsList.GetImageDecoratorSettings(HtmlImageResizeDecorator.Id);
            Size imgSize = HtmlImageResizeDecoratorSettings.GetDefaultImageSize(props);
            return imgSize;
        }

        /// <summary>
        /// Returns the default margin for images.
        /// </summary>
        /// <returns></returns>
        public MarginStyle GetDefaultImageMargin()
        {
            ImageDecoratorsList decoratorsList = LoadDefaultImageDecoratorsList();
            IProperties props = decoratorsList.GetImageDecoratorSettings(HtmlMarginDecorator.Id);
            return HtmlMarginDecoratorSettings.GetImageMargin(props);
        }

        /// <summary>
        /// Return the list of decorators to use if there are no saved decorators for the current context.
        /// </summary>
        /// <returns></returns>
        public static string[] GetImplicitLocalImageDecorators()
        {
            string[] defaultDecoratorIds = new string[]{
                                                            HtmlAltTextDecorator.Id,
                                                            BrightnessDecorator.Id,
                                                            CropDecorator.Id,
                                                            TiltDecorator.Id,
                                                            WatermarkDecorator.Id,
                                                            NoRecolorDecorator.Id,
                                                            NoSharpenDecorator.Id,
                                                            NoBlurDecorator.Id,
                                                            NoEmbossDecorator.Id
                                                        };

            return defaultDecoratorIds;
        }

        /// <summary>
        /// Return the list of decorators to use if there are no saved decorators for the current context.
        /// </summary>
        /// <returns></returns>
        private ImageDecoratorsList GetInitialLocalImageDecoratorsList()
        {
            string[] defaultDecoratorIds = new string[]{
                CropDecorator.Id,
                HtmlImageResizeDecorator.Id,
                HtmlImageTargetDecorator.Id,
                HtmlMarginDecorator.Id,
                HtmlAlignDecorator.Id,
                DropShadowBorderDecorator.Id,
                NoRecolorDecorator.Id,
                NoSharpenDecorator.Id,
                NoBlurDecorator.Id,
                NoEmbossDecorator.Id};

            ImageDecoratorsList decoratorsList = new ImageDecoratorsList(_decoratorsManager, new BlogPostSettingsBag());
            foreach (string decoratorId in defaultDecoratorIds)
                decoratorsList.AddDecorator(decoratorId);
            return decoratorsList;
        }

        /// <summary>
        /// Return the list of decorators that will essentially leave the image untouched.
        /// </summary>
        public ImageDecoratorsList LoadBlankLocalImageDecoratorsList()
        {
            string[] defaultDecoratorIds = new string[] {
                BrightnessDecorator.Id,
                CropDecorator.Id,
                HtmlAlignDecorator.Id,
                HtmlAltTextDecorator.Id,
                HtmlImageResizeDecorator.Id,
                HtmlImageTargetDecorator.Id,
                HtmlMarginDecorator.Id,
                NoBorderDecorator.Id,
                NoRecolorDecorator.Id,
                NoSharpenDecorator.Id,
                NoBlurDecorator.Id,
                NoEmbossDecorator.Id,
                TiltDecorator.Id,
                WatermarkDecorator.Id
            };

            ImageDecoratorsList decoratorsList = new ImageDecoratorsList(_decoratorsManager, new BlogPostSettingsBag());
            foreach (string decoratorId in defaultDecoratorIds)
                decoratorsList.AddDecorator(decoratorId);
            return decoratorsList;
        }

        /// <summary>
        /// The root settings key where default image settings are stored.
        /// </summary>
        public static SettingsPersisterHelper RootSettingsKey
        {
            get
            {
                if (_settingsKey == null)
                {
                    _settingsKey = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("Weblogs");
                }
                return _settingsKey;
            }
            set
            {
                _settingsKey = value;
            }
        }

        /// <summary>
        /// Deletes the default image settings for the specified context.
        /// </summary>
        /// <param name="contextId"></param>
        private static void DeleteImageSettings(string contextId)
        {
            //unset the existing image defaults
            using (SettingsPersisterHelper contextRootSettings = RootSettingsKey.GetSubSettings(contextId))
            {
                if (contextRootSettings.HasSubSettings(ImageDefaultsKey))
                    contextRootSettings.UnsetSubsettingTree(ImageDefaultsKey);
            }
        }

        /// <summary>
        /// Get the settings persister for the specified image context.
        /// </summary>
        /// <param name="contextId"></param>
        /// <param name="autoCreate"></param>
        /// <returns></returns>
        private static SettingsPersisterHelper GetImageSettingsPersister(string contextId, bool autoCreate)
        {
            //unset the existing image defaults
            using (SettingsPersisterHelper contextRootSettings = RootSettingsKey.GetSubSettings(contextId))
            {
                if (autoCreate || contextRootSettings.HasSubSettings(ImageDefaultsKey))
                    return contextRootSettings.GetSubSettings(ImageDefaultsKey);
                else
                    return null;
            }
        }
        private static SettingsPersisterHelper _settingsKey = null;

        class NoOpUndoUnitFactory : IUndoUnitFactory
        {
            public IUndoUnit CreateUndoUnit()
            {
                return new NoOpUndoUnit();
            }

            class NoOpUndoUnit : IUndoUnit
            {
                public void Commit()
                {
                }
                public void Dispose()
                {
                }
            }

        }
    }
}
