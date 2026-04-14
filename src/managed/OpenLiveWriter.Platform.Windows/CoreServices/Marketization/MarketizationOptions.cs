// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using OpenLiveWriter.CoreServices.ResourceDownloading;
using OpenLiveWriter.Localization;
using System.Threading;

namespace OpenLiveWriter.CoreServices.Marketization
{
    /// <summary>
    /// Summary description for MarketizationOptions.
    /// </summary>
    public class MarketizationOptions
    {
        //note: if you add something here, make sure you also add it to the switch statement in this file
        //else your feature will not get read in!
        public enum Feature
        {
            Maps,
            TeamBlog,
            TermsOfUse,
            Privacy,
            Help,
            FTPHelp,
            WLGallery,
            LiveClipboard,
            BlogProviders,
            VideoProviders,
            TagProviders,
            VideoCopyright,
            YouTubeVideo,
            AllowMultiSelectImage,
            Wordpress
        }

        // TODO: Stop hardcoding feature parameters throughout the code base.
        public static class WordpressParameters
        {
            public const string SignupUrl = "SignupUrl";
            public const string LanguageMapping = "LanguageMapping";
        }

        private static readonly Hashtable _masterOptions = new Hashtable();
        private static readonly Hashtable _marketOptions = new Hashtable();

        static MarketizationOptions()
        {
            _masterOptions = CabbedXmlResourceFileDownloader.Instance.ProcessLocalResource(
                                 Assembly.GetExecutingAssembly(),
                                 "Marketization.Master.xml",
                                 ReadMarketizationOptionsMaster
                                 ) as Hashtable;

            _marketOptions = CabbedXmlResourceFileDownloader.Instance.ProcessLocalResource(
                     Assembly.GetExecutingAssembly(),
                     "Marketization.Markets.xml",
                     ReadMarketizationOptionsMarket
                     ) as Hashtable;
        }

        public static bool IsFeatureEnabled(Feature name)
        {
            if (_marketOptions.ContainsKey(name))
                return ((FeatureDescription)_marketOptions[name]).Enabled;
            if (_masterOptions.ContainsKey(name))
                return ((FeatureDescription)_masterOptions[name]).Enabled;
            Debug.Assert(false, name + " not found in market or master hashtable");
            return false;
        }

        public static string GetFeatureParameter(Feature name, string parameter)
        {
            string result;
            result = ContainsFeatureParam(_marketOptions, name, parameter);
            if (null != result) return result;
            result = ContainsFeatureParam(_masterOptions, name, parameter);
            if (null != result) return result;
            Debug.Assert(false, name + " not found in market or master hashtable");
            return null;
        }

        private static string ContainsFeatureParam(Hashtable table, Feature name, string parameter)
        {
            if (table.ContainsKey(name))
            {
                //only return this market's param if it exists
                return ((FeatureDescription)table[name]).GetParamValue(parameter);
            }
            return null;
        }

        private static object ReadMarketizationOptionsMaster(XmlDocument marketizationXml)
        {
            return ReadMarketizationOptions(marketizationXml, "default");
        }

        private static object ReadMarketizationOptionsMarket(XmlDocument marketizationXml)
        {
            return ReadMarketizationOptions(marketizationXml, CultureInfo.CurrentUICulture.Name.ToLower(CultureInfo.InvariantCulture));
        }

        private static object ReadMarketizationOptions(XmlDocument providersDocument, string market)
        {
            //note: this has the effect of wiping out any previous settings, since a new hashtable is created
            Hashtable features = new Hashtable();

            XmlNode featuresNode = providersDocument.SelectSingleNode("//features");
            if (featuresNode == null)
                throw new Exception("Invalid marketizationXml.xml file detected");

            string selectionXpath = String.Format(CultureInfo.InvariantCulture, "//features/market[@name='{0}']/feature", market);
            XmlNodeList featureNodes = providersDocument.SelectNodes(selectionXpath);
            foreach (XmlNode featureNode in featureNodes)
                ProcessFeatureXml(featureNode, features);

            return features;
        }

