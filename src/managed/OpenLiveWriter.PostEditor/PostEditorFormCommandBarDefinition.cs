// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor
{
    public class PostEditorFormCommandBarDefinition : CommandBarDefinition
    {
        public PostEditorFormCommandBarDefinition()

        {
            // required for designer support
            InitializeComponent() ;

        }
        private CommandBarButtonEntry commandBarButtonEntryPostAndPublish;
        private CommandBarButtonEntry commandBarButtonEntrySavePost;
        private CommandBarButtonEntry commandBarButtonEntryNewPost;
        private CommandBarButtonEntry commandBarButtonEntryOpenPost;
        private CommandBarButtonEntry commandBarButtonEntryWeblogMenu;
        private CommandBarSeparatorEntry commandBarSeparatorEntry3;
        private CommandBarSeparatorEntry commandBarSeparatorEntry4;
        private OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry commandBarButtonEntryHtmlView;

        private IContainer components;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.commandBarButtonEntryPostAndPublish = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntrySavePost = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryNewPost = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryOpenPost = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryWeblogMenu = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarSeparatorEntry3 = new OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry(this.components);
            this.commandBarSeparatorEntry4 = new OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry(this.components);
            this.commandBarButtonEntryHtmlView = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            //
            // commandBarButtonEntryPostAndPublish
            //
            this.commandBarButtonEntryPostAndPublish.CommandIdentifier = "OpenLiveWriter.PostAndPublish";
            //
            // commandBarButtonEntrySavePost
            //
            this.commandBarButtonEntrySavePost.CommandIdentifier = "OpenLiveWriter.PostEditor.SavePost";
            //
            // commandBarButtonEntryNewPost
            //
            this.commandBarButtonEntryNewPost.CommandIdentifier = "OpenLiveWriter.PostEditor.NewPost";
            //
            // commandBarButtonEntryOpenPost
            //
            this.commandBarButtonEntryOpenPost.CommandIdentifier = "OpenLiveWriter.PostEditor.OpenPost";
            //
            // commandBarButtonEntryWeblogMenu
            //
            this.commandBarButtonEntryWeblogMenu.CommandIdentifier = "OpenLiveWriter.PostEditor.Commands.WeblogMenu";
            //
            // commandBarButtonEntryHtmlView
            //
            this.commandBarButtonEntryHtmlView.CommandIdentifier = "OpenLiveWriter.PostEditor.Commands.PostHtmlEditing.HtmlViewMenu";
            //
            // PostEditorFormCommandBarDefinition
            //
            this.LeftCommandBarEntries.AddRange(new OpenLiveWriter.ApplicationFramework.CommandBarEntry[] {
                                                                                                                  this.commandBarButtonEntryNewPost,
                                                                                                                  //this.commandBarSeparatorEntry1,
                                                                                                                  this.commandBarButtonEntryOpenPost,
                                                                                                                  //this.commandBarSeparatorEntry2,
                                                                                                                  this.commandBarButtonEntrySavePost,
                                                                                                                  this.commandBarSeparatorEntry3,
                                                                                                                  this.commandBarButtonEntryHtmlView,
                                                                                                                  this.commandBarSeparatorEntry4,
                                                                                                                  this.commandBarButtonEntryPostAndPublish});
            this.RightCommandBarEntries.AddRange(new OpenLiveWriter.ApplicationFramework.CommandBarEntry[] {
                                                                                                                   this.commandBarButtonEntryWeblogMenu});

        }

    }
}
