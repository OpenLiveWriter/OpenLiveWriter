// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Ribbon combobox control for selection lists (e.g., font family, font size).
    /// </summary>
    public class RibbonComboBox : RibbonControlBase
    {
        private readonly ComboBox _innerComboBox;
        private readonly Label _labelControl;
        private string _label;
        private bool _isEditable = true;
        private bool _isAutoCompleteEnabled = true;
        private bool _isLoading; // Flag to prevent command execution during initialization
        private bool _showLabel = true;

        /// <summary>
        /// Gets or sets the label displayed above the combobox.
        /// </summary>
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                if (_labelControl != null)
                {
                    _labelControl.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the combobox is editable.
        /// </summary>
        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                _isEditable = value;
                _innerComboBox.DropDownStyle = value ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
            }
        }

        /// <summary>
        /// Gets or sets whether auto-complete is enabled.
        /// </summary>
        public bool IsAutoCompleteEnabled
        {
            get => _isAutoCompleteEnabled;
            set
            {
                _isAutoCompleteEnabled = value;
                _innerComboBox.AutoCompleteMode = value ? AutoCompleteMode.SuggestAppend : AutoCompleteMode.None;
                _innerComboBox.AutoCompleteSource = value ? AutoCompleteSource.ListItems : AutoCompleteSource.None;
            }
        }

        /// <summary>
        /// Gets or sets whether the label is shown above the combobox.
        /// When false, the combobox is positioned at the top without a label.
        /// </summary>
        public bool ShowLabel
        {
            get => _showLabel;
            set
            {
                _showLabel = value;
                if (_labelControl != null)
                {
                    _labelControl.Visible = value;
                    _innerComboBox.Location = value ? new Point(0, 16) : new Point(0, 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected index.
        /// </summary>
        public int SelectedIndex
        {
            get => _innerComboBox.SelectedIndex;
            set => _innerComboBox.SelectedIndex = value;
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public object SelectedItem
        {
            get => _innerComboBox.SelectedItem;
            set => _innerComboBox.SelectedItem = value;
        }

        /// <summary>
        /// Gets or sets the selected text.
        /// </summary>
        public string SelectedText
        {
            get => _innerComboBox.Text;
            set => _innerComboBox.Text = value;
        }

        /// <summary>
        /// Gets the items collection.
        /// </summary>
        public ComboBox.ObjectCollection Items => _innerComboBox.Items;

        /// <summary>
        /// Occurs when the selected index changes.
        /// </summary>
        public event EventHandler SelectedIndexChanged;

    /// <summary>
    /// Occurs when the text changes.
    /// </summary>
    public new event EventHandler TextChanged;

        public RibbonComboBox()
        {
            Size = new Size(120, 44);

            // Label
            _labelControl = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(Width, 14),
                Font = new Font(SystemFonts.MenuFont.FontFamily, 7.5f),
                ForeColor = RibbonColors.Current.GroupLabelText,
                TextAlign = ContentAlignment.BottomLeft
            };
            Controls.Add(_labelControl);

            // ComboBox - use owner-draw for proper hover colors
            _innerComboBox = new ComboBox
            {
                Location = new Point(0, 16),
                Size = new Size(Width, 23),
                Font = SystemFonts.MenuFont,
                FlatStyle = FlatStyle.System,
                DropDownStyle = ComboBoxStyle.DropDown,
                DrawMode = DrawMode.OwnerDrawFixed,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            _innerComboBox.DrawItem += InnerComboBox_DrawItem;

            _innerComboBox.SelectedIndexChanged += (s, e) =>
            {
                SelectedIndexChanged?.Invoke(this, e);
                // Don't execute command during loading/initialization
                if (!_isLoading)
                {
                    var selectedIndex = _innerComboBox.SelectedIndex;
                    Action execute = () =>
                    {
                        // Push the new selection into the gallery command before executing,
                        // mirroring how RibbonGallery.ExecuteGalleryItem routes gallery picks.
                        if (CommandManager?.GetCommand(CommandId) is IGalleryCommand galleryCommand)
                        {
                            galleryCommand.SelectedIndex = selectedIndex;
                        }
                        ExecuteCommand();
                    };
                    // Defer execution until the ComboBox has finished processing its own
                    // selection notification. Refreshing the combo items or executing the
                    // command reentrantly inside the native notification can crash comctl32.
                    if (IsHandleCreated)
                    {
                        BeginInvoke(execute);
                    }
                    else
                    {
                        execute();
                    }
                }
            };

            _innerComboBox.TextChanged += (s, e) => TextChanged?.Invoke(this, e);

            Controls.Add(_innerComboBox);
        }

        /// <summary>
        /// Custom drawing for combobox items to ensure readable text on hover.
        /// </summary>
        private void InnerComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var isSelected = (e.State & DrawItemState.Selected) != 0;
            var bgColor = isSelected ? RibbonColors.Current.ButtonBackgroundHover : Color.White;
            var textColor = RibbonColors.Current.ButtonText;

            using (var brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            var text = _innerComboBox.Items[e.Index]?.ToString() ?? string.Empty;
            TextRenderer.DrawText(e.Graphics, text, e.Font ?? SystemFonts.MenuFont,
                e.Bounds, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        /// <summary>
        /// Called when the command is updated.
        /// </summary>
        protected override void UpdateFromCommand()
        {
            base.UpdateFromCommand();
            
            if (_innerComboBox != null)
            {
                _innerComboBox.Enabled = CommandEnabled;
            }
            
            LoadItemsFromCommand();
        }

        private void LoadItemsFromCommand()
        {
            var command = CommandManager?.GetCommand(CommandId);
            if (command is IGalleryCommand galleryCommand)
            {
                // Set loading flag to prevent command execution during initialization
                _isLoading = true;
                try
                {
                    // Subscribe to items changed if not already
                    galleryCommand.ItemsChanged -= OnGalleryItemsChanged;
                    galleryCommand.ItemsChanged += OnGalleryItemsChanged;

                    // Load items
                    _innerComboBox.Items.Clear();
                    foreach (var item in galleryCommand.GalleryItems)
                    {
                        _innerComboBox.Items.Add(item.Label ?? item.Tag?.ToString() ?? "");
                    }

                    // Set selected index
                    if (galleryCommand.SelectedIndex >= 0 && galleryCommand.SelectedIndex < _innerComboBox.Items.Count)
                    {
                        _innerComboBox.SelectedIndex = galleryCommand.SelectedIndex;
                    }

                    // Set label from command
                    if (string.IsNullOrEmpty(_label))
                    {
                        Label = command.Label;
                    }
                }
                finally
                {
                    _isLoading = false;
                }
            }
            else
            {
                // If not a gallery command, check for default font values
                SetDefaultFontValues();
            }
        }

        private void SetDefaultFontValues()
        {
            // Set default display values for Font comboboxes for visual matching
            _isLoading = true;
            try
            {
                if (CommandId == OpenLiveWriter.Localization.CommandId.FontFamily)
                {
                    if (_innerComboBox.Items.Count == 0)
                    {
                        _innerComboBox.Items.Add("Calibri");
                        _innerComboBox.SelectedIndex = 0;
                    }
                }
                else if (CommandId == OpenLiveWriter.Localization.CommandId.FontSize)
                {
                    // Show the editor default size as display text (the old ribbon
                    // shows "11" too). It is display text only, not a gallery item,
                    // since the gallery sizes are 8/10/12/14/18/24/36.
                    _innerComboBox.Text = "11";
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void OnGalleryItemsChanged(object sender, EventArgs e)
        {
            LoadItemsFromCommand();
        }

        /// <summary>
        /// Override to fill entire bounds before child controls render.
        /// This prevents black showing through gaps between label and combobox.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Fill entire bounds with opaque background to prevent black in gaps
            e.Graphics.Clear(RibbonColors.Current.GetOpaqueGroupBackground());
            base.OnPaint(e);
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();

            switch (CurrentSize)
            {
                case RibbonGroupSize.Large:
                case RibbonGroupSize.Medium:
                    if (_showLabel)
                    {
                        Size = new Size(Width, 44);
                        _labelControl.Visible = true;
                        _innerComboBox.Location = new Point(0, 16);
                    }
                    else
                    {
                        // No label mode - compact height for font group layout
                        Size = new Size(Width, 23);
                        _labelControl.Visible = false;
                        _innerComboBox.Location = new Point(0, 0);
                    }
                    break;
                case RibbonGroupSize.Small:
                    Size = new Size(80, 24);
                    _labelControl.Visible = false;
                    _innerComboBox.Location = new Point(0, 0);
                    break;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_labelControl != null)
            {
                _labelControl.Width = Width;
            }

            if (_innerComboBox != null)
            {
                _innerComboBox.Width = Width;
            }
        }

        /// <summary>
        /// Adds an item to the combobox.
        /// </summary>
        public void AddItem(object item)
        {
            _innerComboBox.Items.Add(item);
        }

        /// <summary>
        /// Clears all items from the combobox.
        /// </summary>
        public void ClearItems()
        {
            _innerComboBox.Items.Clear();
        }

        /// <summary>
        /// Begins an update batch.
        /// </summary>
        public void BeginUpdate()
        {
            _innerComboBox.BeginUpdate();
        }

        /// <summary>
        /// Ends an update batch.
        /// </summary>
        public void EndUpdate()
        {
            _innerComboBox.EndUpdate();
        }
    }
}
