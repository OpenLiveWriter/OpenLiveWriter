// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml;

namespace OpenLiveWriter.Publishing.Accounts
{
    /// <summary>A single API entry from an RSD document (e.g. the MetaWeblog endpoint).</summary>
    public sealed class RsdApi
    {
        public string Name { get; set; } = string.Empty;
        public bool Preferred { get; set; }
        public string ApiLink { get; set; } = string.Empty;
        public string BlogId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Parsed RSD (Really Simple Discovery) service description — the cross-platform,
    /// MSHTML-free counterpart to the Windows <c>RsdServiceDescription</c>.
    /// </summary>
    public sealed class RsdServiceDescription
    {
        public string SourceUrl { get; set; } = string.Empty;
        public string HomepageLink { get; set; } = string.Empty;
        public string EngineName { get; set; } = string.Empty;
        public string EngineLink { get; set; } = string.Empty;
        public IReadOnlyList<RsdApi> Apis { get; set; } = Array.Empty<RsdApi>();

        /// <summary>Case-insensitive lookup of an API by name (e.g. "MetaWeblog").</summary>
        public RsdApi ScanForApi(string apiName)
        {
            foreach (RsdApi api in Apis)
            {
                if (string.Equals(api.Name, apiName, StringComparison.OrdinalIgnoreCase))
                    return api;
            }
            return null;
        }
    }

    /// <summary>Outcome of an RSD auto-detection attempt.</summary>
    public sealed class RsdDetectionResult
    {
        public bool Found => !string.IsNullOrEmpty(EndpointUrl);
        public string EndpointUrl { get; set; } = string.Empty;
        public string BlogId { get; set; } = string.Empty;
        public string EngineName { get; set; } = string.Empty;
        public string RsdUrl { get; set; } = string.Empty;
        public RsdServiceDescription ServiceDescription { get; set; }
    }

    /// <summary>HTTP fetch seam so detection can be unit-tested without live network.</summary>
    public interface IRsdHttpFetcher
    {
        /// <summary>Fetches the text at <paramref name="url"/>, or null on failure.</summary>
        string Fetch(string url);
    }

    /// <summary>
    /// Cross-platform RSD-based provider endpoint auto-detection, ported from the Windows
    /// <c>RsdServiceDetector</c> without the MSHTML dependency. The HTML/XML parsing steps
    /// are pure static methods (fixture-testable); the network fetch is isolated behind
    /// <see cref="IRsdHttpFetcher"/>.
    ///
    /// Flow: homepage HTML → <c>&lt;link rel="EditURI" ...&gt;</c> → rsd.xml →
    /// <c>&lt;api name="MetaWeblog" apiLink=... blogID=...&gt;</c> → endpoint URL.
    /// </summary>
    public static class RsdServiceDetector
    {
        private const string MetaWeblogApiName = "MetaWeblog";

