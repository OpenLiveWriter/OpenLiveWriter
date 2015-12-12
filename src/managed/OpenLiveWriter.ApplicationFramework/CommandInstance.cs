// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Diagnostics;
using Project31.MindShare.CoreServices;

namespace Project31.ApplicationFramework
{
    /// <summary>
    /// Represents a command in the Project31.ApplicationFramework.
    /// </summary>
    [
    DesignTimeVisible(false),
    ToolboxItem(false)
    ]
    public class CommandInstance : Component
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Menu bitmap for the enabled state.
        /// </summary>
        private CommandDefinition commandDefinition;

        /// <summary>
        /// Gets or sets the menu bitmap for the enabled state.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(false),
        DefaultValue(null),
        Description("Specifies the menu bitmap for the enabled state.")
        ]
        public Bitmap MenuBitmapEnabled
        {
            get
            {
                return menuBitmapEnabled;
            }

            set
            {
                menuBitmapEnabled = value;
            }
        }

        /// <summary>
        /// Menu bitmap for the disabled state.
        /// </summary>
        private Bitmap menuBitmapDisabled;

        /// <summary>
        /// Gets or sets the menu bitmap for the disabled state.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(false),
        DefaultValue(null),
        Description("Specifies the menu bitmap for the disabled state.")
        ]
        public Bitmap MenuBitmapDisabled
        {
            get
            {
                return menuBitmapDisabled;
            }

            set
            {
                menuBitmapDisabled = value;
            }
        }

        /// <summary>
        /// Menu bitmap for the selected state.
        /// </summary>
        private Bitmap menuBitmapSelected;

        /// <summary>
        /// Gets or sets the menu selected bitmap.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(false),
        DefaultValue(null),
        Description("Specifies the menu bitmap for the selected state.")
        ]
        public Bitmap MenuBitmapSelected
        {
            get
            {
                return menuBitmapSelected;
            }

            set
            {
                menuBitmapSelected = value;
            }
        }

        /// <summary>
        /// The menu shortcut of the command.
        /// </summary>
        private Shortcut menuShortcut;

        /// <summary>
        /// Gets or sets the menu shortcut of the command.
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(Shortcut.None),
        Description("Specifies the menu shortcut of the command.")
        ]
        public Shortcut MenuShortcut
        {
            get
            {
                return menuShortcut;
            }
            set
            {
                menuShortcut = value;
            }
        }

        /// <summary>
        /// A value indicating whether the menu shortcut should be shown for the command.
        /// </summary>
        private bool showMenuShortcut;

        /// <summary>
        /// Gets or sets a value indicating whether the menu shortcut should be shown for the command.
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        Description("Specifies whether the menu shortcut should be shown for the command.")
        ]
        public bool ShowMenuShortcut
        {
            get
            {
                return showMenuShortcut;
            }
            set
            {
                showMenuShortcut = value;
            }
        }

        /// <summary>
        /// The command text.  This is the "user visible text" that is associate with the command
        /// (such as "Save All").  It appears whenever the user can see text for the command.
        /// </summary>
        private string text;

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(true),
        DefaultValue(null),
        Description("Specifies the text that is associated with the command.")
        ]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        /// <summary>
        /// The command description.  This is the "user visible description" that is associated
        /// with the command.  It appears whenever the user can see a description for the command.
        /// </summary>
        private string description;

        /// <summary>
        /// Gets or sets the command description.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(true),
        DefaultValue(null),
        Description("Specifies the description for the command.")
        ]
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        /// <summary>
        /// Command bar button bitmap for the disabled state.
        /// </summary>
        private Bitmap commandBarButtonBitmapDisabled;

        /// <summary>
        /// Gets or sets the command bar button bitmap for the disabled state.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(true),
        DefaultValue(null),
        Description("Specifies the command bar button bitmap for the disabled state.")
        ]
        public Bitmap CommandBarButtonBitmapDisabled
        {
            get
            {
                return commandBarButtonBitmapDisabled;
            }

            set
            {
                commandBarButtonBitmapDisabled = value;
            }
        }

        /// <summary>
        /// Command bar button bitmap for the enabled state.
        /// </summary>
        private Bitmap commandBarButtonBitmapEnabled;

        /// <summary>
        /// Gets or sets the command bar button bitmap for the enabled state.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(true),
        DefaultValue(null),
        Description("Specifies the command bar button bitmap for the enabled state.")
        ]
        public Bitmap CommandBarButtonBitmapEnabled
        {
            get
            {
                return commandBarButtonBitmapEnabled;
            }

            set
            {
                commandBarButtonBitmapEnabled = value;
            }
        }

        /// <summary>
        /// Command bar button bitmap for the pushed state.
        /// </summary>
        private Bitmap commandBarButtonBitmapPushed;

        /// <summary>
        /// Gets or sets the command bar button bitmap for the pushed state.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(true),
        DefaultValue(null),
        Description("Specifies the command bar button bitmap for the pushed state.")
        ]
        public Bitmap CommandBarButtonBitmapPushed
        {
            get
            {
                return commandBarButtonBitmapPushed;
            }

            set
            {
                commandBarButtonBitmapPushed = value;
            }
        }

        /// <summary>
        /// Command bar button bitmap for the rollover state.
        /// </summary>
        private Bitmap commandBarButtonBitmapRollover;

        /// <summary>
        /// Gets or sets the command bar button bitmap for the rollover state.
        /// </summary>
        [
        Category("Appearance"),
        Localizable(true),
        DefaultValue(null),
        Description("Specifies the command bar button bitmap for the rollover state.")
        ]
        public Bitmap CommandBarButtonBitmapRollover
        {
            get
            {
                return commandBarButtonBitmapRollover;
            }

            set
            {
                commandBarButtonBitmapRollover = value;
            }
        }

        /// <summary>
        /// A value indicating whether the command is enabled or not (i.e. can respond to user interaction).
        /// </summary>
        private bool enabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether the command is enabled or not (i.e. can respond to user interaction).
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(true),
        Description("Specifies whether the command is enabled by default.")
        ]
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                //	Set the value.
                enabled = value;

                //	Fire the enabled changed event.
                OnEnabledChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when the command is executed.
        /// </summary>
        [
        Category("Action"),
        Description("Occurs when the command is executed.")
        ]
        public event EventHandler Execute;

        /// <summary>
        /// Occurs when the command's enabled state changes.
        /// </summary>
        [
        Category("Property Changed"),
        Description("Occurs when the command's enabled state changes.")
        ]
        public event EventHandler EnabledChanged;

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        /// <param name="container"></param>
        public CommandDefinition(System.ComponentModel.IContainer container)
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
        public CommandDefinition()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
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
        /// This method can be called to raise the Execute event
        /// </summary>
        public void PerformExecute()
        {
            Debug.Assert(Enabled, "Command is disabled.", "It is illogical to execute a command that is disabled.");
            OnExecute(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the Execute event.
        /// </summary>
        protected void OnExecute(EventArgs e)
        {
            if (Execute != null)
                Execute(this, e);
            else
                UnderConstructionForm.Show();
        }

        /// <summary>
        /// Raises the EnabledChanged event.
        /// </summary>
        protected void OnEnabledChanged(EventArgs e)
        {
            if (EnabledChanged != null)
                EnabledChanged(this, e);
        }
    }
}
