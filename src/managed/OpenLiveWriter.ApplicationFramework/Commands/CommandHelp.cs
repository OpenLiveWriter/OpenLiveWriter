// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    public class CommandHelp : Command
    {

        public CommandHelp(System.ComponentModel.IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public CommandHelp()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
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
            // CommandHelp
            //
            this.CommandBarButtonText = "";
            this.ContextMenuPath = "{0} &Help-@101";
            this.Identifier = "OpenLiveWriter.Commands.Help";
            this.MainMenuPath = "&Help@500/&{0} Help-@101";
            this.MenuText = "{0} &Help";
            this.Shortcut = System.Windows.Forms.Shortcut.F1;
            this.Text = "Help";

        }
        #endregion
    }
}
