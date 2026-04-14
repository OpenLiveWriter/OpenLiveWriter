// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Drawing;

namespace OpenLiveWriter.CoreServices
{

    public sealed class XmlHelper
    {
        public static string GetUrl(XmlNode el, string xpath, Uri documentUri)
        {
            return GetUrl(el, xpath, null, documentUri);
        }

        public static DateTime? GetDate(XmlNode el, string xpath, XmlNamespaceManager nsMgr, Uri documentUri)
        {
            if (el == null)
                return null;
            XmlNode node = nsMgr == null
                ? el.SelectSingleNode(xpath)
                : el.SelectSingleNode(xpath, nsMgr);
            if (node == null)
                return null;
            string rawDate = node.Value; // example: 2010-04-02T18:02:55Z
            if (String.IsNullOrEmpty(rawDate))
                return null;

            DateTime dateTime;
            if (DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return dateTime;
            }

            return null;
        }

        public static string GetUrl(XmlNode el, string xpath, XmlNamespaceManager nsMgr, Uri documentUri)
        {
            if (el == null)
                return null;
            XmlNode node = nsMgr == null
                ? el.SelectSingleNode(xpath)
                : el.SelectSingleNode(xpath, nsMgr);
            if (node == null)
                return null;
            string rawUrl = node.Value;
            if (rawUrl == null || rawUrl.Length == 0)
                return null;

            // optimize for common case of absolute path
            if (rawUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return UrlHelper.SafeToAbsoluteUri(new Uri(rawUrl));
                }
                catch { }
            }

            ArrayList ancestors = new ArrayList();

            XmlNode parent = node is XmlAttribute ? ((XmlAttribute)node).OwnerElement : node.ParentNode;
            while (parent != null)
            {
                ancestors.Add(parent);
                parent = parent.ParentNode;
            }

            ancestors.Reverse();

            Uri uri = documentUri;

            foreach (XmlNode anc in ancestors)
            {
                if (anc is XmlElement)
                {
                    XmlElement baseEl = (XmlElement)anc;
                    if (baseEl.HasAttribute("xml:base"))
                    {
                        string thisUri = baseEl.GetAttribute("xml:base");
                        if (uri == null)
                            uri = new Uri(thisUri);
                        else
                            uri = new Uri(uri, thisUri);
                    }
                }
            }

            if (uri == null)
                return UrlHelper.SafeToAbsoluteUri(new Uri(rawUrl));
            else
                return UrlHelper.SafeToAbsoluteUri(new Uri(uri, rawUrl));
        }

        public static void ApplyBaseUri(XmlDocument doc, Uri documentUri)
        {
            Uri uri = documentUri;
            string existingUri = doc.DocumentElement.GetAttribute("xml:base");
            if (existingUri != "")
                uri = new Uri(uri, existingUri);
            doc.DocumentElement.SetAttribute("xml:base", UrlHelper.SafeToAbsoluteUri(uri));
        }

        public static string NodeText(XmlNode node)
        {
            if (node != null)
                return node.InnerText.Trim();
            else
                return String.Empty;
        }

        public static bool NodeBool(XmlNode node, bool defaultValue)
        {
            // get the node text
            string boolValue = NodeText(node);

            // if not empty then read value
            if (boolValue != String.Empty)
            {
                switch (boolValue.Trim().ToUpperInvariant())
                {
                    case "YES":
                    case "TRUE":
                    case "1":
                        return true;
                    case "NO":
                    case "FALSE":
                    case "0":
                        return false;
                }
            }

            // if not specified or invalid value then return default
            return defaultValue;
        }

        public static int NodeInt(XmlNode node, int defaultValue)
        {
            string intValue = NodeText(node);
            try
            {
                return int.Parse(intValue, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static Size NodeSize(XmlNode node, Size defaultValue)
        {
            string sizeValue = NodeText(node);
            string[] strings = ((string)sizeValue).Split(',');
            if (strings.Length != 2)
                return defaultValue;
            try
            {
                return new Size(
                    int.Parse(strings[0], CultureInfo.InvariantCulture),
                    int.Parse(strings[1], CultureInfo.InvariantCulture));
            }
            catch
            {
                return defaultValue;
            }
        }

    }
}
