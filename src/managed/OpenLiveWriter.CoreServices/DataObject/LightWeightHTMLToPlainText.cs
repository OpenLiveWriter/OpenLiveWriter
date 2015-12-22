// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for LightWeightHTMLToPlainText.
    /// </summary>
    [Obsolete("Does not preserve whitespace or escape HTML entities. Use HTMLDocumentHelper.HTMLToPlainText() instead.")]
    public class LightWeightHTMLToPlainText : LightWeightHTMLReplacer
    {
        public static string ToPlainText(string html)
        {
            LightWeightHTMLToPlainText toText = new LightWeightHTMLToPlainText(html, null);
            return toText.DoReplace();
        }

        private LightWeightHTMLToPlainText(string html, string url) : base(html, url)
        {
        }

        protected override void OnText(Text text)
        {
            Emit(text.ToString());
        }

        protected override void OnBeginTag(BeginTag tag)
        {
        }

        protected override void OnComment(Comment comment)
        {

        }

        protected override void OnEndTag(EndTag tag)
        {

        }

        protected override void OnMarkupDirective(MarkupDirective markupDirective)
        {

        }

        protected override void OnScriptComment(ScriptComment scriptComment)
        {

        }

        protected override void OnScriptLiteral(ScriptLiteral literal)
        {

        }

        protected override void OnScriptText(ScriptText scriptText)
        {

        }

        protected override void OnStyleImport(StyleImport styleImport)
        {

        }

        protected override void OnStyleLiteral(StyleLiteral literal)
        {

        }

        protected override void OnStyleText(StyleText text)
        {

        }

        protected override void OnStyleUrl(StyleUrl styleUrl)
        {

        }
    }
}
