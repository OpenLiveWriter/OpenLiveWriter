// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.InternalWriterPlugin
{
    /// <summary>
    /// Helper for dealing with creating and parsing local and remote map URLs.
    /// </summary>
    public class MapUrlHelper
    {
        static string LIVE_MAP_URL = @"http://www.bing.com/maps/default.aspx";
        private const string MAP_QUERY_FORMAT = "v=2&cp={0}&lvl={1}&style={2}"; // change these with great care- they may break the sidebar previewing!
        private const string MAP_WHERE_QUERY_FORMAT = "v=2&where={0}&lvl={1}&style={2}";

        private MapUrlHelper()
        {
        }

        public static string CreateMapUrl(string baseUrl, float latitude, float longitude, string reserved, string style, int level, VEPushpin[] pushpins, VEBirdseyeScene birdseyeScene)
        {
            //string queryString = String.Format(MAP_QUERY_FORMAT, latitude, longitude, level, style);

            StringBuilder sb = new StringBuilder();
            String coords = latitude.ToString(CultureInfo.InvariantCulture) + "~" + longitude.ToString(CultureInfo.InvariantCulture);
            if (reserved != null && reserved != String.Empty)
                coords = reserved;

            sb.AppendFormat(MAP_QUERY_FORMAT, coords, level, style);
            if (birdseyeScene != null)
            {
                sb.Append("&scene=");
                sb.Append(birdseyeScene.SceneId);
            }

            if (pushpins != null && pushpins.Length > 0)
            {
                sb.Append("&sp=");
                for (int i = 0; i < pushpins.Length; i++)
                {
                    VEPushpin pin = pushpins[i];
                    if (i > 0)
                    {
                        sb.Append("~");
                    }
                    sb.AppendFormat(CultureInfo.InvariantCulture, "aN.{0}_{1}_{2}_{3}", pin.VELatLong.Latitude, pin.VELatLong.Longitude, EncodeQueryValue(pin.Title), EncodeQueryValue(pin.Details));
                    if (pin.MoreInfoUrl != String.Empty || pin.PhotoUrl != String.Empty)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "_{0}", EncodeQueryValue(pin.MoreInfoUrl));
                        if (pin.PhotoUrl != String.Empty)
                            sb.AppendFormat(CultureInfo.InvariantCulture, "_{0}", EncodeQueryValue(pin.PhotoUrl));
                    }
                }
            }

            // ja-jp is the only non en supported market for maps, so special case it
            string cultureName = CultureInfo.CurrentUICulture.Name.ToUpperInvariant();
            if (IsSupportedLanguage(cultureName))
                sb.Append("&mkt=" + cultureName);
            else
                sb.Append("&mkt=en-us");

            string queryString = sb.ToString();

            return baseUrl + "?" + queryString;
        }

        private static bool IsSupportedLanguage(string cultureName)
        {
            switch (cultureName)
            {
                case ("JA-JP"):
                case ("IT-IT"):
                case ("FR-FR"):
                case ("ES-ES"):
                    return true;
                default:
                    return false;
            }
        }

        private static string EncodeQueryValue(string v)
        {
            //Hack to create URL encoded strings that are equivalent to those generated on live.com
            //TODO: need to really scrutinize the way strings need to be encoded
            //(for instance, a literal %20 pushpin value gets improperly escaped to a space character)
            string enc = HttpUtility.UrlEncode(v);
            enc = enc.Replace("+", "%20"); //they explicitly convert + chars to %20
            enc = HttpUtility.UrlEncode(enc); //they seem to double-escape the encoded values
            enc = enc.Replace("_", "%25255f"); //they specially escape _ since its a delimiter
            return enc;
        }

        public static string CreateMapUrl(string baseUrl, string address, string style, int level)
        {
            string queryString = String.Format(CultureInfo.InvariantCulture, MAP_WHERE_QUERY_FORMAT, address, level, style);
            return String.Format(CultureInfo.InvariantCulture, "{0}?{1}", baseUrl, queryString);
        }

        public static string CreateLiveUrl(float latitude, float longitude, string reserved, string style, int level, VEPushpin[] pushpins, VEBirdseyeScene birdseyeScene)
        {
            return CreateMapUrl(LIVE_MAP_URL, latitude, longitude, reserved, style, level, pushpins, birdseyeScene);
        }

        public static string FixedUpMapHtml(string origFilePath)
        {
            //1. read in the html
            string fileHtml = FileHelper.ReadFile(origFilePath);
            //2. sub in our marketized url
            string url = MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.Maps, "url");

            fileHtml = fileHtml.Replace("{apiurl}", url);
            //3. create temp file
            string newFilePath = TempFileManager.Instance.CreateTempFile("preview.html");
            //4. write to new file
            FileHelper.WriteFile(newFilePath, fileHtml, false);
            //5. return new file
            return newFilePath;
        }
    }
}
