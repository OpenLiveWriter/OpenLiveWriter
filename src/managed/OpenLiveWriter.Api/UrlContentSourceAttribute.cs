// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContentFromUrl method to enable creation of new content from URLs. The source of
    /// this URL can either be the page the user was navigated to when they pressed the "Blog This"
    /// button or a URL that is pasted or dragged into the editor.
    /// Plugin classes which override this method must also be declared with the UrlContentSourceAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UrlContentSourceAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance of a UrlContentSourceAttribute
        /// </summary>
        /// <param name="urlPattern">Regular expression which indicates which URL this content source can handle</param>
        public UrlContentSourceAttribute(string urlPattern)
        {
            UrlPattern = urlPattern;
        }

        /// <summary>
        /// Regular expression which indicates which URL this content source can handle
        /// </summary>
        public string UrlPattern
        {
            get
            {
                return _urlPattern;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("UrlContentSource.UrlPattern");

                if (!ValidateRegex(value))
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "The regular expression \"{0}\" is invalid.", value), "UrlContentSource.UrlPattern");

                _urlPattern = value;
            }
        }
        private string _urlPattern;

        /// <summary>
        /// Indicates that the UrlContentSource requires a progress dialog during the execution of its CreateContentFromUrl
        /// method. This value should be specified if the content source performs network operations during content creation.
        /// Defaults to false.
        /// </summary>
        public bool RequiresProgress { get { return _requiresProgress; } set { _requiresProgress = value; } }
        private bool _requiresProgress = false;

        /// <summary>
        /// Optional caption used in progress message.
        /// </summary>
        public string ProgressCaption
        {
            get
            {
                return _progressCaption;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("UrlContentSource.ProgressCaption");

                _progressCaption = value;
            }
        }
        private string _progressCaption = String.Empty;

        /// <summary>
        /// Optional descriptive text used in progress message.
        /// </summary>
        public string ProgressMessage
        {
            get
            {
                return _progressMessage;
            }
            set
            {

                if (value == null)
                    throw new ArgumentNullException("UrlContentSource.ProgressMessage");

                _progressMessage = value;
            }
        }
        private string _progressMessage = String.Empty;

        private bool ValidateRegex(string pattern)
        {
            try
            {
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
