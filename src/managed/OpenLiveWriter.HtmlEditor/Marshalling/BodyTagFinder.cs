// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class BodyTagFinder : LightWeightHTMLDocumentIterator
    {
        private BeginTag bodyBeginTag;

        public BodyTagFinder(string html)
            : base(html)
        {
        }

        protected override void OnBeginTag(BeginTag tag)
        {
            if (tag.NameEquals(HTMLTokens.Body))
            {
                bodyBeginTag = tag;
            }

            base.OnBeginTag(tag);
        }

        public BeginTag BodyBeginTag
        {
            get
            {
                return bodyBeginTag;
            }
        }
    }
}
