// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.WebView2Shim
{
    /// <summary>
    /// WebView2 implementation of IHtmlImageElement.
    /// Uses the JS bridge to read/write attributes on the DOM element.
    /// </summary>
    public class WebView2ImageElement : IHtmlImageElement
    {
        private readonly WebView2Bridge _bridge;
        private readonly string _elementId;
        private bool _isValid = true;

        // Cached values (loaded eagerly to avoid deadlocks during command execution)
        private string _src;
        private string _alt;
        private int _width;
        private int _height;
        private int _naturalWidth;
        private int _naturalHeight;
        private string _title;

        public WebView2ImageElement(WebView2Bridge bridge, string elementId)
        {
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
            _elementId = elementId ?? throw new ArgumentNullException(nameof(elementId));

            // Load all properties eagerly (safe - called during BeginInvoke, not during command execution)
            LoadAllProperties();
        }

        private void LoadAllProperties()
        {
            try
            {
                _src = _bridge.GetElementAttribute(_elementId, "src") ?? "";
                _alt = _bridge.GetElementAttribute(_elementId, "alt") ?? "";
                _title = _bridge.GetElementAttribute(_elementId, "title") ?? "";

                var widthStr = _bridge.GetElementAttribute(_elementId, "width");
                _width = int.TryParse(widthStr, out int w) ? w : 0;

                var heightStr = _bridge.GetElementAttribute(_elementId, "height");
                _height = int.TryParse(heightStr, out int h) ? h : 0;

                var naturalWidthStr = _bridge.GetElementProperty(_elementId, "naturalWidth");
                _naturalWidth = int.TryParse(naturalWidthStr, out int nw) ? nw : 0;

                var naturalHeightStr = _bridge.GetElementProperty(_elementId, "naturalHeight");
                _naturalHeight = int.TryParse(naturalHeightStr, out int nh) ? nh : 0;

                // If width/height attributes are 0, use natural dimensions
                if (_width == 0 && _naturalWidth > 0) _width = _naturalWidth;
                if (_height == 0 && _naturalHeight > 0) _height = _naturalHeight;

                _isValid = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2ImageElement.LoadAllProperties error: {ex.Message}");
                _isValid = false;
            }
        }

        public string Src
        {
            get => _src;
            set
            {
                _bridge.SetElementAttribute(_elementId, "src", value ?? "");
                _src = value ?? "";
            }
        }

        public string Alt
        {
            get => _alt;
            set
            {
                _bridge.SetElementAttribute(_elementId, "alt", value ?? "");
                _alt = value ?? "";
            }
        }

        public int Width
        {
            get => _width;
            set
            {
                _bridge.SetElementAttribute(_elementId, "width", value.ToString());
                _width = value;
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                _bridge.SetElementAttribute(_elementId, "height", value.ToString());
                _height = value;
            }
        }

        public int NaturalWidth => _naturalWidth;
        public int NaturalHeight => _naturalHeight;

        public string Title
        {
            get => _title;
            set
            {
                _bridge.SetElementAttribute(_elementId, "title", value ?? "");
                _title = value ?? "";
            }
        }

        public string GetAttribute(string name)
        {
            return _bridge.GetElementAttribute(_elementId, name);
        }

        public void SetAttribute(string name, string value)
        {
            _bridge.SetElementAttribute(_elementId, name, value ?? "");
            
            // Update cache for known properties
            switch (name?.ToLowerInvariant())
            {
                case "src": _src = value ?? ""; break;
                case "alt": _alt = value ?? ""; break;
                case "title": _title = value ?? ""; break;
                case "width": int.TryParse(value, out _width); break;
                case "height": int.TryParse(value, out _height); break;
            }
        }

        public void RemoveAttribute(string name)
        {
            _bridge.RemoveElementAttribute(_elementId, name);
        }

        public string GetStyleProperty(string property)
        {
            return _bridge.GetElementStyleProperty(_elementId, property);
        }

        public void SetStyleProperty(string property, string value)
        {
            _bridge.SetElementStyleProperty(_elementId, property, value ?? "");
        }

        public string GetCurrentStyleProperty(string property)
        {
            // For WebView2, we use getComputedStyle
            return _bridge.GetElementComputedStyle(_elementId, property);
        }

        public string OuterHtml
        {
            get => _bridge.GetElementProperty(_elementId, "outerHTML") ?? "";
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

        public bool IsValid => _isValid;

        /// <summary>
        /// Insert HTML adjacent to this element.
        /// </summary>
        public void InsertAdjacentHtml(string position, string html)
        {
            _bridge.InsertAdjacentHtml(_elementId, position, html);
        }

        /// <summary>
        /// Refresh cached properties from DOM.
        /// Call this if the DOM may have changed externally.
        /// </summary>
        public void Refresh()
        {
            LoadAllProperties();
        }
    }
}
