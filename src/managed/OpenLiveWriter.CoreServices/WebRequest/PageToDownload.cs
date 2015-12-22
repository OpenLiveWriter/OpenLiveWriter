// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Information used to download a page and its references
    /// </summary>
    public class PageToDownload
    {
        public PageToDownload(LightWeightHTMLDocument htmlDocument, string url, string rootFileName) :
            this(htmlDocument, url, rootFileName, null)
        {
        }

        public PageToDownload(LightWeightHTMLDocument htmlDocument, string url, string rootFileName, PageToDownload parentInfo)
        {
            ParentInfo = parentInfo;
            _lightweightHTMLDocument = htmlDocument;
            _urlToReplace = url;
            _anchor = UrlHelper.GetAnchorIdentifier(url);
            m_fileName = rootFileName;
        }

        /// <summary>
        /// The Parent of this page
        /// </summary>
        public PageToDownload ParentInfo;

        /// <summary>
        /// The name of the file representing this page
        /// </summary>
        public string FileName
        {
            get
            {

                if (m_fileName == null)
                {
                    string fileName = string.Empty;

                    if (UrlHelper.IsUrl(AbsoluteUrl))
                        fileName = UrlHelper.GetFileNameWithoutExtensionForUrl(AbsoluteUrl) + UrlHelper.GetExtensionForUrl(AbsoluteUrl);

                    if (fileName == string.Empty)
                        fileName = "index.htm";

                    if (fileName.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
                        fileName = fileName + ".htm";

                    // If the path gets really long, just lop it off since the name isn't that important
                    // for subframes
                    if (ParentInfo != null && fileName.Length > MAX_SUBFILE_NAME)
                        fileName = Guid.NewGuid().ToString().Split('-')[0] + ".htm";

                    fileName = FileHelper.GetValidFileName(fileName);

                    // finally, make sure the filename ends with .htm
                    if (Path.GetExtension(fileName) != ".htm" &&
                        Path.GetExtension(fileName) != ".html" &&
                        Path.GetDirectoryName(fileName) != ".css" &&
                        Path.GetDirectoryName(fileName) != ".js")
                    {
                        fileName = Path.GetFileNameWithoutExtension(fileName) + ".htm";
                    }

                    m_fileName = fileName;
                }
                return m_fileName;
            }
            set
            {
                m_fileName = value;
            }
        }
        private string m_fileName;
        private const int MAX_SUBFILE_NAME = 20;

        /// <summary>
        /// The absolute url that should be used to when addressing this page
        /// </summary>
        public string AbsoluteUrl
        {
            get
            {
                if (m_absoluteUrl == null)
                    m_absoluteUrl = _urlToReplace;
                return m_absoluteUrl;
            }
            set
            {
                m_absoluteUrl = value;
            }
        }
        private string m_absoluteUrl = null;

        /// <summary>
        /// The url that was actually used in the page
        /// </summary>
        public string UrlToReplace
        {
            get
            {
                return _urlToReplace;
            }
            set
            {
                _urlToReplace = value;
            }
        }
        private string _urlToReplace = null;

        private string _anchor = null;

        /// <summary>
        /// The relative url, including file name, that should be used to refer to the
        /// file stored in the relative path
        /// </summary>
        public string RelativeUrl
        {
            get
            {
                string baseUrl = Path.Combine(RelativeBaseUrl, FileName);
                if (_anchor != null)
                {
                    baseUrl = baseUrl + "#" + _anchor;
                }
                return baseUrl;
            }
        }

        /// <summary>
        /// The relative url to the base directory in which the file should be stored.
        /// </summary>
        public string RelativeBaseUrl
        {
            get
            {
                if (IsRootPage)
                    return "";
                else
                    return "references";

            }
        }

        /// <summary>
        /// The relative path, including the file name, where the file representing this
        /// page should be stored
        /// </summary>
        public string RelativePath
        {
            get
            {
                return Path.Combine(RelativeBasePath, FileName);
            }
        }

        /// <summary>
        /// The relative directory in which the file should be stored
        /// </summary>
        public string RelativeBasePath
        {
            get
            {
                if (IsRootPage)
                    return "";
                else
                    return DirectoryToken;
            }
        }

        public string DirectoryToken
        {
            get
            {
                if (IsRootPage)
                {
                    return _directoryToken;
                }
                else
                    return ParentInfo.DirectoryToken;
            }
        }
        private string _directoryToken = Guid.NewGuid().ToString();

        public bool IsRootPage
        {
            get
            {
                return (ParentInfo == null);
            }
        }

        /// <summary>
        /// The relative file path where any referenced files should be stored
        /// </summary>
        public string ReferencedFileRelativePath
        {
            get
            {
                if (IsRootPage)
                    return DirectoryToken;
                else
                    return DirectoryToken + "/" + "references";
            }
        }

        /// <summary>
        /// The relative url that should be used to reference files stored in the referencedFileRelativePath
        /// </summary>
        public string ReferencedFileRelativeUrl
        {
            get
            {
                if (m_referencedFileRelativeUrl == null)
                {
                    // The top level page should use a unique token for storing its
                    // referenced files.  This will help later when trying to make sure
                    // paths can be escaped (GUIDs will be safe to replace).
                    //
                    // Subframes get just the last group of digits from the guids.  This
                    // doesn't guarantee their uniqueness, but since they are all namespaced
                    // by folders, thats ok.  This is provide some protection in the event
                    // that the sub frames are flattened downstream.
                    if (IsRootPage)
                        m_referencedFileRelativeUrl = DirectoryToken;
                    else
                        m_referencedFileRelativeUrl = "references";
                }
                return m_referencedFileRelativeUrl;
            }
        }
        private string m_referencedFileRelativeUrl = null;

        public string GetRelativeUrlForReferencedPage(PageToDownload pageBeingReferenced)
        {
            if (pageBeingReferenced.IsRootPage)
            {
                if (IsRootPage)
                    return pageBeingReferenced.FileName;
                else
                    return "../" + pageBeingReferenced.FileName;
            }
            else
            {
                if (IsRootPage)
                    return DirectoryToken + "/" + pageBeingReferenced.FileName;
                else
                    return pageBeingReferenced.FileName;
            }
        }

        /// <summary>
        /// The IHTMLDocument2 from which HTML should be composed
        /// </summary>
        public LightWeightHTMLDocument LightWeightHTMLDocument
        {
            get
            {
                return _lightweightHTMLDocument;
            }
        }
        private LightWeightHTMLDocument _lightweightHTMLDocument = null;

        /// <summary>
        ///
        /// </summary>
        /// <param name="reference"></param>
        public void AddReference(ReferenceToDownload reference)
        {
            // if we haven't already generated the list of references, we can safely add this
            if (_referencesToDownload == null)
            {
                if (!_explicitReferences.Contains(reference))
                    _explicitReferences.Add(reference);
            }
            else
            {
                // Since we've already generated the new reference list, we should go ahead
                // and add this to the end
                ReferenceToDownload[] newReferenceList = new ReferenceToDownload[_referencesToDownload.Length + 1];
                _referencesToDownload.CopyTo(newReferenceList, 0);
                newReferenceList[_referencesToDownload.Length + 1] = reference;
                _referencesToDownload = newReferenceList;
            }
        }
        private ArrayList _explicitReferences = new ArrayList();

        /// <summary>
        /// The list of references that should be downloaded and escaped for this page.
        /// </summary>
        public ReferenceToDownload[] References
        {
            get
            {
                if (_referencesToDownload == null)
                {
                    ArrayList resourceList = new ArrayList();

                    // track urls that are already on the list
                    ArrayList addedUrls = new ArrayList();

                    // for this page, go get its references
                    foreach (UrlInfo urlInfo in this.LightWeightHTMLDocument.ResourceUrlInfos)
                    {
                        // Don't add the same URL more than once!
                        string url = urlInfo.Url;

                        if (!addedUrls.Contains(url) && url != string.Empty && url != null)
                        {

                            ReferenceToDownload download =
                                new ReferenceToDownload(url, this, urlInfo.Url);
                            resourceList.Add(download);
                            addedUrls.Add(url);
                        }
                    }

                    // Now add the explicit references
                    foreach (ReferenceToDownload explicitReference in _explicitReferences)
                    {
                        if (!resourceList.Contains(explicitReference))
                        {
                            resourceList.Add(explicitReference);
                            addedUrls.Add(explicitReference.AbsoluteUrl);
                        }
                    }

                    _referencesToDownload = (ReferenceToDownload[])resourceList.ToArray(typeof(ReferenceToDownload));

                }
                return _referencesToDownload;
            }
        }
        private ReferenceToDownload[] _referencesToDownload = null;

    }
}
