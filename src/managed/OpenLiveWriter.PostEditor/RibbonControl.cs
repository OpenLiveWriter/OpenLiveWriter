// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;

namespace OpenLiveWriter.PostEditor
{
    public class RibbonControl
    {
        private IHtmlEditorComponentContext componentContext;
        private IHtmlEditorCommandSource commandSource;

        public RibbonControl(IHtmlEditorComponentContext componentContext, IHtmlEditorCommandSource commandSource)
        {
            // Note that this code is *not* called within Mail.
            // Shared canvas commands/code need to go in ContentEditor.

            this.componentContext = componentContext;
            this.commandSource = commandSource;

            componentContext.CommandManager.BeginUpdate();

            componentContext.CommandManager.Add(CommandId.FileMenu, null);

            componentContext.CommandManager.Add(CommandId.HomeTab, null);

            componentContext.CommandManager.Add(new GroupCommand(CommandId.ClipboardGroup, componentContext.CommandManager.Get(CommandId.Paste)));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.PublishGroup, componentContext.CommandManager.Get(CommandId.PostAndPublish)));
            componentContext.CommandManager.Add(new Command(CommandId.ParagraphGroup)); // Has it's own icon
            componentContext.CommandManager.Add(new GroupCommand(CommandId.InsertGroup, componentContext.CommandManager.Get(CommandId.InsertPictureFromFile)));

            componentContext.CommandManager.Add(CommandId.InsertTab, null);
            componentContext.CommandManager.Add(new GroupCommand(CommandId.BreaksGroup, componentContext.CommandManager.Get(CommandId.InsertHorizontalLine)));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.TablesGroup, componentContext.CommandManager.Get(CommandId.InsertTable)));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.MediaGroup, componentContext.CommandManager.Get(CommandId.InsertPictureFromFile)));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.PluginsGroup, componentContext.CommandManager.Get(CommandId.PluginsGallery)));

            componentContext.CommandManager.Add(new Command(CommandId.BlogProviderTab));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.BlogProviderBlogGroup, componentContext.CommandManager.Get(CommandId.ConfigureWeblog)));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.BlogProviderThemeGroup, componentContext.CommandManager.Get(CommandId.UpdateWeblogStyle)));

            componentContext.CommandManager.Add(CommandId.PreviewTab, null);
            // Already added PublishGroup
            componentContext.CommandManager.Add(new GroupCommand(CommandId.BrowserGroup, componentContext.CommandManager.Get(CommandId.UpdateWeblogStyle)));
            // Already added TextEditingGroup
            componentContext.CommandManager.Add(new GroupCommand(CommandId.PreviewGroup, componentContext.CommandManager.Get(CommandId.ClosePreview)));

            componentContext.CommandManager.Add(CommandId.DebugTab, null);
            componentContext.CommandManager.Add(new Command(CommandId.GeneralDebugGroup));
            componentContext.CommandManager.Add(new Command(CommandId.DialogDebugGroup));
            componentContext.CommandManager.Add(new Command(CommandId.TextDebugGroup));
            componentContext.CommandManager.Add(new Command(CommandId.ValidateDebugGroup));

            componentContext.CommandManager.Add(new Command(CommandId.FormatMapGroup));
            componentContext.CommandManager.Add(new Command(CommandId.FormatMapPropertiesGroup));

            componentContext.CommandManager.EndUpdate();
        }

        public void ManageCommands()
        {
            componentContext.CommandManager.SetEnabled(CommandId.SemanticHtmlGallery, commandSource.CanApplyFormatting(CommandId.SemanticHtmlGallery));
        }
    }
}
