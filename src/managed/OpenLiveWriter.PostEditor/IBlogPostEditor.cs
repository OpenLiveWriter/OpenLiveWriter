// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.PostEditor
{
    public interface IBlogPostEditor
    {
        /// <summary>
        /// Initialize the editor with the passed post and supporting files storage
        /// </summary>
        /// <param name="post">post</param>
        void Initialize(IBlogPostEditingContext editorContext, IBlogClientOptions clientOptions);

        /// <summary>
        /// Notification that the target weblog changed (adapt to capabilities, etc.)
        /// </summary>
        /// <param name="blog"></param>
        void OnBlogChanged(Blog newBlog);

        /// <summary>
        /// Notification that the blog settings changed
        /// </summary>
        void OnBlogSettingsChanged(bool templateChanged);

        /// <summary>
        /// Have there been any user edits since the last load or save?
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Save the current contents of the editor to the specified post
        /// </summary>
        void SaveChanges(BlogPost post, BlogPostSaveOptions options);

        /// <summary>
        /// Validate the editor's content for publishing
        /// </summary>
        /// <returns></returns>
        bool ValidatePublish();

        /// <summary>
        /// Notificaiton that a publish succeeded
        /// </summary>
        void OnPublishSucceeded(BlogPost blogPost, PostResult postResult);

        /// <summary>
        /// Notificaiton that the application is about to close
        /// </summary>
        /// <returns></returns>
        void OnClosing(CancelEventArgs e);

        /// <summary>
        /// Notificaiton that the post is about to close
        /// </summary>
        /// <returns></returns>
        void OnPostClosing(CancelEventArgs e);

        /// <summary>
        /// Notification that the application is now closed
        /// </summary>
        void OnClosed();

        /// <summary>
        /// Notification that current blog post is closed
        /// </summary>
        void OnPostClosed();
    }

    public class BlogPostSaveOptions
    {
        /// <summary>
        /// For AutoSave it's more important that the save be fast
        /// than the markup be well-formed, so when this flag is
        /// true the HTML will not be formatted in XHTML mode
        /// regardless of the blog settings.
        /// </summary>
        public bool AutoSave;

        public static BlogPostSaveOptions DefaultOptions
        {
            get { return new BlogPostSaveOptions(); }
        }
    }

    /// <summary>
    /// This class is a dummy that is put into the list of editors so that when a change is made
    /// external to any editor inside the manager, the manager has an editor to put the dirty flag to
    /// true.
    /// </summary>
    internal class ForceDirtyPostEditor : IBlogPostEditor
    {
        void IBlogPostEditor.Initialize(IBlogPostEditingContext editingContext, IBlogClientOptions clientOptions)
        {
            _isDirty = false;
        }

        void IBlogPostEditor.SaveChanges(BlogPost post, BlogPostSaveOptions options)
        {
            _isDirty = false;
        }

        bool IBlogPostEditor.ValidatePublish()
        {
            return true;
        }

        void IBlogPostEditor.OnPublishSucceeded(BlogPost blogPost, PostResult postResult)
        {
            _isDirty = false;
        }

        bool IBlogPostEditor.IsDirty
        {
            get
            {
                if ((ApplicationDiagnostics.AutomationMode || ApplicationDiagnostics.TestMode) && _isDirty)
                {
                    Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "ForceDirtyPostEditor Dirty Values {0}", _isDirty));
                }
                return _isDirty;
            }
        }

        void IBlogPostEditor.OnBlogChanged(Blog newBlog)
        {
        }

        public void OnBlogSettingsChanged(bool templateChanged)
        {
        }

        public void ForceDirty()
        {
            _isDirty = true;
        }

        private bool _isDirty = false;

        public void OnClosing(CancelEventArgs e)
        {
        }

        public void OnPostClosing(CancelEventArgs e)
        {
        }

        public void OnClosed() { }
        public void OnPostClosed() { }
    }
}
