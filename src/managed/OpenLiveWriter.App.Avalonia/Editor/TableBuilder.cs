// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Builds a well-formed <c>&lt;table&gt;</c> element for insertion at the caret,
    /// mirroring the Windows "Insert Table" dialog (rows × columns, optional header
    /// row, optional width). Pure/deterministic so the produced markup can be
    /// asserted headlessly without a live WebView.
    /// </summary>
    public static class TableBuilder
    {
        /// <summary>Upper bound guarding against accidental huge tables.</summary>
        public const int MaxDimension = 100;

        /// <summary>
        /// Builds a table with <paramref name="rows"/> total rows and
        /// <paramref name="columns"/> columns. When <paramref name="headerRow"/> is
        /// true the first row is emitted as a <c>&lt;thead&gt;</c> of <c>&lt;th&gt;</c>
        /// cells and the remaining rows as a <c>&lt;tbody&gt;</c> of <c>&lt;td&gt;</c>
        /// cells; otherwise all rows go in the <c>&lt;tbody&gt;</c>. An optional
        /// <paramref name="width"/> (e.g. "100%" or "500") is applied as an inline
        /// <c>width</c> style. Values are clamped to sane bounds.
        /// </summary>
        public static string BuildTableHtml(int rows, int columns, bool headerRow = true, string width = null)
        {
            rows = Clamp(rows);
            columns = Clamp(columns);

            var sb = new StringBuilder();
            sb.Append("<table");
            string widthStyle = NormalizeWidth(width);
            if (widthStyle != null)
                sb.Append(" style=\"width:").Append(widthStyle).Append("\"");
            sb.Append(">");

            int bodyStartRow = 0;
            if (headerRow)
            {
                sb.Append("<thead><tr>");
                for (int c = 0; c < columns; c++)
                    sb.Append("<th></th>");
                sb.Append("</tr></thead>");
                bodyStartRow = 1;
            }

            sb.Append("<tbody>");
            for (int r = bodyStartRow; r < rows; r++)
            {
                sb.Append("<tr>");
                for (int c = 0; c < columns; c++)
                    sb.Append("<td></td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");

            sb.Append("</table>");
            return sb.ToString();
        }

        /// <summary>
        /// Normalizes a user-supplied width into a CSS length. Accepts a bare number
        /// (treated as pixels), an explicit percentage, or a px value. Returns null
        /// when the input is empty/invalid (no width style applied).
        /// </summary>
        internal static string NormalizeWidth(string width)
        {
            if (string.IsNullOrWhiteSpace(width))
                return null;

            string w = width.Trim();
            if (w.EndsWith("%", StringComparison.Ordinal))
            {
                string num = w.Substring(0, w.Length - 1).Trim();
                return IsPositiveNumber(num) ? num + "%" : null;
            }

            if (w.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            {
                string num = w.Substring(0, w.Length - 2).Trim();
                return IsPositiveNumber(num) ? num + "px" : null;
            }

            return IsPositiveNumber(w) ? w + "px" : null;
        }

        private static bool IsPositiveNumber(string s) =>
            double.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out double v) && v > 0;

        private static int Clamp(int value) =>
            value < 1 ? 1 : (value > MaxDimension ? MaxDimension : value);
    }
}
