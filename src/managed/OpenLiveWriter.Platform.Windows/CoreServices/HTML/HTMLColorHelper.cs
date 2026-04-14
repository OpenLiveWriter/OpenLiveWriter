// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using mshtml;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.HtmlParser.Parser;
using ColorInt = System.Collections.Generic.KeyValuePair<System.Drawing.Color, int>;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for HtmlColorHelper.
    /// </summary>
    public class HTMLColorHelper
    {
        private HTMLColorHelper()
        {
        }

        /// <summary>
        /// Parses an HTML hex color string into a .NET drawing Color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="defaultColor"></param>
        /// <returns></returns>
        public static Color GetColorFromHexColor(string color, Color defaultColor)
        {
            try
            {
                if (color != null && color.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    color = ParseColorToHex(color); //fix case where truncated HEX value is returned.
                    return ColorHelper.StringToColor(color.ToString());
                }
            }
            catch (Exception) { }
            return defaultColor;
        }

        /// <summary>
        /// Returns the .NET color that matches an element's text color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="defaultColor"></param>
        /// <returns></returns>
        public static Color GetTextColor(IHTMLElement element, Color defaultColor)
        {
            return GetElementColor(new GetColorDelegate(GetTextColorString), element, defaultColor);
        }

        public static Color GetBackgroundColor(IHTMLElement element, Color defaultColor)
        {
            return GetBackgroundColor(element, false, null, defaultColor);
        }

        /// <summary>
        /// Returns the .NET color that matches an element's background color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="detectImage">
        /// Takes background-image into account when looking for background color.
        /// This is positionally sensitive so if you're not sure if your elements
        /// are in the "correct" positions relative to each other (such as in
        /// Web Layout view) you may want to set this to false.
        /// </param>
        /// <param name="pageUrl">The URL that should be used to escape relative background image paths. Can be null.</param>
        /// <param name="defaultColor"></param>
        /// <returns></returns>
        public static Color GetBackgroundColor(IHTMLElement element, bool detectImage, string pageUrl, Color defaultColor)
        {
            Rectangle childBounds = new Rectangle(HTMLElementHelper.CalculateOffset(element), new Size(element.offsetWidth, element.offsetHeight));

            while (element != null)
            {
                IHTMLCurrentStyle style = ((IHTMLElement2)element).currentStyle;
                string colorStr = style.backgroundColor as string;
                if (!string.IsNullOrEmpty(colorStr) && colorStr != "transparent")
                {
                    if (colorStr == "inherit")
                        detectImage = false;
                    else
                        return GetColorFromHexColor(ParseColorToHex(colorStr), defaultColor);
                }

                string imageUrl = style.backgroundImage;
                if (detectImage
                    && !string.IsNullOrEmpty(imageUrl)
                    && imageUrl != "none"
                    && (style.backgroundRepeat == "repeat" || style.backgroundRepeat == "repeat-y"))
                {
                    StyleUrl styleUrl = new CssParser(imageUrl.Trim()).Next() as StyleUrl;
                    Trace.Assert(styleUrl != null, "Style URL could not be parsed");
                    if (styleUrl != null)
                    {
                        // If there's a background image URL...
                        string url = styleUrl.LiteralText;
                        using (Stream imageStream = GetStreamForUrl(url, pageUrl, element))
                        {
                            if (imageStream != null)
                            {
                                // ...and we were able to open/download it...
                                using (Image image = Image.FromStream(imageStream))
                                {
                                    if (image is Bitmap)
                                    {
                                        // ...and it's a Bitmap, then use it to get the color.
                                        Rectangle containerBounds = new Rectangle(HTMLElementHelper.CalculateOffset(element), new Size(element.offsetWidth, element.offsetHeight));
                                        Color color = GetColorFromBackgroundImage((Bitmap)image,
                                                                        childBounds,
                                                                        containerBounds,
                                                                        style);
                                        // We can't use semi-transparent backgrounds, keep looking
                                        if (color.A == 255)
                                            return color;
                                    }
                                }
                            }
                        }
                    }
                }

                element = element.parentElement;
            }

            return defaultColor;
        }

        private static Stream GetStreamForUrl(string url, string pageUrl, IHTMLElement element)
        {
            if (UrlHelper.IsFileUrl(url))
            {
                string path = new Uri(url).LocalPath;
                if (File.Exists(path))
                {
                    return File.OpenRead(path);
                }
                else
                {
                    // Missing background image files are common (e.g., old blog templates with dead image links)
                    // Don't fail, just log and return null - caller will use default color
                    Trace.WriteLine("File " + url + " not found");
                    return null;
                }
            }
            else if (UrlHelper.IsUrlDownloadable(url))
            {
                return HttpRequestHelper.SafeDownloadFile(url);
            }
            else
            {
                string baseUrl = HTMLDocumentHelper.GetBaseUrlFromDocument((IHTMLDocument2)element.document) ?? pageUrl ?? ((IHTMLDocument2)element.document).url;
                if (baseUrl == null)
                    return null;

                url = UrlHelper.EscapeRelativeURL(baseUrl, url);
                if (UrlHelper.IsUrlDownloadable(url))
                {
                    return HttpRequestHelper.SafeDownloadFile(url);
                }
            }
            return null;
        }

        private static Color GetColorFromBackgroundImage(Bitmap backgroundImage, Rectangle childBounds, Rectangle containerBounds, IHTMLCurrentStyle containerStyle)
        {
            childBounds.Width = Math.Max(3, childBounds.Width);
            childBounds.Height = Math.Max(3, childBounds.Height);

            Point center =
                new Point(childBounds.X + Convert.ToInt32(childBounds.Width / 2f) - containerBounds.X,
                          childBounds.Y + Convert.ToInt32(childBounds.Height / 2f) - containerBounds.Y);

            // Create a histogram of the 9x9 square around the center
            Dictionary<Color, int> colorCount = new Dictionary<Color, int>();
            int pixelsCounted = 0;
            for (int x = center.X - 4; x < center.X + 5; x++)
            {
                for (int y = center.Y - 4; y < center.Y + 5; y++)
                {
                    if (x < 0 || y < 0)
                        continue;
                    // This is only really valid for tiling background images that are not fixed and
                    // start at the top-left. Oh well.
                    Color pixel = backgroundImage.GetPixel(x % backgroundImage.Width, y % backgroundImage.Height);
                    int count;
                    if (!colorCount.TryGetValue(pixel, out count))
                        count = 0;
                    colorCount[pixel] = ++count;
                    ++pixelsCounted;
                }
            }

            if (pixelsCounted == 0)
                return Color.Transparent;

            List<ColorInt> pairs = new List<ColorInt>(colorCount);
            pairs.Sort(new Comparison<ColorInt>(delegate (ColorInt pairA, ColorInt pairB)
                           {
                               return pairB.Value - pairA.Value;
                           }));

            // If the most common color represents more than half
            // of the pixels, return it
            if (pairs[0].Value > pixelsCounted / 2)
                return pairs[0].Key;

            float alpha = 0, red = 0, green = 0, blue = 0;
            foreach (ColorInt pair in pairs)
            {
                alpha += pair.Key.A * pair.Value;
                red += pair.Key.R * pair.Value;
                green += pair.Key.G * pair.Value;
                blue += pair.Key.B * pair.Value;
            }
            alpha /= pixelsCounted;
            red /= pixelsCounted;
            blue /= pixelsCounted;
            green /= pixelsCounted;

            return Color.FromArgb(
                Convert.ToInt32(alpha),
                Convert.ToInt32(red),
                Convert.ToInt32(green),
                Convert.ToInt32(blue));
        }

        private static Color GetElementColor(GetColorDelegate getter, IHTMLElement element, Color defaultColor)
        {
            string colorString = LookupColor(getter, (IHTMLElement2)element);
            if (colorString != null)
            {
                string textColorHex = ParseColorToHex(colorString);
                Color color = GetColorFromHexColor(textColorHex, defaultColor);
                return color;
            }
            return defaultColor;
        }

        /// <summary>
        /// Returns the background color string associated with the element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string GetTextColorString(IHTMLElement2 element)
        {
            return LookupColor(new GetColorDelegate(_GetTextColorString), element);
        }

        private static string _GetTextColorString(IHTMLElement2 element)
        {
            string color = element.currentStyle.color as string;
            if (color == null)
                color = (string)(element as IHTMLElement).style.color;
            return color;
        }

        /// <summary>
        /// Returns the background color string associated with the element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string GetBackgroundColorString(IHTMLElement2 element)
        {
            return LookupColor(new GetColorDelegate(_GetBackgroundColorString), element);
        }

        private static string _GetBackgroundColorString(IHTMLElement2 element)
        {
            return element.currentStyle.backgroundColor as string;
        }

        /// <summary>
        /// Walks up the element's parent tree to find the color attached to the element.
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private static string LookupColor(GetColorDelegate getter, IHTMLElement2 element)
        {
            string color = getter(element);
            if (color == "transparent" && ((IHTMLElement)element).parentElement != null)
            {
                return LookupColor(getter, (IHTMLElement2)((IHTMLElement)element).parentElement);
            }
            return color;
        }
        private delegate string GetColorDelegate(IHTMLElement2 element);

        /// <summary>
        /// Parses an HTML color value into a properly formed hex RGB string.
        /// </summary>
        /// <param name="htmlColor"></param>
        /// <returns></returns>
        public static string ParseColorToHex(string htmlColor)
        {
            if (htmlColor != null && htmlColor.StartsWith("#", StringComparison.OrdinalIgnoreCase) && htmlColor.Length == 4)
            {
                //in the 3 digit hex representation, repeat each digit to expand it to the 6-digit representation
                htmlColor = "#" + htmlColor[1] + htmlColor[1] + htmlColor[2] + htmlColor[2] + htmlColor[3] + htmlColor[3];
            }
            return htmlColor;
        }

        /// <summary>
        /// Returns true if the hex color is considered dark. This is useful when trying to decide whether to
        /// contrast a color with a light or dark value.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsDarkColor(Color color)
        {
            return color.GetBrightness() < 0.3f;
        }
    }
}
