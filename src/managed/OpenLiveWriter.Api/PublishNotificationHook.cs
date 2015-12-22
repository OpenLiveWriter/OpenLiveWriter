// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Base class for plugins that wish to be notified before and after a post is
    /// uploaded to the server either as a published post or as a draft.
    /// </summary>
    /// <remarks>
    /// There is a single instance of a given PublishNotificationHook created
    /// for each Open Live Writer process. The implementation of PublishNotificationHook
    /// objects must therefore be stateless (the context required to carry out the
    /// responsibilities of the various methods are passed as parameters to the
    /// respective methods).
    /// </remarks>
    public abstract class PublishNotificationHook : WriterPlugin
    {
        /// <summary>
        /// Notifies the plugin that a post publish operation is about to be attempted,
        /// unless this plugin or another publishing notification plugin cancels the
        /// attempt.
        /// </summary>
        /// <param name="dialogOwner">Owner for any dialog boxes shown.</param>
        /// <param name="properties">Property-set that the plugin can use to get and set properties for this post.</param>
        /// <param name="publishingContext">Publishing context for HTML generation.</param>
        /// <param name="publish">If false, the post is being posted as a draft.</param>
        /// <returns>False to cancel the publish operation, otherwise true.</returns>
        public virtual bool OnPrePublish(
            IWin32Window dialogOwner,
            IProperties properties,
            IPublishingContext publishingContext,
            bool publish)
        {
            return true;
        }

        /// <summary>
        /// Notifies the plugin that a blog post was successfully published.
        /// </summary>
        /// <param name="dialogOwner">Owner for any dialog boxes shown.</param>
        /// <param name="properties">Property-set that the plugin can use to get and set properties for this post.</param>
        /// <param name="publishingContext">Publishing context for HTML generation.</param>
        /// <param name="publish">If false, the post was posted as a draft.</param>
        public virtual void OnPostPublish(
            IWin32Window dialogOwner,
            IProperties properties,
            IPublishingContext publishingContext,
            bool publish)
        {
        }
    }
}
