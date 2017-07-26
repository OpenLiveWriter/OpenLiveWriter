using System;
using mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    public class CodeHighlighter
    {
        const string prettyprint = "prettyprint";
        const string preOpen = "<pre>";
        const string preClose = "</pre>";
        static readonly string preOpenStyled = $"<pre class=\"{prettyprint}\">";

        private static readonly string codePrettifyScript =
            "<script src=\"https://cdn.rawgit.com/google/code-prettify/master/loader/run_prettify.js\"></script>";

        public static string StyledHtml(string htmlText, string innerHtml)
        {
            // Using Google's prettifier (https://github.com/google/code-prettify)

            // Add reference to the public JS 
            bool addScript = ShouldAddJsScript(innerHtml);

            htmlText = RemoveCode(htmlText, preOpenStyled, preClose);
            htmlText = RemoveCode(htmlText, preOpen, preClose);

            string script = addScript ? codePrettifyScript : string.Empty;

            string styledHtml = $"{script}{preOpenStyled}{htmlText}{preClose}";
            return styledHtml;
        }

        private static bool ShouldAddJsScript(string innerHtml)
        {
            var body = innerHtml.ToLowerInvariant();
            if (body.Contains(codePrettifyScript))
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
}