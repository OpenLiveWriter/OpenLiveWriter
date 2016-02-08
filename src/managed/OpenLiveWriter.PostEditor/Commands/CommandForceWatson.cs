// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.ApplicationFramework;
using System.ComponentModel;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Summary description for CommandForceWatson.
    /// </summary>
    public class CommandForceWatson : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandForceWatson(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

            //
            //
            //
        }

        public CommandForceWatson()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
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
            // CommandForceWatson
            //
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.ForceWatson";
            this.MenuText = "Force Watson";
            this.Text = "Force Watson";

        }
        #endregion
    }
}
