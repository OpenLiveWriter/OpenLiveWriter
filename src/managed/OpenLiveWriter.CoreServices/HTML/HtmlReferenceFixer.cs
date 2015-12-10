// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices.HTML
{

    public delegate string ReferenceFixer(BeginTag tag, string reference);
    public delegate void ReferenceFixedCallback(string oldReference, string newReference);

    public class HtmlReferenceFixer : LightWeightHTMLDocumentIterator
    {
        public HtmlReferenceFixer(string html)
            : base(html)
        {
        }

        public void FixReferences(TextWriter output, ReferenceFixer referenceFixer, ReferenceFixedCallback referenceFixed)
        {
            _output = output;
            _referenceFixer = referenceFixer;
            _referenceFixed = referenceFixed;
            Parse();
        }

        public static string FixReferences(string html, ReferenceFixer fixer)
        {
            return FixReferences(html, fixer, null);
        }
        public static string FixReferences(string html, ReferenceFixer fixer, ReferenceFixedCallback referenceFixed)
        {
            TextWriter htmlWriter = new StringWriter(CultureInfo.InvariantCulture);
            HtmlReferenceFixer referenceFixer = new HtmlReferenceFixer(html);
            referenceFixer.FixReferences(htmlWriter, fixer, referenceFixed);
            return htmlWriter.ToString();
        }

        public static string FixLocalFileReferences(string html, ReferenceFixer fixer)
        {
            return FixLocalFileReferences(html, fixer, null);
        }

        public static string FixLocalFileReferences(string html, ReferenceFixer fixer, ReferenceFixedCallback referenceFixed)
        {
            return FixReferences(html, new ReferenceFixer(new LocalFileReferenceFixupFilter(fixer).FixReferences), referenceFixed);
        }

        protected override void DefaultAction(Element el)
        {
            _output.Write(el.ToString());
        }

        protected override void OnBeginTag(BeginTag tag)
        {
            if (tag != null && LightWeightHTMLDocument.AllUrlElements.ContainsKey(tag.Name.ToUpper(CultureInfo.InvariantCulture)))
            {
                Attr attr = tag.GetAttribute((string)LightWeightHTMLDocument.AllUrlElements[tag.Name.ToUpper(CultureInfo.InvariantCulture)]);
                if (attr != null)
                {
                    string oldRef = attr.Value;
                    string newRef = _referenceFixer(tag, attr.Value);
                    attr.Value = newRef;
                    if (oldRef != newRef && _referenceFixed != null)
                    {
                        //notify the reference fixed callback that a reference was fixed.
                        _referenceFixed(oldRef, newRef);
                    }
                }
            }
            base.OnBeginTag(tag);
        }

        private TextWriter _output;
        private ReferenceFixer _referenceFixer;
        private ReferenceFixedCallback _referenceFixed;

        private class LocalFileReferenceFixupFilter
        {
            public LocalFileReferenceFixupFilter(ReferenceFixer fixer)
            {
                _fixer = fixer;
            }

            public string FixReferences(BeginTag tag, string reference)
            {
                // protect against unexpected/empty input
                if (!UrlHelper.IsUrl(reference))
                    return reference;

                Uri referenceUri = new Uri(reference);
                if (referenceUri.IsFile)
                    return _fixer(tag, reference);
                else
                    return reference;
            }

            private ReferenceFixer _fixer;
        }
    }
}
