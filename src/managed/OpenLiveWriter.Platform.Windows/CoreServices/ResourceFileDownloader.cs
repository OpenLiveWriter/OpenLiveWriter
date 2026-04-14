// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Xml;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Threading;

namespace OpenLiveWriter.CoreServices
{

    /// <summary>
    /// Class for downloading files from the web and storing them in a local cache. Note
    /// that files downloaded with this class must also exist as resources in the calling
    /// assembly as a last-ditch backup.
    /// </summary>
    public class ResourceFileDownloader
    {
        private static bool _allowResourceFileDownloads = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Resources").GetBoolean("DownloadResources", true);

        /// <summary>
        /// Version of the ResourceFileDownloader with cache mapped to standard Local Settings directory
        /// </summary>
        public static ResourceFileDownloader Application
        {
            get
            {
                return _application;
            }
        }
        private static readonly ResourceFileDownloader _application = new ResourceFileDownloader(Path.Combine(ApplicationEnvironment.LocalApplicationDataDirectory, "ResourceCache"), _allowResourceFileDownloads);

        public object ProcessLocalResourceSafelyAndRefresh(string name, string url, string contentType, ResourceFileProcessor processor, int delayMs)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            lock (_downloadedResources.SyncRoot)
            {
                if (!_downloadedResources.Contains(url))
                {
                    _downloadedResources.Add(url);
                    new DelayUpdateHelper(assembly, name, url, contentType, this, processor, delayMs).StartBackgroundUpdate();
                }
            }
            return ProcessResourceSafely(assembly, name, string.Empty, contentType, REQUIREDFRESHNESSDAYS, TIMEOUTMS, processor);
        }
        private const int REQUIREDFRESHNESSDAYS = 1;
        private const int TIMEOUTMS = 10000;
        private readonly static HashSet _downloadedResources = new HashSet();

        private class DelayUpdateHelper
        {
            /// <summary>
            /// Trims the working set after a given delay
            /// </summary>
            /// <param name="delayMs">The delay prior to trimming the working set</param>
            /// <returns></returns>
            public DelayUpdateHelper(Assembly callingAssembly, string name, string url, string contentType, ResourceFileDownloader downloader, ResourceFileProcessor processor, int delayMs)
            {
                _callingAssembly = callingAssembly;
                _name = name;
                _url = url;
                _contentType = contentType;
                _processor = processor;
                _downloader = downloader;
                _delayMs = delayMs;
            }
            private readonly string _name;
            private readonly string _url;
            private readonly string _contentType;
            private readonly ResourceFileProcessor _processor;
            private readonly ResourceFileDownloader _downloader;
            private readonly Assembly _callingAssembly;
            private readonly int _delayMs;

            public void StartBackgroundUpdate()
            {
                Thread t = ThreadHelper.NewThread(new ThreadStart(DownloadResource), "Background Resource Download", true, false, true);
                t.Start();
            }

            private void DownloadResource()
            {
                Thread.Sleep(_delayMs);
                _downloader.ProcessResourceSafely(_callingAssembly, _name, _url, _contentType, REQUIREDFRESHNESSDAYS, TIMEOUTMS, _processor);

            }
        }

        /// <summary>
        /// Initialize the downloader with the appropriate paths and options
        /// </summary>
        /// <param name="fileCacheBasePath">Base directory for writing cached copies of resources locally</param>
        /// <param name="enableDownloading">Enable downloading of resources from the network</param>
        public ResourceFileDownloader(string fileCacheBasePath, bool enableDownloading)
        {
            _fileCacheBasePath = fileCacheBasePath;
            _enableDownloading = enableDownloading && !ApplicationDiagnostics.SuppressBackgroundRequests;
        }

        /// <summary>
        /// Get a resource -- the name should be the "." separated path to the resource
        /// within the calling assembly. The resource should also be located at the same
        /// path relative to the resourceUrlBasePath on the target web server.
        /// </summary>
        /// <param name="name">resource name</param>
        /// <param name="resourceUrl">url where the resource can be found</param>
        /// <param name="requiredFreshnessDays">number of days for which we can use cached copies</param>
        /// <param name="timeoutMs">timeout in ms for request to web server</param>
        /// <returns>Stream to resource contents</returns>
        public Stream GetResource(string name, string resourceUrl, string contentType, int requiredFreshnessDays, int timeoutMs)
        {
            return GetResource(Assembly.GetCallingAssembly(), name, resourceUrl, contentType, requiredFreshnessDays, timeoutMs);
        }

