// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Xml;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Providers
{
    public class BlogProviderFromXml : BlogProvider
    {
        public BlogProviderFromXml(XmlNode providerNode)
        {
            string id = NodeText(providerNode.SelectSingleNode("id"));
            if (id.Length == 0)
                throw new ArgumentException("Missing Id parameter");

            string name = NodeText(providerNode.SelectSingleNode("name"));
            if (name.Length == 0)
                throw new ArgumentException("Missing Name parameter");

            string description = NodeText(providerNode.SelectSingleNode("description"));

            string link = NodeText(providerNode.SelectSingleNode("link"));

            string clientType = NodeText(providerNode.SelectSingleNode("clientType"));
            if (clientType.Length == 0)
                throw new ArgumentException("Missing ClientType parameter");
            if (!BlogClientManager.IsValidClientType(clientType))
                throw new ArgumentException("Invalid ClientType: " + clientType);

            string postApiUrl = NodeText(providerNode.SelectSingleNode("postApiUrl"));
            if (postApiUrl.Length == 0)
                throw new ArgumentException("Invalid PostApiUrl");

            // visibiilty flag
            bool visible = BlogClientOptions.ReadBool(NodeText(providerNode.SelectSingleNode("visible")), true); ;

            // auto-detection
            string homepageUrlPattern = NodeText(providerNode.SelectSingleNode("homepageUrlPattern"));
            string homepageContentPattern = NodeText(providerNode.SelectSingleNode("homepageContentPattern"));
            string rsdEngineNamePattern = NodeText(providerNode.SelectSingleNode("rsdEngineNamePattern"));
            string rsdHomepageLinkPattern = NodeText(providerNode.SelectSingleNode("rsdHomepageLinkPattern"));

            // rsd client type mappings
            ArrayList rsdClientTypeMappings = new ArrayList();
            XmlNodeList mappingNodes = providerNode.SelectNodes("rsdClientTypeMappings/mapping");
            foreach (XmlNode mappingNode in mappingNodes)
            {
                string rsdClientType = NodeText(mappingNode.SelectSingleNode("@rsdClientType"));
                string writerClientType = NodeText(mappingNode.SelectSingleNode("@clientType"));
                if (rsdClientType != String.Empty && writerClientType != String.Empty)
                    rsdClientTypeMappings.Add(new RsdClientTypeMapping(rsdClientType, writerClientType));
            }

            // provider faults
            ArrayList providerFaults = new ArrayList();
            XmlNodeList faultNodes = providerNode.SelectNodes("faults/fault");
            foreach (XmlNode faultNode in faultNodes)
            {
                string codePattern = NodeText(faultNode.SelectSingleNode("codePattern"));
                string stringPattern = NodeText(faultNode.SelectSingleNode("stringPattern"));
                string messageId = NodeText(faultNode.SelectSingleNode("messageId"));
                if (messageId != String.Empty)
                    providerFaults.Add(new ProviderFault(codePattern, stringPattern, messageId));
            }

            // parse options (create generic options object to populate defaults)
            XmlNode optionsNode = providerNode.SelectSingleNode("options");
            foreach (XmlNode node in optionsNode.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    _options.Add(node.Name, node.InnerText.Trim());
                }
            }

            StringId postApiUrlLabel = StringId.CWSelectProviderApiUrlLabel;
            XmlElement postApiUrlDescriptionNode = providerNode.SelectSingleNode("postApiUrlLabel") as XmlElement;
            if (postApiUrlDescriptionNode != null)
            {
                try
                {
                    postApiUrlLabel = (StringId)Enum.Parse(
                        typeof(StringId),
                        "CWSelectProviderApiUrlLabel_" + postApiUrlDescriptionNode.InnerText,
                        false);
                }
                catch
                {
                    Debug.Fail("Invalid value for postApiUrlLabel");
                }
            }

            string appid = null;
            XmlNode appIdNode = providerNode.SelectSingleNode("appid/text()");
            if (appIdNode != null && !string.IsNullOrEmpty(appIdNode.Value))
                appid = appIdNode.Value;

            // initialize
            Init(id, name, description, link, clientType, postApiUrl, postApiUrlLabel, appid,
                 homepageUrlPattern, homepageContentPattern,
                 rsdClientTypeMappings.ToArray(typeof(RsdClientTypeMapping)) as RsdClientTypeMapping[],
                 rsdEngineNamePattern, rsdHomepageLinkPattern,
                 providerFaults.ToArray(typeof(ProviderFault)) as ProviderFault[],
                 visible);
        }

        public override IBlogClientOptions ConstructBlogOptions(IBlogClientOptions defaultOptions)
        {
            return BlogClientOptions.ApplyOptionOverrides(new OptionReader(ReadProviderOption), defaultOptions);
        }

        private string ReadProviderOption(string optionName)
        {
            return _options[optionName] as string;
        }

        private Hashtable _options = new Hashtable();

        private static string NodeText(XmlNode node)
        {
            if (node != null)
                return node.InnerText.Trim();
            else
                return String.Empty;
        }

    }
}

