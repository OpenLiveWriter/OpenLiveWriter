// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Facilitates HTML formatting for marshalling formats in an HTML control.
    /// </summary>
    public interface IHtmlGenerationService
    {
        /// <summary>
        /// Generates the HTML for linking to a set of files.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        string GenerateHtmlFromFiles(string[] files);

        /// <summary>
        /// Generates the HTML for a link.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="linkText"></param>
        /// <param name="newWindow"></param>
        /// <returns></returns>
        string GenerateHtmlFromLink(string url, string linkText, string linkTitle, string rel, bool newWindow);

        /// <summary>
        /// Transforms an HTML fragment for insertion into the editor.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        string GenerateHtmlFromHtmlFragment(string html, string baseUrl);

        /// <summary>
        /// Transforms a plain-text fragment into HTML for insertion into the editor.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string GenerateHtmlFromPlainText(string text);

        string CleanupHtml(string html, string baseUrl, HtmlCleanupRule cleanupRule);

    }

    public enum HtmlCleanupRule
    {
        Normal,
        PreserveTables
    }
}
