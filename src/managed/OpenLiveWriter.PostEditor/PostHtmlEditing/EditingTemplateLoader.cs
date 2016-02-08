// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Text;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Loads a blog editing template and injects editor-required HTML.
    /// </summary>
    internal class EditingTemplateLoader : LightWeightHTMLDocumentIterator
    {
        private EditingTemplateLoader(string html, IElementBehaviorManager behaviorManager)
            : base(html)
        {
            _templateBuilder = new StringBuilder();
            _behaviorManager = behaviorManager;
            Parse();
        }

        /// <summary>
        /// Loads the blog editing template.
        /// </summary>
        /// <param name="blogId"></param>
        /// <returns></returns>
        internal static string LoadBlogTemplate(string blogId, BlogEditingTemplateType templateType, bool forceRTL)
        {
            using (PostHtmlEditingSettings templateSettings = new PostHtmlEditingSettings(blogId))
            {
                string html = templateSettings.GetEditorTemplateHtml(templateType, forceRTL);
                if (html == null || html == String.Empty ||
                    html.IndexOf(BlogEditingTemplate.POST_TITLE_MARKER) == -1 ||
                    html.IndexOf(BlogEditingTemplate.POST_BODY_MARKER) == -1)
                {
                    html = BlogEditingTemplate.GetDefaultTemplateHtml(forceRTL, true);
                }

                return html;
            }
        }

        internal static void CleanupUnusedTemplates()
        {
            try
            {
                foreach (string blogId in BlogSettings.GetBlogIds())
                {
                    using (PostHtmlEditingSettings templateSettings = new PostHtmlEditingSettings(blogId))
                        templateSettings.CleanupUnusedTemplates();
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception occurred cleaning up unused templates: " + ex.ToString());
            }
        }

        /// <summary>
        /// Returns the processed template HTML.
        /// </summary>
        /// <returns></returns>
        private string GetTemplateHtml()
        {
            return _templateBuilder.ToString();
        }
        /// <summary>
        /// The stringbuilder that holds the post-processed HTML for the template.
        /// </summary>
        StringBuilder _templateBuilder;

        #region LightWeightHTMLDocumentIterator Overrides

        protected override void OnEndTag(EndTag tag)
        {
            if (tag.NameEquals("head"))
            {
                _templateBuilder.Append("<style>");
                AppendBehaviorStyles();
                _templateBuilder.Append("</style>");
                AppendBehaviorObjectTag();
            }
            base.OnEndTag(tag);
        }

        protected override void DefaultAction(Element el)
        {
            _templateBuilder.Append(el.ToString());
        }
        #endregion

        #region HTML Insertion Routines
        /// <summary>
        /// Appends the style HTML for attaching element editing behaviors.
        /// </summary>
        private void AppendBehaviorStyles()
        {
            string behaviorStyles = _behaviorManager.BehaviorStyles;
            _templateBuilder.Append(behaviorStyles);

            //_templateBuilder.Append("img { behavior: url(#default#IMG_BEHAVIOR) }");
        }

        /// <summary>
        /// Appends the behavior factory object element (required for MSHTML editing behaviors).
        /// </summary>
        private void AppendBehaviorObjectTag()
        {
            string objectTags = _behaviorManager.BehaviorObjectTags;
            _templateBuilder.Append(objectTags);

            //_templateBuilder.Append(@"<object id='IMG_BEHAVIOR' clsid='clsid:3C0C37AD-21B5-41f4-A25E-59259B0ED874' style='visibility: hidden' width='0px' height='0px'> </object>");
        }

        private IElementBehaviorManager _behaviorManager;

        #endregion

        internal static string CreateTemplateWithBehaviors(string html, IElementBehaviorManager elementBehaviorManager)
        {
            EditingTemplateLoader loader = new EditingTemplateLoader(html, elementBehaviorManager);
            return loader.GetTemplateHtml();
        }
    }
}
