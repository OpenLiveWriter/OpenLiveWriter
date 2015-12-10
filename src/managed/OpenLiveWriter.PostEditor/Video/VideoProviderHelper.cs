// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.CoreServices.ResourceDownloading;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Video
{
    /// <summary>
    /// Summary description for VideoProviderHelper.
    /// </summary>
    public class VideoProviderHelper
    {
        private static VideoProvider[] _videoProviders;

        public static VideoProvider[] VideoProviders
        {
            get
            {
                if (_videoProviders == null)
                {
                    _videoProviders = GetXmlVideoProviders();
                }
                return _videoProviders;
            }
        }

        private static VideoProvider[] GetXmlVideoProviders()
        {
            try
            {
                return CabbedXmlResourceFileDownloader.Instance.ProcessLocalResource(
                        Assembly.GetExecutingAssembly(),
                        "Video.VideoProvidersB2.xml",
                        ReadXmlVideoProviders) as VideoProvider[];
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to load video providers: " + ex);
                return null;
            }

        }

        private static object ReadXmlVideoProviders(XmlDocument providersDocument)
        {
            XmlNode providersNode = providersDocument.SelectSingleNode("//videoProviders");
            if (providersNode == null)
                throw new Exception("Invalid videoProviders.xml file detected");

            // get the list of providers from the xml
            ArrayList providers = new ArrayList();
            HashSet marketSupportedIds = new HashSet();
            marketSupportedIds.AddAll(
                StringHelper.Split(
                MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.VideoProviders, "supported"), ";"));
            XmlNodeList providerNodes = providersDocument.SelectNodes("//videoProviders/provider");
            foreach (XmlNode providerNode in providerNodes)
            {
                VideoProvider provider = VideoProviderFromXml(providerNode);
                if (marketSupportedIds.Contains(provider.ServiceId))
                    providers.Add(provider);
            }

            // return list of providers
            return providers.ToArray(typeof(VideoProvider));
        }

        public static VideoProvider VideoProviderFromXml(XmlNode providerNode)
        {
            string serviceName = NodeText(providerNode.SelectSingleNode("serviceName"));
            if (serviceName.Length == 0)
                throw new ArgumentException(Res.Get(StringId.Plugin_Video_XML_Error_Service_Name));

            string id = NodeText(providerNode.SelectSingleNode("id"));
            if (id.Length == 0)
                throw new ArgumentException(Res.Get(StringId.Plugin_Video_XML_Error_Service_Id));

            string embedFormat = NodeText(providerNode.SelectSingleNode("embedFormat"));
            if (embedFormat.Length == 0)
                throw new ArgumentException(Res.Get(StringId.Plugin_Video_XML_Error_Embed_Format));
            embedFormat = embedFormat.Replace("{market}", CultureInfo.CurrentUICulture.Name);

            string editorFormat = NodeText(providerNode.SelectSingleNode("editorFormat"));
            editorFormat = editorFormat.Replace("{market}", CultureInfo.CurrentUICulture.Name);

            XmlNode embedPatternNode = providerNode.SelectSingleNode("embedPatterns");
            if (embedPatternNode == null)
                throw new ArgumentException(Res.Get(StringId.Plugin_Video_XML_Error_Embed_Pattern));
            XmlNodeList embedPatterns = embedPatternNode.SelectNodes("pattern");
            if (embedPatterns.Count == 0)
                throw new ArgumentException(Res.Get(StringId.Plugin_Video_XML_Error_Embed_Pattern));

            EmbedPattern[] embedPatternList = new EmbedPattern[embedPatterns.Count];
            int i = 0;
            foreach (XmlNode patternNode in embedPatterns)
            {
                string attribute = patternNode.Attributes["name"].Value;
                string pattern = NodeText(patternNode);
                embedPatternList[i++] = new EmbedPattern(attribute, pattern);
            }

            string urlFormat = NodeText(providerNode.SelectSingleNode("urlFormat"));
            if (urlFormat.Length == 0)
                throw new ArgumentException(Res.Get(StringId.Plugin_Video_XML_Error_Url_Format));

            XmlNode urlPatternNode = providerNode.SelectSingleNode("urlPattern");
            string urlPattern = NodeText(urlPatternNode);
            if (urlPattern.Length == 0)
                throw new ArgumentException(Res.Get(StringId.Plugin_Video_XML_Error_Url_Pattern));

            bool urlConvertingError = false;
            if (urlPatternNode.Attributes["error"] != null)
                urlConvertingError = StringHelper.ToBool(urlPatternNode.Attributes["error"].Value, false);

            int width;
            int height;
            XmlNode sizeNode = providerNode.SelectSingleNode("size");
            if (sizeNode == null)
            {
                throw new ArgumentException(Res.Get(StringId.Plugin_Video_XML_Error_Size));
            }

            width = int.Parse(NodeText(sizeNode.SelectSingleNode("width")), CultureInfo.InvariantCulture);
            height = int.Parse(NodeText(sizeNode.SelectSingleNode("height")), CultureInfo.InvariantCulture);

            XmlNode backgroundNode = providerNode.SelectSingleNode("backgroundColor");
            string backgroundColor = NodeText(backgroundNode);

            XmlNode urlAtomPatternNode = providerNode.SelectSingleNode("urlAtomPattern");
            string urlAtomPattern = String.Empty;
            if (urlAtomPatternNode != null)
                urlAtomPattern = NodeText(urlAtomPatternNode);

            XmlNode urlAtomFormatNode = providerNode.SelectSingleNode("urlAtomFormat");
            string urlAtomFormat = String.Empty;
            if (urlAtomFormatNode != null)
                urlAtomFormat = NodeText(urlAtomFormatNode);

            XmlNode appIdNode = providerNode.SelectSingleNode("appServiceId");
            string appId = NodeText(appIdNode);

            XmlNode urlNode = providerNode.SelectSingleNode("publishPingUrl");
            string[] urls = null;
            if (urlNode != null)
            {
                int start = Convert.ToInt32(urlNode.Attributes["start"].Value, CultureInfo.InvariantCulture);
                int end = Convert.ToInt32(urlNode.Attributes["end"].Value, CultureInfo.InvariantCulture);
                urls = new string[end - start + 1];
                for (int j = 0; j < urls.Length; j++)
                {
                    urls[j] = NodeText(urlNode).Replace("{i}", (j + start).ToString(CultureInfo.InvariantCulture));
                }
            }
            return new VideoProvider(serviceName, id, embedFormat, editorFormat, urlFormat, embedPatternList, urlPattern, urlConvertingError, width, height, backgroundColor, appId, urls, urlAtomPattern, urlAtomFormat);
        }

        private static string NodeText(XmlNode node)
        {
            if (node != null)
                return node.InnerText.Trim();
            else
                return String.Empty;
        }

        public static WhiteList[] WhiteLists
        {
            get
            {
                if (_whiteLists == null)
                {
                    _whiteLists = GetXmlWhiteLists();
                }
                return _whiteLists;
            }
        }

        private static WhiteList[] _whiteLists;

        private static WhiteList[] GetXmlWhiteLists()
        {
            return CabbedXmlResourceFileDownloader.Instance.ProcessLocalResource(
                    Assembly.GetExecutingAssembly(),
                    "Video.VideoProvidersB2.xml",
                    ReadXmlWhiteLists) as WhiteList[];
        }

        private static object ReadXmlWhiteLists(XmlDocument whitelistDocument)
        {
            ArrayList whiteLists = new ArrayList();

            XmlNode whiteListsNode = whitelistDocument.SelectSingleNode("//videoProviders/whitelists");
            if (whiteListsNode != null)
            {
                // get the list of providers from the xml
                XmlNodeList whiteListNodes = whiteListsNode.SelectNodes("whitelist");
                foreach (XmlNode whiteListNode in whiteListNodes)
                    whiteLists.Add(WhiteListFromXml(whiteListNode));
            }
            // return list of whitelists
            return whiteLists.ToArray(typeof(WhiteList));
        }

        public static WhiteList WhiteListFromXml(XmlNode whiteListNode)
        {
            string blogProviderId = whiteListNode.Attributes["blogprovider"].Value;
            if (blogProviderId.Length == 0)
                throw new ArgumentException();

            Hashtable mappings = new Hashtable();

            XmlNodeList items = whiteListNode.SelectNodes("allowed");
            foreach (XmlNode node in items)
            {
                string providerId = node.Attributes["providerId"].Value;
                string pattern = NodeText(node);
                mappings.Add(providerId, pattern);
            }

            return new WhiteList(blogProviderId, mappings);
        }

    }

    public class RectTest
    {
        public readonly double X;
        public readonly double Y;
        public readonly int Width;
        public readonly int Height;
        public readonly Color Color;

        public RectTest(double x, double y, int width, int height, Color color)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Color = color;
        }
    }
}