        // Matches each <link ...> tag; attributes are parsed out separately.
        private static readonly Regex LinkTagRegex = new Regex(
            @"<link\b[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex AttributeRegex = new Regex(
            "(?<name>[\\w:-]+)\\s*=\\s*(?:\"(?<v1>[^\"]*)\"|'(?<v2>[^']*)'|(?<v3>[^\\s\"'>]+))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Finds the RSD (EditURI) link in a blog homepage's HTML and resolves it against
        /// <paramref name="homepageUrl"/>. Recognizes <c>rel="EditURI"</c> and, as a
        /// fallback, <c>type="application/rsd+xml"</c>. Returns null when none is present.
        /// </summary>
        public static string FindRsdUrl(string homepageHtml, string homepageUrl)
        {
            if (string.IsNullOrEmpty(homepageHtml))
                return null;

            foreach (Match linkMatch in LinkTagRegex.Matches(homepageHtml))
            {
                var attrs = ParseAttributes(linkMatch.Value);

                bool isEditUri = attrs.TryGetValue("rel", out string rel) &&
                    string.Equals(rel, "EditURI", StringComparison.OrdinalIgnoreCase);
                bool isRsdType = attrs.TryGetValue("type", out string type) &&
                    string.Equals(type, "application/rsd+xml", StringComparison.OrdinalIgnoreCase);

                if ((isEditUri || isRsdType) && attrs.TryGetValue("href", out string href)
                    && !string.IsNullOrWhiteSpace(href))
                {
                    return ResolveUrl(homepageUrl, href.Trim());
                }
            }

            return null;
        }

        /// <summary>
        /// Parses RSD XML into a <see cref="RsdServiceDescription"/> (or null when no APIs
        /// are found). Namespace-agnostic and tolerant of trailing junk, mirroring the
        /// Windows parser. <paramref name="sourceUrl"/> is used to resolve relative links.
        /// </summary>
        public static RsdServiceDescription ParseRsd(string rsdXml, string sourceUrl = "")
        {
            if (string.IsNullOrWhiteSpace(rsdXml))
                return null;

            var description = new RsdServiceDescription { SourceUrl = sourceUrl ?? string.Empty };
            var apis = new List<RsdApi>();

            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                    IgnoreWhitespace = true
                };

                using var stringReader = new StringReader(rsdXml.TrimStart());
                using var reader = XmlReader.Create(stringReader, settings);

                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    switch (reader.LocalName.ToUpperInvariant())
                    {
                        case "ENGINENAME":
                            description.EngineName = reader.ReadElementContentAsString().Trim();
                            break;
                        case "ENGINELINK":
                            description.EngineLink = ResolveUrl(sourceUrl, reader.ReadElementContentAsString().Trim());
                            break;
                        case "HOMEPAGELINK":
                            description.HomepageLink = ResolveUrl(sourceUrl, reader.ReadElementContentAsString().Trim());
                            break;
                        case "API":
                            var api = new RsdApi();
                            if (reader.HasAttributes)
                            {
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    switch (reader.LocalName.ToUpperInvariant())
                                    {
                                        case "NAME":
                                            api.Name = (reader.Value ?? string.Empty).Trim();
                                            break;
                                        case "PREFERRED":
                                            api.Preferred = string.Equals(
                                                (reader.Value ?? string.Empty).Trim(), "true",
                                                StringComparison.OrdinalIgnoreCase);
                                            break;
                                        case "APILINK":
                                        case "RPCLINK": // radio-userland uses rpcLink
                                            api.ApiLink = ResolveUrl(sourceUrl, (reader.Value ?? string.Empty).Trim());
                                            break;
                                        case "BLOGID":
                                            api.BlogId = (reader.Value ?? string.Empty).Trim();
                                            break;
                                    }
                                }
                                reader.MoveToElement();
                            }
                            apis.Add(api);
                            break;
                    }
                }
            }
            catch (XmlException)
            {
                // Some providers (historically TypePad) append junk after the RSD body,
                // which trips the XML parser; keep whatever APIs we parsed before the fault.
            }

            if (apis.Count == 0)
                return null;

