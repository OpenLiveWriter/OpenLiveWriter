// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.HtmlEditor.Linking.Commands
{
    /// <summary>
    /// Summary description for CommandAddToGlossary.
    /// </summary>
    public class CommandAddToGlossary : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandAddToGlossary(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandAddToGlossary()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // CommandAddToGlossary
            //
            this.ContextMenuPath = "&Add to Glossary@104";
            this.Identifier = "OpenLiveWriter.HtmlEditor.Linking.Commands.CommandAddToGlossary";
            this.Text = "Add To Glossary";
            this.MenuText = "&Add to Glossary";
        }
        #endregion
    }
}
