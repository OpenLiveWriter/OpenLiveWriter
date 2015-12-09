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
    public class NoBorderDecorator : ImageBorderDecorator
    {
        public readonly static string Id = "NoneBorderDecorator";
        public NoBorderDecorator()
        {
        }

        public override void Decorate(ImageDecoratorContext context)
        {
            NoBorderDecoratorSettings settings = new NoBorderDecoratorSettings(context.ImgElement);
            settings.NoBorder = true;

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
            get { return Images.Photo_Border_Default; }
        }
    }

    internal class NoBorderDecoratorSettings
    {
        IHTMLElement ImgElement;
        public NoBorderDecoratorSettings(IHTMLElement imgElement)
        {
            ImgElement = imgElement;
        }

        public bool NoBorder
        {
            get
            {
                return (ImgElement.getAttribute("border", 0) ?? "").ToString() == "0";
            }
            set
            {
                if (value)
                {
                    ImgElement.setAttribute("border", "0", 0);
                    ImgElement.style.border = "0";
                    ImgElement.style.borderWidth = "0";
                    ImgElement.style.borderStyle = "none";
                    ImgElement.style.padding = "0px;";
                    ImgElement.style.backgroundImage = "none";
                }
            }
        }
    }
}
