// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// Summary description for WebPublishSettings.
    /// </summary>
    public class WebPublishSettings
    {

        /// <summary>
        /// construction from an existing destination and publish folder
        /// </summary>
        /// <param name="destinationId"></param>
        /// <param name="publishPath"></param>
        public WebPublishSettings(string destinationId, string publishPath)
        {
            Destination = new WebPublishDestination(destinationId);
            PublishPath = publishPath;
        }

        /// <summary>
        /// Unique-id for the destination
        /// </summary>
        public WebPublishDestination Destination
        {
            get
            {
                return webPublishDestination;
            }
            set
            {
                if (webPublishDestination.Id != value.Id)
                {
                    // update the value
                    webPublishDestination = value;

                    // notify listeners
                    if (DestinationChanged != null)
                        DestinationChanged(this, EventArgs.Empty);
                }
            }
        }
        private WebPublishDestination webPublishDestination = new WebPublishDestination(null);

        /// <summary>
        /// Publish path
        /// </summary>
        public string PublishPath
        {
            get
            {
                // automatically cleanup publish-paths
                return CleanupPath(publishPath).TrimStart('\\', '/');
            }
            set
            {
                if (publishPath != value)
                {
                    // update value
                    publishPath = value;

                    // fire event
                    if (PublishPathChanged != null)
                        PublishPathChanged(this, EventArgs.Empty);
                }
            }
        }
        private string publishPath = String.Empty;

        public string FullPublishPath
        {
            get
            {
                string fullPublishPath = PublishRootPath + "/" + PublishPath;
                string cleanPath = CleanupPath(fullPublishPath);
                return cleanPath;
            }
        }

        public string PublishRootPath
        {
            get
            {
                if (Destination.Profile.Type == DestinationProfile.DestType.FTP)
                {
                    return Destination.Profile.FtpPublishPath;
                }
                else
                {
                    return Destination.Profile.LocalPublishPath;
                }
            }
        }

        public bool DestinationSupportsRss
        {
            get
            {
                string websiteUrl = Destination.Profile.WebsiteURL;
                return (websiteUrl != null && websiteUrl.Length > 0);
            }
        }

        public string PublishUrl
        {
            get
            {
                string destURL = Destination.Profile.WebsiteURL;
                if ((destURL.Length > 0) && !destURL.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    destURL = destURL + "/";
                return destURL;
            }
        }

        /// <summary>
        /// Notify users that the publishing destination has changed
        /// </summary>
        public event EventHandler DestinationChanged;

        /// <summary>
        /// Notify users that the publish folder has changed
        /// </summary>
        public event EventHandler PublishPathChanged;


        /// <summary>
        /// Cleans up the file separators in a path name based on whether the current
        /// destination type is FTP or Windows.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>a clean path name with the correct separators, and double separators removed</returns>
        private string CleanupPath(string path)
        {
            string pathSeparator = "/";
            if (Destination.Profile != null)
            {
                if (Destination.Profile.Type == DestinationProfile.DestType.WINDOWS)
                {
                    pathSeparator = "\\";
                    //replace reversed separators
                    path = path.Replace("/", pathSeparator);
                }
                else
                {
                    pathSeparator = "/";
                    //replace reversed separators
                    path = path.Replace("\\", pathSeparator);
                }
            }

            //prevent the accidental stripping of double backslashes in shared network folders
            //by saving the first slash off in the prefix that can be added back after stripping
            //double separators
            //example:  \\myhost\wwwroot
            string prefix = "";
            if (pathSeparator == "\\" && path.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                prefix = "\\";
                path = path.Substring(1);
            }

            //strip any double or file separators
            string cleanPath = path;
            do
            {
                path = cleanPath;
                cleanPath = path.Replace(pathSeparator + pathSeparator, pathSeparator);
            } while (cleanPath != path);

            //add the prefix back
            cleanPath = prefix + cleanPath;

            return cleanPath;
        }

    }

}
