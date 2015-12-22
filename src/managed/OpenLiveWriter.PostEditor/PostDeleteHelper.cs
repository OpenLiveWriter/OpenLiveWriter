// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{
    public class PostDeleteHelper
    {
        public static bool SafeDeleteRemotePost(string blogId, string postId, bool isPage)
        {
            // screen non-existent blog ids
            if (!BlogSettings.BlogIdIsValid(blogId))
                return true;

            using (Blog blog = new Blog(blogId))
            {
                try
                {
                    if (blog.VerifyCredentials())
                    {
                        // try to delete the post on the remote blog
                        blog.DeletePost(postId, isPage, true);

                        // return success
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (BlogClientOperationCancelledException)
                {
                    // show no UI for operation cancelled
                    Debug.WriteLine("BlogClient operation cancelled");
                    return false;
                }
                catch (Exception ex)
                {
                    DisplayableExceptionDisplayForm.Show(Win32WindowImpl.ForegroundWin32Window, ex);
                    return false;
                }
            }
        }

        public static bool SafeDeleteLocalPost(FileInfo postFile)
        {
            try
            {
                PostEditorFile postEditorFile = PostEditorFile.GetExisting(postFile);
                if (postEditorFile != null)
                    postEditorFile.Delete();
                return true;
            }
            catch (Exception ex)
            {
                DisplayableException displayableException = new DisplayableException(
                    StringId.ErrorOccurredDeletingDraft, StringId.ErrorOccurredDeletingDraftDetails, ex.Message);
                DisplayableExceptionDisplayForm.Show(Win32WindowImpl.ForegroundWin32Window, displayableException);
                return false;
            }
        }

        public static bool SafeDeleteLocalPost(string blogId, string postId)
        {
            try
            {
                PostEditorFile post = PostEditorFile.FindPost(PostEditorFile.RecentPostsFolder, blogId, postId);
                if (post != null)
                {
                    post.Delete();
                }
                return true;
            }
            catch (Exception ex)
            {
                DisplayableException displayableException = new DisplayableException(
                    StringId.ErrorOccurredDeletingDraft, StringId.ErrorOccurredDeletingDraftDetails, ex.Message);
                DisplayableExceptionDisplayForm.Show(Win32WindowImpl.ForegroundWin32Window, displayableException);
                return false;
            }

        }
    }
}
