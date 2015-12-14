// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor
{
    public interface IPublishTimeWorker
    {
        /// <summary>
        /// IPublishTimeWorker can be implemented by plugins that want to do work during the publish workflow.
        /// For example this interface can be used by the photo album plugin to publish the photo album or by
        /// the video plugin if we wanted to publish the video to a web service when publishing and not in the background
        /// </summary>
        /// <param name="progressForm">The form that is showing progress to the user.  If this is null, the plugin cannot show status</param>
        /// <param name="content">The ISmartContent for what should be published</param>
        /// <param name="blogId">The blog id of that is being published too</param>
        /// <param name="publishingContext">Extra metadata about the contents of the post/email that is being published</param>
        /// <param name="context">An object be used as token to pass additional information </param>
        /// <returns></returns>
        string DoPublishWork(UpdateWeblogProgressForm progressForm, ISmartContent content, string blogId, IPublishingContext publishingContext, object context);
    }
}
