// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class Hidden : FormElementWithValue
    {
        public Hidden(HtmlForm parentForm, string name, string value) : base(parentForm, name, value)
        {
        }

        public override bool IsSuccessful
        {
            get { return true; }
        }
    }
}
