// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Abstraction for HTML image element access.
    /// This allows the image decorator pipeline to work with both MSHTML and WebView2.
    /// </summary>
    public interface IHtmlImageElement
    {
        /// <summary>
        /// Get or set the src attribute.
        /// </summary>
        string Src { get; set; }

        /// <summary>
        /// Get or set the alt attribute.
        /// </summary>
        string Alt { get; set; }

        /// <summary>
        /// Get or set the width attribute.
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Get or set the height attribute.
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Get the natural (intrinsic) width of the image.
        /// </summary>
        int NaturalWidth { get; }

        /// <summary>
        /// Get the natural (intrinsic) height of the image.
        /// </summary>
        int NaturalHeight { get; }

        /// <summary>
        /// Get the title attribute.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Get any attribute value by name.
        /// </summary>
        string GetAttribute(string name);

        /// <summary>
        /// Set any attribute value by name.
        /// </summary>
        void SetAttribute(string name, string value);

        /// <summary>
        /// Remove an attribute.
        /// </summary>
        void RemoveAttribute(string name);

        /// <summary>
        /// Get a CSS style property value.
        /// </summary>
        string GetStyleProperty(string property);

        /// <summary>
        /// Set a CSS style property value.
        /// </summary>
        void SetStyleProperty(string property, string value);

        /// <summary>
        /// Get the computed/current style property value.
        /// </summary>
        string GetCurrentStyleProperty(string property);

        /// <summary>
        /// Get the outer HTML of the element.
        /// </summary>
        string OuterHtml { get; }

        /// <summary>
        /// Get the parent element (for anchor detection, etc.)
        /// Returns null if no parent or at document level.
        /// </summary>
        IHtmlElement ParentElement { get; }

        /// <summary>
        /// Check if this image element is valid (not disposed, still in DOM).
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Insert HTML adjacent to this element.
        /// position: "beforebegin", "afterbegin", "beforeend", "afterend"
        /// </summary>
        void InsertAdjacentHtml(string position, string html);
    }

    /// <summary>
    /// Generic HTML element interface for parent element access.
    /// </summary>
    public interface IHtmlElement
    {
        /// <summary>
        /// The tag name (e.g., "A", "DIV").
        /// </summary>
        string TagName { get; }

        /// <summary>
        /// Get any attribute value by name.
        /// </summary>
        string GetAttribute(string name);

        /// <summary>
        /// Set any attribute value by name.
        /// </summary>
        void SetAttribute(string name, string value);

        /// <summary>
        /// Remove an attribute.
        /// </summary>
        void RemoveAttribute(string name);

        /// <summary>
        /// Get the href attribute (for anchors).
        /// </summary>
        string Href { get; set; }

        /// <summary>
        /// Get the target attribute (for anchors).
        /// </summary>
        string Target { get; set; }

        /// <summary>
        /// Get the inner HTML.
        /// </summary>
        string InnerHtml { get; set; }

        /// <summary>
        /// Get the outer HTML.
        /// </summary>
        string OuterHtml { get; set; }

        /// <summary>
        /// Get parent element.
        /// </summary>
        IHtmlElement ParentElement { get; }

        /// <summary>
        /// Insert HTML adjacent to this element.
        /// position: "beforebegin", "afterbegin", "beforeend", "afterend"
        /// </summary>
        void InsertAdjacentHtml(string position, string html);
    }
}
