// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{

    public class CommandPreferences : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandPreferences(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandPreferences()
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
            // CommandConfigureWeblog
            //
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.Preferences";
            this.MainMenuPath = "&Tools@7/-&Preferences...@900";
            this.MenuText = "&Preferences...";

        }
        #endregion
    }
}
