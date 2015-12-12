// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.HtmlEditor.Linking
{

    /// <summary>
    /// </summary>
    public class HyperlinkingContextMenuDefinition : CommandContextMenuDefinition
    {
        private MenuDefinitionEntryCommand menuDefinitionEntryCommandRecentPost;
        private MenuDefinitionEntryCommand menuDefinitionEntryCommandGlossary;

        private IContainer components;

        public HyperlinkingContextMenuDefinition(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();
        }

        public HyperlinkingContextMenuDefinition()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.menuDefinitionEntryCommandRecentPost = new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntryCommand(this.components);
            this.menuDefinitionEntryCommandGlossary = new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntryCommand(this.components);
            //
            // menuDefinitionEntryCommandRecentPost
            //
            this.menuDefinitionEntryCommandRecentPost.CommandIdentifier = "OpenLiveWriter.PostEditor.OpenPost.CommandRecentPost";
            this.menuDefinitionEntryCommandRecentPost.SeparatorAfter = false;
            this.menuDefinitionEntryCommandRecentPost.SeparatorBefore = false;
            //
            // menuDefinitionEntryCommandGlossary
            //
            this.menuDefinitionEntryCommandGlossary.CommandIdentifier = "OpenLiveWriter.HtmlEditor.Linking.Commands.CommandGlossary";
            this.menuDefinitionEntryCommandGlossary.SeparatorAfter = false;
            this.menuDefinitionEntryCommandGlossary.SeparatorBefore = false;
            //
            // LinkingContextMenuDefinition
            //
            this.Entries.AddRange(new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntry[] {
                                                                                                        this.menuDefinitionEntryCommandRecentPost,
                                                                                                    	this.menuDefinitionEntryCommandGlossary});
        }
        #endregion
    }
}
