// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{

    /// <summary>
    /// Helper methods for manipulating html nodes
    /// </summary>
    public class HTMLElementHelper
    {
        /// <summary>
        /// Helper method to insert an element to a specified location (if the element already
        /// exists in the document it will be moved to the location)
        /// </summary>
        static public void InsertElement(IHTMLElement source, HTMLElementLocation location)
        {
            switch (location.Position)
            {
                case HTMLElementRelativePosition.None:
                    Debug.Assert(location.AdjacentElement == null);
                    ((IHTMLDOMNode)location.Container).appendChild((IHTMLDOMNode)source);
                    break;

                case HTMLElementRelativePosition.After:
                    ((IHTMLElement2)location.AdjacentElement).insertAdjacentElement("afterEnd", source);
                    break;

                case HTMLElementRelativePosition.Before:
                    ((IHTMLElement2)location.AdjacentElement).insertAdjacentElement("beforeBegin", source);
                    break;
            }
        }

        public static Rectangle GetClientRectangle(IHTMLElement element)
        {
            IHTMLRect rect = ((IHTMLElement2)element).getBoundingClientRect();
            Rectangle clientRect = new Rectangle(rect.left - 2, rect.top - 2, rect.right - rect.left, rect.bottom - rect.top);
            return clientRect;
        }

        public static bool IsBold(IHTMLElement2 element2)
        {
            return element2.currentStyle.fontWeight is int
                                    ? (int)element2.currentStyle.fontWeight > 400
                                    : element2.currentStyle.fontWeight.ToString() == "bold";
        }

        public static string GetFontFamily(IHTMLElement2 element2)
        {
            string fontFamily = element2.currentStyle.fontFamily;
            if (!String.IsNullOrEmpty(fontFamily))
                return fontFamily.Replace('\"', '\'');

            return fontFamily;
        }

        public static bool IsItalic(IHTMLElement2 element2)
        {
            return element2.currentStyle.fontStyle == "italic" || element2.currentStyle.fontStyle == "oblique";
        }

        public static bool IsUnderline(IHTMLElement2 element2)
        {
            return element2.currentStyle.textDecoration != null && element2.currentStyle.textDecoration.Contains("underline");
        }

        public static bool IsStrikeThrough(IHTMLElement2 element2)
        {
            return element2.currentStyle.textDecoration != null && element2.currentStyle.textDecoration.Contains("line-through");
        }

        public static bool IsOverline(IHTMLElement2 element2)
        {
            return element2.currentStyle.textDecoration != null && element2.currentStyle.textDecoration.Contains("overline");
        }

        public static string GetComposeSettingsForeColor(IHTMLElement2 element2)
        {
            return GetComposeSettingsColor(element2.currentStyle.color);
        }

        public static string GetComposeSettingsBackgroundColor(IHTMLElement2 element2)
        {
            return GetComposeSettingsColor(element2.currentStyle.backgroundColor);
        }

        private static string GetComposeSettingsColorFromThreeCharacterString(string colorTableString)
        {
            Debug.Assert(colorTableString.Length == 3, "Unexpected string length");

            int r = Convert.ToInt32(colorTableString[0].ToString(), 16);
            int g = Convert.ToInt32(colorTableString[1].ToString(), 16);
            int b = Convert.ToInt32(colorTableString[2].ToString(), 16);

            return String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}",
                r << 4 | r,
                g << 4 | g,
                b << 4 | b);
        }

        private static string GetComposeSettingsColorFromSixCharacterString(string colorTableString)
        {
            Debug.Assert(colorTableString.Length == 6, "Unexpected string length");

            int r1 = Convert.ToInt32(colorTableString[0].ToString(), 16);
            int r2 = Convert.ToInt32(colorTableString[1].ToString(), 16);

            int g1 = Convert.ToInt32(colorTableString[2].ToString(), 16);
            int g2 = Convert.ToInt32(colorTableString[3].ToString(), 16);

            int b1 = Convert.ToInt32(colorTableString[4].ToString(), 16);
            int b2 = Convert.ToInt32(colorTableString[5].ToString(), 16);

            return String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}",
                r1 << 4 | r2,
                g1 << 4 | g2,
                b1 << 4 | b2);
        }

        private static Dictionary<string, string> colorTable;
        private static void EnsureColorTable()
        {
            if (colorTable != null)
                return;

            colorTable = new Dictionary<string, string>(147);
            colorTable.Add("transparent", String.Empty);

            // 16 HTML 4.01 standard colors
            colorTable.Add("black", "0.0.0");
            colorTable.Add("silver", "192.192.192");
            colorTable.Add("gray", "128.128.128");
            colorTable.Add("white", "255.255.255");
            colorTable.Add("maroon", "128.0.0");
            colorTable.Add("red", "255.0.0");
            colorTable.Add("purple", "128.0.128");
            colorTable.Add("fuchsia", "255.0.255");
            colorTable.Add("green", "0.128.0");
            colorTable.Add("lime", "0.255.0");
            colorTable.Add("olive", "128.128.0");
            colorTable.Add("yellow", "255.255.0");
            colorTable.Add("navy", "0.0.128");
            colorTable.Add("blue", "0.0.255");
            colorTable.Add("teal", "0.128.128");
            colorTable.Add("aqua", "0.255.255");

            // 1 CSS2.1 color
            colorTable.Add("orange", "255.165.0");

            // 146 - 17 = 129 Non-standard colors: http://msdn.microsoft.com/en-us/library/aa358802(VS.85).aspx
            colorTable.Add("antiquewhite", "250.235.215");
            //colorTable.Add("aqua", "0.255.255");
            colorTable.Add("aquamarine", "127.255.212");
            colorTable.Add("azure", "240.255.255");
            colorTable.Add("beige", "245.245.220");
            colorTable.Add("bisque", "255.228.196");
            //colorTable.Add("black", "0.0.0");
            colorTable.Add("blanchedalmond", "255.235.205");
            //colorTable.Add("blue", "0.0.255");
            colorTable.Add("blueviolet", "138.43.226");
            colorTable.Add("brown", "165.42.42");
            colorTable.Add("burlywood", "222.184.135");
            colorTable.Add("cadetblue", "95.158.160");
            colorTable.Add("chartreuse", "127.255.0");
            colorTable.Add("chocolate", "210.105.30");
            colorTable.Add("coral", "255.127.80");
            colorTable.Add("cornflowerblue", "100.149.237");
            colorTable.Add("cornsilk", "255.248.220");
            colorTable.Add("crimson", "220.20.60");
            colorTable.Add("cyan", "0.255.255");
            colorTable.Add("darkblue", "0.0.139");
            colorTable.Add("darkcyan", "0.139.139");
            colorTable.Add("darkgoldenrod", "184.134.11");
            colorTable.Add("darkgray", "169.169.169");
            colorTable.Add("darkgrey", "169.169.169");
            colorTable.Add("darkgreen", "0.100.0");
            colorTable.Add("darkkhaki", "189.183.107");
            colorTable.Add("darkmagenta", "139.0.139");
            colorTable.Add("darkolivegreen", "85.107.47");
            colorTable.Add("darkorange", "255.140.0");
            colorTable.Add("darkorchid", "153.50.204");
            colorTable.Add("darkred", "139.0.0");
            colorTable.Add("darksalmon", "233.150.122");
            colorTable.Add("darkseagreen", "143.188.143");
            colorTable.Add("darkslateblue", "72.61.139");
            colorTable.Add("darkslategray", "47.79.79");
            colorTable.Add("darkslategrey", "47.79.79");
            colorTable.Add("darkturquoise", "0.206.209");
            colorTable.Add("darkviolet", "148.0.211");
            colorTable.Add("deeppink", "255.20.147");
            colorTable.Add("deepskyblue", "0.191.255");
            colorTable.Add("dimgray", "105.105.105");
            colorTable.Add("dimgrey", "105.105.105");
            colorTable.Add("dodgerblue", "30.144.255");
            colorTable.Add("firebrick", "178.34.34");
            colorTable.Add("floralwhite", "255.250.240");
            colorTable.Add("forestgreen", "34.139.34");
            //colorTable.Add("fuchsia", "255.0.255");
            colorTable.Add("gainsboro", "220.220.220");
            colorTable.Add("ghostwhite", "248.248.255");
            colorTable.Add("gold", "255.215.0");
            colorTable.Add("goldenrod", "218.165.32");
            //colorTable.Add("gray", "128.128.128");
            colorTable.Add("grey", "128.128.128");
            //colorTable.Add("green", "0.128.0");
            colorTable.Add("greenyellow", "173.255.47");
            colorTable.Add("honeydew", "240.255.240");
            colorTable.Add("hotpink", "255.105.180");
            colorTable.Add("indianred", "205.92.92");
            colorTable.Add("indigo", "75.0.130");
            colorTable.Add("ivory", "255.255.240");
            colorTable.Add("khaki", "240.230.140");
            colorTable.Add("lavender", "230.230.250");
            colorTable.Add("lavenderblush", "255.240.245");
            colorTable.Add("lawngreen", "124.252.0");
            colorTable.Add("lemonchiffon", "255.250.205");
            colorTable.Add("lightblue", "173.216.230");
            colorTable.Add("lightcoral", "240.128.128");
            colorTable.Add("lightcyan", "224.255.255");
            colorTable.Add("lightgoldenrodyellow", "250.250.210");
            colorTable.Add("lightgreen", "144.238.144");
            colorTable.Add("lightgray", "211.211.211");
            colorTable.Add("lightgrey", "211.211.211");
            colorTable.Add("lightpink", "255.182.193");
            colorTable.Add("lightsalmon", "255.160.122");
            colorTable.Add("lightseagreen", "32.178.170");
            colorTable.Add("lightskyblue", "135.206.250");
            colorTable.Add("lightslategray", "119.136.153");
            colorTable.Add("lightslategrey", "119.136.153");
            colorTable.Add("lightsteelblue", "176.196.222");
            colorTable.Add("lightyellow", "255.255.224");
            //colorTable.Add("lime", "0.255.0");
            colorTable.Add("limegreen", "50.205.50");
            colorTable.Add("linen", "250.240.230");
            colorTable.Add("magenta", "255.0.255");
            //colorTable.Add("maroon", "128.0.0");
            colorTable.Add("mediumaquamarine", "102.205.170");
            colorTable.Add("mediumblue", "0.0.205");
            colorTable.Add("mediumorchid", "186.85.211");
            colorTable.Add("mediumpurple", "147.112.219");
            colorTable.Add("mediumseagreen", "60.179.113");
            colorTable.Add("mediumslateblue", "123.104.238");
            colorTable.Add("mediumspringgreen", "0.250.154");
            colorTable.Add("mediumturquoise", "72.209.204");
            colorTable.Add("mediumvioletred", "199.21.133");
            colorTable.Add("midnightblue", "25.25.112");
            colorTable.Add("mintcream", "245.255.250");
            colorTable.Add("mistyrose", "255.228.225");
            colorTable.Add("moccasin", "255.228.181");
            colorTable.Add("navajowhite", "255.222.173");
            //colorTable.Add("navy", "0.0.128");
            colorTable.Add("oldlace", "253.245.230");
            //colorTable.Add("olive", "128.128.0");
            colorTable.Add("olivedrab", "107.142.35");
            //colorTable.Add("orange", "255.165.0");
            colorTable.Add("orangered", "255.69.0");
            colorTable.Add("orchid", "218.112.214");
            colorTable.Add("palegoldenrod", "238.232.170");
            colorTable.Add("palegreen", "152.251.152");
            colorTable.Add("paleturquoise", "175.238.238");
            colorTable.Add("palevioletred", "219.112.147");
            colorTable.Add("papayawhip", "255.239.213");
            colorTable.Add("peachpuff", "255.218.185");
            colorTable.Add("peru", "205.133.63");
            colorTable.Add("pink", "255.192.203");
            colorTable.Add("plum", "221.160.221");
            colorTable.Add("powderblue", "176.224.230");
            //colorTable.Add("purple", "128.0.128");
            //colorTable.Add("red", "255.0.0");
            colorTable.Add("rosybrown", "188.143.143");
            colorTable.Add("royalblue", "65.105.225");
            colorTable.Add("saddlebrown", "139.69.19");
            colorTable.Add("salmon", "250.128.114");
            colorTable.Add("sandybrown", "244.164.96");
            colorTable.Add("seagreen", "46.139.87");
            colorTable.Add("seashell", "255.245.238");
            colorTable.Add("sienna", "160.82.45");
            //colorTable.Add("silver", "192.192.192");
            colorTable.Add("skyblue", "135.206.235");
            colorTable.Add("slateblue", "106.90.205");
            colorTable.Add("slategray", "112.128.144");
            colorTable.Add("slategrey", "112.128.144");
            colorTable.Add("snow", "255.250.250");
            colorTable.Add("springgreen", "0.255.127");
            colorTable.Add("steelblue", "70.130.180");
            colorTable.Add("tan", "210.180.140");
            //colorTable.Add("teal", "0.128.128");
            colorTable.Add("thistle", "216.191.216");
            colorTable.Add("tomato", "255.99.71");
            colorTable.Add("turquoise", "64.224.208");
            colorTable.Add("violet", "238.130.238");
            colorTable.Add("wheat", "245.222.179");
            //colorTable.Add("white", "255.255.255");
            colorTable.Add("whitesmoke", "245.245.245");
            //colorTable.Add("yellow", "255.255.0");
            colorTable.Add("yellowgreen", "154.205.50");
        }
        /// <summary>
        /// Get an appropriately formatting IDM.COMPOSESETTINGS string from a color table color.
        /// For further information see:
        /// IDM.COMPOSESETTINGS: http://msdn.microsoft.com/en-us/library/aa769901(VS.85).aspx
        /// Color table: http://msdn.microsoft.com/en-us/library/ms531197(VS.85).aspx
        /// Returns an empty string if the conversion fails.
        /// </summary>
        /// <param name="obj">A valid color table representation, i.e. IHTMLElement2.IHTMLCurrentStyle.color</param>
        /// <returns></returns>
        private static string GetComposeSettingsColor(object obj)
        {
            if (obj is string)
            {
                string colorTableString = (string)obj;
                if (colorTableString.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    // color: #rgb
                    if (colorTableString.Length == 4)
                        return GetComposeSettingsColorFromThreeCharacterString(colorTableString.Substring(1, 3));

                    // color: #rrggbb
                    if (colorTableString.Length == 7)
                        return GetComposeSettingsColorFromSixCharacterString(colorTableString.Substring(1, 6));
                }
                else if (colorTableString.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // color: rgb(r, g, b)
                        string[] rgb = colorTableString.Substring(4).Split(new[] { ' ', ')' },
                                                                           StringSplitOptions.RemoveEmptyEntries);
                        if (rgb.Length == 3)
                        {

                            return String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}",
                                                 MathHelper.Clip(Int32.Parse(rgb[0], CultureInfo.InvariantCulture), 0, 255),
                                                 MathHelper.Clip(Int32.Parse(rgb[1], CultureInfo.InvariantCulture), 0, 255),
                                                 MathHelper.Clip(Int32.Parse(rgb[2], CultureInfo.InvariantCulture), 0, 255));
                        }

                        // color: rgb(100%, 0%, 0%)
                        rgb = colorTableString.Substring(4).Split(new[] { ' ', ')', '%' },
                                                                  StringSplitOptions.RemoveEmptyEntries);
                        if (rgb.Length == 3)
                        {
                            return String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}",
                                                 Convert.ToInt32(255.0 * MathHelper.Clip(Int32.Parse(rgb[0], CultureInfo.InvariantCulture), 0, 100) / 100.0),
                                                 Convert.ToInt32(255.0 * MathHelper.Clip(Int32.Parse(rgb[0], CultureInfo.InvariantCulture), 0, 100) / 100.0),
                                                 Convert.ToInt32(255.0 * MathHelper.Clip(Int32.Parse(rgb[0], CultureInfo.InvariantCulture), 0, 100) / 100.0));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Fail("Exception thrown while parsing color table string: " + ex);
                    }
                }
                else
                {
                    EnsureColorTable();
                    string composeSettingsString;
                    if (colorTable.TryGetValue(colorTableString.ToLowerInvariant(), out composeSettingsString))
                    {
                        return composeSettingsString;
                    }

                    if (colorTableString.Length == 3)
                    {
                        // color: rgb
                        return GetComposeSettingsColorFromThreeCharacterString(colorTableString);
                    }
                    else if (colorTableString.Length == 6)
                    {
                        // color: rrggbb
                        return GetComposeSettingsColorFromSixCharacterString(colorTableString);
                    }
                }
            }
            else if (obj is int)
            {
                int colorTableInt = (int)obj;
                return String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}",
                                     (colorTableInt >> 16) % 256,
                                     (colorTableInt >> 8) % 256,
                                     colorTableInt % 256);
            }

            Debug.Fail("Failed to get formatted color string.");
            return String.Empty;
        }

        public static bool IsRTLElement(IHTMLElement e)
        {
            return IsDirElement(e, "rtl");
        }

        private static bool IsDirElement(IHTMLElement e, string direction)
        {
            string dir = e.getAttribute("dir", 2) as string;
            if (null != dir)
                return String.Compare(dir, direction, StringComparison.OrdinalIgnoreCase) == 0;

            return false;
        }

        /// <summary>
        /// The HTML element that bounds the fragment being edited
        /// </summary>
        public static IHTMLElement GetFragmentElement(IHTMLDocument3 document, string fragmentId)
        {
            return document.getElementById(fragmentId);
        }

        public static Point CalculateOffset(IHTMLElement element)
        {
            Point p = new Point(0, 0);
            while (element != null && !(element is IHTMLBodyElement))
            {
                p.X += element.offsetLeft;
                p.Y += element.offsetTop;
                element = element.offsetParent;
            }
            return p;
        }

        public static bool IsChildOrSameElement(IHTMLElement parent, IHTMLElement child)
        {
            if (parent.sourceIndex == child.sourceIndex)
                return true;

            do
            {
                child = child.parentElement;
            } while (child != null && parent.sourceIndex != child.sourceIndex);

            return child != null;
        }

        /// <summary>
        /// Add a new element of the specified type to the specified container
        /// </summary>
        /// <param name="elementName"></param>
        /// <param name="htmlDocument"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        static public IHTMLElement AddNewElementToContainer(string elementName, IHTMLDocument2 htmlDocument, IHTMLElement container)
        {
            IHTMLElement newElement = htmlDocument.createElement(elementName);
            ((IHTMLDOMNode)container).appendChild((IHTMLDOMNode)newElement);
            return newElement;
        }

        /// <summary>
        /// Helper method to remove an element from a document
        /// </summary>
        /// <param name="element">element to remove</param>
        static public void RemoveElement(IHTMLElement element)
        {
            IHTMLDOMNode elementNode = (IHTMLDOMNode)element;
            elementNode.removeNode(true);
        }

        /// <summary>
        /// Make a clone of the passed html element
        /// </summary>
        /// <param name="element">element to clone</param>
        /// <returns>clone of element</returns>
        static public IHTMLElement CloneElement(IHTMLElement element)
        {
            IHTMLDOMNode elementNode = (IHTMLDOMNode)element;
            return (IHTMLElement)elementNode.cloneNode(true);
        }

        static public void SwapElements(IHTMLElement source, IHTMLElement destination)
        {
            IHTMLDOMNode sourceNode = (IHTMLDOMNode)source;
            IHTMLDOMNode destinationNode = (IHTMLDOMNode)destination;
            sourceNode.swapNode(destinationNode);
        }

        /// <summary>
        /// Get the top of the element relative to the window client area
        /// </summary>
        /// <param name="element">element to get top of</param>
        /// <returns>top of element relative to the client area</returns>
        static public int GetTopRelativeToClient(IHTMLElement htmlElement)
        {
            // start with the offset top of the element
            int top = htmlElement.offsetTop;

            // chase up the offset chain until we find the body (unless this element is the body)
            IHTMLElement2 rootOffsetElement = null;
            IHTMLElement offsetParent = htmlElement.offsetParent;
            if (offsetParent != null)
            {
                // chase up the chain until we find the body
                while (offsetParent.offsetParent != null)
                {
                    top += offsetParent.offsetTop;
                    offsetParent = offsetParent.offsetParent;
                }

                // record a reference to the body element
                rootOffsetElement = (IHTMLElement2)offsetParent;

            }
            else // element is the body
            {
                rootOffsetElement = (IHTMLElement2)htmlElement;
            }

            // now offset the offset based on the scroll position of the body
            Debug.Assert((rootOffsetElement as IHTMLElement).offsetParent == null);
            top -= rootOffsetElement.scrollTop;

            // return the top
            return top;
        }

        /// <summary>
        /// Get the left of the element relative to the window client area
        /// </summary>
        /// <param name="element">element to get top of</param>
        /// <returns>top of element relative to the client area</returns>
        static public int GetLeftRelativeToClient(IHTMLElement htmlElement)
        {
            // start with the offset left of the element
            int left = htmlElement.offsetLeft;

            // chase up the offset chain until we find the body (unless this element is the body)
            IHTMLElement2 rootOffsetElement2 = null;
            IHTMLElement offsetParent = htmlElement.offsetParent;
            if (offsetParent != null)
            {
                // chase up the chain until we find the body
                while (offsetParent.offsetParent != null)
                {
                    left += offsetParent.offsetLeft;
                    offsetParent = offsetParent.offsetParent;
                }
                left += offsetParent.offsetLeft;

                // record a reference to the body element
                rootOffsetElement2 = (IHTMLElement2)offsetParent;

            }
            else // element is the body
            {
                rootOffsetElement2 = (IHTMLElement2)htmlElement;
            }

            // now offset the offset based on the scroll position of the body
            IHTMLElement rootOffsetElement = (IHTMLElement)rootOffsetElement2;
            Debug.Assert(rootOffsetElement.offsetParent == null);
            left -= rootOffsetElement2.scrollLeft;

            // return the left
            return left;
        }

        /// <summary>
        /// Get the top of the element relative to the body
        /// </summary>
        /// <param name="element">element to get top of</param>
        /// <returns>top of element relative to the body</returns>
        static public int GetTopRelativeToBody(IHTMLElement element)
        {
            // get a local reference to the element
            IHTMLElement htmlElement = element;

            // initialize top
            int top = 0;

            // chase up the offset chain until we find the body (unless this element is the body)
            while (!(htmlElement is IHTMLBodyElement) && htmlElement != null)
            {
                top += htmlElement.offsetTop;
                htmlElement = htmlElement.offsetParent;
            }

            // return the top
            return top;
        }

        /// <summary>
        /// Get the left of the elment relative to the body
        /// </summary>
        /// <param name="element">element to get left of</param>
        /// <returns>left of element relative to the body</returns>
        static public int GetLeftRelativeToBody(IHTMLElement element)
        {
            // get a local reference to the element
            IHTMLElement htmlElement = element;

            // initialize left
            int left = 0;

            // chase up the offset chain until we find the body (unless this element is the body)
            while (!(htmlElement is IHTMLBodyElement) && htmlElement != null)
            {
                left += htmlElement.offsetLeft;
                htmlElement = htmlElement.offsetParent;
            }

            // return the left
            return left;
        }

        /// <summary>
        /// Converts a collection of IHTMLElements into an array.
        /// This utility is used as a replacement for ArrayList.ToArray() because
        /// that operation throws bogus "object not castable" exceptions on IHTMLElements
        /// in release mode.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static IHTMLElement[] ToElementArray(ICollection collection)
        {
            IHTMLElement[] array = new IHTMLElement[collection.Count];
            int i = 0;
            foreach (IHTMLElement e in collection)
            {
                array[i++] = e;
            }
            return array;
        }

        /// <summary>
        /// Finds the image whose center is nearest to the center of the element passed in
        /// </summary>
        /// <param name="element">The element to find an image near</param>
        /// <param name="document">The document in which to look for the image</param>
        /// <returns>The image element, or null.</returns>
        public static IHTMLImgElement FindImageNearElement(IHTMLElement element, IHTMLDocument2 document)
        {
            IHTMLElement imageElement = null;
            // Get the center of the element
            int top = GetTopRelativeToBody(element);
            int left = GetLeftRelativeToBody(element);
            int x = (top + element.offsetHeight) / 2;
            int y = (left + element.offsetWidth) / 2;

            // look at each image and find the nearest center
            int minDistance = 9999;
            foreach (IHTMLElement image in document.images)
            {
                int imagex = GetTopRelativeToBody(image);
                int imagey = GetLeftRelativeToBody(image);

                // Get the image whose top left is nearest to the center of the clicked element
                int totalDistance = Math.Abs(x - imagex) + Math.Abs(y - imagey);
                if (totalDistance < minDistance && image.offsetHeight > 2 && image.offsetWidth > 2)
                {
                    minDistance = totalDistance;
                    imageElement = image;
                }
            }
            return (IHTMLImgElement)imageElement;
        }

        /// <summary>
        /// Gets the parent link element (if any) for the passed element
        /// </summary>
        /// <param name="element">element</param>
        /// <returns>link element (or null if this element or one of its parents are not a link)</returns>
        public static IHTMLAnchorElement GetContainingAnchorElement(IHTMLElement element)
        {
            // search up the parent heirarchy
            while (element != null)
            {
                // if it is an anchor that has an HREF (exclude anchors with only NAME)
                // then stop searching
                if (element is IHTMLAnchorElement)
                {
                    if (element.getAttribute("href", 2) != null)
                        return element as IHTMLAnchorElement;
                }

                // search parent
                element = element.parentElement;
            }

            // didn't find an anchor
            return null;
        }

        public static bool ElementsAreEqual(IHTMLElement element1, IHTMLElement element2)
        {
            if (element1 == null || element2 == null)
                return false;
            else
                return element1.sourceIndex == element2.sourceIndex;
        }

        /// <summary>
        /// Warning: This will clear the "id" attribute from the source element since ids must be unique!
        /// </summary>
        public static void CopyAttributes(IHTMLElement sourceElement, IHTMLElement targetElement)
        {
            (targetElement as IHTMLElement2).mergeAttributes(sourceElement);

            // MSHTML doesn't copy over the "id" attribute, so we manually copy it over.
            string sourceId = sourceElement.id;
            if (!String.IsNullOrEmpty(sourceId))
            {
                sourceElement.id = String.Empty;
                targetElement.id = sourceId;
            }
        }

        public static bool HasMeaningfulAttributes(IHTMLElement element)
        {
            IHTMLAttributeCollection attributeCollection = (IHTMLAttributeCollection)((IHTMLDOMNode)element).attributes;
            foreach (IHTMLDOMAttribute attribute in attributeCollection)
            {
                // For our purposes here anything but size is considered meaningful.
                if (attribute.specified && String.Compare(attribute.nodeName, "size", StringComparison.OrdinalIgnoreCase) != 0)
                    return true;
            }

            // If we get here then, size is the only attribute which could have been specified.
            // Note that size="+0" is not a meaningful attribute
            string sizeAttribute = element.getAttribute("size", 2) as string;
            if (sizeAttribute == null || String.Compare(sizeAttribute, "+0", StringComparison.OrdinalIgnoreCase) == 0)
                return false;

            return true;
        }

        public delegate string CSSUnitStringDelegate(IHTMLElement element);
        public delegate float LastChanceDelegate(string cssUnits, IHTMLElement element, bool vertical);

        public static float LastChanceBorderWidthPointSize(string cssUnits, IHTMLElement element, bool vertical)
        {
            if (cssUnits == "THIN")
                return PixelsToPointSize(1, vertical);
            if (cssUnits == "MEDIUM")
                return PixelsToPointSize(3, vertical);
            if (cssUnits == "THICK")
                return PixelsToPointSize(5, vertical);

            return 0;
        }

        private static float LastChanceFontPointSize(string cssUnits, IHTMLElement element, bool vertical)
        {
            // xx-small, x-small, small, medium, large, x-large, xx-large
            if (cssUnits == "XX-SMALL")
                return 8;

            if (cssUnits == "X-SMALL")
                return 10;

            if (cssUnits == "SMALL")
                return 12;

            if (cssUnits == "MEDIUM")
                return 14;

            if (cssUnits == "LARGE")
                return 18;

            if (cssUnits == "X-LARGE")
                return 24;

            if (cssUnits == "XX-LARGE")
                return 36;

            if (cssUnits == "LARGER")
            {
                //Debug.Assert(false, "Implement LARGER");
                return 0;
            }

            if (cssUnits == "SMALLER")
            {
                //Debug.Assert(false, "Implement SMALLER");
                return 0;
            }

            // Finally, we may have a font tag size attribute, e.g. 1-7
            int fontTagAttribute = Convert.ToInt32(cssUnits, CultureInfo.InvariantCulture);
            if (fontTagAttribute >= 1 && fontTagAttribute <= 7)
            {
                return Convert.ToInt32(HtmlFontSizeToPointFontSize(fontTagAttribute));
            }

            return 0;
        }

        public static float LastChanceLineHeightPointSize(string cssUnits, IHTMLElement element, bool vertical)
        {
            if (String.Compare(cssUnits, "normal", StringComparison.OrdinalIgnoreCase) == 0)
                return CSSUnitStringToPointSize(CSSUnitStringFontSize, element, LastChanceFontPointSize, vertical);

            return 0;
        }

        /// <summary>
        /// Returns the font size (pt) given the CSS font size string
        /// Returns 0 if the font size cannot be determined.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static float GetFontPointSize(IHTMLElement element)
        {
            return CSSUnitStringToPointSize(CSSUnitStringFontSize, element, LastChanceFontPointSize, true);
        }

        public static float PixelsToPointSize(float pixels, bool vertical)
        {
            return 72 * pixels / (vertical ? DisplayHelper.PixelsPerLogicalInchY : DisplayHelper.PixelsPerLogicalInchX);
        }

        public static float PointSizeToPixels(float pointSize, bool vertical)
        {
            return pointSize * (vertical ? DisplayHelper.PixelsPerLogicalInchY : DisplayHelper.PixelsPerLogicalInchX) / 72;
        }

        public static float CSSUnitStringToPointSize(CSSUnitStringDelegate cssUnitStringDelegate, IHTMLElement element, LastChanceDelegate lastChanceDelegate, bool vertical)
        {
            if (element == null && lastChanceDelegate == LastChanceFontPointSize)
            {
                const string FONT_SIZE_INITIAL_VALUE = "MEDIUM";

                return LastChanceFontPointSize(FONT_SIZE_INITIAL_VALUE, element, vertical);
            }

            string cssUnits = cssUnitStringDelegate(element);

            if (String.IsNullOrEmpty(cssUnits) ||
                cssUnits == "auto")
                return 0;

            int result = 0;
            try
            {
                // @RIBBON TODO: Do we need to remove spaces also?
                cssUnits.Trim();
                cssUnits = cssUnits.ToUpperInvariant();

                // PERCENTAGE
                int i = cssUnits.IndexOf("%", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    float percentageOfParent = (float)Convert.ToDouble(cssUnits.Substring(0, i), CultureInfo.InvariantCulture);

                    if (percentageOfParent == 0.0f)
                    {
                        // Shortcut if we're given 0%.
                        return 0;
                    }

                    // In the case of percentages, each CSS property defines what the percentage refers to. There are
                    // more cases than just the following (see the Percentages column at http://www.w3.org/TR/CSS21/propidx.html
                    // for a full list) so if percentages are not working correctly you may need to add additional cases.
                    // Right now, this should be the only case that we hit.
                    if (cssUnitStringDelegate == CSSUnitStringLineHeight)
                    {
                        // The percentage for line-height refers to the font size of the element itself.
                        return percentageOfParent * CSSUnitStringToPointSize(CSSUnitStringFontSize, element, LastChanceFontPointSize, vertical) / 100;
                    }

                    return percentageOfParent * CSSUnitStringToPointSize(cssUnitStringDelegate, element.parentElement, lastChanceDelegate, vertical) / 100;
                }

                i = cssUnits.IndexOf("PT", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    return (float)Convert.ToDouble(cssUnits.Substring(0, i), CultureInfo.InvariantCulture);
                }

                i = cssUnits.IndexOf("PC", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    // 1 pc is 12 pt.
                    return (float)Convert.ToDouble(cssUnits.Substring(0, i), CultureInfo.InvariantCulture) * 12;
                }

                i = cssUnits.IndexOf("IN", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    // 1 inch is 72 pt.
                    return (float)Convert.ToDouble(cssUnits.Substring(0, i), CultureInfo.InvariantCulture) * 72;
                }

                i = cssUnits.IndexOf("CM", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    // 2.54 cm is 72pt --> 1 cm is 28.3464567pt.
                    return (float)Convert.ToDouble(cssUnits.Substring(0, i), CultureInfo.InvariantCulture) * 28.3464567f;
                }

                i = cssUnits.IndexOf("MM", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    // 1 mm is 0.283464567pt.
                    return (float)Convert.ToDouble(cssUnits.Substring(0, i), CultureInfo.InvariantCulture) * 0.283464567f;
                }

                // RELATIVE SIZE
                i = cssUnits.IndexOf("EM", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    float parentMultiplier = (float)Convert.ToDouble(cssUnits.Substring(0, i), CultureInfo.InvariantCulture);

                    if (cssUnitStringDelegate == CSSUnitStringFontSize)
                    {
                        // When 'em' occurs in the value of font-size itself, it refers to the font size of the parent element.
                        return parentMultiplier * CSSUnitStringToPointSize(cssUnitStringDelegate, element.parentElement, LastChanceFontPointSize, vertical);
                    }

                    // Otherwise, the 'em' unit is relative to the computed value of the font-size attribute of the current element.
                    return parentMultiplier * CSSUnitStringToPointSize(CSSUnitStringFontSize, element, LastChanceFontPointSize, vertical);
                }

                i = cssUnits.IndexOf("PX", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    float pixels = (float)Convert.ToDouble(cssUnits.Substring(0, i), CultureInfo.InvariantCulture);
                    return 72 * pixels / (vertical ? DisplayHelper.PixelsPerLogicalInchY : DisplayHelper.PixelsPerLogicalInchX);
                }

                i = cssUnits.IndexOf("EX", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    //Debug.Assert(false, "IMPLEMENT EX");
                    return 0;
                }

                double number;
                if (Double.TryParse(cssUnits, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                {
                    // The line-height property supports numbers with no units.
                    if (cssUnitStringDelegate == CSSUnitStringLineHeight)
                    {
                        // The percentage for line-height refers to the font size of the element itself.
                        return ((float)number) * CSSUnitStringToPointSize(CSSUnitStringFontSize, element, lastChanceDelegate, vertical);
                    }
                }

                // Opportunity for property specific handling
                if (lastChanceDelegate != null)
                    return lastChanceDelegate(cssUnits, element, vertical);
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to convert CSS units: " + ex);
            }
            return result;
        }

        public static float CSSUnitStringToPixelSize(CSSUnitStringDelegate cssUnitStringDelegate, IHTMLElement element, LastChanceDelegate lastChanceDelegate, bool vertical)
        {
            return PointSizeToPixels(CSSUnitStringToPointSize(cssUnitStringDelegate, element, lastChanceDelegate, vertical), vertical);
        }

        /// <summary>
        /// Returns the HTML font size (1-7) based on the point size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static float HtmlFontSizeToPointFontSize(int size)
        {
            if (size <= 1)
                return 8;
            else if (size <= 2)
                return 10;
            else if (size <= 3)
                return 12;
            else if (size <= 4)
                return 14;
            else if (size <= 5)
                return 18;
            else if (size <= 6)
                return 24;
            else
                return 36;
        }

        /// <summary>
        /// Returns the HTML font size (1-7) based on the point size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static int PointFontSizeToHtmlFontSize(float size)
        {
            if (size <= 6)
                return 0;
            else if (size <= 8)
                return 1;
            else if (size <= 10)
                return 2;
            else if (size <= 12)
                return 3;
            else if (size <= 14)
                return 4;
            else if (size <= 18)
                return 5;
            else if (size <= 24)
                return 6;
            else
                return 7;
        }

        public static string CSSUnitStringMarginTop(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.marginTop as string;
        }

        public static string CSSUnitStringMarginLeft(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.marginLeft as string;
        }

        public static string CSSUnitStringMarginBottom(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.marginBottom as string;
        }

        public static string CSSUnitStringMarginRight(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.marginRight as string;
        }

        public static string CSSUnitStringPaddingLeft(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.paddingLeft as string;
        }

        public static string CSSUnitStringPaddingTop(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.paddingTop as string;
        }

        public static string CSSUnitStringPaddingRight(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.paddingRight as string;
        }

        public static string CSSUnitStringPaddingBottom(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.paddingBottom as string;
        }

        private static IHTMLElement ParentElement(IHTMLElement element)
        {
            return element.parentTextEdit ?? element.parentElement;
        }

        private static string CSSUnitStringFontSize(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.fontSize as string;
        }

        public static string CSSUnitStringBorderLeft(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.borderLeftWidth as string;
        }

        public static string CSSUnitStringBorderTop(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.borderTopWidth as string;
        }

        public static string CSSUnitStringBorderRight(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.borderRightWidth as string;
        }

        public static string CSSUnitStringBorderBottom(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.borderBottomWidth as string;
        }

        public static string CSSUnitStringWidth(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.width as string;
        }

        public static string CSSUnitStringHeight(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.height as string;
        }

        public static string CSSUnitStringLineHeight(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.lineHeight as string;
        }

        public static string CSSUnitStringBackgroundPositionX(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.backgroundPositionX as string;
        }

        public static string CSSUnitStringBackgroundPositionY(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.backgroundPositionY as string;
        }

        public static string CSSUnitStringBorderSpacingX(IHTMLElement element)
        {
            return CSSUnitStringBorderSpacing(element, true);
        }

        public static string CSSUnitStringBorderSpacingY(IHTMLElement element)
        {
            return CSSUnitStringBorderSpacing(element, false);
        }

        private static string CSSUnitStringBorderSpacing(IHTMLElement element, bool x)
        {
            // Only available on IE8 or higher.
            IHTMLCurrentStyle5 style = ((IHTMLElement2)element).currentStyle as IHTMLCurrentStyle5;
            if (style != null)
            {
                string spacing = style.borderSpacing;
                if (!String.IsNullOrEmpty(spacing))
                {
                    spacing = spacing.Trim();

                    // The borderSpacing property can return one or two length values. If one length is specified, it
                    // represents both the horizontal and vertical spacing. If two are specified, the first represents the
                    // horizontal spacing, the second the vertical spacing.
                    int firstSpace = spacing.IndexOf(' ');
                    if (firstSpace > 0)
                    {
                        if (x)
                        {
                            return spacing.Substring(0, firstSpace);
                        }
                        else
                        {
                            return spacing.Substring(firstSpace, spacing.Length - firstSpace);
                        }
                    }

                    return spacing;
                }
            }

            // Default per CSS 2.1 spec.
            return "0";
        }

        public static string CSSUnitStringTop(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.top as string;
        }

        public static string CSSUnitStringRight(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.right as string;
        }

        public static string CSSUnitStringBottom(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.bottom as string;
        }

        public static string CSSUnitStringLeft(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.left as string;
        }

        public static string CSSUnitStringClipTop(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.clipTop as string;
        }

        public static string CSSUnitStringClipRight(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.clipRight as string;
        }

        public static string CSSUnitStringClipBottom(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.clipBottom as string;
        }

        public static string CSSUnitStringClipLeft(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.clipLeft as string;
        }

        public static string CSSUnitStringLetterSpacing(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.letterSpacing as string;
        }

        public static string CSSUnitStringMaxHeight(IHTMLElement element)
        {
            IHTMLCurrentStyle4 currentStyle4 = (IHTMLCurrentStyle4)((IHTMLElement2)element).currentStyle;
            return currentStyle4.maxHeight as string;
        }

        public static string CSSUnitStringMaxWidth(IHTMLElement element)
        {
            IHTMLCurrentStyle4 currentStyle4 = (IHTMLCurrentStyle4)((IHTMLElement2)element).currentStyle;
            return currentStyle4.maxWidth as string;
        }

        public static string CSSUnitStringMinHeight(IHTMLElement element)
        {
            IHTMLCurrentStyle3 currentStyle3 = (IHTMLCurrentStyle3)((IHTMLElement2)element).currentStyle;
            return currentStyle3.minHeight as string;
        }

        public static string CSSUnitStringMinWidth(IHTMLElement element)
        {
            IHTMLCurrentStyle4 currentStyle4 = (IHTMLCurrentStyle4)((IHTMLElement2)element).currentStyle;
            return currentStyle4.minWidth as string;
        }

        public static string CSSUnitStringOutlineWidth(IHTMLElement element)
        {
            // Only available on IE8 or higher.
            IHTMLCurrentStyle5 currentStyle5 = ((IHTMLElement2)element).currentStyle as IHTMLCurrentStyle5;
            if (currentStyle5 != null)
            {
                try
                {
                    return currentStyle5.outlineWidth as string;
                }
                catch (COMException e)
                {
                    if (e.ErrorCode == HRESULT.E_FAILED)
                    {
                        // Known issue, just ignore the exception.
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Default per CSS 2.1 spec.
            return "MEDIUM";
        }

        public static string CSSUnitStringTextIndent(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.textIndent as string;
        }

        public static string CSSUnitStringVerticalAlign(IHTMLElement element)
        {
            return ((IHTMLElement2)element).currentStyle.verticalAlign as string;
        }

        public static string CSSUnitStringWordSpacing(IHTMLElement element)
        {
            IHTMLCurrentStyle3 currentStyle3 = (IHTMLCurrentStyle3)((IHTMLElement2)element).currentStyle;
            return currentStyle3.wordSpacing as string;
        }

        public static Padding PaddingInPixels(IHTMLElement element)
        {
            float left = CSSUnitStringToPointSize(CSSUnitStringPaddingLeft, element, null, false);
            float top = CSSUnitStringToPointSize(CSSUnitStringPaddingTop, element, null, true);
            float right = CSSUnitStringToPointSize(CSSUnitStringPaddingRight, element, null, false);
            float bottom = CSSUnitStringToPointSize(CSSUnitStringPaddingBottom, element, null, true);

            return new Padding((int)PointSizeToPixels(left, false),
                               (int)PointSizeToPixels(top, true),
                               (int)PointSizeToPixels(right, false),
                               (int)PointSizeToPixels(bottom, true));
        }

        public static Padding BorderInPixels(IHTMLElement element)
        {
            float left = CSSUnitStringToPointSize(CSSUnitStringBorderLeft, element, LastChanceBorderWidthPointSize, false);
            float top = CSSUnitStringToPointSize(CSSUnitStringBorderTop, element, LastChanceBorderWidthPointSize, true);
            float right = CSSUnitStringToPointSize(CSSUnitStringBorderRight, element, LastChanceBorderWidthPointSize, false);
            float bottom = CSSUnitStringToPointSize(CSSUnitStringBorderBottom, element, LastChanceBorderWidthPointSize, true);

            return new Padding((int)PointSizeToPixels(left, false),
                               (int)PointSizeToPixels(top, true),
                               (int)PointSizeToPixels(right, false),
                               (int)PointSizeToPixels(bottom, true));
        }
    }

    /// <summary>
    /// Class used to indicate the location of an HTML element within a document
    /// </summary>
    public class HTMLElementLocation
    {
        /// <summary>
        /// Initialize with only a container (since there is no adjacent element, the 'position'
        /// implied by this is that of the sole element in a container)
        /// </summary>
        /// <param name="container">container</param>
        public HTMLElementLocation(IHTMLElement container)
        {
            _container = container;
        }

        /// <summary>
        /// Initialize the insertion point with a container, adjacent element and relative position
        /// </summary>
        /// <param name="adjacentElement"></param>
        /// <param name="position"></param>
        public HTMLElementLocation(IHTMLElement adjacentElement, HTMLElementRelativePosition position)
        {
            _container = adjacentElement.parentElement;
            _adjacentElement = adjacentElement;
            Position = position;
        }

        /// <summary>
        /// Initialize the insertion point with a container, adjacent element and relative position
        /// </summary>
        /// <param name="container">container</param>
        /// <param name="insertBefore">insert before</param>
        public HTMLElementLocation(IHTMLElement container, IHTMLElement adjacentElement, HTMLElementRelativePosition position)
        {
            _container = container;
            _adjacentElement = adjacentElement;
            Position = position;
        }

        /// <summary>
        /// Container to insert into
        /// </summary>
        public IHTMLElement Container
        {
            get
            {
                return _container;
            }
        }
        private readonly IHTMLElement _container = null;

        /// <summary>
        /// Element to insert next to
        /// </summary>
        public IHTMLElement AdjacentElement
        {
            get
            {
                return _adjacentElement;
            }
        }
        private readonly IHTMLElement _adjacentElement = null;

        /// <summary>
        /// Direction (relative to adjacent element) to do the insert
        /// </summary>
        public readonly HTMLElementRelativePosition Position = HTMLElementRelativePosition.None;

    }

    /// <summary>
    /// Relative position for html element
    /// </summary>
    public enum HTMLElementRelativePosition
    {
        None,
        Before,
        After
    }

}
