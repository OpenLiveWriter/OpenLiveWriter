// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Ribbon.Managed.Commands
{
    /// <summary>
    /// Bridge between the managed ribbon and the existing CommandManager.
    /// Creates IRibbonCommand wrappers for existing Command objects.
    /// </summary>
    public class CommandManagerBridge
    {
        private readonly object _existingCommandManager;
        private readonly RibbonCommandManager _ribbonCommandManager;
        private readonly Dictionary<CommandId, BridgedCommand> _bridgedCommands = new Dictionary<CommandId, BridgedCommand>();

        /// <summary>
        /// Gets the ribbon command manager.
        /// </summary>
        public RibbonCommandManager RibbonCommandManager => _ribbonCommandManager;

        /// <summary>
        /// Initializes a new instance of the CommandManagerBridge class.
        /// </summary>
        /// <param name="existingCommandManager">The existing CommandManager instance.</param>
        public CommandManagerBridge(object existingCommandManager)
        {
            _existingCommandManager = existingCommandManager ?? throw new ArgumentNullException(nameof(existingCommandManager));
            _ribbonCommandManager = new RibbonCommandManager();
        }

        /// <summary>
        /// Registers commands from the existing CommandManager into the ribbon command manager.
        /// </summary>
        public void RegisterCommands(IEnumerable<CommandId> commandIds)
        {
            foreach (var id in commandIds)
            {
                RegisterCommand(id);
            }
        }

        /// <summary>
        /// Registers a single command.
        /// </summary>
        public void RegisterCommand(CommandId commandId)
        {
            if (_bridgedCommands.ContainsKey(commandId))
                return;

            var bridgedCommand = new BridgedCommand(commandId, _existingCommandManager);
            _bridgedCommands[commandId] = bridgedCommand;
            _ribbonCommandManager.RegisterCommand(bridgedCommand);
        }

        /// <summary>
        /// Gets the bridged command for a command ID.
        /// </summary>
        public BridgedCommand GetBridgedCommand(CommandId commandId)
        {
            _bridgedCommands.TryGetValue(commandId, out var command);
            return command;
        }

        /// <summary>
        /// Invalidates a command, refreshing its state from the source.
        /// </summary>
        public void Invalidate(CommandId commandId)
        {
            if (_bridgedCommands.TryGetValue(commandId, out var command))
            {
                command.RefreshFromSource();
            }
        }

        /// <summary>
        /// Invalidates all commands.
        /// </summary>
        public void InvalidateAll()
        {
            foreach (var command in _bridgedCommands.Values)
            {
                command.RefreshFromSource();
            }
        }
    }

    /// <summary>
    /// A bridged command that wraps an existing Command object.
    /// </summary>
    public class BridgedCommand : IRibbonCommand
    {
        private readonly CommandId _commandId;
        private readonly object _existingCommandManager;
        private readonly dynamic _sourceCommand;

        private bool _enabled = true;
        private bool _visible = true;
        private bool _isChecked;
        private string _label;
        private string _tooltip;
        private string _keytip = string.Empty;
        private Image _largeImage;
        private Image _smallImage;

        public CommandId Id => _commandId;
        public string Label => _label ?? _commandId.ToString();
        public string Tooltip => _tooltip ?? Label;
        public string Keytip => _keytip;
        public Image LargeImage => _largeImage;
        public Image SmallImage => _smallImage;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnStateChanged();
                }
            }
        }

        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnStateChanged();
                }
            }
        }

        public bool Checked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnStateChanged();
                }
            }
        }

        public event EventHandler Execute;
        public event EventHandler StateChanged;

        public BridgedCommand(CommandId commandId, object existingCommandManager)
        {
            _commandId = commandId;
            _existingCommandManager = existingCommandManager;

            // Try to get source command
            try
            {
                var type = existingCommandManager.GetType();
                var getMethod = type.GetMethod("Get", new[] { typeof(CommandId) });
                if (getMethod != null)
                {
                    _sourceCommand = getMethod.Invoke(existingCommandManager, new object[] { commandId });
                }
            }
            catch
            {
                // If reflection fails, we'll use defaults
            }

            RefreshFromSource();
        }

        /// <summary>
        /// Refreshes command state from the source command.
        /// </summary>
        public void RefreshFromSource()
        {
            if (_sourceCommand == null)
            {
                // Use command ID as fallback label
                _label = _commandId.ToString();
                _tooltip = _commandId.ToString();
                return;
            }

            try
            {
                // Read properties from source command using reflection
                var sourceType = ((object)_sourceCommand).GetType();

                // Get enabled state
                var enabledProp = sourceType.GetProperty("Enabled");
                if (enabledProp != null)
                {
                    _enabled = (bool)enabledProp.GetValue(_sourceCommand);
                }

                // Get visible state
                var visibleProp = sourceType.GetProperty("On") ?? sourceType.GetProperty("Visible");
                if (visibleProp != null)
                {
                    _visible = (bool)visibleProp.GetValue(_sourceCommand);
                }

                // Get checked/latched state
                var latchedProp = sourceType.GetProperty("Latched") ?? sourceType.GetProperty("Checked");
                if (latchedProp != null)
                {
                    _isChecked = (bool)latchedProp.GetValue(_sourceCommand);
                }

                // Get label
                var labelProp = sourceType.GetProperty("LabelTitle") ?? sourceType.GetProperty("Text");
                if (labelProp != null)
                {
                    _label = (string)labelProp.GetValue(_sourceCommand);
                }

                // Get tooltip
                var tooltipProp = sourceType.GetProperty("TooltipTitle") ?? sourceType.GetProperty("ToolTip");
                if (tooltipProp != null)
                {
                    _tooltip = (string)tooltipProp.GetValue(_sourceCommand);
                }

                // Get large image
                var largeImageProp = sourceType.GetProperty("LargeImage") ?? sourceType.GetProperty("CommandBarButtonBitmapLarge");
                if (largeImageProp != null)
                {
                    _largeImage = largeImageProp.GetValue(_sourceCommand) as Image;
                }

                // Get small image
                var smallImageProp = sourceType.GetProperty("SmallImage") ?? sourceType.GetProperty("CommandBarButtonBitmapSmall");
                if (smallImageProp != null)
                {
                    _smallImage = smallImageProp.GetValue(_sourceCommand) as Image;
                }
            }
            catch
            {
                // If any reflection fails, continue with existing values
            }

            OnStateChanged();
        }

        public void PerformExecute()
        {
            if (!Enabled)
                return;

            Execute?.Invoke(this, EventArgs.Empty);

            // Execute the source command
            if (_sourceCommand != null)
            {
                try
                {
                    var sourceType = ((object)_sourceCommand).GetType();
                    var executeMethod = sourceType.GetMethod("PerformExecute") ??
                                       sourceType.GetMethod("Execute");

                    if (executeMethod != null)
                    {
                        executeMethod.Invoke(_sourceCommand, null);
                    }
                }
                catch
                {
                    // Execution failed
                }
            }
        }

        public void Invalidate()
        {
            RefreshFromSource();
        }

        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
