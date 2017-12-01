// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;

namespace OpenLiveWriter.Api
{
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
        /// Create content using an Insert dialog. Plugin classes which override this method must
        /// also be declared with the InsertableContentSourceAttribute.
        /// </summary>
        /// <param name="dialogOwner">Owner for any dialogs shown.</param>
        /// <param name="newContent">SmartContent object which is populated with initial values by this method.</param>
        /// <returns>DialogResult.OK if content was successfully created, DialogResult.Cancel
        /// if the user cancels the Insert dialog.</returns>
        /// <exception cref="ContentCreationException">Thrown if an error occurs during the creation of content.</exception>
        public virtual DialogResult CreateContent(IWin32Window dialogOwner, ISmartContent newContent)
        {
            throw new NotImplementedException("SmartContentSource.CreateContent");
        }

        /// <summary>
        /// Create content using the contents of a LiveClipboard Xml document. Plugin classes which override
        /// this method must also be declared with the LiveClipboardContentSourceAttribute.
        /// </summary>
        /// <param name="dialogOwner">Owner for any dialogs shown.</param>
        /// <param name="lcDocument">LiveClipboard Xml document</param>
        /// <param name="newContent">SmartContent object which is populated with initial values by this method.</param>
        /// <returns>DialogResult.OK if content was successfully created, DialogResult.Cancel
        /// if the user cancels the Insert dialog.</returns>
        /// <exception cref="ContentCreationException">Thrown if an error occurs during the creation of content.</exception>
        public virtual DialogResult CreateContentFromLiveClipboard(IWin32Window dialogOwner, XmlDocument lcDocument, ISmartContent newContent)
        {
            throw new NotImplementedException("SmartContentSource.CreateContentFromLiveClipboard");
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
        public virtual void CreateContentFromUrl(string url, ref string title, ISmartContent newContent)
        {
            throw new NotImplementedException("SmartContentSource.CreateContentFromUrl");
        }

        /// <summary>
        /// Generate the HTML content which is used to represent the SmartContent item within
        /// the post editor. The default behavior for this method if it is not overridden is to
        /// call GeneratePublishHtml.
        /// </summary>
        /// <param name="content">SmartContent object to generate HTML for.</param>
        /// <param name="publishingContext">Publishing context for HTML generation.</param>
        /// <returns>HTML content to be inserted into the post editor.</returns>
        public virtual string GenerateEditorHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            return GeneratePublishHtml(content, publishingContext);
        }

        /// <summary>
        /// Generate the HTML content which is published to the destination weblog.
        /// </summary>
        /// <param name="content">SmartContent object to generate HTML for.</param>
        /// <param name="publishingContext">Publishing context for HTML generation.</param>
        /// <returns>HTML to be published to the destination weblog.</returns>
        public abstract string GeneratePublishHtml(ISmartContent content, IPublishingContext publishingContext);

        /// <summary>
        /// Create a new SmartContentEditor for this ContentSource. The SmartContentEditor is the control
        /// that appears in the Sidebar whenever a SmartContent object created by this content source
        /// is selected within the PostEditor. This method must be overridden by all subclasses of SmartContentSource.
        /// </summary>
        /// <param name="editorSite">Interface to the SmartContentEditor's site.</param>
        /// <returns>A new instance of a class derived from SmartContentEditor.</returns>
        public abstract SmartContentEditor CreateEditor(ISmartContentEditorSite editorSite);

        /// <summary>
        /// Resize capabilities for this SmartContentSource (defaults to ResizeCapabilities.None).
        /// Depending upon the flags specified in this return value the SmartContentSource should
        /// also override the appropriate OnResize methods to implement desired resizing behavior.
        /// </summary>
        public virtual ResizeCapabilities ResizeCapabilities
        {
            get { return ResizeCapabilities.None; }
        }

        /// <summary>
        /// Notification that the resizing of the specified ISmartContent object is about to begin.
        /// Overrides of this method can set the values within the passed ResizeOptions class
        /// to control various aspects of resizing behavior.
        /// </summary>
        /// <param name="content">SmartContent which is being resized.</param>
        /// <param name="options">Options which control resizing behavior.</param>
        public virtual void OnResizeStart(ISmartContent content, ResizeOptions options)
        {
        }

