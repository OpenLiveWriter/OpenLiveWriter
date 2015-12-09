// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.CoreServices.ResourceDownloading;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tagging
{
    /// <summary>
    /// Summary description for TagProviderManager.
    /// </summary>
    public class TagProviderManager
    {
        public TagProviderManager(IProperties properties)
        {
            _properties = properties;
        }
        private IProperties _properties;

        public TagProvider[] TagProviders
        {
            get
            {
                return MergeTagProviders(XmlTagProviders, LoadUserTagProviders());
            }
        }

        public void Save(TagProvider provider)
        {
            IProperties providerProps = TagProviderOptions.GetSubProperties(provider.Id);
            providerProps.SetString(NAME, provider.Name);
            providerProps.SetString(CAPTION, provider.Caption);
            providerProps.SetString(FORMAT, provider.HtmlFormat);
            providerProps.SetString(SEPARATOR, provider.Separator);
        }

        public void Delete(TagProvider provider)
        {
            TagProviderOptions.RemoveSubProperties(provider.Id);
            foreach (TagProvider xmlProvider in XmlTagProviders)
            {
                if (xmlProvider.Id == provider.Id)
                {
                    Suppress(provider);
                    break;
                }
            }
        }

        private IProperties TagProviderOptions
        {
            get
            {
                return _properties.GetSubProperties("TagProviders");
            }
        }

        private bool IsSuppressed(TagProvider provider)
        {
            return _properties.GetSubProperties(SUPPRESSED).GetBoolean(provider.Id, false);
        }

        private void Suppress(TagProvider provider)
        {
            _properties.GetSubProperties(SUPPRESSED).SetBoolean(provider.Id, true);
        }

        public void RestoreDefaults()
        {
            foreach (TagProvider provider in XmlTagProviders)
            {
                TagProviderOptions.RemoveSubProperties(provider.Id);
            }
            _properties.RemoveSubProperties(SUPPRESSED);
        }

        private TagProvider[] MergeTagProviders(TagProvider[] writerProviders, TagProvider[] userProviders)
        {
            ArrayList mergedProviders = new ArrayList();
            mergedProviders.AddRange(userProviders);
            foreach (TagProvider provider in writerProviders)
            {
                if (!mergedProviders.Contains(provider) && !IsSuppressed(provider))
                    mergedProviders.Add(provider);
            }
            return (TagProvider[])mergedProviders.ToArray(typeof(TagProvider));
        }

        private TagProvider[] LoadUserTagProviders()
        {
            ArrayList providers = new ArrayList();
            string[] providerIds = TagProviderOptions.SubPropertyNames;
            foreach (string id in providerIds)
                providers.Add(LoadTagProvider(id));

            return (TagProvider[])providers.ToArray(typeof(TagProvider));
        }

        private TagProvider LoadTagProvider(string id)
        {
            IProperties providerProps = TagProviderOptions.GetSubProperties(id);
            string name = providerProps.GetString(NAME, "");
            string caption = providerProps.GetString(CAPTION, null);
            string htmlFormat = providerProps.GetString(FORMAT, null);
            string separator = providerProps.GetString(SEPARATOR, null);
            string encodingName = providerProps.GetString(ENCODINGNAME, null);
            return new TagProvider(id, name, caption, htmlFormat, separator, encodingName);
        }

        private static TagProvider[] XmlTagProviders
        {
            get
            {
                if (_tagProviders == null)
                {

                    _tagProviders = LoadXmlTagProviders(false);
                }
                return _tagProviders;
            }
        }
        private static TagProvider[] _tagProviders;

        private static TagProvider[] LoadXmlTagProviders(bool allowDownload)
        {
            return CabbedXmlResourceFileDownloader.Instance.ProcessLocalResource(
                Assembly.GetExecutingAssembly(),
                "Tagging.TagProviders.xml",
                ReadXmlTagProviders) as TagProvider[];
        }

        private static object ReadXmlTagProviders(XmlDocument providersDocument)
        {
            XmlNode providersNode = providersDocument.SelectSingleNode("//tagProviders");
            if (providersNode == null)
                throw new Exception("Invalid tagproviders.xml file detected");

            // get the list of providers from the xml
            HashSet marketSupportedIds = new HashSet();
            marketSupportedIds.AddAll(
                StringHelper.Split(
                MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.TagProviders, "supported"), ";"));
            ArrayList providers = new ArrayList();
            XmlNodeList providerNodes = providersDocument.SelectNodes("//tagProviders/provider");
            foreach (XmlNode providerNode in providerNodes)
            {
                TagProvider provider = TagProviderFromXml(providerNode);
                if (marketSupportedIds.Contains(provider.Id))
                    providers.Add(provider);
            }

            // return list of providers
            return providers.ToArray(typeof(TagProvider));
        }

        private static TagProvider TagProviderFromXml(XmlNode providerNode)
        {
            string id = GetString(providerNode, ID);
            string name = GetString(providerNode, NAME);
            string caption = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.TagsCaptionFormat), name);
            string separator = GetString(providerNode, SEPARATOR);

            string formatString = GetString(providerNode, FORMAT);
            string encodingName = GetString(providerNode, ENCODINGNAME);

            return new TagProvider(id, name, caption, formatString, separator, encodingName);
        }

        private static string GetString(XmlNode node, string nodeName)
        {
            string nodeValue = NodeText(node.SelectSingleNode(nodeName));
            if (nodeValue.Length == 0 && nodeName != ENCODINGNAME)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Res.Get(StringId.TagsXmlParsingError), nodeName));
            return nodeValue.Trim();
        }

        private static string NodeText(XmlNode node)
        {
            if (node != null)
                return node.InnerText;
            else
                return String.Empty;
        }

        private const string ID = "id";
        private const string CAPTION = "caption";
        private const string FORMAT = "htmlFormatString";
        private const string SEPARATOR = "separator";
        private const string NAME = "name";
        private const string SUPPRESSED = "suppressed";
        private const string ENCODINGNAME = "encodingName";

    }
}
