// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for LightweightCSSIterator.
    /// </summary>
    public class LightweightCSSIterator
    {
        public LightweightCSSIterator(string css)
        {
            _css = css;
        }
        private string _css = null;

        public void Parse()
        {
            CssParser parser = new CssParser(_css);
            OnDocumentBegin();
            while (true)
            {
                StyleElement element = parser.Next();

                if (element == null)
                {
                    OnDocumentEnd();
                    return;
                }

                StyleText styleText = element as StyleText;
                if (styleText != null)
                    OnStyleText(styleText);

                StyleLiteral styleLiteral = element as StyleLiteral;
                if (styleLiteral != null)
                    OnStyleLiteral(styleLiteral);

                StyleUrl styleUrl = element as StyleUrl;
                if (styleUrl != null)
                    OnStyleUrl(styleUrl);

                StyleImport styleImport = element as StyleImport;
                if (styleImport != null)
                    OnStyleImport(styleImport);

                StyleComment styleComment = element as StyleComment;
                if (styleComment != null)
                    OnStyleComment(styleComment);
            }
        }

        protected virtual void OnDocumentBegin()
        { }

        protected virtual void OnDocumentEnd()
        { }

        protected virtual void OnStyleUrl(StyleUrl styleUrl)
        { }

        protected virtual void OnStyleLiteral(StyleLiteral styleLiteral)
        { }

        protected virtual void OnStyleImport(StyleImport styleImport)
        { }

        protected virtual void OnStyleComment(StyleComment styleComment)
        { }

        protected virtual void OnStyleText(StyleText styleText)
        { }

    }
}
