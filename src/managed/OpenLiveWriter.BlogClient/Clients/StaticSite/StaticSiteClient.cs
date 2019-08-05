using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;


namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    [BlogClient(StaticSiteClient.CLIENT_TYPE, StaticSiteClient.CLIENT_TYPE)]
    public class StaticSiteClient : BlogClientBase, IBlogClient
    {
        // The 'provider' concept doesn't really apply to local static sites
        // Store these required constants here so they're in one place
        public const string PROVIDER_ID = "D0E0062F-7540-4462-94FD-DC55004D95E6";
        public const string SERVICE_NAME = "Static Site Generator";
        public const string POST_API_URL = "http://localhost/"; // A valid URI is required for BlogClientManager to instantiate a URI object on.
        public const string CLIENT_TYPE = "StaticSite";

        public static Regex WEB_UNSAFE_CHARS = new Regex("[^A-Za-z0-9- ]*");

        public IBlogClientOptions Options { get; private set; }

        private StaticSiteConfig Config;

        public StaticSiteClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            Config = StaticSiteConfig.LoadConfigFromCredentials(credentials);
            
            // Set the client options
            var options = new BlogClientOptions();
            ConfigureClientOptions(options);
            Options = options;
        }

        protected override void VerifyCredentials(TransientCredentials transientCredentials)
        {

        }

        public void OverrideOptions(IBlogClientOptions newClientOptions)
        {
            Options = newClientOptions;
        }

        public BlogInfo[] GetUsersBlogs() => new BlogInfo[0];

        public BlogPostCategory[] GetCategories(string blogId) =>
            StaticSitePost.GetAllPosts(Config, false)
            .SelectMany(post => post.BlogPost.Categories.Select(cat => cat.Name))
            .Distinct()
            .Select(cat => new BlogPostCategory(cat))
            .ToArray();

        public BlogPostKeyword[] GetKeywords(string blogId) => new BlogPostKeyword[0];

        /// <summary>
        /// Returns recent posts
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="maxPosts"></param>
        /// <param name="includeCategories"></param>
        /// <param name="now">If null, then includes future posts.  If non-null, then only includes posts before the *UTC* 'now' time.</param>
        /// <returns></returns>
        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now) =>
            StaticSitePost.GetAllPosts(Config, true)
            .Select(post => post.BlogPost)
            .Where(post => post != null && (now == null || post.DatePublished < now))
            .OrderByDescending(post => post.DatePublished)
            .Take(maxPosts)
            .ToArray();

        public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            if(!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Static site does not support drafts, cannot post.");
                throw new BlogClientPostAsDraftUnsupportedException();
            }
            remotePost = null;
            etag = "";

            // Create a StaticSitePost on the provided post
            return DoNewItem(new StaticSitePost(Config, post, !publish));
        }

        public bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Static site does not support drafts, cannot post.");
                throw new BlogClientPostAsDraftUnsupportedException();
            }
            remotePost = null;
            etag = "";

            // Create a StaticSitePost on the provided post
            var ssgPost = new StaticSitePost(Config, post, !publish);

            if(ssgPost.FilePathById == null)
            {
                // If we are publishing and there exists a draft with this ID, delete it.
                if (publish)
                {
                    var filePath = new StaticSitePost(Config, post, true).FilePathById;
                    if (filePath != null) File.Delete(filePath);
                }

                // Existing post could not be found to edit, call NewPost instead;
                NewPost(blogId, post, newCategoryContext, publish, out etag, out remotePost);
                return true;
            }

            // Set slug to existing slug on post
            ssgPost.Slug = post.Slug;

            return DoEditItem(ssgPost);
        }

        /// <summary>
        /// Attempt to get a post with the specified id (note: may return null
        /// if the post could not be found on the remote server)
        /// </summary>
        public BlogPost GetPost(string blogId, string postId)
            => StaticSitePost.GetPostById(Config, postId).BlogPost;

        public void DeletePost(string blogId, string postId, bool publish)
        {
            var post = StaticSitePost.GetPostById(Config, postId);
            if (post == null) throw new BlogClientException(
                Res.Get(StringId.SSGErrorPostDoesNotExistTitle),
                Res.Get(StringId.SSGErrorPostDoesNotExistText));
            DoDeleteItem(post);
        }

        public BlogPost GetPage(string blogId, string pageId)
        {
            var page = StaticSitePage.GetPageById(Config, pageId);
            page.ResolveParent();
            return page.BlogPost;
        }

        public PageInfo[] GetPageList(string blogId) => 
            StaticSitePage.GetAllPages(Config).Select(page => page.PageInfo).ToArray();

        public BlogPost[] GetPages(string blogId, int maxPages) =>
            StaticSitePage.GetAllPages(Config)
            .Select(page => page.BlogPost)
            .OrderByDescending(page => page.DatePublished)
            .Take(maxPages)
            .ToArray();

        public string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            if(!publish)
            {
                Trace.Fail("Posting pages as drafts not yet implemented.");
                throw new BlogClientPostAsDraftUnsupportedException();
            }
            //if (!publish && !Options.SupportsPostAsDraft)
            //{
            //    Trace.Fail("Static site does not support drafts, cannot post.");
            //    throw new BlogClientPostAsDraftUnsupportedException();
            //}

            remotePost = null;
            etag = "";

            // Create a StaticSitePost on the provided page
            return DoNewItem(new StaticSitePage(Config, page));
        }

        public bool EditPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            if (!publish)
            {
                Trace.Fail("Posting pages as drafts not yet implemented.");
                throw new BlogClientPostAsDraftUnsupportedException();
            }
            //if (!publish && !Options.SupportsPostAsDraft)
            //{
            //    Trace.Fail("Static site does not support drafts, cannot post.");
            //    throw new BlogClientPostAsDraftUnsupportedException();
            //}
            remotePost = null;
            etag = "";

            // Create a StaticSitePage on the provided page
            var ssgPage = new StaticSitePage(Config, page);

            if (ssgPage.FilePathById == null)
            {
                // Existing page could not be found to edit, call NewPage instead;
                NewPage(blogId, page, publish, out etag, out remotePost);
                return true;
            }

            // Set slug to existing slug on page
            ssgPage.Slug = page.Slug;

            return DoEditItem(ssgPage);
        }

        public void DeletePage(string blogId, string pageId)
        {
            var page = StaticSitePage.GetPageById(Config, pageId);
            if (page == null) throw new BlogClientException(
                Res.Get(StringId.SSGErrorPageDoesNotExistTitle),
                Res.Get(StringId.SSGErrorPageDoesNotExistText));
            DoDeleteItem(page);
        }

        public AuthorInfo[] GetAuthors(string blogId) => throw new NotImplementedException();

        public bool? DoesFileNeedUpload(IFileUploadContext uploadContext) => null;

        public string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            string path = uploadContext.GetContentsLocalFilePath();
            return DoPostImage(path);
        }

        public void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        public string AddCategory(string blogId, BlogPostCategory category) =>
            throw new BlogClientMethodUnsupportedException("AddCategory");

        public BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
            => throw new BlogClientMethodUnsupportedException("SuggestCategories");

        /// <summary>
        /// Currently sends an UNAUTHENTICATED HTTP request. 
        /// If a static site requires authentication, this may be implemented here later.
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="timeoutMs"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter)
            => BlogClientHelper.SendAuthenticatedHttpRequest(requestUri, filter, (HttpWebRequest request) => {});

        public BlogInfo[] GetImageEndpoints() 
            => throw new NotImplementedException();

        /// <summary>
        /// Returns if this StaticSiteGeneratorClient is secure
        /// Returns true for now as we trust the user publish script
        /// </summary>
        public bool IsSecure => true;

        /// <summary>
        /// Remote detection is now possible as SendAuthenticatedHttpRequest has been implemented.
        /// </summary>
        public override bool RemoteDetectionPossible => true;

        // Authentication is handled by publish script at the moment 
        protected override bool RequiresPassword => false;

        #region StaticSiteItem generic methods

        /// <summary>
        /// Generic method to prepare and publish a new StaticSiteItem derived instance 
        /// </summary>
        /// <param name="item">a new StaticSiteItem derived instance</param>
        /// <returns>the new StaticSitePost ID</returns>
        private string DoNewItem(StaticSiteItem item)
        {
            // Ensure the post has an ID
            var newPostId = item.EnsureId();
            // Ensure the post has a date
            item.EnsureDatePublished();
            // Ensure the post has a safe slug
            item.EnsureSafeSlug();
            // Save the post to disk under it's new slug-based path
            item.SaveToFile(item.FilePathBySlug);

            try
            {
                // Build the site, if required
                if (Config.BuildCommand != string.Empty) DoSiteBuild();

                // Publish the site 
                DoSitePublish();

                return newPostId;
            }
            catch (Exception ex)
            {
                // Clean up our output file
                File.Delete(item.FilePathBySlug);
                // Throw the exception up
                throw ex;
            }

        }

        /// <summary>
        /// Generic method to edit an already-published StaticSiteItem derived instance 
        /// </summary>
        /// <param name="item">an existing StaticSiteItem derived instance</param>
        /// <returns>True if successful</returns>
        private bool DoEditItem(StaticSiteItem item)
        {
            // Copy the existing post to a temporary file
            var backupFileName = Path.GetTempFileName();
            File.Copy(item.FilePathById, backupFileName, true);

            bool renameOccurred = false;
            // Store the old file path and slug
            string oldPath = item.FilePathById;
            //string oldSlug = item.DiskSlugFromFilePathById;

            try
            {
                // Determine if the post file needs renaming (slug, date or parent change)
                if (item.FilePathById != item.FilePathBySlug)
                {
                    renameOccurred = true;

                    // Find a new safe slug for the post
                    item.Slug = item.FindNewSlug(item.Slug, safe: true);
                    // Remove the old file
                    File.Delete(oldPath);
                    // Save to the new file
                    item.SaveToFile(item.FilePathBySlug);
                }
                else
                {
                    // Save the post to disk based on it's existing id
                    item.SaveToFile(item.FilePathById);
                }

                // Build the site, if required
                if (Config.BuildCommand != string.Empty) DoSiteBuild();

                // Publish the site
                DoSitePublish();

                return true;
            }
            catch (Exception ex)
            {
                // Clean up the failed output
                if (renameOccurred)
                {
                    // Delete the rename target
                    File.Delete(item.FilePathBySlug);
                }
                else
                {
                    // Delete the original file
                    File.Delete(item.FilePathById);
                }

                // Copy the backup to the old location
                File.Copy(backupFileName, oldPath, overwrite: true);

                // Delete the backup
                File.Delete(backupFileName);

                // Throw the exception up
                throw ex;
            }
        }

        /// <summary>
        /// Delete a StaticSiteItem from disk, and publish the changes
        /// </summary>
        /// <param name="item">a StaticSiteItem</param>
        private void DoDeleteItem(StaticSiteItem item)
        {
            var backupFileName = Path.GetTempFileName();
            File.Copy(item.FilePathById, backupFileName, true);

            try
            {
                File.Delete(item.FilePathById);

                // Build the site, if required
                if (Config.BuildCommand != string.Empty) DoSiteBuild();

                // Publish the site
                DoSitePublish();
            }
            catch (Exception ex)
            {
                File.Copy(backupFileName, item.FilePathById, overwrite: true);
                File.Delete(backupFileName);

                // Throw the exception up
                throw ex;
            }
        }

        #endregion

        /// <summary>
        /// Copy image to images directory, returning the URL on site (eg. http://example.com/images/test.jpg)
        /// This method does not upload the image, it is assumed this will be done later on.
        /// </summary>
        /// <param name="filePath">Path to image on disk</param>
        /// <returns>URL to image on site</returns>
        private string DoPostImage(string filePath)
        {
            // Generate a unique file name 
            var fileExt = Path.GetExtension(filePath);
            string uniqueName = "";

            for (int i = 0; i <= 1000; i++)
            {
                uniqueName = Path.GetFileNameWithoutExtension(filePath).Replace(" ", "");

                if (i == 1000)
                {
                    // Failed to find a unique file name, return a GUID
                    uniqueName = Guid.NewGuid().ToString();
                    break;
                }
                if (i > 0) uniqueName += $"-{i}";
                if (!File.Exists(Path.Combine(Config.LocalSitePath, Config.ImagesPath, uniqueName + fileExt ))) break;
            }

            // Copy the image to the images path
            File.Copy(filePath, Path.Combine(Config.LocalSitePath, Config.ImagesPath, uniqueName + fileExt));

            // I attempted to return an absolute server path here, however other parts of OLW expect a fully formed URI
            // This may cause issue for users who decide to relocate their site to a different URL.
            // I also attempted to strip the protocol here, however C# does not think protocol-less URIs are valid
            return Path.Combine(Config.SiteUrl, Config.ImagesPath, uniqueName + fileExt).Replace("\\", "/");
        }

        /// <summary>
        /// Build the static site
        /// </summary>
        private void DoSiteBuild()
        {
            string stdout, stderr;
            var proc = RunSiteCommand(Config.BuildCommand, out stdout, out stderr);
            if (proc.ExitCode != 0)
            {
                throw new BlogClientException(
                    StringId.SSGBuildErrorTitle,
                    StringId.SSGBuildErrorText,
                    Res.Get(StringId.ProductNameVersioned),
                    proc.ExitCode.ToString(),
                    Config.ShowCmdWindows ? "N/A" : stdout,
                    Config.ShowCmdWindows ? "N/A" : stderr
                );
            }
        }

        /// <summary>
        /// Publish the static site
        /// </summary>
        private void DoSitePublish()
        {
            string stdout, stderr;
            var proc = RunSiteCommand(Config.PublishCommand, out stdout, out stderr);
            if (proc.ExitCode != 0)
            {
                throw new BlogClientException(
                    StringId.SSGPublishErrorTitle,
                    StringId.SSGPublishErrorText,
                    Res.Get(StringId.ProductNameVersioned),
                    proc.ExitCode.ToString(),
                    Config.ShowCmdWindows ? "N/A" : stdout,
                    Config.ShowCmdWindows ? "N/A" : stderr
                );
            }
        }

        /// <summary>
        /// Run a command from the site directory
        /// </summary>
        /// <param name="localCommand">Command to run, releative to site directory</param>
        /// <param name="stdout">String which will receive the command stdout</param>
        /// <param name="stderr">String which will receive the command stderr</param>
        /// <returns></returns>
        private Process RunSiteCommand(string localCommand, out string outStdout, out string outStderr)
        {
            var proc = new Process();
            string stdout = "";
            string stderr = "";

            // If a 32-bit process on a 64-bit system, call the 64-bit cmd
            proc.StartInfo.FileName = (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem) ? 
                $"{Environment.GetEnvironmentVariable("windir")}\\Sysnative\\cmd.exe" : // 32-on-64, launch sysnative cmd
                "cmd.exe"; // Launch regular cmd

            // Set working directory to local site path
            proc.StartInfo.WorkingDirectory = Config.LocalSitePath;

            proc.StartInfo.RedirectStandardInput = !Config.ShowCmdWindows;
            proc.StartInfo.RedirectStandardError = !Config.ShowCmdWindows;
            proc.StartInfo.RedirectStandardOutput = !Config.ShowCmdWindows;
            proc.StartInfo.CreateNoWindow = !Config.ShowCmdWindows;
            proc.StartInfo.UseShellExecute = false;
            
            proc.StartInfo.Arguments = $"/C {localCommand}";

            if(!Config.ShowCmdWindows)
            {

                proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        stdout += e.Data;
                        Trace.WriteLine($"StaticSiteClient stdout: {e.Data}");
                    }
                });

                proc.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        stderr += e.Data;
                        Trace.WriteLine($"StaticSiteClient stderr: {e.Data}");
                    }
                });
            }

            proc.Start();
            if(!Config.ShowCmdWindows)
            {
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
            }

            if(Config.CmdTimeoutMs < 0)
            {
                // If timeout is negative, timeout is disabled.
                proc.WaitForExit();
            } else
            {
                if (!proc.WaitForExit(Config.CmdTimeoutMs))
                {
                    // Timeout reached
                    try { proc.Kill(); } catch { } // Attempt to kill the process
                    throw new BlogClientException(
                        Res.Get(StringId.SSGErrorCommandTimeoutTitle),
                        Res.Get(StringId.SSGErrorCommandTimeoutText));
                }
            }

            // The caller will have all output waiting in outStdout and outStderr
            outStdout = stdout;
            outStderr = stderr;
            return proc;
        }

        /// <summary>
        /// Sets the relevant BlogClientOptions for this client based on values from the StaticSiteConfig
        /// </summary>
        /// <param name="clientOptions">A BlogClientOptions instance</param>
        private void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            clientOptions.SupportsPages = clientOptions.SupportsPageParent = Config.PagesEnabled;
            clientOptions.SupportsPostAsDraft = Config.DraftsEnabled;
            clientOptions.SupportsFileUpload = Config.ImagesEnabled;
            clientOptions.SupportsImageUpload = Config.ImagesEnabled ? SupportsFeature.Yes : SupportsFeature.No;
            clientOptions.SupportsScripts = clientOptions.SupportsEmbeds = SupportsFeature.Yes;
            clientOptions.SupportsExtendedEntries = true;

            // Blog template is downloaded from publishing a test post
            clientOptions.SupportsAutoUpdate = true;

            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsKeywords = false;

            clientOptions.FuturePublishDateWarning = true;
            clientOptions.SupportsCustomDate = clientOptions.SupportsCustomDateUpdate = true;
            clientOptions.SupportsSlug = true;
            clientOptions.SupportsAuthor = false;
        }
    }
}
