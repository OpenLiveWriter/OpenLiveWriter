// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Interop.Com.Ribbon;

namespace OpenLiveWriter.ApplicationFramework
{
    public delegate void ExecuteWithArgsDelegate(ExecuteEventHandlerArgs args);

    public class CommandManager : Component, IUICommandHandler, IUICommandHandlerOverride
    {
        #region Private Class Declarations

        /// <summary>
        ///	Manages command instances.
        /// </summary>
        private class CommandInstanceManager
        {
            /// <summary>
            ///	The command instance collection.
            /// </summary>
            private CommandCollection commandInstanceCollection = new CommandCollection();

            /// <summary>
            /// Initializes a new instance of the CommandInstanceManager class.
            /// </summary>
            /// <param name="command">The initial command instance to add.</param>
            public CommandInstanceManager(Command command)
            {
                Add(command);
            }

            /// <summary>
            /// Gets a value indicating whether the CommandInstanceManager is empty.
            /// </summary>
            public bool IsEmpty
            {
                get
                {
                    return commandInstanceCollection.Count == 0;
                }
            }

            /// <summary>
            /// Gets the active command instance.
            /// </summary>
            public Command ActiveCommandInstance
            {
                get
                {
                    return IsEmpty ? null : commandInstanceCollection[commandInstanceCollection.Count - 1];
                }
            }

            /// <summary>
            /// Adds a command instance.
            /// </summary>
            /// <param name="command">The command instance to add.</param>
            public void Add(Command command)
            {
                //	Ensure that the command instance has not already been added.
                Debug.Assert(!commandInstanceCollection.Contains(command), String.Format(CultureInfo.InvariantCulture, "Command instance {0} already added.", command.Identifier));

                //	Add the command instance.
                if (!commandInstanceCollection.Contains(command))
                    commandInstanceCollection.Add(command);
            }

            /// <summary>
            /// Removes a command instance.
            /// </summary>
            /// <param name="command">The command to remove.</param>
            public void Remove(Command command)
            {
                //	Ensure that the command instance has been added and is not active.
                Debug.Assert(commandInstanceCollection.Contains(command), String.Format(CultureInfo.InvariantCulture, "Command instance {0} not found.", command.Identifier));

                //	Remove the command instance.
                if (commandInstanceCollection.Contains(command))
                    commandInstanceCollection.Remove(command);
            }
        }

        #endregion Private Class Declarations

        #region Private Member Variables

        private GenericCommandHandler genericCommandHandler;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// The command Command table, keyed by command identifier.
        /// </summary>
        private Hashtable commandTable = new Hashtable();

        /// <summary>
        ///	The cross-referenced set of active Shortcuts.  Keyed by Command.Shortcut.  This table
        ///	is not constructed or maintained until it is first used.
        /// </summary>
        private Hashtable commandShortcutTable = null;

        /// <summary>
        /// If true, the commandShortcutTable needs to be repopulated before the next time it is
        /// consulted. The two things that affect the state here are 1) the set of commands that
        /// are loaded (whether enabled or disabled), and 2) the shortcuts/advanced shortcuts of
        /// those commands. If either changes then this flag needs to be set to true.
        ///
        /// The reason this is important is because otherwise the commandShortcutTable will be
        /// fully rebuilt on every keypress in the editor (actually it currently happens twice),
        /// which is enough to make typing feel sluggish.
        /// </summary>
        private bool commandShortcutTableIsStale = true;

        /// <summary>
        ///	The cross-referenced set of active AcceleratorMnemonic values. Keyed by Command.AcceleratorMnemonic.
        ///	This table is not constructed or maintained until it is first used.
        /// </summary>
        private Hashtable acceleratorMnemonicTable = null;

        /// <summary>
        ///	The cross-referenced set of active CommandBarButtonContextMenuAcceleratorMnemonic
        ///	values. Keyed by Command.CommandBarButtonContextMenuAcceleratorMnemonic.  This table is
        ///	not constructed or maintained until it is first used.
        /// </summary>
        private Hashtable commandBarButtonContextMenuAcceleratorMnemonicTable = null;

        /// <summary>
        /// A set of shortcuts that need to be ignored. Always check for null before accessing.
        /// </summary>
        private HashSet maskedShortcuts;

