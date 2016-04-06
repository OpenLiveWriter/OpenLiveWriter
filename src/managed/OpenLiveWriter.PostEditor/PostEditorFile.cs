// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Com.StructuredStorage;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.PostEditor.SupportingFiles;

namespace OpenLiveWriter.PostEditor
{

    public class PostEditorFile
    {

        #region Static Interface

        public static PostEditorFile CreateNew(DirectoryInfo targetDirectory)
        {
            return new PostEditorFile(targetDirectory);
        }

        public static PostEditorFile GetExisting(FileInfo file)
        {
            return new PostEditorFile(file);
        }

        public static PostEditorFile FindPost(DirectoryInfo directory, string blogId, string postId)
        {
            // Fix bug 551719: WLW Opens Random .wpost Files
            if (string.IsNullOrEmpty(blogId) || string.IsNullOrEmpty(postId))
                return null;

            FileInfo file = PostEditorFileLookupCache.Lookup(directory, "cache.xml", ReadFile, "*" + Extension, blogId, postId);
            if (file == null)
                return null;
            return GetExisting(file);
        }

        /// This is only public because the unit test needs to get to it
        public static PostEditorFileLookupCache.PostKey ReadFile(FileInfo file)
        {
            try
            {
                using (Storage postStorage = new Storage(file.FullName, StorageMode.Open, false))
                {
                    string blogId = ReadString(postStorage, DESTINATION_BLOG_ID);
                    string postId = ReadString(postStorage, POST_ID);
                    return new PostEditorFileLookupCache.PostKey(blogId, postId);
                }
            }
            catch (Exception e)
            {
                Debug.Fail(String.Format(CultureInfo.InvariantCulture, "Error opening post file [{0}]: {1}", file.FullName, e));
                return null;
            }
        }

        public static DirectoryInfo MyWeblogPostsFolder
        {
            get
            {
                return new DirectoryInfo(ApplicationEnvironment.MyWeblogPostsFolder);
            }
        }

        public static DirectoryInfo DraftsFolder
        {
            get
            {
                return new DirectoryInfo(Path.Combine(MyWeblogPostsFolder.FullName, "Drafts"));
            }
        }

        public static DirectoryInfo RecentPostsFolder
        {
            get
            {
                return new DirectoryInfo(Path.Combine(MyWeblogPostsFolder.FullName, "Recent Posts"));
            }
        }

        public static PostInfo[] GetRecentPosts(DirectoryInfo directory, RecentPostRequest request)
        {
            try
            {
                ArrayList blogPosts = new ArrayList();
                FileInfo[] files = directory.GetFiles("*" + Extension);
                Array.Sort(files, new FileInfoByModifiedDateComparer());

                int filesReturned = 0;
                foreach (FileInfo file in files)
                {
                    // check for exceeding maximum number of posts
                    if (!request.AllPosts && filesReturned++ >= request.NumberOfPosts)
                        break;

                    try
                    {
                        // get post
                        blogPosts.Add(GetPostInfo(file));
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Unexpected exception getting Recent post info [{0}]:{1}", file.Name, e.ToString()));
                    }
                }

                return (PostInfo[])blogPosts.ToArray(typeof(PostInfo));

            }
            catch (Exception ex)
            {
                throw PostEditorStorageException.Create(ex);
            }
        }

        public static PostInfo GetPostInfo(FileInfo file)
        {
            try
            {
                // get post
                using (Storage postStorage = new Storage(file.FullName, StorageMode.Open, false))
                {
                    PostInfo postInfo = new PostInfo();
                    postInfo.Id = file.FullName;
                    postInfo.Title = ReadString(postStorage, POST_TITLE);
                    postInfo.Permalink = SafeReadString(postStorage, POST_PERMALINK, String.Empty);
                    postInfo.IsPage = SafeReadBoolean(postStorage, POST_ISPAGE, false);
                    postInfo.BlogName = ReadBlogName(postStorage);
                    postInfo.BlogId = ReadString(postStorage, DESTINATION_BLOG_ID);
                    postInfo.BlogPostId = ReadString(postStorage, POST_ID);
                    postInfo.Contents = ReadStringUtf8(postStorage, POST_CONTENTS);
                    postInfo.DateModified = file.LastWriteTimeUtc;
                    return postInfo;
                }
            }
            catch (Exception ex)
            {
                throw PostEditorStorageException.Create(ex);
            }
        }

