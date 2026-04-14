// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Web;
using Microsoft.Win32;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for GLink.
    /// </summary>
    public class GLink
    {
        public static GLink Instance
        {
            get
            {
                return _instance;
            }
        }
        private static readonly GLink _instance;

        static GLink()
        {
            _instance = new GLink();
        }

        private GLink()
        {
        }

        public string CreateMicrosoftAccountID
        {
            get
            {
                return GetGLink("CreateMicrosoftAccountID");
            }
        }

        public string LearnMore
        {
            get
            {
                return GetGLink("LearnMore");
            }
        }

        public string FTPHelp
        {
            get
            {
                return GetGLink(MarketizationOptions.Feature.FTPHelp);
            }
        }

        public string DownloadPlugins
        {
            get
            {
                return GetGLink(MarketizationOptions.Feature.WLGallery);
            }
        }

        public string DownloadUpdatedWriter
        {
            get
            {
                return GetGLink("WriterMerchPage");
            }
        }

        public string NewUser
        {
            get
            {
                return GetGLink("NewUser");
            }
        }

        public string PhotosHome
        {
            get
            {
                return GetGLink("PhotosHome");
            }
        }

        public string Help
        {
            get
            {
                return GetGLink(MarketizationOptions.Feature.Help);
            }
        }

        public string ConfigurationData
        {
            get
            {
                return GetGLink("ConfigurationData");
            }
        }

        public string PrivacyStatement
        {
            get
            {
                return GetGLink(MarketizationOptions.Feature.Privacy);
            }
        }

        public string CEIP
        {
            get
            {
                return GetGLink("WriterCEIP");
            }
        }

        public string MoreAboutLiveClipboard
        {
            get
            {
                return GetGLink(MarketizationOptions.Feature.LiveClipboard);
            }
        }

        public string Community
        {
            get
            {
                return GetGLink("WriterForumV3");
            }
        }

        public string TermOfUse
        {
            get
            {
                return GetGLink(MarketizationOptions.Feature.TermsOfUse);
            }
        }

        public string CodeOfConduct
        {
            get
            {
                return GetGLink("CodeOfConduct");
            }
        }

        public string YouTubeTermOfUse
        {
            get
            {
                return GetGLink(MarketizationOptions.Feature.YouTubeVideo, null, "YouTubeTOU");
            }
        }

        public string YouTubeSafety
        {
            get
            {
                return GetGLink(MarketizationOptions.Feature.YouTubeVideo, null, "YouTubeSafety");
            }
        }

        public string YouTubeRegister
        {
            get
            {
                return GetGLink(MarketizationOptions.Feature.YouTubeVideo, null, "YouTubeRegister");
            }
        }

        private string GetGLink(MarketizationOptions.Feature id)
        {
            return GetGLink(id, null);
        }

        private string GetGLink(MarketizationOptions.Feature id, string queryString)
        {
            return GetGLink(id, queryString, "Glink");
        }

        private string GetGLink(MarketizationOptions.Feature id, string queryString, string paramName)
        {
            string glink = MarketizationOptions.GetFeatureParameter(id, paramName);
            return FixUpGLink(glink, queryString);
        }

        private string GetGLink(string id)
        {
            // Allow override of g-links
            string url = _settings.GetString(id, null);
            if (url != null)
            {
                Trace.WriteLine("Using registry override for g-link " + id + " : " + url);
                return url;
            }

            string glink = string.Format(CultureInfo.InvariantCulture, "http://openlivewriter.com/WriterRedirect/{0}", id);
            return FixUpGLink(glink, null);
        }

        private string FixUpGLink(string glink, string queryString)
        {
            queryString = string.Format(CultureInfo.InvariantCulture, "{0}Version={1}&Build={2}&Market={3}",
                queryString != null ? queryString : "?",
                ApplicationEnvironment.ProductVersionMajor,
                ApplicationEnvironment.ProductVersionMinor,
                CultureInfo.CurrentUICulture.Name);

            return glink + queryString;
        }

        private readonly SettingsPersisterHelper _settings = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("glinks");
    }

}
