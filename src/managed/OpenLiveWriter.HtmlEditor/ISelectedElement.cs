// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Abstraction for a selected HTML element that works with both MSHTML and WebView2.
    /// This interface provides a clean separation from browser-specific DOM APIs.
    /// </summary>
    public interface ISelectedElement
    {
        /// <summary>
        /// The tag name of the element (e.g., "IMG", "TABLE", "DIV").
        /// </summary>
        string TagName { get; }

        /// <summary>
        /// The element's ID attribute, or null if not set.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The internal tracking ID used by the editor (e.g., "olw-1").
        /// </summary>
        string EditorId { get; }

        /// <summary>
        /// Gets an attribute value from the element.
        /// </summary>
        string GetAttribute(string name);

        /// <summary>
        /// Sets an attribute value on the element.
        /// </summary>
        Task SetAttributeAsync(string name, string value);

        /// <summary>
        /// Removes an attribute from the element.
        /// </summary>
        Task RemoveAttributeAsync(string name);

        /// <summary>
        /// Gets the inner HTML of the element.
        /// </summary>
        string InnerHtml { get; }

        /// <summary>
        /// Gets the outer HTML of the element.
        /// </summary>
        string OuterHtml { get; }

        /// <summary>
        /// Gets a computed style value.
        /// </summary>
        Task<string> GetComputedStyleAsync(string property);

        /// <summary>
        /// Sets an inline style property.
        /// </summary>
        Task SetStyleAsync(string property, string value);
    }

    /// <summary>
    /// Extended interface for image elements with image-specific properties.
    /// </summary>
    public interface ISelectedImage : ISelectedElement
    {
        /// <summary>
        /// The image source URL.
        /// </summary>
        string Src { get; }

        /// <summary>
        /// The alt text for the image.
        /// </summary>
        string Alt { get; }

        /// <summary>
        /// The display width of the image in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The display height of the image in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The natural (intrinsic) width of the image in pixels.
        /// </summary>
        int NaturalWidth { get; }

        /// <summary>
        /// The natural (intrinsic) height of the image in pixels.
        /// </summary>
        int NaturalHeight { get; }

        /// <summary>
        /// Sets the image source URL.
        /// </summary>
        Task SetSrcAsync(string src);

        /// <summary>
        /// Sets the alt text.
        /// </summary>
        Task SetAltAsync(string alt);

        /// <summary>
        /// Sets the display dimensions.
        /// </summary>
        Task SetDimensionsAsync(int width, int height);
    }
}
