// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using CoreServices.HTML;
    using HtmlEditor.Marshalling.Data_Handlers;

    /// <summary>
    /// Fixes up image URLs that are being pasted into the Shared Canvas.
    /// </summary>
    public class SharedCanvasImageReferenceFixer : IImageReferenceFixer
    {
        private ReferenceFixer _internalReferenceFixer;

        /// <summary>
        /// Initializes a new instance of the SharedCanvasImageReferenceFixer class.
        /// </summary>
        /// <param name="internalReferenceFixer">A delegate that fixes up image URLs that originated from the Shared Canvas itself.</param>
        public SharedCanvasImageReferenceFixer(ReferenceFixer internalReferenceFixer)
        {
            if (internalReferenceFixer == null)
            {
                throw new ArgumentNullException("internalReferenceFixer");
            }

            _internalReferenceFixer = internalReferenceFixer;
        }

        /// <summary>
        /// Iterates through the provided HTML and fixes up image URLs.
        /// </summary>
        /// <param name="html">The HTML to iterate through.</param>
        /// <param name="sourceUrl">The source URL that the HTML originated from.</param>
        /// <returns>The fixed up HTML.</returns>
        public string FixImageReferences(string html, string sourceUrl)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }

            if (sourceUrl == null)
            {
                throw new ArgumentNullException("sourceUrl");
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                if (HtmlHandler.IsSharedCanvasTempUrl(sourceUrl))
                {
                    HtmlReferenceFixer fixer = new HtmlReferenceFixer(html);
                    fixer.FixReferences(writer, _internalReferenceFixer, null);
                }
                else
                {
                    return html;
                }
            }

            return sb.ToString();
        }
    }
}
