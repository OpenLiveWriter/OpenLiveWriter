// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public abstract class FormElementWithValue : FormElement
    {
        private string value;

        public FormElementWithValue(HtmlForm parentForm, string name, string value) : base(parentForm, name)
        {
            this.value = value;
        }

        public override void AddData(FormData data)
        {
            if (Name != null)
                data.Add(Name, Value);
        }

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }
}
