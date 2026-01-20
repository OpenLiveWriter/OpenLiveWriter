// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.WebView2Shim
{
    /// <summary>
    /// WebView2 implementation of the IHtmlElement abstraction interface.
    /// This is used by the image decorator pipeline for parent element access (anchors, etc.).
    /// Not to be confused with WebView2Element which implements the MSHTML IHTMLElement interface.
    /// </summary>
    public class WebView2HtmlElement : IHtmlElement
    {
        private readonly WebView2Bridge _bridge;
        private readonly string _elementId;

        public WebView2HtmlElement(WebView2Bridge bridge, string elementId)
        {
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
            _elementId = elementId ?? throw new ArgumentNullException(nameof(elementId));
        }

        /// <summary>
        /// The element's data-olw-id value.
        /// </summary>
        public string ElementId => _elementId;

        public string TagName
        {
            get
            {
                var result = _bridge.ExecuteScript($"(function() {{ var el = document.querySelector('[data-olw-id=\"{_elementId}\"]'); return el ? el.tagName : ''; }})()");
                return CleanJsonString(result);
            }
        }

        public string GetAttribute(string name)
        {
            return _bridge.GetElementAttribute(_elementId, name);
        }

        public void SetAttribute(string name, string value)
        {
            _bridge.SetElementAttribute(_elementId, name, value ?? "");
        }

        public void RemoveAttribute(string name)
        {
            _bridge.RemoveElementAttribute(_elementId, name);
        }

        public string Href
        {
            get => GetAttribute("href") ?? "";
            set => SetAttribute("href", value);
        }

        public string Target
        {
            get => GetAttribute("target") ?? "";
            set => SetAttribute("target", value);
        }

        public string InnerHtml
        {
            get
            {
                var result = _bridge.ExecuteScript($"(function() {{ var el = document.querySelector('[data-olw-id=\"{_elementId}\"]'); return el ? el.innerHTML : ''; }})()");
                return CleanJsonString(result);
            }
            set
            {
                var escaped = EscapeJsString(value);
                _bridge.ExecuteScriptFireAndForget($"(function() {{ var el = document.querySelector('[data-olw-id=\"{_elementId}\"]'); if (el) el.innerHTML = '{escaped}'; }})()");
            }
        }

        public string OuterHtml
        {
            get
            {
                var result = _bridge.ExecuteScript($"(function() {{ var el = document.querySelector('[data-olw-id=\"{_elementId}\"]'); return el ? el.outerHTML : ''; }})()");
                return CleanJsonString(result);
            }
            set
            {
                var escaped = EscapeJsString(value);
                _bridge.ExecuteScriptFireAndForget($"(function() {{ var el = document.querySelector('[data-olw-id=\"{_elementId}\"]'); if (el) el.outerHTML = '{escaped}'; }})()");
            }
        }

        public IHtmlElement ParentElement
        {
            get
            {
                var parentId = _bridge.GetParentElementId(_elementId);
                if (string.IsNullOrEmpty(parentId)) return null;
                return new WebView2HtmlElement(_bridge, parentId);
            }
        }

        public void InsertAdjacentHtml(string position, string html)
        {
            _bridge.InsertAdjacentHtml(_elementId, position, html);
        }

        private static string EscapeJsString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private static string CleanJsonString(string json)
        {
            if (string.IsNullOrEmpty(json)) return "";
            if (json.StartsWith("\"") && json.EndsWith("\""))
                return json.Substring(1, json.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
            return json;
        }
    }
}