        /// <summary>
        /// Get and process a resource (see comment on GetResource for more info on resources)
        /// This method attempts to load the resource normally (i.e. from the network). If
        /// an exception of any kind happens during processing we revert back to processing
        /// the version built into the assembly.
        /// </summary>
        /// <param name="name">resource name</param>
        /// <param name="requiredFreshnessDays">number of days for which we can use cached copies</param>
        /// <param name="timeoutMs">timeout in ms for request to web server</param>
        /// <param name="processor">callback used for processing the resource</param>
        /// <returns>result of processing</returns>
        public object ProcessResourceSafely(string name, string resourceUrl, string contentType, int requiredFreshnessDays, int timeoutMs, ResourceFileProcessor processor)
        {
            // record calling assembly
            return ProcessResourceSafely(Assembly.GetCallingAssembly(), name, resourceUrl, contentType, requiredFreshnessDays, timeoutMs, processor);
        }

        private object ProcessResourceSafely(Assembly callingAssembly, string name, string resourceUrl, string contentType, int requiredFreshnessDays, int timeoutMs, ResourceFileProcessor processor)
        {
            if (contentType == MimeHelper.TEXT_XML)
                processor = new ResourceFileProcessor(new XPPResourceFileProcessor(processor).ProcessXmlResource);

            // attempt to load and process the resource (potentially loading new data from the network)
            try
            {
                using (Stream stream = GetResource(callingAssembly, name, resourceUrl, contentType, requiredFreshnessDays, timeoutMs))
                {
                    return processor(stream);
                }
                // verify content type of responding url
            }
            catch (Exception ex)
            {
                // report failure
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Unexpected exception occurred while processing resource {0}: {1}", name, ex.ToString()));

                // NOTE: we should save the last successfully download/used file and
                // "restore" from this rather than the embedded copy in the case of
                // an error or failure reading from the downloaded file.

                // safely delete the locally cached file (it contains bad data)
                SafeDeleteLocalCacheFile(callingAssembly, name);

                // try again on local data
                using (Stream stream = GetResourceLocal(callingAssembly, name))
                    return processor(stream);
            }

        }

        private class XPPResourceFileProcessor
        {
            public XPPResourceFileProcessor(ResourceFileProcessor processor)
            {
                _processor = processor;
            }
            private ResourceFileProcessor _processor;

            public object ProcessXmlResource(Stream stream)
            {
                XmlPreprocessor preprocessor = new XmlPreprocessor();
                preprocessor.Add("version", new Version(ApplicationEnvironment.ProductVersion));
                preprocessor.Add("language", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                preprocessor.Add("culture", CultureInfo.CurrentUICulture.Name);

                XmlDocument document = new XmlDocument();
                document.Load(stream);
                preprocessor.Munge(document);

                MemoryStream processedStream = new MemoryStream();
                document.Save(processedStream);
                processedStream.Seek(0, SeekOrigin.Begin);

                return _processor(processedStream);
            }

        }

