// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Ribbon.Managed.Configuration
{
    /// <summary>
    /// Provides the default ribbon configuration matching the original Ribbon.xml structure.
    /// </summary>
    public static class DefaultRibbonConfiguration
    {
        /// <summary>
        /// Creates the default ribbon configuration.
        /// </summary>
        public static RibbonConfiguration Create()
        {
            var config = new RibbonConfiguration();

            // Application Menu
            ConfigureApplicationMenu(config);

            // Quick Access Toolbar
            ConfigureQuickAccessToolbar(config);

            // Main Tabs
            config.Tabs.Add(CreateHomeTab());
            config.Tabs.Add(CreateInsertTab());
            config.Tabs.Add(CreateBlogProviderTab());
            config.Tabs.Add(CreatePreviewTab());

            // Contextual Tab Groups - simplified for now
            // These would be expanded when the full CommandId enum is available
            config.ContextualTabGroups.Add(CreateImageToolsGroup());

            return config;
        }

        private static void ConfigureApplicationMenu(RibbonConfiguration config)
        {
            config.ApplicationMenu.CommandId = CommandId.FileMenu;
            config.ApplicationMenu.Label = "File";
            config.ApplicationMenu.MaxRecentItems = 10;

            var majorItems = new MenuGroupConfig { Class = "MajorItems" };
            majorItems.Items.Add(new MenuItemConfig { CommandId = CommandId.NewPost });
            majorItems.Items.Add(new MenuItemConfig { CommandId = CommandId.OpenPost });
            majorItems.Items.Add(new MenuItemConfig { CommandId = CommandId.SavePost });
            majorItems.Items.Add(new MenuItemConfig { CommandId = CommandId.PostAndPublish });
            config.ApplicationMenu.MenuGroups.Add(majorItems);

            var standardItems = new MenuGroupConfig { Class = "StandardItems" };
            standardItems.Items.Add(new MenuItemConfig { CommandId = CommandId.PrintPreview });
            standardItems.Items.Add(new MenuItemConfig { CommandId = CommandId.Print });
            standardItems.Items.Add(new MenuItemConfig { IsSeparator = true });
            standardItems.Items.Add(new MenuItemConfig { CommandId = CommandId.About });
            config.ApplicationMenu.MenuGroups.Add(standardItems);
        }

        private static void ConfigureQuickAccessToolbar(RibbonConfiguration config)
        {
            config.QuickAccessToolbar.CommandId = CommandId.QAT;
            config.QuickAccessToolbar.DefaultCommands.Add(CommandId.SavePost);
            config.QuickAccessToolbar.DefaultCommands.Add(CommandId.Undo);
            config.QuickAccessToolbar.DefaultCommands.Add(CommandId.Redo);
        }

        private static TabConfig CreateHomeTab()
        {
            var tab = new TabConfig
            {
                CommandId = CommandId.HomeTab,
                Label = "Home",
                Keytip = "H",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Clipboard Group - Large Paste button only (Cut/Copy are in context menu)
            var clipboardGroup = CreateGroup(CommandId.ClipboardGroup, "Clipboard", "X");
            clipboardGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.Paste, ButtonType = RibbonButtonType.SplitButton });
            tab.Groups.Add(clipboardGroup);

            // Publish Group - Globe icon, blog dropdown, post draft button
            var publishGroup = CreateGroup(CommandId.PublishGroup, "Publish", "PB");
            publishGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.PostAndPublish, ButtonType = RibbonButtonType.SplitButton });
            publishGroup.Controls.Add(new ComboBoxConfig { CommandId = CommandId.SelectBlog, IsEditable = false, PreferredWidth = 140 });
            publishGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.PostAsDraft, ButtonType = RibbonButtonType.Button, PreferredSize = RibbonGroupSize.Medium });
            tab.Groups.Add(publishGroup);

            // Font Group - Dropdowns at top, small formatting buttons below
            var fontGroup = CreateGroup(CommandId.None, "Font", "FN");
            fontGroup.Controls.Add(new ComboBoxConfig { CommandId = CommandId.FontFamily, PreferredWidth = 120 });
            fontGroup.Controls.Add(new ComboBoxConfig { CommandId = CommandId.FontSize, PreferredWidth = 45 });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Bold, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Italic, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Underline, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Strikethrough, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Subscript, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Superscript, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ColorPickerConfig { CommandId = CommandId.FontBackgroundColor, ColorTemplate = RibbonColorTemplate.HighlightColors });
            fontGroup.Controls.Add(new ColorPickerConfig { CommandId = CommandId.FontColor, ColorTemplate = RibbonColorTemplate.StandardColors });
            tab.Groups.Add(fontGroup);

            // HTML Styles Group - Paragraph button and style gallery
            var htmlStylesGroup = CreateGroup(CommandId.SemanticHtmlGroup, "HTML styles", "HS");
            htmlStylesGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ParagraphGroup, ButtonType = RibbonButtonType.DropDownButton });
            htmlStylesGroup.Controls.Add(new GalleryConfig 
            { 
                CommandId = CommandId.SemanticHtmlGallery, 
                GalleryType = RibbonGalleryType.InRibbon,
                ItemWidth = 72,
                ItemHeight = 48,
                Columns = 1,
                MaxRows = 1
            });
            tab.Groups.Add(htmlStylesGroup);

            // Insert Group - Medium-sized buttons with text labels (using split commands with short labels)
            var insertGroup = CreateGroup(CommandId.InsertGroup, "Insert", "I");
            insertGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertLink, ButtonType = RibbonButtonType.SplitButton, PreferredSize = RibbonGroupSize.Medium });
            insertGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertImageSplit, ButtonType = RibbonButtonType.DropDownButton, PreferredSize = RibbonGroupSize.Medium });
            insertGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertVideoSplit, ButtonType = RibbonButtonType.DropDownButton, PreferredSize = RibbonGroupSize.Medium });
            tab.Groups.Add(insertGroup);

            // Editing Group - Spell check
            var editingGroup = CreateGroup(CommandId.None, "Editing", "E");
            editingGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.CheckSpelling });
            tab.Groups.Add(editingGroup);

            return tab;
        }

        private static TabConfig CreateInsertTab()
        {
            var tab = new TabConfig
            {
                CommandId = CommandId.InsertTab,
                Label = "Insert",
                Keytip = "N",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Breaks Group
            var breaksGroup = CreateGroup(CommandId.BreaksGroup, "Breaks", "B");
            breaksGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertHorizontalLine });
            breaksGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertClearBreak });
            tab.Groups.Add(breaksGroup);

            // Tables Group
            var tablesGroup = CreateGroup(CommandId.TablesGroup, "Tables", "T");
            tablesGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertTable, ButtonType = RibbonButtonType.DropDownButton });
            tab.Groups.Add(tablesGroup);

            // Media Group
            var mediaGroup = CreateGroup(CommandId.MediaGroup, "Media", "M");
            mediaGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertPictureFromFile, ButtonType = RibbonButtonType.SplitButton });
            mediaGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertVideoFromWeb });
            mediaGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertVideoFromFile });
            mediaGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertEmoticon, ButtonType = RibbonButtonType.DropDownButton });
            tab.Groups.Add(mediaGroup);

            // Plugins Group
            var pluginsGroup = CreateGroup(CommandId.PluginsGroup, "Content Source", "C");
            pluginsGroup.VisibleModes = RibbonApplicationMode.WithPlugins;
            pluginsGroup.Controls.Add(new GalleryConfig { CommandId = CommandId.PluginsGallery, GalleryType = RibbonGalleryType.InRibbon });
            tab.Groups.Add(pluginsGroup);

            return tab;
        }

        private static TabConfig CreateBlogProviderTab()
        {
            var tab = new TabConfig
            {
                CommandId = CommandId.BlogProviderTab,
                Label = "Blog Account",
                Keytip = "A",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Blog Group
            var blogGroup = CreateGroup(CommandId.BlogProviderBlogGroup, "Blog", "B");
            blogGroup.Controls.Add(new GalleryConfig { CommandId = CommandId.SelectBlog, GalleryType = RibbonGalleryType.InRibbon });
            blogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.AddWeblog });
            blogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ConfigureWeblog });
            tab.Groups.Add(blogGroup);

            // Theme Group
            var themeGroup = CreateGroup(CommandId.BlogProviderThemeGroup, "Theme", "T");
            themeGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.UpdateWeblogStyle });
            tab.Groups.Add(themeGroup);

            return tab;
        }

        private static TabConfig CreatePreviewTab()
        {
            var tab = new TabConfig
            {
                CommandId = CommandId.PreviewTab,
                Label = "Preview",
                Keytip = "P",
                VisibleModes = RibbonApplicationMode.Preview
            };

            // Browser Group
            var browserGroup = CreateGroup(CommandId.BrowserGroup, "Browser", "B");
            browserGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ViewPreview });
            tab.Groups.Add(browserGroup);

            // Preview Group
            var previewGroup = CreateGroup(CommandId.PreviewGroup, "Preview", "P");
            previewGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ClosePreview });
            tab.Groups.Add(previewGroup);

            return tab;
        }

        private static ContextualTabGroupConfig CreateImageToolsGroup()
        {
            var group = new ContextualTabGroupConfig
            {
                CommandId = CommandId.None, // Placeholder
                GroupType = RibbonContextualTabGroup.ImageTools,
                Label = "Picture Tools"
            };

            var formatTab = new TabConfig
            {
                CommandId = CommandId.None, // Placeholder
                Label = "Format",
                Keytip = "JP",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Effects Group
            var effectsGroup = CreateGroup(CommandId.None, "Effects", "E");
            effectsGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ImageRotate, ButtonType = RibbonButtonType.DropDownButton });
            effectsGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ImageTilt, ButtonType = RibbonButtonType.DropDownButton });
            effectsGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ImageCrop });
            effectsGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ImageReset });
            formatTab.Groups.Add(effectsGroup);

            group.Tabs.Add(formatTab);
            return group;
        }

        private static GroupConfig CreateGroup(CommandId commandId, string label, string keytip)
        {
            return new GroupConfig
            {
                CommandId = commandId,
                Label = label,
                Keytip = keytip,
                VisibleModes = RibbonApplicationMode.All
            };
        }
    }
}
