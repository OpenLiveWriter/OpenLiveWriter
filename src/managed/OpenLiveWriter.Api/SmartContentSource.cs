// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Xml;

    using JetBrains.Annotations;

    /// <summary>
    /// <para>Base class for plugins that wish to enable the insertion of smart, two-way editable HTML content
    /// into posts. The source of content to be inserted can any or all of the following: an Insert dialog,
    /// a URL, or LiveClipboard data.</para>
    /// <para>Implementors of this class should override the CreateContent method(s) corresponding to
    /// the content-sources they wish to support. Note also that each of the CreateContent methods
    /// has a corresponding class-level attribute that must be specified along with the override.</para>
    /// <para>There is a single instance of a given SmartContentSource created for each Open Live Writer
    /// process. The implementation of SmartContentSource objects must therefore be stateless (the
    /// context required to carry out the responsibilities of the various methods are passed as parameters to
    /// the respective methods).</para>
    /// </summary>
    public abstract class SmartContentSource : WriterPlugin
    {
        /// <summary>
        /// Resize capabilities for this SmartContentSource (defaults to ResizeCapabilities.None).
        /// Depending upon the flags specified in this return value the SmartContentSource should
        /// also override the appropriate OnResize methods to implement desired resizing behavior.
        /// </summary>
        public virtual ResizeCapabilities ResizeCapabilities => ResizeCapabilities.None;

        /// <summary>
        /// Create content using an Insert dialog. Plugin classes which override this method must
        /// also be declared with the <see cref="InsertableContentSourceAttribute" />.
        /// </summary>
        /// <param name="dialogOwner">Owner for any dialogs shown.</param>
        /// <param name="newContent">SmartContent object which is populated with initial values by this method.</param>
        /// <returns>DialogResult.OK if content was successfully created, DialogResult.Cancel
        /// if the user cancels the Insert dialog.</returns>
        /// <exception cref="ContentCreationException">Thrown if an error occurs during the creation of content.</exception>
#pragma warning disable CC0057 // Unused parameters
        public virtual DialogResult CreateContent([NotNull] IWin32Window dialogOwner, [NotNull] ISmartContent newContent)
#pragma warning restore CC0057 // Unused parameters
        {
            throw new NotImplementedException(nameof(SmartContentSource.CreateContent));
        }

        /// <summary>
        /// Create content using the contents of a LiveClipboard Xml document. Plugin classes which override
        /// this method must also be declared with the LiveClipboardContentSourceAttribute.
        /// </summary>
        /// <param name="dialogOwner">Owner for any dialogs shown.</param>
        /// <param name="liveClipboardDocument">LiveClipboard Xml document</param>
        /// <param name="newContent">SmartContent object which is populated with initial values by this method.</param>
        /// <returns>DialogResult.OK if content was successfully created, DialogResult.Cancel
        /// if the user cancels the Insert dialog.</returns>
        /// <exception cref="ContentCreationException">Thrown if an error occurs during the creation of content.</exception>
#pragma warning disable CC0057 // Unused parameters
        public virtual DialogResult CreateContentFromLiveClipboard([NotNull] IWin32Window dialogOwner, [NotNull] XmlDocument liveClipboardDocument, [NotNull] ISmartContent newContent)
#pragma warning restore CC0057 // Unused parameters
        {
            throw new NotImplementedException(nameof(SmartContentSource.CreateContentFromLiveClipboard));
        }

        /// <summary>
        /// Create content based on a URL. The source of this URL can either be the page the user was
        /// navigated to when they pressed the "Blog This" button or a URL that is pasted or dragged
        /// into the editor. Plugin classes which override this method must also be declared with the
        /// UrlContentSourceAttribute.
        /// </summary>
        /// <param name="url">Url to create content from.</param>
        /// <param name="title">Default title of post (used for "Blog This" case).</param>
        /// <param name="newContent">SmartContent object which is populated with initial values by this method.</param>
        /// <exception cref="ContentCreationException">Thrown if an error occurs during the creation of content.</exception>
#pragma warning disable CC0057 // Unused parameters
        public virtual void CreateContentFromUrl(
            [NotNull] string url,
            [NotNull] ref string title,
            [NotNull] ISmartContent newContent)
#pragma warning restore CC0057 // Unused parameters
        {
            throw new NotImplementedException(nameof(SmartContentSource.CreateContentFromUrl));
        }

        /// <summary>
        /// Create a new SmartContentEditor for this ContentSource. The SmartContentEditor is the control
        /// that appears in the Sidebar whenever a SmartContent object created by this content source
        /// is selected within the PostEditor. This method must be overridden by all subclasses of SmartContentSource.
        /// </summary>
        /// <param name="editorSite">Interface to the SmartContentEditor's site.</param>
        /// <returns>A new instance of a class derived from SmartContentEditor.</returns>
        [NotNull]
        public abstract SmartContentEditor CreateEditor([NotNull] ISmartContentEditorSite editorSite);

        /// <summary>
        /// Generate the HTML content which is used to represent the SmartContent item within
        /// the post editor. The default behavior for this method if it is not overridden is to
        /// call GeneratePublishHtml.
        /// </summary>
        /// <param name="content">SmartContent object to generate HTML for.</param>
        /// <param name="publishingContext">Publishing context for HTML generation.</param>
        /// <returns>HTML content to be inserted into the post editor.</returns>
        [NotNull]
        public virtual string GenerateEditorHtml(
            [NotNull] ISmartContent content,
            [NotNull] IPublishingContext publishingContext) => this.GeneratePublishHtml(content, publishingContext);

        /// <summary>
        /// Generate the HTML content which is published to the destination weblog.
        /// </summary>
        /// <param name="content">SmartContent object to generate HTML for.</param>
        /// <param name="publishingContext">Publishing context for HTML generation.</param>
        /// <returns>HTML to be published to the destination weblog.</returns>
        [NotNull]
        public abstract string GeneratePublishHtml(
            [NotNull] ISmartContent content,
            [NotNull] IPublishingContext publishingContext);

        /// <summary>
        /// Notification that the sizing of an object is complete. The  implementation of
        /// this method should update the ISmartContent object as appropriate based on the
        /// new size. The editor will first call this method and then call the GenerateEditorHtml
        /// method to update the display based on the new size.
        /// </summary>
        /// <param name="content">SmartContent which is being resized.</param>
        /// <param name="newSize">New size of object (or of resizable element if the ResizeOptions.ResizableElementId option was specified).</param>
#pragma warning disable CC0057 // Unused parameters
        public virtual void OnResizeComplete([NotNull] ISmartContent content, Size newSize)
#pragma warning restore CC0057 // Unused parameters
        {
        }

        /// <summary>
        /// Notification that the resizing of the specified ISmartContent object is about to begin.
        /// Overrides of this method can set the values within the passed ResizeOptions class
        /// to control various aspects of resizing behavior.
        /// </summary>
        /// <param name="content">SmartContent which is being resized.</param>
        /// <param name="options">Options which control resizing behavior.</param>
#pragma warning disable CC0057 // Unused parameters
        public virtual void OnResizeStart(
            [NotNull] ISmartContent content,
            [NotNull] ResizeOptions options)
#pragma warning restore CC0057 // Unused parameters
        {
        }

        /// <summary>
        /// Notification of real time resizing of the object (this will only be called if the
        /// ResizeCapabilities include real time resizing). The implementation of this method
        /// should update the ISmartContent object as appropriate based on the new size.
        /// The editor will first call this method and then call the GenerateEditorHtml
        /// method to update the display as the user resizes.
        /// </summary>
        /// <param name="content">SmartContent which is being resized.</param>
        /// <param name="newSize">New size of object (or of resizable element if the ResizeOptions.ResizableElementId option was specified).</param>
#pragma warning disable CC0057 // Unused parameters
        public virtual void OnResizing([NotNull] ISmartContent content, Size newSize)
#pragma warning restore CC0057 // Unused parameters
        {
        }
    }
}
