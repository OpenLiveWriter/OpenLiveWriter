// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// Summary description for CommandImageDecoratorApply.
    /// </summary>
    public class CommandAddToDictionary : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandAddToDictionary(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandAddToDictionary()
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
            // CommandAddToDictionary
            //
            this.CommandBarButtonText = "";
            this.Identifier = "OpenLiveWriter.SpellChecker.CommandAddToDictionary";
            this.MenuText = "&Add to Dictionary";
            this.Text = "Add this word to the local dictionary";

        }
        #endregion
    }
}

