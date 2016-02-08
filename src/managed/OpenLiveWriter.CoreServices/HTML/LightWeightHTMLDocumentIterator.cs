// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    public abstract class LightWeightHTMLDocumentIterator
    {
        public LightWeightHTMLDocumentIterator(string html)
        {
            _html = html;
        }

        protected virtual string Html
        {
            get
            {
                return _html;
            }
        }
        private string _html;

        public void Parse()
        {
            SimpleHtmlParser parser = new SimpleHtmlParser(_html);

            OnDocumentBegin();
            while (true)
            {
                Element currentElement = parser.Next();

                BeginTag beginTag = currentElement as BeginTag;
                if (beginTag != null)
                {
                    OnBeginTag(beginTag);
                    continue;
                }

                EndTag endTag = currentElement as EndTag;
                if (endTag != null)
                {
                    OnEndTag(endTag);
                    continue;
                }

                ScriptLiteral literal = currentElement as ScriptLiteral;
                if (literal != null)
                {
                    OnScriptLiteral(literal);
                    continue;
                }

                Comment comment = currentElement as Comment;
                if (comment != null)
                {
                    OnComment(comment);
                    continue;
                }

                MarkupDirective markupDirective = currentElement as MarkupDirective;
                if (markupDirective != null)
                {
                    OnMarkupDirective(markupDirective);
                    continue;
                }

                ScriptText scriptText = currentElement as ScriptText;
                if (scriptText != null)
                {
                    OnScriptText(scriptText);
                    continue;
                }

                ScriptComment scriptComment = currentElement as ScriptComment;
                if (scriptComment != null)
                {
                    OnScriptComment(scriptComment);
                    continue;
                }

                StyleText styleText = currentElement as StyleText;
                if (styleText != null)
                {
                    OnStyleText(styleText);
                    continue;
                }

                StyleUrl styleUrl = currentElement as StyleUrl;
                if (styleUrl != null)
                {
                    OnStyleUrl(styleUrl);
                    continue;
                }

                StyleImport styleImport = currentElement as StyleImport;
                if (styleImport != null)
                {
                    OnStyleImport(styleImport);
                    continue;
                }

                StyleComment styleComment = currentElement as StyleComment;
                if (styleComment != null)
                {
                    OnStyleComment(styleComment);
                    continue;
                }

                StyleLiteral styleLiteral = currentElement as StyleLiteral;
                if (styleLiteral != null)
                {
                    OnStyleLiteral(styleLiteral);
                    continue;
                }

                Text text = currentElement as Text;
                if (text != null)
                {
                    OnText(text);
                    continue;
                }

                if (currentElement == null)
                {
                    OnDocumentEnd();
                    return;
                }

                Debug.Fail("Unrecognized element in LightWeightHTMLDocumentIterator");
            }
        }

        protected virtual void OnDocumentBegin()
        { }

        protected virtual void OnDocumentEnd()
        { }

        protected virtual void DefaultAction(Element el)
        {
        }

        protected virtual void OnStyleImport(StyleImport styleImport)
        {
            DefaultAction(styleImport);
        }

        protected virtual void OnStyleText(StyleText text)
        {
            DefaultAction(text);
        }

        protected virtual void OnStyleLiteral(StyleLiteral literal)
        {
            DefaultAction(literal);
        }

        protected virtual void OnBeginTag(BeginTag tag)
        {
            DefaultAction(tag);
        }

        protected virtual void OnEndTag(EndTag tag)
        {
            DefaultAction(tag);
        }

        protected virtual void OnScriptLiteral(ScriptLiteral literal)
        {
            DefaultAction(literal);
        }

        protected virtual void OnComment(Comment comment)
        {
            DefaultAction(comment);
        }

        protected virtual void OnMarkupDirective(MarkupDirective markupDirective)
        {
            DefaultAction(markupDirective);
        }

        protected virtual void OnScriptText(ScriptText scriptText)
        {
            DefaultAction(scriptText);
        }

        protected virtual void OnScriptComment(ScriptComment scriptComment)
        {
            DefaultAction(scriptComment);
        }

        protected virtual void OnText(Text text)
        {
            DefaultAction(text);
        }

        protected virtual void OnStyleUrl(StyleUrl styleUrl)
        {
            DefaultAction(styleUrl);
        }

        protected virtual void OnStyleComment(StyleComment styleComment)
        {
            DefaultAction(styleComment);
        }
    }

}
