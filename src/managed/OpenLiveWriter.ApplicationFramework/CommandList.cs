// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Project31.CoreServices;

namespace Project31.ApplicationFramework
{
    /// <summary>
    /// Summary description for CommandList.
    /// </summary>
    [Designer(typeof(ComponentRootDesigner), typeof(IRootDesigner))]
    public class CommandList : Component
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// The command collection for this command list.
        /// </summary>
        private CommandCollection commands = new CommandCollection();

        /// <summary>
        /// Gets or sets the command collection for this command list.
        /// </summary>
        [
        Localizable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)
        ]
        public CommandCollection Commands
        {
            get
            {
                return commands;
            }
            set
            {
                commands = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the CommandList class.
        /// </summary>
        /// <param name="container">Component container.</param>
        public CommandList(System.ComponentModel.IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the CommandList class.
        /// </summary>
        public CommandList()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        /// <summary>
        /// Helper to enable all the commands in the command list.
        /// </summary>
        public void Enable()
        {
            foreach (Command command in commands)
                command.Enabled = true;
        }

        /// <summary>
        /// Helper to disable all the commands in the command list.
        /// </summary>
        public void Disable()
        {
            foreach (Command command in commands)
                command.Enabled = false;
        }
    }
}
