// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.FileDestinations;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{
    public abstract class BlogFileUploader : IDisposable
    {
        protected BlogFileUploader(string destinationContext, string postContext, string blogId)
        {
            DestinationContext = destinationContext;
            PostContext = postContext;
            BlogId = blogId;
        }

        public virtual void Dispose()
        {
            try
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception attempting to disconnect from BlogFileUploader: " + ex.ToString());
            }
            GC.SuppressFinalize(this);
        }

        ~BlogFileUploader()
        {
            Debug.Fail("Failed to Dispose BlogFileUploader!");
        }

        public virtual void Connect() { }

        public virtual void Disconnect() { }

        public abstract Uri DoUploadWorkBeforePublish(IFileUploadContext uploadContext);

        public virtual void DoUploadWorkAfterPublish(IFileUploadContext uploadContext)
        {
        }

        public abstract string FormatUploadFileName(string filename, string conflictToken);

        public readonly string BlogId;

        protected readonly string PostContext;

        public readonly string DestinationContext;

        protected string FormatFileName(string format, string filename, string conflictToken)
        {
            string postTitle = PostContext;
            string postRandomizerToken = PostContext;
            int uniqueIndex = PostContext.LastIndexOf("_");
            if (uniqueIndex != -1)
            {
                postTitle = postTitle.Substring(0, uniqueIndex);
                postRandomizerToken = PostContext.TrimEnd('/').Substring(uniqueIndex + 1);
            }

            if (format == String.Empty)
            {
                format = "{PostTitle}_{PostRandomizer}/{FileName}";
            }

            string formattedName = format.Replace("{FileName}", filename);
            formattedName = formattedName.Replace("{FileNameWithoutExtension}", Path.GetFileNameWithoutExtension(filename));
            formattedName = formattedName.Replace("{AsciiFileName}", FileHelper.GetValidAnsiFileName(filename));
            formattedName = formattedName.Replace("{AsciiFileNameWithoutExtension}", Path.GetFileNameWithoutExtension(FileHelper.GetValidAnsiFileName(filename)));
            formattedName = ReplaceVariableFormatted(formattedName, "FileNameConflictToken", conflictToken != null ? new StringFormatter(conflictToken) : null);
            formattedName = formattedName.Replace("{FileExtension}", Path.GetExtension(filename));
            formattedName = formattedName.Replace("{PostRandomizer}", postRandomizerToken);
            formattedName = formattedName.Replace("{PostTitle}", postTitle);
            formattedName = formattedName.Replace("{OpenLiveWriter}", FileHelper.GetValidAnsiFileName(ApplicationEnvironment.ProductName));
            formattedName = ReplaceVariableFormatted(formattedName, "UploadDate", DateTime.Now);
            formattedName = formattedName.Replace("{Randomizer}", GuidHelper.GetVeryShortGuid());

            return formattedName;
        }

        string ReplaceVariableFormatted(string input, string variableName, object val)
        {
            MatchCollection mc = Regex.Matches(input, "{" + variableName + "([^}]*)");
            StringBuilder sb = new StringBuilder();
            int inputIndex = 0;
            foreach (Match m in mc)
            {
                if (m.Success)
                {
                    sb.Append(input.Substring(inputIndex, m.Index - inputIndex));

                    Group tokenFormatGroup = null;
                    if (m.Groups.Count > 1)
                        tokenFormatGroup = m.Groups[1];

                    if (val != null)
                    {
                        if (tokenFormatGroup != null)
                        {
                            val = String.Format(CultureInfo.InvariantCulture, "{0" + tokenFormatGroup.Value + "}", val);
                        }
                        sb.Append(val);
                    }

                    inputIndex = m.Index + m.Length + 1;
                }
            }

            if (inputIndex < input.Length)
                sb.Append(input.Substring(inputIndex));
            return sb.ToString();
        }

        /// <summary>
        /// Formattable wrapper for a string that allows the string's format to print itself.
        /// Note: the format syntax uses a ? character to replace its value
        /// Example: String.Format{"0:---?----"}, new StringFormatter("test")) outputs: "---test----"
        /// </summary>
        private class StringFormatter : IFormattable
        {
            private string _value;
            public StringFormatter(string val)
            {
                _value = val;
            }
            public string ToString(string format, IFormatProvider formatProvider)
            {
                string formattedString = _value;
                if (format != null)
                    formattedString = String.Format(CultureInfo.InvariantCulture, format.Replace("?", "{0}"), _value);
                return formattedString;
            }

            public override string ToString()
            {
                return _value;
            }
        }

        public static BlogFileUploader CreateFileUploader(Blog blog, string postContextName)
        {
            string destinationContext = GetFileUploadDestinationContext(blog.Id, blog.FileUploadSupport, blog.FileUploadSettings);
            switch (blog.FileUploadSupport)
            {
                case FileUploadSupport.Weblog:
                    return new WeblogBlogFileUploader(destinationContext, postContextName, blog.Id, blog.HostBlogId);

                case FileUploadSupport.FTP:
                    return new FTPBlogFileUploader(destinationContext, postContextName, new FtpUploaderSettings(blog.FileUploadSettings), blog.Id);

                default:
                    Trace.Fail("Unexpected value for blog.FileUploadSupport: " + blog.FileUploadSupport.ToString());
                    goto case FileUploadSupport.Weblog;
            }
        }

        /// <summary>
        /// Returns the file upload destination context string for the specified blog based on its upload settings.
        /// (used for persisting upload information in the ISupportingFile).
        /// </summary>
        /// <param name="blogId"></param>
        /// <returns></returns>
        public static string GetFileUploadDestinationContext(string blogId)
        {
            using (BlogSettings blogSettings = BlogSettings.ForBlogId(blogId))
            {
                IBlogFileUploadSettings uploadSettings = blogSettings.FileUploadSettings;
                FileUploadSupport fileUploadSupport = blogSettings.FileUploadSupport;
                return GetFileUploadDestinationContext(blogId, fileUploadSupport, uploadSettings);
            }
        }

        static string GetFileUploadDestinationContext(string blogId, FileUploadSupport fileUploadSupport, IBlogFileUploadSettings uploadSettings)
        {
            switch (fileUploadSupport)
            {
                case FileUploadSupport.Weblog:
                    return blogId;

                case FileUploadSupport.FTP:
                    FtpUploaderSettings ftpSettings = new FtpUploaderSettings(uploadSettings);
                    return String.Format(CultureInfo.InvariantCulture, "ftp://{0}@{1}{2}", ftpSettings.Username, ftpSettings.FtpServer, ftpSettings.PublishPath);

                default:
                    Trace.Fail("Unexpected value for fileUploadSupport: " + fileUploadSupport.ToString());
                    goto case FileUploadSupport.Weblog;
            }
        }

        public virtual bool DoesFileNeedUpload(ISupportingFile file, IFileUploadContext uploadContext)
        {
            // Check to see if we have already uploaded this file.
            if (!file.IsUploaded(DestinationContext))
            {
                return true;
            }
            else
            {
                Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "File is up-to-date: {0}", file.FileName));
                return false;
            }
        }
    }

    public class NullBlogFileUploader : BlogFileUploader
    {
        public NullBlogFileUploader(string destinationContext, string contextName, string blogId)
            : base(destinationContext, contextName, blogId)
        {
        }

        public override Uri DoUploadWorkBeforePublish(IFileUploadContext uploadContext)
        {
            throw new BlogClientFileUploadNotSupportedException();
        }

        public override string FormatUploadFileName(string filename, string conflictToken)
        {
            return filename;
        }
    }

    public class WeblogBlogFileUploader : BlogFileUploader
    {
        public WeblogBlogFileUploader(string destinationContext, string postContext, string localBlogId, string remoteBlogId)
            : base(destinationContext,
                   postContext,
                    remoteBlogId)
        {
            _blogSettings = BlogSettings.ForBlogId(localBlogId);
            _blogClient = BlogClientManager.CreateClient(_blogSettings);
            _blogHomepageUrl = _blogSettings.HomepageUrl;
        }

        public override void Dispose()
        {
            base.Dispose();
            _blogSettings.Dispose();
        }

        public override Uri DoUploadWorkBeforePublish(IFileUploadContext uploadContext)
        {
            try
            {
                string uploadUrl = _blogClient.DoBeforePublishUploadWork(uploadContext);

                if (!UrlHelper.IsUrl(uploadUrl))
                {
                    string baseURL;
                    if (uploadUrl.StartsWith("/"))
                        baseURL = UrlHelper.GetBaseUrl(_blogHomepageUrl);
                    else
                        baseURL = UrlHelper.GetBasePathUrl(_blogHomepageUrl);
                    uploadUrl = UrlHelper.UrlCombineIfRelative(baseURL, uploadUrl);
                }

                return new Uri(uploadUrl);
            }
            catch (BlogClientOperationCancelledException)
            {
                throw; // No need to assert when an operation is cancelled
            }
            catch (IOException ex)
            {
                Trace.Fail(ex.ToString());
                throw new BlogClientIOException(new FileInfo(uploadContext.GetContentsLocalFilePath()), ex);
            }
            catch (BlogClientAuthenticationException ex)
            {
                Trace.Fail(ex.ToString());
                throw;
            }
            catch (BlogClientProviderException ex)
            {
                Trace.Fail(ex.ToString());

                // provider exceptions that are not authentication exceptions are presumed
                // to be lack of support for newMediaObject we may want to filter this down
                // further -- not sure how to do this other than by trial and error with the
                // various services.
                throw new BlogClientFileUploadNotSupportedException(ex.ErrorCode, ex.ErrorString);
            }
            catch (BlogClientException ex)
            {
                Trace.Fail(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.ToString());
                if (ex is WebException)
                {
                    HttpRequestHelper.LogException((WebException)ex);
                }
                throw new BlogClientException(Res.Get(StringId.FileUploadFailedException), ex.Message);
            }
        }

        public override void DoUploadWorkAfterPublish(IFileUploadContext uploadContext)
        {
            try
            {
                _blogClient.DoAfterPublishUploadWork(uploadContext);
            }
            catch (IOException ex)
            {
                Trace.Fail(ex.ToString());
                throw new BlogClientIOException(new FileInfo(uploadContext.GetContentsLocalFilePath()), ex);
            }
            catch (BlogClientException ex)
            {
                Trace.Fail(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.ToString());
                throw new BlogClientException(Res.Get(StringId.FileUploadFailedException), ex.Message);
            }
            return;
        }

        public override string FormatUploadFileName(string filename, string conflictToken)
        {
            string format = _blogClient.Options.FileUploadNameFormat;
            if (format == String.Empty)
            {
                format = "{OpenLiveWriter}/{PostTitle}_{PostRandomizer}/{FileNameWithoutExtension}{FileNameConflictToken:_?}{FileExtension}";
            }
            return base.FormatFileName(format, filename, conflictToken);
        }

        public override bool DoesFileNeedUpload(ISupportingFile file, IFileUploadContext uploadContext)
        {
            // Let the blog client decide if it wants to upload this file or not
            bool? shouldUpload = _blogClient.DoesFileNeedUpload(uploadContext);

            // Check to see if the blog client made a decision, if so, then use it
            if (shouldUpload != null)
            {
                return shouldUpload.Value;
            }

            // Check to see if it was already uploaded and saved in the content for this post
            return base.DoesFileNeedUpload(file, uploadContext);
        }

        private BlogSettings _blogSettings;
        private string _blogHomepageUrl;
        private IBlogClient _blogClient;
    }

    public class FTPBlogFileUploader : BlogFileUploader
    {
        private static Hashtable _credentials = new Hashtable();
        public FTPBlogFileUploader(string destinationContext, string postContext, FtpUploaderSettings settings, string blogId)
            : base(destinationContext, postContext, blogId)
        {
            _settings = settings;
        }

        public static void ClearCachedCredentials(string blogId)
        {
            String destinationContext = GetFileUploadDestinationContext(blogId);
            _credentials.Remove(destinationContext);
        }

        private void ConnectForUpload()
        {
            if (_fileDestination == null)
            {
                try
                {
                    bool loggedIn = false;

                    FtpCredentials credentials = (FtpCredentials)_credentials[DestinationContext];
                    string username = credentials != null ? credentials.Username : _settings.Username;
                    string password = credentials != null ? credentials.Password : _settings.Password;

                    while (!loggedIn)
                    {
                        if (password == String.Empty)
                        {
                            CredentialsDomain cd = new CredentialsDomain(Res.Get(StringId.FtpLoginDomain), _settings.FtpServer, null, FtpIconBytes);
                            CredentialsPromptResult result = CredentialsHelper.PromptForCredentials(ref username, ref password, cd);
                            if (result == CredentialsPromptResult.Cancel || result == CredentialsPromptResult.Abort)
                            {
                                throw new OperationCancelledException();
                            }
                            else
                            {
                                //save the user/pass as appropriate
                                if (result == CredentialsPromptResult.SaveUsername)
                                {
                                    _settings.Username = username;
                                    _settings.Password = String.Empty;
                                }
                                else if (result == CredentialsPromptResult.SaveUsernameAndPassword)
                                {
                                    _settings.Username = username;
                                    _settings.Password = password;
                                }
                            }
                        }
                        try
                        {
                            // create and connect to the destination
                            _fileDestination = new WinInetFTPFileDestination(
                                _settings.FtpServer,
                                _settings.PublishPath,
                                username,
                                password);

                            _fileDestination.Connect();

                            //save the validated credentials so we don't need to prompt again later
                            _credentials[DestinationContext] = new FtpCredentials(DestinationContext, username, password);

                            loggedIn = true;
                        }
                        catch (LoginException)
                        {
                            loggedIn = false;
                            password = String.Empty;
                            _credentials.Remove(DestinationContext);
                        }
                    }

                    // calculate the target path and ensure that it exists
                    _fileDestination.InsureDirectoryExists(PostContext);
                }
                catch (Exception ex)
                {
                    WebPublishMessage message = WebPublishUtils.ExceptionToErrorMessage(ex);
                    throw new BlogClientFileTransferException(Res.Get(StringId.BCEFileTransferConnectingToDestination), message.Title, message.Text);
                }
            }
        }

        private static byte[] ftpIconBytes;
        private static byte[] FtpIconBytes
        {
            get
            {
                if (ftpIconBytes == null)
                {
                    MemoryStream memStream = new MemoryStream();
                    ResourceHelper.SaveAssemblyResourceToStream("Images.FtpIcon.png", memStream);
                    ftpIconBytes = memStream.ToArray();
                }
                return ftpIconBytes;
            }
        }

        private class FtpCredentials
        {
            public FtpCredentials(string connectionId, string username, string password)
            {
                ConnectionId = connectionId;
                Username = username;
                Password = password;
            }
            public string Username;
            public string Password;
            public string ConnectionId;
        }

        public override void Disconnect()
        {
            try
            {
                if (_fileDestination != null)
                {
                    _fileDestination.Disconnect();
                    _fileDestination.Dispose();
                    _fileDestination = null;
                }
            }
            catch (Exception ex)
            {
                WebPublishMessage message = WebPublishUtils.ExceptionToErrorMessage(ex);
                throw new BlogClientFileTransferException("disconnecting from file destination", message.Text, message.Title);
            }
        }

        public override Uri DoUploadWorkBeforePublish(IFileUploadContext uploadContext)
        {
            try
            {
                ConnectForUpload();

                string uploadPath = uploadContext.Settings.GetString(UPLOAD_PATH, null);
                bool overwrite = uploadPath != null;
                if (uploadPath == null)
                {
                    string uploadFolder = null;
                    string fileName = uploadContext.PreferredFileName;
                    string filePath = uploadContext.FormatFileName(fileName);
                    string[] pathParts = filePath.Split('/');
                    if (pathParts.Length > 1)
                    {
                        uploadFolder = FileHelper.GetValidAnsiFileName(pathParts[0]);
                        for (int i = 1; i < pathParts.Length - 1; i++)
                            uploadFolder = uploadFolder + "/" + FileHelper.GetValidAnsiFileName(pathParts[i]);
                    }

                    fileName = FileHelper.GetValidAnsiFileName(pathParts[pathParts.Length - 1]);
                    uploadPath = _fileDestination.CombinePath(uploadFolder, fileName);
                    if (_fileDestination.FileExists(uploadPath))
                    {
                        string fileBaseName = Path.GetFileNameWithoutExtension(fileName);
                        string fileExtension = Path.GetExtension(fileName);
                        try
                        {
                            Hashtable existingFiles = new Hashtable();
                            foreach (string name in _fileDestination.ListFiles(uploadFolder))
                                existingFiles[name] = name;
                            for (int i = 3; i < Int32.MaxValue && existingFiles.ContainsKey(fileName); i++)
                            {
                                fileName = FileHelper.GetValidAnsiFileName(fileBaseName + "_" + i + fileExtension);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Fail("Error while calculating unique filename", e.ToString());
                        }

                        uploadPath = _fileDestination.CombinePath(uploadFolder, fileName);
                        if (_fileDestination.FileExists(uploadPath))
                        {
                            Debug.Fail("Failed to calculate unique filename");
                            fileName = FileHelper.GetValidAnsiFileName(fileBaseName + Guid.NewGuid().ToString() + fileExtension);
                            uploadPath = _fileDestination.CombinePath(uploadFolder, fileName);
                        }
                    }
                }

                // transfer the file
                _fileDestination.DoTransfer(
                    uploadContext.GetContentsLocalFilePath(),
                    uploadPath,
                    overwrite);

                uploadContext.Settings.SetString(UPLOAD_PATH, uploadPath);

                // return the url to the transferred file
                string baseUrl = UrlHelper.InsureTrailingSlash(_settings.UrlMapping);
                string relativeUrl = uploadPath;
                return new Uri(UrlHelper.UrlCombine(baseUrl, relativeUrl));
            }
            catch (Exception ex)
            {
                WebPublishMessage message = WebPublishUtils.ExceptionToErrorMessage(ex);
                throw new BlogClientFileTransferException(new FileInfo(uploadContext.GetContentsLocalFilePath()), message.Title, message.Text);
            }
        }
        private const string UPLOAD_PATH = "upload.path";

        public override string FormatUploadFileName(string filename, string conflictToken)
        {
            string format = _settings.FileUploadFormat;
            return base.FormatFileName(format, filename, conflictToken);
        }

        private FtpUploaderSettings _settings;
        private FileDestination _fileDestination;
    }

    public class FtpUploaderSettings
    {
        public static void Copy(IBlogFileUploadSettings source, IBlogFileUploadSettings destination)
        {
            // create typesafe wrappers for source and destination
            FtpUploaderSettings sourceSettings = new FtpUploaderSettings(source);
            FtpUploaderSettings destinationSettings = new FtpUploaderSettings(destination);

            // copy the values
            destinationSettings.FtpServer = sourceSettings.FtpServer;
            destinationSettings.PublishPath = sourceSettings.PublishPath;
            destinationSettings.UrlMapping = sourceSettings.UrlMapping;
            destinationSettings.Username = sourceSettings.Username;
            destinationSettings.Password = sourceSettings.Password;
        }

        public FtpUploaderSettings(IBlogFileUploadSettings settings)
        {
            _settings = settings;
        }

        public string FtpServer
        {
            get { return _settings.GetValue(FTP_SERVER); }
            set { _settings.SetValue(FTP_SERVER, value); }
        }
        private const string FTP_SERVER = "FtpServer";

        public string PublishPath
        {
            get { return _settings.GetValue(PUBLISH_PATH); }
            set { _settings.SetValue(PUBLISH_PATH, value); }
        }
        private const string PUBLISH_PATH = "PublishPath";

        public string UrlMapping
        {
            get { return _settings.GetValue(URL_MAPPING); }
            set { _settings.SetValue(URL_MAPPING, value); }
        }
        private const string URL_MAPPING = "UrlMapping";

        public string Username
        {
            get { return _settings.GetValue(USERNAME); }
            set { _settings.SetValue(USERNAME, value); }
        }
        private const string USERNAME = "Username";

        public string Password
        {
            get
            {
                //load the decrypted password
                try
                {
                    string base64EncodedPass = _settings.GetValue(PASSWORD);
                    if (!string.IsNullOrEmpty(base64EncodedPass))
                    {
                        if (base64EncodedPass.Length < 200) //encoded passwords are always larger than 200, and non-encoded passwords are unlikely to be
                        {

                            //then this password is not stored encrypted (probably because of a bug introduced in M2, so resave the password in encrypted form)
                            Password = base64EncodedPass;
                            base64EncodedPass = _settings.GetValue(PASSWORD);
                            Trace.WriteLine("FTP password was auto-encrypted");
                        }
                        byte[] encrypted = Convert.FromBase64String(base64EncodedPass);
                        string password = CryptHelper.Decrypt(encrypted);
                        return password;
                    }
                }
                catch (Exception e)
                {
                    Trace.Fail("Failed to decrypt password: " + e);
                }
                return String.Empty;
            }
            set
            {
                if (value != String.Empty)
                {
                    //save an encrypted password
                    try
                    {
                        _settings.SetValue(PASSWORD, Convert.ToBase64String(CryptHelper.Encrypt(value)));
                    }
                    catch (Exception e)
                    {
                        //if an exception occurs, just leave the password empty
                        Trace.Fail("Failed to encrypt password: " + e);
                    }
                }
                else
                    _settings.SetValue(PASSWORD, value);
            }
        }
        private const string PASSWORD = "Password";

        public string FileUploadFormat
        {
            get
            {
                string format = _settings.GetValue(FILE_UPLOAD_FORMAT);
                if (format == null)
                {
                    format = "{PostTitle}_{PostRandomizer}/{FileName}";
                }
                return format;
            }
            set { _settings.SetValue(FILE_UPLOAD_FORMAT, value); }
        }
        private const string FILE_UPLOAD_FORMAT = "FileUploadFormat";

        private IBlogFileUploadSettings _settings;
    }
}
