// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Text;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for LightWeightCSSReplacer.
    /// </summary>
    public class LightWeightCSSReplacer : LightweightCSSIterator
    {
        public LightWeightCSSReplacer(string css) : base(css)
        {
        }

        public void AddUrlToReplace(UrlToReplace urlToReplace)
        {
            if (!_urlsToReplace.Contains(urlToReplace))
                _urlsToReplace.Add(urlToReplace);
        }
        private ArrayList _urlsToReplace = new ArrayList();

        public string DoReplace()
        {
            Parse();
            return _cssBuilder.ToString();
        }
        private StringBuilder _cssBuilder = new StringBuilder();

        protected override void OnStyleComment(StyleComment styleComment)
        {
            Emit(styleComment);
            base.OnStyleComment(styleComment);
        }

        protected override void OnStyleLiteral(StyleLiteral styleLiteral)
        {
            Emit(styleLiteral);
            base.OnStyleLiteral(styleLiteral);
        }

        protected override void OnStyleText(StyleText styleText)
        {
            Emit(styleText);
            base.OnStyleText(styleText);
        }

        protected override void OnStyleImport(StyleImport styleImport)
        {
            if (styleImport != null)
                styleImport.LiteralText = ReplaceUrlAsNecessary(styleImport.LiteralText);
            Emit(styleImport);
            base.OnStyleImport(styleImport);
        }

        protected override void OnStyleUrl(StyleUrl styleUrl)
        {
            if (styleUrl != null)
                styleUrl.LiteralText = ReplaceUrlAsNecessary(styleUrl.LiteralText);
            Emit(styleUrl);
            base.OnStyleUrl(styleUrl);
        }

        private string ReplaceUrlAsNecessary(string currentUrl)
        {
            foreach (UrlToReplace urlToReplace in _urlsToReplace)
                if (urlToReplace.oldUrl == currentUrl)
                    return urlToReplace.newUrl;
            return currentUrl;
        }

        private void Emit(StyleElement styleElement)
        {
            if (styleElement != null)
                _cssBuilder.Append(styleElement.ToString());
        }

    }
}
