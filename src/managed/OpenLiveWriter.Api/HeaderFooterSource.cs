// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Base class for plugins that wish to add HTML to the beginning or end
    /// of a post when the post is being previewed or published.
    /// </summary>
    /// <remarks>
    /// There is a single instance of a given FooterPlugin created
    /// for each Open Live Writer process. The implementation of FooterPlugin
    /// objects must therefore be stateless (the context required to carry out the
    /// responsibilities of the various methods are passed as parameters to the
    /// respective methods).
    /// </remarks>
    public abstract class HeaderFooterSource : WriterPlugin
    {
        /// <summary>
        /// Determines whether content should be placed above or below the
        /// post body.
        /// </summary>
        public enum Position
        {
            /// <summary>
            /// Places content above the post body.
            /// </summary>
            Header,
            /// <summary>
            /// Places content below the post body.
            /// </summary>
            Footer
        };

        /// <summary>
        /// Subclasses should override this method and return true if a
        /// permalink is required for GeneratePublishFooter calls to
        /// work.
        /// </summary>
        /// <remarks>
        /// Since new posts don't have permalinks until after they are
        /// posted to the server, returning true may cause Writer to
        /// perform an extra publish operation. Therefore, only return
        /// true if it is absolutely necessary for your plugin to function
        /// correctly.
        /// </remarks>
        public virtual bool RequiresPermalink { get { return false; } }

        /// <summary>
        /// Generate the HTML that should be inserted at the beginning or end of
        /// the blog post when the editor is switched to Preview mode.
        /// </summary>
        /// <remarks>
        /// The returned HTML will be wrapped inside a <c>div</c>
        /// whose float and margins can be controlled by the <c>Layout</c>
        /// property on the <c>ISmartContent</c> parameter.
        /// </remarks>
        /// <param name="smartContent">Can be used to get/set properties for this post, add files for upload, and control the layout of the containing <c>div</c>.</param>
        /// <param name="publishingContext">Publishing context for HTML generation.</param>
        /// <param name="position">The position where the generated HTML should be inserted.</param>
        /// <returns>The HTML that should appear at the end of the blog post.</returns>
        public abstract string GeneratePreviewHtml(
            ISmartContent smartContent,
            IPublishingContext publishingContext,
            out Position position);

        /// <summary>
        /// Generate the HTML that should be inserted at the beginning or end of
        /// the blog post during publishing.
        /// </summary>
        /// <remarks>
        /// The returned HTML will be wrapped inside a <c>div</c>
        /// whose float and margins can be controlled by the <c>Layout</c>
        /// property on the <c>ISmartContent</c> parameter.
        /// </remarks>
        /// <param name="dialogOwner">Owner for any dialog boxes shown.</param>
        /// <param name="smartContent">Can be used to get/set properties for this post, add files for upload, and control the layout of the containing <c>div</c>.</param>
        /// <param name="publishingContext">Publishing context for HTML generation.</param>
        /// <param name="publish">If false, the post is being posted as a draft.</param>
        /// <param name="position">The position where the generated HTML should be inserted.</param>
        /// <returns>The HTML that should appear at the end of the blog post.</returns>
        public abstract string GeneratePublishHtml(
            IWin32Window dialogOwner,
            ISmartContent smartContent,
            IPublishingContext publishingContext,
            bool publish,
            out Position position);
    }
}
