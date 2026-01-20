// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Represents errors that occur during application execution.
    /// </summary>
    public class Exceptor : System.ComponentModel.Component
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// The message template.
        /// </summary>
        private string messageTemplate;

        /// <summary>
        /// Gets or sets the message template.
        /// </summary>
        public string MessageTemplate
        {
            get
            {
                return messageTemplate;
            }
            set
            {
                messageTemplate = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Exceptor class.
        /// </summary>
        /// <param name="container"></param>
        public Exceptor(System.ComponentModel.IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Initializes a new instance of the Exceptor class.
        /// </summary>
        public Exceptor()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
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

    }
}
