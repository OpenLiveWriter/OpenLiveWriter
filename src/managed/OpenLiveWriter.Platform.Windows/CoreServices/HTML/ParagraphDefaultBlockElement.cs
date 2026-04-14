// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.HTML
{
    using mshtml;

    /// <summary>
    /// Provides convenient access to certain properties related to the default block element.
    /// </summary>
    public class ParagraphDefaultBlockElement : DefaultBlockElement
    {
        /// <summary>
        /// Initializes a new instance of the ParagraphDefaultBlockElement class.
        /// </summary>
        public ParagraphDefaultBlockElement()
            : base(_ELEMENT_TAG_ID.TAGID_P, "p", 2)
        {
        }
    }
}
