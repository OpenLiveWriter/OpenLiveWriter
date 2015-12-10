// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for Decorator.
    /// </summary>
    public class DropShadowBorderDecorator : ImageBorderDecorator
    {
        public readonly static string Id = "DropShadowBorder";
        public DropShadowBorderDecorator()
        {
        }

        public override void Decorate(ImageDecoratorContext context)
        {
            const int BORDER_WIDTH = 0;
            const int SHADOW_WIDTH = 4;
            DropShadowBorderDecoratorSettings settings = new DropShadowBorderDecoratorSettings(context);
            if (context.InvocationSource == ImageDecoratorInvocationSource.InitialInsert ||
                context.InvocationSource == ImageDecoratorInvocationSource.Reset)
            {
            }
            context.Image = ImageUtils.ApplyDropShadowOutside(context.Image, settings.BackgroundColor, settings.ShadowColor, SHADOW_WIDTH, BORDER_WIDTH);

            //update the margin value to reflect the border added by this decorator.
            int borderWidth = BORDER_WIDTH * 2 + SHADOW_WIDTH;
            int borderHeight = borderWidth;
            context.BorderMargin =
                new ImageBorderMargin(borderWidth, borderHeight, new BorderCalculation(borderWidth, borderHeight));

            HideHtmlBorder(context);
        }

        public override ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }

        public override Bitmap BitmapLarge
        {
            get { return Images.Photo_Border_Drop_shadow; }
        }
    }

    internal class DropShadowBorderDecoratorSettings
    {
        private readonly IHTMLElement Element;
        private readonly IEditorOptions editorOptions;
        public DropShadowBorderDecoratorSettings(ImageDecoratorContext context)
        {
            Element = context.ImgElement;
            editorOptions = context.EditorOptions;
        }

        public Color BackgroundColor
        {
            get
            {
                Color bgColor = Color.FromArgb(editorOptions.PostBodyBackgroundColor);
                if (!bgColor.Equals(Color.Transparent) && bgColor.A == 255)
                    return bgColor;

                int htmlColor = HTMLColorHelper.GetBackgroundColor(Element.parentElement, Color.White).ToArgb();
                return Color.FromArgb(htmlColor);
            }
        }

        public Color ShadowColor
        {
            get
            {
                if (HTMLColorHelper.IsDarkColor(BackgroundColor))
                    return Color.DarkGray;
                else
                    return Color.Black;
            }
        }
    }
}
