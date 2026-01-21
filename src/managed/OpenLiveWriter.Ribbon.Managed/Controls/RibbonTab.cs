// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Represents a tab in the ribbon.
    /// </summary>
    public class RibbonTab : UserControl
    {
        private const int GROUP_SPACING = 2;
        private const int GROUP_LABEL_HEIGHT = 18;
        private const int CONTENT_PADDING = 4;

        private RibbonCommandManager _commandManager;
        private CommandId _commandId;
        private string _label;
        private string _keytip;
        private RibbonApplicationMode _visibleModes = RibbonApplicationMode.All;
        private RibbonContextualTabGroup _contextualGroup = RibbonContextualTabGroup.None;

        private readonly List<RibbonGroup> _groups = new List<RibbonGroup>();
        private readonly FlowLayoutPanel _contentPanel;

        /// <summary>
        /// Gets or sets the command ID for this tab.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CommandId CommandId
        {
            get => _commandId;
            set => _commandId = value;
        }

        /// <summary>
        /// Gets or sets the display label.
        /// </summary>
        public string Label
        {
            get => _label;
            set => _label = value;
        }

        /// <summary>
        /// Gets or sets the keytip for keyboard navigation.
        /// </summary>
        public string Keytip
        {
            get => _keytip;
            set => _keytip = value;
        }

        /// <summary>
        /// Gets or sets the application modes where this tab is visible.
        /// </summary>
        public RibbonApplicationMode VisibleModes
        {
            get => _visibleModes;
            set => _visibleModes = value;
        }

        /// <summary>
        /// Gets or sets the contextual tab group this tab belongs to.
        /// </summary>
        public RibbonContextualTabGroup ContextualGroup
        {
            get => _contextualGroup;
            set => _contextualGroup = value;
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
                _commandManager = value;
                foreach (var group in _groups)
                {
                    group.CommandManager = value;
                }
            }
        }

        /// <summary>
        /// Gets the header bounds (for hit testing in RibbonPanel).
        /// </summary>
        internal Rectangle HeaderBounds { get; set; }

        /// <summary>
        /// Gets the groups in this tab.
        /// </summary>
        public IReadOnlyList<RibbonGroup> Groups => _groups.AsReadOnly();

        public RibbonTab()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            BackColor = RibbonColors.Current.TabBackgroundSelected;

            _contentPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                BackColor = Color.Transparent,
                Padding = new Padding(CONTENT_PADDING, CONTENT_PADDING, CONTENT_PADDING, 0)
            };

            Controls.Add(_contentPanel);
        }

        /// <summary>
        /// Adds a group to this tab.
        /// </summary>
        public void AddGroup(RibbonGroup group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));

            group.CommandManager = _commandManager;
            _groups.Add(group);
            _contentPanel.Controls.Add(group);
        }

        /// <summary>
        /// Removes a group from this tab.
        /// </summary>
        public void RemoveGroup(RibbonGroup group)
        {
            if (group == null) return;

            if (_groups.Remove(group))
            {
                _contentPanel.Controls.Remove(group);
            }
        }

        /// <summary>
        /// Clears all groups from this tab.
        /// </summary>
        public void ClearGroups()
        {
            foreach (var group in _groups)
            {
                _contentPanel.Controls.Remove(group);
                group.Dispose();
            }
            _groups.Clear();
        }

        /// <summary>
        /// Updates group sizes based on scaling policy and available width.
        /// </summary>
        public void UpdateScaling()
        {
            var availableWidth = Width - CONTENT_PADDING * 2;
            var totalWidth = 0;

            // Calculate total width needed at current sizes
            foreach (var group in _groups)
            {
                if (!group.Visible) continue;
                totalWidth += group.GetPreferredWidth() + GROUP_SPACING;
            }

            // If we need to scale down, reduce group sizes
            if (totalWidth > availableWidth)
            {
                // Apply scaling steps - reduce largest groups first
                foreach (var group in _groups)
                {
                    if (!group.Visible) continue;

                    if (group.CurrentSize > RibbonGroupSize.Small)
                    {
                        group.CurrentSize = (RibbonGroupSize)((int)group.CurrentSize + 1);

                        // Recalculate total width
                        totalWidth = 0;
                        foreach (var g in _groups)
                        {
                            if (!g.Visible) continue;
                            totalWidth += g.GetPreferredWidth() + GROUP_SPACING;
                        }

                        if (totalWidth <= availableWidth)
                            break;
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScaling();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw bottom border
            using (var pen = new Pen(RibbonColors.Current.RibbonBorder))
            {
                e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            }
        }
    }
}
