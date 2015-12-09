// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Base class of command bar entries.
    /// </summary>
    [
        DesignTimeVisible(false),
            ToolboxItem(false)
    ]
    public class CommandBarEntry : Component
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarEntry(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        public CommandBarEntry()
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
            components = new Container();
        }

        /// <summary>
        /// Gets the command bar lightweight control for this command bar entry.
        /// </summary>
        /// <returns>Lightweight control.</returns>
        public virtual LightweightControl GetLightweightControl(CommandBarLightweightControl commandBarLightweightControl, bool rightAligned)
        {
            return null;
        }
    }
}