        public static void ProcessFeatureXml(XmlNode featureNode, Hashtable features)
        {
            FeatureDescription description = new FeatureDescription();

            string feature = NodeText(featureNode.Attributes["name"]);
            Feature thisFeature;
            switch (feature)
            {
                case "Writer Blog": thisFeature = Feature.TeamBlog; break;
                case "Insert Maps": thisFeature = Feature.Maps; break;
                case "Terms of Use": thisFeature = Feature.TermsOfUse; break;
                case "Privacy": thisFeature = Feature.Privacy; break;
                case "Help": thisFeature = Feature.Help; break;
                case "FTP Help": thisFeature = Feature.FTPHelp; break;
                case "Gallery": thisFeature = Feature.WLGallery; break;
                case "Live Clipboard": thisFeature = Feature.LiveClipboard; break;
                case "Blog Providers": thisFeature = Feature.BlogProviders; break;
                case "Insert Video": thisFeature = Feature.VideoProviders; break;
                case "Tag Providers": thisFeature = Feature.TagProviders; break;
                case "Video Copyright Link": thisFeature = Feature.VideoCopyright; break;
                case "YouTube Video": thisFeature = Feature.YouTubeVideo; break;
                case "Allow Multiselect Images": thisFeature = Feature.AllowMultiSelectImage; break;
                case "Wordpress": thisFeature = Feature.Wordpress; break;
                default: return;
            }

            string enabled = NodeText(featureNode.Attributes["enabled"]);
            description.Enabled = (enabled == "true") ? true : false;

            //check for parameters
            XmlNodeList parameters = featureNode.SelectNodes("parameter");
            foreach (XmlNode param in parameters)
            {
                description.AddParam(param.Attributes["name"].Value, param.Attributes["value"].Value);
            }

            if (features.ContainsKey(thisFeature))
                features[thisFeature] = description;
            else
                features.Add(thisFeature, description);

        }

        private static string NodeText(XmlNode node)
        {
            if (node != null)
                return node.InnerText.Trim();
            else
                return String.Empty;
        }

        private class FeatureDescription
        {
            private bool _enabled = false;
            private readonly Hashtable _parameters = new Hashtable();

            public void AddParam(string paramName, string paramValue)
            {
                if (_parameters.Contains(paramName))
                    _parameters[paramName] = paramValue;
                else
                    _parameters.Add(paramName, paramValue);
            }

            public bool Enabled
            {
                get
                {
                    return _enabled;
                }
                set
                {
                    _enabled = value;
                }
            }

            public string GetParamValue(string paramName)
            {
                if (_parameters.Contains(paramName))
                    return (string)_parameters[paramName];
                return null;
            }

        }
    }

    public sealed class WordpressSettings
    {
        private WordpressSettings()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;
            string map = MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.Wordpress, MarketizationOptions.WordpressParameters.LanguageMapping);
            string mapTo = null;
            if (!string.IsNullOrEmpty(map))
            {
                string[] mapSplit = map.Split(',');
                for (int i = 1; i < mapSplit.Length; i += 2)
                {
                    // in our mapping table (markets.xml), if the "from" entry is longer than two chars, we compare it against
                    // the current UI culture *name*, not the current UI culture *TwoLetterISOLanguageName*.
                    string mapFrom = mapSplit[i - 1].Trim();
                    string currentCultureIdOrLanguage = mapFrom.Length > 2 ? currentCulture.Name : currentCulture.TwoLetterISOLanguageName;

                    if (string.Compare(mapFrom, currentCultureIdOrLanguage, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        mapTo = mapSplit[i].Trim();
                        break;
                    }
                }
            }

            // If no mapping, by default we use the current culture's two-letter-ISO-langauge-name.
            if (mapTo == null)
            {
                mapTo = currentCulture.TwoLetterISOLanguageName;
            }

            string url = string.Format(CultureInfo.InvariantCulture, MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.Wordpress, MarketizationOptions.WordpressParameters.SignupUrl), mapTo);
            this.uri = new Uri(url);
        }

        private Uri uri;

        public Uri Uri
        {
            get { return uri; }
        }

        private static WordpressSettings value;

        public static WordpressSettings Value
        {
            get
            {
                if (WordpressSettings.value == null)
                {
                    WordpressSettings.value = new WordpressSettings();
                }
                return WordpressSettings.value;
            }
        }
    }
}