        public static bool IsValid(string filePath)
        {
            try
            {
                if (filePath == null || filePath == String.Empty)
                    return false;

                filePath = Kernel32.GetLongPathName(filePath);

                // first check the extension
                if (string.Compare(Path.GetExtension(filePath), Extension, true, CultureInfo.InvariantCulture) == 0)
                {
                    // check the CLSID of the file
                    using (Storage storage = new Storage(filePath, StorageMode.Open, false))
                    {
                        // check file version
                        if (storage.Clsid == Version1FormatCLSID || storage.Clsid == Version2FormatCLSID)
                        {
                            // ensure that this isn't a storage with no streams!
                            try
                            {
                                ReadString(storage, POST_ID);
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception testing PostEditorFile format: " + ex.ToString());
                return false;
            }
        }

        #endregion

        #region Initialization Helpers

        private PostEditorFile(DirectoryInfo targetDirectory)
        {
            TargetDirectory = targetDirectory;

            ListenForDirectoryChanges();
        }

        private void ListenForDirectoryChanges()
        {
            var preferences = PostEditorPreferences.Instance;

            preferences.PreferencesChanged -= PreferencesOnPreferencesChanged;
            preferences.PreferencesChanged += PreferencesOnPreferencesChanged;
        }

        private void PreferencesOnPreferencesChanged(object sender, EventArgs e)
        {
            if (TargetDirectory?.FullName != PostEditorSettings.AutoSaveDirectory)
            {
                TargetDirectory = new DirectoryInfo(PostEditorPreferences.Instance.WeblogPostsFolder);
            }
        }

        private PostEditorFile(FileInfo file)
        {
            TargetFile = file;
            ListenForDirectoryChanges();
        }

        // auto-create drafts and recent-posts directories
        public static void Initialize()
        {
            try
            {
                if (!DraftsFolder.Exists)
                    DraftsFolder.Create();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could not access drafts folder: " + ex);
                throw new DirectoryException(DraftsFolder.FullName);

            }

            try
            {
                if (!RecentPostsFolder.Exists)
                    RecentPostsFolder.Create();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could not access recent posts folder: " + ex);
                throw new DirectoryException(RecentPostsFolder.FullName);
            }
        }

        #endregion

        #region File Properties

        public bool IsSaved
        {
            get
            {
                return TargetFile != null;
            }
        }

        public bool IsDeleted
        {
            get
            {
                return IsSaved && !File.Exists(TargetFile.FullName);
            }
        }

        public DateTime DateSaved
        {
            get
            {
                if (IsSaved)
                    return TargetFile.LastWriteTimeUtc;
                else
                    return DateTime.MinValue;
            }
        }

        public bool IsDraft
        {
            get
            {
                return TargetDirectory.FullName.ToLower(CultureInfo.CurrentCulture) == DraftsFolder.FullName.ToLower(CultureInfo.CurrentCulture);
            }
        }

        public bool IsRecentPost
        {
            get
            {
                return TargetDirectory.FullName.ToLower(CultureInfo.CurrentCulture) == RecentPostsFolder.FullName.ToLower(CultureInfo.CurrentCulture);
            }
        }

        #endregion

        #region Loading/Saving/Deleting

        public IBlogPostEditingContext Load()
        {
            return Load(true);
        }

        public IBlogPostEditingContext Load(bool addToRecentDocs)
        {
            try
            {
                if (!IsSaved)
                    throw new InvalidOperationException("Attempted to load a PostEditorFile that has never been saved!");

                // add to shell recent documents
                if (addToRecentDocs)
                    Shell32.SHAddToRecentDocs(SHARD.PATHW, TargetFile.FullName);

                using (Storage postStorage = new Storage(TargetFile.FullName, StorageMode.Open, false))
                {
                    // meta-data
                    string destinationBlogId = ReadString(postStorage, DESTINATION_BLOG_ID);
                    string serverSupportingFileDirectory = ReadString(postStorage, SERVER_SUPPORTING_FILE_DIR);

                    // blog post
                    BlogPost blogPost = new BlogPost();
                    blogPost.Id = ReadString(postStorage, POST_ID);
                    blogPost.IsPage = SafeReadBoolean(postStorage, POST_ISPAGE, false);
                    blogPost.Title = ReadString(postStorage, POST_TITLE);
                    blogPost.Categories = (BlogPostCategory[])ReadXml(postStorage, POST_CATEGORIES, new XmlReadHandler(ReadCategories));
                    blogPost.NewCategories = (BlogPostCategory[])SafeReadXml(postStorage, POST_NEW_CATEGORIES, new XmlReadHandler(ReadCategories), new BlogPostCategory[] { });
                    blogPost.DatePublished = ReadDateTime(postStorage, POST_DATEPUBLISHED);
                    blogPost.DatePublishedOverride = ReadDateTime(postStorage, POST_DATEPUBLISHED_OVERRIDE);
                    blogPost.CommentPolicy = ReadCommentPolicy(postStorage);
                    blogPost.TrackbackPolicy = ReadTrackbackPolicy(postStorage);
                    blogPost.Keywords = ReadString(postStorage, POST_KEYWORDS);
                    blogPost.Excerpt = ReadString(postStorage, POST_EXCERPT);
                    blogPost.Permalink = SafeReadString(postStorage, POST_PERMALINK, String.Empty);
                    blogPost.PingUrlsPending = (string[])ReadXml(postStorage, POST_PINGURLS_PENDING, new XmlReadHandler(ReadPingUrls));
                    blogPost.PingUrlsSent = (string[])SafeReadXml(postStorage, POST_PINGURLS_SENT, new XmlReadHandler(ReadPingUrls), new string[0]);
                    blogPost.Slug = SafeReadString(postStorage, POST_SLUG, String.Empty);
                    blogPost.Password = SafeReadString(postStorage, POST_PASSWORD, String.Empty);
                    string authorId = SafeReadString(postStorage, POST_AUTHOR_ID, String.Empty);
                    string authorName = SafeReadString(postStorage, POST_AUTHOR_NAME, String.Empty);
                    blogPost.Author = new PostIdAndNameField(authorId, authorName);
                    string pageParentId = SafeReadString(postStorage, POST_PAGE_PARENT_ID, String.Empty);
                    string pageParentName = SafeReadString(postStorage, POST_PAGE_PARENT_NAME, String.Empty);
                    blogPost.PageParent = new PostIdAndNameField(pageParentId, pageParentName);
                    blogPost.PageOrder = SafeReadString(postStorage, POST_PAGE_ORDER, String.Empty);
                    blogPost.ETag = SafeReadString(postStorage, POST_ETAG, String.Empty);
                    blogPost.AtomRemotePost = (XmlDocument)SafeReadXml(postStorage, POST_ATOM_REMOTE_POST, new XmlReadHandler(XmlDocReadHandler), null);

                    try
                    {
                        blogPost.ContentsVersionSignature = ReadString(postStorage, POST_CONTENTS_VERSION_SIGNATURE);
                    }
                    catch (StorageFileNotFoundException) { } //BACKWARDS_COMPATABILITY: occurs if this file was created before the introduction of content signatures (pre-Beta2)

                    // post contents (must extract supporting files -- protect against leakage with try/catch
                    BlogPostSupportingFileStorage supportingFileStorage = new BlogPostSupportingFileStorage();
                    using (Storage postSupportStorage = postStorage.OpenStorage(POST_SUPPORTING_FILES, StorageMode.Open, false))
                    {
                        SupportingFilePersister supportingFilePersister = new SupportingFilePersister(postSupportStorage, supportingFileStorage);

                        //read the attached files
                        SupportingFileService supportingFileService = new SupportingFileService(supportingFileStorage);
                        try
                        {
                            ReadXml(postStorage, POST_ATTACHED_FILES, new XmlReadHandler(new AttachedFileListReader(supportingFileService, supportingFilePersister).ReadAttachedFileList));
                        }
                        catch (StorageFileNotFoundException) { } //occurs if this file was created before the introduction of extension data

                        //read in the image data (note: this must happen before fixing the file references)
                        BlogPostImageDataList imageDataList = (BlogPostImageDataList)ReadXml(postStorage, POST_IMAGE_FILES, new XmlReadHandler(new ImageListReader(supportingFilePersister, supportingFileService).ReadImageFiles));

                        //read the extension data settings
                        BlogPostExtensionDataList extensionDataList = new BlogPostExtensionDataList(supportingFileService);
                        try
                        {
                            ReadXml(postStorage, POST_EXTENSION_DATA_LIST, new XmlReadHandler(new ExtensionDataListReader(extensionDataList, supportingFilePersister, supportingFileService).ReadExtensionDataList));
                        }
                        catch (StorageFileNotFoundException) { } //occurs if this file was created before the introduction of extension data

                        //fix up the HTML content to reference the extracted files
                        blogPost.Contents = supportingFilePersister.FixupHtmlReferences(ReadStringUtf8(postStorage, POST_CONTENTS));

                        string originalSourcePath = SafeReadString(postStorage, ORIGINAL_SOURCE_PATH, null);
                        PostEditorFile autoSaveFile;
                        PostEditorFile file = GetFileFromSourcePath(originalSourcePath, out autoSaveFile);

                        // return init params
                        return new BlogPostEditingContext(destinationBlogId, blogPost, file, autoSaveFile, serverSupportingFileDirectory, supportingFileStorage, imageDataList, extensionDataList, supportingFileService);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception type in PostEditorFile.Load. It is critical that only IO exceptions occur at this level of the system so please check the code which threw the exeption and see if there is a way to behave more robustly!\r\n"
                    + ex.ToString());
                throw PostEditorStorageException.Create(ex);
            }
        }

        private PostEditorFile GetFileFromSourcePath(string originalSourcePath, out PostEditorFile autoSaveFile)
        {
            if (string.IsNullOrEmpty(originalSourcePath))
            {
                autoSaveFile = null;
                return this;
            }

            if (Directory.Exists(originalSourcePath))
            {
                autoSaveFile = this;
                return new PostEditorFile(new DirectoryInfo(originalSourcePath));
            }

            if (File.Exists(originalSourcePath))
            {
                autoSaveFile = this;
                return new PostEditorFile(new FileInfo(originalSourcePath));
            }

            Trace.WriteLine("Original source path no longer exists: " + originalSourcePath);
            autoSaveFile = this;
            return new PostEditorFile(DraftsFolder);
        }

        /// <summary>
        /// Saves state to this file that includes changes the user has made
        /// but not saved to autoSaveSourceFile.
        /// </summary>
        /// <param name="editingContext"></param>
        /// <param name="autoSaveSourceFile">The original blog post file that
        /// this AutoSave operation is storing changes for.</param>
        public void AutoSave(IBlogPostEditingContext editingContext, PostEditorFile autoSaveSourceFile)
        {
            SaveCore(editingContext, autoSaveSourceFile, ManagePostFilePath(editingContext.BlogPost.IsPage, editingContext.BlogPost.Title));
        }

        public void SaveBlogPost(IBlogPostEditingContext editingContext)
        {
            string filePath = ManagePostFilePath(editingContext.BlogPost.IsPage, editingContext.BlogPost.Title);
            SaveCore(editingContext, null, filePath);
            Shell32.SHAddToRecentDocs(SHARD.PATHW, filePath);
        }

        public void SaveContentEditorFile(IBlogPostEditingContext editingContext, string fileNameOverride, bool addToRecentDocs)
        {
            SaveCore(editingContext, null, fileNameOverride);
        }

        private void SaveCore(IBlogPostEditingContext editingContext, PostEditorFile autoSaveSourceFile, string filePath)
        {
            // did this file exist prior to the attempt to save (if no, we need to delete
            // it if an exceptoin occurs -- otherwise we leave a "zombie" post file with
            // no available streams
            bool isPreviouslyUnsaved = !IsSaved;

            try
            {
                try
                {
                    // alias blog-post
                    BlogPost blogPost = editingContext.BlogPost;
                    Debug.Assert(!blogPost.IsTemporary, "Saving temporary style detection post!?");

                    // write out all of the fields
                    using (Storage postStorage = new Storage(filePath, StorageMode.OpenOrCreate, true))
                    {
                        // file-format clsid
                        postStorage.Clsid = Version2FormatCLSID;

                        // meta-data
                        WriteString(postStorage, DESTINATION_BLOG_ID, editingContext.BlogId);
                        WriteString(postStorage, SERVER_SUPPORTING_FILE_DIR, editingContext.ServerSupportingFileDirectory);

                        // blog post
                        WriteString(postStorage, POST_ID, blogPost.Id);
                        WriteBoolean(postStorage, POST_ISPAGE, blogPost.IsPage);
                        WriteString(postStorage, POST_TITLE, blogPost.Title);
                        WriteXml(postStorage, POST_CATEGORIES, blogPost.Categories, new XmlWriteHandler(WriteCategories));
                        WriteXml(postStorage, POST_NEW_CATEGORIES, blogPost.NewCategories, new XmlWriteHandler(WriteCategories));
                        WriteDateTime(postStorage, POST_DATEPUBLISHED, blogPost.DatePublished);
                        WriteDateTime(postStorage, POST_DATEPUBLISHED_OVERRIDE, blogPost.DatePublishedOverride);
                        WriteCommentPolicy(postStorage, blogPost.CommentPolicy);
                        WriteTrackbackPolicy(postStorage, blogPost.TrackbackPolicy);
                        WriteString(postStorage, POST_KEYWORDS, blogPost.Keywords);
                        WriteString(postStorage, POST_EXCERPT, blogPost.Excerpt);
                        WriteString(postStorage, POST_PERMALINK, blogPost.Permalink);
                        WriteString(postStorage, POST_LINK, blogPost.Permalink); // write for legacy compatability with beta 1
                        WriteXml(postStorage, POST_PINGURLS_PENDING, blogPost.PingUrlsPending, new XmlWriteHandler(WritePingUrls));
                        WriteXml(postStorage, POST_PINGURLS_SENT, blogPost.PingUrlsSent, new XmlWriteHandler(WritePingUrls));
                        WriteString(postStorage, POST_SLUG, blogPost.Slug);
                        WriteString(postStorage, POST_PASSWORD, blogPost.Password);
                        WriteString(postStorage, POST_AUTHOR_ID, blogPost.Author.Id);
                        WriteString(postStorage, POST_AUTHOR_NAME, blogPost.Author.Name);
                        WriteString(postStorage, POST_PAGE_PARENT_ID, blogPost.PageParent.Id);
                        WriteString(postStorage, POST_PAGE_PARENT_NAME, blogPost.PageParent.Name);
                        WriteString(postStorage, POST_PAGE_ORDER, blogPost.PageOrder);
                        WriteString(postStorage, POST_ETAG, blogPost.ETag);
                        WriteXml(postStorage, POST_ATOM_REMOTE_POST, blogPost.AtomRemotePost, new XmlWriteHandler(XmlDocWriteHandler));

                        //save the post info hash
                        WriteString(postStorage, POST_CONTENTS_VERSION_SIGNATURE, blogPost.ContentsVersionSignature);

                        // contents (with fixups for local files)
                        SupportingFilePersister supportingFilePersister = new SupportingFilePersister(postStorage.OpenStorage(POST_SUPPORTING_FILES, StorageMode.Create, true));
                        //BlogPostReferenceFixedHandler fixedReferenceHandler = new BlogPostReferenceFixedHandler(editingContext.ImageDataList);
                        //string fixedUpPostContents = supportingFilePersister.SaveFilesAndFixupReferences(blogPost.Contents, new ReferenceFixedCallback(fixedReferenceHandler.HandleReferenceFixed)) ;

                        //write the attached file data
                        //supportingFilePersister.
                        SupportingFileReferenceList referenceList = SupportingFileReferenceList.CalculateReferencesForSave(editingContext);
                        WriteXml(postStorage, POST_ATTACHED_FILES, null, new XmlWriteHandler(new AttachedFileListWriter(supportingFilePersister, editingContext, referenceList).WriteAttachedFileList));

                        WriteXml(postStorage, POST_IMAGE_FILES, editingContext.ImageDataList, new XmlWriteHandler(new AttachedImageListWriter(referenceList).WriteImageFiles));

                        //write the extension data
                        WriteXml(postStorage, POST_EXTENSION_DATA_LIST, editingContext.ExtensionDataList, new XmlWriteHandler(new ExtensionDataListWriter(supportingFilePersister, blogPost.Contents).WriteExtensionDataList));

                        //Convert file references in the HTML contents to the new storage path
                        string fixedUpPostContents = supportingFilePersister.FixupHtmlReferences(blogPost.Contents);
                        WriteStringUtf8(postStorage, POST_CONTENTS, fixedUpPostContents);

                        string originalSourcePath = autoSaveSourceFile == null ? ""
                            : autoSaveSourceFile.IsSaved ? autoSaveSourceFile.TargetFile.FullName
                            : autoSaveSourceFile.TargetDirectory.FullName;
                        WriteStringUtf8(postStorage, ORIGINAL_SOURCE_PATH, originalSourcePath);

                        // save to storage
                        postStorage.Commit();

                        // mark file as saved
                        TargetFile = new FileInfo(filePath);
                    }
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception type in PostEditorFile.Save. It is critical that only IO exceptions occur at this level of the system so please check the code which threw the exeption and see if there is a way to behave more robustly!\r\n"
                        + ex.ToString());
                    throw PostEditorStorageException.Create(ex);
                }
            }
            catch
            {
                // if we had no file previously and an exception occurs then
                // we need to delete the file
                if (isPreviouslyUnsaved && File.Exists(filePath))
                    try { File.Delete(filePath); }
                    catch { }

                throw;
            }
        }

        private void XmlDocWriteHandler(XmlTextWriter writer, object data)
        {
            XmlDocument xmlDoc = (XmlDocument)data;
            if (data == null)
            {
                writer.WriteStartElement("null");
                writer.WriteEndElement();
            }
            else
            {
                xmlDoc.DocumentElement.WriteTo(writer);
            }
        }

        private object XmlDocReadHandler(XmlTextReader reader)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reader);
            XmlElement docEl = xmlDoc.DocumentElement;
            if (docEl.Name == "null" && docEl.NamespaceURI == "")
                return null;
            return xmlDoc;
        }

        public void Delete()
        {
            try
            {
                if (IsSaved)
                {
                    TargetFile.Delete();
                }
                TargetFile = null;
            }
            catch (IOException ex)
            {
                throw PostEditorStorageException.Create(ex);
            }
        }

        #endregion

        #region Identity

        public override bool Equals(object obj)
        {
            FileInfo compareToTargetFile = (obj as PostEditorFile).TargetFile;
            if (compareToTargetFile == null && TargetFile == null)
                return true;
            else if (compareToTargetFile == null || TargetFile == null)
                return false;
            else
                return compareToTargetFile.FullName.Equals(TargetFile.FullName);
        }

        public override int GetHashCode()
        {
            if (TargetFile == null)
                return 0;
            else
                return TargetFile.GetHashCode();
        }

        #endregion

        #region Helpers for managing post file names, etc.

        private string ManagePostFilePath(bool isPage, string postTitle)
        {
            // ideally what should the filename be for this post?
            string targetFilePath = Path.Combine(TargetDirectory.FullName, FileNameForTitle(isPage, postTitle));

            // if this is a new unsaved post then make sure it has a unique name
            // within the target directory
            if (!IsSaved)
            {
                return GetUniqueFileName(targetFilePath);
            }

            // if the post is already saved but needs to be saved under a new title
            // then manage uniqueness then rename the file
            else if (targetFilePath != TargetFile.FullName)
            {
                // first manage uniqueness
                targetFilePath = GetUniqueFileName(targetFilePath);

                // actually rename the file and update the internal reference
                File.Move(TargetFile.FullName, targetFilePath);
                TargetFile = new FileInfo(targetFilePath);

                // return the new filename
                return targetFilePath;
            }

            // post already saved with the correct title, just return the name
            else
            {
                return targetFilePath;
            }
        }

        private string GetUniqueFileName(string targetFilePath)
        {
            // manage filename uniqueness based on existing contents of the directory
            StringBuilder buffer = new StringBuilder(Kernel32.MAX_PATH);
            string folderSpec = Path.GetDirectoryName(targetFilePath);
            if (!folderSpec.EndsWith("\\"))
                folderSpec += "\\";
            string fileSpec = Path.GetFileName(targetFilePath);
            // attempt to manage uniqueness of the name within the target directory

            // GetNonConflictingPath returns the name and creates a file to holds its place.
            // The delete removes that duplicated file.
            string uniqueName = PathHelper.GetNonConflictingPath(targetFilePath);
            File.Delete(uniqueName);

            return uniqueName;
            /*
            if ( Shell32.PathYetAnotherMakeUniqueName(buffer, folderSpec, null, fileSpec) )
            {
                return buffer.ToString() ;
            }
            else
            {
                // NOTE: if this fails for some unforseen reason then we may end up overwriting
                // an existing file of the same name -- alternative would be to throw an exception
                // we don't expect it to ever fail (and we will log it if it does) so for now just
                // let it fail with a silent overwrite
                Trace.Fail( "Unexpected error attempting to manage uniqueness for file: " + targetFilePath +
                    "\r\nError: " + Kernel32.GetLastError().ToString());

                return targetFilePath ;
            }
            */
        }

        private string FileNameForTitle(bool isPage, string postTitle)
        {
            // default name for untitled posts
            if (postTitle == String.Empty)
                postTitle = isPage ? PostInfo.UntitledPage : PostInfo.UntitledPost;

            return Path.ChangeExtension(FileHelper.GetValidFileName(postTitle), Extension);
        }

        private class FileInfoByModifiedDateComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return -(x as FileInfo).LastWriteTimeUtc.CompareTo((y as FileInfo).LastWriteTimeUtc);
            }
        }

