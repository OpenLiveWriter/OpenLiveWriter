// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Collections;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.PostEditor;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for PostHtmlEditingSettings.
    /// </summary>
    public class PostHtmlEditingSettings : IDisposable
    {
        private string _blogId;
        public PostHtmlEditingSettings(string blogId)
        {
            _blogId = blogId;
            using (SettingsPersisterHelper blogSettings = BlogSettings.GetWeblogSettingsKey(blogId))
            {
                _editorTemplateSettings = blogSettings.GetSubSettings("EditorTemplate");
            }
        }

        public string LastEditingView
        {
            get { return _editorTemplateSettings.GetString(LAST_EDITING_VIEW, String.Empty); }
            set { _editorTemplateSettings.SetString(LAST_EDITING_VIEW, value); }
        }
        private const string LAST_EDITING_VIEW = "LastEditingView";

        public bool EditUsingBlogStylesIsSet
        {
            get { return _editorTemplateSettings.HasValue(EDIT_USING_STYLES); }
        }

        public bool EditUsingBlogStyles
        {
            get { return _editorTemplateSettings.GetBoolean(EDIT_USING_STYLES, LastEditingView != EditingViews.Normal); }
            set { _editorTemplateSettings.SetBoolean(EDIT_USING_STYLES, value); }
        }
        private const string EDIT_USING_STYLES = "EditUsingStyles";

        public bool DisplayWebLayoutWarning
        {
            get { return _editorTemplateSettings.GetBoolean(DISPLAY_WEB_LAYOUT_WARNING, true); }
            set { _editorTemplateSettings.SetBoolean(DISPLAY_WEB_LAYOUT_WARNING, value); }
        }
        private const string DISPLAY_WEB_LAYOUT_WARNING = "DisplayWebLayoutWarning";

        public BlogEditingTemplateFile[] EditorTemplateHtmlFiles
        {
            get
            {
                SettingsPersisterHelper templates = _editorTemplateSettings.GetSubSettings(EDITOR_TEMPLATES_KEY);
                string[] templateTypes = templates.GetNames();
                BlogEditingTemplateFile[] templateFiles = new BlogEditingTemplateFile[templateTypes.Length];
                for (int i = 0; i < templateTypes.Length; i++)
                {
                    string templateTypeStr = templateTypes[i];
                    string templateFile = templates.GetString(templateTypeStr, BlogEditingTemplate.GetBlogTemplateDir(_blogId));
                    BlogEditingTemplateType templateType =
                        (BlogEditingTemplateType)BlogEditingTemplateType.Parse(typeof(BlogEditingTemplateType), templateTypeStr);
                    templateFiles[i] = new BlogEditingTemplateFile(templateType, templateFile);
                }
                return templateFiles;
            }
            set
            {
                if (_editorTemplateSettings.HasSubSettings(EDITOR_TEMPLATES_KEY))
                    _editorTemplateSettings.UnsetSubsettingTree(EDITOR_TEMPLATES_KEY);
                for (int i = 0; i < value.Length; i++)
                {
                    SettingsPersisterHelper templates = _editorTemplateSettings.GetSubSettings(EDITOR_TEMPLATES_KEY);
                    BlogEditingTemplateFile templateFile = value[i];
                    templates.SetString(templateFile.TemplateType.ToString(), MakeRelative(templateFile.TemplateFile));
                }
            }
        }
        private const string EDITOR_TEMPLATES_KEY = "templates";

        public string GetEditorTemplateHtml(BlogEditingTemplateType templateType, bool forceRTL)
        {
            SettingsPersisterHelper templates = _editorTemplateSettings.GetSubSettings(EDITOR_TEMPLATES_KEY);
            string templateHtmlFile = templates.GetString(templateType.ToString(), null);
            // Sometimes templateHtmlFile is relative, sometimes it is already absolute (from older builds).
            templateHtmlFile = MakeAbsolute(templateHtmlFile);

            if (templateHtmlFile != null && File.Exists(templateHtmlFile))
            {
                string templateHtml;
                using (StreamReader reader = new StreamReader(templateHtmlFile, Encoding.UTF8))
                    templateHtml = reader.ReadToEnd();

                if (File.Exists(templateHtmlFile + ".path"))
                {
                    string origPath = File.ReadAllText(templateHtmlFile + ".path");
                    string newPath = Path.Combine(Path.GetDirectoryName(templateHtmlFile), Path.GetFileName(origPath));
                    string newUri = UrlHelper.SafeToAbsoluteUri(new Uri(newPath));
                    templateHtml = templateHtml.Replace(origPath, newUri);
                }

                /*Parse meta tags in order to set CSS3 compatibility*/
                Regex metatag = new Regex(@"<(?i:meta)(\s)+(?i:http-equiv)(\s)*=""(?:X-UA-Compatible)""(\s)+(?i:content)(\s)*=""(?i:IE=edge)""(\s)*/>");
                Match match = metatag.Match(templateHtml);
                
                if (!match.Success)
                {
                    // prepend the metatag to make css3 compatible at least on edge (Windows 8+)
                    int i = templateHtml.IndexOf("<HEAD>", StringComparison.OrdinalIgnoreCase);
                    if (i > 0)
                    {                        
                        templateHtml = ( templateHtml.Substring(0, i + 6) 
                                        + "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />" 
                                        + templateHtml.Substring(i + 6) );
                    }
                }


                return templateHtml;
            }
            else
            {
                return BlogEditingTemplate.GetDefaultTemplateHtml(forceRTL, templateType != BlogEditingTemplateType.Normal);
            }
        }

        private string MakeAbsolute(string templateHtmlFile)
        {
            if (templateHtmlFile == null)
                return null;

            if (!Path.IsPathRooted(templateHtmlFile))
                templateHtmlFile = Path.Combine(BlogEditingTemplate.GetBlogTemplateDir(_blogId), templateHtmlFile);
            return templateHtmlFile;
        }

        private string MakeRelative(string templateHtmlFile)
        {
            if (templateHtmlFile == null)
                return null;

            if (!Path.IsPathRooted(templateHtmlFile))
                return templateHtmlFile;

            string filename = Path.GetFileName(templateHtmlFile);
            if (File.Exists(Path.Combine(BlogEditingTemplate.GetBlogTemplateDir(_blogId), filename)))
                return filename;
            else
            {
                Trace.Fail("Failed to make relative path: " + templateHtmlFile);
                return templateHtmlFile;
            }
        }

        internal void CleanupUnusedTemplates()
        {
            try
            {
                using (SettingsPersisterHelper templates = _editorTemplateSettings.GetSubSettings(EDITOR_TEMPLATES_KEY))
                {
                    // get the list of templates which are legit
                    ArrayList templatesInUse = new ArrayList();
                    foreach (string key in templates.GetNames())
                        templatesInUse.Add(MakeAbsolute(templates.GetString(key, String.Empty)).Trim().ToLower(CultureInfo.CurrentCulture));

                    // delete each of the template files in the directory which
                    // are not contained in our list of valid templates
                    if (templatesInUse.Count > 0)
                    {
                        string templateDirectory = Path.GetDirectoryName((string)templatesInUse[0]);
                        if (Directory.Exists(templateDirectory))
                        {
                            string[] templateFiles = Directory.GetFiles(templateDirectory, "*.htm");
                            foreach (string templateFile in templateFiles)
                            {
                                string templateFileNormalized = templateFile.Trim().ToLower(CultureInfo.CurrentCulture);
                                if (!templatesInUse.Contains(templateFileNormalized))
                                    CleanupTemplateAndSupportingFiles(templateFile);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Error occurred cleaning up unused templates: " + ex.ToString());
            }
        }

        private void CleanupTemplateAndSupportingFiles(string templateFile)
        {
            try
            {
                // determine the name of the supporting file directory
                string templateContents;
                using (StreamReader reader = new StreamReader(templateFile, Encoding.UTF8))
                    templateContents = reader.ReadToEnd().ToLower(CultureInfo.CurrentCulture);

                // determine the template path
                string templateDirectory = Path.GetDirectoryName(templateFile);
                string templatePathReference = UrlHelper.InsureTrailingSlash(UrlHelper.CreateUrlFromPath(templateDirectory)).Replace("%20", " ");
                int pathRefIndex = templateContents.IndexOf(templatePathReference.ToLower(CultureInfo.CurrentCulture));

                // if there are references to the template path within the file then
                // use it to derive the supporting file directory and delete it
                if (pathRefIndex != -1)
                {
                    int endPathRefIndex = pathRefIndex + templatePathReference.Length;
                    int nextSlashIndex = templateContents.IndexOf('/', endPathRefIndex);
                    int length = nextSlashIndex - endPathRefIndex;
                    Trace.Assert(length > 0);
                    string supportingFilePath = templateContents.Substring(endPathRefIndex, length);

                    // delete the supporting file directory
                    Directory.Delete(Path.Combine(templateDirectory, supportingFilePath), true);
                }

                // delete the template file
                File.Delete(templateFile);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error occurred cleaning up template {0}: {1}", templateFile, ex.ToString()));
            }
        }

        public void Dispose()
        {
            if (_editorTemplateSettings != null)
            {
                _editorTemplateSettings.Dispose();
                _editorTemplateSettings = null;
            }
        }

        private SettingsPersisterHelper _editorTemplateSettings;
    }

}
