// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Globalization;
using System.IO;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class FormFactory
    {
        private SimpleHtmlParser parser;

        public FormFactory(string html)
        {
            parser = new SimpleHtmlParser(html);
        }

        public FormFactory(Stream s)
        {
            using (StreamReader reader = new StreamReader(s))
            {
                parser = new SimpleHtmlParser(reader.ReadToEnd());
            }
        }

        public HtmlForm NextForm()
        {
            Element el;
            while (null != (el = parser.Next()))
            {
                BeginTag tag = el as BeginTag;
                if (tag == null)
                    continue;

                if (tag.NameEquals("form"))
                    return HandleForm(tag);
            }
            return null;
        }

        private HtmlForm HandleForm(BeginTag formTag)
        {
            string name = formTag.GetAttributeValue("name");
            string action = formTag.GetAttributeValue("action");
            string method = formTag.GetAttributeValue("method");

            HtmlForm htmlForm = new HtmlForm(name, action, method);

            Element el;
            while (null != (el = parser.Next()))
            {
                if (el is EndTag && ((EndTag)el).NameEquals("form"))
                    break;

                BeginTag tag = el as BeginTag;
                if (tag == null)
                    continue;

                switch (tag.Name.ToLowerInvariant())
                {
                    case "input":
                        HandleInput(htmlForm, tag);
                        break;
                    case "select":
                        HandleSelect(htmlForm, tag);
                        break;
                    case "textarea":
                        HandleTextarea(htmlForm, tag);
                        break;
                }
            }

            return htmlForm;
        }

        private void HandleInput(HtmlForm parentForm, BeginTag inputTag)
        {
            string type = inputTag.GetAttributeValue("type");
            if (type != null)
                type = type.Trim().ToLowerInvariant();

            string name = inputTag.GetAttributeValue("name");
            string value = inputTag.GetAttributeValue("value");

            switch (type)
            {
                case "password":
                    new Textbox(parentForm, name, value);
                    break;

                case "checkbox":
                    {
                        int dummy;
                        bool isChecked = inputTag.GetAttribute("checked", true, 0, out dummy) != null;
                        new Checkbox(parentForm, name, value, isChecked);
                        break;
                    }

                case "radio":
                    {
                        int dummy;
                        bool isChecked = inputTag.GetAttribute("checked", true, 0, out dummy) != null;
                        new Radio(parentForm, name, value, isChecked);
                        break;
                    }

                case "submit":
                    new SubmitButton(parentForm, name, value);
                    break;

                case "image":
                    new ImageButton(parentForm, name, value, inputTag.GetAttributeValue("src"));
                    break;

                case "hidden":
                    new Hidden(parentForm, name, value);
                    break;

                case "text":
                default:
                    new Textbox(parentForm, name, value);
                    break;
            }
        }

        private void HandleSelect(HtmlForm parentForm, BeginTag selectTag)
        {
            string name = selectTag.GetAttributeValue("name");
            int dummy;
            bool multiple = selectTag.GetAttribute("multiple", true, 0, out dummy) != null;

            ArrayList optionInfos = new ArrayList();

            Element el = parser.Next();
            while (el != null)
            {
                BeginTag tag = el as BeginTag;
                if (tag != null && tag.NameEquals("option"))
                {
                    string value = tag.GetAttributeValue("value");
                    bool isSelected = tag.GetAttribute("selected", true, 0, out dummy) != null;

                    string label = string.Empty;
                    el = parser.Next();
                    if (el != null && el is Text)
                    {
                        label = HtmlUtils.UnEscapeEntities(el.ToString(), HtmlUtils.UnEscapeMode.NonMarkupText).TrimEnd(' ', '\r', '\n', '\t');
                        el = parser.Next();
                    }
                    optionInfos.Add(new OptionInfo(value, label, isSelected));
                    continue;
                }

                if (el is EndTag && ((EndTag)el).NameEquals("select"))
                {
                    new Select(parentForm, name, multiple, (OptionInfo[])optionInfos.ToArray(typeof(OptionInfo)));
                    return;
                }

                el = parser.Next();
            }
        }

        private void HandleTextarea(HtmlForm parentForm, BeginTag textareaTag)
        {
            string name = textareaTag.GetAttributeValue("name");
            string value = parser.CollectTextUntil("textarea");

            new Textarea(parentForm, name, value);
        }
    }
}
