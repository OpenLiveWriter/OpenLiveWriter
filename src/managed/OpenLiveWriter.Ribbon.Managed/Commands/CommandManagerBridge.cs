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
        private readonly LazyRibbonCommandManager _ribbonCommandManager;
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
            _ribbonCommandManager = new LazyRibbonCommandManager(this);
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
        /// Registers a single command (or gets existing one).
        /// </summary>
        public BridgedCommand RegisterCommand(CommandId commandId)
        {
            if (_bridgedCommands.TryGetValue(commandId, out var existing))
                return existing;

            var bridgedCommand = new BridgedCommand(commandId, _existingCommandManager);
            _bridgedCommands[commandId] = bridgedCommand;
            _ribbonCommandManager.RegisterCommandInternal(bridgedCommand);
            return bridgedCommand;
        }

        /// <summary>
        /// Gets or creates the bridged command for a command ID.
        /// </summary>
        public BridgedCommand GetOrCreateBridgedCommand(CommandId commandId)
        {
            return RegisterCommand(commandId);
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
    public class BridgedCommand : IGalleryCommand
    {
        private readonly CommandId _commandId;
        private readonly object _existingCommandManager;
        private object _sourceCommand;

        private bool _enabled = true;
        private bool _visible = true;
        private bool _isChecked;
        private string _label;
        private string _tooltip;
        private string _keytip = string.Empty;
        private Image _largeImage;
        private Image _smallImage;
        private List<CommandGalleryItem> _galleryItems = new List<CommandGalleryItem>();
        private int _selectedIndex = -1;

        public CommandId Id => _commandId;
        public string Label => _label ?? _commandId.ToString();
        public string Tooltip => _tooltip ?? Label;
        public string Keytip => _keytip;
        public Image LargeImage => _largeImage;
        public Image SmallImage => _smallImage;
        public IReadOnlyList<CommandGalleryItem> GalleryItems => _galleryItems.AsReadOnly();
        
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnStateChanged();
                }
            }
        }

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
        public event EventHandler ItemsChanged;

        public BridgedCommand(CommandId commandId, object existingCommandManager)
        {
            _commandId = commandId;
            _existingCommandManager = existingCommandManager;

            RefreshFromSource();
        }

        /// <summary>
        /// Gets the source command, refreshing if not yet available.
        /// </summary>
        private object GetSourceCommand()
        {
            if (_sourceCommand != null)
                return _sourceCommand;

            // Try to get source command
            try
            {
                var type = _existingCommandManager.GetType();
                var getMethod = type.GetMethod("Get", new[] { typeof(CommandId) });
                if (getMethod != null)
                {
                    _sourceCommand = getMethod.Invoke(_existingCommandManager, new object[] { _commandId });
                }
            }
            catch
            {
                // If reflection fails, we'll use defaults
            }

            return _sourceCommand;
        }

        /// <summary>
        /// Refreshes command state from the source command.
        /// </summary>
        public void RefreshFromSource()
        {
            var source = GetSourceCommand();
            if (source == null)
            {
                // Use command ID as fallback label
                _label = _commandId.ToString();
                _tooltip = _commandId.ToString();
                return;
            }

            try
            {
                // Read properties from source command using reflection
                var sourceType = source.GetType();

                // Get enabled state
                var enabledProp = sourceType.GetProperty("Enabled");
                if (enabledProp != null)
                {
                    _enabled = (bool)enabledProp.GetValue(source);
                }

                // Get visible state
                var visibleProp = sourceType.GetProperty("On") ?? sourceType.GetProperty("Visible");
                if (visibleProp != null)
                {
                    _visible = (bool)visibleProp.GetValue(source);
                }

                // Get checked/latched state
                var latchedProp = sourceType.GetProperty("Latched") ?? sourceType.GetProperty("Checked");
                if (latchedProp != null)
                {
                    _isChecked = (bool)latchedProp.GetValue(source);
                }

                // Get label
                var labelProp = sourceType.GetProperty("LabelTitle") ?? sourceType.GetProperty("Text");
                if (labelProp != null)
                {
                    _label = (string)labelProp.GetValue(source);
                }

                // Get tooltip
                var tooltipProp = sourceType.GetProperty("TooltipTitle") ?? sourceType.GetProperty("ToolTip");
                if (tooltipProp != null)
                {
                    _tooltip = (string)tooltipProp.GetValue(source);
                }

                // Get large image
                var largeImageProp = sourceType.GetProperty("LargeImage") ?? sourceType.GetProperty("CommandBarButtonBitmapLarge");
                if (largeImageProp != null)
                {
                    _largeImage = largeImageProp.GetValue(source) as Image;
                }

                // Get small image
                var smallImageProp = sourceType.GetProperty("SmallImage") ?? sourceType.GetProperty("CommandBarButtonBitmapSmall");
                if (smallImageProp != null)
                {
                    _smallImage = smallImageProp.GetValue(source) as Image;
                }

                // Get gallery items (for SelectGalleryCommand derived classes)
                var itemsChanged = LoadGalleryItems(source, sourceType);
                if (itemsChanged)
                {
                    ItemsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch
            {
                // If any reflection fails, continue with existing values
            }

            OnStateChanged();
        }

        private bool LoadGalleryItems(object source, Type sourceType)
        {
            var oldCount = _galleryItems.Count;
            
            try
            {
                // Check if source has Items property (for gallery commands)
                var itemsProp = sourceType.GetProperty("Items");
                if (itemsProp != null)
                {
                    var items = itemsProp.GetValue(source) as System.Collections.IList;
                    if (items != null)
                    {
                        _galleryItems.Clear();
                        foreach (var item in items)
                        {
                            if (item == null) continue;
                            
                            var itemType = item.GetType();
                            var labelProp = itemType.GetProperty("Label");
                            var imageProp = itemType.GetProperty("Image");
                            var cookieProp = itemType.GetProperty("Cookie");
                            
                            var galleryItem = new CommandGalleryItem
                            {
                                Label = labelProp?.GetValue(item) as string ?? item.ToString(),
                                Image = imageProp?.GetValue(item) as Image,
                                Tag = cookieProp?.GetValue(item)
                            };
                            _galleryItems.Add(galleryItem);
                        }
                    }
                }

                // Get selected index
                var selectedIndexProp = sourceType.GetProperty("SelectedIndex") ?? sourceType.GetProperty("selectedIndex");
                if (selectedIndexProp != null)
                {
                    var value = selectedIndexProp.GetValue(source);
                    if (value is int idx)
                    {
                        _selectedIndex = idx;
                    }
                }
            }
            catch
            {
                // If gallery loading fails, continue
            }

            return _galleryItems.Count != oldCount || oldCount > 0;
        }

        public void PerformExecute()
        {
            System.Diagnostics.Debug.WriteLine($"BridgedCommand.PerformExecute for {_commandId}, Enabled={Enabled}");
            
            // Refresh to ensure we have the latest source command and state
            var source = GetSourceCommand();
            
            // For semantic HTML commands, they should generally be enabled
            // Check the source command's enabled state directly
            bool sourceEnabled = true;
            if (source != null)
            {
                try
                {
                    var sourceType = source.GetType();
                    var enabledProp = sourceType.GetProperty("Enabled");
                    var onProp = sourceType.GetProperty("On");
                    if (enabledProp != null)
                    {
                        sourceEnabled = (bool)enabledProp.GetValue(source);
                    }
                    bool sourceOn = onProp != null ? (bool)onProp.GetValue(source) : true;
                    System.Diagnostics.Debug.WriteLine($"  Source: Type={sourceType.Name}, Enabled={sourceEnabled}, On={sourceOn}");
                }
                catch { }
            }

            if (!sourceEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"  Skipping execution - source command disabled");
                return;
            }

            Execute?.Invoke(this, EventArgs.Empty);

            // Execute the source command
            if (source != null)
            {
                try
                {
                    var sourceType = source.GetType();
                    var executeMethod = sourceType.GetMethod("PerformExecute", Type.EmptyTypes);

                    if (executeMethod != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Calling PerformExecute on source");
                        executeMethod.Invoke(source, null);
                        System.Diagnostics.Debug.WriteLine($"  PerformExecute completed");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  No PerformExecute method found on source");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Command execution failed for {_commandId}: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Inner exception: {ex.InnerException.Message}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"  No source command found");
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

    /// <summary>
    /// A RibbonCommandManager that lazily creates commands via the bridge.
    /// </summary>
    internal class LazyRibbonCommandManager : RibbonCommandManager
    {
        private readonly CommandManagerBridge _bridge;

        public LazyRibbonCommandManager(CommandManagerBridge bridge)
        {
            _bridge = bridge;
        }

        /// <summary>
        /// Gets a command by ID, auto-creating it if needed.
        /// </summary>
        public override IRibbonCommand GetCommand(CommandId id)
        {
            var command = base.GetCommand(id);
            if (command == null && id != CommandId.None)
            {
                // Auto-create the bridged command
                command = _bridge.GetOrCreateBridgedCommand(id);
            }
            return command;
        }

        /// <summary>
        /// Internal registration that doesn't trigger lazy creation.
        /// </summary>
        internal void RegisterCommandInternal(IRibbonCommand command)
        {
            base.RegisterCommand(command);
        }
    }
}
