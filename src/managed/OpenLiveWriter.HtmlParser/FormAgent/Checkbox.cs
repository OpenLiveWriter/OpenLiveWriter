// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class Checkbox : FormElementWithValue
    {
        private bool isChecked;

        public Checkbox(HtmlForm parentForm, string name, string value, bool isChecked) : base(parentForm, name, value)
        {
            this.isChecked = isChecked;
        }

        public bool Checked
        {
            get { return isChecked; }
            set { isChecked = value; }
        }

        public override bool IsSuccessful
        {
            get { return isChecked; }
        }
    }
}
