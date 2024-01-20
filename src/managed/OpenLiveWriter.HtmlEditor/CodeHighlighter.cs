using System;

namespace OpenLiveWriter.HtmlEditor
{
    public class CodeHighlighter
    {
        private const int TAB_SIZE = 4;

        private const string PRE_OPEN = "<pre>";
        private const string PRE_CLOSE = "</pre>";
        private const string PRE_OPEN_STYLED = "<pre class=\"prettyprint\">";
        private const string PRETTIFY_SCRIPT_NAME = "run_prettify.js";
        private const string DEFAULT_SKIN = CodeHighlighterSkins.Sunburst;

        private static readonly string _prettifyScript =
            $"<script src=\"https://cdn.rawgit.com/google/code-prettify/master/loader/{PRETTIFY_SCRIPT_NAME}?skin={DEFAULT_SKIN}\"></script>";

        public static string StyledHtml(string htmlText, string innerHtml)
        {
            // Using Google's prettifier (https://github.com/google/code-prettify)
            htmlText = SwapTabsForSpaces(htmlText);
            htmlText = ReplaceLineEndings(htmlText);
            htmlText = RemoveCode(htmlText, PRE_OPEN_STYLED, PRE_CLOSE);
            htmlText = RemoveCode(htmlText, PRE_OPEN, PRE_CLOSE);

            bool addScript = ShouldAddJsScript(innerHtml);
            string script = addScript ? _prettifyScript : string.Empty;

            string styledHtml = $"{script}{PRE_OPEN_STYLED}{htmlText}{PRE_CLOSE}";
            return styledHtml;
        }

        private static string ReplaceLineEndings(string htmlText)
        {
            htmlText = htmlText.Replace(Environment.NewLine, "");
            return htmlText;
        }

        private static string SwapTabsForSpaces(string text)
        {
            return text.Replace("\t", " ".PadLeft(TAB_SIZE));
        }

        private static bool ShouldAddJsScript(string innerHtml)
        {
            var body = innerHtml.ToLowerInvariant();
            if (body.Contains(PRETTIFY_SCRIPT_NAME))
            {
                return false;
            }

            return true;
        }

        private static string RemoveCode(string htmlText, string open, string close)
        {
            string htmlL = htmlText.ToLowerInvariant();
            int index = htmlL.IndexOf(open, StringComparison.InvariantCulture);
            if (index >= 0)
            {
                htmlText = htmlText.Remove(index, open.Length);
                htmlL = htmlText.ToLowerInvariant();

                index = htmlL.IndexOf(close, StringComparison.InvariantCulture);
                if (index >= 0)
                {
                    htmlText = htmlText.Remove(index, close.Length);
                }

            }

            return htmlText;
        }
    }

    // To be used when adding support for skins 
    // For example: https://cdn.rawgit.com/google/code-prettify/master/loader/run_prettify.js?skin=sunburst
    // All Styles: https://rawgit.com/google/code-prettify/master/styles/index.html
    public static class CodeHighlighterSkins
    {
        public const string Default = "default";
        public const string Sunburst = "sunburst";
        public const string SonsOfObsidian = "sons-of-obsidian";
        public const string Desert = "desert";
    }
}