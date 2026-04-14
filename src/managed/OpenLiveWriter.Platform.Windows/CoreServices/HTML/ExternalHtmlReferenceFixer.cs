// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.HTML
{
    using System.IO;
    using HtmlParser.Parser;

    /// <summary>
    /// Inspects an HTML tag and reference and returns a new reference.
    /// </summary>
    /// <param name="tag">The BeginTag that this reference was defined on.</param>
    /// <param name="reference">The current value of the reference.</param>
    /// <param name="sourceUrl">The source URL that the HTML originated from.</param>
    /// <returns>The new reference.</returns>
    public delegate string ExternalReferenceFixer(BeginTag tag, string reference, string sourceUrl);

    /// <summary>
    /// Iterates through HTML that originated externally and fixes up references.
    /// </summary>
    public class ExternalHtmlReferenceFixer
    {
        private HtmlReferenceFixer _htmlReferenceFixer;
        private string _sourceUrl;

        /// <summary>
        /// Initializes a new instance of the ExternalHtmlReferenceFixer class.
        /// </summary>
        /// <param name="html">The HTML to iterate through.</param>
        /// <param name="sourceUrl">The source URL that the HTML originated from.</param>
        public ExternalHtmlReferenceFixer(string html, string sourceUrl)
        {
            _htmlReferenceFixer = new HtmlReferenceFixer(html);
            _sourceUrl = sourceUrl;
        }

        /// <summary>
        /// Iterates through the provided HTML and fixes up references.
        /// </summary>
        /// <param name="output">The TextWriter to write the output to.</param>
        /// <param name="externalReferenceFixer">A delegate that fixes up the references.</param>
        /// <param name="referenceFixed">A callback after the reference is fixed.</param>
        public void FixReferences(TextWriter output, ExternalReferenceFixer externalReferenceFixer, ReferenceFixedCallback referenceFixed)
        {
            var referenceFixer = new ReferenceFixer(
                delegate (BeginTag beginTag, string reference)
                {
                    return externalReferenceFixer(beginTag, reference, _sourceUrl);
                });

            _htmlReferenceFixer.FixReferences(output, referenceFixer, referenceFixed);
        }
    }
}
