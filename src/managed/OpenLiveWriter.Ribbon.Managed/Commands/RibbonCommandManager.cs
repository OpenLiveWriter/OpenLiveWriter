// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Ribbon.Managed.Commands
{
    /// <summary>
    /// Manages the mapping between CommandIds and ribbon controls.
    /// Acts as a bridge to the existing CommandManager.
    /// </summary>
    public class RibbonCommandManager
    {
        private readonly Dictionary<CommandId, IRibbonCommand> _commands = new Dictionary<CommandId, IRibbonCommand>();
        private readonly Dictionary<CommandId, List<Action>> _invalidationCallbacks = new Dictionary<CommandId, List<Action>>();

        /// <summary>
        /// Event raised when any command state changes.
        /// </summary>
        public event EventHandler<CommandStateChangedEventArgs> CommandStateChanged;

        /// <summary>
        /// Registers a command with the manager.
        /// </summary>
        public void RegisterCommand(IRibbonCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _commands[command.Id] = command;
            command.StateChanged += OnCommandStateChanged;
        }

        /// <summary>
        /// Unregisters a command from the manager.
        /// </summary>
        public void UnregisterCommand(CommandId id)
        {
            if (_commands.TryGetValue(id, out var command))
            {
                command.StateChanged -= OnCommandStateChanged;
                _commands.Remove(id);
            }
        }

        /// <summary>
        /// Gets a command by its ID.
        /// </summary>
        public IRibbonCommand GetCommand(CommandId id)
        {
            _commands.TryGetValue(id, out var command);
            return command;
        }

        /// <summary>
        /// Checks if a command is registered.
        /// </summary>
        public bool HasCommand(CommandId id)
        {
            return _commands.ContainsKey(id);
        }

        /// <summary>
        /// Sets the enabled state of a command.
        /// </summary>
        public void SetEnabled(CommandId id, bool enabled)
        {
            if (_commands.TryGetValue(id, out var command))
            {
                command.Enabled = enabled;
            }
        }

        /// <summary>
        /// Sets the visible state of a command.
        /// </summary>
        public void SetVisible(CommandId id, bool visible)
        {
            if (_commands.TryGetValue(id, out var command))
            {
                command.Visible = visible;
            }
        }

        /// <summary>
        /// Sets the checked state of a command.
        /// </summary>
        public void SetChecked(CommandId id, bool isChecked)
        {
            if (_commands.TryGetValue(id, out var command))
            {
                command.Checked = isChecked;
            }
        }

        /// <summary>
        /// Executes a command.
        /// </summary>
        public void Execute(CommandId id)
        {
            if (_commands.TryGetValue(id, out var command))
            {
                command.PerformExecute();
            }
        }

        /// <summary>
        /// Invalidates a command, causing it to refresh its state.
        /// </summary>
        public void Invalidate(CommandId id)
        {
            if (_commands.TryGetValue(id, out var command))
            {
                command.Invalidate();
            }

            // Notify any registered callbacks
            if (_invalidationCallbacks.TryGetValue(id, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback?.Invoke();
                }
            }
        }

        /// <summary>
        /// Registers a callback to be invoked when a command is invalidated.
        /// </summary>
        public void RegisterInvalidationCallback(CommandId id, Action callback)
        {
            if (!_invalidationCallbacks.TryGetValue(id, out var callbacks))
            {
                callbacks = new List<Action>();
                _invalidationCallbacks[id] = callbacks;
            }
            callbacks.Add(callback);
        }

        /// <summary>
        /// Gets all registered commands.
        /// </summary>
        public IEnumerable<IRibbonCommand> GetAllCommands()
        {
            return _commands.Values;
        }

        private void OnCommandStateChanged(object sender, EventArgs e)
        {
            if (sender is IRibbonCommand command)
            {
                CommandStateChanged?.Invoke(this, new CommandStateChangedEventArgs(command.Id));
            }
        }
    }

    /// <summary>
    /// Event args for command state changes.
    /// </summary>
    public class CommandStateChangedEventArgs : EventArgs
    {
        public CommandId CommandId { get; }

        public CommandStateChangedEventArgs(CommandId commandId)
        {
            CommandId = commandId;
        }
    }
}
