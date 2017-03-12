// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Globalization;
using System.Text;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    public class UrlToReplace
    {
        public UrlToReplace(string oldUrl, string newUrl)
        {
            _oldUrl = oldUrl;
            _newUrl = newUrl;
        }

        public string newUrl
        {
            get { return _newUrl; }
        }
        private string _newUrl;

        public string oldUrl
        {
            get { return _oldUrl; }
        }
        private string _oldUrl;
    }

    /// <summary>
    /// Summary description for LightWeightHTMLReplacer.
    /// </summary>
    public class LightWeightHTMLReplacer : LightWeightHTMLDocumentIterator
    {
        public LightWeightHTMLReplacer(string html, string url) : this(html, url, null)
        {
        }

        public LightWeightHTMLReplacer(string html, string url, LightWeightHTMLMetaData metaData) : this(html, url, metaData, false)
        {

        }

        public LightWeightHTMLReplacer(string html, string url, LightWeightHTMLMetaData metaData, bool fragmentMode) : base(html)
        {
            _url = url;
            _metaData = metaData;
            if (fragmentMode)
            {
                _firstTag = false;
                _seenHead = true;
                _seenBody = true;
            }
        }

        private LightWeightHTMLMetaData _metaData = null;

        public void AddUrlToReplace(UrlToReplace urlToReplace)
        {
            if (!_urlsToReplace.Contains(urlToReplace))
                _urlsToReplace.Add(urlToReplace);
        }

        public void AddSubstitionUrl(UrlToReplace substitutionUrl)
        {
            if (!_substitutionUrls.ContainsKey(substitutionUrl.newUrl))
                _substitutionUrls[substitutionUrl.newUrl] = substitutionUrl.oldUrl;
        }
        private Hashtable _substitutionUrls = new Hashtable();

        private ArrayList _urlsToReplace = new ArrayList();

        public string DoReplace()
        {
            Parse();
            string html = _htmlBuilder.ToString();
            _htmlBuilder = new StringBuilder();
            return InsertSpecialHeaders(html);
        }

        protected string Url
        {
            get
            {
                return _url;
            }
        }
        private string _url = null;

        protected virtual string InsertSpecialHeaders(string html)
        {
            // For local file urls, preserve the 'saved from' so that the page or snippet stays in the correct sandbox
            // For web based captures, ignore the existing savedFrom and use the current url as the saved from
            HTMLDocumentHelper.SpecialHeaders specialHeaders = HTMLDocumentHelper.GetSpecialHeaders(html, Url);
            if (specialHeaders.SavedFrom == null && _metaData != null && _metaData.SavedFrom != null)
                html = html.Insert(0, _metaData.SavedFrom + "\r\n");
            else if (specialHeaders.SavedFrom == null && Url != null && Url != string.Empty && !UrlHelper.IsFileUrl(Url))
                html = html.Insert(0, UrlHelper.GetSavedFromString(Url) + "\r\n");

            // Insure any doctype declaration is there
            if (_metaData != null && _metaData.DocType != null)
                html = html.Insert(0, _metaData.DocType);

            return html;
        }

        private int _scriptDepth = 0;
        protected override void OnBeginTag(BeginTag tag)
        {
            if (tag == null)
                return;

            if (_firstTag)
            {
                if (!tag.NameEquals(HTMLTokens.Html))
                    EmitTag(HTMLTokens.Html);
                _firstTag = false;
            }

            if (!_seenHead && !TagPermittedAboveBody(tag))
            {
                Emit("<head>");
                EmitAdditionalMetaData();
                Emit("</head>");
                _seenHead = true;
            }

            if (tag.NameEquals(HTMLTokens.Script))
            {
                if (!tag.Complete)
                    _scriptDepth++;
                return;
            }

            if (tag.NameEquals(HTMLTokens.Head))
                _seenHead = true;
            else if (!_seenBody && !tag.NameEquals(HTMLTokens.Body))
            {
                if (!TagPermittedAboveBody(tag))
                {
                    EmitTag(HTMLTokens.Body);
                    _seenBody = true;
                }
            }
            else if (!_seenBody && tag.NameEquals(HTMLTokens.Body))
                _seenBody = true;

            if (tag.NameEquals(HTMLTokens.Base))
            {
                if (_metaData == null || _metaData.Base == null)
                    return;
                else
                {
                    Attr href = tag.GetAttribute(HTMLTokens.Href);
                    if (href != null)
                        href.Value = _metaData.Base;
                }
                _emittedMetaData.Add(HTMLTokens.Base);
            }

            if (tag.NameEquals(HTMLTokens.Meta))
                ModifyMetaDataAsNecessary(tag);

            foreach (Attr attr in tag.Attributes)
                if (attr != null)
                {
                    if (IsScriptAttribute(attr))
                        tag.RemoveAttribute(attr.Name);
                    else
                        attr.Value = ReplaceValue(attr.Value);
                }

            Emit(tag.ToString());
            base.OnBeginTag(tag);
        }
        private bool _firstTag = true;
        private bool _seenHead = false;
        private bool _seenBody = false;

        private ArrayList _emittedTagsToClose = new ArrayList();

        private void EmitTag(string tagName)
        {
            Emit(string.Format(CultureInfo.InvariantCulture, "<{0}>", tagName));
            _emittedTagsToClose.Add(tagName);
        }

        private bool TagPermittedAboveBody(BeginTag tag)
        {
            foreach (string permittedAboveBody in _permittedBeforeBody)
                if (tag.NameEquals(permittedAboveBody))
                    return true;
            return false;
        }

        private void EmitClosingTags()
        {
            ArrayList tagsToEmit = _emittedTagsToClose;
            tagsToEmit.Reverse();
            foreach (string tagName in tagsToEmit)
                EmitCloseTag(tagName);
        }

        private void EmitCloseTag(string tagName)
        {
            Emit(string.Format(CultureInfo.InvariantCulture, "</{0}>", tagName));
        }

        protected override void OnDocumentEnd()
        {
            EmitClosingTags();
            base.OnDocumentEnd();
        }

        protected override void OnScriptLiteral(ScriptLiteral literal)
        {
            if (literal == null || _scriptDepth > 0)
                return;

            literal.LiteralText = ReplaceValue(literal.LiteralText);
            Emit(literal.ToString());
            base.OnScriptLiteral(literal);
        }

        protected override void OnStyleLiteral(StyleLiteral literal)
        {
            if (literal == null || _scriptDepth > 0)
                return;

            literal.LiteralText = ReplaceValue(literal.LiteralText);
            Emit(literal.ToString());
            base.OnStyleLiteral(literal);
        }

        protected override void OnComment(Comment comment)
        {
            if (comment == null || _scriptDepth > 0)
                return;

            Emit(comment.ToString());
            base.OnComment(comment);
        }

        protected override void OnEndTag(EndTag tag)
        {
            if (tag == null)
                return;

            if (tag.NameEquals(HTMLTokens.Script))
            {
                if (!tag.Implicit)
                    if (_scriptDepth > 0)
                        _scriptDepth--;
                return;
            }

            if (tag.NameEquals(HTMLTokens.Head))
                EmitAdditionalMetaData();

            if (tag.NameEquals(HTMLTokens.Html) && _emittedTagsToClose.Contains(HTMLTokens.Body))
            {
                EmitCloseTag(HTMLTokens.Body);
                _emittedTagsToClose.Remove(HTMLTokens.Body);

            }

            Emit(tag.ToString());
            base.OnEndTag(tag);
        }

        protected override void OnMarkupDirective(MarkupDirective markupDirective)
        {
            if (markupDirective == null || _scriptDepth > 0)
                return;

            Emit(markupDirective.ToString());
            base.OnMarkupDirective(markupDirective);
        }

        protected override void OnScriptComment(ScriptComment scriptComment)
        {
            if (scriptComment == null || _scriptDepth > 0)
                return;

            Emit(scriptComment.ToString());
            base.OnScriptComment(scriptComment);
        }

        protected override void OnScriptText(ScriptText scriptText)
        {
            if (scriptText == null || _scriptDepth > 0)
                return;

            Emit(scriptText.ToString());
            base.OnScriptText(scriptText);
        }

        protected override void OnStyleText(StyleText text)
        {
            if (text == null || _scriptDepth > 0)
                return;

            Emit(text.ToString());
            base.OnStyleText(text);
        }

        protected override void OnText(Text text)
        {
            if (text == null || _scriptDepth > 0)
                return;

            Emit(text.ToString());
            base.OnText(text);
        }

        protected override void OnStyleComment(StyleComment styleComment)
        {
            if (styleComment == null || _scriptDepth > 0)
                return;

            Emit(styleComment.ToString());
            base.OnStyleComment(styleComment);
        }

        protected override void OnStyleImport(StyleImport styleImport)
        {
            if (styleImport == null || _scriptDepth > 0)
                return;

            styleImport.LiteralText = ReplaceValue(styleImport.LiteralText);
            Emit(styleImport.ToString());

            base.OnStyleImport(styleImport);
        }

        protected override void OnStyleUrl(StyleUrl styleUrl)
        {
            if (styleUrl == null || _scriptDepth > 0)
                return;

            styleUrl.LiteralText = ReplaceValue(styleUrl.LiteralText);
            Emit(styleUrl.ToString());

            base.OnStyleUrl(styleUrl);
        }

        protected void Emit(string text)
        {
            _htmlBuilder.Append(text);
        }

        private bool CompareUrls(string oldUrl, string newUrl)
        {
            string oldUrlNoAnchor = UrlHelper.GetUrlWithoutAnchorIdentifier(oldUrl);
            string newUrlNoAnchor = UrlHelper.GetUrlWithoutAnchorIdentifier(newUrl);
            return oldUrlNoAnchor == newUrlNoAnchor;
        }

        private string ReplaceUrlPreservingAnchor(string oldUrl, string newUrl)
        {
            string finalUrl = newUrl;
            string oldUrlAnchor = UrlHelper.GetAnchorIdentifier(oldUrl);
            if (oldUrlAnchor != null)
                finalUrl = finalUrl + "#" + oldUrlAnchor;
            return finalUrl;
        }

        protected virtual string ReplaceValue(string value)
        {
            if (value == null)
                return value;

            // Handle any url substitutions
            foreach (UrlToReplace urlToReplace in _urlsToReplace)
            {
                string subUrl = (string)_substitutionUrls[urlToReplace.oldUrl];
                if (subUrl != null && CompareUrls(subUrl, value))
                {
                    value = ReplaceUrlPreservingAnchor(value, urlToReplace.newUrl);
                    break;
                }
            }

            foreach (UrlToReplace urlToReplace in _urlsToReplace)
            {
                if (CompareUrls(urlToReplace.oldUrl, value))
                {
                    value = ReplaceUrlPreservingAnchor(value, urlToReplace.newUrl);
                    break;
                }
            }

            //check substitution Urls
            return value;
        }

        private bool IsScriptAttribute(Attr attribute)
        {
            foreach (string jscriptAttribute in _jscriptAttributes)
                if (attribute.NameEquals(jscriptAttribute))
                    return true;
            return false;
        }
        private static string[] _jscriptAttributes = new string[] { "onload", "onclick", "onblur", "onchange", "onerror", "onfocus", "onmouseout", "onmouseover", "onreset", "onsubmit", "onselect", "onunload", "onmousedown", "onmouseup", "ondblclick", "onmousemove", "onkeypress", "onkeydown", "onkeyup", };

        private void ModifyMetaDataAsNecessary(BeginTag tag)
        {
            if (_metaData == null)
                return;

            Attr nameAttr = tag.GetAttribute(HTMLTokens.Name);
            if (nameAttr == null)
            {
                nameAttr = tag.GetAttribute(HTMLTokens.HttpEquiv);
            }

            if (nameAttr != null)
            {
                switch (nameAttr.Value.ToUpper(CultureInfo.InvariantCulture))
                {
                    case (HTMLTokens.Author):
                        ModifyMetaDataAttribute(tag, HTMLTokens.Content, _metaData.Author);
                        break;
                    case (HTMLTokens.ContentType):
                        ModifyMetaDataAttribute(tag, HTMLTokens.Content, string.Format(CultureInfo.InvariantCulture, "text/html; {0}", _metaData.Charset));
                        break;
                    case (HTMLTokens.Charset):
                        ModifyMetaDataAttribute(tag, HTMLTokens.Content, _metaData.Charset);
                        break;
                    case (HTMLTokens.Description):
                        ModifyMetaDataAttribute(tag, HTMLTokens.Content, _metaData.Description);
                        break;
                    case (HTMLTokens.Generator):
                        ModifyMetaDataAttribute(tag, HTMLTokens.Content, _metaData.Generator);
                        break;
                    case (HTMLTokens.CopyRight):
                        ModifyMetaDataAttribute(tag, HTMLTokens.Content, _metaData.CopyRight);
                        break;
                    case (HTMLTokens.Keywords):
                        ModifyMetaDataAttribute(tag, HTMLTokens.Content, _metaData.KeywordString);
                        break;
                    case (HTMLTokens.Pragma):
                        ModifyMetaDataAttribute(tag, HTMLTokens.Content, _metaData.Pragma);
                        break;
                    case (HTMLTokens.Robots):
                        ModifyMetaDataAttribute(tag, HTMLTokens.Content, _metaData.Robots);
                        break;
                }
                _emittedMetaData.Add(nameAttr.Value.ToUpper(CultureInfo.InvariantCulture));
            }
        }

        private void ModifyMetaDataAttribute(BeginTag tag, string attributeName, string valueToChangeTo)
        {
            Attr valueAttr = tag.GetAttribute(attributeName);
            if (valueAttr != null && valueToChangeTo != null)
                valueAttr.Value = valueToChangeTo;
        }

        private ArrayList _emittedMetaData = new ArrayList();

        private void EmitAdditionalMetaData()
        {
            if (_metaData == null)
                return;

            string contentFormat = "<meta name=\"{0}\" content=\"{1}\">";
            string equivFormat = "<meta http-equiv=\"{0}\" content=\"{1}\">";

            if (_metaData.Base != null && !_emittedMetaData.Contains(HTMLTokens.Base))
                Emit(string.Format(CultureInfo.InvariantCulture, "<base href=\"{0}\">", _metaData.Base));

            if (_metaData.Author != null && !_emittedMetaData.Contains(HTMLTokens.Author))
                Emit(string.Format(CultureInfo.InvariantCulture, contentFormat, HTMLTokens.Author, _metaData.Author));

            if (_metaData.Charset != null && !_emittedMetaData.Contains(HTMLTokens.Charset) && !_emittedMetaData.Contains(HTMLTokens.ContentType))
                Emit(string.Format(CultureInfo.InvariantCulture, equivFormat, HTMLTokens.ContentType, string.Format(CultureInfo.InvariantCulture, "text/html; {0}", _metaData.Charset)));

            if (_metaData.Description != null && !_emittedMetaData.Contains(HTMLTokens.Description))
                Emit(string.Format(CultureInfo.InvariantCulture, contentFormat, HTMLTokens.Description, _metaData.Description));

            if (_metaData.CopyRight != null && !_emittedMetaData.Contains(HTMLTokens.CopyRight))
                Emit(string.Format(CultureInfo.InvariantCulture, contentFormat, HTMLTokens.CopyRight, _metaData.CopyRight));

            if (_metaData.Generator != null && !_emittedMetaData.Contains(HTMLTokens.Generator))
                Emit(string.Format(CultureInfo.InvariantCulture, contentFormat, HTMLTokens.Generator, _metaData.Generator));

            if (_metaData.KeywordString != null && !_emittedMetaData.Contains(HTMLTokens.Keywords))
                Emit(string.Format(CultureInfo.InvariantCulture, contentFormat, HTMLTokens.Keywords, _metaData.KeywordString));

            if (_metaData.Robots != null && !_emittedMetaData.Contains(HTMLTokens.Robots))
                Emit(string.Format(CultureInfo.InvariantCulture, contentFormat, HTMLTokens.Robots, _metaData.Robots));

            if (_metaData.Pragma != null && !_emittedMetaData.Contains(HTMLTokens.Pragma))
                Emit(string.Format(CultureInfo.InvariantCulture, equivFormat, HTMLTokens.Pragma, _metaData.Pragma));

        }

        private StringBuilder _htmlBuilder = new StringBuilder();

        private static string[] _permittedBeforeBody = new string[] { HTMLTokens.Html, HTMLTokens.Head, HTMLTokens.Title, HTMLTokens.Script, HTMLTokens.Style, HTMLTokens.Meta, HTMLTokens.Link, HTMLTokens.Object, HTMLTokens.Base, HTMLTokens.Frame, HTMLTokens.FrameSet, HTMLTokens.NoScript };
    }

}