        private Stream GetResource(Assembly assembly, string name, string resourceUrl, string contentType, int requiredFreshnessDays, int timeoutMs)
        {
            // get the path to the resource
            string assemblyResourcePath = FormatResourcePath(assembly, name);

            // in debug builds we need to verify that the resource has been included in the assembly
            DebugVerifyResourcePath(assembly, assemblyResourcePath);

            try
            {
                // first see if there is a fresh enough local file version
                string fileResourcePath = GetLocalCachePath(assemblyResourcePath);
                if (File.Exists(fileResourcePath) &&
                    (File.GetLastWriteTime(fileResourcePath).AddDays(requiredFreshnessDays) > DateTime.Now))
                {
                    return new FileStream(fileResourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }

                // local file doesn't exist or is out of date, try downloading from the web
                if (_enableDownloading && (resourceUrl != String.Empty))
                {
                    using (Stream urlStream = SafeDownloadUrl(resourceUrl, contentType, timeoutMs))
                    {
                        if (urlStream != null)
                        {
                            // got a stream, save it to our cache
                            Directory.CreateDirectory(Path.GetDirectoryName(fileResourcePath));
                            using (FileStream fileStream = new FileStream(fileResourcePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                            {
                                StreamHelper.Transfer(urlStream, fileStream);
                            }

                            // return reference to local file stream
                            return new FileStream(fileResourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }
                        else
                        {
                            if (File.Exists(fileResourcePath))
                                return new FileStream(fileResourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail(String.Format(CultureInfo.InvariantCulture, "Unexpected exception attempting to load resource {0}: {1}", assemblyResourcePath, ex.ToString()));
            }

            // if we get this far it means downloading was disabled, we couldn't find the file
            // on the web, or an unexpected error occurred (e.g. file sharing violation), in
            // these cases fallback to returning the stream from within the assembly
            return assembly.GetManifestResourceStream(assemblyResourcePath);
        }

        private Stream SafeDownloadUrl(string url, string contentType, int timeoutMs)
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(timeoutMs);
                var response = HttpClientService.DefaultClient.GetAsync(url, cts.Token).GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                    return null;

                string responseContentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
                string responseUri = response.RequestMessage?.RequestUri?.AbsoluteUri ?? url;

                if (!responseUri.StartsWith("http://www.msn.com/", StringComparison.OrdinalIgnoreCase)) // This seems weird, but our g-links redirect to www.msn.com (rather than 404) if they aren't deployed. good times
                    Debug.Assert(responseContentType == contentType, "Mismatching content type on downloaded resource. Expected " + contentType + " and got " + responseContentType);
                else
                    Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "The resource url at {0} appears not to be deployed.", url));

                if (responseContentType == contentType)
                {
                    // Copy to memory stream since we need to return a seekable stream
                    var ms = new MemoryStream();
                    response.Content.ReadAsStreamAsync().GetAwaiter().GetResult().CopyTo(ms);
                    ms.Position = 0;
                    return ms;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is HttpRequestException)
            {
                if (ApplicationDiagnostics.TestMode)
                    Trace.WriteLine($"SafeDownloadUrl failed for {url}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the specified resource from the local cache
        /// </summary>
        /// <param name="name">name of resource</param>
        /// <returns>stream to resource</returns>
        public Stream GetResourceLocal(string name)
        {
            return GetResourceLocal(Assembly.GetCallingAssembly(), name);
        }

        private Stream GetResourceLocal(Assembly assembly, string name)
        {
            // calculate the full resource path name
            string assemblyResourcePath = FormatResourcePath(assembly, name);

            // return the resource stream
            return assembly.GetManifestResourceStream(assemblyResourcePath);
        }


        private void SafeDeleteLocalCacheFile(Assembly assembly, string resourceName)
        {
            try
            {
                string resourcePath = FormatResourcePath(assembly, resourceName);
                string localCacheFilePath = GetLocalCachePath(resourcePath);
                File.Delete(localCacheFilePath);
            }
            catch
            {
            }
        }

        private string GetLocalCachePath(string assemblyResourcePath)
        {
            if (_fileCacheBasePath != String.Empty)
                return Path.Combine(_fileCacheBasePath, ConvertResourcePath(assemblyResourcePath, '\\'));
            else
                return String.Empty;
        }

        private string FormatResourcePath(Assembly assembly, string name)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", assembly.GetName().Name, name);
        }

        private string ConvertResourcePath(string resourcePath, char pathSeparator)
        {
            int extensionIndex = resourcePath.LastIndexOf('.');
            string path = resourcePath.Substring(0, extensionIndex);
            string extension = resourcePath.Substring(extensionIndex);
            return path.Replace('.', pathSeparator) + extension;
        }

        private void DebugVerifyResourcePath(Assembly assembly, string assemblyResourcePath)
        {
#if DEBUG
            using (Stream stream = assembly.GetManifestResourceStream(assemblyResourcePath))
            {
                if (stream == null)
                    Debug.Fail("Resource requested from ResourceFileDownloader not included in calling asssembly! (" + assemblyResourcePath + ")");
            }
#endif
        }

        private string _fileCacheBasePath;
        private bool _enableDownloading;
    }

    /// <summary>
    /// Callback for processing the contents of a resource file
    /// </summary>
    public delegate object ResourceFileProcessor(Stream resourceFile);

}
