// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class OptionInfo
    {
        private readonly string value;
        private readonly string label;
        private bool selected;

        public OptionInfo(string value, string label, bool selected)
        {
            this.value = value;
            this.label = label;
            this.selected = selected;
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
            set { selected = value; }
        }
    }
}
