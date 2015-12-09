// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for Decorator.
    /// </summary>
    public class HtmlMarginDecorator : IImageDecorator, IImageDecoratorDefaultSettingsCustomizer
    {
        public readonly static string Id = "HtmlMargin";
        public HtmlMarginDecorator()
        {
        }

        public void Decorate(ImageDecoratorContext context)
        {
            if (context.InvocationSource == ImageDecoratorInvocationSource.InitialInsert ||
                context.InvocationSource == ImageDecoratorInvocationSource.Reset)
            {
                HtmlMarginDecoratorSettings settings = new HtmlMarginDecoratorSettings(context.Settings, context.ImgElement);
                if (settings.UseUserCustomMargin)
                {
                    settings.Margin = settings.UserDefaultMargin;
                }
                else
                {
                    settings.Margin = HtmlMarginDecoratorSettings.WriterDefaultMargin;
                }
            }
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return new HtmlMarginEditor(commandManager);
        }

        void IImageDecoratorDefaultSettingsCustomizer.CustomizeDefaultSettingsBeforeSave(ImageDecoratorEditorContext context, IProperties defaultSettings)
        {
            HtmlMarginDecoratorSettings settings = new HtmlMarginDecoratorSettings(defaultSettings, context.ImgElement);
            settings.UseUserCustomMargin = settings.HasCustomMargin;
            settings.UserDefaultMargin = settings.Margin;
        }
    }

    internal class HtmlMarginDecoratorSettings
    {
        private const string CUSTOM_MARGIN = "UseUserCustomMargin";
        private const string MARGIN_TOP = "DefaultMarginTop";
        private const string MARGIN_RIGHT = "DefaultMarginRight";
        private const string MARGIN_BOTTOM = "DefaultMarginBottom";
        private const string MARGIN_LEFT = "DefaultMarginLeft";

        private readonly IProperties Settings;
        private readonly IHTMLElement ImgElement;
        public HtmlMarginDecoratorSettings(IProperties settings, IHTMLElement imgElement)
        {
            Settings = settings;
            ImgElement = imgElement;
        }

        public bool UseUserCustomMargin
        {
            get { return Settings.GetBoolean(CUSTOM_MARGIN, false); }
            set { Settings.SetBoolean(CUSTOM_MARGIN, value); }
        }

        /// <summary>
        /// Get/Set the user-supplied default margin of the image.
        /// </summary>
        public MarginStyle UserDefaultMargin
        {
            get
            {
                int top = Settings.GetInt(MARGIN_TOP, WriterDefaultMargin.Top);
                int right = Settings.GetInt(MARGIN_RIGHT, WriterDefaultMargin.Right);
                int bottom = Settings.GetInt(MARGIN_BOTTOM, WriterDefaultMargin.Bottom);
                int left = Settings.GetInt(MARGIN_LEFT, WriterDefaultMargin.Left);
                return new MarginStyle(top, right, bottom, left, StyleSizeUnit.PX);
            }
            set
            {
                Settings.SetInt(MARGIN_TOP, value.Top);
                Settings.SetInt(MARGIN_RIGHT, value.Right);
                Settings.SetInt(MARGIN_BOTTOM, value.Bottom);
                Settings.SetInt(MARGIN_LEFT, value.Left);
            }
        }

        /// <summary>
        /// Get/Set the Writer default margin of the image.
        /// </summary>
        public static MarginStyle WriterDefaultMargin
        {
            get
            {
                return new MarginStyle(0, 0, 0, 0, StyleSizeUnit.PX);
            }
        }

        public bool HasCustomMargin
        {
            get
            {
                string margin = ImgElement.style.margin;
                if (margin != null && margin != "auto")
                    return true;
                else
                    return Margin != WriterDefaultMargin;
            }
        }

        public static MarginStyle GetImageMargin(IProperties props)
        {
            if (props.GetBoolean(CUSTOM_MARGIN, false))
            {
                int top = props.GetInt(MARGIN_TOP, WriterDefaultMargin.Top);
                int right = props.GetInt(MARGIN_RIGHT, WriterDefaultMargin.Right);
                int bottom = props.GetInt(MARGIN_BOTTOM, WriterDefaultMargin.Bottom);
                int left = props.GetInt(MARGIN_LEFT, WriterDefaultMargin.Left);
                return new MarginStyle(top, right, bottom, left, StyleSizeUnit.PX);
            }
            else
            {
                return WriterDefaultMargin;
            }
        }

        /// <summary>
        /// Get/Set the margin of the image.
        /// </summary>
        public MarginStyle Margin
        {
            get
            {
                return GetMarginStyleFromHtml(ImgElement);
            }
            set
            {
                if (value != null)
                    ImgElement.style.margin = GetHtmlMarginFromMarginStyle(value, ImgElement);
                else
                    //unset any explicit margin style info so that the default is inherited.
                    ImgElement.style.margin = null;
            }
        }

        private static string GetHtmlMarginFromMarginStyle(MarginStyle margin, IHTMLElement element)
        {
            string unitSize = margin.SizeUnit.ToString().ToLower(CultureInfo.InvariantCulture);
            string marginRight = margin.Right.ToString(CultureInfo.InvariantCulture) + unitSize;
            string marginLeft = margin.Left.ToString(CultureInfo.InvariantCulture) + unitSize;

            string currentRightMargin = "";
            if (element.style.marginRight != null)
                currentRightMargin = element.style.marginRight.ToString();

            string currentLeftMargin = "";
            if (element.style.marginLeft != null)
                currentLeftMargin = element.style.marginLeft.ToString();

            // This is because margins and centering images can conflict with eachother
            if (margin.Right == 0 && currentRightMargin == "auto" &&
                margin.Left == 0 && currentLeftMargin == "auto")
            {
                marginRight = "auto";
                marginLeft = "auto";
            }
            else if ((margin.Right != 0 || margin.Left != 0) && currentRightMargin == "auto" && currentLeftMargin == "auto")
            {
                // The user is breaking their centered image by setting a L/R margin
                element.style.display = "inline";
            }

            string marginString = String.Format(CultureInfo.InvariantCulture, "{0}{4} {1} {2}{4} {3}",
                margin.Top, marginRight, margin.Bottom, marginLeft, unitSize);
            return marginString;
        }

        public MarginStyle GetMarginStyleFromHtml(IHTMLElement img)
        {
            IHTMLElement2 img2 = (IHTMLElement2)img;
            int marginTop = GetPixels(img2.currentStyle.marginTop as string);
            int marginRight = GetPixels(img2.currentStyle.marginRight as string);
            int marginBottom = GetPixels(img2.currentStyle.marginBottom as string);
            int marginLeft = GetPixels(img2.currentStyle.marginLeft as string);
            MarginStyle margin = new MarginStyle(marginTop, marginRight, marginBottom, marginLeft, StyleSizeUnit.PX);
            return margin;
        }

        public static int GetPixels(object pixelString)
        {
            if (pixelString == null)
                return 0;

            string htmlString = pixelString.ToString();

            Match match = marginRegex.Match(htmlString);
            string pixelCount = match.Groups["pixels"].Value;
            if (pixelCount == null || pixelCount == String.Empty)
                return 0;
            else
            {
                try
                {
                    return Int32.Parse(pixelCount, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        private static Regex marginRegex = new Regex(@"(?<pixels>\d{1,4})\s*px");
    }
    public enum StyleSizeUnit { PX, EM };
    public class MarginStyle
    {
        public MarginStyle(int top, int right, int bottom, int left, StyleSizeUnit unit)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
            SizeUnit = unit;
        }
        public int Top;
        public int Bottom;
        public int Left;
        public int Right;
        public StyleSizeUnit SizeUnit;

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}{4} {1}{4} {2}{4} {3}{4}", Top, Right, Bottom, Left, SizeUnit.ToString().ToLower(CultureInfo.InvariantCulture));
        }
    }
}
