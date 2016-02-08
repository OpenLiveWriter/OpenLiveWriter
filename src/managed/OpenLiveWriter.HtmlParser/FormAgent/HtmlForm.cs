// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Globalization;

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class HtmlForm : IEnumerable
    {
        private readonly string name;
        private readonly string action;
        private readonly string method;
        private readonly ArrayList elements;

        public HtmlForm(string name, string action, string method)
        {
            this.name = name;
            this.action = action;
            this.method = method;
            this.elements = new ArrayList();
        }

        public string Name
        {
            get { return name; }
        }

        public string Action
        {
            get { return action; }
        }

        public string Method
        {
            get { return method; }
        }

        internal void Add(FormElement el)
        {
            if (el != null)
                elements.Add(el);
        }

        public int ElementCount
        {
            get
            {
                return elements.Count;
            }
        }

        public FormElement GetElementByIndex(int index)
        {
            return (FormElement)elements[index];
        }

        public FormElement[] GetElementsByName(string name)
        {
            ArrayList results = new ArrayList();
            foreach (FormElement el in elements)
            {
                if (el.Name != null && el.Name.ToLowerInvariant() == name)
                    results.Add(el);
            }
            return (FormElement[])results.ToArray(typeof(FormElement));
        }

        public FormElement GetSingleElementByName(string name)
        {
            FormElement[] results = GetElementsByName(name);
            if (results.Length == 0)
                return null;
            else
                return results[0];
        }

        public IEnumerator GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        public FormData Submit(SubmitButton button)
        {
            FormData formData = new FormData();

            foreach (FormElement el in elements)
            {
                if (el.IsSuccessful || object.ReferenceEquals(button, el))
                {
                    el.AddData(formData);
                }
            }

            return formData;
        }

    }
}
