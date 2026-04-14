// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Web;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// This class represents a page's reference that should be downloaded (for example, an image).
    /// </summary>
    public class ReferenceToDownload
    {
        /// <summary>
        /// Constructs a new reference download info
        /// </summary>
        /// <param name="url">The Url of the reference</param>
        /// <param name="parentInfo">The parent info that references this url</param>
        public ReferenceToDownload(string url, PageToDownload parentPageToDownload)
        {
            m_urlToReplace = url;
            ParentInfo = parentPageToDownload;
        }

        public ReferenceToDownload(string url, PageToDownload parentPageToDownload, string absoluteUrl) : this(url, parentPageToDownload)
        {
            m_absoluteUrl = absoluteUrl;
        }

        /// <summary>
        /// The name of the file represented by this reference
        /// </summary>
        public string FileName
        {
            get
            {
                if (m_fileName == null)
                {
                    if (UrlHelper.IsUrl(AbsoluteUrl))
                    {
                        Uri uri = new Uri(AbsoluteUrl);
                        string leftPart = uri.GetLeftPart(UriPartial.Path);

                        if (leftPart.IndexOfAny(Path.GetInvalidPathChars()) == -1)
                            m_fileName = FileHelper.GetValidFileName(Path.GetFileName(leftPart), 30);

                        // If we still can't get a good filename, just
                        // grab the url and make it a file name
                        if (m_fileName == string.Empty || m_fileName == null)
                            m_fileName = Guid.NewGuid().ToString("n").Substring(0, 8);

                        m_fileName = HttpUtility.UrlDecode(m_fileName);
                    }
                    else
                        m_fileName = "unknown file";

                    m_fileName = FileHelper.GetValidFileName(m_fileName);
                }
                return m_fileName;
            }
        }
        private string m_fileName;

        public void SetFileName(ref string fileName)
        {
            fileName = FileHelper.GetValidFileName(fileName);
            m_fileName = fileName;
        }

        /// <summary>
        /// The relative path, including file name, that this reference should be written to
        /// </summary>
        public string RelativePath
        {
            get
            {
                return Path.Combine(ParentInfo.ReferencedFileRelativePath, FileName);
            }
        }

        /// <summary>
        /// The relative url, including file name, that should be used to reference this item
        /// </summary>
        public string RelativeUrl
        {
            get
            {
                string path = Path.Combine(ParentInfo.ReferencedFileRelativeUrl, FileName);

                // reverse the separators (Path.combine uses '\' as a separator)
                return path.Replace("\\", "/");
            }
        }

        /// <summary>
        /// The absolute url to the item (the url from which the item can be downloaded).
        /// </summary>
        public string AbsoluteUrl
        {
            get
            {
                if (m_absoluteUrl == null)
                    m_absoluteUrl = UrlHelper.EscapeRelativeURL(ParentInfo.AbsoluteUrl, UrlToReplace);
                return m_absoluteUrl;
            }
        }
        private string m_absoluteUrl = null;

        public string GetRelativeUrlForReference(PageToDownload referencingPage)
        {
            if (!referencingPage.IsRootPage && ParentInfo.IsRootPage)
                return FileName;
            else if (referencingPage.IsRootPage && !ParentInfo.IsRootPage)
                return ParentInfo.DirectoryToken + "/references/" + FileName;
            else
                return
                    RelativeUrl;
        }

        public string UrlToReplace
        {
            get
            {
                return m_urlToReplace;
            }
        }
        private string m_urlToReplace;

        /// <summary>
        /// The PageDownloadInfo that parents this item.
        /// </summary>
        public PageToDownload ParentInfo;
    }
}
