// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Configuration;
using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.PostEditor.Configuration.Settings
{

    public sealed class WeblogSettingsManager
    {
        public static bool EditSettings(IWin32Window owner, string id, Type selectedPanel)
        {
            return EditSettings(owner, id, true, selectedPanel);
        }

        public static bool EditSettings(IWin32Window owner, string id, bool showAccountSettings, Type selectedPanel)
        {
            TemporaryBlogSettings temporaryBlogSettings = TemporaryBlogSettings.ForBlogId(id);
            if (EditSettings(owner, temporaryBlogSettings, showAccountSettings, selectedPanel))
            {
                using (BlogSettings blogSettings = BlogSettings.ForBlogId(id))
                    temporaryBlogSettings.Save(blogSettings);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool EditSettings(IWin32Window owner, TemporaryBlogSettings blogSettings, bool showAccountSettings, Type selectedPanel)
        {
            // make a copy of the blog settings for editing
            TemporaryBlogSettings editableBlogSettings = blogSettings.Clone() as TemporaryBlogSettings;

            // show form
            using (PreferencesForm preferencesForm = new PreferencesForm())
            {
                using (BlogClientUIContextScope uiContextScope = new BlogClientUIContextScope(preferencesForm))
                {
                    // customize form title and behavior
                    preferencesForm.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.WeblogSettings), blogSettings.BlogName);
                    preferencesForm.HideApplyButton();

                    // panels
                    int iPanel = 0;
                    if (showAccountSettings)
                        preferencesForm.SetEntry(iPanel++, new AccountPanel(blogSettings, editableBlogSettings));

                    preferencesForm.SetEntry(iPanel++, new ImagesPanel(blogSettings, editableBlogSettings));
                    preferencesForm.SetEntry(iPanel++, new EditingPanel(blogSettings, editableBlogSettings));
                    preferencesForm.SetEntry(iPanel++, new BlogPluginsPanel(blogSettings, editableBlogSettings));
                    preferencesForm.SetEntry(iPanel++, new AdvancedPanel(blogSettings, editableBlogSettings));
                    preferencesForm.SelectEntry(selectedPanel);

                    // show the dialog
                    return (preferencesForm.ShowDialog(owner) == DialogResult.OK);
                }
            }
        }

        public static bool EditFtpImageUpload(IWin32Window owner, string id)
        {
            using (BlogSettings blogSettings = BlogSettings.ForBlogId(id))
            {
                DialogResult result = FTPSettingsForm.ShowFTPSettingsForm(owner, blogSettings);
                if (result == DialogResult.OK)
                {
                    blogSettings.FileUploadSupport = FileUploadSupport.FTP;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
