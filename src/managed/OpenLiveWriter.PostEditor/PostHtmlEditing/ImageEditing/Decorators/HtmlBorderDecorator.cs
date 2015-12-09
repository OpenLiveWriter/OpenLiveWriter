// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for Decorator.
    /// </summary>
    public class HtmlBorderDecorator : ImageBorderDecorator
    {
        public readonly static string Id = "HtmlBorderDecorator";
        public HtmlBorderDecorator()
        {
        }

        public override void Decorate(ImageDecoratorContext context)
        {
            HtmlBorderDecoratorSettings settings = new HtmlBorderDecoratorSettings(context.ImgElement);
            settings.InheritBorder = true;

            if (context.ImageEmbedType == ImageEmbedType.Embedded)
            {
                context.BorderMargin = ImageBorderMargin.Empty;
            }
        }

        public override ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }

        public override Bitmap BitmapLarge
        {
            get { return Images.Photo_Border_Inherit; }
        }
    }

    internal class HtmlBorderDecoratorSettings
    {
        IHTMLElement ImgElement;
        public HtmlBorderDecoratorSettings(IHTMLElement imgElement)
        {
            ImgElement = imgElement;
        }

        public bool InheritBorder
        {
            get
            {
                return (ImgElement.getAttribute("border", 0) ?? "").ToString() == "";
            }
            set
            {
                if (value)
                {
                    ImgElement.removeAttribute("border", 2);
                    ImgElement.style.border = null;
                    ImgElement.style.borderWidth = null;
                    ImgElement.style.borderStyle = null;
                    ImgElement.style.padding = null;
                    ImgElement.style.backgroundImage = null;
                }
            }
        }
    }
}
