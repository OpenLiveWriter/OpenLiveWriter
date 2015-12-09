// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    class CommandBarSpacerLightweightControl : LightweightControl
    {
        /// <summary>
        /// The default width.
        /// </summary>
        private const int DEFAULT_WIDTH = 10;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the CommandBarSeparatorLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarSpacerLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarSeparatorLightweightControl class.
        /// </summary>
        public CommandBarSpacerLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
            InitializeObject();
        }

        private void InitializeObject()
        {
            VirtualSize = DefaultVirtualSize;
            AccessibleRole = System.Windows.Forms.AccessibleRole.Separator;
            AccessibleName = "Spacer";
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

        }
        #endregion

        /// <summary>
        /// Gets the default virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size DefaultVirtualSize
        {
            get
            {
                return new Size(DEFAULT_WIDTH, 0);
            }
        }

        /// <summary>
        /// Gets the minimum virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size MinimumVirtualSize
        {
            get
            {
                return DefaultVirtualSize;
            }
        }

    }
}
