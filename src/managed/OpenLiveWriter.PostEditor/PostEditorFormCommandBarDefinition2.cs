// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor
{
    public class PostEditorFormCommandBarDefinition2 : CommandBarDefinition
    {
        public PostEditorFormCommandBarDefinition2()
        {
            // required for designer support
            InitializeComponent() ;

        }

        private CommandBarButtonEntry commandBarButtonEntryBold;
        private CommandBarButtonEntry commandBarButtonEntryItalic;
        private CommandBarButtonEntry commandBarButtonEntryUnderline;
        private CommandBarSeparatorEntry commandBarSeparatorEntry1;
        private CommandBarButtonEntry commandBarButtonEntryBullets;
        private CommandBarButtonEntry commandBarButtonEntryNumbers;
        private CommandBarSeparatorEntry commandBarSeparatorEntry3;
        private CommandBarButtonEntry commandBarButtonEntryInsertLink;
        private CommandBarButtonEntry commandBarButtonEntryNormalView ;
        private CommandBarButtonEntry commandBarButtonEntryCodeView ;
//		private CommandBarButtonEntry commandBarButtonEntryPostProperties ;
        private CommandBarSeparatorEntry commandBarSeparatorEntry2;
        private CommandBarSeparatorEntry commandBarSeparatorEntry4;
        private CommandBarSeparatorEntry commandBarSeparatorEntry5;
//		internal CommandBarControlEntry commandBarControlEntryCategories;
        internal CommandBarControlEntry commandBarControlEntryStyle;
//		private CommandBarButtonEntry commandBarButtonEntryInsertPicture;
        private OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry commandBarButtonEntryStrikethrough;
        private OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry commandBarButtonEntryBlockquote;
        private OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry commandBarButtonEntryRemoveLink;
        private CommandBarButtonEntry commandBarButtonEntrySpellCheck;
//		private CommandBarSeparatorEntry commandBarSeparatorEntry6;
        private OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry commandBarButtonEntryTableMenu;
        private OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry commandBarSeparatorEntry7;

        private IContainer components;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.commandBarButtonEntryBold = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryItalic = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryUnderline = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarSeparatorEntry1 = new OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry(this.components);
            this.commandBarButtonEntryBullets = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryNumbers = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryBlockquote = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarSeparatorEntry3 = new OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry(this.components);
//			this.commandBarButtonEntryFontColor = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryInsertLink = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryNormalView = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryCodeView = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
//			this.commandBarButtonEntryPostProperties = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarSeparatorEntry2 = new OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry(this.components);
            this.commandBarSeparatorEntry4 = new OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry(this.components);
            this.commandBarSeparatorEntry5 = new OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry(this.components);
//			this.commandBarControlEntryCategories = new OpenLiveWriter.ApplicationFramework.CommandBarControlEntry(this.components);
            this.commandBarControlEntryStyle = new OpenLiveWriter.ApplicationFramework.CommandBarControlEntry(this.components);
//			this.commandBarButtonEntryInsertPicture = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryStrikethrough = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntryRemoveLink = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarButtonEntrySpellCheck = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
//			this.commandBarSeparatorEntry6 = new OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry(this.components);
            this.commandBarButtonEntryTableMenu = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            this.commandBarSeparatorEntry7 = new OpenLiveWriter.ApplicationFramework.CommandBarSeparatorEntry(this.components);
            //
            // commandBarButtonEntryBold
            //
            this.commandBarButtonEntryBold.CommandIdentifier = "MindShare.ApplicationCore.Commands.Bold";
            //
            // commandBarButtonEntryItalic
            //
            this.commandBarButtonEntryItalic.CommandIdentifier = "MindShare.ApplicationCore.Commands.Italic";
            //
            // commandBarButtonEntryUnderline
            //
            this.commandBarButtonEntryUnderline.CommandIdentifier = "MindShare.ApplicationCore.Commands.Underline";
            //
            // commandBarButtonEntryBullets
            //
            this.commandBarButtonEntryBullets.CommandIdentifier = "MindShare.ApplicationCore.Commands.Bullets";
            //
            // commandBarButtonEntryNumbers
            //
            this.commandBarButtonEntryNumbers.CommandIdentifier = "MindShare.ApplicationCore.Commands.Numbers";
            //
            // commandBarButtonEntryBlockquote
            //
            this.commandBarButtonEntryBlockquote.CommandIdentifier = "MindShare.ApplicationCore.Commands.Blockquote";
            //
            // commandBarButtonEntryFontColor
            //
//			this.commandBarButtonEntryFontColor.CommandIdentifier = "MindShare.ApplicationCore.Commands.FontColor";
            //
            // commandBarButtonEntryInsertLink
            //
            this.commandBarButtonEntryInsertLink.CommandIdentifier = "OpenLiveWriter.ApplicationFramework.Commands.InsertLink";
            //
            // commandBarButtonEntryNormalView
            //
            this.commandBarButtonEntryNormalView.CommandIdentifier = "OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.ViewNormal";
            //
            // commandBarButtonEntryCodeView
            //
            this.commandBarButtonEntryCodeView.CommandIdentifier = "OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.ViewCode";
            //
            // commandBarButtonEntryPostProperties
            //
//			this.commandBarButtonEntryPostProperties.CommandIdentifier = "OpenLiveWriter.PostEditor.Commands.PostProperties";
            //
            // commandBarControlEntryCategories
            //
//			this.commandBarControlEntryCategories.Control = null;
            //
            // commandBarControlEntryStyle
            //
            this.commandBarControlEntryStyle.Control = null;
            //
            // commandBarButtonEntryInsertPicture
            //
//			this.commandBarButtonEntryInsertPicture.CommandIdentifier = "OpenLiveWriter.PostEditor.Commands.PostHtmlEditing.InsertPicture";
            //
            // commandBarButtonEntryStrikethrough
            //
            this.commandBarButtonEntryStrikethrough.CommandIdentifier = "MindShare.ApplicationCore.Commands.Strikethrough";
            //
            // commandBarButtonEntryRemoveLink
            //
            this.commandBarButtonEntryRemoveLink.CommandIdentifier = "OpenLiveWriter.PostEditor.Commands.PostHtmlEditing.CommandRemoveLink";
            //
            // commandBarButtonEntrySpellCheck
            //
            this.commandBarButtonEntrySpellCheck.CommandIdentifier = "MindShare.ApplicationCore.Commands.CheckSpelling";
            //
            // commandBarButtonEntryTableMenu
            //
            this.commandBarButtonEntryTableMenu.CommandIdentifier = "OpenLiveWriter.PostEditor.Tables.Commands.TableMenu";
            //
            // PostEditorFormCommandBarDefinition2
            //
            this.LeftCommandBarEntries.AddRange(new OpenLiveWriter.ApplicationFramework.CommandBarEntry[] {
                                                                                                                  this.commandBarControlEntryStyle,
                                                                                                                  this.commandBarButtonEntryBold,
                                                                                                                  this.commandBarButtonEntryItalic,
                                                                                                                  this.commandBarButtonEntryUnderline,
                                                                                                                  this.commandBarButtonEntryStrikethrough,
//																												  this.commandBarButtonEntryFontColor,
                                                                                                                  this.commandBarSeparatorEntry1,
                                                                                                                  this.commandBarButtonEntryNumbers,
                                                                                                                  this.commandBarButtonEntryBullets,
                                                                                                                  this.commandBarSeparatorEntry2,
                                                                                                                  this.commandBarButtonEntryBlockquote,
                                                                                                                  this.commandBarSeparatorEntry3,
                                                                                                                  this.commandBarButtonEntryInsertLink,
                                                                                                                  this.commandBarButtonEntryRemoveLink,
//																												  this.commandBarButtonEntryInsertPicture,
                                                                                                                  this.commandBarSeparatorEntry7,
                                                                                                                  this.commandBarButtonEntryTableMenu,
                                                                                                                  this.commandBarSeparatorEntry5,
                                                                                                                  this.commandBarButtonEntrySpellCheck,
//																												  this.commandBarSeparatorEntry6,
//																												  this.commandBarButtonEntryPostProperties,
                                                                                                              });
/*
            this.RightCommandBarEntries.AddRange(new OpenLiveWriter.ApplicationFramework.CommandBarEntry[] {
                                                                                                                   this.commandBarControlEntryCategories});
*/

        }


    }
}
