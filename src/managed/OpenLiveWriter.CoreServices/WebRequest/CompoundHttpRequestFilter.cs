// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System.Net;

    using JetBrains.Annotations;

    /// <summary>
    /// Allow chaining together of http request filters
    /// </summary>
    public class CompoundHttpRequestFilter
    {
        /// <summary>
        /// The filters
        /// </summary>
        [NotNull]
        private readonly HttpRequestFilter[] filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundHttpRequestFilter"/> class.
        /// </summary>
        /// <param name="filters">The filters.</param>
        private CompoundHttpRequestFilter([NotNull] HttpRequestFilter[] filters)
        {
            this.filters = filters;
        }

        /// <summary>
        /// Creates the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns>A HttpRequestFilter.</returns>
        [NotNull]
        public static HttpRequestFilter Create([NotNull] HttpRequestFilter[] filters)
            => new CompoundHttpRequestFilter(filters).Filter;

        /// <summary>
        /// Filters the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        private void Filter([NotNull] HttpWebRequest request)
        {
            foreach (var filter in this.filters)
            {
                filter(request);
            }
        }
    }
}