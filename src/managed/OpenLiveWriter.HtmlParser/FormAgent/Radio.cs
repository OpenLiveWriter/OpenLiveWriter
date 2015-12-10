// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class Radio : FormElementWithValue
    {
        private bool isChecked;

        public Radio(HtmlForm parentForm, string name, string value, bool isChecked) : base(parentForm, name, value)
        {
            this.isChecked = isChecked;
        }

        public bool Checked
        {
            get { return isChecked; }
        }

        public void Select()
        {
            if (isChecked)
                return;

            foreach (FormElement el in ParentForm.GetElementsByName(Name))
            {
                if (el is Radio && ((Radio)el).Checked)
                    ((Radio)el).isChecked = false;
            }

            isChecked = true;
        }

        public override bool IsSuccessful
        {
            get { return isChecked; }
        }
    }
}
