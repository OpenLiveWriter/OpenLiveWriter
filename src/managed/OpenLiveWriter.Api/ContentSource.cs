// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Xml;
using System.Windows.Forms;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// <para>Base class for plugins that wish to enable the insertion of custom HTML content into posts.
    /// The source of content to be inserted can any or all of the following: an Insert dialog,
    /// a URL, or LiveClipboard data.</para>
    /// <para>Implementors of this class should override the CreateContent method(s) corresponding to
    /// the content-sources they wish to support. Note also that each of the CreateContent methods
    /// has a corresponding class-level attribute that must be specified along with the override.</para>
    /// <para>There is a single instance of a given ContentSource created for each Open Live Writer
    /// process. The implementation of ContentSource objects must therefore be stateless (the
    /// context required to carry out the responsibilities of the various methods are passed as parameters to
    /// the respective methods).</para>
    /// </summary>
    public class ContentSource : WriterPlugin
    {
        /// <summary>
        /// Create content using an Insert dialog. Plugin classes which override this method must
        /// also be declared with the InsertableContentSourceAttribute.
        /// </summary>
        /// <param name="dialogOwner">Owner for any dialogs shown.</param>
        /// <param name="content">Newly created content.</param>
        /// <returns>DialogResult.OK if content was successfully created, DialogResult.Cancel
        /// if the user cancels the Insert dialog.</returns>
        /// <exception cref="ContentCreationException">Thrown if an error occurs during the creation of content.</exception>
        public virtual DialogResult CreateContent(IWin32Window dialogOwner, ref string content)
        {
            throw new NotImplementedException("ContentSource.CreateContent");
        }

        /// <summary>
        /// Create content using the contents of a LiveClipboard Xml document. Plugin classes which override
        /// this method must also be declared with the LiveClipboardContentSourceAttribute.
        /// </summary>
        /// <param name="dialogOwner">Owner for any dialogs shown.</param>
        /// <param name="lcDocument">LiveClipboard Xml document</param>
        /// <param name="newContent">Newly created content.</param>
        /// <returns>DialogResult.OK if content was successfully created, DialogResult.Cancel
        /// if the user cancels the Insert dialog.</returns>
        /// <exception cref="ContentCreationException">Thrown if an error occurs during the creation of content.</exception>
        public virtual DialogResult CreateContentFromLiveClipboard(IWin32Window dialogOwner, XmlDocument lcDocument, ref string newContent)
        {
            throw new NotImplementedException("ContentSource.CreateContentFromLiveClipboard");
        }

        /// <summary>
        /// Create content based on a URL. The source of this URL can either be the page the user was
        /// navigated to when they pressed the "Blog This" button or a URL that is pasted or dragged
        /// into the editor. Plugin classes which override this method must also be declared with
        /// the UrlContentSourceAttribute.
        /// </summary>
        /// <param name="url">Url to create content from.</param>
        /// <param name="title">Default title of post (used for "Blog This" case).</param>
        /// <param name="newContent">Newly created content</param>
        /// <exception cref="ContentCreationException">Thrown if an error occurs during the creation of content.</exception>
        public virtual void CreateContentFromUrl(string url, ref string title, ref string newContent)
        {
            throw new NotImplementedException("ContentSource.CreateContentFromUrl");
        }
    }
}
