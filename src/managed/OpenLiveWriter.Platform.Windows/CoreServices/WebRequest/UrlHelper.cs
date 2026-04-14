// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Helper utilities for working with Urls.
    /// </summary>
    public class UrlHelper
    {
        /// <summary>
        /// ALWAYS use this instead of Uri.AbsoluteUri!
        ///
        /// The Uri.AbsoluteUri property does escaping of non-ASCII
        /// characters. When done to file URIs, this results in URIs
        /// that MSHTML does not know how to resolve, as it doesn't
        /// know how to interpret the Uri class's UTF-8 encoding
        /// scheme.
        ///
        /// We work around this by un-escaping the escaped high
        /// characters, but leave reserved URI characters alone.
        ///
        /// Fixes bug 741447 - Images loaded from a wpost, when the %TEMP% path contains a char needing html escaping, appear as a red X.
        /// Set %TEMP% to contain a char that requires html escaping.
        /// Open Writer
        /// Insert image
        /// Save Draft
        /// Click New Post
        /// Open draft
        /// </summary>
        public static string SafeToAbsoluteUri(Uri uri)
        {
            string u = uri.AbsoluteUri;

            // Only necessary for file URIs
            if (!uri.IsFile)
                return u;

            return Regex.Replace(u, @"(?:%([0-9a-z]{2}))+", SafeToAbsoluteUri_Evaluator, RegexOptions.IgnoreCase);
        }

        private static string SafeToAbsoluteUri_Evaluator(Match m)
        {
            int len = m.Length / 3;
            List<byte> bytes = new List<byte>(len);
            foreach (Capture cap in m.Groups[1].Captures)
            {
                int hi = MathHelper.HexToInt(cap.Value[0]);
                int lo = MathHelper.HexToInt(cap.Value[1]);
                bytes.Add((byte)(lo | (hi << 4)));
            }

            string s = Encoding.UTF8.GetString(bytes.ToArray());

            return Regex.Replace(s, @"[:/?#\[\]@%!$&'()*+,;=]", SafeToAbsoluteUri_Evaluator2);
        }

        private static string SafeToAbsoluteUri_Evaluator2(Match match)
        {
            Debug.Assert(match.Length == 1);
            return "%" + ((int)match.Value[0]).ToString("X2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Insures a url has leading http address
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The corrected url</returns>
        public static string FixUpUrl(string url)
        {
            url = url.Trim();
            if (url == string.Empty)
                url = "";
            else if (url.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                url = "http:" + url;
            else if (url.IndexOf(":", StringComparison.OrdinalIgnoreCase) < 0)
                url = "http://" + url;
            return url;
        }

        /// <summary>
        /// Insures that a url has a trailing slash (if it needs it)
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The corrected url</returns>
        public static string InsureTrailingSlash(string url)
        {
            if ((url == null) || (url.Trim() == string.Empty))
                return string.Empty;

            string newUrl = url;
            if (UrlHelper.GetExtensionForUrl(newUrl) == string.Empty && !newUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                newUrl = newUrl + "/";
            }
            return newUrl;
        }

        /// <summary>
        /// Create a valid url from a file path
        /// </summary>
        /// <param name="path">file path</param>
        /// <returns>valid url</returns>
        public static string CreateUrlFromPath(string path)
        {
            return SafeToAbsoluteUri(new Uri(path));
#if FALSE
            // This is the old implementation which exposes problems with URL encoded characters in paths.
            //  (such as c:\temp\232323232%7Ffp%3A%3C%3Dot%3E2378%3D664%3D88%3B%3DXROQDF%3E2323%3A77%3B9%3B537ot1lsi.gif)
            //
            // It was replaced with the above implementation to address this type of bug, (bug #608613)
            // The only downside of using the Uri above is that it doesn't handle UNC paths of the form:
            // \\?\UNC\...

            // allocate buffer to hold url
            uint bufferSize = 4096 ;
            StringBuilder buffer = new StringBuilder(Convert.ToInt32(bufferSize)) ;

            // normalize the url
            int result = Shlwapi.UrlCreateFromPath( path, buffer, ref bufferSize, 0 ) ;

            // successfully converted
            if ( result == HRESULT.S_OK )
            {
                // return URL converted to a .NET URL encoded value
                string url = buffer.ToString();
                url = ShlwapiFileUrlToDotnetEncodedUrl(url); //fixes bug 47859

                try
                {
                    if(new FileInfo(path).FullName != new FileInfo(new Uri(url).LocalPath).FullName)
                    {
                        Trace.Fail("Possible bug encoding/decoding path: " + path);
                    }
                }
                catch(Exception ex)
                {
                    Trace.Fail("Exception while checking path encoding. Original path: " + path + " url from Shlwapi: " + url);
                    throw;
                }
                return url;
            }
                // didn't need conversion
            else if ( result == HRESULT.S_FALSE )

            {
                // docs say that even if we don't need conversion it will
                // copy the buffer we passed it to the result
                Debug.Assert( path.Equals(buffer.ToString()) );

                // return start url
                return path ;
            }
                // unexpected error occurred!
            else
            {
                throw new
                    COMException( "Error calling UrlCreateFromPath for path " + path, result ) ;
            }
#endif
        }

        /// <summary>
        /// Converts a Shlwapi encoded file URL to a dotnet encoded URL
        /// (required for decoding the url string with the URI class)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string ShlwapiFileUrlToDotnetEncodedUrl(string s)
        {
            if (!s.StartsWith(FILE_SCHEME, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Fail("URL is not a file url:", s);
                return s;
            }

            int bufferSize = s.Length;
            bool containsUnsafeChars = false;
            StringBuilder sb = new StringBuilder();
            for (int i = FILE_SCHEME.Length; i < bufferSize; i++)
            {
                char ch = s[i];
                if ((ch == '%') && (i < (bufferSize - 2))) //decode encoded hex bytes
                {
                    int num1 = MathHelper.HexToInt(s[i + 1]);
                    int num2 = MathHelper.HexToInt(s[i + 2]);
                    if ((num1 >= 0) && (num2 >= 0))
                    {
                        byte b = (byte)((num1 << 4) | num2);
                        ch = (char)b;
                        i += 2;
                    }
                }

                if (!containsUnsafeChars) //scan for unsafe chars - fixes bug 482444
                {
                    foreach (char unsafeChar in UNSAFE_URL_CHARS)
                    {
                        if (ch == unsafeChar)
                            containsUnsafeChars = true;
                    }
                }

                sb.Append(ch);
            }

            //re-encode the URL using the .NET urlPathEncoding scheme
            string decodedUrl = sb.ToString();
            string encodedUrl = FILE_SCHEME + HttpUtility.UrlPathEncode(decodedUrl);

            if (containsUnsafeChars) //fixes bug 482444
            {
                foreach (char unsafeChar in UNSAFE_URL_CHARS)
                {
                    string unsafeCharStr = unsafeChar + String.Empty;
                    encodedUrl = encodedUrl.Replace(unsafeCharStr, HttpUtility.UrlEncode(unsafeCharStr)); //URLPathEncode doesn't encode # signs.
                }
            }
            return encodedUrl;
        }
        private const string FILE_SCHEME = "file:";
        private static char[] UNSAFE_URL_CHARS = new char[] { '#' }; //chars not escaped by UrlPathEncode that need to be encoded in the URL

        /// <summary>
        /// Gets the host name portion of a url, not including the 'www'
        /// or returns the full hostname if the hostname is an ip addy
        /// </summary>
        /// <param name="url">The url for which to get the host name</param>
        /// <returns>The host name</returns>
        public static string GetDomain(string url)
        {
            if (!IsUrl(url))
                return null;

            Uri uri = new Uri(url);

            // If the url is at least like x.y.z, split it and return the last 2 parts
            string[] parts = uri.Host.Split('.');
            if (parts.Length > 2)
            {

                return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", parts[parts.Length - 2], parts[parts.Length - 1]);
            }
            else
                return uri.Host;
        }

        /// <summary>
        /// Gets the host name portion of a url, not including the 'www'
        /// or returns the full hostname if the hostname is an ip addy
        /// </summary>
        /// <param name="url">The url for which to get the host name</param>
        /// <returns>The host name</returns>
        public static string GetHostName(string url)
        {
            if (!IsUrl(url))
                return null;
            return new Uri(url).Host;
        }

        public static string GetUrlWithoutAnchorIdentifier(string url)
        {
            int octPosition = url.LastIndexOf('#');
            if (octPosition > -1)
            {
                url = url.Substring(0, octPosition);
            }
            return url;
        }

        public static string GetAnchorIdentifier(string url)
        {
            string anchor = null;
            int octPosition = url.LastIndexOf('#');
            if (octPosition > -1)
            {
                int startPos = octPosition + 1;
                anchor = url.Substring(startPos, url.Length - startPos);
            }
            return anchor;
        }

        public static string GetPrettyUrl(string url, int length)
        {
            if (!IsUrl(url))
                return null;

            if (url.Length > length)
                return url.Substring(0, length - 3) + Res.Get(StringId.WithEllipses);
            else
                return url;

        }

        /// <summary>
        /// Determines whether a string is a valid Url
        /// </summary>
        /// <param name="url">The url to validate</param>
        /// <returns>true if the url is a valid url, otherwise false</returns>
        public static bool IsUrl(string url)
        {
            if (url != null && url.IndexOf("://", StringComparison.OrdinalIgnoreCase) > -1)
            {
                try
                {
                    Uri uri = new Uri(url);
                }
                catch (UriFormatException)
                {
                    return false;
                }
                return true;
            }
            else
                return false;
#if false
            // TODO: For some reason, IsvalidURL is always returning 1 (S_FALSE)
            // no matter what URL you pass into the sucker.
            // Handle only the base URL
            if (url.IndexOf("?") > -1)
                url = url.Substring(0, url.IndexOf("?"));

            int hResult = UrlMon.IsValidURL(
                IntPtr.Zero,
                url,
                0);

            switch (hResult)
            {
                case HRESULT.S_OK:
                    return true;
                case HRESULT.E_INVALIDARG:
                    Trace.Log("IsUrl returned HRESULT.E_INVALIDARG for this url: " + url);
                    return false;
                case HRESULT.S_FALSE:
                default:
                    return false;
            }
#endif
        }

        /// <summary>
        /// Indicates whether the scheme of the current Url is a well known scheme.  This is helpful
        /// since certain urls (like outlook urls of the format outbind://173-000000007A8E4513E635304C91A43CFC57ADB0BA04F52700/)
        /// will validate as legal Uris, yet for many applications will not be useful.
        /// </summary>
        /// <param name="url">The url for which to validate the scheme</param>
        /// <returns>True if the scheme is well known, otherwise false</returns>
        public static bool IsKnownScheme(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                foreach (string scheme in KnownSchemes)
                    if (uri.Scheme == scheme)
                        return true;
            }
            catch (Exception)
            {
                //may occur if the URL is malformed
            }
            return false;
        }

        /// <summary>
        /// Returns true if the string starts with a known scheme, even if it is not a valid URL.
        /// </summary>
        public static bool StartsWithKnownScheme(string url)
        {
            foreach (string scheme in KnownSchemes)
                if (url.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// The list of well known schemes
        /// </summary>
        private static string[] KnownSchemes = new string[] {   Uri.UriSchemeFile, Uri.UriSchemeFtp, Uri.UriSchemeGopher,
                                                                Uri.UriSchemeHttp, Uri.UriSchemeHttps, Uri.UriSchemeMailto,
                                                                Uri.UriSchemeNews, Uri.UriSchemeNntp, "telnet", "wais", "ldap" };

        /// <summary>
        /// Gets the file extension for a url (including ignoring query strings and the like)
        /// </summary>
        /// <param name="url">The url for which to get the file extension</param>
        /// <returns>The file extension (with the .)</returns>
        public static string GetExtensionForUrl(string url)
        {
            // Try to discard the query string, if possible
            if (IsUrl(url))
            {
                url = new Uri(url).GetLeftPart(UriPartial.Path);

            }
            return Path.GetExtension(url);
        }

        /// <summary>
        ///  Returns the name of the file for a given URL
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The name of the file</returns>
        public static string GetFileNameWithoutExtensionForUrl(string url)
        {
            // Try to discard the query string, if possible
            if (IsUrl(url))
            {
                url = new Uri(url).GetLeftPart(UriPartial.Path);

            }
            return Path.GetFileNameWithoutExtension(url);
        }

        /// <summary>
        /// Returns the name of the file for a given URL
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The name of the file</returns>
        public static string GetFileNameForUrl(string url)
        {
            // Try to discard the query string, if possible
            if (IsUrl(url))
            {
                url = new Uri(url).GetLeftPart(UriPartial.Path);

            }
            return Path.GetFileName(url);
        }

        /// <summary>
        /// Gets the content type for a given url
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The content type</returns>
        public static UrlContentTypeInfo GetUrlContentType(string url)
        {
            return ContentTypeHelper.ExpensivelyGetUrlContentType(url, 5000);
        }

        public static bool IsFileUrl(string url)
        {
            if (IsUrl(url))
                return new Uri(url).IsFile;
            else
                return false;
        }

        public static bool IsFile(string url)
        {
            try
            {
                return new Uri(url).IsFile;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        public static bool UrlsAreEqual(string url, string secondUrl)
        {
            if (UrlHelper.IsUrl(url) && UrlHelper.IsUrl(secondUrl))
            {
                Uri uri = new Uri(url);
                Uri secondUri = new Uri(secondUrl);
                return SafeToAbsoluteUri(uri) == SafeToAbsoluteUri(secondUri);
            }
            else
                return url == secondUrl;
        }

        /// <summary>
        /// Escapes a relative URL against a HTML document contained in an IDataObject
        /// </summary>
        /// <param name="baseUrl">The root url against which to escape the relative
        /// URL</param>
        /// <param name="relativeUrl">The relative URL to escape</param>
        /// <returns>The absolute path</returns>
        public static string EscapeRelativeURL(string baseUrl, string relativeUrl)
        {
            // Handle urls that don't contain the base url
            if (relativeUrl.StartsWith(AboutBlank, StringComparison.OrdinalIgnoreCase))
            {
                relativeUrl = relativeUrl.Replace(AboutBlank, "");
            }
            if (relativeUrl.StartsWith(About, StringComparison.OrdinalIgnoreCase))
            {
                relativeUrl = relativeUrl.Replace(About, "");
            }

            if (relativeUrl.StartsWith("//", StringComparison.OrdinalIgnoreCase))
            {
                relativeUrl = relativeUrl.Replace("//", "http://");
            }

            if (!UrlHelper.IsUrl(relativeUrl) && baseUrl != null && baseUrl != string.Empty)
            {
                //bug fix: Use UrlCombine() to compensate for bugs in Shlwapi.UrlCombine()
                relativeUrl = UrlCombine(baseUrl, relativeUrl);
            }

            return relativeUrl;
        }

        /// <summary>
        /// Returns url with appended query string
        /// For example, if url is http://blah?a=1 and parameters is "b=2", this will return http://blah?a=1&b=2
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string AppendQueryParameters(string url, string[] parameters)
        {
            if (String.IsNullOrEmpty(url))
                return url;

            Debug.Assert(IsUrl(url));

            Uri uri = new Uri(url);

            string appendedUrl = url;
            bool queryWasEmpty = String.IsNullOrEmpty(uri.Query);
            if (parameters.Length > 0)
            {
                if (queryWasEmpty)
                    appendedUrl += "?";
                else
                    appendedUrl += "&";
            }

            Debug.Assert(!new List<string>(parameters).Exists((p) => String.IsNullOrEmpty(p)));

            appendedUrl += String.Join("&", parameters);

            Debug.Assert(IsUrl(appendedUrl));

            return appendedUrl;
        }

        /// <summary>
        /// Gets the base Url from a full Url
        /// The baseUrl is considered the scheme+hostname with only the root path.
        /// Use GetBasePathUrl if you want the base directory of a file URL.
        ///
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The base url</returns>
        public static string GetBaseUrl(string url)
        {
            if (url == null || url.Length == 0)
                return String.Empty;

            try
            {
                Uri uri = new Uri(url);
                return uri.GetLeftPart(UriPartial.Authority);
            }
            catch (Exception e)
            {
                if (!url.StartsWith("outbind", StringComparison.OrdinalIgnoreCase)) // Outlook URLs (pre-2007) don't have base URLs
                    Trace.Fail("Failed to parse URI: " + url + "\r\n" + e.ToString());
                return string.Empty;
            }
        }

        public static string GetServerPath(string url)
        {
            if (url == null || url.Length == 0)
                return String.Empty;

            Uri uri = new Uri(url);
            return uri.AbsolutePath;
        }

        /// <summary>
        /// Returns the base path from a full Url.
        /// This basically strips the filename from a URL leaving only its directory.
        /// Warning: if a URL directory is passed in without a trailing slash, the directory
        /// name will be considered a filename, and will be stripped.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetBasePathUrl(string url)
        {
            if (String.IsNullOrEmpty(url))
                return string.Empty;

            try
            {
                //trim off everything after the last slash in the path
                UriBuilder uriBuilder = new UriBuilder(new Uri(url));
                int index = uriBuilder.Path.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    uriBuilder.Path = uriBuilder.Path.Substring(0, index + 1);
                    uriBuilder.Fragment = string.Empty;
                    uriBuilder.Query = string.Empty;
                }

                string baseUrl = SafeToAbsoluteUri(uriBuilder.Uri);
                return baseUrl;
            }
            catch (Exception e)
            {
                if (!url.StartsWith("outbind", StringComparison.OrdinalIgnoreCase)) // Outlook URLs (pre-2007) don't have base URLs
                    Trace.Fail("Failed to parse URI: " + url + "\r\n" + e.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// Cleans up a url
        /// </summary>
        /// <param name="url">The url to clean up</param>
        /// <returns>The clean url</returns>
        public static string CleanUpUrl(string url)
        {
            if (url != null)
            {
                url = url.Replace("&", "&amp;");
                url = url.Replace(AboutBlank, "");
            }
            return url;
        }

        /// <summary>
        /// Returns a hashtable of the query parameters parsed from a URL
        /// </summary>
        /// <param name="url">The url from which to get the query parameters (Note: This will be fooled
        /// by html escaped strings such as &amp; in the url)</param>
        /// <returns>A hashtable of the parameters</returns>
        public static Hashtable GetQueryParams(string url)
        {
            Hashtable queryParams = new DefaultHashtable(new DefaultHashtable.DefaultValuePump(DefaultHashtable.ArrayListDefault));

            Uri uri = new Uri(url);

            string trimmedUrl = uri.Query.TrimStart('?');
            string[] pairs = trimmedUrl.Split('&');
            foreach (string pair in pairs)
            {
                string[] splitPairs = pair.Split('=');
                if (splitPairs.Length == 2)
                    ((ArrayList)queryParams[splitPairs[0]]).Add(splitPairs[1]);
            }

            return queryParams;
        }

        /// <summary>
        /// Helper to get the value of a named query string parameter.
        /// </summary>
        /// <param name="url">Valid url to parse for the parameter</param>
        /// <param name="paramName">The name of the parameter whose value will be returned</param>
        /// <returns>null, if paramName does not exist in query string</returns>
        public static string GetQueryParamValue(string url, string paramName)
        {
            if (String.IsNullOrEmpty(url))
                throw new ArgumentException("Invalid url.");

            if (String.IsNullOrEmpty(paramName))
                throw new ArgumentException("Invalid parameter name.");

            Hashtable queryParams = GetQueryParams(url);
            if (queryParams.ContainsKey(paramName))
            {
                if (((ArrayList)queryParams[paramName]).Count == 0)
                {
                    Debug.Fail("The implementation of GetQueryParams changed, breaking this!");
                    return null;
                }

                return (string)((ArrayList)queryParams[paramName])[0];
            }

            return null;
        }

        public static byte[] GetShortcutFileBytesForUrl(string url)
        {
            // TODO: make this use the URL creation API instead of hacking together the file contents
            string urlFile = "[DEFAULT]\nBASEURL=" + url + "\n[InternetShortcut]\nURL=" + url + "\nModified=1";
            char[] chars = urlFile.ToCharArray();
            byte[] bytes = new byte[urlFile.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(chars[i] & 0xFF);
            }
            return bytes;
        }

        public static string GenerateShortcutFileForUrl(string fileName, string url)
        {
            byte[] shortcutFileBytes = GetShortcutFileBytesForUrl(url);
            string shortcutFile = TempFileManager.Instance.CreateTempFile(fileName + ".url");
            using (FileStream stream = new FileStream(shortcutFile, FileMode.Create, FileAccess.Write))
                stream.Write(shortcutFileBytes, 0, shortcutFileBytes.Length);
            return shortcutFile;
        }

        public static string GetLocalFileUrl(string filePath)
        {
            // Set dontEscape flag to true so that we prevent URI escaping from
            // replacing high ascii or unicode characters which can make the path be invalid

            // We've been running on .Net 2.0 for some time in the field without error
            // (.Net 2.0 changed the behavior of the Uri constructor to ignore the dontEscape bool) so
            // we feel safe simply dropping that value.
            return SafeToAbsoluteUri(new Uri(filePath));
        }

        public static string GetUrlFromShortCutFile(string pathToShortCut)
        {
            const int URL_BUFFER_SIZE = 4096;
            const string SECTION_NAME = "InternetShortcut";
            const string KEY_NAME = "URL";

            if (PathHelper.IsPathUrlFile(pathToShortCut))
            {
                // Read the INI file entry
                StringBuilder urlBuilder = new StringBuilder(URL_BUFFER_SIZE, URL_BUFFER_SIZE);
                Kernel32.GetPrivateProfileString(
                    SECTION_NAME,
                    KEY_NAME,
                    "(no URL Found)",
                    urlBuilder,
                    URL_BUFFER_SIZE,
                    pathToShortCut
                    );

                return urlBuilder.ToString();
            }
            else
                return null;
        }
        /// <summary>
        /// Combines a basepath and relative path to create a complete URL.
        /// Note: this utility provides workarounds to problems with Shlwapi.UrlCombine()
        /// </summary>
        public static string UrlCombine(string baseUrl, string relativeUrl)
        {
            //bug fix: Shlwapi.UrlCombine() fails with combining paths for mhtml:file:// URLs
            if (!baseUrl.StartsWith("mhtml:file://", StringComparison.OrdinalIgnoreCase))
            {
                // UrlCombine is escaping the urls, which means if the URL is already escaped, it is being
                // double escaped.
                if (IsUrl(baseUrl))
                    baseUrl = HttpUtility.UrlDecode(baseUrl);

                relativeUrl = Shlwapi.UrlCombine(
                    baseUrl,
                    relativeUrl);
            }
            else
            {
                while (relativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    relativeUrl = relativeUrl.Substring(1);

                if (baseUrl.IndexOf("!", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    baseUrl = baseUrl.Split('!')[0];
                }

                relativeUrl = String.Format(CultureInfo.InvariantCulture, "{0}!{1}", baseUrl, relativeUrl);
            }
            return relativeUrl;
        }

        public static string UrlCombineIfRelative(string baseUrl, string relativeUrl)
        {
            if (UrlHelper.IsUrl(relativeUrl) || baseUrl == null)
                return relativeUrl;
            else
                return UrlCombine(baseUrl, relativeUrl);
        }

        public static string GetSavedFromString(string url)
        {
            return string.Format(CultureInfo.InvariantCulture, "<!-- saved from url=({0:0000}){1} -->\r\n", url.Length, url);
        }

        public static bool IsUrlLinkable(string url)
        {
            if (UrlHelper.IsUrl(url))
            {
                Uri uri = new Uri(url);
                foreach (string scheme in NonlinkableSchemes)
                    if (uri.Scheme == scheme)
                        return false;

            }
            return true;
        }
        private static string[] NonlinkableSchemes = new string[] { Uri.UriSchemeFile };

        public static bool IsUrlDownloadable(string url)
        {
            bool isDownloadable = false;
            if (UrlHelper.IsUrl(url))
            {
                Uri uri = new Uri(url);
                foreach (string scheme in DownloadableSchemes)
                    if (uri.Scheme == scheme)
                    {
                        isDownloadable = true;
                        break;
                    }
            }

            return isDownloadable;
        }
        private static string[] DownloadableSchemes = new string[] { Uri.UriSchemeFile, Uri.UriSchemeHttp, Uri.UriSchemeHttps };

        public static readonly string AboutBlank = "about:blank";
        public static readonly string About = "about:";
    }
}