        /// <summary>
        /// Update count.
        /// </summary>
        private int updateCount = 0;

        /// <summary>
        /// Change count.
        /// </summary>
        private bool pendingChange = false;

        /// <summary>
        /// A value which indicates whether we are suppressing events.
        /// </summary>
        private bool suppressEvents;

        #endregion Private Member Variables

        #region Public Events

        /// <summary>
        /// The changed event.
        /// </summary>
        public event EventHandler Changed;

        #endregion Public Events

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        public CommandManager(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            CreateGenericCommandHandler();
            container.Add(this);
            InitializeComponent();
        }

        private void CreateGenericCommandHandler()
        {
            genericCommandHandler = new GenericCommandHandler(this);
        }

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        public CommandManager()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            CreateGenericCommandHandler();
            InitializeComponent();
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

        #endregion Class Initialization & Termination

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

        #region Public Properties

        /// <summary>
        /// Gets or sets a value which indicates whether we are suppressing events.
        /// </summary>
        public bool SuppressEvents
        {
            get
            {
                return suppressEvents;
            }
            set
            {
                suppressEvents = value;
            }
        }

        /// <summary>
        /// Gets the count of commands in the command manager.
        /// </summary>
        public int Count
        {
            get
            {
                return commandTable.Count;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Begins update to CommandManager allowing multiple Change events to be batched.
        /// </summary>
        public void BeginUpdate()
        {
            updateCount++;
        }

        /// <summary>
        /// Ends update to CommandManager allowing multiple Change events to be batched.
        /// </summary>
        public void EndUpdate()
        {
            EndUpdate(false);
        }

        public void EndUpdate(bool forceUpdate)
        {
            Debug.Assert(updateCount > 0, "EndUpdate called incorrectly.");
            if (updateCount > 0)
                if (--updateCount == 0)
                {
                    if (pendingChange || forceUpdate)
                    {
                        pendingChange = false;

                        foreach (var entry in batchedCommands)
                        {
                            if (CommandStateChanged != null)
                                CommandStateChanged(entry.Key, entry.Value);
                        }
                        batchedCommands.Clear();

                        OnChanged(EventArgs.Empty);
                    }
                }
        }

        /// <summary>
        /// Adds a set of Commands.
        /// </summary>
        /// <param name="commands">The Command(s) to add.</param>
        public void Add(params Command[] commands)
        {
            foreach (Command command in commands)
            {
                AddCommand(command);
                command.StateChanged += OnCommandStateChanged;
                OnCommandStateChanged(command, EventArgs.Empty);
            }

            OnChanged(EventArgs.Empty);
        }

        private const int MAX_BATCHED_INVALIDATIONS = 90;
        private Dictionary<object, EventArgs> batchedCommands = new Dictionary<object, EventArgs>(MAX_BATCHED_INVALIDATIONS);

        private void OnCommandStateChanged(object sender, EventArgs e)
        {
            // If we're in batch mode, then make a note of the sender and batch notifications
            if (updateCount > 0)
            {
                if (!batchedCommands.ContainsKey(sender))
                {
                    pendingChange = true;
                    batchedCommands.Add(sender, e);
                }

                Debug.Assert(batchedCommands.Count <= MAX_BATCHED_INVALIDATIONS, "Need to increase the size of MAX_BATCHED_INVALIDATIONS.");
            }
            else
            {
                // Send notification directly
                if (CommandStateChanged != null)
                    CommandStateChanged(sender, e);
            }

        }

        public Command Add(CommandId commandId, EventHandler handler)
        {
            Command command = new Command(commandId);
            command.Execute += handler;
            Add(command);
            return command;
        }

        public Command Add(Command command, ExecuteEventHandler handler)
        {
            command.ExecuteWithArgs += handler;
            Add(command);
            return command;
        }

        public Command Add(CommandId commandId, EventHandler handler, bool enabled)
        {
            Command command = new Command(commandId);
            command.Execute += handler;
            command.Enabled = enabled;
            Add(command);
            return command;
        }

        public event EventHandler CommandStateChanged;

        /// <summary>
        /// Adds a set of Commands.
        /// </summary>
        /// <param name="commandCollection">The Command(s) to add.</param>
        public void Add(CommandCollection commandCollection)
        {
            foreach (Command command in commandCollection)
                AddCommand(command);
            OnChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Removes a set of Commands.
        /// </summary>
        /// <param name="commands">The Command(s) to remove.</param>
        public void Remove(params Command[] commands)
        {
            foreach (Command command in commands)
                RemoveCommand(command);
            OnChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Removes a set of Commands.
        /// </summary>
        /// <param name="commandCollection">The Command(s) to remove.</param>
        public void Remove(CommandCollection commandCollection)
        {
            foreach (Command command in commandCollection)
                RemoveCommand(command);
            OnChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Processes a CmdKey for Command.Shortcut matches.
        /// </summary>
        /// <param name="keyData">Specifies key codes and modifiers and process.</param>
        /// <returns>true if the Keys value was processed; otherwise, false.</returns>
        public bool ProcessCmdKeyShortcut(Keys keyData)
        {
            Command command = null;

            if (keyData != Keys.None)
                command = FindCommandWithShortcut(keyData);

            if (command != null)
            {
                if (command.On && command.Enabled)
                {
                    //	Ensure that any command initialization has been performed by manually
                    //	firing the BeforeShowInMenu event on the command.  Very important.
                    command.InvokeBeforeShowInMenu(EventArgs.Empty);
                    if (!command.On || !command.Enabled)
                        return false;

                    //	Execute the command.
                    ExecuteCommandAndFireEvents(command);
                    return true;
                }
            }

            //	We did not process the CmdKey.
            return false;
        }

        /// <summary>
        /// Processes a CmdKey for Command.AcceleratorMnemonic matches.
        /// </summary>
        /// <param name="keyData">Specifies key codes and modifiers and process.</param>
        /// <returns>true if the Keys value was processed; otherwise, false.</returns>
        public bool ProcessCmdKeyAcceleratorMnemonic(Keys keyData)
        {
            //	Attempt to process the Keys value as a Command.AcceleratorMnemonic.
            AcceleratorMnemonic acceleratorMnemonic = KeyboardHelper.MapToAcceleratorMnemonic(keyData);
            if (acceleratorMnemonic != AcceleratorMnemonic.None)
            {
                Command command = FindCommandWithAcceleratorMnemonic(acceleratorMnemonic);
                if (command != null)
                {
                    if (command.On && command.Enabled)
                    {
                        //	Ensure that any command initialization has been performed by manually
                        //	firing the BeforeShowInMenu event on the command.  Very important.
                        command.InvokeBeforeShowInMenu(EventArgs.Empty);
                        if (!command.On || !command.Enabled)
                            return false;

                        //	Execute the command.
                        ExecuteCommandAndFireEvents(command);
                        return true;
                    }
                }
            }

            //	We did not process the CmdKey.
            return false;
        }

        /// <summary>
        /// Processes a CmdKey for Command.CommandBarButtonContextMenuAcceleratorMnemonic matches.
        /// </summary>
        /// <param name="keyData">Specifies key codes and modifiers and process.</param>
        /// <returns>true if the Keys value was processed; otherwise, false.</returns>
        public bool ProcessCmdKeyCommandBarButtonContextMenuAcceleratorMnemonic(Keys keyData)
        {
            //	Attempt to process the Keys value as a Command.CommandBarButtonContextMenuAcceleratorMnemonic.
            AcceleratorMnemonic acceleratorMnemonic = KeyboardHelper.MapToAcceleratorMnemonic(keyData);
            if (acceleratorMnemonic != AcceleratorMnemonic.None)
            {
                Command command = FindCommandWithCommandBarButtonContextMenuAcceleratorMnemonic(acceleratorMnemonic);
                if (command != null)
                {
                    if (command.On && command.Enabled)
                    {
                        command.PerformShowCommandBarButtonContextMenu();
                        return true;
                    }
                }
            }

            //	We did not process the accelerator.
            return false;
        }

        /// <summary>
        /// Processes a CmdKey for Command.Shortcut, Command.AcceleratorMnemonic and
        /// Command.CommandBarButtonContextMenuAcceleratorMnemonic matches.
        /// </summary>
        /// <param name="keyData">Specifies key codes and modifiers and process.</param>
        /// <returns>true if the Keys value was processed; otherwise, false.</returns>
        public bool ProcessCmdKeyAll(Keys keyData)
        {
            if (ProcessCmdKeyShortcut(keyData))
                return true;
            if (ProcessCmdKeyAcceleratorMnemonic(keyData))
                return true;
            if (ProcessCmdKeyCommandBarButtonContextMenuAcceleratorMnemonic(keyData))
                return true;
            return false;
        }

        /// <summary>
        /// Instructs the command manager to ignore the shortcut until
        /// UnignoreShortcut is called.
        ///
        /// LIMITATION: You cannot currently ignore an AdvancedShortcut
        /// (i.e. one based on Keys instead of Shortcut).
        /// </summary>
        public void IgnoreShortcut(Shortcut shortcut)
        {
            if (maskedShortcuts == null)
                maskedShortcuts = new HashSet();
            bool isNewElement = maskedShortcuts.Add(shortcut);
            Debug.Assert(isNewElement, "Shortcut " + shortcut + " was already masked");
        }

        /// <summary>
        /// Instructs the command manager to respond to the shortcut again.
        /// </summary>
        public void UnignoreShortcut(Shortcut shortcut)
        {
            Trace.Assert(maskedShortcuts != null, "UnignoreShortcut called before IgnoreShortcut");
            if (maskedShortcuts != null)
            {
                bool wasPresent = maskedShortcuts.Remove(shortcut);
                Trace.Assert(wasPresent, "Shortcut " + shortcut + " was not masked");
                if (maskedShortcuts.Count == 0)
                    maskedShortcuts = null;
            }
        }

        public bool ShouldIgnore(Shortcut shortcut)
        {
            if (maskedShortcuts != null && maskedShortcuts.Contains(shortcut))
                return true;
            return false;
        }

        public bool ShouldIgnore(Keys keys)
        {
            if (maskedShortcuts != null && maskedShortcuts.Contains(KeyboardHelper.MapToShortcut(keys)))
                return true;
            return false;
        }

        /// <summary>
        /// Gets the command with the specified command identifier.
        /// </summary>
        /// <param name="commandIdentifier">The command identifier of the command to get.</param>
        /// <returns>The command, or null if a command with the specified command identifier cannot be found.</returns>
        public Command Get(string commandIdentifier)
        {
            CommandInstanceManager commandInstanceManager = (CommandInstanceManager)commandTable[commandIdentifier];
            Command command = (commandInstanceManager == null) ? null : commandInstanceManager.ActiveCommandInstance;
            return command;
        }

        Dictionary<CommandId, string> commandIdToString = new Dictionary<CommandId, string>();

        public Command Get(CommandId commandIdentifier)
        {
            string str;
            if (!commandIdToString.TryGetValue(commandIdentifier, out str))
            {
                str = commandIdentifier.ToString();
                commandIdToString.Add(commandIdentifier, str);
            }
            CommandInstanceManager commandInstanceManager = (CommandInstanceManager)commandTable[str];
            Command command = (commandInstanceManager == null) ? null : commandInstanceManager.ActiveCommandInstance;
            return command;
        }

        /// <summary>
        /// Gets the command with the specified Shortcut.
        /// </summary>
        /// <param name="commandShortcut">The shortcut of the command to get.</param>
        /// <returns>The command, or null if a command with the specified Shortcut cannot be found.</returns>
        public Command FindCommandWithShortcut(Keys commandShortcut)
        {
            RebuildCommandShortcutTable(true);

            // WinLive 293185: If the shortcut involves CTRL - (right) ALT key, ignore it.
            if (KeyboardHelper.IsCtrlRightAlt(commandShortcut))
                return null;

            //	Return the command with the matching shortcut.
            Command command = (Command)commandShortcutTable[commandShortcut];
            if (command != null)
                return command;

            Shortcut shortcut = KeyboardHelper.MapToShortcut(commandShortcut);
            if (shortcut != Shortcut.None && !ShouldIgnore(shortcut))
                return (Command)commandShortcutTable[shortcut];

            return null;
        }

        /// <summary>
        /// Gets the command with the specified AcceleratorMnemonic.
        /// </summary>
        /// <param name="acceleratorMnemonic">The AcceleratorMnemonic of the command to get.</param>
        /// <returns>The command, or null if a command with the specified AcceleratorMnemonic cannot be found.</returns>
        public Command FindCommandWithAcceleratorMnemonic(AcceleratorMnemonic acceleratorMnemonic)
        {
            //	If the AcceleratorMnemonic table has not been built, build it.
            if (acceleratorMnemonicTable == null)
            {
                //	Instantiate the AcceleratorMnemonic table.
                acceleratorMnemonicTable = new Hashtable();

                //	Rebuild the AcceleratorMnemonic table.
                RebuildAcceleratorMnemonicTable();
            }

            //	Return the command with the specified AcceleratorMnemonic.
            return (Command)acceleratorMnemonicTable[acceleratorMnemonic];
        }

        /// <summary>
        /// Gets the command with the specified CommandBarButtonContextMenuAcceleratorMnemonic.
        /// </summary>
        /// <param name="acceleratorMnemonic">The CommandBarButtonContextMenuAcceleratorMnemonic of the command to get.</param>
        /// <returns>The command, or null if a command with the specified CommandBarButtonContextMenuAcceleratorMnemonic cannot be found.</returns>
        public Command FindCommandWithCommandBarButtonContextMenuAcceleratorMnemonic(AcceleratorMnemonic acceleratorMnemonic)
        {
            //	If the CommandBarButtonContextMenuAcceleratorMnemonic table has not been built, build it.
            if (commandBarButtonContextMenuAcceleratorMnemonicTable == null)
            {
                //	Instantiate the CommandBarButtonContextMenuAcceleratorMnemonic table.
                commandBarButtonContextMenuAcceleratorMnemonicTable = new Hashtable();

                //	Rebuild the CommandBarButtonContextMenuAcceleratorMnemonic table.
                RebuildCommandBarButtonContextMenuAcceleratorMnemonicTable();
            }

            //	Return the command with the specified CommandBarButtonContextMenuAcceleratorMnemonic.
            return (Command)commandBarButtonContextMenuAcceleratorMnemonicTable[acceleratorMnemonic];
        }

        /// <summary>
        /// Builds a menu of the specified MenuType from the commands in the command manager.
        /// </summary>
        /// <param name="menuType">Specifies the type of menu to build.</param>
        /// <returns>An array of MenuItem values for the menu.</returns>
        public MenuItem[] BuildMenu(MenuType menuType)
        {
            //	Instantiate a new CommandMenuBuilder so we can build the menu from the set of
            //	commands in this command manager.
            CommandMenuBuilder commandMenuBuilder = new CommandMenuBuilder(menuType);

            //	Enumerate the commands and merge each one into the merge menu.
            foreach (CommandInstanceManager commandInstanceManager in commandTable.Values)
            {
                if (commandInstanceManager.ActiveCommandInstance != null && commandInstanceManager.ActiveCommandInstance.On)
                    commandMenuBuilder.MergeCommand(commandInstanceManager.ActiveCommandInstance);
            }

            //	Return the menu items.
            return commandMenuBuilder.CreateMenuItems();
        }

        /// <summary>
        /// Clears the command manager.
        /// </summary>
        public void Clear()
        {
            commandShortcutTableIsStale = true;
            commandTable.Clear();
            if (commandShortcutTable != null)
                commandShortcutTable.Clear();
            if (acceleratorMnemonicTable != null)
                acceleratorMnemonicTable.Clear();
            if (commandBarButtonContextMenuAcceleratorMnemonicTable != null)
                commandBarButtonContextMenuAcceleratorMnemonicTable.Clear();
            OnChanged(EventArgs.Empty);
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Raises the Changed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        public virtual void OnChanged(EventArgs e)
        {
            if (updateCount > 0)
                pendingChange = true;
            else
            {
                RebuildCommandShortcutTable(false);
                RebuildAcceleratorMnemonicTable();
                RebuildCommandBarButtonContextMenuAcceleratorMnemonicTable();
                if (!suppressEvents && Changed != null)
                    Changed(null, e);
            }
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Adds a command instance.
        /// </summary>
        /// <param name="command">The Command instance to add.</param>
        private void AddCommand(Command command)
        {
            commandShortcutTableIsStale = true;
            CommandInstanceManager commandInstanceManager = (CommandInstanceManager)commandTable[command.Identifier];
            if (commandInstanceManager == null)
                commandTable[command.Identifier] = new CommandInstanceManager(command);
            else
                commandInstanceManager.Add(command);
        }

        /// <summary>
        /// Removes a command instance.
        /// </summary>
        /// <param name="commandList">The Command instance to remove.</param>
        private void RemoveCommand(Command command)
        {
            commandShortcutTableIsStale = true;
            CommandInstanceManager commandInstanceManager = (CommandInstanceManager)commandTable[command.Identifier];
            if (commandInstanceManager != null)
            {
                commandInstanceManager.Remove(command);
                if (commandInstanceManager.IsEmpty)
                    commandTable.Remove(command.Identifier);
            }
        }

        /// <summary>
        /// Rebuilds the Command Shortcut table.
        /// </summary>
        private void RebuildCommandShortcutTable(bool createIfNecessary)
        {
            if (createIfNecessary && commandShortcutTable == null)
                commandShortcutTable = new Hashtable();

            if (commandShortcutTable != null)
            {
                if (!commandShortcutTableIsStale)
                    return;

                commandShortcutTable.Clear();
                foreach (CommandInstanceManager commandInstanceManager in commandTable.Values)
                {
                    Command cmd = commandInstanceManager.ActiveCommandInstance;
                    if (cmd != null)
                    {
                        if (cmd.AdvancedShortcut != Keys.None)
                        {
                            Debug.Assert(!commandShortcutTable.ContainsKey(cmd.AdvancedShortcut), "Shortcut " + cmd.AdvancedShortcut + " is already registered");
                            commandShortcutTable[cmd.AdvancedShortcut] = cmd;
                        }

                        if (cmd.Shortcut != Shortcut.None)
                        {
                            Debug.Assert(!commandShortcutTable.ContainsKey(cmd.Shortcut), "Shortcut " + cmd.Shortcut + " is already registered");
                            commandShortcutTable[cmd.Shortcut] = cmd;
                        }
                    }
                }

                commandShortcutTableIsStale = false;
            }
        }

        /// <summary>
        /// Rebuilds the AcceleratorMnemonicTable table.
        /// </summary>
        private void RebuildAcceleratorMnemonicTable()
        {
            if (acceleratorMnemonicTable != null)
            {
                acceleratorMnemonicTable.Clear();
                foreach (CommandInstanceManager commandInstanceManager in commandTable.Values)
                {
                    if (commandInstanceManager.ActiveCommandInstance != null && commandInstanceManager.ActiveCommandInstance.AcceleratorMnemonic != AcceleratorMnemonic.None)
                        acceleratorMnemonicTable[commandInstanceManager.ActiveCommandInstance.AcceleratorMnemonic] = commandInstanceManager.ActiveCommandInstance;
                }
            }
        }

        /// <summary>
        /// Rebuilds the CommandBarButtonContextMenu AcceleratorMnemonicTable table.
        /// </summary>
        private void RebuildCommandBarButtonContextMenuAcceleratorMnemonicTable()
        {
            if (commandBarButtonContextMenuAcceleratorMnemonicTable != null)
            {
                commandBarButtonContextMenuAcceleratorMnemonicTable.Clear();
                foreach (CommandInstanceManager commandInstanceManager in commandTable.Values)
                {
                    if (commandInstanceManager.ActiveCommandInstance != null && commandInstanceManager.ActiveCommandInstance.CommandBarButtonContextMenuAcceleratorMnemonic != AcceleratorMnemonic.None)
                        commandBarButtonContextMenuAcceleratorMnemonicTable[commandInstanceManager.ActiveCommandInstance.CommandBarButtonContextMenuAcceleratorMnemonic] = commandInstanceManager.ActiveCommandInstance;
                }
            }
        }

        #endregion Private Methods

        private static PropertyKey[] ImageKeys = new[] { PropertyKeys.SmallImage, PropertyKeys.SmallHighContrastImage, PropertyKeys.LargeImage, PropertyKeys.LargeHighContrastImage };
        public void InvalidateAllImages()
        {
            foreach (CommandInstanceManager commandInstanceManager in commandTable.Values)
            {
                if (commandInstanceManager.ActiveCommandInstance != null)
                    commandInstanceManager.ActiveCommandInstance.Invalidate(ImageKeys);
            }
        }

        public void Invalidate(CommandId commandId)
        {
            Command command = Get(commandId);
            if (command != null)
                command.Invalidate();
        }

        public bool IsEnabled(CommandId id)
        {
            Command c = this.Get(id);
            if (c != null)
                return c.Enabled;
            return false;
        }

        public void SetEnabled(CommandId id, bool enabled)
        {
            Command c = this.Get(id);
            if (c != null)
                c.Enabled = enabled;
        }

        public void Execute(CommandId commandId)
        {
            Command command = Get(commandId);
            if (command != null)
                ExecuteCommandAndFireEvents(command);
        }

        private void ExecuteCommandAndFireEvents(Command command)
        {
            FireBeforeExecute(command.CommandId);
            try
            {
                command.PerformExecute();
            }
            finally
            {
                FireAfterExecute(command.CommandId);
            }
        }

        #region Implementation of IUICommandHandler

        public int Execute(uint commandId, CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
        {
            try
            {
                Command command = Get((CommandId)commandId);

                if (verb != CommandExecutionVerb.Execute)
                    return HRESULT.S_OK;

                FireBeforeExecute((CommandId)commandId);
                int result;
                try
                {
                    result = command.PerformExecute(verb, key, currentValue, commandExecutionProperties);
                }
                finally
                {
                    FireAfterExecute((CommandId)commandId);
                }

                return result;
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception thrown when executing " + (CommandId)commandId + ": " + ex);
            }

            return HRESULT.S_OK;
        }

        public event CommandManagerExecuteEventHandler BeforeExecute;
        protected void FireBeforeExecute(CommandId commandId)
        {
            if (BeforeExecute != null)
            {
                BeforeExecute(this, new CommandManagerExecuteEventArgs(commandId));
            }
        }

        public event CommandManagerExecuteEventHandler AfterExecute;
        protected void FireAfterExecute(CommandId commandId)
        {
            if (AfterExecute != null)
            {
                AfterExecute(this, new CommandManagerExecuteEventArgs(commandId));
            }
        }

        public int UpdateProperty(uint commandId, ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            try
            {
                Command command = Get((CommandId)commandId);
                if (command == null)
                {
                    return genericCommandHandler.NullCommandUpdateProperty(commandId, ref key, currentValue, out newValue);
                }

                return command.UpdateProperty(ref key, currentValue, out newValue);
            }
            catch (Exception ex)
            {
                Debug.Fail("Exception throw in CommandManager.UpdateProperty: " + ex + "\r\n\r\nCommand: " + commandId + " Key: " + PropertyKeys.GetName(key));
                throw;
            }
        }

        #endregion

        #region IUICommandHandlerOverride Members

        public int OverrideProperty(uint commandId, ref PropertyKey key, PropVariantRef overrideValue)
        {
            try
            {
                IOverridableCommand overridableCommand = Get((CommandId)commandId) as IOverridableCommand;
                if (overridableCommand == null)
                    return HRESULT.E_INVALIDARG;

                return overridableCommand.OverrideProperty(ref key, overrideValue);
            }
            catch (Exception ex)
            {
                Debug.Fail("Exception throw in CommandManager.OverrideProperty: " + ex + "\r\n\r\nCommand: " + commandId + " Key: " + PropertyKeys.GetName(key));
                throw;
            }
        }

        public int CancelOverride(uint commandId, ref PropertyKey key)
        {
            try
            {
                IOverridableCommand overridableCommand = Get((CommandId)commandId) as IOverridableCommand;
                if (overridableCommand == null)
                    return HRESULT.E_INVALIDARG;

                return overridableCommand.CancelOverride(ref key);
            }
            catch (Exception ex)
            {
                Debug.Fail("Exception throw in CommandManager.OverrideProperty: " + ex + "\r\n\r\nCommand: " + commandId + " Key: " + PropertyKeys.GetName(key));
                throw;
            }
        }

        #endregion
    }

    public delegate void CommandManagerExecuteEventHandler(object sender, CommandManagerExecuteEventArgs eventArgs);

    public class CommandManagerExecuteEventArgs : EventArgs
    {
        public CommandId CommandId { get; set; }

        public CommandManagerExecuteEventArgs(CommandId commandId)
        {
            CommandId = commandId;
        }
    }

    public interface ICommandManagerHost
    {
        CommandManager CommandManager { get; }
    }
}