        /// <summary>
        /// Notification of realtime resizing of the object (this will only be called if the
        /// ResizeCapabilities include RealtimeResizing). The implementation of this method
        /// should update the ISmartContent object as appropriate based on the new size.
        /// The editor will first call this method and then call the GenerateEditorHtml
        /// method to update the display as the user resizes.
        /// </summary>
        /// <param name="content">SmartContent which is being resized.</param>
        /// <param name="newSize">New size of object (or of resizable element if the ResizeOptions.ResizableElementId option was specified).</param>
        public virtual void OnResizing(ISmartContent content, Size newSize)
        {
        }

        /// <summary>
        /// Notification that the sizing of an object is complete. The  implementation of
        /// this method should update the ISmartContent object as appropriate based on the
        /// new size. The editor will first call this method and then call the GenerateEditorHtml
        /// method to update the display based on the new size.
        /// </summary>
        /// <param name="content">SmartContent which is being resized.</param>
        /// <param name="newSize">New size of object (or of resizable element if the ResizeOptions.ResizableElementId option was specified).</param>
        public virtual void OnResizeComplete(ISmartContent content, Size newSize)
        {
        }
    }

    /// <summary>
    /// Resize capabilities for a SmartContentSource.
    /// </summary>
    [Flags]
    public enum ResizeCapabilities
    {
        /// <summary>
        /// SmartContentSource is not resizable (size grippers will not appear when the object is selected in the editor).
        /// </summary>
        None = 0,

        /// <summary>
        /// SmartContentSource is resizable (size grippers will appear when the object is selected within the editor).
        /// If this flag is specified as part of ResizeCapabilities then the OnResizeComplete method should also be
        /// overridden to update the ISmartContent as necessary with the new size of the SmartContent object.
        /// </summary>
        Resizable = 1,

        /// <summary>
        /// Preserve the aspect ratio of the object during resizing. The default aspect ratio to be enforced is the
        /// ratio of the object prior to resizing. If the desired aspect ratio is statically known it is highly recommended
        /// that this ratio be specified within an override of the OnResizeStart method (will eliminate the problem
        /// of "creeping" change to the aspect ratios with continued resizing).
        /// </summary>
        PreserveAspectRatio = 2,

        /// <summary>
        /// Update the appearance of the smart content object in realtime as the user resizes the object. If this
        /// flag is specified then the OnResizing method should be overridden to update the state of the ISmartContent
        /// object as resizing occurs. The editor will first call this method and then call the GenerateEditorHtml
        /// method to update the display as the user resizes.
        /// </summary>
        LiveResize = 4
    }

    /// <summary>
    /// Options which control resizing behavior.
    /// </summary>
    public class ResizeOptions
    {
        /// <summary>
        /// Specify the ID of an HTML element that should be used as the "target" for resizing. This is useful
        /// in the case where the ISmartContent object is principly represented by a single element (such as an image)
        /// but which also contains other elements (such as an image caption line). In this case proportional sizing
        /// should apply to the image rather than the entire object's HTML. If a ResizableElementId is specified then
        /// the Size parameters passed to the OnResize methods refer to the size of this element rather than to
        /// the size of the entire SmartContent object.
        /// </summary>
        public string ResizeableElementId
        {
            get { return _resizableElementId; }
            set { _resizableElementId = value; }
        }
        private string _resizableElementId = null;

        /// <summary>
        /// Aspect ratio to be enforced if the ResizeCapabilities.PreserveAspectRatio flag is specified. If the
        /// desired aspect ratio is statically known it is highly recommended that this ratio be specified within
        /// the OnResizeStart method (will eliminate the problem of "creeping" change to the aspect ratios with continued resizing).
        /// </summary>
        public double AspectRatio
        {
            get { return _aspectRatio; }
            set { _aspectRatio = value; }
        }
        private double _aspectRatio = 0;
    }

}
