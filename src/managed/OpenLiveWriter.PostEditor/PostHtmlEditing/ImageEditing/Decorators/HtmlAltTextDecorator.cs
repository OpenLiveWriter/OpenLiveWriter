// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for Decorator.
    /// </summary>
    public class HtmlAltTextDecorator : IImageDecorator
    {
        public readonly static string Id = "HtmlAltText";
        public HtmlAltTextDecorator()
        {
        }

        public void Decorate(ImageDecoratorContext context)
        {
            if (context.ImageEmbedType == ImageEmbedType.Embedded &&
               (context.InvocationSource == ImageDecoratorInvocationSource.InitialInsert ||
                context.InvocationSource == ImageDecoratorInvocationSource.Reset)
                )
            {
                HtmlAltTextDecoratorSettings settings = new HtmlAltTextDecoratorSettings(context.ImgElement);
                Uri uri = context.SourceImageUri;

                //set the default AltText value
                string altText = CalculateAltText(uri);
                settings.AltText = altText;
                settings.Title = altText;
            }
        }

        /// <summary>
        /// Calculates a good default alt text for an image.
        /// Returns the image title or description metadata (if present),
        /// otherwise returns the filename of the image.
        /// </summary>
        /// <param name="imageSourceUri"></param>
        /// <returns></returns>
        private static string CalculateAltText(Uri imageSourceUri)
        {
            string altText = null;
            if (imageSourceUri.LocalPath != null)
            {
                //try to use XMP data to get a good altText
                XmpMetadata xmpMetadata = XmpMetadata.FromFile(imageSourceUri.LocalPath);
                if (xmpMetadata != null)
                {
                    altText = xmpMetadata.Title;
                    if (altText == null)
                        altText = xmpMetadata.Description;
                }
            }

            //use the filename
            if (altText == null)
            {
                string imageName = imageSourceUri.Segments[imageSourceUri.Segments.Length - 1];
                imageName = HttpUtility.UrlDecode(imageName);
                altText = Path.GetFileNameWithoutExtension(imageName);
            }
            return altText;
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return new HtmlAltTextEditor();
        }
    }
    internal class HtmlAltTextDecoratorSettings
    {
        IHTMLElement ImgElement;
        public HtmlAltTextDecoratorSettings(IHTMLElement imgElement)
        {
            ImgElement = imgElement;
        }

        public string AltText
        {
            get
            {
                return ((IHTMLImgElement)ImgElement).alt;
            }
            set
            {
                IHTMLImgElement img = (IHTMLImgElement)ImgElement;
                img.alt = value;
            }
        }

        public string Title
        {
            get { return ImgElement.title; }
            set { ImgElement.title = value; }
        }
    }
}
