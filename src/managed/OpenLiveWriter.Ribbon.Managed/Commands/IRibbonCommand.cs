// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Ribbon.Managed.Commands
{
    /// <summary>
    /// Represents an item in a gallery or combobox.
    /// </summary>
    public class CommandGalleryItem
    {
        public string Label { get; set; }
        public Image Image { get; set; }
        public object Tag { get; set; }

        public CommandGalleryItem() { }
        public CommandGalleryItem(string label, Image image = null, object tag = null)
        {
            Label = label;
            Image = image;
            Tag = tag;
        }
    }

    /// <summary>
    /// Interface for commands that provide gallery items (for galleries and comboboxes).
    /// </summary>
    public interface IGalleryCommand : IRibbonCommand
    {
        /// <summary>
        /// Gets the gallery items.
        /// </summary>
        IReadOnlyList<CommandGalleryItem> GalleryItems { get; }

        /// <summary>
        /// Gets or sets the selected index.
        /// </summary>
        int SelectedIndex { get; set; }

        /// <summary>
        /// Occurs when gallery items have changed.
        /// </summary>
        event EventHandler ItemsChanged;
    }

    /// <summary>
    /// Interface for ribbon commands that can be bound to ribbon controls.
    /// </summary>
    public interface IRibbonCommand
    {
        /// <summary>
        /// Gets the unique identifier for this command.
        /// </summary>
        CommandId Id { get; }

        /// <summary>
        /// Gets the display label for this command.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the tooltip text for this command.
        /// </summary>
        string Tooltip { get; }

        /// <summary>
        /// Gets the keytip for keyboard navigation.
        /// </summary>
        string Keytip { get; }

        /// <summary>
        /// Gets the large image (32x32) for this command.
        /// </summary>
        Image LargeImage { get; }

        /// <summary>
        /// Gets the small image (16x16) for this command.
        /// </summary>
        Image SmallImage { get; }

        /// <summary>
        /// Gets or sets whether this command is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets whether this command is visible.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets whether this command is in a checked/pressed state (for toggle buttons).
        /// </summary>
        bool Checked { get; set; }

        /// <summary>
        /// Occurs when the command should be executed.
        /// </summary>
        event EventHandler Execute;

        /// <summary>
        /// Occurs when command state (enabled, visible, checked) has changed.
        /// </summary>
        event EventHandler StateChanged;

        /// <summary>
        /// Performs the command action.
        /// </summary>
        void PerformExecute();

        /// <summary>
        /// Notifies that the command state needs to be refreshed.
        /// </summary>
        void Invalidate();
    }

    /// <summary>
    /// Base implementation of IRibbonCommand.
    /// </summary>
    public class RibbonCommand : IRibbonCommand
    {
        private bool _enabled = true;
        private bool _visible = true;
        private bool _checked;

        public RibbonCommand(CommandId id)
        {
            Id = id;
        }

        public RibbonCommand(CommandId id, string label, string tooltip = null)
        {
            Id = id;
            Label = label;
            Tooltip = tooltip ?? label;
        }

        public CommandId Id { get; }
        public string Label { get; set; }
        public string Tooltip { get; set; }
        public string Keytip { get; set; }
        public Image LargeImage { get; set; }
        public Image SmallImage { get; set; }

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
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    OnStateChanged();
                }
            }
        }

        public event EventHandler Execute;
        public event EventHandler StateChanged;

        public void PerformExecute()
        {
            if (Enabled)
            {
                Execute?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Invalidate()
        {
            OnStateChanged();
        }

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
