// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class Textbox : FormElementWithValue
    {
        public Textbox(HtmlForm parentForm, string name, string value) : base(parentForm, name, value)
        {
            if (value == null)
                this.Value = "";
        }

        public override bool IsSuccessful
        {
            get { return true; }
        }

        public override void AddData(FormData data)
        {
            if (Value == null)
                data.Add(Name, string.Empty);
            else
                data.Add(Name, Value);
        }

    }
}
