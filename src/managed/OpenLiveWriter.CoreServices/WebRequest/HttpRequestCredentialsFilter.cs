// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System.Net;

    using JetBrains.Annotations;

    /// <summary>
    /// Class HttpRequestCredentialsFilter.
    /// </summary>
    public class HttpRequestCredentialsFilter
    {
        /// <summary>
        /// The digest only
        /// </summary>
        private readonly bool digestOnly;

        /// <summary>
        /// The password
        /// </summary>
        [CanBeNull]
        private readonly string password;

        /// <summary>
        /// The URL
        /// </summary>
        [CanBeNull]
        private readonly string url;

        /// <summary>
        /// The user name
        /// </summary>
        [CanBeNull]
        private readonly string username;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestCredentialsFilter"/> class.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="url">The URL.</param>
        /// <param name="digestOnly">if set to <c>true</c> [digest only].</param>
        private HttpRequestCredentialsFilter(
            [CanBeNull] string username,
            [CanBeNull] string password,
            [CanBeNull] string url,
            bool digestOnly)
        {
            this.username = username;
            this.password = password;
            this.url = url;
            this.digestOnly = digestOnly;
        }

        /// <summary>
        /// Creates the specified user name.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="url">The URL.</param>
        /// <param name="digestOnly">if set to <c>true</c> [digest only].</param>
        /// <returns>A HttpRequestFilter.</returns>
        [NotNull]
        public static HttpRequestFilter Create(
                [CanBeNull] string username,
                [CanBeNull] string password,
                [CanBeNull] string url,
                bool digestOnly)
            => new HttpRequestFilter(new HttpRequestCredentialsFilter(username, password, url, digestOnly).Filter);

        /// <summary>
        /// Filters the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        private void Filter([NotNull] HttpWebRequest request)
        {
            request.Credentials = HttpRequestHelper.CreateHttpCredentials(
                this.username,
                this.password,
                this.url,
                this.digestOnly);
        }
    }
}
