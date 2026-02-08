// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Rendering;

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
        private static ToolTip _sharedToolTip;
        private string _currentTooltipText;

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
                     ControlStyles.OptimizedDoubleBuffer, true);

            // NOTE: We intentionally do NOT set SupportsTransparentBackColor or BackColor = Transparent.
            // Those flags cause WinForms to set WS_EX_TRANSPARENT on the window, which forces
            // the parent to repaint its area (including this control's region) every time this
            // control repaints. Combined with multiple layers of transparent controls (RibbonGroup
            // → TransparentPanel → RibbonButton), this creates a cascade of unnecessary repaints.
            // Since OnPaintBackground always fills with an opaque color, transparency simulation
            // is not needed and only causes overhead and potential rendering artifacts.

            // Initialize shared tooltip if needed
            if (_sharedToolTip == null)
            {
                _sharedToolTip = new ToolTip
                {
                    AutoPopDelay = 5000,
                    InitialDelay = 500,
                    ReshowDelay = 200,
                    ShowAlways = true
                };
            }
        }

        /// <summary>
        /// Override to initialize the double buffer with a proper background color.
        /// With OptimizedDoubleBuffer, an empty OnPaintBackground leaves the buffer
        /// uninitialized (black). We fill with the group background color to ensure
        /// no black shows through gaps in OnPaint rendering.
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Fill with opaque group background to initialize the double buffer
            e.Graphics.Clear(RibbonColors.Current.GetOpaqueGroupBackground());
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateTooltip();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _sharedToolTip?.SetToolTip(this, null);
            _currentTooltipText = null;
        }

        /// <summary>
        /// Updates the tooltip text from the associated command.
        /// </summary>
        protected virtual void UpdateTooltip()
        {
            var tooltip = CommandTooltip;
            if (!string.IsNullOrEmpty(tooltip) && tooltip != _currentTooltipText)
            {
                _currentTooltipText = tooltip;
                _sharedToolTip?.SetToolTip(this, tooltip);
            }
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
                // For ribbon controls, use command.Enabled for both enabled and disabled states
                // We keep controls visible in the ribbon - they should be disabled, not hidden
                // This matches standard Windows Ribbon behavior
                Enabled = command.Enabled;
                // Don't hide ribbon controls based on command visibility
                // Visibility is controlled by VisibleModes/ApplicationMode instead

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