        #endregion

        #region Helpers for reading and writing data

        private static Encoding ucs16Encoding = new UnicodeEncoding(false, true);
        private static Encoding utf8Encoding = new UTF8Encoding(true, false);

        private static void WriteString(Storage postStorage, string fieldName, string fieldValue)
        {
            using (StreamWriter writer = new StreamWriter(postStorage.OpenStream(fieldName, StorageMode.Create, true), ucs16Encoding))
                writer.Write(fieldValue);
        }

        private static string ReadString(Storage postStorage, string fieldName)
        {

            using (StreamReader reader = new StreamReader(postStorage.OpenStream(fieldName, StorageMode.Open, false), ucs16Encoding))
                return reader.ReadToEnd();
        }

        private static string SafeReadString(Storage postStorage, string fieldName, string defaultValue)
        {
            try
            {
                return ReadString(postStorage, fieldName);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static void WriteBoolean(Storage postStorage, string fieldName, bool fieldValue)
        {
            if (fieldValue)
                WriteString(postStorage, fieldName, "1");
            else
                WriteString(postStorage, fieldName, "0");
        }

        private static bool SafeReadBoolean(Storage postStorage, string fieldName, bool defaultValue)
        {
            try
            {
                return ReadBoolean(postStorage, fieldName);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static bool ReadBoolean(Storage postStorage, string fieldName)
        {
            string booleanValue = ReadString(postStorage, fieldName);
            return (booleanValue == "1");
        }

        private static void WriteStringUtf8(Storage postStorage, string fieldName, string fieldValue)
        {
            using (StreamWriter writer = new StreamWriter(postStorage.OpenStream(fieldName, StorageMode.Create, true), utf8Encoding))
                writer.Write(fieldValue);
        }

        private static string ReadStringUtf8(Storage postStorage, string fieldName)
        {

            using (StreamReader reader = new StreamReader(postStorage.OpenStream(fieldName, StorageMode.Open, false), utf8Encoding))
                return reader.ReadToEnd();
        }

        private static DateTime ReadDateTime(Storage postStorage, string fieldName)
        {
            using (BinaryReader reader = new BinaryReader(postStorage.OpenStream(fieldName, StorageMode.Open, false)))
                return new DateTime(reader.ReadInt64());
        }

        private static void WriteDateTime(Storage postStorage, string fieldName, DateTime fieldValue)
        {
            using (BinaryWriter writer = new BinaryWriter(postStorage.OpenStream(fieldName, StorageMode.Create, true)))
                writer.Write(fieldValue.Ticks);
        }

        private static void WriteXml(Storage postStorage, string streamName, object data, XmlWriteHandler writeHandler)
        {
            XmlTextWriter writer = new XmlTextWriter(postStorage.OpenStream(streamName, StorageMode.Create, true), utf8Encoding);
            try
            {
                writer.WriteStartDocument();
                writeHandler(writer, data);
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Close();
            }
        }

        private static object ReadXml(Storage postStorage, string streamName, XmlReadHandler readHandler)
        {
            XmlTextReader reader = new XmlTextReader(new StreamReader(postStorage.OpenStream(streamName, StorageMode.Open, false), utf8Encoding, false));
            try
            {
                return readHandler(reader);
            }
            finally
            {
                reader.Close();
            }
        }

        private delegate void XmlWriteHandler(XmlTextWriter writer, object data);
        private delegate object XmlReadHandler(XmlTextReader reader);

        private static object SafeReadXml(Storage postStorage, string streamName, XmlReadHandler readHandler, object defaultValue)
        {
            try
            {
                return ReadXml(postStorage, streamName, readHandler);
            }
            catch
            {
                return defaultValue;
            }
        }

        private void WriteCommentPolicy(Storage postStorage, BlogCommentPolicy commentPolicy)
        {
            switch (commentPolicy)
            {
                case BlogCommentPolicy.Unspecified:
                    WriteString(postStorage, POST_COMMENT_POLICY, COMMENT_POLICY_UNSPECIFIED);
                    break;
                case BlogCommentPolicy.None:
                    WriteString(postStorage, POST_COMMENT_POLICY, COMMENT_POLICY_NONE);
                    break;
                case BlogCommentPolicy.Closed:
                    WriteString(postStorage, POST_COMMENT_POLICY, COMMENT_POLICY_CLOSED);
                    break;
                case BlogCommentPolicy.Open:
                    WriteString(postStorage, POST_COMMENT_POLICY, COMMENT_POLICY_OPEN);
                    break;
                default:
                    Debug.Fail("Unexpected BlogCommentPolicy");
                    WriteString(postStorage, POST_COMMENT_POLICY, COMMENT_POLICY_UNSPECIFIED);
                    break;
            }
        }

        private BlogCommentPolicy ReadCommentPolicy(Storage postStorage)
        {
            string commentPolicy = ReadString(postStorage, POST_COMMENT_POLICY);
            switch (commentPolicy)
            {
                case COMMENT_POLICY_UNSPECIFIED:
                    return BlogCommentPolicy.Unspecified;
                case COMMENT_POLICY_NONE:
                    return BlogCommentPolicy.None;
                case COMMENT_POLICY_OPEN:
                    return BlogCommentPolicy.Open;
                case COMMENT_POLICY_CLOSED:
                    return BlogCommentPolicy.Closed;
                default:
                    Debug.Fail("Unexpected BlogCommentPolicy");
                    return BlogCommentPolicy.Unspecified;
            }
        }

        private void WriteTrackbackPolicy(Storage postStorage, BlogTrackbackPolicy trackbackPolicy)
        {
            switch (trackbackPolicy)
            {
                case BlogTrackbackPolicy.Unspecified:
                    WriteString(postStorage, POST_TRACKBACK_POLICY, TRACKBACK_POLICY_UNSPECIFIED);
                    break;
                case BlogTrackbackPolicy.Allow:
                    WriteString(postStorage, POST_TRACKBACK_POLICY, TRACKBACK_POLICY_ALLOW);
                    break;
                case BlogTrackbackPolicy.Deny:
                    WriteString(postStorage, POST_TRACKBACK_POLICY, TRACKBACK_POLICY_DENY);
                    break;
                default:
                    Debug.Fail("Unexpected Trackback policy");
                    WriteString(postStorage, POST_TRACKBACK_POLICY, TRACKBACK_POLICY_UNSPECIFIED);
                    break;
            }
        }

        private BlogTrackbackPolicy ReadTrackbackPolicy(Storage postStorage)
        {
            string trackbackPolicy = ReadString(postStorage, POST_TRACKBACK_POLICY);
            switch (trackbackPolicy)
            {
                case TRACKBACK_POLICY_UNSPECIFIED:
                    return BlogTrackbackPolicy.Unspecified;
                case TRACKBACK_POLICY_ALLOW:
                    return BlogTrackbackPolicy.Allow;
                case TRACKBACK_POLICY_DENY:
                    return BlogTrackbackPolicy.Deny;
                default:
                    Debug.Fail("Unexpected trackback policy");
                    return BlogTrackbackPolicy.Unspecified;
            }
        }

        private void WritePingUrls(XmlTextWriter writer, object pingUrls)
        {
            writer.WriteStartElement(PING_URLS_ELEMENT);
            foreach (string pingUrl in (pingUrls as string[]))
            {
                writer.WriteStartElement(PING_URL_ELEMENT);
                writer.WriteString(pingUrl);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private object ReadPingUrls(XmlTextReader reader)
        {
            ArrayList pingUrls = new ArrayList();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == PING_URL_ELEMENT)
                    pingUrls.Add(reader.ReadString());
            }
            return pingUrls.ToArray(typeof(string)) as string[];
        }

        private void WriteCategories(XmlTextWriter writer, object categories)
        {
            writer.WriteStartElement(CATEGORIES_ELEMENT);
            foreach (BlogPostCategory category in (categories as BlogPostCategory[]))
            {
                writer.WriteStartElement(CATEGORY_ELEMENT);
                writer.WriteAttributeString(CATEGORY_ID_ATTRIBUTE, category.Id);
                writer.WriteAttributeString(CATEGORY_NAME_ATTRIBUTE, category.Name);
                writer.WriteAttributeString(CATEGORY_PARENT_ATTRIBUTE, category.Parent);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private object ReadCategories(XmlTextReader reader)
        {
            ArrayList categories = new ArrayList();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == CATEGORY_ELEMENT)
                {
                    string id = null, name = null, parent = String.Empty;
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToAttribute(i);
                        switch (reader.LocalName)
                        {
                            case CATEGORY_ID_ATTRIBUTE:
                                id = reader.Value;
                                break;
                            case CATEGORY_NAME_ATTRIBUTE:
                                name = reader.Value;
                                break;
                            case CATEGORY_PARENT_ATTRIBUTE:
                                parent = reader.Value;
                                break;
                        }
                    }
                    categories.Add(new BlogPostCategory(id, name, parent));
                }
            }
            return categories.ToArray(typeof(BlogPostCategory));
        }

        private class AttachedImageListWriter
        {
            SupportingFileReferenceList _referenceList;
            public AttachedImageListWriter(SupportingFileReferenceList referenceList)
            {
                _referenceList = referenceList;
            }

            private bool IsReferencedImage(BlogPostImageData imageInfo)
            {
                ImageFileData[] imageFileArray = new ImageFileData[] { imageInfo.InlineImageFile, imageInfo.LinkedImageFile };
                foreach (ImageFileData imageFile in imageFileArray)
                {
                    if (imageFile != null && _referenceList.IsReferenced(imageFile.SupportingFile))
                    {
                        return true;
                    }
                }
                return false;
            }
            public void WriteImageFiles(XmlTextWriter writer, object imageFiles)
            {
                writer.WriteStartElement(IMAGE_FILES_ELEMENT);
                foreach (BlogPostImageData imageInfo in (imageFiles as BlogPostImageDataList))
                {
                    if (IsReferencedImage(imageInfo))
                    {
                        ImageFileData[] imageFileArray = new ImageFileData[] { imageInfo.ImageSourceFile, imageInfo.InlineImageFile, imageInfo.LinkedImageFile, imageInfo.ImageSourceShadowFile };
                        writer.WriteStartElement(IMAGE_FILE_ELEMENT);

                        for (int i = 0; i < imageFileArray.Length; i++)
                        {
                            ImageFileData imageFile = imageFileArray[i];
                            if (imageFile != null)
                            {
                                writer.WriteStartElement(SUPPORTING_FILE_LINK_ELEMENT);
                                writer.WriteAttributeString(SUPPORTING_FILE_LINK_FILE_ID_ATTRIBUTE, imageFile.SupportingFile.FileId);
                                writer.WriteEndElement(); //end SUPPORTING_FILE_LINK_ELEMENT
                            }
                        }

                        //write out the image decorator settings
                        WriteBlogPostSettingsBag(writer, imageInfo.ImageDecoratorSettings, IMAGE_DECORATORS_SETTINGSBAG_NAME);

                        writer.WriteStartElement(IMAGE_UPLOAD_ELEMENT);
                        WriteNonNullAttribute(writer, IMAGE_UPLOAD_SERVICE_ID_ATTRIBUTE, imageInfo.UploadInfo.ImageServiceId);
                        WriteBlogPostSettingsBag(writer, imageInfo.UploadInfo.Settings, IMAGE_UPLOAD_SETTINGSBAG_NAME);
                        writer.WriteEndElement(); //end IMAGE_UPLOAD_ELEMENT

                        writer.WriteEndElement(); //end IMAGE_FILE_ELEMENT
                    }
                }
                writer.WriteEndElement();	//end IMAGE_FILES_ELEMENT
            }
        }

        class ImageListReader
        {
            private SupportingFileService _fileService;
            SupportingFilePersister _supportingFilePersister;
            public ImageListReader(SupportingFilePersister supportingFilePersister, SupportingFileService fileService)
            {
                _supportingFilePersister = supportingFilePersister;
                _fileService = fileService;
            }
            public object ReadImageFiles(XmlTextReader reader)
            {
                BlogPostImageDataList imageFiles = new BlogPostImageDataList();
                BlogPostImageData blogPostImageData = null;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.LocalName == IMAGE_FILE_ELEMENT)
                        {
                            blogPostImageData = new BlogPostImageData();
                        }
                        else if (reader.LocalName == IMAGE_FILE_LINK_ELEMENT || reader.LocalName == SUPPORTING_FILE_LINK_ELEMENT)
                        {
                            if (blogPostImageData == null)
                                throw PostEditorStorageException.Create(new StorageInvalidFormatException());

                            ImageFileData imageFileData;
                            if (reader.LocalName == IMAGE_FILE_LINK_ELEMENT) //BACKWARDS_COMPATABILITY: used for backward compatibility with pre-Beta2 Writer files
                            {
                                ISupportingFile supportingFile;
                                string storagePath = reader.GetAttribute(IMAGE_FILE_LINK_PATH_ATTRIBUTE);

                                //the old image data saves a "corrupted" lowercase version of the embedded URL,
                                //so we need to take that into account and fix it
                                bool embeddedFile =
                                    storagePath.StartsWith(SupportingFilePersister.SUPPORTING_FILE_PREFIX.ToLower(CultureInfo.InvariantCulture));
                                if (embeddedFile)
                                {
                                    //fix up the corrupted storage path URL so that the reference fixer can resolve it.
                                    storagePath = storagePath.Replace(SupportingFilePersister.SUPPORTING_FILE_PREFIX.ToLower(CultureInfo.InvariantCulture),
                                        SupportingFilePersister.SUPPORTING_FILE_PREFIX).TrimEnd('/');

                                    //convert the internal storage path to the local temp path
                                    storagePath = _supportingFilePersister.LoadFilesAndFixupReferences(new string[] { storagePath })[0];

                                    //register the supporting file
                                    supportingFile = _fileService.CreateSupportingFileFromStoragePath(Path.GetFileName(storagePath), storagePath);
                                }
                                else
                                {
                                    //register the linked supporting file
                                    supportingFile = _fileService.AddLinkedSupportingFileReference(new Uri(storagePath));
                                }

                                imageFileData = new ImageFileData(supportingFile);
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    switch (reader.LocalName)
                                    {
                                        case IMAGE_FILE_LINK_WIDTH_ATTRIBUTE:
                                            imageFileData.Width = Int32.Parse(reader.Value, CultureInfo.InvariantCulture);
                                            break;
                                        case IMAGE_FILE_LINK_HEIGHT_ATTRIBUTE:
                                            imageFileData.Height = Int32.Parse(reader.Value, CultureInfo.InvariantCulture);
                                            break;
                                        case IMAGE_FILE_LINK_RELATIONSHIP_ATTRIBUTE:
                                            imageFileData.Relationship = (ImageFileRelationship)ImageFileRelationship.Parse(typeof(ImageFileRelationship), reader.Value);
                                            break;
                                        case IMAGE_FILE_LINK_PUBLISH_URL_ATTRIBUTE: //BACKWARDS_COMPATABILITY: not compatible with Beta2
                                            //imageFileData.PublishedUri = new Uri(reader.Value);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                string fileId = reader.GetAttribute(SUPPORTING_FILE_LINK_FILE_ID_ATTRIBUTE);
                                ISupportingFile supportingFile = _fileService.GetFileById(fileId);
                                if (supportingFile != null)
                                {
                                    imageFileData = new ImageFileData(supportingFile);
                                }
                                else
                                {
                                    Debug.Fail("Invalid post file state detected: image is referencing a file that is not attached");
                                    imageFileData = null;
                                }
                            }
                            if (imageFileData != null)
                            {
                                if (imageFileData.Relationship == ImageFileRelationship.Inline)
                                    blogPostImageData.InlineImageFile = imageFileData;
                                else if (imageFileData.Relationship == ImageFileRelationship.Linked)
                                    blogPostImageData.LinkedImageFile = imageFileData;
                                else if (imageFileData.Relationship == ImageFileRelationship.Source)
                                    blogPostImageData.ImageSourceFile = imageFileData;
                                else if (imageFileData.Relationship == ImageFileRelationship.SourceShadow)
                                    blogPostImageData.ImageSourceShadowFile = imageFileData;
                                else
                                    Debug.Fail("Unknown image file relationship detected: " + imageFileData.Relationship.ToString());
                            }
                        }
                        else if (reader.LocalName == IMAGE_UPLOAD_ELEMENT)
                        {
                            for (int i = 0; i < reader.AttributeCount; i++)
                            {
                                reader.MoveToAttribute(i);
                                switch (reader.LocalName)
                                {
                                    case IMAGE_UPLOAD_SERVICE_ID_ATTRIBUTE:
                                        blogPostImageData.UploadInfo.ImageServiceId = reader.Value;
                                        break;
                                }
                            }
                        }
                        else if (reader.LocalName == SETTINGS_BAG_ELEMENT)
                        {
                            BlogPostSettingsBag settings = new BlogPostSettingsBag();
                            string settingsBagName = ReadBlogPostSettingsBag(reader, settings);
                            if (settingsBagName == IMAGE_DECORATORS_SETTINGSBAG_NAME)
                            {
                                blogPostImageData.ImageDecoratorSettings = settings;
                            }
                            else if (settingsBagName == IMAGE_UPLOAD_SETTINGSBAG_NAME)
                            {
                                blogPostImageData.UploadInfo.Settings = settings;
                            }
                            else
                                Debug.Fail("Unknown settings bag encountered: " + settingsBagName);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == IMAGE_FILE_ELEMENT)
                    {
                        //initialize the shadow file if one doesn't already exist. (necessary for pre-beta2 post files)
                        blogPostImageData.InitShadowFile(_fileService);

                        imageFiles.AddImage(blogPostImageData);
                        blogPostImageData = null;
                    }
                }
                return imageFiles;
            }
        }

        private class ExtensionDataListWriter
        {
            SupportingFilePersister _supportingFilePersister;
            string _content;
            public ExtensionDataListWriter(SupportingFilePersister supportingFilePersister, string content)
            {
                _supportingFilePersister = supportingFilePersister;
                _content = content;
            }

            public void WriteExtensionDataList(XmlTextWriter writer, object extensionDataList)
            {
                BlogPostExtensionDataList exList = (BlogPostExtensionDataList)extensionDataList;
                writer.WriteStartElement(EXTENSION_DATA_LIST_ELEMENT);
                IExtensionData[] extensionDatas = exList.CalculateReferencedExtensionData(_content);
                foreach (BlogPostExtensionData exData in extensionDatas)
                {
                    writer.WriteStartElement(EXTENSION_DATA_ELEMENT);
                    writer.WriteAttributeString("id", exData.Id);
                    WriteBlogPostSettingsBag(writer, exData.Settings, EXTENSION_DATA_SETTINGSBAG_NAME);

                    foreach (string fileId in exData.FileIds)
                    {
                        ISupportingFile file = exData.GetSupportingFile(fileId);
                        if (file != null)
                        {
                            writer.WriteStartElement(EXTENSION_DATA_FILE_ELEMENT);
                            writer.WriteAttributeString(EXTENSION_DATA_FILE_ID_ATTRIBUTE, fileId);
                            writer.WriteAttributeString(EXTENSION_DATA_SUPPORTING_FILE_ID_ATTRIBUTE, file.FileId);
                            writer.WriteEndElement();
                        }
                        else
                            Trace.Fail("Invalid reference to supporting plugin file detected");
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        private class ExtensionDataListReader
        {
            BlogPostExtensionDataList _extensionDataList;
            SupportingFilePersister _supportingFilePersister;
            SupportingFileService _fileService;
            public ExtensionDataListReader(BlogPostExtensionDataList extensionDataList, SupportingFilePersister supportingFilePersister, SupportingFileService fileService)
            {
                _extensionDataList = extensionDataList;
                _supportingFilePersister = supportingFilePersister;
                _fileService = fileService;
            }

            public object ReadExtensionDataList(XmlTextReader reader)
            {
                ArrayList extensionErrors = null;
                while (reader.Read())
                {
                    if (reader.LocalName == EXTENSION_DATA_LIST_ELEMENT)
                    {
                    }
                    else if (reader.LocalName == EXTENSION_DATA_ELEMENT)
                    {
                        try
                        {
                            ReadExtensionData(reader);
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(e.ToString());
                            if (extensionErrors == null)
                                extensionErrors = new ArrayList();
                            extensionErrors.Add(e);
                        }
                    }

                    Trace.Assert(extensionErrors == null, "Failure(s) while to read extension data");
                }
                return null;
            }

            public BlogPostExtensionData ReadExtensionData(XmlTextReader reader)
            {
                Debug.Assert(reader.LocalName == EXTENSION_DATA_ELEMENT, "Xml reader is improperly positioned");
                int depth = 0;
                string extensionId = reader.GetAttribute("id");
                BlogPostExtensionData extensionData = (BlogPostExtensionData)_extensionDataList.CreateExtensionData(extensionId);
                if (!reader.IsEmptyElement)
                {
                    depth = 1;
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            bool isEmptyElement = reader.IsEmptyElement;
                            depth++;
                            if (reader.LocalName == SETTINGS_BAG_ELEMENT)
                            {
                                ReadBlogPostSettingsBag(reader, extensionData.Settings);

                                //this element was completely handled, and the stack was reset to its end element,
                                //so unset the depth and re-start the loop
                                depth--;
                                continue;
                            }
                            else if (reader.LocalName == EXTENSION_DATA_FILE_ELEMENT)
                            {
                                string fileId = reader.GetAttribute(EXTENSION_DATA_FILE_ID_ATTRIBUTE);
                                string supportingFileId = reader.GetAttribute(EXTENSION_DATA_SUPPORTING_FILE_ID_ATTRIBUTE);
                                ISupportingFile supportingFile;
                                if (supportingFileId == null) //BACKWARDS_COMPATABILITY: required for pre-beta2 Writer files
                                {
                                    //required
                                    string storagePath = reader.GetAttribute(EXTENSION_DATA_FILE_PATH_ATTRIBUTE);

                                    //fixup the reference path
                                    storagePath = _supportingFilePersister.LoadFilesAndFixupReferences(new string[] { storagePath })[0];
                                    supportingFile = _fileService.CreateSupportingFileFromStoragePath(fileId, storagePath);
                                }
                                else
                                {
                                    supportingFile = _fileService.GetFileById(supportingFileId);
                                }

                                extensionData.AddStorageFileMapping(fileId, supportingFile);
                            }
                            if (isEmptyElement)
                                depth--;
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            depth--;
                        }

                        if (depth == 0)
                            break;
                    }
                }
                Debug.Assert(depth == 0 && reader.LocalName == EXTENSION_DATA_ELEMENT, "Xmlreader is unexpectedly positioned (probably read to far!)");
                return extensionData;
            }
        }

        public static string ATTACHED_FILE_LIST_ELEMENT = "AttachedFiles";
        public static string ATTACHED_FILE_ELEMENT = "AttachedFile";
        public static string ATTACHED_FILE_VERSION_ELEMENT = "AttachedFileVersion";
        public static string ATTACHED_FILE_ID = "id";
        public static string ATTACHED_FILE_NAME = "FileName";
        public static string ATTACHED_FILE_NAME_UNIQUETOKEN = "FileNameUniqueToken";
        public static string ATTACHED_FILE_EMBEDDED_ATTRIBUTE = "Embedded";
        public static string ATTACHED_FILE_VERSION_ATTRIBUTE = "CurrentVersion";
        public static string ATTACHED_FILE_NEXT_VERSION_ATTRIBUTE = "NextVersion";
        public static string ATTACHED_FILE_UPLOAD_ELEMENT = "UploadInfo";
        public static string ATTACHED_FILE_UPLOAD_CONTEXT_ATTRIBUTE = "ContextId";
        public static string ATTACHED_FILE_UPLOAD_VERSION_ATTRIBUTE = "Version";
        public static string ATTACHED_FILE_UPLOAD_URI_ATTRIBUTE = "UploadUri";
        public static string ATTACHED_FILE_URI_ATTRIBUTE = "FileUri";
        private class AttachedFileListWriter
        {
            SupportingFilePersister _supportingFilePersister;
            IBlogPostEditingContext _editingContext;
            SupportingFileReferenceList _referenceList;
            public AttachedFileListWriter(SupportingFilePersister supportingFilePersister, IBlogPostEditingContext editingContext, SupportingFileReferenceList referenceList)
            {
                _supportingFilePersister = supportingFilePersister;
                _editingContext = editingContext;
                _referenceList = referenceList;
            }

            public void WriteAttachedFileList(XmlTextWriter writer, object arg)
            {
                SupportingFileService exList = (SupportingFileService)_editingContext.SupportingFileService;

                writer.WriteStartElement(ATTACHED_FILE_LIST_ELEMENT);
                foreach (string fileId in exList.GetFileFactoryIds())
                {
                    SupportingFileFactory fileFactory = exList.GetFileFactory(fileId);
                    if (_referenceList.IsReferenced(fileFactory))
                    {
                        writer.WriteStartElement(ATTACHED_FILE_ELEMENT);
                        writer.WriteAttributeString(ATTACHED_FILE_ID, fileFactory.FileId);
                        writer.WriteAttributeString(ATTACHED_FILE_NAME, fileFactory.FileName);
                        writer.WriteAttributeString(ATTACHED_FILE_NEXT_VERSION_ATTRIBUTE, fileFactory.NextVersion.ToString(CultureInfo.InvariantCulture));
                        foreach (SupportingFileFactory.VersionedSupportingFile supportingFile in fileFactory.GetVersionedFiles())
                        {
                            if (_referenceList.IsReferenced(supportingFile))
                            {
                                writer.WriteStartElement(ATTACHED_FILE_VERSION_ELEMENT);
                                writer.WriteAttributeString(ATTACHED_FILE_NAME, supportingFile.FileName);
                                writer.WriteAttributeString(ATTACHED_FILE_NAME_UNIQUETOKEN, supportingFile.FileNameUniqueToken);

                                writer.WriteAttributeString(ATTACHED_FILE_VERSION_ATTRIBUTE, supportingFile.FileVersion.ToString(CultureInfo.InvariantCulture));
                                writer.WriteAttributeString(ATTACHED_FILE_EMBEDDED_ATTRIBUTE, supportingFile.Embedded.ToString());
                                string storagePath = UrlHelper.SafeToAbsoluteUri(supportingFile.FileUri);
                                if (supportingFile.Embedded)
                                    storagePath = _supportingFilePersister.SaveFilesAndFixupReferences(new string[] { storagePath })[0];
                                writer.WriteAttributeString(ATTACHED_FILE_URI_ATTRIBUTE, storagePath);
                                WriteBlogPostSettingsBag(writer, supportingFile.Settings, null);

                                writer.WriteEndElement(); //end ATTACHED_FILE_VERSION_ELEMENT
                            }
                        }

                        foreach (string uploadContext in fileFactory.GetAllUploadContexts())
                        {
                            ISupportingFileUploadInfo uploadInfo = fileFactory.GetUploadInfo(uploadContext);
                            writer.WriteStartElement(ATTACHED_FILE_UPLOAD_ELEMENT);

                            writer.WriteAttributeString(ATTACHED_FILE_UPLOAD_CONTEXT_ATTRIBUTE, uploadContext);
                            if (uploadInfo.UploadUri != null)
                                writer.WriteAttributeString(ATTACHED_FILE_UPLOAD_URI_ATTRIBUTE, UrlHelper.SafeToAbsoluteUri(uploadInfo.UploadUri));
                            if (uploadInfo.UploadedFileVersion != -1)
                                writer.WriteAttributeString(ATTACHED_FILE_UPLOAD_VERSION_ATTRIBUTE, uploadInfo.UploadedFileVersion.ToString(CultureInfo.InvariantCulture));

                            WriteBlogPostSettingsBag(writer, uploadInfo.UploadSettings, null);

                            writer.WriteEndElement(); //end ATTACHED_FILE_UPLOAD_ELEMENT
                        }

                        writer.WriteEndElement(); //end ATTACHED_FILE_ELEMENT
                    }
                }
                writer.WriteEndElement();
            }
        }

        private class AttachedFileListReader
        {
            SupportingFileService _fileService;
            SupportingFilePersister _supportingFilePersister;
            public AttachedFileListReader(SupportingFileService supportingFileService, SupportingFilePersister supportingFilePersister)
            {
                _fileService = supportingFileService;
                _supportingFilePersister = supportingFilePersister;
            }

            public object ReadAttachedFileList(XmlTextReader reader)
            {
                ArrayList extensionErrors = null;
                try
                {
                    while (reader.Read())
                    {
                        if (reader.LocalName == ATTACHED_FILE_LIST_ELEMENT)
                        {
                        }
                        else if (reader.LocalName == ATTACHED_FILE_ELEMENT)
                        {
                            ReadFileData(reader);
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    if (extensionErrors == null)
                        extensionErrors = new ArrayList();
                    extensionErrors.Add(e);
                }

                Trace.Assert(extensionErrors == null, "Failure(s) while reading attached file data");
                return null;
            }

            public SupportingFileFactory ReadFileData(XmlTextReader reader)
            {
                Debug.Assert(reader.LocalName == ATTACHED_FILE_ELEMENT, "Xml reader is improperly positioned");
                int depth = 0;

                string fileId = reader.GetAttribute(ATTACHED_FILE_ID);
                string fileName = reader.GetAttribute(ATTACHED_FILE_NAME);
                string nextFileVersionString = reader.GetAttribute(ATTACHED_FILE_NEXT_VERSION_ATTRIBUTE);
                int nextFileVersion = 1;
                if (nextFileVersionString != null)
                    nextFileVersion = Int32.Parse(nextFileVersionString, CultureInfo.InvariantCulture);

                SupportingFileFactory fileFactory = _fileService.CreateSupportingFile(fileId, fileName, nextFileVersion);

                if (!reader.IsEmptyElement)
                {
                    depth = 1;
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            bool isEmptyElement = reader.IsEmptyElement;
                            depth++;

                            if (reader.LocalName == ATTACHED_FILE_VERSION_ELEMENT)
                            {
                                ReadFileVersionData(reader, fileFactory);

                                //this element was completely handled, and the stack was reset to its end element,
                                //so unset the depth and re-start the loop
                                depth--;
                                continue;
                            }
                            else if (reader.LocalName == ATTACHED_FILE_UPLOAD_ELEMENT)
                            {
                                ReadUploadInfoData(reader, fileFactory);

                                //this element was completely handled, and the stack was reset to its end element,
                                //so unset the depth and re-start the loop
                                depth--;
                                continue;
                            }
                            if (isEmptyElement)
                                depth--;
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            depth--;
                        }

                        if (depth == 0)
                            break;
                    }
                }

                Debug.Assert(depth == 0 && reader.LocalName == ATTACHED_FILE_ELEMENT, "Xmlreader is unexpectedly positioned (probably read to far!)");
                return fileFactory;
            }

            public void ReadUploadInfoData(XmlTextReader reader, SupportingFileFactory fileFactory)
            {
                Debug.Assert(reader.LocalName == ATTACHED_FILE_UPLOAD_ELEMENT, "Xml reader is improperly positioned");
                int depth = 0;

                string contextId = reader.GetAttribute(ATTACHED_FILE_UPLOAD_CONTEXT_ATTRIBUTE);
                string uploadUri = reader.GetAttribute(ATTACHED_FILE_UPLOAD_URI_ATTRIBUTE);
                string uploadVersionString = reader.GetAttribute(ATTACHED_FILE_UPLOAD_VERSION_ATTRIBUTE);
                int uploadVersion = -1;
                if (uploadVersionString != null)
                    uploadVersion = Int32.Parse(uploadVersionString, CultureInfo.InvariantCulture);

                //fixup the reference path
                BlogPostSettingsBag settings = new BlogPostSettingsBag();
                if (!reader.IsEmptyElement)
                {
                    depth = 1;
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            bool isEmptyElement = reader.IsEmptyElement;
                            depth++;
                            if (reader.LocalName == SETTINGS_BAG_ELEMENT)
                            {
                                ReadBlogPostSettingsBag(reader, settings);

                                //this element was completely handled, and the stack was reset to its end element,
                                //so unset the depth and re-start the loop
                                depth--;
                                continue;
                            }
                            if (isEmptyElement)
                                depth--;
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            depth--;
                        }

                        if (depth == 0)
                            break;
                    }
                }

                Trace.Assert(uploadUri != null, "Informational: UploadUri is null");

                fileFactory.AddUploadInfo(contextId, uploadVersion, uploadUri == null ? null : new Uri(uploadUri), settings);
                Debug.Assert(depth == 0 && reader.LocalName == ATTACHED_FILE_UPLOAD_ELEMENT, "Xmlreader is unexpectedly positioned (probably read to far!)");
            }

            public void ReadFileVersionData(XmlTextReader reader, SupportingFileFactory fileFactory)
            {
                Debug.Assert(reader.LocalName == ATTACHED_FILE_VERSION_ELEMENT, "Xml reader is improperly positioned");
                int depth = 0;

                string fileName = reader.GetAttribute(ATTACHED_FILE_NAME);

                string fileNameUniqueToken = reader.GetAttribute(ATTACHED_FILE_NAME_UNIQUETOKEN);
                if (fileNameUniqueToken == null) //necessary for interim Wave2 M1 builds
                    fileNameUniqueToken = _fileService.CreateUniqueFileNameToken(fileName);

                string fileVersionString = reader.GetAttribute(ATTACHED_FILE_VERSION_ATTRIBUTE);
                int fileVersion = -1;
                if (fileVersionString != null)
                    fileVersion = Int32.Parse(fileVersionString, CultureInfo.InvariantCulture);

                bool embedded = bool.Parse(reader.GetAttribute(ATTACHED_FILE_EMBEDDED_ATTRIBUTE));
                string fileUriString = reader.GetAttribute(ATTACHED_FILE_URI_ATTRIBUTE);

                //fixup the reference path
                if (embedded)
                    fileUriString = _supportingFilePersister.LoadFilesAndFixupReferences(new string[] { fileUriString })[0];

                BlogPostSettingsBag settings = new BlogPostSettingsBag();
                if (!reader.IsEmptyElement)
                {
                    depth = 1;
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            bool isEmptyElement = reader.IsEmptyElement;
                            depth++;
                            if (reader.LocalName == SETTINGS_BAG_ELEMENT)
                            {
                                ReadBlogPostSettingsBag(reader, settings);

                                //this element was completely handled, and the stack was reset to its end element,
                                //so unset the depth and re-start the loop
                                depth--;
                                continue;
                            }
                            if (isEmptyElement)
                                depth--;
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            depth--;
                        }

                        if (depth == 0)
                            break;
                    }
                }

                fileFactory.CreateSupportingFile(new Uri(UrlHelper.CreateUrlFromPath(fileUriString)), fileVersion, fileName, fileNameUniqueToken, embedded, settings);

                Debug.Assert(depth == 0 && reader.LocalName == ATTACHED_FILE_VERSION_ELEMENT, "Xmlreader is unexpectedly positioned (probably read to far!)");
            }
        }

