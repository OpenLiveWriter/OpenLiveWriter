// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class Select : FormElement, IEnumerable
    {
        private readonly bool multiple;
        private readonly Option[] options;

        public Select(HtmlForm parentForm, string name, bool multiple, OptionInfo[] optionInfos) : base(parentForm, name)
        {
            this.multiple = multiple;
            this.options = new Option[optionInfos.Length];
            for (int i = 0; i < optionInfos.Length; i++)
                this.options[i] = new Option(this, optionInfos[i]);
        }

        public bool Multiple
        {
            get { return multiple; }
        }

        public Option GetOptionByIndex(int index)
        {
            return options[index];
        }

        public Option GetOptionByValue(string value)
        {
            foreach (Option opt in options)
            {
                if (opt.Value == value)
                    return opt;
            }
            return null;
        }

        public override bool IsSuccessful
        {
            get
            {
                foreach (Option opt in options)
                {
                    if (opt.Selected)
                        return true;
                }
                return false;
            }
        }

        public override void AddData(FormData data)
        {
            foreach (Option opt in options)
            {
                if (opt.Selected)
                    data.Add(Name, opt.Value);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return options.GetEnumerator();
        }

    }
}
