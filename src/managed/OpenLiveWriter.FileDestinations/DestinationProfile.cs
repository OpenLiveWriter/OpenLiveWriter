// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// Represents a DestinationProfile.
    /// </summary>
    public class DestinationProfile
    {
        public enum DestType { FTP = 0, WINDOWS = 1 };

        public DestinationProfile()
        {
        }

        private string name = "";
        /// <summary>
        /// Profile name as it will appear in any UI
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string id;
        /// <summary>
        /// Unique profile ID (typically same as name).
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        private DestType type;
        /// <summary>
        /// The type of this destination (0=FTP|1=Windows)
        /// </summary>
        public DestType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        private string websiteURL = "";
        /// <summary>
        /// The base URL of this mapping
        /// </summary>
        public string WebsiteURL
        {
            get
            {
                return websiteURL;
            }
            set
            {
                websiteURL = value;
            }
        }

        private string ftpServer = "";
        /// <summary>
        /// The hostname of the FTP server
        /// </summary>
        public string FtpServer
        {
            get
            {
                return ftpServer;
            }
            set
            {
                ftpServer = value;
            }
        }

        private string userName = "";
        /// <summary>
        /// The username (used for authentication)
        /// </summary>
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }

        private string password = "";
        /// <summary>
        /// Profile name as it will appear in any UI
        /// </summary>
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        private string localPublishPath = "";
        /// <summary>
        /// The base local/remote publish directory
        /// </summary>
        public string LocalPublishPath
        {
            get
            {
                return localPublishPath;
            }
            set
            {
                localPublishPath = value;
            }
        }

        private string ftpPublishPath = "";
        /// <summary>
        /// The base ftp publish directory
        /// </summary>
        public string FtpPublishPath
        {
            get
            {
                return ftpPublishPath;
            }
            set
            {
                ftpPublishPath = value;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
