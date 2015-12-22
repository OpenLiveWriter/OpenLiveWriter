// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.HTML
{
    using System;
    using mshtml;

    /// <summary>
    /// Represents the default block element to use during editing. This element is automatically inserted by MSHTML
    /// when the user hits Enter. However, we may have to fix up plain-text or other HTML and this class provides
    /// convenient access to certain properties related to the default block element. By default, MSHTML uses the
    /// paragraph element as the default block element unless the DOCHOSTUIFLAG_DIV_BLOCKDEFAULT flag is set, in which
    /// case it uses the div element as the default block element.
    /// </summary>
    public abstract class DefaultBlockElement
    {
        /// <summary>
        /// Initializes a new instance of the DefaultBlockElement class.
        /// </summary>
        /// <param name="tagId">The tag id associated with the default block element.</param>
        /// <param name="tagName">The tag name associated with the default block element.</param>
        /// <param name="numberOfNewLinesToReplace">The number of plain-text CRLFs that should be replaced when
        /// inserting the default block element. This is used when converting plain-text to HTML in order to maintain
        /// similar rendering between plain-text and  HTML. Must be greater than zero.</param>
        protected DefaultBlockElement(_ELEMENT_TAG_ID tagId, string tagName, int numberOfNewLinesToReplace)
        {
            if (!Enum.IsDefined(typeof(_ELEMENT_TAG_ID), tagId))
            {
                throw new ArgumentException("tagId");
            }

            if (String.IsNullOrEmpty(tagName))
            {
                throw new ArgumentException("tagName");
            }

            if (numberOfNewLinesToReplace < 1)
            {
                throw new ArgumentOutOfRangeException("numberOfNewLinesToReplace");
            }

            this.BeginTag = String.Format("<{0}>", tagName);
            this.EndTag = String.Format("</{0}>", tagName);
            this.TagId = tagId;
            this.TagName = tagName;
            this.NumberOfNewLinesToReplace = numberOfNewLinesToReplace;
        }

        /// <summary>
        /// Gets the HTML needed to begin the block element.
        /// </summary>
        public string BeginTag
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the HTML needed to end the block element.
        /// </summary>
        public string EndTag
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the tag id associated with the default block element.
        /// </summary>
        public _ELEMENT_TAG_ID TagId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the tag name associated with the default block element.
        /// </summary>
        public string TagName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of plain-text CRLFs that should be replaced when inserting the default block element. This
        /// is used when converting plain-text to HTML in order to maintain similar rendering between plain-text and
        /// HTML.
        /// </summary>
        public int NumberOfNewLinesToReplace
        {
            get;
            private set;
        }
    }
}
