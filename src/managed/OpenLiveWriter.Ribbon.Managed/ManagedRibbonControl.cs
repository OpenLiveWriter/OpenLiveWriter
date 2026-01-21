// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Configuration;
using OpenLiveWriter.Ribbon.Managed.Controls;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed
{
    /// <summary>
    /// Main entry point for the managed ribbon control.
    /// This class replaces the native C++ ribbon and provides a fully managed implementation.
    /// </summary>
    public class ManagedRibbonControl : UserControl
    {
        private const int DEFAULT_HEIGHT = 120;
        private const int QAT_HEIGHT = 24;

        private RibbonPanel _ribbonPanel;
        private QuickAccessToolbar _quickAccessToolbar;
        private CommandManagerBridge _commandBridge;
        private RibbonConfiguration _configuration;
        private RibbonApplicationMode _currentMode = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.WithPlugins;

        private bool _isInitialized;

        /// <summary>
        /// Gets the ribbon panel.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonPanel RibbonPanel => _ribbonPanel;

        /// <summary>
        /// Gets the quick access toolbar.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public QuickAccessToolbar QuickAccessToolbar => _quickAccessToolbar;

        /// <summary>
        /// Gets the command bridge.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CommandManagerBridge CommandBridge => _commandBridge;

        /// <summary>
        /// Gets or sets the current application mode.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonApplicationMode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    if (_ribbonPanel != null)
                    {
                        _ribbonPanel.CurrentMode = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the height of the ribbon.
        /// </summary>
        public int RibbonHeight => _ribbonPanel?.RibbonHeight ?? DEFAULT_HEIGHT;

        /// <summary>
        /// Occurs when the application menu is opened.
        /// </summary>
        public event EventHandler ApplicationMenuOpened;

        /// <summary>
        /// Occurs when a contextual tab group visibility changes.
        /// </summary>
        public event EventHandler<ContextualTabEventArgs> ContextualTabVisibilityChanged;

        public ManagedRibbonControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Dock = DockStyle.Top;
            Height = DEFAULT_HEIGHT;
            BackColor = RibbonColors.Current.RibbonBackground;
        }

        /// <summary>
        /// Initializes the ribbon with an existing command manager.
        /// </summary>
        /// <param name="existingCommandManager">The existing CommandManager instance.</param>
        public void Initialize(object existingCommandManager)
        {
            if (_isInitialized)
                return;

            // Create command bridge
            _commandBridge = new CommandManagerBridge(existingCommandManager);

            // Create and configure ribbon panel
            _ribbonPanel = new RibbonPanel
            {
                Dock = DockStyle.Fill,
                CommandManager = _commandBridge.RibbonCommandManager
            };
            _ribbonPanel.ApplicationMenuClicked += (s, e) => ApplicationMenuOpened?.Invoke(this, EventArgs.Empty);

            // Create quick access toolbar
            _quickAccessToolbar = new QuickAccessToolbar
            {
                Location = new Point(60, 0),
                CommandManager = _commandBridge.RibbonCommandManager
            };

            // Add controls
            Controls.Add(_ribbonPanel);
            Controls.Add(_quickAccessToolbar);
            _quickAccessToolbar.BringToFront();

            _isInitialized = true;
        }

        /// <summary>
        /// Builds the ribbon from the default configuration.
        /// </summary>
        public void BuildDefaultConfiguration()
        {
            BuildFromConfiguration(DefaultRibbonConfiguration.Create());
        }

        /// <summary>
        /// Builds the ribbon from a configuration.
        /// </summary>
        public void BuildFromConfiguration(RibbonConfiguration config)
        {
            _configuration = config ?? throw new ArgumentNullException(nameof(config));

            // Register all commands
            RegisterAllCommands(config);

            // Build ribbon structure
            _ribbonPanel.BuildFromConfiguration(config);

            // Configure QAT
            if (config.QuickAccessToolbar?.DefaultCommands != null)
            {
                _quickAccessToolbar.SetCommands(config.QuickAccessToolbar.DefaultCommands);
            }

            // Set initial mode
            _ribbonPanel.CurrentMode = _currentMode;

            // Update height
            Height = _ribbonPanel.RibbonHeight;
        }

        private void RegisterAllCommands(RibbonConfiguration config)
        {
            var commandIds = new HashSet<CommandId>();

            // Collect all command IDs from configuration
            CollectCommandIds(config.Tabs, commandIds);

            foreach (var ctg in config.ContextualTabGroups)
            {
                commandIds.Add(ctg.CommandId);
                CollectCommandIds(ctg.Tabs, commandIds);
            }

            // Register with bridge
            _commandBridge.RegisterCommands(commandIds);
        }

        private void CollectCommandIds(IEnumerable<TabConfig> tabs, HashSet<CommandId> commandIds)
        {
            foreach (var tab in tabs)
            {
                commandIds.Add(tab.CommandId);

                foreach (var group in tab.Groups)
                {
                    if (group.CommandId != CommandId.None)
                        commandIds.Add(group.CommandId);

                    foreach (var control in group.Controls)
                    {
                        if (control.CommandId != CommandId.None)
                            commandIds.Add(control.CommandId);
                    }
                }
            }
        }

        #region Application Mode Management

        /// <summary>
        /// Sets the application modes.
        /// </summary>
        public void SetModes(int modeFlags)
        {
            var mode = (RibbonApplicationMode)modeFlags;
            CurrentMode = mode;
        }

        /// <summary>
        /// Enables or disables preview mode.
        /// </summary>
        public void SetPreviewMode(bool enabled)
        {
            if (enabled)
            {
                _currentMode |= RibbonApplicationMode.Preview;
                _currentMode &= ~RibbonApplicationMode.Normal;
            }
            else
            {
                _currentMode &= ~RibbonApplicationMode.Preview;
                _currentMode |= RibbonApplicationMode.Normal;
            }

            if (_ribbonPanel != null)
                _ribbonPanel.CurrentMode = _currentMode;
        }

        /// <summary>
        /// Sets the text direction mode.
        /// </summary>
        public void SetTextDirection(bool isRtl)
        {
            if (isRtl)
            {
                _currentMode &= ~RibbonApplicationMode.LTR;
                _currentMode |= RibbonApplicationMode.RTL;
            }
            else
            {
                _currentMode &= ~RibbonApplicationMode.RTL;
                _currentMode |= RibbonApplicationMode.LTR;
            }

            if (_ribbonPanel != null)
                _ribbonPanel.CurrentMode = _currentMode;
        }

        /// <summary>
        /// Sets whether plugins are available.
        /// </summary>
        public void SetPluginsAvailable(bool hasPlugins)
        {
            if (hasPlugins)
            {
                _currentMode &= ~RibbonApplicationMode.WithoutPlugins;
                _currentMode |= RibbonApplicationMode.WithPlugins;
            }
            else
            {
                _currentMode &= ~RibbonApplicationMode.WithPlugins;
                _currentMode |= RibbonApplicationMode.WithoutPlugins;
            }

            if (_ribbonPanel != null)
                _ribbonPanel.CurrentMode = _currentMode;
        }

        /// <summary>
        /// Sets debug mode visibility.
        /// </summary>
        public void SetDebugMode(bool enabled)
        {
            if (enabled)
            {
                _currentMode |= RibbonApplicationMode.Debug;
            }
            else
            {
                _currentMode &= ~RibbonApplicationMode.Debug;
            }

            if (_ribbonPanel != null)
                _ribbonPanel.CurrentMode = _currentMode;
        }

        #endregion

        #region Contextual Tab Management

        /// <summary>
        /// Shows a contextual tab group.
        /// </summary>
        public void ShowContextualTabGroup(RibbonContextualTabGroup group)
        {
            _ribbonPanel?.ShowContextualTabGroup(group);
            ContextualTabVisibilityChanged?.Invoke(this, new ContextualTabEventArgs(group, true));
        }

        /// <summary>
        /// Hides a contextual tab group.
        /// </summary>
        public void HideContextualTabGroup(RibbonContextualTabGroup group)
        {
            _ribbonPanel?.HideContextualTabGroup(group);
            ContextualTabVisibilityChanged?.Invoke(this, new ContextualTabEventArgs(group, false));
        }

        /// <summary>
        /// Shows or hides contextual tabs based on content type.
        /// </summary>
        public void UpdateContextualTabs(string contentType)
        {
            // Hide all first
            HideContextualTabGroup(RibbonContextualTabGroup.ImageTools);
            HideContextualTabGroup(RibbonContextualTabGroup.VideoTools);
            HideContextualTabGroup(RibbonContextualTabGroup.TableTools);
            HideContextualTabGroup(RibbonContextualTabGroup.MapTools);
            HideContextualTabGroup(RibbonContextualTabGroup.TagTools);

            // Show based on content type
            switch (contentType?.ToLowerInvariant())
            {
                case "image":
                case "picture":
                    ShowContextualTabGroup(RibbonContextualTabGroup.ImageTools);
                    break;

                case "video":
                    ShowContextualTabGroup(RibbonContextualTabGroup.VideoTools);
                    break;

                case "table":
                    ShowContextualTabGroup(RibbonContextualTabGroup.TableTools);
                    break;

                case "map":
                    ShowContextualTabGroup(RibbonContextualTabGroup.MapTools);
                    break;

                case "tag":
                    ShowContextualTabGroup(RibbonContextualTabGroup.TagTools);
                    break;
            }
        }

        #endregion

        #region Command Invalidation

        /// <summary>
        /// Invalidates a specific command.
        /// </summary>
        public void InvalidateCommand(CommandId commandId)
        {
            _commandBridge?.Invalidate(commandId);
        }

        /// <summary>
        /// Invalidates all commands.
        /// </summary>
        public void InvalidateAllCommands()
        {
            _commandBridge?.InvalidateAll();
        }

        #endregion

        #region Settings Persistence

        /// <summary>
        /// Loads ribbon settings from a stream.
        /// </summary>
        public void LoadSettings(Stream stream)
        {
            _quickAccessToolbar?.LoadSettings(stream);
        }

        /// <summary>
        /// Saves ribbon settings to a stream.
        /// </summary>
        public void SaveSettings(Stream stream)
        {
            _quickAccessToolbar?.SaveSettings(stream);
        }

        #endregion

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_quickAccessToolbar != null)
            {
                _quickAccessToolbar.Location = new Point(60, 2);
            }
        }
    }

    /// <summary>
    /// Event args for contextual tab visibility changes.
    /// </summary>
    public class ContextualTabEventArgs : EventArgs
    {
        public RibbonContextualTabGroup TabGroup { get; }
        public bool IsVisible { get; }

        public ContextualTabEventArgs(RibbonContextualTabGroup tabGroup, bool isVisible)
        {
            TabGroup = tabGroup;
            IsVisible = isVisible;
        }
    }
}
