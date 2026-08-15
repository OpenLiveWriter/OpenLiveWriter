// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.Tests.Ribbon
{
    [TestFixture]
    public class RibbonConfigurationTests
    {
        private RibbonConfiguration _config;

        [SetUp]
        public void SetUp()
        {
            _config = DefaultRibbonConfiguration.Create();
        }

        #region Tab Tests

        [Test]
        public void Configuration_HasCorrectNumberOfTabs()
        {
            // Home, Insert, Blog Account, Debug, plus contextual tabs
            Assert.That(_config.Tabs.Count, Is.GreaterThanOrEqualTo(4));
        }

        [Test]
        public void Configuration_HomeTabIsFirst()
        {
            var homeTab = _config.Tabs[0];
            Assert.That(homeTab.CommandId, Is.EqualTo(CommandId.HomeTab));
            Assert.That(homeTab.Label, Is.EqualTo("Home"));
        }

        [Test]
        public void Configuration_InsertTabIsSecond()
        {
            var insertTab = _config.Tabs[1];
            Assert.That(insertTab.CommandId, Is.EqualTo(CommandId.InsertTab));
            Assert.That(insertTab.Label, Is.EqualTo("Insert"));
        }

        [Test]
        public void Configuration_BlogProviderTabIsThird()
        {
            var blogTab = _config.Tabs[2];
            Assert.That(blogTab.CommandId, Is.EqualTo(CommandId.BlogProviderTab));
            Assert.That(blogTab.Label, Is.EqualTo("Blog Account"));
        }

        [Test]
        public void Configuration_PreviewTabIsFourth()
        {
            var previewTab = _config.Tabs[3];
            Assert.That(previewTab.CommandId, Is.EqualTo(CommandId.PreviewTab));
            Assert.That(previewTab.Label, Is.EqualTo("Preview"));
        }

        #endregion

        #region Home Tab Group Tests

        [Test]
        public void HomeTab_HasCorrectNumberOfGroups()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            // Clipboard, Publish, Font, Paragraph, HTML styles, Insert, Editing
            Assert.That(homeTab.Groups.Count, Is.EqualTo(7));
        }

        [Test]
        public void HomeTab_ClipboardGroup_HasCorrectControls()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var clipboardGroup = homeTab.Groups.First(g => g.CommandId == CommandId.ClipboardGroup);

            Assert.That(clipboardGroup.Controls.Count, Is.EqualTo(3));
            
            // Paste (large), Cut (small), Copy (small)
            var pasteBtn = clipboardGroup.Controls[0] as ButtonConfig;
            Assert.That(pasteBtn.CommandId, Is.EqualTo(CommandId.Paste));
            Assert.That(pasteBtn.PreferredSize, Is.EqualTo(RibbonGroupSize.Large));

            var cutBtn = clipboardGroup.Controls[1] as ButtonConfig;
            Assert.That(cutBtn.CommandId, Is.EqualTo(CommandId.Cut));
            Assert.That(cutBtn.PreferredSize, Is.EqualTo(RibbonGroupSize.Small));

            var copyBtn = clipboardGroup.Controls[2] as ButtonConfig;
            Assert.That(copyBtn.CommandId, Is.EqualTo(CommandId.CopyCommand));
            Assert.That(copyBtn.PreferredSize, Is.EqualTo(RibbonGroupSize.Small));
        }

        [Test]
        public void HomeTab_PublishGroup_HasCorrectControls()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var publishGroup = homeTab.Groups.First(g => g.CommandId == CommandId.PublishGroup);

            Assert.That(publishGroup.Controls.Count, Is.EqualTo(3));
            
            // PostAndPublish (large), SelectBlog (gallery), PostAsDraft (medium)
            var publishBtn = publishGroup.Controls[0] as ButtonConfig;
            Assert.That(publishBtn.CommandId, Is.EqualTo(CommandId.PostAndPublish));
            Assert.That(publishBtn.PreferredSize, Is.EqualTo(RibbonGroupSize.Large));

            var selectBlogGallery = publishGroup.Controls[1] as GalleryConfig;
            Assert.That(selectBlogGallery.CommandId, Is.EqualTo(CommandId.SelectBlog));

            var draftBtn = publishGroup.Controls[2] as ButtonConfig;
            Assert.That(draftBtn.CommandId, Is.EqualTo(CommandId.PostAsDraft));
            Assert.That(draftBtn.PreferredSize, Is.EqualTo(RibbonGroupSize.Medium));
        }

        [Test]
        public void HomeTab_FontGroup_HasCorrectControls()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var fontGroup = homeTab.Groups.First(g => g.CommandId == CommandId.FontGroup);

            // FontFamily, FontSize, ClearFormatting, Bold, Italic, Underline, Strikethrough, 
            // Subscript, Superscript, FontBackgroundColor, FontColor
            Assert.That(fontGroup.Controls.Count, Is.EqualTo(11));

            var fontFamily = fontGroup.Controls[0] as ComboBoxConfig;
            Assert.That(fontFamily.CommandId, Is.EqualTo(CommandId.FontFamily));

            var fontSize = fontGroup.Controls[1] as ComboBoxConfig;
            Assert.That(fontSize.CommandId, Is.EqualTo(CommandId.FontSize));

            var clearFormatting = fontGroup.Controls[2] as ButtonConfig;
            Assert.That(clearFormatting.CommandId, Is.EqualTo(CommandId.ClearFormatting));
        }

        [Test]
        public void HomeTab_ParagraphGroup_HasCorrectControls()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var paragraphGroup = homeTab.Groups.First(g => g.CommandId == CommandId.ParagraphGroup);

            // Bullets, Numbers, Blockquote, AlignLeft, AlignCenter, AlignRight, Justify
            Assert.That(paragraphGroup.Controls.Count, Is.EqualTo(7));

            var bullets = paragraphGroup.Controls[0] as ToggleButtonConfig;
            Assert.That(bullets.CommandId, Is.EqualTo(CommandId.Bullets));
            Assert.That(bullets.PreferredSize, Is.EqualTo(RibbonGroupSize.Small));

            var alignLeft = paragraphGroup.Controls[3] as ToggleButtonConfig;
            Assert.That(alignLeft.CommandId, Is.EqualTo(CommandId.AlignLeft));
        }

        [Test]
        public void HomeTab_HtmlStylesGroup_HasGallery()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var htmlStylesGroup = homeTab.Groups.First(g => g.CommandId == CommandId.SemanticHtmlGroup);

            Assert.That(htmlStylesGroup.Controls.Count, Is.EqualTo(1));

            var gallery = htmlStylesGroup.Controls[0] as GalleryConfig;
            Assert.That(gallery.CommandId, Is.EqualTo(CommandId.SemanticHtmlGallery));
            Assert.That(gallery.GalleryType, Is.EqualTo(RibbonGalleryType.InRibbon));
        }

        [Test]
        public void HomeTab_InsertGroup_HasLargeButtons()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var insertGroup = homeTab.Groups.First(g => g.CommandId == CommandId.InsertGroup);

            // InsertLink, InsertImageSplit, InsertVideoSplit
            Assert.That(insertGroup.Controls.Count, Is.EqualTo(3));

            foreach (var control in insertGroup.Controls)
            {
                var button = control as ButtonConfig;
                Assert.That(button, Is.Not.Null);
                Assert.That(button.PreferredSize, Is.EqualTo(RibbonGroupSize.Large));
            }
        }

        [Test]
        public void HomeTab_EditingGroup_HasFourButtons()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var editingGroup = homeTab.Groups.First(g => g.CommandId == CommandId.TextEditingGroup);

            // CheckSpelling, WordCount, FindButton, SelectAll
            Assert.That(editingGroup.Controls.Count, Is.EqualTo(4));

            var spellCheck = editingGroup.Controls[0] as ButtonConfig;
            Assert.That(spellCheck.CommandId, Is.EqualTo(CommandId.CheckSpelling));

            var wordCount = editingGroup.Controls[1] as ButtonConfig;
            Assert.That(wordCount.CommandId, Is.EqualTo(CommandId.WordCount));

            var find = editingGroup.Controls[2] as ButtonConfig;
            Assert.That(find.CommandId, Is.EqualTo(CommandId.FindButton));

            var selectAll = editingGroup.Controls[3] as ButtonConfig;
            Assert.That(selectAll.CommandId, Is.EqualTo(CommandId.SelectAll));
        }

        #endregion

        #region Insert Tab Group Tests

        [Test]
        public void InsertTab_HasCorrectGroups()
        {
            var insertTab = _config.Tabs.First(t => t.CommandId == CommandId.InsertTab);
            
            // Breaks, Tables, Media, Plugins (2 versions for with/without plugins)
            Assert.That(insertTab.Groups.Count, Is.GreaterThanOrEqualTo(4));
        }

        [Test]
        public void InsertTab_BreaksGroup_HasThreeControls()
        {
            var insertTab = _config.Tabs.First(t => t.CommandId == CommandId.InsertTab);
            var breaksGroup = insertTab.Groups.First(g => g.CommandId == CommandId.BreaksGroup);

            // HorizontalLine, ClearBreak, ExtendedEntry (split post)
            Assert.That(breaksGroup.Controls.Count, Is.EqualTo(3));

            var horizontalLine = breaksGroup.Controls[0] as ButtonConfig;
            Assert.That(horizontalLine.CommandId, Is.EqualTo(CommandId.InsertHorizontalLine));

            var clearBreak = breaksGroup.Controls[1] as ButtonConfig;
            Assert.That(clearBreak.CommandId, Is.EqualTo(CommandId.InsertClearBreak));

            var splitPost = breaksGroup.Controls[2] as ButtonConfig;
            Assert.That(splitPost.CommandId, Is.EqualTo(CommandId.InsertExtendedEntry));
        }

        [Test]
        public void InsertTab_MediaGroup_HasFiveControls()
        {
            var insertTab = _config.Tabs.First(t => t.CommandId == CommandId.InsertTab);
            var mediaGroup = insertTab.Groups.First(g => g.CommandId == CommandId.MediaGroup);

            // InsertLink, InsertImageSplit, InsertVideoSplit, InsertTags, InsertEmoticon
            // (InsertMap was removed along with its deprecation modal)
            Assert.That(mediaGroup.Controls.Count, Is.EqualTo(5));

            var map = mediaGroup.Controls.FirstOrDefault(c => c.CommandId == CommandId.InsertMap);
            Assert.That(map, Is.Null, "InsertMap should no longer appear in the ribbon");

            var tags = mediaGroup.Controls.FirstOrDefault(c => c.CommandId == CommandId.InsertTags);
            Assert.That(tags, Is.Not.Null);
        }

        #endregion

        #region Blog Account Tab Tests

        [Test]
        public void BlogAccountTab_HasCorrectGroups()
        {
            var blogTab = _config.Tabs.First(t => t.CommandId == CommandId.BlogProviderTab);
            
            // Blog options, Shortcuts, Theme
            Assert.That(blogTab.Groups.Count, Is.EqualTo(3));
        }

        [Test]
        public void BlogAccountTab_BlogOptionsGroup_HasConfigureButton()
        {
            var blogTab = _config.Tabs.First(t => t.CommandId == CommandId.BlogProviderTab);
            var blogGroup = blogTab.Groups.First(g => g.CommandId == CommandId.BlogProviderBlogGroup);

            var configureBtn = blogGroup.Controls[0] as ButtonConfig;
            Assert.That(configureBtn.CommandId, Is.EqualTo(CommandId.ConfigureWeblog));
        }

        [Test]
        public void BlogAccountTab_ShortcutsGroup_HasGallery()
        {
            var blogTab = _config.Tabs.First(t => t.CommandId == CommandId.BlogProviderTab);
            var shortcutsGroup = blogTab.Groups.First(g => g.CommandId == CommandId.BlogProviderShortcutsGroup);

            var gallery = shortcutsGroup.Controls[0] as GalleryConfig;
            Assert.That(gallery.CommandId, Is.EqualTo(CommandId.BlogProviderButtonsGallery));
            Assert.That(gallery.GalleryType, Is.EqualTo(RibbonGalleryType.InRibbon));
        }

        [Test]
        public void BlogAccountTab_ThemeGroup_HasToggleAndButton()
        {
            var blogTab = _config.Tabs.First(t => t.CommandId == CommandId.BlogProviderTab);
            var themeGroup = blogTab.Groups.First(g => g.CommandId == CommandId.BlogProviderThemeGroup);

            Assert.That(themeGroup.Controls.Count, Is.EqualTo(2));

            var viewUseStyles = themeGroup.Controls[0] as ToggleButtonConfig;
            Assert.That(viewUseStyles.CommandId, Is.EqualTo(CommandId.ViewUseStyles));

            var updateStyle = themeGroup.Controls[1] as ButtonConfig;
            Assert.That(updateStyle.CommandId, Is.EqualTo(CommandId.UpdateWeblogStyle));
        }

        #endregion

        #region Application Menu Tests

        [Test]
        public void ApplicationMenu_HasCorrectCommandId()
        {
            Assert.That(_config.ApplicationMenu.CommandId, Is.EqualTo(CommandId.FileMenu));
        }

        [Test]
        public void ApplicationMenu_HasMajorAndStandardItems()
        {
            Assert.That(_config.ApplicationMenu.MenuGroups.Count, Is.EqualTo(2));

            var majorItems = _config.ApplicationMenu.MenuGroups[0];
            Assert.That(majorItems.Class, Is.EqualTo("MajorItems"));

            var standardItems = _config.ApplicationMenu.MenuGroups[1];
            Assert.That(standardItems.Class, Is.EqualTo("StandardItems"));
        }

        [Test]
        public void ApplicationMenu_MajorItems_ContainsNewOpenSavePublish()
        {
            var majorItems = _config.ApplicationMenu.MenuGroups[0];
            
            var newPost = majorItems.Items.FirstOrDefault(i => i.CommandId == CommandId.NewPost);
            Assert.That(newPost, Is.Not.Null);

            var openPost = majorItems.Items.FirstOrDefault(i => i.CommandId == CommandId.OpenPost);
            Assert.That(openPost, Is.Not.Null);

            var savePost = majorItems.Items.FirstOrDefault(i => i.CommandId == CommandId.SavePost);
            Assert.That(savePost, Is.Not.Null);

            var publish = majorItems.Items.FirstOrDefault(i => i.CommandId == CommandId.PostAndPublish);
            Assert.That(publish, Is.Not.Null);

            var draft = majorItems.Items.FirstOrDefault(i => i.CommandId == CommandId.PostAsDraft);
            Assert.That(draft, Is.Not.Null);
        }

        #endregion

        #region Quick Access Toolbar Tests

        [Test]
        public void QuickAccessToolbar_HasCorrectDefaultCommands()
        {
            var qat = _config.QuickAccessToolbar;
            
            Assert.That(qat.DefaultCommands.Count, Is.EqualTo(3));
            Assert.That(qat.DefaultCommands, Contains.Item(CommandId.SavePost));
            Assert.That(qat.DefaultCommands, Contains.Item(CommandId.Undo));
            Assert.That(qat.DefaultCommands, Contains.Item(CommandId.Redo));
        }

        #endregion

        #region SizeDefinition Tests

        [Test]
        public void ClipboardGroup_HasCorrectSizeDefinition()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var clipboardGroup = homeTab.Groups.First(g => g.CommandId == CommandId.ClipboardGroup);

            Assert.That(clipboardGroup.SizeDefinition, Is.EqualTo("OneLargeAndTwoSmall"));
        }

        [Test]
        public void FontGroup_HasCorrectSizeDefinition()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var fontGroup = homeTab.Groups.First(g => g.CommandId == CommandId.FontGroup);

            Assert.That(fontGroup.SizeDefinition, Is.EqualTo("FontGroup"));
        }

        [Test]
        public void InsertGroup_HasCorrectSizeDefinition()
        {
            var homeTab = _config.Tabs.First(t => t.CommandId == CommandId.HomeTab);
            var insertGroup = homeTab.Groups.First(g => g.CommandId == CommandId.InsertGroup);

            Assert.That(insertGroup.SizeDefinition, Is.EqualTo("ThreeLargeButtons"));
        }

        #endregion

        #region Contextual Tab Group Tests

        [Test]
        public void Configuration_HasContextualTabGroups()
        {
            Assert.That(_config.ContextualTabGroups.Count, Is.GreaterThanOrEqualTo(4));
        }

        [Test]
        public void Configuration_HasImageToolsGroup()
        {
            var imageTools = _config.ContextualTabGroups.FirstOrDefault(g => 
                g.GroupType == RibbonContextualTabGroup.ImageTools);
            
            Assert.That(imageTools, Is.Not.Null);
            Assert.That(imageTools.Label, Is.EqualTo("Picture Tools"));
        }

        [Test]
        public void Configuration_HasVideoToolsGroup()
        {
            var videoTools = _config.ContextualTabGroups.FirstOrDefault(g => 
                g.GroupType == RibbonContextualTabGroup.VideoTools);
            
            Assert.That(videoTools, Is.Not.Null);
            Assert.That(videoTools.Label, Is.EqualTo("Video Tools"));
        }

        #endregion
    }
}
