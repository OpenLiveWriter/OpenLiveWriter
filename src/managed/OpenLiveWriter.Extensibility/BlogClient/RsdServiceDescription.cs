// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;

namespace OpenLiveWriter.Extensibility.BlogClient
{
    public class RsdApi
    {
        public string Name = String.Empty;
        public bool Preferred = false;
        public string ApiLink = String.Empty;
        public string BlogId = String.Empty;
        public IDictionary Settings = new Hashtable();
    }

    /// <summary>
    /// The back-end posting service for a weblog
    /// </summary>
    public class RsdServiceDescription
    {
        /// <summary>
        /// Source URL for for this service description
        /// </summary>
        public string SourceUrl = String.Empty;

        /// <summary>
        /// Title of homepage this service is associated with
        /// </summary>
        public string HomepageTitle = String.Empty;

        /// <summary>
        /// Link to homepage this service is associated with
        /// </summary>
        public string HomepageLink = String.Empty;

        /// <summary>
        /// Name of back-end posting engine (e.g. Blogger, Typepad, etc.)
        /// </summary>
        public string EngineName = String.Empty;

        /// <summary>
        /// Url for back-end posting engine (e.g. www.typepad.com)
        /// </summary>
        public string EngineLink = String.Empty;

        /// <summary>
        /// Supported Apis
        /// </summary>
        public RsdApi[] Apis = new RsdApi[] { };

        public RsdApi ScanForApi(string apiName)
        {
            foreach (RsdApi api in Apis)
            {
                if (api.Name.ToUpperInvariant() == apiName.ToUpperInvariant())
                    return api;
            }

            // api not found
            return null;
        }
    }
}

