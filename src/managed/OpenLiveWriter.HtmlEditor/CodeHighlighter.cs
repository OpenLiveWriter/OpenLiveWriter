using System;

namespace OpenLiveWriter.HtmlEditor
{
    public class CodeHighlighter
    {
        public static string StyledHtml(string htmlText)
        {
            // Using Google's prettifier (https://github.com/google/code-prettify)
            const string preOpenStyled = "<pre class=\"prettyprint\">";
            const string preOpen = "<pre>";
            const string preClose = "</pre>";
            htmlText = RemoveCode(htmlText, preOpenStyled, preClose);
            htmlText = RemoveCode(htmlText, preOpen, preClose);

            string styledHtml = $"{preOpenStyled}{htmlText}{preClose}";
            return styledHtml;
        }

        private static string RemoveCode(string htmlText, string preOpen, string preClose)
        {
            string htmlL = htmlText.ToLowerInvariant();
            int index = htmlL.IndexOf(preOpen, StringComparison.InvariantCulture);
            if (index >= 0)
            {
                htmlText = htmlText.Remove(index, preOpen.Length);
                htmlL = htmlText.ToLowerInvariant();

                index = htmlL.IndexOf(preClose, StringComparison.InvariantCulture);
                if (index >= 0)
                {
                    htmlText = htmlText.Remove(index, preClose.Length);
                }

            }

            return htmlText;
        }
    }
}