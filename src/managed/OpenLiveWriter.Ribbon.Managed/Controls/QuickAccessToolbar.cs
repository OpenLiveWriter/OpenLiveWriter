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
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Quick Access Toolbar control for the ribbon.
    /// </summary>
    public class QuickAccessToolbar : UserControl
    {
        private const int BUTTON_SIZE = 20;
        private const int BUTTON_PADDING = 2;
        private const int DROPDOWN_BUTTON_WIDTH = 12;

        private RibbonCommandManager _commandManager;
        private readonly List<CommandId> _commands = new List<CommandId>();
        private readonly List<Rectangle> _buttonBounds = new List<Rectangle>();
        private int _hoveredIndex = -1;
        private int _pressedIndex = -1;
        private bool _dropDownHovered;
        private Rectangle _dropDownBounds;

        private ContextMenuStrip _customizeMenu;

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
                _commandManager = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets the commands on the QAT.
        /// </summary>
        public IReadOnlyList<CommandId> Commands => _commands.AsReadOnly();

        /// <summary>
        /// Occurs when a command is executed from the QAT.
        /// </summary>
        public event EventHandler<QatCommandEventArgs> CommandExecuted;

        public QuickAccessToolbar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            Height = BUTTON_SIZE + BUTTON_PADDING * 2;
            BackColor = Color.Transparent;

            // Add default commands
            AddDefaultCommands();
        }

        private void AddDefaultCommands()
        {
            _commands.Add(CommandId.SavePost);
            _commands.Add(CommandId.Undo);
            _commands.Add(CommandId.Redo);
        }

        /// <summary>
        /// Adds a command to the QAT.
        /// </summary>
        public void AddCommand(CommandId commandId)
        {
            if (!_commands.Contains(commandId))
            {
                _commands.Add(commandId);
                UpdateLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// Removes a command from the QAT.
        /// </summary>
        public void RemoveCommand(CommandId commandId)
        {
            if (_commands.Remove(commandId))
            {
                UpdateLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// Sets the commands on the QAT.
        /// </summary>
        public void SetCommands(IEnumerable<CommandId> commands)
        {
            _commands.Clear();
            foreach (var cmd in commands)
            {
                _commands.Add(cmd);
            }
            UpdateLayout();
            Invalidate();
        }

        private void UpdateLayout()
        {
            _buttonBounds.Clear();
            var x = BUTTON_PADDING;

            foreach (var cmd in _commands)
            {
                _buttonBounds.Add(new Rectangle(x, BUTTON_PADDING, BUTTON_SIZE, BUTTON_SIZE));
                x += BUTTON_SIZE + BUTTON_PADDING;
            }

            _dropDownBounds = new Rectangle(x, BUTTON_PADDING, DROPDOWN_BUTTON_WIDTH, BUTTON_SIZE);

            Width = x + DROPDOWN_BUTTON_WIDTH + BUTTON_PADDING;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            // Background
            using (var brush = new SolidBrush(RibbonColors.Current.QatBackground))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // Buttons
            for (int i = 0; i < _commands.Count; i++)
            {
                if (i < _buttonBounds.Count)
                {
                    DrawQatButton(g, _buttonBounds[i], _commands[i],
                        i == _hoveredIndex, i == _pressedIndex);
                }
            }

            // Dropdown button
            DrawDropDownButton(g, _dropDownBounds, _dropDownHovered);
        }

        private void DrawQatButton(Graphics g, Rectangle bounds, CommandId commandId,
            bool isHovered, bool isPressed)
        {
            var command = _commandManager?.GetCommand(commandId);
            var isEnabled = command?.Enabled ?? true;

            // Background
            if (isPressed)
            {
                using (var brush = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                {
                    g.FillRectangle(brush, bounds);
                }
            }
            else if (isHovered)
            {
                using (var brush = new SolidBrush(RibbonColors.Current.QatButtonBackgroundHover))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // Icon
            var image = command?.SmallImage;
            if (image != null)
            {
                var imageBounds = new Rectangle(
                    bounds.X + (bounds.Width - 16) / 2,
                    bounds.Y + (bounds.Height - 16) / 2,
                    16, 16);

                if (isEnabled)
                {
                    g.DrawImage(image, imageBounds);
                }
                else
                {
                    // Draw grayed out
                    using (var attributes = new System.Drawing.Imaging.ImageAttributes())
                    {
                        var matrix = new System.Drawing.Imaging.ColorMatrix();
                        matrix.Matrix33 = 0.5f;
                        attributes.SetColorMatrix(matrix);
                        g.DrawImage(image, imageBounds, 0, 0, image.Width, image.Height,
                            GraphicsUnit.Pixel, attributes);
                    }
                }
            }
        }

        private void DrawDropDownButton(Graphics g, Rectangle bounds, bool isHovered)
        {
            // Background
            if (isHovered)
            {
                using (var brush = new SolidBrush(RibbonColors.Current.QatButtonBackgroundHover))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // Arrow
            var arrowSize = 4;
            var arrowX = bounds.X + (bounds.Width - arrowSize) / 2;
            var arrowY = bounds.Y + (bounds.Height - arrowSize / 2) / 2;

            using (var brush = new SolidBrush(Color.White))
            {
                var points = new Point[]
                {
                    new Point(arrowX, arrowY),
                    new Point(arrowX + arrowSize, arrowY),
                    new Point(arrowX + arrowSize / 2, arrowY + arrowSize / 2 + 1)
                };
                g.FillPolygon(brush, points);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var newHovered = -1;
            var newDropDownHovered = false;

            for (int i = 0; i < _buttonBounds.Count; i++)
            {
                if (_buttonBounds[i].Contains(e.Location))
                {
                    newHovered = i;
                    break;
                }
            }

            if (newHovered < 0)
            {
                newDropDownHovered = _dropDownBounds.Contains(e.Location);
            }

            if (newHovered != _hoveredIndex || newDropDownHovered != _dropDownHovered)
            {
                _hoveredIndex = newHovered;
                _dropDownHovered = newDropDownHovered;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_hoveredIndex >= 0 || _dropDownHovered)
            {
                _hoveredIndex = -1;
                _dropDownHovered = false;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left) return;

            for (int i = 0; i < _buttonBounds.Count; i++)
            {
                if (_buttonBounds[i].Contains(e.Location))
                {
                    _pressedIndex = i;
                    Invalidate();
                    return;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left) return;

            var wasPressed = _pressedIndex;
            _pressedIndex = -1;

            if (wasPressed >= 0 && wasPressed < _buttonBounds.Count &&
                _buttonBounds[wasPressed].Contains(e.Location))
            {
                var commandId = _commands[wasPressed];
                var command = _commandManager?.GetCommand(commandId);

                if (command?.Enabled ?? true)
                {
                    CommandExecuted?.Invoke(this, new QatCommandEventArgs(commandId));
                    _commandManager?.Execute(commandId);
                }
            }

            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            var mousePos = PointToClient(MousePosition);

            if (_dropDownBounds.Contains(mousePos))
            {
                ShowCustomizeMenu();
            }
        }

        private void ShowCustomizeMenu()
        {
            if (_customizeMenu == null)
            {
                _customizeMenu = new ContextMenuStrip();
            }

            _customizeMenu.Items.Clear();

            // Add items for common commands
            var commonCommands = new[]
            {
                CommandId.NewPost,
                CommandId.OpenPost,
                CommandId.SavePost,
                CommandId.Undo,
                CommandId.Redo,
                CommandId.Print,
                CommandId.PrintPreview
            };

            foreach (var cmdId in commonCommands)
            {
                var command = _commandManager?.GetCommand(cmdId);
                var label = command?.Label ?? cmdId.ToString();
                var item = new ToolStripMenuItem(label);
                item.Tag = cmdId;
                item.Checked = _commands.Contains(cmdId);
                item.Click += (s, e) =>
                {
                    var mi = (ToolStripMenuItem)s;
                    var id = (CommandId)mi.Tag;
                    if (_commands.Contains(id))
                        RemoveCommand(id);
                    else
                        AddCommand(id);
                };
                _customizeMenu.Items.Add(item);
            }

            _customizeMenu.Items.Add(new ToolStripSeparator());

            var showBelow = new ToolStripMenuItem("Show Below the Ribbon");
            showBelow.Click += (s, e) =>
            {
                // Toggle position - would need parent ribbon to implement
            };
            _customizeMenu.Items.Add(showBelow);

            _customizeMenu.Show(this, new Point(_dropDownBounds.Left, _dropDownBounds.Bottom));
        }

        /// <summary>
        /// Saves QAT settings to a stream.
        /// </summary>
        public void SaveSettings(Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(_commands.Count);
                foreach (var cmd in _commands)
                {
                    writer.Write((int)cmd);
                }
            }
        }

        /// <summary>
        /// Loads QAT settings from a stream.
        /// </summary>
        public void LoadSettings(Stream stream)
        {
            try
            {
                using (var reader = new BinaryReader(stream))
                {
                    var count = reader.ReadInt32();
                    _commands.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        var cmdId = (CommandId)reader.ReadInt32();
                        _commands.Add(cmdId);
                    }
                }
                UpdateLayout();
                Invalidate();
            }
            catch
            {
                // If loading fails, keep defaults
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }
    }

    /// <summary>
    /// Event args for QAT command execution.
    /// </summary>
    public class QatCommandEventArgs : EventArgs
    {
        public CommandId CommandId { get; }

        public QatCommandEventArgs(CommandId commandId)
        {
            CommandId = commandId;
        }
    }
}
