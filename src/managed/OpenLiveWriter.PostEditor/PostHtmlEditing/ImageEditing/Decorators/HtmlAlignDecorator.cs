// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for Decorator.
    /// </summary>
    public class HtmlAlignDecorator : IImageDecorator, IImageDecoratorDefaultSettingsCustomizer
    {
        public readonly static string Id = "HtmlAlign";
        public HtmlAlignDecorator()
        {
        }

        public void Decorate(ImageDecoratorContext context)
        {
            if (context.InvocationSource == ImageDecoratorInvocationSource.Reset ||
                context.InvocationSource == ImageDecoratorInvocationSource.InitialInsert)
            {
                //set the default HTML for this decorator.
                HtmlAlignDecoratorSettings settings = new HtmlAlignDecoratorSettings(context.Settings, context.ImgElement, context.InvocationSource);
                settings.Alignment = settings.DefaultAlignment;
            }
            //Note: all other times, this decorator is applied directly by the editor.
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return new HtmlAlignEditor(commandManager);
        }

        void IImageDecoratorDefaultSettingsCustomizer.CustomizeDefaultSettingsBeforeSave(ImageDecoratorEditorContext context, IProperties defaultSettings)
        {
            HtmlAlignDecoratorSettings settings = new HtmlAlignDecoratorSettings(defaultSettings, context.ImgElement);
            settings.DefaultAlignment = settings.Alignment;
        }
    }
}
