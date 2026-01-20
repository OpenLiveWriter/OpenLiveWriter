// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.WebView2Shim
{
    /// <summary>
    /// Adapter that converts an ISelectedImage to an IHtmlImageElement.
    /// This allows the image decorator pipeline to work with WebView2 selections.
    /// </summary>
    public class SelectedImageAdapter : IHtmlImageElement
    {
        private readonly ISelectedImage _selectedImage;
        private readonly WebView2Bridge _bridge;
        private readonly string _editorId;

        public SelectedImageAdapter(ISelectedImage selectedImage, WebView2Bridge bridge)
        {
            _selectedImage = selectedImage ?? throw new ArgumentNullException(nameof(selectedImage));
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
            _editorId = selectedImage.EditorId;
        }

        public string Src
        {
            get => _selectedImage.Src;
            set
            {
                if (_bridge != null)
                    _bridge.SetElementAttribute(_editorId, "src", value);
                // Note: Can't update _selectedImage.Src as it's read-only
            }
        }

        public string Alt
        {
            get => _selectedImage.Alt;
            set
            {
                if (_bridge != null)
                    _bridge.SetElementAttribute(_editorId, "alt", value);
            }
        }

        public int Width
        {
            get => _selectedImage.Width;
            set
            {
                if (_bridge != null)
                    _bridge.SetElementAttribute(_editorId, "width", value.ToString());
            }
        }

        public int Height
        {
            get => _selectedImage.Height;
            set
            {
                if (_bridge != null)
                    _bridge.SetElementAttribute(_editorId, "height", value.ToString());
            }
        }

        public int NaturalWidth => _selectedImage.NaturalWidth;
        public int NaturalHeight => _selectedImage.NaturalHeight;

        public string Title
        {
            get => _selectedImage.GetAttribute("title") ?? "";
            set
            {
                if (_bridge != null)
                    _bridge.SetElementAttribute(_editorId, "title", value);
            }
        }

        public string GetAttribute(string name)
        {
            return _selectedImage.GetAttribute(name);
        }

        public void SetAttribute(string name, string value)
        {
            if (_bridge != null)
                _bridge.SetElementAttribute(_editorId, name, value);
        }

        public void RemoveAttribute(string name)
        {
            if (_bridge != null)
                _bridge.RemoveElementAttribute(_editorId, name);
        }

        public string GetStyleProperty(string property)
        {
            if (_bridge != null)
                return _bridge.GetElementStyleProperty(_editorId, property);
            return null;
        }

        public void SetStyleProperty(string property, string value)
        {
            if (_bridge != null)
                _bridge.SetElementStyleProperty(_editorId, property, value);
        }

        public string GetCurrentStyleProperty(string property)
        {
            if (_bridge != null)
                return _bridge.GetElementComputedStyleProperty(_editorId, property);
            return null;
        }

        public string OuterHtml => _selectedImage.OuterHtml;

        public IHtmlElement ParentElement
        {
            get
            {
                if (_bridge == null) return null;
                var parentId = _bridge.GetParentElementId(_editorId);
                if (string.IsNullOrEmpty(parentId)) return null;
                return new WebView2HtmlElement(_bridge, parentId);
            }
        }
        
        public void InsertAdjacentHtml(string position, string html)
        {
            _bridge?.InsertAdjacentHtml(_editorId, position, html);
        }

        public bool IsValid => !string.IsNullOrEmpty(_editorId) && _bridge != null;
    }
}
