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

            // Clipboard Group
            var clipboardGroup = CreateGroup(CommandId.ClipboardGroup, "Clipboard", "X");
            clipboardGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.Paste, ButtonType = RibbonButtonType.SplitButton });
            clipboardGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.Cut });
            tab.Groups.Add(clipboardGroup);

            // Publish Group
            var publishGroup = CreateGroup(CommandId.PublishGroup, "Publish", "PB");
            publishGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.PostAndPublish, ButtonType = RibbonButtonType.SplitButton });
            tab.Groups.Add(publishGroup);

            // Paragraph Group
            var paragraphGroup = CreateGroup(CommandId.ParagraphGroup, "Paragraph", "P");
            paragraphGroup.Controls.Add(new GalleryConfig { CommandId = CommandId.SemanticHtmlGallery, GalleryType = RibbonGalleryType.InRibbon });
            paragraphGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.Bullets });
            paragraphGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.Numbers });
            paragraphGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.Blockquote });
            paragraphGroup.Controls.Add(new SeparatorConfig());
            paragraphGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.AlignLeft });
            paragraphGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.AlignCenter });
            paragraphGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.AlignRight });
            paragraphGroup.Controls.Add(new SeparatorConfig());
            paragraphGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.Indent });
            paragraphGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.Outdent });
            tab.Groups.Add(paragraphGroup);

            // Font Group
            var fontGroup = CreateGroup(CommandId.None, "Font", "FN");
            fontGroup.Controls.Add(new ComboBoxConfig { CommandId = CommandId.FontFamily });
            fontGroup.Controls.Add(new ComboBoxConfig { CommandId = CommandId.FontSize });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Bold });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Italic });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Underline });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Strikethrough });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Subscript });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Superscript });
            fontGroup.Controls.Add(new SeparatorConfig());
            fontGroup.Controls.Add(new ColorPickerConfig { CommandId = CommandId.FontColor, ColorTemplate = RibbonColorTemplate.StandardColors });
            fontGroup.Controls.Add(new SeparatorConfig());
            fontGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ClearFormatting });
            tab.Groups.Add(fontGroup);

            // Insert Group
            var insertGroup = CreateGroup(CommandId.InsertGroup, "Insert", "I");
            insertGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertPictureFromFile, ButtonType = RibbonButtonType.SplitButton });
            insertGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertLink });
            tab.Groups.Add(insertGroup);

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
