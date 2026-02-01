// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Ribbon.Managed.Configuration
{
    /// <summary>
    /// Provides the default ribbon configuration matching the original Ribbon.xml structure.
    /// This configuration is separated from implementation to allow easy customization.
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

            // Help Button
            ConfigureHelpButton(config);

            // Main Tabs
            config.Tabs.Add(CreateHomeTab());
            config.Tabs.Add(CreateInsertTab());
            config.Tabs.Add(CreateBlogProviderTab());
            config.Tabs.Add(CreatePreviewTab());
            config.Tabs.Add(CreateDebugTab());

            // Contextual Tab Groups
            config.ContextualTabGroups.Add(CreateImageToolsGroup());
            config.ContextualTabGroups.Add(CreateVideoToolsGroup());
            config.ContextualTabGroups.Add(CreateTableToolsGroup());
            config.ContextualTabGroups.Add(CreateMapToolsGroup());
            config.ContextualTabGroups.Add(CreateTagToolsGroup());

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
            majorItems.Items.Add(new MenuItemConfig { CommandId = CommandId.DeleteDraft });
            majorItems.Items.Add(new MenuItemConfig { CommandId = CommandId.PostAndPublish });
            majorItems.Items.Add(new MenuItemConfig { CommandId = CommandId.PostAsDraft });
            config.ApplicationMenu.MenuGroups.Add(majorItems);

            var standardItems = new MenuGroupConfig { Class = "StandardItems" };
            standardItems.Items.Add(new MenuItemConfig { CommandId = CommandId.PrintPreview });
            standardItems.Items.Add(new MenuItemConfig { CommandId = CommandId.Print });
            standardItems.Items.Add(new MenuItemConfig { IsSeparator = true });
            standardItems.Items.Add(new MenuItemConfig { CommandId = CommandId.Options });
            standardItems.Items.Add(new MenuItemConfig { CommandId = CommandId.About });
            standardItems.Items.Add(new MenuItemConfig { CommandId = CommandId.Close });
            config.ApplicationMenu.MenuGroups.Add(standardItems);
        }

        private static void ConfigureQuickAccessToolbar(RibbonConfiguration config)
        {
            config.QuickAccessToolbar.CommandId = CommandId.QAT;
            config.QuickAccessToolbar.DefaultCommands.Add(CommandId.SavePost);
            config.QuickAccessToolbar.DefaultCommands.Add(CommandId.Undo);
            config.QuickAccessToolbar.DefaultCommands.Add(CommandId.Redo);
        }

        private static void ConfigureHelpButton(RibbonConfiguration config)
        {
            config.HelpButton = new HelpButtonConfig
            {
                CommandId = CommandId.Help,
                TooltipTitle = "Online help (F1)",
                TooltipDescription = "Get help on using Open Live Writer.",
                Keytip = "H"
            };
        }

        #region Home Tab

        private static TabConfig CreateHomeTab()
        {
            var tab = new TabConfig
            {
                CommandId = CommandId.HomeTab,
                Label = "Home",
                Keytip = "H",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Clipboard Group - SizeDefinition="OneBigControlAndTwoSmallControls"
            // Paste (large with "Clipboard" label to match native) + Cut/Copy (small stacked)
            var clipboardGroup = CreateGroup(CommandId.ClipboardGroup, "Clipboard", "X");
            clipboardGroup.SizeDefinition = "OneLargeAndTwoSmall";
            var pasteButton = new ButtonConfig 
            { 
                CommandId = CommandId.Paste, 
                ButtonType = RibbonButtonType.SplitButton,
                PreferredSize = RibbonGroupSize.Large,
                Label = "Clipboard"  // Display "Clipboard" instead of "Paste" to match native ribbon
            };
            pasteButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.Paste });
            pasteButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.PasteSpecial });
            clipboardGroup.Controls.Add(pasteButton);
            clipboardGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.Cut, 
                PreferredSize = RibbonGroupSize.Small 
            });
            clipboardGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.CopyCommand, 
                PreferredSize = RibbonGroupSize.Small 
            });
            tab.Groups.Add(clipboardGroup);

            // Publish Group - SizeDefinition="OneBigButtonOneComboboxAndOneSmallButton"
            // Layout: [Large Publish Button] | [Blog Selector Dropdown (stacked above)]
            //                                | [Post Draft Button (stacked below)]
            var publishGroup = CreateGroup(CommandId.PublishGroup, "Publish", "PB");
            publishGroup.SizeDefinition = "OneLargeComboSmall";
            publishGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.PostAndPublish, 
                ButtonType = RibbonButtonType.Button,
                PreferredSize = RibbonGroupSize.Large
            });
            var selectBlogGallery = new GalleryConfig 
            { 
                CommandId = CommandId.SelectBlog, 
                GalleryType = RibbonGalleryType.CompactDropDown,
                TextPosition = RibbonTextPosition.Right,
                ItemHeight = 16,   // Match native ribbon: ItemHeight="16"
                ItemWidth = 16,    // Match native ribbon: ItemWidth="16"
                MaxColumns = 1,    // Single column for blog list
                MaxRows = 10       // Show up to 10 blogs
            };
            var selectBlogMenuGroup = new MenuGroupConfig { Class = "StandardItems" };
            selectBlogMenuGroup.Items.Add(new MenuItemConfig { CommandId = CommandId.AddWeblog });
            selectBlogMenuGroup.Items.Add(new MenuItemConfig { CommandId = CommandId.Accounts });
            selectBlogGallery.MenuGroups.Add(selectBlogMenuGroup);
            publishGroup.Controls.Add(selectBlogGallery);
            publishGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.PostAsDraft, 
                PreferredSize = RibbonGroupSize.Medium 
            });
            tab.Groups.Add(publishGroup);

            // Font Group - SizeDefinition="CustomFontControl"
            var fontGroup = CreateGroup(CommandId.FontGroup, "Font", "FN");
            fontGroup.SizeDefinition = "FontGroup";
            fontGroup.Controls.Add(new ComboBoxConfig { CommandId = CommandId.FontFamily, PreferredWidth = 95, IsEditable = true, IsAutoCompleteEnabled = true });
            fontGroup.Controls.Add(new ComboBoxConfig { CommandId = CommandId.FontSize, PreferredWidth = 45, IsEditable = true, IsAutoCompleteEnabled = true });
            fontGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ClearFormatting, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Bold, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Italic, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Underline, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Strikethrough, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Subscript, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Superscript, PreferredSize = RibbonGroupSize.Small });
            fontGroup.Controls.Add(new ColorPickerConfig { CommandId = CommandId.FontBackgroundColor, ColorTemplate = RibbonColorTemplate.HighlightColors });
            fontGroup.Controls.Add(new ColorPickerConfig { CommandId = CommandId.FontColor, ColorTemplate = RibbonColorTemplate.StandardColors });
            tab.Groups.Add(fontGroup);

            // Paragraph Group - SizeDefinition="SevenSmallButtons" (LTR mode)
            var paragraphGroup = CreateGroup(CommandId.ParagraphGroup, "Paragraph", "P");
            paragraphGroup.SizeDefinition = "SevenSmallButtons";
            paragraphGroup.VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR;
            paragraphGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Bullets, PreferredSize = RibbonGroupSize.Small });
            paragraphGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Numbers, PreferredSize = RibbonGroupSize.Small });
            paragraphGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Blockquote, PreferredSize = RibbonGroupSize.Small });
            paragraphGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.AlignLeft, PreferredSize = RibbonGroupSize.Small });
            paragraphGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.AlignCenter, PreferredSize = RibbonGroupSize.Small });
            paragraphGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.AlignRight, PreferredSize = RibbonGroupSize.Small });
            paragraphGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.Justify, PreferredSize = RibbonGroupSize.Small });
            tab.Groups.Add(paragraphGroup);

            // HTML Styles Group - SizeDefinition="OneInRibbonGallery"
            // Match native ribbon: MaxColumns="7", ItemHeight="36", ItemWidth="64", MaxRows="3"
            var htmlStylesGroup = CreateGroup(CommandId.SemanticHtmlGroup, "HTML styles", "HS");
            htmlStylesGroup.SizeDefinition = "OneInRibbonGallery";
            htmlStylesGroup.Controls.Add(new GalleryConfig 
            { 
                CommandId = CommandId.SemanticHtmlGallery, 
                GalleryType = RibbonGalleryType.InRibbon,
                TextPosition = RibbonTextPosition.Bottom,
                ItemHeight = 36,  // Match native ribbon: ItemHeight="36"
                ItemWidth = 64,   // Match native ribbon: ItemWidth="64"
                MaxColumns = 7,   // Match native ribbon: MaxColumns="7"
                MaxRows = 3,      // Match native ribbon: MaxRows="3"
                Columns = 2       // 2 columns in collapsed view
            });
            tab.Groups.Add(htmlStylesGroup);

            // Insert Group - SizeDefinition="ThreeButtons" (Large buttons)
            var insertGroup = CreateGroup(CommandId.InsertGroup, "Insert", "I");
            insertGroup.SizeDefinition = "ThreeLargeButtons";
            insertGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.InsertLink, 
                PreferredSize = RibbonGroupSize.Large 
            });
            var insertImageButton = new ButtonConfig 
            { 
                CommandId = CommandId.InsertImageSplit, 
                ButtonType = RibbonButtonType.DropDownButton,
                PreferredSize = RibbonGroupSize.Large 
            };
            insertImageButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.InsertPictureFromFile });
            insertImageButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.WebImage });
            insertGroup.Controls.Add(insertImageButton);
            var insertVideoButton = new ButtonConfig 
            { 
                CommandId = CommandId.InsertVideoSplit, 
                ButtonType = RibbonButtonType.DropDownButton,
                PreferredSize = RibbonGroupSize.Large 
            };
            insertVideoButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.InsertVideoFromWeb });
            insertVideoButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.InsertVideoFromFile });
            insertVideoButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.InsertVideoFromService });
            insertGroup.Controls.Add(insertVideoButton);
            tab.Groups.Add(insertGroup);

            // Spelling Group - Single large button
            var spellingGroup = CreateGroup(CommandId.TextEditingGroup, "Spelling", "S");
            spellingGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.CheckSpelling, PreferredSize = RibbonGroupSize.Large });
            tab.Groups.Add(spellingGroup);

            // Editing Group - 3 medium buttons stacked vertically (Word count, Find, Select all)
            var editingGroup = CreateGroup(CommandId.TextEditingGroup, "Editing", "E");
            editingGroup.SizeDefinition = "ThreeMediumButtons";
            editingGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.WordCount, PreferredSize = RibbonGroupSize.Medium });
            editingGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.FindButton, PreferredSize = RibbonGroupSize.Medium });
            editingGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.SelectAll, PreferredSize = RibbonGroupSize.Medium });
            tab.Groups.Add(editingGroup);

            return tab;
        }

        #endregion

        #region Insert Tab

        private static TabConfig CreateInsertTab()
        {
            var tab = new TabConfig
            {
                CommandId = CommandId.InsertTab,
                Label = "Insert",
                Keytip = "N",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Breaks Group - SizeDefinition="ThreeButtons"
            var breaksGroup = CreateGroup(CommandId.BreaksGroup, "Breaks", "B");
            breaksGroup.SizeDefinition = "ThreeMediumButtons";
            breaksGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertHorizontalLine, PreferredSize = RibbonGroupSize.Medium });
            breaksGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertClearBreak, PreferredSize = RibbonGroupSize.Medium });
            breaksGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertExtendedEntry, PreferredSize = RibbonGroupSize.Medium });
            tab.Groups.Add(breaksGroup);

            // Tables Group - SizeDefinition="OneButton"
            var tablesGroup = CreateGroup(CommandId.TablesGroup, "Tables", "T");
            tablesGroup.SizeDefinition = "OneLargeButton";
            tablesGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.InsertTable, 
                ButtonType = RibbonButtonType.DropDownButton,
                PreferredSize = RibbonGroupSize.Large 
            });
            tab.Groups.Add(tablesGroup);

            // Media Group - SizeDefinition="SixButtons"
            var mediaGroup = CreateGroup(CommandId.MediaGroup, "Media", "M");
            mediaGroup.SizeDefinition = "SixLargeButtons";
            mediaGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertLink, PreferredSize = RibbonGroupSize.Large });
            var mediaInsertImageButton = new ButtonConfig 
            { 
                CommandId = CommandId.InsertImageSplit, 
                ButtonType = RibbonButtonType.DropDownButton,
                PreferredSize = RibbonGroupSize.Large 
            };
            mediaInsertImageButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.InsertPictureFromFile });
            mediaInsertImageButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.WebImage });
            mediaGroup.Controls.Add(mediaInsertImageButton);
            var mediaInsertVideoButton = new ButtonConfig 
            { 
                CommandId = CommandId.InsertVideoSplit, 
                ButtonType = RibbonButtonType.DropDownButton,
                PreferredSize = RibbonGroupSize.Large 
            };
            mediaInsertVideoButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.InsertVideoFromWeb });
            mediaInsertVideoButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.InsertVideoFromFile });
            mediaInsertVideoButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.InsertVideoFromService });
            mediaGroup.Controls.Add(mediaInsertVideoButton);
            mediaGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertMap, PreferredSize = RibbonGroupSize.Large });
            mediaGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertTags, PreferredSize = RibbonGroupSize.Large });
            mediaGroup.Controls.Add(new GalleryConfig 
            { 
                CommandId = CommandId.InsertEmoticon, 
                GalleryType = RibbonGalleryType.DropDown,
                TextPosition = RibbonTextPosition.Hide,
                ItemHeight = 22,
                ItemWidth = 22,
                MaxColumns = 10,
                MaxRows = 5
            });
            tab.Groups.Add(mediaGroup);

            // Plugins Group (without plugins) - SizeDefinition="TwoButtons"
            var pluginsGroupNoPlugins = CreateGroup(CommandId.PluginsGroup, "Plug-ins", "P");
            pluginsGroupNoPlugins.SizeDefinition = "TwoLargeButtons";
            pluginsGroupNoPlugins.VisibleModes = RibbonApplicationMode.WithoutPlugins;
            pluginsGroupNoPlugins.Controls.Add(new ButtonConfig { CommandId = CommandId.AddPlugin, PreferredSize = RibbonGroupSize.Large });
            pluginsGroupNoPlugins.Controls.Add(new ButtonConfig { CommandId = CommandId.ManagePlugins, PreferredSize = RibbonGroupSize.Large });
            tab.Groups.Add(pluginsGroupNoPlugins);

            // Plugins Group (with plugins) - SizeDefinition="InRibbonGalleryAndButtons"
            var pluginsGroupWithPlugins = CreateGroup(CommandId.PluginsGroup, "Plug-ins", "P");
            pluginsGroupWithPlugins.SizeDefinition = "GalleryAndTwoButtons";
            pluginsGroupWithPlugins.VisibleModes = RibbonApplicationMode.WithPlugins;
            pluginsGroupWithPlugins.Controls.Add(new GalleryConfig 
            { 
                CommandId = CommandId.PluginsGallery, 
                GalleryType = RibbonGalleryType.InRibbon,
                TextPosition = RibbonTextPosition.Right,
                ItemHeight = 16,
                ItemWidth = 16,
                MaxColumns = 1,
                MaxRows = 3
            });
            pluginsGroupWithPlugins.Controls.Add(new ButtonConfig { CommandId = CommandId.AddPlugin, PreferredSize = RibbonGroupSize.Medium });
            pluginsGroupWithPlugins.Controls.Add(new ButtonConfig { CommandId = CommandId.ManagePlugins, PreferredSize = RibbonGroupSize.Medium });
            tab.Groups.Add(pluginsGroupWithPlugins);

            return tab;
        }

        #endregion

        #region Blog Provider Tab

        private static TabConfig CreateBlogProviderTab()
        {
            var tab = new TabConfig
            {
                CommandId = CommandId.BlogProviderTab,
                Label = "Blog Account",
                Keytip = "A",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Blog options Group - SizeDefinition="OneButton"
            var blogGroup = CreateGroup(CommandId.BlogProviderBlogGroup, "Blog options", "B");
            blogGroup.SizeDefinition = "OneLargeButton";
            blogGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.ConfigureWeblog, 
                PreferredSize = RibbonGroupSize.Large 
            });
            tab.Groups.Add(blogGroup);

            // Shortcuts Group - SizeDefinition="OneInRibbonGallery"
            // Use MinColumnsLarge=1 to ensure compact width for this list-style gallery
            var shortcutsGroup = CreateGroup(CommandId.BlogProviderShortcutsGroup, "Shortcuts", "S");
            shortcutsGroup.SizeDefinition = "OneInRibbonGallery";
            shortcutsGroup.Controls.Add(new GalleryConfig 
            { 
                CommandId = CommandId.BlogProviderButtonsGallery, 
                GalleryType = RibbonGalleryType.InRibbon,
                TextPosition = RibbonTextPosition.Right,
                ItemHeight = 16,
                ItemWidth = 16,
                Columns = 1,  // Single column for list-style layout
                MaxColumns = 1,
                MaxRows = 3,
                MinColumnsLarge = 1  // Enforce compact width
            });
            tab.Groups.Add(shortcutsGroup);

            // Theme Group - SizeDefinition="TwoControls"
            var themeGroup = CreateGroup(CommandId.BlogProviderThemeGroup, "Theme", "T");
            themeGroup.SizeDefinition = "TwoLargeButtons";
            themeGroup.Controls.Add(new ToggleButtonConfig 
            { 
                CommandId = CommandId.ViewUseStyles, 
                PreferredSize = RibbonGroupSize.Large 
            });
            themeGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.UpdateWeblogStyle, 
                PreferredSize = RibbonGroupSize.Large 
            });
            tab.Groups.Add(themeGroup);

            return tab;
        }

        #endregion

        #region Preview Tab

        private static TabConfig CreatePreviewTab()
        {
            var tab = new TabConfig
            {
                CommandId = CommandId.PreviewTab,
                Label = "Preview",
                Keytip = "P",
                VisibleModes = RibbonApplicationMode.Preview
            };

            // Publish Group (duplicate for preview mode)
            var publishGroup = CreateGroup(CommandId.PublishGroup, "Publish", "PB");
            publishGroup.SizeDefinition = "OneLargeComboSmall";
            publishGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.PostAndPublish, 
                PreferredSize = RibbonGroupSize.Large 
            });
            var previewSelectBlogGallery = new GalleryConfig 
            { 
                CommandId = CommandId.SelectBlog, 
                GalleryType = RibbonGalleryType.CompactDropDown,
                TextPosition = RibbonTextPosition.Right,
                ItemHeight = 16,   // Match native ribbon: ItemHeight="16"
                ItemWidth = 16,    // Match native ribbon: ItemWidth="16"
                MaxColumns = 1,    // Single column for blog list
                MaxRows = 10       // Show up to 10 blogs
            };
            var previewSelectBlogMenuGroup = new MenuGroupConfig { Class = "StandardItems" };
            previewSelectBlogMenuGroup.Items.Add(new MenuItemConfig { CommandId = CommandId.AddWeblog });
            previewSelectBlogMenuGroup.Items.Add(new MenuItemConfig { CommandId = CommandId.Accounts });
            previewSelectBlogGallery.MenuGroups.Add(previewSelectBlogMenuGroup);
            publishGroup.Controls.Add(previewSelectBlogGallery);
            publishGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.PostAsDraft, 
                PreferredSize = RibbonGroupSize.Medium 
            });
            tab.Groups.Add(publishGroup);

            // Browser Group
            var browserGroup = CreateGroup(CommandId.BrowserGroup, "Browser", "B");
            browserGroup.SizeDefinition = "OneLargeButton";
            browserGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.UpdateWeblogStyle, 
                PreferredSize = RibbonGroupSize.Large 
            });
            tab.Groups.Add(browserGroup);

            // Preview Group
            var previewGroup = CreateGroup(CommandId.PreviewGroup, "Preview", "P");
            previewGroup.SizeDefinition = "OneLargeButton";
            previewGroup.Controls.Add(new ButtonConfig 
            { 
                CommandId = CommandId.ClosePreview, 
                PreferredSize = RibbonGroupSize.Large 
            });
            tab.Groups.Add(previewGroup);

            return tab;
        }

        #endregion

        #region Debug Tab

        private static TabConfig CreateDebugTab()
        {
            var tab = new TabConfig
            {
                CommandId = CommandId.DebugTab,
                Label = "Debug",
                Keytip = "D",
                VisibleModes = RibbonApplicationMode.Debug
            };

            // General Debug Group - SizeDefinition="FiveButtons"
            var generalGroup = CreateGroup(CommandId.GeneralDebugGroup, "General", "G");
            generalGroup.SizeDefinition = "FiveButtons";
            generalGroup.VisibleModes = RibbonApplicationMode.Debug;
            generalGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.TerminateProcess, PreferredSize = RibbonGroupSize.Medium });
            generalGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.RaiseAssertion, PreferredSize = RibbonGroupSize.Medium });
            generalGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.DiagnosticsConsole, PreferredSize = RibbonGroupSize.Medium });
            generalGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.BlogClientOptions, PreferredSize = RibbonGroupSize.Medium });
            generalGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ViewSource, PreferredSize = RibbonGroupSize.Medium });
            tab.Groups.Add(generalGroup);

            // Dialog Debug Group - SizeDefinition="EightButtons"
            var dialogGroup = CreateGroup(CommandId.DialogDebugGroup, "Dialog", "DL");
            dialogGroup.SizeDefinition = "EightButtons";
            dialogGroup.VisibleModes = RibbonApplicationMode.Debug;
            dialogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ShowBetaExpiredDialogs, PreferredSize = RibbonGroupSize.Medium });
            dialogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ShowUpdateMessage, PreferredSize = RibbonGroupSize.Medium });
            dialogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ShowWebLayoutWarning, PreferredSize = RibbonGroupSize.Medium });
            dialogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ShowErrorDialog, PreferredSize = RibbonGroupSize.Medium });
            dialogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ShowDisplayMessageTestForm, PreferredSize = RibbonGroupSize.Medium });
            dialogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ShowSupportingFilesForm, PreferredSize = RibbonGroupSize.Medium });
            dialogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ShowAtomImageEndpointSelector, PreferredSize = RibbonGroupSize.Medium });
            dialogGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ShowGoogleCaptcha, PreferredSize = RibbonGroupSize.Medium });
            tab.Groups.Add(dialogGroup);

            // Text Debug Group - SizeDefinition="OneButton"
            var textGroup = CreateGroup(CommandId.TextDebugGroup, "Text", "T");
            textGroup.SizeDefinition = "OneButton";
            textGroup.VisibleModes = RibbonApplicationMode.Debug;
            textGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertLoremIpsum, PreferredSize = RibbonGroupSize.Large });
            tab.Groups.Add(textGroup);

            // Validate Debug Group - SizeDefinition="ThreeButtons"
            var validateGroup = CreateGroup(CommandId.ValidateDebugGroup, "Validate", "V");
            validateGroup.SizeDefinition = "ThreeButtons";
            validateGroup.VisibleModes = RibbonApplicationMode.Debug;
            validateGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ValidateHtml, PreferredSize = RibbonGroupSize.Medium });
            validateGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ValidateXhtml, PreferredSize = RibbonGroupSize.Medium });
            validateGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ValidateLocalizedResources, PreferredSize = RibbonGroupSize.Medium });
            tab.Groups.Add(validateGroup);

            return tab;
        }

        #endregion

        #region Contextual Tab Groups

        private static ContextualTabGroupConfig CreateImageToolsGroup()
        {
            var group = new ContextualTabGroupConfig
            {
                CommandId = CommandId.ImageContextTabGroup,
                GroupType = RibbonContextualTabGroup.ImageTools,
                Label = "Picture Tools"
            };

            var formatTab = new TabConfig
            {
                CommandId = CommandId.FormatImageTab,
                Label = "Format",
                Keytip = "JP",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Size Group
            var sizeGroup = CreateGroup(CommandId.FormatImageSizeGroup, "Size", "S");
            sizeGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ImageCrop, PreferredSize = RibbonGroupSize.Large });
            sizeGroup.Controls.Add(new SpinnerConfig { CommandId = CommandId.FormatImageAdjustWidth });
            sizeGroup.Controls.Add(new SpinnerConfig { CommandId = CommandId.FormatImageAdjustHeight });
            var customSizeButton = new ButtonConfig 
            { 
                CommandId = CommandId.CustomSizeGallery, 
                ButtonType = RibbonButtonType.DropDownButton,
                PreferredSize = RibbonGroupSize.Medium
            };
            customSizeButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.CustomSizeSmall });
            customSizeButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.CustomSizeMedium });
            customSizeButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.CustomSizeLarge });
            customSizeButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.CustomSizeOriginal });
            customSizeButton.MenuItems.Add(new MenuItemConfig { IsSeparator = true });
            customSizeButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.SetCustomSizeDefaults });
            sizeGroup.Controls.Add(customSizeButton);
            sizeGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.FormatImageLockAspectRatio, PreferredSize = RibbonGroupSize.Small });
            formatTab.Groups.Add(sizeGroup);

            // Rotate Group
            var rotateGroup = CreateGroup(CommandId.FormatImageRotateGroup, "Rotate", "R");
            rotateGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ImageRotateCW, PreferredSize = RibbonGroupSize.Medium });
            rotateGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ImageRotateCCW, PreferredSize = RibbonGroupSize.Medium });
            rotateGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ImageTilt, PreferredSize = RibbonGroupSize.Medium });
            formatTab.Groups.Add(rotateGroup);

            // Styles Group
            var stylesGroup = CreateGroup(CommandId.FormatImageStyleGroup, "Picture styles", "ST");
            stylesGroup.Controls.Add(new GalleryConfig 
            { 
                CommandId = CommandId.ImageBorderGallery, 
                GalleryType = RibbonGalleryType.InRibbon,
                TextPosition = RibbonTextPosition.Hide,
                ItemWidth = 64,
                ItemHeight = 48,
                MaxColumns = 3,
                MaxRows = 3
            });
            var effectsButton = new ButtonConfig 
            { 
                CommandId = CommandId.ImageEffectsGallery, 
                ButtonType = RibbonButtonType.DropDownButton,
                PreferredSize = RibbonGroupSize.Medium
            };
            effectsButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.ImageEffectsRecolorGallery });
            effectsButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.ImageEffectsSharpenGallery });
            effectsButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.ImageEffectsBlurGallery });
            effectsButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.ImageEffectsEmbossGallery });
            stylesGroup.Controls.Add(effectsButton);
            stylesGroup.Controls.Add(new SpinnerConfig { CommandId = CommandId.ImageContrast });
            stylesGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.Watermark, PreferredSize = RibbonGroupSize.Medium });
            formatTab.Groups.Add(stylesGroup);

            // Properties Group
            var imagePropertiesGroup = CreateGroup(CommandId.FormatImagePropertiesGroup, "Properties", "P");
            var selectLinkButton = new ButtonConfig 
            { 
                CommandId = CommandId.FormatImageSelectLink, 
                ButtonType = RibbonButtonType.DropDownButton,
                PreferredSize = RibbonGroupSize.Medium
            };
            selectLinkButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.ImageLinkToSource });
            selectLinkButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.ImageLinkToUrl });
            selectLinkButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.ImageLinkToNone });
            imagePropertiesGroup.Controls.Add(selectLinkButton);
            imagePropertiesGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.FormatImageLinkOptions, PreferredSize = RibbonGroupSize.Medium });
            imagePropertiesGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.FormatImageAltText, PreferredSize = RibbonGroupSize.Medium });
            formatTab.Groups.Add(imagePropertiesGroup);

            // Settings Group
            var settingsGroup = CreateGroup(CommandId.FormatImageSettingsGroup, "Settings", "SE");
            settingsGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ImageSaveDefaults, PreferredSize = RibbonGroupSize.Medium });
            settingsGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.FormatImageRevertSettings, PreferredSize = RibbonGroupSize.Medium });
            formatTab.Groups.Add(settingsGroup);

            group.Tabs.Add(formatTab);
            return group;
        }

        private static ContextualTabGroupConfig CreateVideoToolsGroup()
        {
            var group = new ContextualTabGroupConfig
            {
                CommandId = CommandId.VideoContextTabGroup,
                GroupType = RibbonContextualTabGroup.VideoTools,
                Label = "Video Tools"
            };

            var formatTab = new TabConfig
            {
                CommandId = CommandId.FormatVideoTab,
                Label = "Format",
                Keytip = "V",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Video Format Group
            var formatGroup = CreateGroup(CommandId.FormatVideoGroup, "Video", "V");
            formatGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.VideoWebPreview, PreferredSize = RibbonGroupSize.Large });
            formatTab.Groups.Add(formatGroup);

            // Aspect Ratio Group
            var aspectGroup = CreateGroup(CommandId.VideoAspectRatioGroup, "Aspect ratio", "A");
            aspectGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.VideoWidescreenAspectRatio, PreferredSize = RibbonGroupSize.Medium });
            aspectGroup.Controls.Add(new ToggleButtonConfig { CommandId = CommandId.VideoStandardAspectRatio, PreferredSize = RibbonGroupSize.Medium });
            formatTab.Groups.Add(aspectGroup);

            group.Tabs.Add(formatTab);
            return group;
        }

        private static ContextualTabGroupConfig CreateTableToolsGroup()
        {
            var group = new ContextualTabGroupConfig
            {
                CommandId = CommandId.TableContextTabGroup,
                GroupType = RibbonContextualTabGroup.TableTools,
                Label = "Table Tools"
            };

            var layoutTab = new TabConfig
            {
                CommandId = CommandId.FormatTableTab,
                Label = "Layout",
                Keytip = "T",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Properties Group
            var propertiesGroup = CreateGroup(CommandId.FormatTablePropertiesGroup, "Properties", "P");
            var tablePropertiesButton = new ButtonConfig 
            { 
                CommandId = CommandId.FormatTablePropertiesSplit, 
                ButtonType = RibbonButtonType.DropDownButton,
                PreferredSize = RibbonGroupSize.Large 
            };
            tablePropertiesButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.TableProperties });
            tablePropertiesButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.RowProperties });
            tablePropertiesButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.ColumnProperties });
            tablePropertiesButton.MenuItems.Add(new MenuItemConfig { CommandId = CommandId.CellProperties });
            propertiesGroup.Controls.Add(tablePropertiesButton);
            layoutTab.Groups.Add(propertiesGroup);

            // Editing Group (Delete)
            var editingGroup = CreateGroup(CommandId.FormatTableEditingGroup, "Delete", "D");
            editingGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.DeleteRow, PreferredSize = RibbonGroupSize.Medium });
            editingGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.DeleteColumn, PreferredSize = RibbonGroupSize.Medium });
            editingGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.DeleteTable, PreferredSize = RibbonGroupSize.Medium });
            editingGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ClearCell, PreferredSize = RibbonGroupSize.Medium });
            layoutTab.Groups.Add(editingGroup);

            // Insert Group
            var insertGroup = CreateGroup(CommandId.FormatTableInsertGroup, "Insert", "I");
            insertGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertRowAbove, PreferredSize = RibbonGroupSize.Medium });
            insertGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertRowBelow, PreferredSize = RibbonGroupSize.Medium });
            insertGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertColumnLeft, PreferredSize = RibbonGroupSize.Medium });
            insertGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.InsertColumnRight, PreferredSize = RibbonGroupSize.Medium });
            layoutTab.Groups.Add(insertGroup);

            // Move Group
            var moveGroup = CreateGroup(CommandId.FormatTableMoveGroup, "Move", "M");
            moveGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.MoveRowUp, PreferredSize = RibbonGroupSize.Medium });
            moveGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.MoveRowDown, PreferredSize = RibbonGroupSize.Medium });
            moveGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.MoveColumnLeft, PreferredSize = RibbonGroupSize.Medium });
            moveGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.MoveColumnRight, PreferredSize = RibbonGroupSize.Medium });
            layoutTab.Groups.Add(moveGroup);

            group.Tabs.Add(layoutTab);
            return group;
        }

        private static ContextualTabGroupConfig CreateTagToolsGroup()
        {
            var group = new ContextualTabGroupConfig
            {
                CommandId = CommandId.TagContextTabGroup,
                GroupType = RibbonContextualTabGroup.TagTools,
                Label = "Tag Tools"
            };

            var formatTab = new TabConfig
            {
                CommandId = CommandId.FormatTagTab,
                Label = "Format",
                Keytip = "T",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Properties Group
            var propertiesGroup = CreateGroup(CommandId.FormatTagPropertiesGroup, "Properties", "P");
            propertiesGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.EditTags, PreferredSize = RibbonGroupSize.Large });
            formatTab.Groups.Add(propertiesGroup);

            // Providers Group
            var providersGroup = CreateGroup(CommandId.FormatTagProvidersGroup, "Providers", "V");
            providersGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.AddTagProvider, PreferredSize = RibbonGroupSize.Large });
            providersGroup.Controls.Add(new ButtonConfig { CommandId = CommandId.ManageTagProviders, PreferredSize = RibbonGroupSize.Large });
            formatTab.Groups.Add(providersGroup);

            group.Tabs.Add(formatTab);
            return group;
        }

        private static ContextualTabGroupConfig CreateMapToolsGroup()
        {
            var group = new ContextualTabGroupConfig
            {
                CommandId = CommandId.MapContextTabGroup,
                GroupType = RibbonContextualTabGroup.MapTools,
                Label = "Map Tools"
            };

            var formatTab = new TabConfig
            {
                CommandId = CommandId.FormatMapTab,
                Label = "Format",
                Keytip = "M",
                VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.RTL
            };

            // Alignment Group
            var alignmentGroup = CreateGroup(CommandId.AlignmentGroup, "Alignment", "A");
            alignmentGroup.Controls.Add(new GalleryConfig 
            { 
                CommandId = CommandId.AlignmentGallery, 
                GalleryType = RibbonGalleryType.InRibbon 
            });
            formatTab.Groups.Add(alignmentGroup);

            // Margins Group
            var marginsGroup = CreateGroup(CommandId.MarginsGroup, "Margins", "M");
            marginsGroup.Controls.Add(new SpinnerConfig { CommandId = CommandId.AdjustTopMargin });
            marginsGroup.Controls.Add(new SpinnerConfig { CommandId = CommandId.AdjustBottomMargin });
            marginsGroup.Controls.Add(new SpinnerConfig { CommandId = CommandId.AdjustLeftMargin });
            marginsGroup.Controls.Add(new SpinnerConfig { CommandId = CommandId.AdjustRightMargin });
            formatTab.Groups.Add(marginsGroup);

            group.Tabs.Add(formatTab);
            return group;
        }

        #endregion

        #region Helpers

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

        #endregion
    }
}
