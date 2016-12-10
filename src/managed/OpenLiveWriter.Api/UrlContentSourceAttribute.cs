// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using JetBrains.Annotations;

    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContentFromUrl method to enable creation of new content from URLs. The source of
    /// this URL can either be the page the user was navigated to when they pressed the "Blog This"
    /// button or a URL that is pasted or dragged into the editor.
    /// Plugin classes which override this method must also be declared with the UrlContentSourceAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UrlContentSourceAttribute : Attribute
    {
        /// <summary>
        /// The progress caption
        /// </summary>
        [NotNull]
        private string progressCaption = string.Empty;

        /// <summary>
        /// The progress message
        /// </summary>
        [NotNull]
        private string progressMessage = string.Empty;

        /// <summary>
        /// The URL pattern
        /// </summary>
        [NotNull]
        private string urlPattern = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlContentSourceAttribute"/> class.
        /// </summary>
        /// <param name="urlPattern">
        /// Regular expression which indicates which URL this content source can handle
        /// </param>
        public UrlContentSourceAttribute([NotNull] string urlPattern)
        {
            this.UrlPattern = urlPattern;
        }

        /// <summary>
        /// Gets or sets the optional caption used in progress message.
        /// </summary>
        [NotNull]
        public string ProgressCaption
        {
            get
            {
                return this.progressCaption;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(UrlContentSourceAttribute.ProgressCaption));
                }

                this.progressCaption = value;
            }
        }

        /// <summary>
        /// Gets or sets the optional descriptive text used in progress message.
        /// </summary>
        [NotNull]
        public string ProgressMessage
        {
            get
            {
                return this.progressMessage;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(UrlContentSourceAttribute.ProgressMessage));
                }

                this.progressMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the UrlContentSource requires a progress dialog during the execution of its CreateContentFromUrl
        /// method. This value should be specified if the content source performs network operations during content creation.
        /// Defaults to false.
        /// </summary>
        public bool RequiresProgress { get; set; } = false;

        /// <summary>
        /// Gets or sets the regular expression which indicates which URL this content source can handle
        /// </summary>
        [NotNull]
        public string UrlPattern
        {
            get
            {
                return this.urlPattern;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(UrlContentSourceAttribute.UrlPattern));
                }

                if (!UrlContentSourceAttribute.ValidateRegex(value))
                {
                    throw new ArgumentException(
                              string.Format(
                                  CultureInfo.CurrentCulture,
                                  "The regular expression \"{0}\" is invalid.",
                                  value),
                              nameof(UrlContentSourceAttribute.UrlPattern));
                }

                this.urlPattern = value;
            }
        }

        /// <summary>
        /// Validates the regex.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool ValidateRegex([NotNull] string pattern)
        {
            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
