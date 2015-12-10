// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class IncompleteListFinder : LightWeightHTMLDocumentIterator
    {
        private bool hasIncompleteList = false;
        private int unorderedListLevel = 0;
        private int orderedListLevel = 0;

        public IncompleteListFinder(string html)
            : base(html)
        {
        }

        protected override void OnBeginTag(BeginTag tag)
        {
            if (tag.NameEquals(HTMLTokens.Ul))
            {
                unorderedListLevel++;
            }
            else if (tag.NameEquals(HTMLTokens.Ol))
            {
                orderedListLevel++;
            }
            else if ((unorderedListLevel < 1) &&
                (orderedListLevel < 1) &&
                (tag.NameEquals(HTMLTokens.Li)))
            {
                hasIncompleteList = true;
            }

            base.OnBeginTag(tag);
        }

        protected override void OnEndTag(EndTag tag)
        {
            if (tag.NameEquals(HTMLTokens.Ul))
            {
                unorderedListLevel--;
            }
            else if (tag.NameEquals(HTMLTokens.Ol))
            {
                orderedListLevel--;
            }

            base.OnEndTag(tag);
        }

        public bool HasIncompleteList
        {
            get
            {
                return hasIncompleteList;
            }
        }
    }
}
