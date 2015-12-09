// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public abstract class FormElement
    {
        private readonly HtmlForm parentForm;
        private readonly string name;

        public FormElement(HtmlForm parentForm, string name)
        {
            this.parentForm = parentForm;
            this.name = name;
            this.parentForm.Add(this);
        }

        /// <summary>
        /// If true, then should be included in a form post.
        /// </summary>
        public abstract bool IsSuccessful { get; }

        public abstract void AddData(FormData data);

        public HtmlForm ParentForm
        {
            get { return parentForm; }
        }

        public string Name
        {
            get { return name; }
        }
    }
}