            description.Apis = apis;
            return description;
        }

        /// <summary>
        /// Selects the MetaWeblog endpoint from a parsed description: prefers the API named
        /// "MetaWeblog", then any API flagged preferred, then the first API. Returns null
        /// when no usable <c>apiLink</c> is present.
        /// </summary>
        public static RsdApi SelectMetaWeblogApi(RsdServiceDescription description)
        {
            if (description?.Apis == null || description.Apis.Count == 0)
                return null;

            RsdApi metaWeblog = description.ScanForApi(MetaWeblogApiName);
            if (metaWeblog != null && !string.IsNullOrEmpty(metaWeblog.ApiLink))
                return metaWeblog;

            RsdApi preferred = null;
            foreach (RsdApi api in description.Apis)
            {
                if (api.Preferred && !string.IsNullOrEmpty(api.ApiLink))
                {
                    preferred = api;
                    break;
                }
            }
            if (preferred != null)
                return preferred;

            foreach (RsdApi api in description.Apis)
            {
                if (!string.IsNullOrEmpty(api.ApiLink))
                    return api;
            }

            return null;
        }

        /// <summary>
        /// Full auto-detection: fetch the homepage, find the RSD link, fetch the RSD, parse
        /// it, and resolve the MetaWeblog endpoint. Returns a result whose
        /// <see cref="RsdDetectionResult.Found"/> is false when detection could not
        /// complete. Never throws for a fetch/parse miss.
        /// </summary>
        public static RsdDetectionResult Detect(string homepageUrl, IRsdHttpFetcher fetcher)
        {
            if (fetcher == null) throw new ArgumentNullException(nameof(fetcher));

            var result = new RsdDetectionResult();
            if (string.IsNullOrWhiteSpace(homepageUrl))
                return result;

            string homepageHtml = SafeFetch(fetcher, homepageUrl);
            if (string.IsNullOrEmpty(homepageHtml))
                return result;

            string rsdUrl = FindRsdUrl(homepageHtml, homepageUrl);
            if (string.IsNullOrEmpty(rsdUrl))
                return result;

            result.RsdUrl = rsdUrl;

            string rsdXml = SafeFetch(fetcher, rsdUrl);
            if (string.IsNullOrEmpty(rsdXml))
                return result;

            RsdServiceDescription description = ParseRsd(rsdXml, rsdUrl);
            if (description == null)
                return result;

            result.ServiceDescription = description;
            result.EngineName = description.EngineName;

            RsdApi api = SelectMetaWeblogApi(description);
            if (api != null)
            {
                result.EndpointUrl = api.ApiLink;
                result.BlogId = api.BlogId;
            }

            return result;
        }

        private static string SafeFetch(IRsdHttpFetcher fetcher, string url)
        {
            try { return fetcher.Fetch(url); }
            catch { return null; }
        }

        private static Dictionary<string, string> ParseAttributes(string tag)
        {
            var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in AttributeRegex.Matches(tag))
            {
                string name = m.Groups["name"].Value;
                string value = m.Groups["v1"].Success ? m.Groups["v1"].Value
                    : m.Groups["v2"].Success ? m.Groups["v2"].Value
                    : m.Groups["v3"].Value;
                attrs[name] = value;
            }
            return attrs;
        }

        // A URI scheme prefix such as "http:", "https:", "ftp:". Used to decide whether a
        // URL is absolute — we can't rely on Uri.TryCreate(UriKind.Absolute) because on
        // Unix a leading-slash path is (mis)parsed as an absolute file URI.
        private static readonly Regex SchemeRegex = new Regex(
            "^[a-zA-Z][a-zA-Z0-9+.-]*:", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Resolves <paramref name="url"/> against <paramref name="baseUrl"/> when relative.</summary>
        private static string ResolveUrl(string baseUrl, string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            if (SchemeRegex.IsMatch(url))
                return url; // already absolute (has a scheme)
            if (!string.IsNullOrEmpty(baseUrl) &&
                Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri baseUri) &&
                Uri.TryCreate(baseUri, url, out Uri combined))
            {
                return combined.ToString();
            }
            return url;
        }
    }

    /// <summary>
    /// Default <see cref="IRsdHttpFetcher"/> backed by <see cref="HttpClient"/>. Used by the
    /// live (opt-in) path; unit tests inject an in-memory fake instead.
    /// </summary>
    public sealed class HttpRsdFetcher : IRsdHttpFetcher
    {
        private readonly HttpClient _httpClient;
        private static readonly HttpClient SharedHttpClient = new HttpClient();

        public HttpRsdFetcher(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? SharedHttpClient;
        }

        public string Fetch(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("User-Agent", "OpenLiveWriter");
            HttpResponseMessage response = _httpClient.Send(request);
            response.EnsureSuccessStatusCode();
            using var stream = response.Content.ReadAsStream();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
