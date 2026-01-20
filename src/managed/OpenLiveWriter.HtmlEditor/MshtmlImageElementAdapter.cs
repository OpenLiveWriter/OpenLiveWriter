// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Adapter that wraps MSHTML IHTMLElement to implement IHtmlImageElement.
    /// This provides backward compatibility for code that still uses MSHTML.
    /// </summary>
    public class MshtmlImageElementAdapter : IHtmlImageElement
    {
        private readonly IHTMLElement _element;
        private readonly IHTMLImgElement _imgElement;

        public MshtmlImageElementAdapter(IHTMLElement element)
        {
            _element = element;
            _imgElement = element as IHTMLImgElement;
        }

        /// <summary>
        /// Get the underlying MSHTML element for legacy code.
        /// </summary>
        public IHTMLElement Element => _element;

        public string Src
        {
            get => _imgElement?.src ?? GetAttribute("src");
            set
            {
                if (_imgElement != null)
                    _imgElement.src = value;
                else
                    SetAttribute("src", value);
            }
        }

        public string Alt
        {
            get => _imgElement?.alt ?? GetAttribute("alt");
            set
            {
                if (_imgElement != null)
                    _imgElement.alt = value;
                else
                    SetAttribute("alt", value);
            }
        }

        public int Width
        {
            get => _imgElement?.width ?? 0;
            set
            {
                if (_imgElement != null)
                    _imgElement.width = value;
                else
                    SetAttribute("width", value.ToString());
            }
        }

        public int Height
        {
            get => _imgElement?.height ?? 0;
            set
            {
                if (_imgElement != null)
                    _imgElement.height = value;
                else
                    SetAttribute("height", value.ToString());
            }
        }

        public int NaturalWidth
        {
            get
            {
                // MSHTML doesn't have naturalWidth, use width if no style override
                var img5 = _element as IHTMLImgElement2;
                if (img5 != null)
                {
                    // Try to get naturalWidth if available (IE9+)
                    try
                    {
                        var result = _element.getAttribute("naturalWidth", 0);
                        if (result != null && int.TryParse(result.ToString(), out int w))
                            return w;
                    }
                    catch { }
                }
                return _imgElement?.width ?? 0;
            }
        }

        public int NaturalHeight
        {
            get
            {
                var img5 = _element as IHTMLImgElement2;
                if (img5 != null)
                {
                    try
                    {
                        var result = _element.getAttribute("naturalHeight", 0);
                        if (result != null && int.TryParse(result.ToString(), out int h))
                            return h;
                    }
                    catch { }
                }
                return _imgElement?.height ?? 0;
            }
        }

        public string Title
        {
            get => _element?.title ?? "";
            set
            {
                if (_element != null)
                    _element.title = value;
            }
        }

        public string GetAttribute(string name)
        {
            if (_element == null) return null;
            var result = _element.getAttribute(name, 0);
            return result?.ToString();
        }

        public void SetAttribute(string name, string value)
        {
            _element?.setAttribute(name, value, 0);
        }

        public void RemoveAttribute(string name)
        {
            _element?.removeAttribute(name, 0);
        }

        public string GetStyleProperty(string property)
        {
            var style = _element?.style as IHTMLStyle;
            if (style == null) return null;

            // Get property via reflection since IHTMLStyle has many individual properties
            return style.getAttribute(property, 0)?.ToString();
        }

        public void SetStyleProperty(string property, string value)
        {
            var style = _element?.style as IHTMLStyle;
            style?.setAttribute(property, value, 0);
        }

        public string GetCurrentStyleProperty(string property)
        {
            var element2 = _element as IHTMLElement2;
            if (element2 == null) return null;

            var currentStyle = element2.currentStyle as IHTMLCurrentStyle;
            if (currentStyle == null) return null;

            return currentStyle.getAttribute(property, 0)?.ToString();
        }

        public string OuterHtml => _element?.outerHTML ?? "";

        public IHtmlElement ParentElement
        {
            get
            {
                var parent = _element?.parentElement;
                return parent != null ? new MshtmlElementAdapter(parent) : null;
            }
        }

        public bool IsValid => _element != null;

        public void InsertAdjacentHtml(string position, string html)
        {
            _element?.insertAdjacentHTML(position, html);
        }
    }

    /// <summary>
    /// Adapter for generic MSHTML elements.
    /// </summary>
    public class MshtmlElementAdapter : IHtmlElement
    {
        private readonly IHTMLElement _element;

        public MshtmlElementAdapter(IHTMLElement element)
        {
            _element = element;
        }

        /// <summary>
        /// Get the underlying MSHTML element.
        /// </summary>
        public IHTMLElement Element => _element;

        public string TagName => _element?.tagName ?? "";

        public string GetAttribute(string name)
        {
            var result = _element?.getAttribute(name, 0);
            return result?.ToString();
        }

        public void SetAttribute(string name, string value)
        {
            _element?.setAttribute(name, value, 0);
        }

        public void RemoveAttribute(string name)
        {
            _element?.removeAttribute(name, 0);
        }

        public string Href
        {
            get
            {
                var anchor = _element as IHTMLAnchorElement;
                return anchor?.href ?? GetAttribute("href");
            }
            set
            {
                var anchor = _element as IHTMLAnchorElement;
                if (anchor != null)
                    anchor.href = value;
                else
                    SetAttribute("href", value);
            }
        }

        public string Target
        {
            get
            {
                var anchor = _element as IHTMLAnchorElement;
                return anchor?.target ?? GetAttribute("target");
            }
            set
            {
                var anchor = _element as IHTMLAnchorElement;
                if (anchor != null)
                    anchor.target = value;
                else
                    SetAttribute("target", value);
            }
        }

        public string InnerHtml
        {
            get => _element?.innerHTML ?? "";
            set
            {
                if (_element != null)
                    _element.innerHTML = value;
            }
        }

        public string OuterHtml
        {
            get => _element?.outerHTML ?? "";
            set
            {
                if (_element != null)
                    _element.outerHTML = value;
            }
        }

        public IHtmlElement ParentElement
        {
            get
            {
                var parent = _element?.parentElement;
                return parent != null ? new MshtmlElementAdapter(parent) : null;
            }
        }

        public void InsertAdjacentHtml(string position, string html)
        {
            _element?.insertAdjacentHTML(position, html);
        }
    }
}
