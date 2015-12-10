// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class Option
    {
        private readonly Select parentSelect;
        private readonly string value;
        private readonly string label;
        private bool selected;

        public Option(Select parentSelect, OptionInfo optionInfo)
        {
            this.parentSelect = parentSelect;
            this.label = optionInfo.Label;
            this.value = optionInfo.Value;
            if (this.value == null)
                this.value = this.label;
            this.selected = optionInfo.Selected;
        }

        public Select ParentSelect
        {
            get { return parentSelect; }
        }

        public string Value
        {
            get { return this.value; }
        }

        public string Label
        {
            get { return label; }
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (parentSelect.Multiple)
                    selected = value;
                else
                {
                    if (!value)
                    {
                        throw new InvalidOperationException("Cannot unselect an option that is in a non-multiple select");
                    }
                    else
                    {
                        foreach (Option opt in parentSelect)
                        {
                            opt.selected = false;
                        }
                        selected = true;
                    }
                }
            }
        }
    }
}
