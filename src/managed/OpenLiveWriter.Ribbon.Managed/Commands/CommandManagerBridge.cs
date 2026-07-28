// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using OpenLiveWriter.ApplicationFramework;
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
        private Color? _selectedColor;

        public CommandId Id => _commandId;
        public string Label => _label ?? _commandId.ToString();
        public string Tooltip => _tooltip ?? Label;
        public string Keytip => _keytip;
        public Image LargeImage => _largeImage;
        public Image SmallImage => _smallImage;
        
        /// <summary>
        /// Gets the gallery items. If items are empty, tries to refresh from source command.
        /// </summary>
        public IReadOnlyList<CommandGalleryItem> GalleryItems
        {
            get
            {
                // If we don't have items yet, try to refresh from source
                // This handles the case where the source command wasn't available at construction time
                if (_galleryItems.Count == 0 && _sourceCommand == null)
                {
                    RefreshFromSource();
                }
                return _galleryItems.AsReadOnly();
            }
        }
        
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

        /// <summary>
        /// Gets or sets the color passed to the source command on the next execution
        /// (used by color picker commands such as FontColorPicker). Cleared after use.
        /// </summary>
        public Color? SelectedColor
        {
            get => _selectedColor;
            set => _selectedColor = value;
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
        /// Gets the source command, attempting to find it if not yet available.
        /// Will retry looking for the command if it wasn't found previously.
        /// </summary>
        private object GetSourceCommand()
        {
            // If we already have the source command, return it
            if (_sourceCommand != null)
                return _sourceCommand;

            // Try to get source command (this may be called multiple times if source wasn't available initially)
            try
            {
                var type = _existingCommandManager.GetType();
                var getMethod = type.GetMethod("Get", new[] { typeof(CommandId) });
                if (getMethod != null)
                {
                    var source = getMethod.Invoke(_existingCommandManager, new object[] { _commandId });
                    
                    // If we found the source command
                    if (source != null)
                    {
                        _sourceCommand = source;
                        
                        // Subscribe to source command's StateChanged event
                        SubscribeToSourceStateChanged(_sourceCommand);
                    }
                }
            }
            catch
            {
                // If source command lookup fails, continue with null
            }

            return _sourceCommand;
        }

        /// <summary>
        /// Subscribes to the source command's StateChanged event using reflection.
        /// </summary>
        private void SubscribeToSourceStateChanged(object source)
        {
            try
            {
                var sourceType = source.GetType();
                var stateChangedEvent = sourceType.GetEvent("StateChanged");
                if (stateChangedEvent != null)
                {
                    var handler = new EventHandler(OnSourceStateChanged);
                    stateChangedEvent.AddEventHandler(source, handler);
                }
            }
            catch
            {
                // If event subscription fails, state changes won't be tracked
            }
        }

        /// <summary>
        /// Handler for when the source command's state changes.
        /// </summary>
        private void OnSourceStateChanged(object sender, EventArgs e)
        {
            RefreshFromSource();
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

                // Get large image (32x32)
                // Try LargeImage first (the standard ribbon property)
                var largeImageProp = sourceType.GetProperty("LargeImage");
                if (largeImageProp != null)
                {
                    _largeImage = ExtractBitmapFromProperty(largeImageProp, source);
                }

                // Get small image (16x16)
                // Try SmallImage first
                var smallImageProp = sourceType.GetProperty("SmallImage");
                if (smallImageProp != null)
                {
                    _smallImage = ExtractBitmapFromProperty(smallImageProp, source);
                }

                // If SmallImage is null, try CommandBarButtonBitmapEnabled (legacy command bar property)
                if (_smallImage == null)
                {
                    var cmdBarBitmapProp = sourceType.GetProperty("CommandBarButtonBitmapEnabled");
                    if (cmdBarBitmapProp != null)
                    {
                        _smallImage = ExtractBitmapFromProperty(cmdBarBitmapProp, source);
                    }
                }

                // If LargeImage is null, try CommandBarButtonBitmapEnabled as fallback
                if (_largeImage == null)
                {
                    var cmdBarBitmapProp = sourceType.GetProperty("CommandBarButtonBitmapEnabled");
                    if (cmdBarBitmapProp != null)
                    {
                        _largeImage = ExtractBitmapFromProperty(cmdBarBitmapProp, source);
                    }
                }

                // If LargeImage is null but SmallImage exists, scale up the small image
                if (_largeImage == null && _smallImage != null)
                {
                    _largeImage = ScaleImage(_smallImage, 32, 32);
                }
                else if (_smallImage == null && _largeImage != null)
                {
                    _smallImage = ScaleImage(_largeImage, 16, 16);
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
                // Try to find Items property in the type hierarchy
                var itemsProp = sourceType.GetProperty("Items");
                if (itemsProp == null)
                {
                    // Try to find it in base types
                    var baseType = sourceType.BaseType;
                    while (baseType != null && itemsProp == null)
                    {
                        itemsProp = baseType.GetProperty("Items");
                        baseType = baseType.BaseType;
                    }
                }
                
                if (itemsProp != null)
                {
                    var itemsValue = itemsProp.GetValue(source);
                    
                    var items = itemsValue as System.Collections.IList;
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
                            
                            var label = labelProp?.GetValue(item) as string ?? item.ToString();
                            var image = imageProp?.GetValue(item) as Image;
                            var cookie = cookieProp?.GetValue(item);
                            
                            var galleryItem = new CommandGalleryItem
                            {
                                Label = label,
                                Image = image,
                                Tag = cookie
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
                // If gallery item loading fails, continue with existing items
            }

            return _galleryItems.Count != oldCount || oldCount > 0;
        }

        public void PerformExecute()
        {
            // Refresh to ensure we have the latest source command and state
            var source = GetSourceCommand();

            // If no source command exists, we can't execute - fail early
            if (source == null)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] BridgedCommand.PerformExecute: {_commandId} has no source command");
                return;
            }

            // Check the source command's enabled and on state directly
            // The underlying Command.PerformExecute() requires both On && Enabled to be true
            bool sourceEnabled = true;
            bool sourceOn = true;
            try
            {
                var sourceType = source.GetType();
                var enabledProp = sourceType.GetProperty("Enabled");
                var onProp = sourceType.GetProperty("On");
                if (enabledProp != null)
                {
                    sourceEnabled = (bool)enabledProp.GetValue(source);
                }
                if (onProp != null)
                {
                    sourceOn = (bool)onProp.GetValue(source);
                }
            }
            catch { }

            if (!sourceEnabled || !sourceOn)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] BridgedCommand.PerformExecute: {_commandId} gated (enabled={sourceEnabled}, on={sourceOn})");
                return;
            }

            Execute?.Invoke(this, EventArgs.Empty);

            // Execute the source command
            try
            {
                // Cast to Command to use direct method calls instead of reflection
                if (source is Command command)
                {
                    // Check if this is a gallery command (has SelectedIndex property)
                    var sourceType = source.GetType();
                    var selectedIndexProp = sourceType.GetProperty("SelectedIndex");
                    bool isGalleryCommand = selectedIndexProp != null && selectedIndexProp.CanWrite;

                    if (isGalleryCommand)
                    {
                        // Set the selected index on the source command
                        selectedIndexProp.SetValue(source, _selectedIndex);

                        // Gallery commands use PerformExecuteWithArgs with ExecuteEventHandlerArgs
                        var args = new ExecuteEventHandlerArgs(_commandId.ToString(), _selectedIndex);
                        command.PerformExecuteWithArgs(args);
                        return;
                    }
                    else
                    {
                        // Color picker command - pass the selected color as an execution arg
                        if (_selectedColor.HasValue)
                        {
                            var colorArgs = new ExecuteEventHandlerArgs();
                            colorArgs.Add("SelectedColor", _selectedColor.Value);
                            _selectedColor = null;
                            command.PerformExecuteWithArgs(colorArgs);
                            return;
                        }

                        // Non-gallery command - use regular PerformExecute
                        command.PerformExecute();
                        return;
                    }
                }
                
                // Fallback for non-Command types (shouldn't happen but just in case)
                var sourceType2 = source.GetType();
                var executeMethod = sourceType2.GetMethod("PerformExecute", Type.EmptyTypes);
                executeMethod?.Invoke(source, null);
            }
            catch (Exception ex)
            {
                // If command execution fails, swallow the exception
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] BridgedCommand.PerformExecute: {_commandId} threw: {ex}");
            }
        }

        public void Invalidate()
        {
            RefreshFromSource();
        }

        /// <summary>
        /// Forces loading of gallery items by calling LoadItems() on the source command.
        /// This should be called before showing a dropdown to ensure items are populated.
        /// </summary>
        public void ForceLoadGalleryItems()
        {
            var source = GetSourceCommand();
            if (source == null) return;
            
            var sourceType = source.GetType();
            var loadItemsMethod = sourceType.GetMethod("LoadItems");
            if (loadItemsMethod != null)
            {
                try
                {
                    loadItemsMethod.Invoke(source, null);
                    
                    // Reload gallery items after LoadItems() call
                    LoadGalleryItems(source, sourceType);
                    ItemsChanged?.Invoke(this, EventArgs.Empty);
                }
                catch
                {
                    // If forced gallery item loading fails, continue with existing items
                }
            }
        }

        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Extracts a Bitmap from a property that may return either LazyLoader&lt;Bitmap&gt; or Bitmap directly.
        /// Also filters out placeholder images (MissingLarge/MissingSmall).
        /// </summary>
        private static Image ExtractBitmapFromProperty(PropertyInfo property, object source)
        {
            try
            {
                var propertyValue = property.GetValue(source);
                if (propertyValue == null)
                    return null;

                // Check if it's already a Bitmap/Image
                if (propertyValue is Image directImage)
                {
                    // Check if it's a placeholder image - skip those
                    if (IsPlaceholderImage(directImage))
                        return null;
                    return directImage;
                }

                // Check if it's a LazyLoader<Bitmap>
                var propertyType = propertyValue.GetType();
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().Name == "LazyLoader`1")
                {
                    // Get the Value property from LazyLoader
                    var valueProperty = propertyType.GetProperty("Value");
                    if (valueProperty != null)
                    {
                        var bitmap = valueProperty.GetValue(propertyValue) as Image;
                        // Check if it's a placeholder image - skip those
                        if (bitmap != null && IsPlaceholderImage(bitmap))
                            return null;
                        return bitmap;
                    }
                }

                // Try direct cast as fallback
                return propertyValue as Image;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if an image is a placeholder image (MissingLarge or MissingSmall).
        /// Only filters if we can confirm it's the actual placeholder by reference comparison.
        /// </summary>
        private static bool IsPlaceholderImage(Image image)
        {
            if (image == null)
                return true;

            // Try to get the MissingLarge and MissingSmall images for comparison
            try
            {
                var commandResourceLoaderType = Type.GetType("OpenLiveWriter.ApplicationFramework.CommandResourceLoader, OpenLiveWriter.ApplicationFramework");
                if (commandResourceLoaderType != null)
                {
                    var missingLargeProp = commandResourceLoaderType.GetProperty("MissingLarge", BindingFlags.Static | BindingFlags.Public);
                    var missingSmallProp = commandResourceLoaderType.GetProperty("MissingSmall", BindingFlags.Static | BindingFlags.Public);

                    if (missingLargeProp != null)
                    {
                        var missingLarge = missingLargeProp.GetValue(null) as Image;
                        // Only filter if it's the exact same reference (most reliable check)
                        if (missingLarge != null && ReferenceEquals(image, missingLarge))
                            return true;
                    }

                    if (missingSmallProp != null)
                    {
                        var missingSmall = missingSmallProp.GetValue(null) as Image;
                        // Only filter if it's the exact same reference (most reliable check)
                        if (missingSmall != null && ReferenceEquals(image, missingSmall))
                            return true;
                    }
                }
            }
            catch
            {
                // If we can't check, assume it's not a placeholder to avoid false positives
            }

            return false;
        }

        /// <summary>
        /// Scales an image to the specified size with high quality interpolation.
        /// </summary>
        private static Image ScaleImage(Image source, int width, int height)
        {
            if (source == null) return null;
            if (source.Width == width && source.Height == height) return source;

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
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
