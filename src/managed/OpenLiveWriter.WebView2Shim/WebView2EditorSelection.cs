// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text.Json;
using System.Threading.Tasks;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.WebView2Shim
{
    /// <summary>
    /// WebView2 implementation of IEditorSelection.
    /// Uses the JavaScript bridge to query selection state from the DOM.
    /// </summary>
    public class WebView2EditorSelection : IEditorSelection
    {
        private readonly WebView2Bridge _bridge;
        private readonly Func<Task<string>> _executeScriptAsync;
        
        // Cached selection state - updated when selection changes
        private SelectionType _selectionType = SelectionType.None;
        private WebView2SelectedElement _selectedElement;
        private WebView2SelectedImage _selectedImage;
        private string _selectedText;
        private string _selectedHtml;
        private bool _isValid;

        public WebView2EditorSelection(WebView2Bridge bridge, Func<Task<string>> executeScriptAsync)
        {
            _bridge = bridge;
            _executeScriptAsync = executeScriptAsync;
        }

        /// <summary>
        /// Updates the selection state from a control selection event.
        /// Called when JavaScript notifies us of a control selection.
        /// </summary>
        public void UpdateFromControlSelection(string tagName, string editorId)
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2EditorSelection.UpdateFromControlSelection: tagName={tagName}, editorId={editorId}");
            
            if (string.IsNullOrEmpty(tagName))
            {
                System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] WebView2EditorSelection: tagName is empty, clearing selection");
                ClearSelection();
                return;
            }

            _isValid = true;
            
            // Determine selection type from tag name
            switch (tagName.ToUpperInvariant())
            {
                case "IMG":
                    _selectionType = SelectionType.Image;
                    _selectedImage = new WebView2SelectedImage(_bridge, _executeScriptAsync, editorId);
                    _selectedElement = _selectedImage;
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2EditorSelection: Set to Image, type={_selectionType}");
                    break;
                case "TABLE":
                    _selectionType = SelectionType.Table;
                    _selectedElement = new WebView2SelectedElement(_bridge, _executeScriptAsync, tagName, editorId);
                    _selectedImage = null;
                    break;
                default:
                    // Check if this is smart content (has contentEditable=false or specific class)
                    _selectionType = SelectionType.Control;
                    _selectedElement = new WebView2SelectedElement(_bridge, _executeScriptAsync, tagName, editorId);
                    _selectedImage = null;
                    break;
            }

            _selectedText = null;
            _selectedHtml = null;
        }

        /// <summary>
        /// Updates the selection state for a text selection.
        /// </summary>
        public void UpdateFromTextSelection(string selectedText, string selectedHtml)
        {
            _isValid = !string.IsNullOrEmpty(selectedText) || !string.IsNullOrEmpty(selectedHtml);
            _selectionType = _isValid ? SelectionType.Text : SelectionType.None;
            _selectedElement = null;
            _selectedImage = null;
            _selectedText = selectedText;
            _selectedHtml = selectedHtml;
        }

        /// <summary>
        /// Clears the selection state.
        /// </summary>
        public void ClearSelection()
        {
            _isValid = false;
            _selectionType = SelectionType.None;
            _selectedElement = null;
            _selectedImage = null;
            _selectedText = null;
            _selectedHtml = null;
        }

        #region IEditorSelection Implementation

        public SelectionType SelectionType => _selectionType;

        public bool IsValid => _isValid;

        public bool HasTextSelection => _selectionType == SelectionType.Text;

        public bool HasControlSelection => _selectionType == SelectionType.Image ||
                                           _selectionType == SelectionType.Table ||
                                           _selectionType == SelectionType.Control ||
                                           _selectionType == SelectionType.SmartContent;

        public ISelectedElement SelectedElement => _selectedElement;

        public ISelectedImage SelectedImage => _selectedImage;

        public string SelectedText => _selectedText;

        public string SelectedHtml => _selectedHtml;

        #endregion
    }

    /// <summary>
    /// WebView2 implementation of ISelectedElement.
    /// Provides access to element properties via JavaScript bridge.
    /// </summary>
    public class WebView2SelectedElement : ISelectedElement
    {
        protected readonly WebView2Bridge _bridge;
        protected readonly Func<Task<string>> _executeScriptAsync;
        protected readonly string _tagName;
        protected readonly string _editorId;

        // Cached attribute values - populated lazily
        protected string _id;
        protected string _innerHtml;
        protected string _outerHtml;

        public WebView2SelectedElement(WebView2Bridge bridge, Func<Task<string>> executeScriptAsync, string tagName, string editorId)
        {
            _bridge = bridge;
            _executeScriptAsync = executeScriptAsync;
            _tagName = tagName;
            _editorId = editorId;
        }

        public string TagName => _tagName;

        public string Id => _id ??= GetAttribute("id");

        public string EditorId => _editorId;

        public virtual string GetAttribute(string name)
        {
            // Use synchronous bridge method for getting attributes
            return _bridge.GetElementAttribute(_editorId, name);
        }

        public virtual async Task SetAttributeAsync(string name, string value)
        {
            var script = $"(function() {{ var el = document.querySelector('[data-olw-id=\"{_editorId}\"]'); if (el) {{ el.setAttribute('{EscapeJs(name)}', '{EscapeJs(value)}'); return 'ok'; }} return 'not found'; }})()";
            await _executeScriptAsync();
            // Note: The actual script execution happens through the bridge
            _bridge.SetElementAttribute(_editorId, name, value);
        }

        public virtual async Task RemoveAttributeAsync(string name)
        {
            _bridge.RemoveElementAttribute(_editorId, name);
            await Task.CompletedTask;
        }

        public string InnerHtml
        {
            get
            {
                if (_innerHtml == null)
                {
                    _innerHtml = _bridge.GetElementProperty(_editorId, "innerHTML") ?? string.Empty;
                }
                return _innerHtml;
            }
        }

        public string OuterHtml
        {
            get
            {
                if (_outerHtml == null)
                {
                    _outerHtml = _bridge.GetElementProperty(_editorId, "outerHTML") ?? string.Empty;
                }
                return _outerHtml;
            }
        }

        public async Task<string> GetComputedStyleAsync(string property)
        {
            return _bridge.GetElementComputedStyle(_editorId, property);
        }

        public async Task SetStyleAsync(string property, string value)
        {
            _bridge.SetElementStyle(_editorId, property, value);
            await Task.CompletedTask;
        }

        protected static string EscapeJs(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }

    /// <summary>
    /// WebView2 implementation of ISelectedImage with image-specific properties.
    /// </summary>
    public class WebView2SelectedImage : WebView2SelectedElement, ISelectedImage
    {
        // Cached image properties - populated eagerly in constructor to avoid DoEvents deadlocks
        private string _src;
        private string _alt;
        private int _width;
        private int _height;
        private int _naturalWidth;
        private int _naturalHeight;

        public WebView2SelectedImage(WebView2Bridge bridge, Func<Task<string>> executeScriptAsync, string editorId)
            : base(bridge, executeScriptAsync, "IMG", editorId)
        {
            // Eagerly load all properties to avoid DoEvents deadlock when accessed during ribbon commands
            _src = GetAttribute("src") ?? string.Empty;
            _alt = GetAttribute("alt") ?? string.Empty;
            
            var widthStr = GetAttribute("width");
            _width = int.TryParse(widthStr, out int w) ? w : 0;
            
            var heightStr = GetAttribute("height");
            _height = int.TryParse(heightStr, out int h) ? h : 0;
            
            var nwStr = bridge.GetElementProperty(editorId, "naturalWidth");
            _naturalWidth = int.TryParse(nwStr, out int nw) ? nw : 0;
            
            var nhStr = bridge.GetElementProperty(editorId, "naturalHeight");
            _naturalHeight = int.TryParse(nhStr, out int nh) ? nh : 0;
        }

        public string Src => _src;
        public string Alt => _alt;
        public int Width => _width;
        public int Height => _height;
        public int NaturalWidth => _naturalWidth;
        public int NaturalHeight => _naturalHeight;

        /// <summary>
        /// Get an IHtmlImageElement wrapper for the decorator pipeline.
        /// </summary>
        public IHtmlImageElement AsHtmlImageElement()
        {
            return new SelectedImageAdapter(this, _bridge);
        }

        /// <summary>
        /// Get the underlying bridge (for creating ImagePropertiesInfo).
        /// </summary>
        public WebView2Bridge Bridge => _bridge;

        public async Task SetSrcAsync(string src)
        {
            await SetAttributeAsync("src", src);
            _src = src;
        }

        public async Task SetAltAsync(string alt)
        {
            // Use bridge directly (synchronous, but fire-and-forget style)
            _bridge.SetElementAttribute(_editorId, "alt", alt ?? "");
            _alt = alt;
            await Task.CompletedTask;
        }

        public async Task SetDimensionsAsync(int width, int height)
        {
            // Use batch method to set both attributes in one script call
            // This avoids nested DoEvents loops and also syncs the body to bridge
            _bridge.SetElementAttributes(_editorId, 
                ("width", width.ToString()), 
                ("height", height.ToString()));
            _width = width;
            _height = height;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Insert HTML adjacent to this image element.
        /// </summary>
        /// <param name="position">beforebegin, afterbegin, beforeend, afterend</param>
        /// <param name="html">HTML to insert</param>
        public void InsertAdjacentHtml(string position, string html)
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SelectedImage.InsertAdjacentHtml: position={position}, html={html}");
            _bridge.InsertAdjacentHtml(_editorId, position, html);
        }

        /// <summary>
        /// Wrap this image with an anchor tag.
        /// </summary>
        public void WrapWithAnchor(string href, string target = null)
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SelectedImage.WrapWithAnchor: href={href}, target={target}");
            _bridge.WrapElementWithAnchor(_editorId, href, target);
        }

        /// <summary>
        /// Remove anchor wrapper from this image (if any).
        /// </summary>
        public void UnwrapFromAnchor()
        {
            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] WebView2SelectedImage.UnwrapFromAnchor");
            _bridge.UnwrapElementFromAnchor(_editorId);
        }
    }
}
