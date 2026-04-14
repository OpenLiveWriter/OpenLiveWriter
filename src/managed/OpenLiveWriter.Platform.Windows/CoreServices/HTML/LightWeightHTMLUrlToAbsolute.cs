// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for LightWeightHTMLUrlToAbsolute.
    /// </summary>
    public class LightWeightHTMLUrlToAbsolute : LightWeightHTMLReplacer
    {
        public static string ConvertToAbsolute(string html, string url)
        {
            return ConvertToAbsolute(html, url, true);
        }

        public static string ConvertToAbsolute(string html, string url, bool fixupSpecialHeaders)
        {
            return ConvertToAbsolute(html, url, fixupSpecialHeaders, true);
        }

        public static string ConvertToAbsolute(string html, string url, bool fixupSpecialHeaders, bool escapeEmptyString)
        {
            return ConvertToAbsolute(html, url, fixupSpecialHeaders, escapeEmptyString, false);
        }

        public static string ConvertToAbsolute(string html, string url, bool fixupSpecialHeaders, bool escapeEmptyString, bool fragmentMode)
        {
            LightWeightHTMLUrlToAbsolute urlConverter = new LightWeightHTMLUrlToAbsolute(html, url, fixupSpecialHeaders, escapeEmptyString, fragmentMode);
            return urlConverter.DoReplace();
        }

        private LightWeightHTMLUrlToAbsolute(string html, string url, bool fixupSpecialHeaders, bool escapeEmptyString, bool fragmentMode) : base(html, url, null, fragmentMode)
        {
            _fixupSpecialHeaders = fixupSpecialHeaders;
            _escapeEmptyString = escapeEmptyString;
        }

        protected override string InsertSpecialHeaders(string html)
        {
            if (_fixupSpecialHeaders)
                return base.InsertSpecialHeaders(html);
            else
                return html;
        }
        private bool _fixupSpecialHeaders;
        private bool _escapeEmptyString;

        private string BaseUrl
        {
            get
            {
                if (_baseUrl == null)
                {
                    LightWeightHTMLDocument document = LightWeightHTMLDocument.FromString(Html, Url, false);
                    document.Parse();
                    LightWeightTag[] baseTags = document.GetTagsByName(HTMLTokens.Base);
                    foreach (LightWeightTag baseTag in baseTags)
                    {
                        Attr attr = baseTag.BeginTag.GetAttribute(HTMLTokens.Href);
                        if (attr != null)
                        {
                            _baseUrl = attr.Value;
                            break;
                        }
                    }

                    if (_baseUrl == null)
                        _baseUrl = Url;
                }
                return _baseUrl;
            }
        }
        private string _baseUrl;

        protected override void OnBeginTag(BeginTag tag)
        {
            if (tag != null && LightWeightHTMLDocument.AllUrlElements.ContainsKey(tag.Name.ToUpper(CultureInfo.InvariantCulture)))
            {
                Attr attr = tag.GetAttribute((string)LightWeightHTMLDocument.AllUrlElements[tag.Name.ToUpper(CultureInfo.InvariantCulture)]);
                if (attr != null)
                {
                    string url = attr.Value;
                    if (!UrlHelper.IsUrl(url) && ShouldEscapeRelativeUrl(url))
                        attr.Value = UrlHelper.EscapeRelativeURL(BaseUrl, url);
                }
            }

            // Special case params
            if (tag != null && tag.NameEquals(HTMLTokens.Param))
            {
                // Handle Params
                foreach (string paramValue in LightWeightHTMLDocument.ParamsUrlElements)
                {
                    Attr attr = tag.GetAttribute(HTMLTokens.Name);
                    if (attr != null)
                    {
                        if (attr.Value.ToUpper(CultureInfo.InvariantCulture) == paramValue)
                        {
                            Attr valueAttr = tag.GetAttribute(HTMLTokens.Value);
                            if (valueAttr != null)
                            {
                                string url = valueAttr.Value;
                                if (!UrlHelper.IsUrl(url))
                                    valueAttr.Value = UrlHelper.EscapeRelativeURL(BaseUrl, url);
                            }
                        }
                    }
                }
            }
            base.OnBeginTag(tag);
        }

        private bool ShouldEscapeRelativeUrl(string url)
        {
            if ((url == String.Empty) && !_escapeEmptyString)
                return false;
            else
                return true;
        }

        protected override void OnStyleUrl(StyleUrl styleUrl)
        {
            if (styleUrl != null)
            {
                string url = styleUrl.LiteralText;
                if (!UrlHelper.IsUrl(url))
                    styleUrl.LiteralText = UrlHelper.EscapeRelativeURL(BaseUrl, url);
            }
            base.OnStyleUrl(styleUrl);
        }

        protected override void OnStyleImport(StyleImport styleImport)
        {
            if (styleImport != null)
            {
                string url = styleImport.LiteralText;
                if (!UrlHelper.IsUrl(url))
                    styleImport.LiteralText = UrlHelper.EscapeRelativeURL(BaseUrl, url);
            }
            base.OnStyleImport(styleImport);
        }

    }
}
