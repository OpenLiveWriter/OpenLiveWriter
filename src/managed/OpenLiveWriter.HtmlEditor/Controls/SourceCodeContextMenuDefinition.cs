// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.HtmlEditor.Controls
{
    /// <summary>
    /// Summary description for SourceCodeContextMenuDefinition.
    /// </summary>
    public class SourceCodeContextMenuDefinition : CommandContextMenuDefinition
    {
        private MenuDefinitionEntryCommand menuDefinitionEntryCommandCut;
        private MenuDefinitionEntryCommand menuDefinitionEntryCommandCopy;
        private MenuDefinitionEntryCommand menuDefinitionEntryCommandPaste;
        private MenuDefinitionEntryCommand menuDefinitionEntryCommandPasteSpecial;
        private MenuDefinitionEntryCommand menuDefinitionEntryCommandSelectAll;
        private MenuDefinitionEntryCommand menuDefinitionEntryCommandInsertLink;

        private IContainer components;

        public SourceCodeContextMenuDefinition(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();
        }

        public SourceCodeContextMenuDefinition()
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
            this.menuDefinitionEntryCommandCut = new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntryCommand(this.components);
            this.menuDefinitionEntryCommandCopy = new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntryCommand(this.components);
            this.menuDefinitionEntryCommandPaste = new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntryCommand(this.components);
            this.menuDefinitionEntryCommandPasteSpecial = new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntryCommand(this.components);
            this.menuDefinitionEntryCommandSelectAll = new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntryCommand(this.components);
            this.menuDefinitionEntryCommandInsertLink = new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntryCommand(this.components);
            //
            // menuDefinitionEntryCommandCut
            //
            this.menuDefinitionEntryCommandCut.CommandIdentifier = "MindShare.ApplicationCore.Commands.Cut";
            this.menuDefinitionEntryCommandCut.SeparatorAfter = false;
            this.menuDefinitionEntryCommandCut.SeparatorBefore = false;
            //
            // menuDefinitionEntryCommandCopy
            //
            this.menuDefinitionEntryCommandCopy.CommandIdentifier = "MindShare.ApplicationCore.Commands.Copy";
            this.menuDefinitionEntryCommandCopy.SeparatorAfter = false;
            this.menuDefinitionEntryCommandCopy.SeparatorBefore = false;
            //
            // menuDefinitionEntryCommandPaste
            //
            this.menuDefinitionEntryCommandPaste.CommandIdentifier = "MindShare.ApplicationCore.Commands.Paste";
            this.menuDefinitionEntryCommandPaste.SeparatorAfter = false;
            this.menuDefinitionEntryCommandPaste.SeparatorBefore = false;
            //
            // menuDefinitionEntryCommandPasteSpecial
            //
            this.menuDefinitionEntryCommandPasteSpecial.CommandIdentifier = "MindShare.ApplicationCore.Commands.PasteSpecial";
            this.menuDefinitionEntryCommandPasteSpecial.SeparatorAfter = false;
            this.menuDefinitionEntryCommandPasteSpecial.SeparatorBefore = false;
            //
            // menuDefinitionEntryCommandSelectAll
            //
            this.menuDefinitionEntryCommandSelectAll.CommandIdentifier = "MindShare.ApplicationCore.Commands.SelectAll";
            this.menuDefinitionEntryCommandSelectAll.SeparatorAfter = true;
            this.menuDefinitionEntryCommandSelectAll.SeparatorBefore = true;
            //
            // menuDefinitionEntryCommandInsertLink
            //
            this.menuDefinitionEntryCommandInsertLink.CommandIdentifier = "OpenLiveWriter.ApplicationFramework.Commands.InsertLink";
            this.menuDefinitionEntryCommandInsertLink.SeparatorAfter = true;
            this.menuDefinitionEntryCommandInsertLink.SeparatorBefore = true;

            this.Entries.AddRange(new OpenLiveWriter.ApplicationFramework.MenuDefinitionEntry[] {
                                                                                                        this.menuDefinitionEntryCommandCut,
                                                                                                        this.menuDefinitionEntryCommandCopy,
                                                                                                        this.menuDefinitionEntryCommandPaste,
                                                                                                        this.menuDefinitionEntryCommandPasteSpecial,
                                                                                                        this.menuDefinitionEntryCommandSelectAll,
                                                                                                        this.menuDefinitionEntryCommandInsertLink});

        }
        #endregion
    }
}
