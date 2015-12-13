// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using OpenLiveWriter.Api;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageDecoratorsManager.
    /// </summary>
    public class ImageDecoratorsManager : IDisposable
    {
        public static readonly string BORDER_GROUP = "Borders";
        public static readonly string RECOLOR_GROUP = "Recolor";
        public static readonly string SHARPEN_GROUP = "Sharpen";
        public static readonly string BLUR_GROUP = "Gaussian";
        public static readonly string EMBOSS_GROUP = "Emboss";
        public static readonly string BASIC_GROUP = "Basic";
        public static readonly string ADJUST_COLOR_GROUP = "Adjust Color";
        public static readonly string HTML_GROUP = "Html";
        public readonly CommandManager CommandManager;
        private IContainer components;
        Hashtable _decorators = new Hashtable();
        public ImageDecoratorsManager(IContainer components, CommandManager commandManager, bool includeInheritBorder)
        {
            this.components = components;
            CommandManager = commandManager;
            //load the default image decorators
            ImageDecoratorGroup basic = new ImageDecoratorGroup(BASIC_GROUP, false,
                new ImageDecorator[] {
                                         new ImageDecorator(CropDecorator.Id, "Crop", typeof(CropDecorator), BASIC_GROUP, true, true, false, false)
                                     });
            ImageDecoratorGroup borders = new ImageDecoratorGroup(BORDER_GROUP, true,
                new ImageDecorator[] {
                                         new ImageDecorator(HtmlBorderDecorator.Id, Res.Get(StringId.DecoratorInheritBorder), typeof(HtmlBorderDecorator), BORDER_GROUP, true, false, true, true),
                                         new ImageDecorator(NoBorderDecorator.Id, Res.Get(StringId.DecoratorNoBorder), typeof(NoBorderDecorator), BORDER_GROUP, true, false, true, true),
                                         new ImageDecorator(DropShadowBorderDecorator.Id, Res.Get(StringId.DecoratorDropShadow), typeof(DropShadowBorderDecorator), BORDER_GROUP, true, false, true, false),
                                         new ImageDecorator(PolaroidBorderDecorator.Id, Res.Get(StringId.DecoratorInstantPhoto), typeof(PolaroidBorderDecorator), BORDER_GROUP, true, false, true, false),
                                         new ImageDecorator(PhotoBorderDecorator.Id, Res.Get(StringId.DecoratorPhotopaper), typeof(PhotoBorderDecorator), BORDER_GROUP, true, false, true, false),
                                         new ImageDecorator(ReflectionBorderDecorator.Id, Res.Get(StringId.DecoratorReflection), typeof(ReflectionBorderDecorator), BORDER_GROUP, true, false, true, false),
                                         new ImageDecorator(RoundedCornersBorderDecorator.Id, Res.Get(StringId.DecoratorRoundedCorners), typeof(RoundedCornersBorderDecorator), BORDER_GROUP, true, false, true, false),
                                         //new ImageDecorator(SoftShadowBorderDecorator.Id, "Soft Shadow", typeof(SoftShadowBorderDecorator), true, false, true, false),
                                         new ImageDecorator(ThinSolidBorderDecorator.Id, Res.Get(StringId.DecoratorSolid1px), typeof(ThinSolidBorderDecorator), BORDER_GROUP, true, false, true, false),
                                         new ImageDecorator(ThickSolidBorderDecorator.Id, Res.Get(StringId.DecoratorSolid3px), typeof(ThickSolidBorderDecorator), BORDER_GROUP, true, false, true, false),
            });
            ImageDecoratorGroup colors = new ImageDecoratorGroup(ADJUST_COLOR_GROUP, false,
                new ImageDecorator[] {
                                         new ImageDecorator(BrightnessDecorator.Id, "Brightness/Contrast", typeof(BrightnessDecorator), ADJUST_COLOR_GROUP, true, true, false, false),
                                         new ImageDecorator(TiltDecorator.Id, "Tilt", typeof(TiltDecorator), ADJUST_COLOR_GROUP, true, true, false, false),
                                         new ImageDecorator(WatermarkDecorator.Id, "Watermark", typeof(WatermarkDecorator), ADJUST_COLOR_GROUP, true, true, true, false)
                                     });
            ImageDecoratorGroup html = new ImageDecoratorGroup(HTML_GROUP, false,
                new ImageDecorator[] {
                                         new ImageDecorator(HtmlMarginDecorator.Id, "Margin", typeof(HtmlMarginDecorator), HTML_GROUP, true, true, true, true),
                                         new ImageDecorator(HtmlAlignDecorator.Id, "Text Wrapping", typeof(HtmlAlignDecorator), HTML_GROUP, true, true, true, true),
                                         new ImageDecorator(HtmlAltTextDecorator.Id, "Alt Text", typeof(HtmlAltTextDecorator), HTML_GROUP, true, true, false, true),
                                         new ImageDecorator(HtmlImageResizeDecorator.Id, "Embedded Size", typeof(HtmlImageResizeDecorator), HTML_GROUP, true, true, true, true),
                                         new ImageDecorator(HtmlImageTargetDecorator.Id, "Image Target", typeof(HtmlImageTargetDecorator), HTML_GROUP, true, true, true, true),
            });

            ImageDecoratorGroup recolor = new ImageDecoratorGroup(RECOLOR_GROUP, true,
                new ImageDecorator[] {
                                        new ImageDecorator(NoRecolorDecorator.Id, Res.Get(StringId.DecoratorNoRecolorLabel), typeof(NoRecolorDecorator), RECOLOR_GROUP, false, true, false, false),
                                        new ImageDecorator(BlackandWhiteDecorator.Id, Res.Get(StringId.DecoratorBWLabel), typeof(BlackandWhiteDecorator), RECOLOR_GROUP, false, true, false, false),
                                        new ImageDecorator(SepiaToneDecorator.Id, Res.Get(StringId.DecoratorSepiaLabel), typeof(SepiaToneDecorator), RECOLOR_GROUP, false, true, false, false),
                                        new ImageDecorator(SaturationDecorator.Id, Res.Get(StringId.DecoratorSaturationLabel), typeof(SaturationDecorator), RECOLOR_GROUP, false, true, false, false),
                                        new ImageDecorator(CoolestDecorator.Id, Res.Get(StringId.DecoratorCoolestTemperatureLabel), typeof(CoolestDecorator), RECOLOR_GROUP, false, true, false, false),
                                        new ImageDecorator(CoolDecorator.Id, Res.Get(StringId.DecoratorCoolTemperatureLabel), typeof(CoolDecorator), RECOLOR_GROUP, false, true, false, false),
                                        new ImageDecorator(WarmDecorator.Id, Res.Get(StringId.DecoratorWarmTemperatureLabel), typeof(WarmDecorator), RECOLOR_GROUP, false, true, false, false),
                                        new ImageDecorator(WarmestDecorator.Id, Res.Get(StringId.DecoratorWarmestTemperatureLabel), typeof(WarmestDecorator), RECOLOR_GROUP, false, true, false, false)
                                     });

            ImageDecoratorGroup sharpen = new ImageDecoratorGroup(SHARPEN_GROUP, true,
                new ImageDecorator[] {
                                        new ImageDecorator(NoSharpenDecorator.Id, Res.Get(StringId.DecoratorNoSharpenLabel), typeof(NoSharpenDecorator), SHARPEN_GROUP, false, true, false, false),
                                        new ImageDecorator(SharpenDecorator.Id, Res.Get(StringId.DecoratorSharpenLabel), typeof(SharpenDecorator), SHARPEN_GROUP, false, true, false, false)
                                     });

            ImageDecoratorGroup blur = new ImageDecoratorGroup(BLUR_GROUP, true,
                new ImageDecorator[] {
                                        new ImageDecorator(NoBlurDecorator.Id, Res.Get(StringId.DecoratorNoBlurLabel), typeof(NoBlurDecorator), BLUR_GROUP, false, true, false, false),
                                        new ImageDecorator(BlurDecorator.Id, Res.Get(StringId.DecoratorGaussianBlurLabel), typeof(BlurDecorator), BLUR_GROUP, false, true, false, false)
                                     });

            ImageDecoratorGroup emboss = new ImageDecoratorGroup(EMBOSS_GROUP, true,
                new ImageDecorator[] {
                                        new ImageDecorator(NoEmbossDecorator.Id, Res.Get(StringId.DecoratorNoEmbossLabel), typeof(NoEmbossDecorator), EMBOSS_GROUP, false, true, false, false),
                                        new ImageDecorator(EmbossDecorator.Id, Res.Get(StringId.DecoratorEmbossLabel), typeof(EmbossDecorator), EMBOSS_GROUP, false, true, false, false)
                                     });

            ImageDecoratorGroup[] groups = new ImageDecoratorGroup[] { basic, colors, html, borders, recolor, sharpen, blur, emboss };
            foreach (ImageDecoratorGroup group in groups)
            {
                _imageDecoratorGroups[group.GroupName] = group;
                foreach (ImageDecorator decorator in group.ImageDecorators)
                {
                    _decorators[decorator.Id] = decorator;
                }
            }

            _groups = groups;

            //load the custom image decorator plugins
            RegisterImageDecoratorPlugins();

            //load the image decorator commands
            InitializeCommands();

            if (!includeInheritBorder)
                borders.ImageDecorators[0].Command.On = false;
        }

        public ImageDecorator GetImageDecorator(string id)
        {
            return (ImageDecorator)_decorators[id];
        }

        public ImageDecoratorGroup GetImageDecoratorsGroup(string groupName)
        {
            return (ImageDecoratorGroup)_imageDecoratorGroups[groupName];
        }

        public ImageDecorator[] GetImageDecoratorsFromGroup(string groupName)
        {
            ImageDecoratorGroup group = GetImageDecoratorsGroup(groupName);
            if (group == null)
                return new ImageDecorator[0];
            else
                return group.ImageDecorators;
        }

        public ImageDecoratorGroup[] ImageDecoratorGroups
        {
            get { return (ImageDecoratorGroup[])_groups.Clone(); }
        }
        private Hashtable _imageDecoratorGroups = new Hashtable();
        private ImageDecoratorGroup[] _groups;

        public ImageDecorator[] GetDefaultRemoteImageDecorators()
        {
            ImageDecorator imageResizeDecorator = this.GetImageDecorator(HtmlImageResizeDecorator.Id);
            ImageDecorator imageTargetDecorator = this.GetImageDecorator(HtmlImageTargetDecorator.Id);
            ImageDecorator htmlMarginDecorator = this.GetImageDecorator(HtmlMarginDecorator.Id);
            ImageDecorator htmlAlignDecorator = this.GetImageDecorator(HtmlAlignDecorator.Id);
            ImageDecorator htmlAltTextDecorator = this.GetImageDecorator(HtmlAltTextDecorator.Id);
            ImageDecorator recolorDecorator = this.GetImageDecorator(NoRecolorDecorator.Id);
            ImageDecorator sharpenDecorator = this.GetImageDecorator(NoSharpenDecorator.Id);
            ImageDecorator blurDecorator = this.GetImageDecorator(NoBlurDecorator.Id);
            ImageDecorator embossDecorator = this.GetImageDecorator(NoEmbossDecorator.Id);
            return new ImageDecorator[] { imageResizeDecorator, imageTargetDecorator, htmlMarginDecorator, htmlAlignDecorator, htmlAltTextDecorator, recolorDecorator, sharpenDecorator, blurDecorator, embossDecorator };

        }

        private const string DefaultGroupName = "Plugins";
        private void RegisterImageDecoratorPlugins()
        {
#if SUPPORT_PLUGINS
            Type[] pluginImplTypes = PostEditorPluginManager.Instance.GetPlugins(typeof(IImageDecorator));
            foreach(Type pluginImplType in pluginImplTypes)
            {
                object[] attrs = pluginImplType.GetCustomAttributes(typeof(ImageDecoratorAttribute), true);
                if(attrs.Length > 0)
                {
                    ImageDecoratorAttribute decAttr = (ImageDecoratorAttribute)attrs[0];
                    AddDecorator(
                        decAttr.Group != null ? decAttr.Group : DefaultGroupName,
                        new ImageDecorator(
                        decAttr.Id != null ? decAttr.Id : pluginImplType.FullName,
                        decAttr.Name,
                        pluginImplType, false));
                }
                else
                    AddDecorator(DefaultGroupName, new ImageDecorator(pluginImplType.FullName, pluginImplType.Name, pluginImplType, false));
            }
#endif
        }

        private void InitializeCommands()
        {
            commandContextManager = new CommandContextManager(CommandManager);
            commandContextManager.BeginUpdate();

            for (int i = 0; i < ImageDecoratorGroups.Length; i++)
            {
                ImageDecoratorGroup imageDecoratorGroup = ImageDecoratorGroups[i];
                foreach (ImageDecorator imageDecorator in imageDecoratorGroup.ImageDecorators)
                {
                    Command ImageDecoratorApplyCommand = new Command(CommandId.ImageDecoratorApply);
                    ImageDecoratorApplyCommand.Identifier += imageDecorator.Id;
                    ImageDecoratorApplyCommand.Text = imageDecorator.DecoratorName;
                    ImageDecoratorApplyCommand.MenuText = imageDecorator.DecoratorName;
                    imageDecorator.SetCommand(ImageDecoratorApplyCommand);
                    commandContextManager.AddCommand(ImageDecoratorApplyCommand, CommandContext.Normal);

                    ImageDecoratorApplyCommand.Execute += new EventHandler(imageDecoratorCommand_Execute);
                    ImageDecoratorApplyCommand.Tag = imageDecorator.Id;
                }
            }

            commandContextManager.EndUpdate();
        }
        CommandContextManager commandContextManager;

        internal void Activate()
        {
            if (!activated)
            {
                CommandManager.SuppressEvents = true;
                try
                {
                    commandContextManager.Activate();
                    commandContextManager.Enter();
                    activated = true;
                }
                finally
                {
                    CommandManager.SuppressEvents = false;
                }
            }
        }

        internal void Deactivate()
        {
            if (activated)
            {
                CommandManager.SuppressEvents = true;
                try
                {
                    commandContextManager.Leave();
                    commandContextManager.Deactivate();
                    activated = false;
                }
                finally
                {
                    CommandManager.SuppressEvents = false;
                }
            }
        }
        private bool activated;

        public delegate void ImageDecoratorAppliedHandler(string decoratorId);
        public event ImageDecoratorAppliedHandler ImageDecoratorApplied;
        private void imageDecoratorCommand_Execute(object sender, EventArgs e)
        {
            Command command = (Command)sender;
            string decoratorId = (string)command.Tag;
            if (ImageDecoratorApplied != null)
            {
                ImageDecoratorApplied(decoratorId);
            }
        }
        #region IDisposable Members

        public void Dispose()
        {
            if (commandContextManager != null)
            {
                commandContextManager.Close();
                commandContextManager = null;
            }
        }

        #endregion
    }

    public class ImageDecorator : IImageDecorator, IImageDecoratorOriginalSizeAdjuster, IComparable
    {
        string _decoratorId;
        string _decoratorName;
        Type _decoratorType;
        string _groupName;
        bool _decoratorHidden;
        bool _decoratorEditable;
        bool _decoratorDefaultable;
        bool _webImageSupported;
        Command _command;

        /// <summary>
        ///
        /// </summary>
        /// <param name="decoratorId">The unique decorator id.</param>
        /// <param name="decoratorName">The user-visible name of the decorator.</param>
        /// <param name="decoratorType">The type of the class that implements the decorator.</param>
        /// <param name="hideDecorator">If true, this decorator is marked as hidden so it will not show up in the applied decorators list.</param>
        internal ImageDecorator(string decoratorId, string decoratorName, Type decoratorType, string groupName, bool hideDecorator)
            : this(decoratorId, decoratorName, decoratorType, groupName, hideDecorator, true, true, false)
        {
        }
        internal ImageDecorator(string decoratorId, string decoratorName, Type decoratorType, string groupName, bool hideDecorator, bool hasEditor, bool defaultable, bool webImageSupported)
        {
            _decoratorId = decoratorId;
            _decoratorName = decoratorName;
            _decoratorType = decoratorType;
            _groupName = groupName;
            _decoratorHidden = hideDecorator;
            _decoratorEditable = hasEditor;
            _decoratorDefaultable = defaultable;
            _webImageSupported = webImageSupported;
        }
        public void Decorate(ImageDecoratorContext context)
        {
            RawImageDecorator.Decorate(context);
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return RawImageDecorator.CreateEditor(commandManager);
        }

        public void AdjustOriginalSize(IProperties properties, ref Size size)
        {
            if (RawImageDecorator is IImageDecoratorOriginalSizeAdjuster)
                ((IImageDecoratorOriginalSizeAdjuster)RawImageDecorator).AdjustOriginalSize(properties, ref size);
        }

        public string DecoratorName
        {
            get
            {
                return _decoratorName;
            }
        }

        public string Id
        {
            get
            {
                return _decoratorId;
            }
        }

        public string GroupName
        {
            get
            {
                return _groupName;
            }
        }

        public Bitmap IconLarge
        {
            get
            {
                return RawImageDecorator is IImageDecoratorIcons ? ((IImageDecoratorIcons)RawImageDecorator).BitmapLarge : null;
            }
        }

        public string SettingsNamespace
        {
            get
            {
                return _decoratorId;
            }
        }

        public bool IsBorderDecorator
        {
            get
            {
                return RawImageDecorator is ImageBorderDecorator;
            }
        }

        public bool IsHidden
        {
            get
            {
                return _decoratorHidden;
            }
        }

        public bool IsEditable
        {
            get
            {
                return _decoratorEditable;
            }
        }

        public bool IsDefaultable
        {
            get
            {
                return _decoratorDefaultable;
            }
        }

        public bool IsWebImageSupported
        {
            get
            {
                return _webImageSupported;
            }
        }

        internal void ApplyCustomizeDefaultSettingsHook(ImageDecoratorEditorContext context, IProperties settings)
        {
            if (RawImageDecorator is IImageDecoratorDefaultSettingsCustomizer)
            {
                ((IImageDecoratorDefaultSettingsCustomizer)RawImageDecorator).CustomizeDefaultSettingsBeforeSave(context, settings);
            }
        }

        private IImageDecorator RawImageDecorator
        {
            get
            {
                if (_rawImageDecorator == null)
                {
                    _rawImageDecorator = (IImageDecorator)Activator.CreateInstance(_decoratorType);
                }
                return _rawImageDecorator;
            }
        }
        private IImageDecorator _rawImageDecorator;

        public Command Command
        {
            get
            {
                return _command;
            }
        }
        internal void SetCommand(Command command)
        {
            _command = command;
        }

        public override bool Equals(object obj)
        {
            ImageDecorator decorator = obj as ImageDecorator;
            return decorator != null && decorator.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is ImageDecorator)
            {
                ImageDecorator otherImageDecorator = (ImageDecorator)obj;
                return Id.CompareTo(otherImageDecorator.Id);
            }
            else if (obj == null)
            {
                // By definition, any object compares greater than a null reference.
                return 1;
            }

            throw new ArgumentException("object is not an ImageDecorator");
        }

        #endregion
    }

    public class ImageDecoratorGroup
    {
        public ImageDecoratorGroup(string groupName, bool mutuallyExclusive, ImageDecorator[] ImageDecorators)
        {
            _groupName = groupName;
            _mutuallyExclusive = mutuallyExclusive;
            _decorators = new List<ImageDecorator>(ImageDecorators);
        }
        public string GroupName
        {
            get { return _groupName; }
        }
        private string _groupName;

        public bool MutuallyExclusive
        {
            get { return _mutuallyExclusive; }
        }
        private bool _mutuallyExclusive;

        public void AddDecorator(params ImageDecorator[] decorators)
        {
            _decorators.AddRange(decorators);
        }

        public void RemoveDecoratorAt(int index)
        {
            _decorators.RemoveAt(index);
        }

        public ImageDecorator[] ImageDecorators
        {
            get { return _decorators.ToArray(); }
        }
        private List<ImageDecorator> _decorators;
    }

    internal class ImageDecoratorSettingsBagAdapter : IProperties
    {
        BlogPostSettingsBag _settingsBag;
        public ImageDecoratorSettingsBagAdapter(BlogPostSettingsBag settingsBag)
        {
            _settingsBag = settingsBag;
        }

        public string this[string key]
        {
            get { return _settingsBag[key]; }
            set { _settingsBag[key] = value; }
        }

        public string GetString(string key, string defaultValue)
        {
            return _settingsBag.GetString(key, defaultValue);
        }

        public void SetString(string key, string value)
        {
            _settingsBag.SetString(key, value);
        }

        public int GetInt(string key, int defaultValue)
        {
            return _settingsBag.GetInt(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            _settingsBag.SetInt(key, value);
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            return _settingsBag.GetBoolean(key, defaultValue);
        }

        public void SetBoolean(string key, bool value)
        {
            _settingsBag.SetBoolean(key, value);
        }

        public float GetFloat(string key, float defaultValue)
        {
            return _settingsBag.GetFloat(key, defaultValue);
        }

        public void SetFloat(string key, float value)
        {
            _settingsBag.SetFloat(key, value);
        }

        public decimal GetDecimal(string key, decimal defaultValue)
        {
            return _settingsBag.GetDecimal(key, defaultValue);
        }
        public void SetDecimal(string key, decimal value)
        {
            _settingsBag.SetDecimal(key, value);
        }

        public void Remove(string key)
        {
            _settingsBag.Remove(key);
        }

        public void RemoveAll()
        {
            foreach (string propertyName in Names)
                Remove(propertyName);

            foreach (string subPropertyName in SubPropertyNames)
                RemoveSubProperties(subPropertyName);
        }

        public string[] Names
        {
            get { return _settingsBag.Names; }
        }

        public bool Contains(string key)
        {
            return _settingsBag.Contains(key);
        }

        public string[] SubPropertyNames
        {
            get
            {
                return ((IProperties)_settingsBag).SubPropertyNames;
            }
        }

        public IProperties GetSubProperties(string key)
        {
            return ((IProperties)_settingsBag).GetSubProperties(key);
        }

        public void RemoveSubProperties(string key)
        {
            ((IProperties)_settingsBag).RemoveSubProperties(key);
        }

        public bool ContainsSubProperties(string key)
        {
            return ((IProperties)_settingsBag).ContainsSubProperties(key);
        }
    }
}
