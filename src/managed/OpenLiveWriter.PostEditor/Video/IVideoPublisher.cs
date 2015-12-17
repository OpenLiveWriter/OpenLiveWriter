// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.PostEditor.Video.VideoService;

namespace OpenLiveWriter.PostEditor.Video
{

    internal interface IVideoPublisher : IMediaPublisher
    {
        /// <summary>
        /// Once the video is ready to be published this function will be called
        /// with all the information the user has entered.  This function will only be called
        /// if the user has accepted the terms of service.
        /// </summary>
        /// <param name="path">The filesystem path to the video file.</param>
        IStatusWatcher Publish(string title, string description, string tags, string categoryId, string categoryString, string permissionValue, string permissionString, string path);
        /// <summary>
        /// Gets the information that will be used to represent the video in the blog post.
        /// </summary>
        /// <param name="title">The title of the video according to the user.</param>
        /// <param name="description">A description of the video according to the user</param>
        /// <param name="tags">A comma separated list of tags the user wants to apply to the video.</param>
        /// <param name="categoryId">The Id of the CategoryItem which was selected by the user</param>
        /// <param name="permissionValue">A string to represent the type of permission the user wants to have on the video.</param>
        /// <param name="error">
        /// If this is a string that string will be shown on a error message box if null is returned.  If it is "" then a generic error
        /// will be shown.  If it is null, no error will be shown.
        /// </param>
        /// <returns></returns>
        Video GetVideo(string title, string description, string tags, string categoryId, string categoryString, string permissionValue, string permissionString);
        /// <summary>
        /// A list of categories that a user can have their video be tagged under.
        /// </summary>
        /// <returns></returns>
        List<CategoryItem> Categories { get; }
        /// <summary>
        /// GetAcceptanceText should return the text that will appear next to the checkbox that the user
        /// must agree to before the video can be published.  Text within the { } of the string will
        /// be hyperlinked.
        /// </summary>
        /// <returns></returns>
        string AcceptanceText { get; }
        /// <summary>
        /// Returns the URL that the user should be directed to when they click the text that was encapsulated
        /// in the { } from the GetAcceptanceText method.
        /// </summary>
        /// <returns></returns>
        string AcceptanceUrl { get; }
        /// <summary>
        /// Returns the title of the url that will be seen in the link label for the terms of use
        /// </summary>
        /// <returns></returns>
        string AcceptanceTitle { get; }
        /// <summary>
        /// This will be used if we can identify the publisher after a post is loaded
        /// and we want to try and make a status watcher for the item.  If the publisher cannot
        /// make a new status watcher, perhaps because of authentication problems, then it can return null.
        /// </summary>
        /// <param name="video">
        /// A video object that contains as much information as we know about the video
        /// from the saved post.
        /// </param>
        /// <returns>
        /// If null is returned, the video element will be considered to be in an error state.
        /// We will use the video helper to try and take a snapshot of the video.
        /// </returns>
        IStatusWatcher CreateStatusWatcher(Video video);
        /// <summary>
        /// SafteyTip link label
        /// </summary>
        string SafetyTipUrl { get; }
        string SafetyTipTitle { get; }
        string FormatTags(string rawTags);
    }

    public class VideoPublishException : Exception
    {
        public VideoPublishException(string message) : base(message)
        {

        }
    }

    [Flags]
    public enum VideoPublishStatus
    {
        LocalProcessing = 1,
        RemoteProcessing = 2,
        Error = 4,
        Completed = 8
    }
    internal interface IStatusWatcher : IDisposable
    {
        PublishStatus Status { get; }
        bool IsCancelable { get; }
        void CancelPublish();
    }

    internal class PublishStatus
    {
        public readonly string DisplayMessage;
        public readonly VideoPublishStatus Status;
        public readonly string Id;

        internal PublishStatus(VideoPublishStatus status, string displayMessage, string id)
        {
            DisplayMessage = displayMessage;
            Status = status;
            Id = id;
        }
    }
}
