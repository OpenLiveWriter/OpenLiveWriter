// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// Summary description for CommandImageDecoratorApply.
    /// </summary>
    public class CommandOpenSpellingForm : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandOpenSpellingForm(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandOpenSpellingForm()
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
            // CommandOpenSpellingForm
            //
            this.CommandBarButtonText = "";
            this.Identifier = "OpenLiveWriter.SpellChecker.CommandOpenSpellingForm";
            this.MenuText = "&Open Spelling Dialog";
            this.Text = "Open the spelling dialog";

        }
        #endregion
    }
}

