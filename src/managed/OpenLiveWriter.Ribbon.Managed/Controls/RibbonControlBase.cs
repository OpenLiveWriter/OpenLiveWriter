// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Base class for all ribbon controls.
    /// </summary>
    public abstract class RibbonControlBase : Control
    {
        private RibbonCommandManager _commandManager;
        private CommandId _commandId;
        private RibbonApplicationMode _visibleModes = RibbonApplicationMode.All;
        private RibbonGroupSize _currentSize = RibbonGroupSize.Large;

        /// <summary>
        /// Gets or sets the command ID for this control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CommandId CommandId
        {
            get => _commandId;
            set
            {
                _commandId = value;
                UpdateFromCommand();
            }
        }

        /// <summary>
        /// Gets or sets the command manager.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonCommandManager CommandManager
        {
            get => _commandManager;
            set
            {
                if (_commandManager != value)
                {
                    if (_commandManager != null)
                    {
                        _commandManager.CommandStateChanged -= OnCommandStateChanged;
                    }
                    _commandManager = value;
                    if (_commandManager != null)
                    {
                        _commandManager.CommandStateChanged += OnCommandStateChanged;
                    }
                    UpdateFromCommand();
                }
            }
        }

        /// <summary>
        /// Gets or sets the application modes where this control is visible.
        /// </summary>
        public RibbonApplicationMode VisibleModes
        {
            get => _visibleModes;
            set => _visibleModes = value;
        }

        /// <summary>
        /// Gets or sets the current size for rendering.
        /// </summary>
        public RibbonGroupSize CurrentSize
        {
            get => _currentSize;
            set
            {
                if (_currentSize != value)
                {
                    _currentSize = value;
                    UpdateSize();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets the label text from the associated command.
        /// </summary>
        public string CommandLabel
        {
            get
            {
                var command = _commandManager?.GetCommand(_commandId);
                return command?.Label ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the tooltip text from the associated command.
        /// </summary>
        protected string CommandTooltip
        {
            get
            {
                var command = _commandManager?.GetCommand(_commandId);
                return command?.Tooltip ?? CommandLabel;
            }
        }

        /// <summary>
        /// Gets the large image from the associated command.
        /// </summary>
        protected Image CommandLargeImage
        {
            get
            {
                var command = _commandManager?.GetCommand(_commandId);
                return command?.LargeImage;
            }
        }

        /// <summary>
        /// Gets the small image from the associated command.
        /// </summary>
        protected Image CommandSmallImage
        {
            get
            {
                var command = _commandManager?.GetCommand(_commandId);
                return command?.SmallImage;
            }
        }

        /// <summary>
        /// Gets whether the associated command is enabled.
        /// </summary>
        protected bool CommandEnabled
        {
            get
            {
                var command = _commandManager?.GetCommand(_commandId);
                return command?.Enabled ?? true;
            }
        }

        /// <summary>
        /// Gets whether the associated command is checked.
        /// </summary>
        protected bool CommandChecked
        {
            get
            {
                var command = _commandManager?.GetCommand(_commandId);
                return command?.Checked ?? false;
            }
        }

        protected RibbonControlBase()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
        }

        /// <summary>
        /// Called when the command state changes.
        /// </summary>
        private void OnCommandStateChanged(object sender, CommandStateChangedEventArgs e)
        {
            if (e.CommandId == _commandId)
            {
                UpdateFromCommand();
                Invalidate();
            }
        }

        /// <summary>
        /// Updates the control from the associated command.
        /// </summary>
        protected virtual void UpdateFromCommand()
        {
            var command = _commandManager?.GetCommand(_commandId);
            if (command != null)
            {
                Enabled = command.Enabled;
                Visible = command.Visible;

                // Set tooltip
                if (!string.IsNullOrEmpty(command.Tooltip))
                {
                    // Could set up a ToolTip here
                }
            }
        }

        /// <summary>
        /// Updates the control size based on CurrentSize.
        /// </summary>
        protected virtual void UpdateSize()
        {
            // Override in derived classes to adjust size
        }

        /// <summary>
        /// Executes the associated command.
        /// </summary>
        public void ExecuteCommand()
        {
            _commandManager?.Execute(_commandId);
        }

        /// <summary>
        /// Simulates a click on the control.
        /// </summary>
        public virtual void PerformClick()
        {
            OnClick(EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_commandManager != null)
                {
                    _commandManager.CommandStateChanged -= OnCommandStateChanged;
                }
            }
            base.Dispose(disposing);
        }
    }
}
