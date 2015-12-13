// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandOutdent : Command
    {

        public CommandOutdent(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandOutdent()
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
            // CommandOutdent
            //
            this.ContextMenuPath = "&Decrease Indent@111";
            this.Identifier = "MindShare.ApplicationCore.Commands.Outdent";
            this.MainMenuPath = "F&ormat@5/&Indent@500/&Decrease@101";
            this.MenuText = "&Decrease Indent";
            this.Text = "Decrease Indent";

        }
        #endregion
    }
}
