// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class SubmitButton : FormElementWithValue
    {
        public SubmitButton(HtmlForm parentForm, string name, string value) : base(parentForm, name, value)
        {
        }

        public override bool IsSuccessful
        {
            get { return false; }
        }

        public override void AddData(FormData data)
        {
            if (Value != null)
                base.AddData(data);
        }

        public FormData Click()
        {
            return ParentForm.Submit(this);
        }
    }
}