        private static void WriteBlogPostSettingsBag(XmlTextWriter writer, BlogPostSettingsBag settings, string name)
        {
            writer.WriteStartElement(SETTINGS_BAG_ELEMENT);
            WriteNonNullAttribute(writer, SETTINGS_BAG_NAME_ATTRIBUTE, name);
            foreach (string key in settings.Names)
            {
                writer.WriteStartElement(SETTINGS_BAG_SETTING_ELEMENT);
                writer.WriteAttributeString(SETTINGS_BAG_NAME_ATTRIBUTE, key);
                WriteNonNullAttribute(writer, SETTINGS_BAG_VALUE_ATTRIBUTE, settings[key]);
                writer.WriteEndElement(); //end SETTINGS_BAG_SETTING_ELEMENT
            }

            //save out the subsettings
            foreach (string subSettingName in settings.SubsettingNames)
            {
                WriteBlogPostSettingsBag(writer, settings.GetSubSettings(subSettingName), subSettingName);
            }

            writer.WriteEndElement();	//end SETTINGS_BAG_ELEMENT
        }

        private static string ReadBlogPostSettingsBag(XmlTextReader reader, BlogPostSettingsBag settings)
        {
            Debug.Assert(reader.LocalName == SETTINGS_BAG_ELEMENT, "Xml reader is improperly positioned");
            int depth = 0;
            string settingsBagName = reader.GetAttribute(SETTINGS_BAG_NAME_ATTRIBUTE);
            if (!reader.IsEmptyElement)
            {
                depth = 1;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        bool isEmptyElement = reader.IsEmptyElement;
                        depth++;
                        if (reader.LocalName == SETTINGS_BAG_ELEMENT)
                        {
                            string subsettingsName = reader.GetAttribute(SETTINGS_BAG_NAME_ATTRIBUTE);
                            BlogPostSettingsBag subsettings = settings.CreateSubSettings(subsettingsName);
                            ReadBlogPostSettingsBag(reader, subsettings);

                            //this element was completely handled, and the stack was reset to its end element,
                            //so unset the depth and re-start the loop
                            depth--;
                            continue;
                        }
                        else if (reader.LocalName == SETTINGS_BAG_SETTING_ELEMENT)
                        {
                            string settingName = null;
                            string settingValue = null;

                            for (int i = 0; i < reader.AttributeCount; i++)
                            {
                                reader.MoveToAttribute(i);
                                switch (reader.LocalName)
                                {
                                    case SETTINGS_BAG_NAME_ATTRIBUTE:
                                        settingName = reader.Value;
                                        break;
                                    case SETTINGS_BAG_VALUE_ATTRIBUTE:
                                        settingValue = reader.Value;
                                        break;
                                }
                            }
                            settings[settingName] = settingValue;
                        }
                        if (isEmptyElement)
                            depth--;
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        depth--;
                    }

                    if (depth == 0)
                        break;
                }
            }
            Debug.Assert(depth == 0 && reader.LocalName == SETTINGS_BAG_ELEMENT, "Xmlreader is unexpectedly positioned (probably read to far!)");
            return settingsBagName;
        }

        private static string ReadBlogName(Storage postStorage)
        {
            string blogId = ReadString(postStorage, DESTINATION_BLOG_ID);
            if (BlogSettings.BlogIdIsValid(blogId))
            {
                using (Blog blog = new Blog(blogId))
                    return blog.Name;
            }
            else
            {
                return String.Empty;
            }
        }

        private static void WriteNonNullAttribute(XmlTextWriter writer, string name, string value)
        {
            if (value != null)
                writer.WriteAttributeString(name, value);
        }

        private const string DESTINATION_BLOG_ID = "DestinationBlogId";
        private const string SERVER_SUPPORTING_FILE_DIR = "ServerSupportingFileDir";
        private const string POST_ID = "Id";
        private const string POST_ISPAGE = "IsPage";
        private const string POST_TITLE = "Title";
        private const string POST_LINK = "Link";
        private const string POST_CONTENTS = "Contents";
        private const string POST_CONTENTS_VERSION_SIGNATURE = "ContentsVersionSignature";
        private const string POST_ETAG = "ETag";
        private const string POST_CATEGORIES = "Categories";
        private const string POST_NEW_CATEGORIES = "NewCategories";
        private const string POST_IMAGE_FILES = "ImageFiles";
        private const string POST_ATTACHED_FILES = "AttachedFiles";
        private const string POST_DATEPUBLISHED = "DatePublished";
        private const string POST_DATEPUBLISHED_OVERRIDE = "DatePublishedOverride";
        private const string POST_COMMENT_POLICY = "CommentPolicy";
        private const string POST_TRACKBACK_POLICY = "TrackbackPolicy";
        private const string POST_ATOM_REMOTE_POST = "AtomRemotePost";
        private const string POST_KEYWORDS = "Keywords";
        private const string POST_EXCERPT = "Exerpt";
        private const string POST_PERMALINK = "PermaLink";
        private const string POST_PINGURLS_PENDING = "PingUrls";
        private const string POST_PINGURLS_SENT = "SentPingUrls";
        private const string POST_SLUG = "Slug";
        private const string POST_PASSWORD = "Password";
        private const string POST_AUTHOR_ID = "AuthorId";
        private const string POST_AUTHOR_NAME = "AuthorName";
        private const string POST_PAGE_PARENT_ID = "PageParentId";
        private const string POST_PAGE_PARENT_NAME = "PageParentName";
        private const string POST_PAGE_ORDER = "PageOrder";
        private const string POST_SUPPORTING_FILES = "SupportingFiles";
        private const string POST_EXTENSION_DATA_LIST = "ExtensionDataList";

        private const string COMMENT_POLICY_UNSPECIFIED = "Unspecified";
        private const string COMMENT_POLICY_CLOSED = "Closed";
        private const string COMMENT_POLICY_OPEN = "Open";
        private const string COMMENT_POLICY_NONE = "None";

        private const string TRACKBACK_POLICY_UNSPECIFIED = "Unspecified";
        private const string TRACKBACK_POLICY_ALLOW = "Allow";
        private const string TRACKBACK_POLICY_DENY = "Deny";

        private const string PING_URLS_ELEMENT = "PingUrls";
        private const string PING_URL_ELEMENT = "PingUrl";

        private const string CATEGORIES_ELEMENT = "Categories";
        private const string CATEGORY_ELEMENT = "Category";
        private const string CATEGORY_ID_ATTRIBUTE = "Id";
        private const string CATEGORY_NAME_ATTRIBUTE = "Name";
        private const string CATEGORY_PARENT_ATTRIBUTE = "Parent";

        private const string SUPPORTING_FILE_LINK_ELEMENT = "SupportingFileLink";
        private const string SUPPORTING_FILE_LINK_NAME_ATTRIBUTE = "Name";
        private const string SUPPORTING_FILE_LINK_FILE_ID_ATTRIBUTE = "SupportingFileId";

        private const string IMAGE_FILES_ELEMENT = "ImageFiles";
        private const string IMAGE_FILE_ELEMENT = "ImageFile";
        private const string IMAGE_FILE_SOURCE_URI_ATTRIBUTE = "ImageFileSourceUri";
        private const string IMAGE_FILE_SOURCE_WIDTH_ATTRIBUTE = "ImageFileSourceWidth";
        private const string IMAGE_FILE_SOURCE_HEIGHT_ATTRIBUTE = "ImageFileSourceHeight";

        //--BACKWARDS_COMPATABILITY: pre-Beta2 image file data (auto converted to supporting files in Beta2)
        private const string IMAGE_FILE_LINK_ELEMENT = "ImageFileLink";
        private const string IMAGE_FILE_LINK_WIDTH_ATTRIBUTE = "Width";
        private const string IMAGE_FILE_LINK_HEIGHT_ATTRIBUTE = "Height";
        private const string IMAGE_FILE_LINK_RELATIONSHIP_ATTRIBUTE = "Relationship";
        private const string IMAGE_FILE_LINK_PATH_ATTRIBUTE = "Path";
        private const string IMAGE_FILE_LINK_PUBLISH_URL_ATTRIBUTE = "PublishUrl";
        //--end pre-Beta2 image file data

        private const string IMAGE_DECORATORS_SETTINGSBAG_NAME = "ImageDecorators";
        private const string IMAGE_UPLOAD_ELEMENT = "ImageUpload";
        private const string IMAGE_UPLOAD_SERVICE_ID_ATTRIBUTE = "ServiceId";
        private const string EXTENSION_DATA_LIST_ELEMENT = "ExtensionDataList";
        private const string EXTENSION_DATA_ELEMENT = "ExtensionData";
        private const string EXTENSION_DATA_SETTINGSBAG_NAME = "ExtensionDataSettings";
        private const string EXTENSION_DATA_FILE_ELEMENT = "SupportingFile";
        private const string EXTENSION_DATA_FILE_ID_ATTRIBUTE = "fileId";
        private const string EXTENSION_DATA_SUPPORTING_FILE_ID_ATTRIBUTE = "supportingFileId";
        private const string EXTENSION_DATA_FILE_PATH_ATTRIBUTE = "filePath";
        private const string IMAGE_UPLOAD_SETTINGSBAG_NAME = "ImageUploadSettings";

        private const string SETTINGS_BAG_ELEMENT = "Settings";
        private const string SETTINGS_BAG_SETTING_ELEMENT = "Setting";
        private const string SETTINGS_BAG_NAME_ATTRIBUTE = "Name";
        private const string SETTINGS_BAG_VALUE_ATTRIBUTE = "Value";

        private const string ORIGINAL_SOURCE_PATH = "OriginalSourcePath";

        /// <summary>
        /// Utility class for saving and loading supporting files into structured storage
        /// </summary>
        private class SupportingFilePersister
        {
            Hashtable referencesTable = new Hashtable();
            public SupportingFilePersister(Storage fileSubStorage)
            {
                _fileSubStorage = fileSubStorage;
            }

            public SupportingFilePersister(Storage fileSubStorage, BlogPostSupportingFileStorage supportingFileStorage)
                : this(fileSubStorage)
            {
                _supportingFileStorage = supportingFileStorage;
            }

            /// <summary>
            /// Save files referred to in the post contents and fixup references to point to internal storage
            /// </summary>
            /// <param name="postContents"></param>
            /// <returns></returns>
            public string FixupHtmlReferences(string postContents)
            {
                string fixedUpContents = HtmlReferenceFixer.FixReferences(postContents, new ReferenceFixer(FixReference));
                return fixedUpContents;
            }

            public string[] SaveFilesAndFixupReferences(string[] references)
            {
                string[] fixedReferences = new string[references.Length];
                for (int i = 0; i < references.Length; i++)
                {
                    fixedReferences[i] = SaveToStorageReferenceFixer(references[i]);
                }
                _fileSubStorage.Commit();

                return fixedReferences;
            }

            public string FixReference(BeginTag tag, string reference)
            {
                if (UrlHelper.IsUrl(reference))
                {
                    Uri referenceUri = new Uri(reference);
                    if (referencesTable.ContainsKey(referenceUri))
                        return (string)referencesTable[referenceUri];
                }
                return reference;
            }

            /// <summary>
            /// Input: A URI to a file.
            /// Output: A newly created unique supporting file URI (SupportingFileReference://3EF3AA3F-5560-4750-9C97-171B37D7F45A)
            /// Side effects:
            ///		1) A substorage is made that is named after the GUID in the output URI.
            ///		2) Two streams are written to the substorage: SupportingFileName and SupportingFileContents
            ///
            /// If the reference has already been seen, the side effects do not occur and an existing SupportingFileReference URI is returned.
            /// </summary>
            private string SaveToStorageReferenceFixer(string reference)
            {
                Uri referenceUri = new Uri(reference);
                if (referencesTable.ContainsKey(referenceUri))
                    return (string)referencesTable[referenceUri];

                string filePath = referenceUri.LocalPath;
                using (FileStream referenceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    Guid localFileAlias = Guid.NewGuid();
                    using (Storage localFileStorage = _fileSubStorage.OpenStorage(localFileAlias, StorageMode.Create, true))
                    {
                        WriteString(localFileStorage, SUPPORTING_FILE_NAME, Path.GetFileName(filePath));

                        using (Stream localFileStream = localFileStorage.OpenStream(SUPPORTING_FILE_CONTENTS, StorageMode.Create, true))
                            StreamHelper.Transfer(referenceStream, localFileStream);

                        localFileStorage.Commit();
                    }

                    string fixedReference = SUPPORTING_FILE_PREFIX + localFileAlias.ToString();
                    referencesTable[referenceUri] = fixedReference;
                    return fixedReference;
                }

            }

            public string[] LoadFilesAndFixupReferences(string[] references)
            {
                string[] fixedReferences = new string[references.Length];
                for (int i = 0; i < references.Length; i++)
                    fixedReferences[i] = LoadFromStorageReferenceFixer(references[i]);
                return fixedReferences;
            }

            private string LoadFromStorageReferenceFixer(string reference)
            {
                //if ( reference.ToLower(CultureInfo.InvariantCulture).StartsWith(SUPPORTING_FILE_PREFIX.ToLower(CultureInfo.InvariantCulture)) )
                if (reference.StartsWith(SUPPORTING_FILE_PREFIX))
                {
                    string guidAlias = reference.Substring(SUPPORTING_FILE_PREFIX.Length);
                    Guid localFileAlias = new Guid(guidAlias);

                    //see if this reference has already been loaded and added to the supporting files store
                    if (referencesTable.ContainsKey(reference))
                        return (string)referencesTable[new Uri(reference)];

                    using (Storage localFileStorage = _fileSubStorage.OpenStorage(localFileAlias, StorageMode.Open, false))
                    {
                        string fileName = ReadString(localFileStorage, SUPPORTING_FILE_NAME);

                        using (Stream localFileStream = localFileStorage.OpenStream(SUPPORTING_FILE_CONTENTS, StorageMode.Open, false))
                        {
                            string localFileUri = _supportingFileStorage.AddFile(fileName, localFileStream);

                            //cache this reference so it doesn't get re-added to the files store if referenced more than once.
                            referencesTable[new Uri(reference)] = localFileUri;
                            return localFileUri;
                        }
                    }
                }
                else
                {
                    return reference;
                }
            }

            private Storage _fileSubStorage;
            private BlogPostSupportingFileStorage _supportingFileStorage;

            public const string SUPPORTING_FILE_PREFIX = "SupportingFileReference://";
            private const string SUPPORTING_FILE_NAME = "SupportingFileName";
            private const string SUPPORTING_FILE_CONTENTS = "SupportingFileContents";
        }

        #endregion

        #region Private Data

        private FileInfo TargetFile
        {
            get
            {
                return _targetFile;
            }
            set
            {
                _targetFile = value;
                if (_targetFile != null)
                    _targetDirectory = _targetFile.Directory;

            }
        }
        private FileInfo _targetFile = null;

        private DirectoryInfo TargetDirectory
        {
            get
            {
                return _targetDirectory;
            }
            set
            {
                _targetDirectory = value;
            }
        }

        private DirectoryInfo _targetDirectory = null;

        internal const string Extension = ".wpost";
        private static readonly Guid Version1FormatCLSID = new Guid("20EBD150-5362-417a-8221-84331F79D41D");
        private static readonly Guid Version2FormatCLSID = new Guid("23F4998B-67EB-450b-A41B-C978F5B4AE25");

        #endregion
    }
}
