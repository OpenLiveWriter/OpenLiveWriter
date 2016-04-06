// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Use this if you need a command bar as a heavyweight control.
    /// </summary>
    public class CommandBarControl : LightweightControlContainerControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        protected CommandBarLightweightControl _commandBar;

        public CommandBarControl()
        {
            InitializeComponent();
        }

        public CommandBarControl(CommandBarLightweightControl commandBar, CommandBarDefinition commandBarDefinition)
        {
            // It's important that the commandBarDefinition not be set
            // on the command bar until after the command bar has a heavyweight parent.
            // Otherwise, command bar entries that have heavyweight controls will never
            // be parented properly and therefore never show up.

            Debug.Assert(commandBar.CommandBarDefinition == null,
                         "Don't set the command bar definition before creating CommandBarControl!");

            InitializeComponent();

            _commandBar = commandBar;

            _commandBar.LightweightControlContainerControl = this;
            _commandBar.CommandBarDefinition = commandBarDefinition;

            _commandBar.VirtualBounds = new Rectangle(new Point(0, 0), _commandBar.DefaultVirtualSize);
            Height = _commandBar.VirtualHeight;
            SizeChanged += new EventHandler(CommandBarControl_SizeChanged);

            AccessibleRole = System.Windows.Forms.AccessibleRole.ToolBar;
            AccessibleName = commandBar.AccessibleName;
            InitFocusAndAccessibility();
        }

        private void InitFocusAndAccessibility()
        {
            InitFocusManager();
            AddFocusableControls(_commandBar.GetAccessibleControls());
        }

        public CommandBarLightweightControl CommandBarLightweightControl
        {
            get
            {
                return _commandBar;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        private void CommandBarControl_SizeChanged(object sender, EventArgs e)
        {
            _commandBar.VirtualWidth = Width;
            _commandBar.VirtualHeight = Height;
        }
    }
}
