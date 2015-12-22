// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for LightweightHTMLThinner.
    /// </summary>
    public class LightWeightHTMLThinner : LightWeightHTMLReplacer
    {
        [Flags]
        public enum ThinnerBehavior
        {
            ThinForPage = 1,
            ThinForSnippet = 2,
            PreserveImages = 4
        }

        public static string ThinHTML(string html)
        {
            return ThinHTML(html, ThinnerBehavior.ThinForPage);
        }

        public static string ThinHTML(string html, string url)
        {
            return ThinHTML(html, url, ThinnerBehavior.ThinForPage);
        }

        public static string ThinHTML(string html, ThinnerBehavior behavior)
        {
            return ThinHTML(html, null, behavior);
        }

        public static string ThinHTML(string html, string url, ThinnerBehavior behavior)
        {
            LightWeightHTMLThinner thinner = new LightWeightHTMLThinner(html, url, behavior);
            return thinner.DoReplace();
        }

        private LightWeightHTMLThinner(string html, string url, ThinnerBehavior behavior) : base(html, url)
        {
            _behavior = behavior;
        }
        private ThinnerBehavior _behavior;

        private ArrayList TagsToPreserve
        {
            get
            {
                if (_tagsToPreserve == null)
                {
                    if ((_behavior & ThinnerBehavior.ThinForPage) == ThinnerBehavior.ThinForPage)
                        _tagsToPreserve = (ArrayList)PreserveTagsForPage.Clone();
                    else
                        _tagsToPreserve = (ArrayList)PreserveTagsForSnippets.Clone();

                    if ((_behavior & ThinnerBehavior.PreserveImages) == ThinnerBehavior.PreserveImages)
                        _tagsToPreserve = IncludeImages(_tagsToPreserve);
                }
                return _tagsToPreserve;
            }
        }
        private ArrayList _tagsToPreserve = null;

        protected override void OnBeginTag(BeginTag tag)
        {
            if (tag.NameEquals(HTMLTokens.Title) && !tag.Complete)
                _inTitle = true;

            if (TagsToPreserve.Contains(tag.Name.ToUpper(CultureInfo.InvariantCulture)))
            {
                EmitTagAndAttributes(tag.Name, tag);
            }
            else if (ReplaceTags.ContainsKey(tag.Name.ToUpper(CultureInfo.InvariantCulture)))
            {
                EmitTagAndAttributes((string)ReplaceTags[tag.Name.ToUpper(CultureInfo.InvariantCulture)], tag);
            }
        }

        private bool _inTitle = false;

        protected override string InsertSpecialHeaders(string html)
        {
            return html;
        }

        private void EmitTagAndAttributes(string tagName, Tag tag)
        {
            BeginTag beginTag = tag as BeginTag;
            if (beginTag != null)
            {
                Emit(string.Format(CultureInfo.InvariantCulture, "<{0}", tagName));
                foreach (Attr attr in beginTag.Attributes)
                    if (PreserveAttributes.Contains(attr.Name.ToUpper(CultureInfo.InvariantCulture)))
                        Emit(" " + attr.ToString());
                if (beginTag.Complete)
                    Emit("/");
                Emit(">");
            }
            else
            {
                if (tagName.ToUpper(CultureInfo.InvariantCulture) != HTMLTokens.Br && tagName.ToUpper(CultureInfo.InvariantCulture) != HTMLTokens.Img)
                    Emit(string.Format(CultureInfo.InvariantCulture, "</{0}>", tagName));
            }
        }

        protected override void OnEndTag(EndTag tag)
        {
            if (tag.Implicit)
                return;
            if (tag.NameEquals(HTMLTokens.Title))
                _inTitle = false;

            if (TagsToPreserve.Contains(tag.Name.ToUpper(CultureInfo.InvariantCulture)))
                EmitTagAndAttributes(tag.Name, tag);
            else if (ReplaceTags.ContainsKey(tag.Name.ToUpper(CultureInfo.InvariantCulture)))
            {
                EmitTagAndAttributes((string)ReplaceTags[tag.Name.ToUpper(CultureInfo.InvariantCulture)], tag);
            }
        }

        protected override void OnComment(Comment comment)
        {
            base.OnComment (comment);
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

        protected override void OnStyleLiteral(StyleLiteral literal)
        {
        }

        protected override void OnStyleText(StyleText text)
        {
        }

        protected override void OnStyleUrl(StyleUrl styleUrl)
        {
        }

        protected override void OnDocumentBegin()
        {
        }

        protected override void OnDocumentEnd()
        {
        }

        protected override void OnText(Text text)
        {
            if (_inTitle)
                return;

            string textToEmit = text.RawText;
            textToEmit = textToEmit.Replace("\n", " ");
            textToEmit = textToEmit.Replace("\r", " ");
            textToEmit = textToEmit.Replace("\r\n", " ");
            Emit(textToEmit);
        }

        protected override void OnStyleImport(StyleImport styleImport)
        {

        }

        /// <summary>
        /// A table of tags and their replacements when thinning
        /// </summary>
        private static Hashtable ReplaceTags
        {
            get
            {
                if (m_replaceTags == null)
                {
                    m_replaceTags = new Hashtable();
                    m_replaceTags.Add(HTMLTokens.H1, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H2, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H3, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H4, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H5, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.H6, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Hr, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Li, HTMLTokens.Br);
                    m_replaceTags.Add(HTMLTokens.Dt, HTMLTokens.Br);
                    m_replaceTags.Add(HTMLTokens.Dd, HTMLTokens.Br);
                    m_replaceTags.Add(HTMLTokens.Menu, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Ul, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Ol, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Dir, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Dl, HTMLTokens.P);
                    m_replaceTags.Add(HTMLTokens.Blockquote, HTMLTokens.P);

                }
                return m_replaceTags;
            }
        }
        private static Hashtable m_replaceTags = null;

        /// <summary>
        /// The list of attributes that should be preserved
        /// </summary>
        private static ArrayList PreserveAttributes
        {
            get
            {
                if (m_preserveAttributes == null)
                {
                    m_preserveAttributes = new ArrayList(
                        new string[]{HTMLTokens.Href,HTMLTokens.Name,HTMLTokens.Value,HTMLTokens.Action,HTMLTokens.Method,
                                        HTMLTokens.Enctype,HTMLTokens.Size,HTMLTokens.Type,HTMLTokens.Src, HTMLTokens.Content, HTMLTokens.HttpEquiv,
                                        HTMLTokens.Height, HTMLTokens.Width, HTMLTokens.Alt});
                }
                return m_preserveAttributes;
            }
        }
        private static ArrayList m_preserveAttributes = null;

        /// <summary>
        /// The list of tags that should be preserved by the thinner, including images
        /// </summary>
        private ArrayList IncludeImages(ArrayList tagList)
        {
            tagList.Add(HTMLTokens.Img);
            return tagList;
        }

        private static ArrayList PreserveTagsForPage
        {
            get
            {
                if (_preserverTagsForPage == null)
                {
                    _preserverTagsForPage = new ArrayList();
                    foreach (string s in PreserveTagsForSnippets)
                        _preserverTagsForPage.Add(s);
                    _preserverTagsForPage.Add(HTMLTokens.Meta);
                    _preserverTagsForPage.Add(HTMLTokens.Head);
                    _preserverTagsForPage.Add(HTMLTokens.Body);
                }
                return _preserverTagsForPage;
            }
        }
        private static ArrayList _preserverTagsForPage;

        /// <summary>
        /// The list of tags that will be preserved when the HTML is thinned.
        /// </summary>
        private static ArrayList PreserveTagsForSnippets
        {
            get
            {
                if (m_preserveTags == null)
                {
                    m_preserveTags = new ArrayList();
                    m_preserveTags.Add(HTMLTokens.A);
                    m_preserveTags.Add(HTMLTokens.P);
                    m_preserveTags.Add(HTMLTokens.Br);
                    m_preserveTags.Add(HTMLTokens.Form);
                    m_preserveTags.Add(HTMLTokens.Input);
                    m_preserveTags.Add(HTMLTokens.Select);
                    m_preserveTags.Add(HTMLTokens.Option);
                    m_preserveTags.Add(HTMLTokens.Ilayer);
                    m_preserveTags.Add(HTMLTokens.Div);
                    m_preserveTags.Add(HTMLTokens.IFrame);
                    m_preserveTags.Add(HTMLTokens.Pre);
                }
                return m_preserveTags;
            }
        }
        private static ArrayList m_preserveTags = null;
    }
}
