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
        private const int CONTENT_PADDING = 2;

        private RibbonCommandManager _commandManager;
        private CommandId _commandId;
        private string _label;
        private string _keytip;
        private RibbonApplicationMode _visibleModes = RibbonApplicationMode.All;
        private RibbonContextualTabGroup _contextualGroup = RibbonContextualTabGroup.None;

        private readonly List<RibbonGroup> _groups = new List<RibbonGroup>();
        private readonly Panel _contentPanel;

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
        /// Updates visibility of groups based on the current application mode.
        /// </summary>
        /// <param name="currentMode">The current application mode.</param>
        public void UpdateGroupVisibility(RibbonApplicationMode currentMode)
        {
            bool anyChanged = false;
            foreach (var group in _groups)
            {
                var shouldBeVisible = (group.VisibleModes & currentMode) != 0;
                if (group.Visible != shouldBeVisible)
                {
                    group.Visible = shouldBeVisible;
                    anyChanged = true;
                }
            }

            // Re-layout if any group visibility changed
            if (anyChanged)
            {
                UpdateScaling();
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
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = RibbonColors.Current.TabBackgroundSelected;

            // Use a TransparentPanel for proper transparent background support
            _contentPanel = new TransparentPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                BackColor = Color.Transparent
            };

            Controls.Add(_contentPanel);
        }

        /// <summary>
        /// A Panel that properly supports transparent background.
        /// Standard Panel doesn't have SupportsTransparentBackColor style set,
        /// which can cause rendering issues with transparent backgrounds.
        /// </summary>
        private class TransparentPanel : Panel
        {
            public TransparentPanel()
            {
                // Use AllPaintingInWmPaint to ensure background paints before child controls
                // This prevents black areas on first render
                SetStyle(ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.SupportsTransparentBackColor |
                         ControlStyles.AllPaintingInWmPaint, true);
                BackColor = Color.Transparent;
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // Fill with opaque group background to initialize buffer
                e.Graphics.Clear(RibbonColors.Current.GetOpaqueGroupBackground());
            }
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
            
            // Calculate total width needed at current sizes
            int CalculateTotalWidth()
            {
                var total = 0;
                foreach (var group in _groups)
                {
                    if (!group.Visible) continue;
                    total += group.GetPreferredWidth() + GROUP_SPACING;
                }
                return total;
            }

            var totalWidth = CalculateTotalWidth();

            // If we need to scale down, reduce group sizes iteratively until they all fit
            if (totalWidth > availableWidth)
            {
                // Phase 1: Scale down groups from Large -> Medium -> Small
                bool anyGroupScaled;
                do
                {
                    anyGroupScaled = false;
                    
                    // Scale down groups that are larger than Small, prioritizing larger groups first
                    // Sort groups by current size (largest first) to scale down largest groups first
                    var groupsToScale = new List<RibbonGroup>();
                    foreach (var group in _groups)
                    {
                        if (!group.Visible) continue;
                        if (group.CurrentSize > RibbonGroupSize.Small)
                        {
                            groupsToScale.Add(group);
                        }
                    }
                    
                    // Sort by size descending: Large (0) -> Medium (1) -> Small (2)
                    // This ensures we scale down the largest groups first for better visual consistency
                    groupsToScale.Sort((a, b) => b.CurrentSize.CompareTo(a.CurrentSize));
                    
                    // Scale down each group that can be scaled
                    foreach (var group in groupsToScale)
                    {
                        if (group.CurrentSize > RibbonGroupSize.Small)
                        {
                            group.CurrentSize = (RibbonGroupSize)((int)group.CurrentSize + 1);
                            anyGroupScaled = true;
                            
                            // Recalculate total width after each scaling step
                            totalWidth = CalculateTotalWidth();
                            
                            // If we've achieved the target width, stop scaling
                            if (totalWidth <= availableWidth)
                                break;
                        }
                    }
                    
                    // Continue scaling until no more groups can be scaled or we've achieved the target width
                } while (anyGroupScaled && totalWidth > availableWidth);

                // Phase 2: If groups still don't fit at Small size, scale some to Popup mode
                // Scale groups to Popup one at a time, prioritizing wider groups first
                while (totalWidth > availableWidth)
                {
                    // Find groups that are still at Small size (not already Popup)
                    var groupsAtSmall = new List<RibbonGroup>();
                    foreach (var group in _groups)
                    {
                        if (!group.Visible) continue;
                        if (group.CurrentSize == RibbonGroupSize.Small)
                        {
                            groupsAtSmall.Add(group);
                        }
                    }

                    // If no groups can be scaled to Popup, we're done (all are already Popup or smaller)
                    if (groupsAtSmall.Count == 0)
                        break;

                    // Sort by width descending to scale the widest groups to Popup first
                    // This maximizes space savings per group scaled
                    groupsAtSmall.Sort((a, b) => b.GetPreferredWidth().CompareTo(a.GetPreferredWidth()));

                    // Scale the widest group to Popup
                    var groupToScale = groupsAtSmall[0];
                    groupToScale.CurrentSize = RibbonGroupSize.Popup;
                    
                    // Recalculate total width
                    totalWidth = CalculateTotalWidth();
                    
                    // If we've achieved the target width, stop scaling
                    if (totalWidth <= availableWidth)
                        break;
                }
            }

            // Layout groups manually with correct height
            LayoutGroups();
        }

        private void LayoutGroups()
        {
            var x = CONTENT_PADDING;
            var groupHeight = _contentPanel.Height - 2; // Full height minus border

            foreach (var group in _groups)
            {
                if (!group.Visible) continue;

                var groupWidth = group.GetPreferredWidth();
                group.Location = new Point(x, 0);
                group.Size = new Size(groupWidth, groupHeight);

                x += groupWidth + GROUP_SPACING;
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
