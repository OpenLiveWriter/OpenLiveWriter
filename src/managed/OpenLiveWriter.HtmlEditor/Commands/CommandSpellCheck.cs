// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.HtmlEditor.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandSpellCheck : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandSpellCheck(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

        }

        public CommandSpellCheck()
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
            //
            // CommandSpellCheck
            //
            this.ContextMenuPath = "";
            this.Identifier = "Onfolio.Core.HtmlEditor.Commands.SpellCheck";
            this.MainMenuPath = "Too&ls@9/Check &Spelling...@100";
            this.Shortcut = System.Windows.Forms.Shortcut.ShiftF7;
            this.Text = "Check Spelling";

        }
        #endregion
    }
}
